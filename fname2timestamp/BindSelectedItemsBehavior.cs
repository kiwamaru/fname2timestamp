using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace fname2timestamp
{
    /// <summary>
    /// DataGrid内で選択されたアイテムのコレクションを保持するビヘイビア
    /// </summary>
    public class BindSelectedItemsBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// アタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                //DataGridの選択アイテム変更時のハンドラを登録
                grid.SelectionChanged += grid_SelectionChanged;
            }
        }

        /// <summary>
        /// デタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                //DataGridの選択アイテム変更時のハンドラの登録を解除
                grid.SelectionChanged -= grid_SelectionChanged;
            }

            base.OnDetaching();
        }

        #region 選択されたアイテムリストの依存プロパティの登録
        public static DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(BindSelectedItemsBehavior), new PropertyMetadata(null));

        /// <summary>
        /// 選択されたアイテムリスト
        /// </summary>
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        #endregion

        /// <summary>
        /// 選択されたアイテムリストを選択されたアイテムの変更に応じて更新
        /// </summary>
        void grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //新たに選択されたアイテムをリストに追加
            foreach (var addedItem in e.AddedItems)
            {
                SelectedItems.Add(addedItem);
            }

            //選択解除されたアイテムをリストから削除
            foreach (var removedItem in e.RemovedItems)
            {
                SelectedItems.Remove(removedItem);
            }
        }
    }
}
