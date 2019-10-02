using AutoMapper;
using Links.Common.Base;
using Links.Localization;

namespace Links.WPF
{
    public class RenameViewModel : DirtyScreen<RenameViewModel>
    {
        #region Fields

        private string _name;

        #endregion Fields

        #region Constructors

        public RenameViewModel(IMapper mapper) : base(mapper)
        {
            DisplayName = Translations.Rename;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        #endregion Properties
    }
}