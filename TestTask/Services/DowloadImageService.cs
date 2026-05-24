using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TestTask.Services
{
    //логика загрузки картинки по ссылке
    internal class DowloadImageService
    {
        private static HttpClient _httpClient = new HttpClient();
        private const int SIZE_BYTE = 8192;
        public async Task<BitmapImage> DowloadImageAsync(string imageUrl, IProgress<int> progress, CancellationToken ct)
        {
            //получить размер файла через Headers
            using(HttpResponseMessage responseMessage = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                //проверка выполнения запроса
                responseMessage.EnsureSuccessStatusCode();
                //вес картинки для расчета прогресса
                long? totalBytes = responseMessage.Content.Headers.ContentLength;
                using (Stream stream = await responseMessage.Content.ReadAsStreamAsync())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        //подгружаем картинку по 8кб для отслеживания прогресса
                        byte[] buffer = new byte[SIZE_BYTE];
                        long totalRead = 0;
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                        {
                            await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);
                            totalRead += bytesRead;

                            //ProgressBar
                            if (progress!=null && totalBytes.HasValue && totalBytes.Value > 0)
                            {
                                int percent = (int)((totalRead * 100)/totalBytes.Value);
                                progress.Report(percent);
                            }
                        }
                        //после всей загрузки ProgressBar = 100 
                        if (progress != null)
                        {
                            progress.Report(100);
                        }
                        memoryStream.Position = 0;
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = memoryStream;
                        image.EndInit();
                        image.Freeze();
                        return image;
                    }
                }
            }
        }
    }
}
