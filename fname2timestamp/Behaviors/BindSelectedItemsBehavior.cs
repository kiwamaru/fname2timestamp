using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace fname2timestamp.Behaviors
{
    class DataGridSelectedItemsBlendBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                "SelectedItems",
                typeof(IList<object>),
                typeof(DataGridSelectedItemsBlendBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged)
            );

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null) AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems == null) return;

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems) SelectedItems.Add(item);
            }

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems) SelectedItems.Remove(item);
            }
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridSelectedItemsBlendBehavior behavior)
            {
                var dataGrid = behavior.AssociatedObject;

                if (behavior.SelectedItems != null && dataGrid?.SelectedItems != null)
                {
                    dataGrid.SelectionChanged -= behavior.OnSelectionChanged;

                    dataGrid.SelectedItems.Clear();
                    foreach (var item in behavior.SelectedItems)
                    {
                        dataGrid.SelectedItems.Add(item);
                    }

                    dataGrid.SelectionChanged += behavior.OnSelectionChanged;
                }
            }
        }

        public IList<object> SelectedItems
        {
            get { return (IList<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
    }
}
