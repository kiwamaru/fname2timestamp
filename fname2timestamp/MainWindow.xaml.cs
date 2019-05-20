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
        //private uint index;
        private readonly ProgresBarWindow pbw;

        public FileListModel fileListModel { get; } = new FileListModel();


        public MainWindow()
        {
            InitializeComponent();

            //fileListModel = new FileListModel();
            pbw = new ProgresBarWindow();

            fileListModel.PropertyChanged += FileListModel_PropertyChanged;
            dataGrid.ItemsSource = fileListModel.dataGridFiles;
            FilesInfoTextBlock.Text = "↑ファイルをドロップしてください";

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            pbw.Close();
        }
        /// <summary>
        /// FileListModelのプロパティチェンジイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileListModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is FileListModel))
            {
                return;
            }

            if (e.PropertyName == "CurrentProgress")
            {
                int progn = fileListModel.CurrentProgress;
                if (progn > 0)
                {
                    pbw.Show();
                    pbw.Topmost = true;
                    pbw.FileRegexProgress(progn);
                }
                else
                {
                    pbw.Hide();
                }
            }
            else if (e.PropertyName == "FileListCount" || e.PropertyName == "CanFileListCount")
            {
                if (fileListModel.FileListCount > 0)
                {
                    FilesInfoTextBlock.Text = fileListModel.FileListCount + "個のファイル  うちタイムスタンプ変更可能ファイル" + fileListModel.CanFileListCount + "個";
                }
                else
                {
                    FilesInfoTextBlock.Text = "↑ファイルをドロップしてください";
                }
            }
        }


        /// <summary>
        /// ファイルドロップ時のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (fileListModel.DropFileListToDataGridFile(files, GetUpdateFlag()) <= 0)
            {
                MessageBox.Show("ファイルがありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private FileListModel.UPDATE_FLAG GetUpdateFlag()
        {
            FileListModel.UPDATE_FLAG upflag = 0;
            if (updateTimeStampChcekBox.IsChecked == true)
            {
                upflag |= FileListModel.UPDATE_FLAG.UPDATE_DATE;
            }
            if (creationTimeStampChcekBox.IsChecked == true)
            {
                upflag |= FileListModel.UPDATE_FLAG.CREATTION_DATE;
            }
            return upflag;
        }
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            // ファイルをドロップされた場合のみ e.Handled を True にする
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        /// <summary>
        /// 選択されたデータを変換するボタンのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelExec_Click(object sender, RoutedEventArgs e)
        {

            List<DataGridFile> sel_list = dataGrid.SelectedItems.Cast<DataGridFile>().ToList();
            if (!fileListModel.ChangeTimeStamp(sel_list, GetUpdateFlag()))
            {
                MessageBox.Show("時刻変換可能なファイルがリストにありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            dataGrid.SelectedIndex = -1;
        }

        /**
         * 有効な全データを変換
         */
        private void btnAllExec_Click(object sender, RoutedEventArgs e)
        {

            List<DataGridFile> all_list = dataGrid.Items.Cast<DataGridFile>().ToList();
            if (!fileListModel.ChangeTimeStamp(all_list, GetUpdateFlag()))
            {
                MessageBox.Show("時刻変換可能なファイルがリストにありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            dataGrid.SelectedIndex = -1;
        }
        /**
         * 選択されたデータを変換リストから削除
         */
        private void btnSelDel_Click(object sender, RoutedEventArgs e)
        {
            List<DataGridFile> sel_list = dataGrid.SelectedItems.Cast<DataGridFile>().ToList();

            fileListModel.RemoveDataGridFile(sel_list);
            dataGrid.SelectedIndex = -1;
        }
        /// <summary>
        /// 全データを変換リストから削除するボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllDel_Click(object sender, RoutedEventArgs e)
        {
            if (fileListModel.FileListCount > 0)
            {
                fileListModel.RemoveAllDataGridFile();
                dataGrid.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("ファイルがリストにありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /**
         * 選択ボタンの有効・無効を切り替える
         */
        private void DG_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //Get the newly selected cells
            List<DataGridFile> sel_list = dataGrid.SelectedItems.Cast<DataGridFile>().ToList();
            bool can_exe = false;
            foreach (DataGridFile x in sel_list)
            {
                if (x.isValid == true)
                {
                    can_exe = true;
                }
            }
            if (can_exe)
            {
                btnSelExec.IsEnabled = true;
            }
            else
            {
                btnSelExec.IsEnabled = false;
            }
            if (sel_list.Count >= 1)
            {
                btnSelDel.IsEnabled = true;
            }
            else
            {
                btnSelDel.IsEnabled = false;
            }
        }
        /**
         * 作成日時変更チェックボックスのクリックイベント
         */
        private void creationDateChcekBox_Checked(object sender, RoutedEventArgs e)
        {

            if (GetUpdateFlag() == 0)
            {
                creationTimeStampChcekBox.IsChecked = true;
                MessageBox.Show("両方共外すことは出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            fileListModel.UpdateList(GetUpdateFlag());
            dataGrid.SelectedIndex = -1;
        }

        /**
         * 更新日時変更チェックボックスのクリックイベント
         */
        private void updateChcekBox_Checked(object sender, RoutedEventArgs e)
        {
            if (GetUpdateFlag() == 0)
            {
                updateTimeStampChcekBox.IsChecked = true;
                MessageBox.Show("両方共外すことは出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            fileListModel.UpdateList(GetUpdateFlag());
            dataGrid.SelectedIndex = -1;
        }

    }

}
