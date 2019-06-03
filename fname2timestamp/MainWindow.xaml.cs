using DataGridFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace fname2timestamp
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ファイルドロップ時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (!ViewModel.AddFiles(files))
            {
                MessageBox.Show("ファイルがありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            // ファイルをドロップされた場合のみ e.Handled を True にする
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        /**
         * 選択ボタンの有効・無効を切り替える
         */
        private void DG_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //Get the newly selected cells
            this.ViewModel.SelChanged();
        }
        /**
         * 作成日時変更チェックボックスのクリックイベント
         */
        private void creationDateChcekBox_Checked(object sender, RoutedEventArgs e)
        {

            if (creationTimeStampChcekBox.IsChecked == false && updateTimeStampChcekBox.IsChecked == false)
            {
                creationTimeStampChcekBox.IsChecked = true;
                MessageBox.Show("両方共外すことは出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        /**
         * 更新日時変更チェックボックスのクリックイベント
         */
        private void updateChcekBox_Checked(object sender, RoutedEventArgs e)
        {
            if (creationTimeStampChcekBox.IsChecked == false && updateTimeStampChcekBox.IsChecked == false)
            {
                updateTimeStampChcekBox.IsChecked = true;
                MessageBox.Show("両方共外すことは出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ViewModel.OnClosing();
        }
    }

}
