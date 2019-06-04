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
    public class FileDragAndDropBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty DraggedFilesProperty =
            DependencyProperty.RegisterAttached(
                nameof(DraggedFiles), typeof(IEnumerable<string>),
                typeof(FileDragAndDropBehavior),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = null,
                    BindsTwoWayByDefault = true
                });

        public IEnumerable<string> DraggedFiles
        {
            get
            {
                return (IEnumerable<string>)
                  this.GetValue(DraggedFilesProperty);
            }
            set { this.SetValue(DraggedFilesProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewDragOver += this.PreviewDragOver;
            AssociatedObject.Drop += this.Drop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.PreviewDragOver -= this.PreviewDragOver;
                AssociatedObject.Drop -= this.Drop;
            }
        }

        private void PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                this.DraggedFiles =
                    (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            }
            else
            {
                this.DraggedFiles = null;
            }
        }
    }
}
