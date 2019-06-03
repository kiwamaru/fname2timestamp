using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using DataGridFiles;
using System.Collections.ObjectModel;

namespace fname2timestamp
{
    public class MainWindowViewModel : BindableBase
    {
        //private uint index;
        private readonly ProgresBarWindow pbw;

        public FileListModel fileListModel { get; } = new FileListModel();

        public ObservableCollection<DataGridFile> listSelectedItem = new ObservableCollection<DataGridFile>();
        public ObservableCollection<DataGridFile> ListSelectedItem
        {
            get { return listSelectedItem; }
            set { this.SetProperty(ref listSelectedItem, value); }
        }

        private bool creationDate;
        public bool CreationDate
        {
            get { return creationDate; }
            set { this.SetProperty(ref creationDate, value); }
        }
        private bool updateDate;
        public bool UpdateDate
        {
            get { return updateDate; }
            set { this.SetProperty(ref updateDate, value); }
        }

        private FileListModel.UPDATE_FLAG upflag;
        public FileListModel.UPDATE_FLAG Upflag
        {
            get { return upflag; }
            set { this.SetProperty(ref upflag, value); }
        }
        private String statusBarMessage;
        public String StatusBarMessage
        {
            get { return statusBarMessage; }
            set { this.SetProperty(ref statusBarMessage, value); }
        }
        
        private bool hasFile;
        public bool HasFile
        {
            get { return hasFile; }
            set { this.SetProperty(ref hasFile, value); }
        }
        private bool canChangeTimestamp;
        public bool CanChangeTimestamp
        {
            get { return canChangeTimestamp; }
            set { this.SetProperty(ref canChangeTimestamp, value); }
        }
        private bool canChangeAllTimestamp;
        public bool CanChangeAllTimestamp
        {
            get { return canChangeAllTimestamp; }
            set { this.SetProperty(ref canChangeAllTimestamp, value); }
        }
        private bool canDeleteFile;
        public bool CanDeleteFile
        {
            get { return canDeleteFile; }
            set { this.SetProperty(ref canDeleteFile, value); }
        }

        /// <summary>
        /// タイムスタンプ変更実行コマンド
        /// </summary>
        public DelegateCommand ChangeTimestampCommand { get; private set; }
        public DelegateCommand RemoveFileCommand { get; private set; }
        public DelegateCommand ChangeAllTimestampCommand { get; private set; }
        public DelegateCommand RemoveAllFileCommand { get; private set; }
        public DelegateCommand UpdateFileCommand { get; private set; }


        private ObservableCollection<DataGridFile> fileList;
        public ObservableCollection<DataGridFile> FileList
        {
            get { return fileList; }
            set { this.SetProperty(ref fileList, value); }
        }

        public MainWindowViewModel()
        {
            pbw = new ProgresBarWindow();

            fileListModel.PropertyChanged += FileListModel_PropertyChanged;
            FileList = fileListModel.DataGridFiles;
            ChangeTimestampCommand = new DelegateCommand(
                () =>
                {
                    OnChangeTimestamp();
                }, () => true);
            RemoveFileCommand = new DelegateCommand(
                () =>
                {
                    RemoveFile();
                }, () => true);
            ChangeAllTimestampCommand = new DelegateCommand(
                () =>
                {
                    OnChangeAllTimestamp();
                }, () => true);
            RemoveAllFileCommand = new DelegateCommand(
                () =>
                {
                    RemoveAllFile();
                }, () => true);
            UpdateFileCommand = new DelegateCommand(
                () =>
                {
                    UpdateFile();
                }, () => true);
            this.CanChangeTimestamp = false;
            this.CanChangeAllTimestamp = false;
            this.CanDeleteFile = false;
            this.StatusBarMessage = "↑ファイルをドロップしてください";
            this.CreationDate = true;
            this.UpdateDate = true;
            this.HasFile = false;

        }


        public void SelChanged()
        {
            this.CanChangeTimestamp = false;
            foreach (var x in ListSelectedItem)
            {
                if (x.isValid == true)
                {
                    this.CanChangeTimestamp = true;
                }
            }
            if (ListSelectedItem.Count() >= 1)
            {
                this.CanDeleteFile = true;
            }
            else
            {
                this.CanDeleteFile = false;
            }

        }
        public void OnClosing()
        {
            pbw.Close();
        }
        private FileListModel.UPDATE_FLAG GetUpdateFlag()
        {
            FileListModel.UPDATE_FLAG u = 0;
            if (UpdateDate == true)
            {
                u |= FileListModel.UPDATE_FLAG.UPDATE_DATE;
            }
            if (CreationDate == true)
            {
                u |= FileListModel.UPDATE_FLAG.CREATTION_DATE;
            }
            return u;
        }

        public bool AddFiles(string [] files)
        {
            ListSelectedItem.Clear();
            if (fileListModel.DropFileListToDataGridFile(files, GetUpdateFlag()) <= 0)
            {
                return false;
            }
            return true;
        }
        public void OnChangeAllTimestamp()
        {
            if (!fileListModel.ChangeTimeStamp(fileListModel.DataGridFiles.ToList(), GetUpdateFlag()))
            {
                //MessageBox.Show("時刻変換可能なファイルがリストにありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ListSelectedItem.Clear();
        }
        public void RemoveAllFile()
        {
            fileListModel.RemoveDataGridFile(fileListModel.DataGridFiles.ToList());
        }
        public void OnChangeTimestamp()
        {
            if (!fileListModel.ChangeTimeStamp(ListSelectedItem.ToList(), GetUpdateFlag()))
            {
                //MessageBox.Show("時刻変換可能なファイルがリストにありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ListSelectedItem.Clear();
        }
        public void RemoveFile()
        {
            fileListModel.RemoveDataGridFile(ListSelectedItem.ToList());
            ListSelectedItem.Clear();
        }
        public void UpdateFile()
        {
            fileListModel.UpdateList(GetUpdateFlag());
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
                    this.StatusBarMessage = fileListModel.FileListCount + "個のファイル  うちタイムスタンプ変更可能ファイル" + fileListModel.CanFileListCount + "個";
                    if (fileListModel.CanFileListCount > 0)
                    {
                        CanChangeAllTimestamp = true;
                    }else
                    {
                        CanChangeAllTimestamp = false;
                    }
                    HasFile = true;
                }
                else
                {
                    this.StatusBarMessage = "↑ファイルをドロップしてください";
                    HasFile = false;
                }
            }

        }


    }
}
