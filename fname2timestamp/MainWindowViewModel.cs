using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using DataGridFiles;
using System.Collections.ObjectModel;
using Prism.Interactivity.InteractionRequest;

namespace fname2timestamp
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        //private uint index;
        private readonly ProgresBarWindow pbw;

        public FileListModel fileListModel { get; } = new FileListModel();

        /// <summary>
        /// ドロップしたファイルのファイルリスト
        /// </summary>
        private IEnumerable<string> draggedFiles;
        public IEnumerable<string> DraggedFiles
        {
            get { return draggedFiles; }
            set { this.SetProperty(ref draggedFiles, value); }
        }

        /// <summary>
        /// DataGridで選択中のファイルリスト
        /// </summary>
        private ObservableCollection<DataGridFile> listSelectedItem = new ObservableCollection<DataGridFile>();
        public ObservableCollection<DataGridFile> ListSelectedItem
        {
            get { return listSelectedItem; }
            set { this.SetProperty(ref listSelectedItem, value); }
        }
        /// <summary>
        /// 作成日を変更するかどうか
        /// </summary>
        private bool creationDate;
        public bool CreationDate
        {
            get { return creationDate; }
            set { this.SetProperty(ref creationDate, value); }
        }
        /// <summary>
        /// 更新日を変更するかどうか
        /// </summary>
        private bool updateDate;
        public bool UpdateDate
        {
            get { return updateDate; }
            set { this.SetProperty(ref updateDate, value); }
        }

        /// <summary>
        /// Vのステータスバーのテキストメッセージ
        /// </summary>
        private String statusBarMessage;
        public String StatusBarMessage
        {
            get { return statusBarMessage; }
            set { this.SetProperty(ref statusBarMessage, value); }
        }

        /// <summary>
        /// 1つでもファイルリストがあるかどうかを保持 V側でバインド
        /// </summary>
        private bool hasFile;
        public bool HasFile
        {
            get { return hasFile; }
            set { this.SetProperty(ref hasFile, value); }
        }
        /// <summary>
        /// 選択したファイルがタイムスタンプ変更可能かどうかを保持 V側でバインド
        /// </summary>
        private bool canChangeTimestamp;
        public bool CanChangeTimestamp
        {
            get { return canChangeTimestamp; }
            set { this.SetProperty(ref canChangeTimestamp, value); }
        }
        /// <summary>
        /// タイムスタンプ変更可能なファイルが１つでもあるかどうかを保持 V側でバインド
        /// </summary>
        private bool canChangeAllTimestamp;
        public bool CanChangeAllTimestamp
        {
            get { return canChangeAllTimestamp; }
            set { this.SetProperty(ref canChangeAllTimestamp, value); }
        }
        /// <summary>
        /// リストから削除可能なファイルが１つでもあるかどうかを保持 V側でバインド
        /// </summary>
        private bool canDeleteFile;
        public bool CanDeleteFile
        {
            get { return canDeleteFile; }
            set { this.SetProperty(ref canDeleteFile, value); }
        }

        /// <summary>
        /// コマンド
        /// </summary>
        public DelegateCommand ChangeTimestampCommand { get; private set; }
        public DelegateCommand RemoveFileCommand { get; private set; }
        public DelegateCommand ChangeAllTimestampCommand { get; private set; }
        public DelegateCommand RemoveAllFileCommand { get; private set; }
        public DelegateCommand UpdateFileCommand { get; private set; }
        public DelegateCommand SelectedCellsChangedCommand { get; private set; }

        /// <summary>情報MessageBoxを表示します。</summary>
        public InteractionRequest<INotification> MessageBoxRequest { get; private set; }

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
            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
            this.MessageBoxRequest = new InteractionRequest<INotification>();

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
                    ChangeTimestampType();
                }, () => true);
            SelectedCellsChangedCommand = new DelegateCommand(
                () =>
                {
                    SelChanged();
                }, () => true);
            this.CanChangeTimestamp = false;
            this.CanChangeAllTimestamp = false;
            this.CanDeleteFile = false;
            this.StatusBarMessage = "↑ファイルをドロップしてください";
            this.CreationDate = true;
            this.UpdateDate = true;
            this.HasFile = false;

        }
        public void Dispose()
        {
            pbw.Close();
        }
        /// <summary>情報メッセージボックスを表示します。</summary>
        /// <param name="message">メッセージボックスに表示する内容を表す文字列。</param>
        /// <param name="title">メッセージボックスのタイトルを表す文字列。</param>
        private void showInformationMessage(string message, string title = "情報")
        {
            var notify = new Notification()
            {
                Content = message,
                Title = title
            };

            this.MessageBoxRequest.Raise(notify);
        }
        /// <summary>
        /// DataGrid選択変更イベント
        /// </summary>
        public void SelChanged()
        {
            if (ListSelectedItem.Count() == 0)
            {
                this.CanDeleteFile = false;
                this.CanChangeTimestamp = false;
                return;
            }
            this.CanDeleteFile = true;
            foreach (var x in ListSelectedItem)
            {
                if (x.isValid == true)
                {
                    this.CanChangeTimestamp = true;
                }
            }

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

        public void AddFiles(IEnumerable<string> files)
        {
            ListSelectedItem.Clear();
            if (fileListModel.DropFileListToDataGridFile(files, GetUpdateFlag()) <= 0)
            {
                this.showInformationMessage("ファイルがありません", "エラー");
            }
        }
        public void OnChangeAllTimestamp()
        {
            ListSelectedItem.Clear();
            if (!fileListModel.ChangeTimeStamp(fileListModel.DataGridFiles.ToList(), GetUpdateFlag()))
            {
                this.showInformationMessage("時刻変換可能なファイルがリストにありません", "エラー");
            }
        }
        public void OnChangeTimestamp()
        {
            var l = new ObservableCollection<DataGridFile>(ListSelectedItem);
            ListSelectedItem.Clear();
            if (!fileListModel.ChangeTimeStamp(l.ToList(), GetUpdateFlag()))
            {
                this.showInformationMessage("時刻変換可能なファイルがリストにありません", "エラー");
            }
        }
        public void RemoveAllFile()
        {
            ListSelectedItem.Clear();
            fileListModel.RemoveDataGridFile(fileListModel.DataGridFiles.ToList());
        }
        public void RemoveFile()
        {
            var l = new ObservableCollection < DataGridFile > (ListSelectedItem);
            ListSelectedItem.Clear();
            fileListModel.RemoveDataGridFile(l.ToList());
        }
        public void ChangeTimestampType()
        {
            fileListModel.UpdateList(GetUpdateFlag());
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "CreationDate")
            {
                if (CreationDate == false && UpdateDate == false)
                {
                    CreationDate = true;
                    this.showInformationMessage("両方共外すことは出来ません", "エラー");
                }

            }
            if (e.PropertyName == "UpdateDate")
            {
                if (CreationDate == false && UpdateDate == false)
                {
                    UpdateDate = true;
                    this.showInformationMessage("両方共外すことは出来ません", "エラー");
                }

            }
            if(e.PropertyName == "DraggedFiles")
            {
                AddFiles(DraggedFiles);
            }
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
