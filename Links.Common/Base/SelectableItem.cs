using Caliburn.Micro;
using System;

namespace Links.Common.Base
{
    public class SelectableItem<T> : Screen
    {
        #region Fields

        private bool _isSelected;
        private T _item;

        #endregion Fields

        #region Properties

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (Set(ref _isSelected, value))
                {
                    if (value && ItemSelected != null)
                        ItemSelected(Item);
                    else if (!value && ItemUnselected != null)
                        ItemUnselected(Item);
                }
            }
        }

        public T Item
        {
            get { return _item; }
            set { Set(ref _item, value); }
        }

        public Action<T> ItemSelected { get; set; }

        public Action<T> ItemUnselected { get; set; }

        #endregion Properties
    }
}