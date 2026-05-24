using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TestTask.Commands;
using TestTask.Models;
using TestTask.Services;
namespace TestTask.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private DowloadImageService _downloadImageService;
        public ObservableCollection<ImageModel> Images { get; set; }
        #region TotalProgress
        private int _totalProgress;
        public int TotalProgress
        {
            get => _totalProgress;
            set
            {
                _totalProgress = value;
                OnPropertyChanged();
            }
        }
        private void UpdateTotalProgress()
        {
            var itemsToShow = Images.Where(img => img.IsLoad || img.Progress > 0).ToList();

            if (itemsToShow.Count == 0)
            {
                TotalProgress = 0;
                return;
            }
            int totalProgressSum = itemsToShow.Sum(img => img.Progress);
            TotalProgress = totalProgressSum / itemsToShow.Count;
        }
        #endregion

        public MainWindowViewModel()
        {
            _downloadImageService = new DowloadImageService();
            Images = new ObservableCollection<ImageModel>();

            //3 пустых объекта
            for (int i = 0; i < 3; i++)
            {
                Images.Add(new ImageModel
                {
                    Url = string.Empty,
                    Progress = 0,
                    IsLoad = false
                });
            }
            #region Команды
            LoadAllCommand = new RelayCommand(OnLoadAllCommand, CanLoadAllCommand);
            StartCommand = new RelayCommand(OnStartCommand, CanStartCommand);
            StopCommand = new RelayCommand(OnStopCommand, CanStopCommand);
            #endregion
        }

        #region Commands

        #region LoadAllCommand
        public ICommand LoadAllCommand { get;}
        private bool CanLoadAllCommand(object p)
        {
            //если есть хотя бы одно изображение, которое не загружено
            return Images != null && Images.Any(img => !img.IsLoad && !string.IsNullOrWhiteSpace(img.Url));
        }
        private async void OnLoadAllCommand(object p)
        {
            var tasks = Images.Select(image =>
            {
                //у каждой картинки свой CancellationTokenSource
                if (image.Cts == null || image.Cts.Token.IsCancellationRequested)
                {
                    image.Cts = new CancellationTokenSource();
                }

                //загрузка, если URL не пустой и картинка не загружается
                if (!string.IsNullOrWhiteSpace(image.Url) && !image.IsLoad)
                {
                    return StartDownloadAsync(image, image.Cts.Token);
                }
                return Task.CompletedTask;
            });
            await Task.WhenAll(tasks);
        }
        #endregion

        #region StartCommand
        public ICommand StartCommand { get; }
        private bool CanStartCommand(object p)
        {
            //проверка на уже нажатую кнопку Start
            if (p is ImageModel image)
            {
                return !image.IsLoad && !string.IsNullOrWhiteSpace(image.Url);
            }
            return false;
        }
        private async void OnStartCommand(object p)
        {
            if (p is ImageModel image)
            {
                image.Progress = 0;
                image.Cts = new CancellationTokenSource();
                await StartDownloadAsync(image, image.Cts.Token);
            }
        }
        #endregion

        #region StopCommand
        public ICommand StopCommand { get; }
        private bool CanStopCommand(object p)
        {
            //если изображение в процессе загрузки
            if (p is ImageModel image)
            {
                return image.IsLoad && image.Cts != null;
            }
            return false;
        }

        private void OnStopCommand(object p)
        {
            if (p is ImageModel image && image.Cts != null)
            {
                image.Cts.Cancel();
                UpdateTotalProgress();
            }
        }

        #endregion

        #endregion

        private async Task StartDownloadAsync(ImageModel image, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(image.Url))
                return;
            image.IsLoad = true;
            UpdateTotalProgress();
            var progress = new Progress<int>(value =>
            {
                image.Progress = value;
                UpdateTotalProgress();
            });
            try
            {
                var bitmap = await _downloadImageService.DowloadImageAsync(image.Url, progress, ct);
                image.BitmapImage = bitmap;
                image.IsLoad = false;
            }
            catch (OperationCanceledException)
            {
                image.IsLoad = false;
                UpdateTotalProgress();
            }
            catch (Exception ex)
            {
                image.IsLoad = false;
                UpdateTotalProgress();
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
