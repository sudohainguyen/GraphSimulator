using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GraphSimulator.User_Controls
{
    public class PseudoListBox : ListBox
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PseudoListBoxItem();
        }
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PseudoListBoxItem;
        }
    }
    public class PseudoListBoxItem : ListBoxItem
    {
        private Selector ParentSelector
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this) as Selector; }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
        }
    }
}
