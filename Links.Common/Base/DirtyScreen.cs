using Links.Common.Extensions;
using AutoMapper;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Links.Common.Base
{
    public class DirtyScreen<TViewModel> : Screen
        where TViewModel : Screen
    {
        private TViewModel _oldVM;
        private bool _isBusy;

        public DirtyScreen(
            IMapper mapper)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; }

        public void Init()
        {
            _oldVM = Mapper.Map<TViewModel>(this as TViewModel);
        }

        [AlwaysClean]
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        [AlwaysClean]
        public bool IsDirty => _oldVM == null ? false : (this as TViewModel)?.IsDirty(_oldVM) ?? false;

        public override bool Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (base.Set(ref oldValue, newValue, propertyName))
            {
                OnIsDirtyChanged();
                return true;
            }
            return false;
        }

        public virtual void OnIsDirtyChanged()
        {
            NotifyOfPropertyChange(() => IsDirty);
        }
    }
}
