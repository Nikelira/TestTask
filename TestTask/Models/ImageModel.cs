using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TestTask.Models
{
    internal class ImageModel : INotifyPropertyChanged
    {
        private string _url;
        private BitmapImage _image;
        private bool _isLoad;
        private int _progress;
        private CancellationTokenSource _cts;
        public ImageModel()
        {
            _cts = new CancellationTokenSource();
        }
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged("Url");
            }
        }
        public BitmapImage BitmapImage
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("BitmapImage");
            }
        }
        public bool IsLoad
        {
            get { return _isLoad; }
            set
            {
                _isLoad = value;
                OnPropertyChanged("IsLoad");
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public CancellationTokenSource Cts
        {
            get { return _cts; }
            set
            {
                _cts = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
