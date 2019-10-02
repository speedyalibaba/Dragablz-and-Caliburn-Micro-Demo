namespace Links {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using AutoMapper;
	using Links.Contract;
	using Links.Contract.Services;
	using Links.Localization;
	using Links.Services;
	using Links.WPF;
    using Caliburn.Micro;
    using Dragablz;
    using MaterialDesignColors;
    using MaterialDesignThemes.Wpf;
    using Links.UserControls;

    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
            LogManager.GetLog = type => new Log4netLogger(type);
        }

        protected override void Configure()
        {
            ConfigureCaliburnConventions();

            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, MahappsWindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IConfigurationService, ConfigurationService>();
            _container.Singleton<IApplicationService, ApplicationService>();
            _container.Singleton<IDialogManager, MaterialDesignDialogManager>();
            _container.Singleton<LayoutManager>();
            _container.PerRequest<IInterTabClient, CaliburnInterTabClient>();
			_container.PerRequest<IInterLayoutClient, CaliburnInterLayoutClient>();
            _container.PerRequest<IShell, ShellViewModel>();

            _container.RegisterHandler(typeof(ILayoutManager), null, (_) => _container.GetInstance<LayoutManager>());
            _container.RegisterHandler(typeof(ISnackbarMessageQueue), null, (_) => new SnackbarMessageQueue(TimeSpan.FromMilliseconds(5000)));

			var mapperConfig = ConfigureAutoMapper(_container);
            _container.RegisterInstance(typeof(IMapper), null, mapperConfig.CreateMapper());

            _container.PerRequest<NewViewModel>();
            _container.PerRequest<MenuViewModel>();
            _container.PerRequest<Test1ViewModel>();
            _container.PerRequest<Test2ViewModel>();
            _container.PerRequest<ShellViewModel>();
            _container.PerRequest<RenameViewModel>();
            _container.PerRequest<SettingsViewModel>();
            _container.PerRequest<EditUsersViewModel>();
            _container.PerRequest<MessageBoxViewModel>();

#if DEBUG
            mapperConfig.AssertConfigurationIsValid();  //checks if MapperConfiguration is valid
#endif

            ////log everything
            //LogManager.GetLog = t => new DebugLog(t);

            //log only messages of ViewLocator
            var baseGetLog = LogManager.GetLog;
            LogManager.GetLog = t => t == typeof(ViewLocator) ? new DebugLog(t) : baseGetLog(t);
        }

        private void ConfigureCaliburnConventions()
        {
            var baseApplyValueConverter = ConventionManager.ApplyStringFormat;

            ConventionManager.ApplyStringFormat = (binding, bindableProperty, property) =>
            {
                baseApplyValueConverter(binding, bindableProperty, property);

                if (typeof(DateTime?).IsAssignableFrom(property.PropertyType))
                {
                    binding.StringFormat = "{0:d}";
                }
            };
        }

        protected MapperConfiguration ConfigureAutoMapper(SimpleContainer container)
		{
			var configuration = new MapperConfiguration(config =>
			{
				config.ConstructServicesUsing(type => container.GetInstance(type, null));

                config.CreateMap<Configuration, Configuration>();	//for cloning objects
                config.CreateMap<EditUsersViewModel.User, EditUsersViewModel.User>();
                config.CreateMap<EditUsersViewModel, EditUsersViewModel>().ConstructUsingServiceLocator();	//for DI
                config.CreateMap<RenameViewModel, RenameViewModel>().ConstructUsingServiceLocator();    //needs to be cloned for DirtyScreen
                config.CreateMap<SettingsViewModel, SettingsViewModel>().ConstructUsingServiceLocator();
            });

			return configuration;
		}

		protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            ApplicationService.IsStarting = true;

            var config = IoC.Get<IConfigurationService>().ActiveConfiguration;
            SetColors(config);
            LanguageHelper.ChangeLanguage(config.Language);

            var splashScreen = new SplashScreen();
            splashScreen.Show();

            RegisterGlobalExceptionHandling(LogManager.GetLog(typeof(AppBootstrapper)), IoC.Get<IDialogManager>());

			Dictionary<string, object> windowSettings = new Dictionary<string, object>();
			windowSettings.Add("Title", Translations.Links);
            windowSettings.Add("Height", 0D);
            windowSettings.Add("Width", 0D);

            DisplayRootViewFor<IShell>(windowSettings);

            splashScreen.Close();
            ApplicationService.IsStarting = false;
        }

        private void SetColors(Configuration configuration)
        {
            var swatches = new SwatchesProvider().Swatches;
            var paletteHelper = new PaletteHelper();
            var primary = swatches.FirstOrDefault(s => s.Name == configuration.PrimaryColor);
            var accent = swatches.FirstOrDefault(s => s.Name == configuration.AccentColor);
            if (primary != null)
            {
                paletteHelper.ReplacePrimaryColor(primary);
            }
            if (accent != null)
            {
                paletteHelper.ReplaceAccentColor(accent);
            }
            if (configuration.DarkMode)
            {
                paletteHelper.SetLightDark(true);
            }
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            //return all assemblies in which to search after views for specified viewmodels
            return new[] {
                Assembly.GetExecutingAssembly(),
                Assembly.GetAssembly(typeof(SettingsViewModel))
            };
        }

        private void RegisterGlobalExceptionHandling(ILog log, IDialogManager dialogManager)
        {
            //For Exceptions thrown in Caliburn Coroutines
            Coroutine.Completed += Coroutine_Completed;

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => CurrentDomainOnUnhandledException(args, log, dialogManager);

            //For Exceptions on a specific UI-Thread
            //Application.Dispatcher.UnhandledException +=
            //	(sender, args) => DispatcherOnUnhandledException(args, log, dialogManager);

            //For Exceptions on the main UI-Thread
            Application.DispatcherUnhandledException +=
                (sender, args) => CurrentOnDispatcherUnhandledException(args, log, dialogManager);

            TaskScheduler.UnobservedTaskException +=
                (sender, args) => TaskSchedulerOnUnobservedTaskException(args, log, dialogManager);
        }

        private void Coroutine_Completed(object sender, ResultCompletionEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.GetLog(typeof(AppBootstrapper)).Error(e.Error);

                IoC.Get<IDialogManager>().ShowErrorMessageBox(e.Error.ToString());
            }
        }

        private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args, ILog log, IDialogManager dialogManager)
        {
            log.Error(args.Exception);
            args.SetObserved();

            dialogManager.ShowErrorMessageBox(args.Exception.ToString());
        }

        private static void CurrentOnDispatcherUnhandledException(DispatcherUnhandledExceptionEventArgs args, ILog log, IDialogManager dialogManager)
        {
            log.Error(args.Exception);
            args.Handled = true;

            dialogManager.ShowErrorMessageBox(args.Exception.ToString());
        }

        //private static void DispatcherOnUnhandledException(DispatcherUnhandledExceptionEventArgs args, ILog log, MaterialDesignDialogManager dialogManager)
        //{
        //	log.Error(args.Exception);
        //	args.Handled = true;
        //
        //  dialogManager.ShowErrorMessageBox(args.Exception.ToString());
        //}

        private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args, ILog log, IDialogManager dialogManager)
        {
            var exception = args.ExceptionObject as Exception;
            var terminatingMessage = args.IsTerminating ? " The application is terminating." : string.Empty;
            var exceptionMessage = exception?.Message ?? "An unmanaged exception occured.";
            var message = string.Concat(exceptionMessage, terminatingMessage);
            ((Log4netLogger)log).Error(message, exception);

            dialogManager.ShowErrorMessageBox(args.ExceptionObject.ToString());
        }
    }
}