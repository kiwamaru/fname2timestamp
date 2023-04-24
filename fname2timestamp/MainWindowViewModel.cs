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
    public enum Mode
    {
        Timestamp,
        Rename,
    }
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        //private uint index;
        private readonly ProgresBarWindow pbw;

        public FileListModel fileListModel { get; } = new FileListModel();

        public RenameFileListModel renamefileListModel { get; } = new RenameFileListModel();

        private Mode _mode = Mode.Timestamp;
        public Mode Mode
        {
            get { return _mode; }
            set { this.SetProperty(ref _mode, value); }
        }
        private bool _changeTimestamp = true;
        public bool ChangeTimestamp
        {
            get { return _changeTimestamp; }
            set { this.SetProperty(ref _changeTimestamp, value); }
        }
        /// <summary>
        /// �h���b�v�����t�@�C���̃t�@�C�����X�g
        /// </summary>
        private IEnumerable<string> draggedFiles;
        public IEnumerable<string> DraggedFiles
        {
            get { return draggedFiles; }
            set { this.SetProperty(ref draggedFiles, value); }
        }

        /// <summary>
        /// DataGrid�őI�𒆂̃t�@�C�����X�g
        /// </summary>
        private ObservableCollection<DataGridFile> listSelectedItem = new ObservableCollection<DataGridFile>();
        public ObservableCollection<DataGridFile> ListSelectedItem
        {
            get { return listSelectedItem; }
            set { this.SetProperty(ref listSelectedItem, value); }
        }
        /// <summary>
        /// �쐬����ύX���邩�ǂ���
        /// </summary>
        private bool creationDate;
        public bool CreationDate
        {
            get { return creationDate; }
            set { this.SetProperty(ref creationDate, value); }
        }
        /// <summary>
        /// �X�V����ύX���邩�ǂ���
        /// </summary>
        private bool updateDate;
        public bool UpdateDate
        {
            get { return updateDate; }
            set { this.SetProperty(ref updateDate, value); }
        }
        /// <summary>
        /// �쐬����ύX���邩�ǂ���
        /// </summary>
        private bool creationDateRename;
        public bool CreationDateRename
        {
            get { return creationDateRename; }
            set { this.SetProperty(ref creationDateRename, value); }
        }
        /// <summary>
        /// �X�V����ύX���邩�ǂ���
        /// </summary>
        private bool updateDateRename;
        public bool UpdateDateRename
        {
            get { return updateDateRename; }
            set { this.SetProperty(ref updateDateRename, value); }
        }

        /// <summary>
        /// �t�@�C����������������폜���邩
        /// </summary>
        private bool removeDateRename = false;
        public bool RemoveDateRename
        {
            get { return removeDateRename; }
            set { this.SetProperty(ref removeDateRename, value); }
        }

        /// <summary>
        /// V�̃X�e�[�^�X�o�[�̃e�L�X�g���b�Z�[�W
        /// </summary>
        private String statusBarMessage;
        public String StatusBarMessage
        {
            get { return statusBarMessage; }
            set { this.SetProperty(ref statusBarMessage, value); }
        }

        /// <summary>
        /// 1�ł��t�@�C�����X�g�����邩�ǂ�����ێ� V���Ńo�C���h
        /// </summary>
        private bool hasFile;
        public bool HasFile
        {
            get { return hasFile; }
            set { this.SetProperty(ref hasFile, value); }
        }
        /// <summary>
        /// �I�������t�@�C�����^�C���X�^���v�ύX�\���ǂ�����ێ� V���Ńo�C���h
        /// </summary>
        private bool canChangeTimestamp;
        public bool CanChangeTimestamp
        {
            get { return canChangeTimestamp; }
            set { this.SetProperty(ref canChangeTimestamp, value); }
        }
        /// <summary>
        /// �^�C���X�^���v�ύX�\�ȃt�@�C�����P�ł����邩�ǂ�����ێ� V���Ńo�C���h
        /// </summary>
        private bool canChangeAllTimestamp;
        public bool CanChangeAllTimestamp
        {
            get { return canChangeAllTimestamp; }
            set { this.SetProperty(ref canChangeAllTimestamp, value); }
        }
        /// <summary>
        /// ���X�g����폜�\�ȃt�@�C�����P�ł����邩�ǂ�����ێ� V���Ńo�C���h
        /// </summary>
        private bool canDeleteFile;
        public bool CanDeleteFile
        {
            get { return canDeleteFile; }
            set { this.SetProperty(ref canDeleteFile, value); }
        }

        /// <summary>
        /// �R�}���h
        /// </summary>
        public DelegateCommand ChangeTimestampCommand { get; private set; }
        public DelegateCommand RenameFromTimestampCommand { get; private set; }
        
        public DelegateCommand ListClearCommand { get; private set; }
        public DelegateCommand UpdateFileCommand { get; private set; }
        public DelegateCommand SelectedCellsChangedCommand { get; private set; }

        /// <summary>���MessageBox��\�����܂��B</summary>
        public InteractionRequest<INotification> MessageBoxRequest { get; private set; }

        private ObservableCollection<DataGridFile> fileList;
        public ObservableCollection<DataGridFile> FileList
        {
            get { return fileList; }
            set { this.SetProperty(ref fileList, value); }
        }
        private ObservableCollection<DataGridFile> renamefileList;
        public ObservableCollection<DataGridFile> RenameFileList
        {
            get { return renamefileList; }
            set { this.SetProperty(ref renamefileList, value); }
        }

        public MainWindowViewModel()
        {
            pbw = new ProgresBarWindow();

            fileListModel.PropertyChanged += FileListModel_PropertyChanged;
            renamefileListModel.PropertyChanged += RenamefileListModel_PropertyChanged;
            FileList = fileListModel.DataGridFiles;
            RenameFileList = renamefileListModel.DataGridFiles;
            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
            this.MessageBoxRequest = new InteractionRequest<INotification>();

            ChangeTimestampCommand = new DelegateCommand(
                () =>
                {
                    OnChangeTimestamp();
                }, () => true);
            RenameFromTimestampCommand = new DelegateCommand(
                () =>
                {
                    OnRenameFromTimestamp();
                }, () => true);
            ListClearCommand = new DelegateCommand(
                () =>
                {
                    ClearFileList();
                }, () => HasFile).ObservesProperty(() => HasFile);
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
            this.StatusBarMessage = "���t�@�C�����h���b�v���Ă�������";
            this.CreationDate = true;
            this.CreationDateRename = true;
            this.UpdateDate = true;
            this.HasFile = false;

        }



        public void Dispose()
        {
            pbw.Close();
        }
        /// <summary>��񃁃b�Z�[�W�{�b�N�X��\�����܂��B</summary>
        /// <param name="message">���b�Z�[�W�{�b�N�X�ɕ\��������e��\��������B</param>
        /// <param name="title">���b�Z�[�W�{�b�N�X�̃^�C�g����\��������B</param>
        private void showInformationMessage(string message, string title = "���")
        {
            var notify = new Notification()
            {
                Content = message,
                Title = title
            };

            this.MessageBoxRequest.Raise(notify);
        }
        /// <summary>
        /// DataGrid�I��ύX�C�x���g
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
            if(RemoveDateRename == true)
            {
                u |= FileListModel.UPDATE_FLAG.REMOVE_DATE_FNAME;
            }
            return u;
        }
        private FileListModel.UPDATE_FLAG GetUpdateFlagRename()
        {
            FileListModel.UPDATE_FLAG u = 0;
            if (UpdateDateRename == true)
            {
                u |= FileListModel.UPDATE_FLAG.UPDATE_DATE;
            }
            if (CreationDateRename == true)
            {
                u |= FileListModel.UPDATE_FLAG.CREATTION_DATE;
            }
            return u;
        }
        public void AddFiles(IEnumerable<string> files)
        {
            ListSelectedItem.Clear();
            if(this.Mode == Mode.Timestamp) 
            { 
                if (fileListModel.DropFileListToDataGridFile(files, GetUpdateFlag()) <= 0)
                {
                    this.showInformationMessage("�t�@�C��������܂���", "�G���[");
                }
            }
            else
            {
                if(this.renamefileListModel.DropFileListToDataGridFile(files, GetUpdateFlagRename()) <= 0)
                {
                    this.showInformationMessage("�t�@�C��������܂���", "�G���[");
                }
            }
        }
        public void OnChangeAllTimestamp()
        {
            ListSelectedItem.Clear();
            if (!fileListModel.ChangeTimeStamp(fileListModel.DataGridFiles.ToList(), GetUpdateFlag(), RemoveDateRename))
            {
                this.showInformationMessage("�����ϊ��\�ȃt�@�C�������X�g�ɂ���܂���", "�G���[");
            }
        }
        public void OnChangeTimestamp()
        {
            var l = new ObservableCollection<DataGridFile>(ListSelectedItem);
            ListSelectedItem.Clear();
            if (!fileListModel.ChangeTimeStamp(l.ToList(), GetUpdateFlag(), RemoveDateRename))
            {
                this.showInformationMessage("�����ϊ��\�ȃt�@�C�������X�g�ɂ���܂���", "�G���[");
            }
        }
        public void OnRenameFromTimestamp()
        {
            var l = new ObservableCollection<DataGridFile>(ListSelectedItem);
            ListSelectedItem.Clear();
            if (!renamefileListModel.RenameFromTimestamp(l.ToList(), GetUpdateFlag()))
            {
                this.showInformationMessage("���l�[���\�ȃt�@�C�������X�g�ɂ���܂���", "�G���[");
            }
        }
        public void ClearFileList()
        {
            ListSelectedItem.Clear();
            fileListModel.RemoveDataGridFile(fileListModel.DataGridFiles.ToList());
            renamefileListModel.RemoveDataGridFile(renamefileListModel.DataGridFiles.ToList());
        }
        public void ChangeTimestampType()
        {
            fileListModel.UpdateList(GetUpdateFlag());
            renamefileListModel.UpdateList(GetUpdateFlagRename());
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CreationDate))
            {
                if (CreationDate == false && UpdateDate == false && RemoveDateRename == false)
                {
                    CreationDate = true;
                    this.showInformationMessage("���ׂĊO�����Ƃ͏o���܂���", "�G���[");
                }

            }
            if (e.PropertyName == nameof(UpdateDate))
            {
                if (CreationDate == false && UpdateDate == false && RemoveDateRename == false)
                {
                    UpdateDate = true;
                    this.showInformationMessage("���ׂĊO�����Ƃ͏o���܂���", "�G���[");
                }
            }
            if(e.PropertyName == nameof(RemoveDateRename))
            {
                if (CreationDate == false && UpdateDate == false && RemoveDateRename == false)
                {
                    RemoveDateRename = true;
                    this.showInformationMessage("���ׂĊO�����Ƃ͏o���܂���", "�G���[");
                }
            }
            if (e.PropertyName == nameof(CreationDateRename) || e.PropertyName == nameof(UpdateDateRename))
            {
                //ChangeTimestampType();
            }

            if (CreationDate == true && UpdateDate == true) ChangeTimestamp = true;
            else ChangeTimestamp = false;

            if (e.PropertyName == nameof(DraggedFiles))
            {
                AddFiles(DraggedFiles);
            }
            if(e.PropertyName == nameof(Mode))
            {
                this.ClearFileList();
            }
        }


        /// <summary>
        /// FileListModel�̃v���p�e�B�`�F���W�C�x���g�n���h��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileListModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is FileListModel))
            {
                return;
            }

            if (e.PropertyName == nameof(FileListModel.CurrentProgress))
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
            else if (e.PropertyName == nameof(FileListModel.FileListCount) || e.PropertyName == nameof(FileListModel.CanFileListCount))
            {
                if (fileListModel.FileListCount > 0)
                {
                    this.StatusBarMessage = fileListModel.FileListCount + "�̃t�@�C��  �����^�C���X�^���v�ύX�\�t�@�C��" + fileListModel.CanFileListCount + "��";
                    if (fileListModel.CanFileListCount > 0)
                    {
                        CanChangeAllTimestamp = true;
                    }
                    else
                    {
                        CanChangeAllTimestamp = false;
                    }
                    HasFile = true;
                }
                else
                {
                    this.StatusBarMessage = "���t�@�C�����h���b�v���Ă�������";
                    HasFile = false;
                }
            }

        }
        private void RenamefileListModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is RenameFileListModel))
            {
                return;
            }

            if (e.PropertyName == nameof(RenameFileListModel.CurrentProgress))
            {
                int progn = renamefileListModel.CurrentProgress;
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
            else if (e.PropertyName == nameof(RenameFileListModel.FileListCount) || e.PropertyName == nameof(RenameFileListModel.CanFileListCount))
            {
                if (renamefileListModel.FileListCount > 0)
                {
                    this.StatusBarMessage = renamefileListModel.FileListCount + "�̃t�@�C��  �����^�C���X�^���v�ύX�\�t�@�C��" + renamefileListModel.CanFileListCount + "��";
                    if (renamefileListModel.CanFileListCount > 0)
                    {
                        CanChangeAllTimestamp = true;
                    }
                    else
                    {
                        CanChangeAllTimestamp = false;
                    }
                    HasFile = true;
                }
                else
                {
                    this.StatusBarMessage = "���t�@�C�����h���b�v���Ă�������";
                    HasFile = false;
                }
            }
        }

    }
}
