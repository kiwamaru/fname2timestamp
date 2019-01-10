using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataGridFiles
{
    public class DataGridFile : INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;//DataGridに値の変更を通知

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
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
            get{
                return _success;
            } 
            set{
                _success = value;
                NotifyPropertyChanged();
            }
        }
        private string _err_message;
        public bool isValid
        {
            get{
                return _isValid;
            } 
            set{
                _isValid = value;
                NotifyPropertyChanged();
            }
        }
        private string full_path;
        private bool fname_validate;
        public uint num { get; set; }
        public string name{get;set;}
        public string path{get;set;}
        public string ext { get; set; }
        public DateTime update_dtime
        {
            get{
                return _update_dtime;
            } 
            set{
                _update_dtime = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime create_dtime
        {
            get{
                return _create_dtime;
            } 
            set{
                _create_dtime = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime access_dtime { get; set; }
        public long size { get; set; }
        public DateTime f2t_dtime { get; set; }
        public string err_message
        {
            get
            {
                return _err_message;
            }
            set
            {
                _err_message = value;
                NotifyPropertyChanged();
            }
        }
    }
}