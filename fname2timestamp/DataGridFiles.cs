using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataGridFiles
{
    public class DataGridFile : INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;//DataGridに値の変更を通知

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private DateTime _update_dtime;
        private DateTime _create_dtime;
        private bool _success;
        private bool _isValid;
        public bool success
        {
            get => _success;
            set
            {
                _success = value;
                NotifyPropertyChanged();
            }
        }
        private string _err_message;
        public bool isValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                NotifyPropertyChanged();
            }
        }
#pragma warning disable CS0169 // フィールド 'DataGridFile.full_path' は使用されていません。
        private readonly string full_path;
#pragma warning restore CS0169 // フィールド 'DataGridFile.full_path' は使用されていません。
#pragma warning disable CS0169 // フィールド 'DataGridFile.fname_validate' は使用されていません。
        private readonly bool fname_validate;
#pragma warning restore CS0169 // フィールド 'DataGridFile.fname_validate' は使用されていません。
        public uint num { get; set; }
        public string FileName { get; set; }
        public string path { get; set; }
        public string ext { get; set; }
        public string RenameFileName { get; set; }
        public DateTime update_dtime
        {
            get => _update_dtime;
            set
            {
                _update_dtime = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime create_dtime
        {
            get => _create_dtime;
            set
            {
                _create_dtime = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime access_dtime { get; set; }
        public long size { get; set; }
        public DateTime f2t_dtime { get; set; }
        public string err_message
        {
            get => _err_message;
            set
            {
                _err_message = value;
                NotifyPropertyChanged();
            }
        }
    }
}