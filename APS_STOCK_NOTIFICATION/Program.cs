using APS_STOCK_NOTIFICATION.Controller;
using APS_STOCK_NOTIFICATION.Model;
using SkiaSharp;
using System.Data;
using System.Net.Http.Headers;

using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp.BrowserData;
using System.Net;

namespace APS_STOCK_NOTIFICATION
{
    internal class Program
    {

        private static readonly string lineNotifyToken = "9VWUsTW1DwYrwYLCGafYFo1848adYWaIufNmU4K2fFP";
        static ReportAPSController SrvReportAPS = new ReportAPSController();


        static async Task Main(string[] args)
        {

            #region capture image
            Console.WriteLine("init");
            // Download Chromium if necessary
            //await new BrowserFetcher().DownloadAsync();


            Console.WriteLine("download end");
           
            string imgFileName = $"D:\\STOCK_WIP_NOTIFATION\\image\\wrn_model_{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";
            //string imgFileName = $@"D:\\Project\\2024\\APS_STOCK_NOTIFICATION\\APS_STOCK_NOTIFICATION\\image\\wrn_model_{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";

            // Launch a headless browser
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                // Specify the path to the downloaded Chromium executable
                //ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"

                //ExecutablePath = @"D:\chrome-win_1371831\chrome.exe"
                 ExecutablePath = @"D:\chrome-win_1371831\chrome.exe"

            });

            //Console.ReadKey();
            Console.WriteLine("pupeteer");

            // Open a new page
            var page = await browser.NewPageAsync();

            Console.WriteLine("page");


            // Navigate to a URL and wait until the page is fully loaded
            await page.GoToAsync("http://dciweb.dci.daikin.co.jp/EkbReportApp/LineReport", new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } // Wait until there are no network connections for at least 500ms
            });

            Console.WriteLine("open page");
            

            // Hide the scrollbar using CSS before taking the screenshot
            await page.EvaluateExpressionAsync(@"document.body.style.overflow = 'hidden'");



            // Take a full-page screenshot (without the scrollbar)
            await page.ScreenshotAsync(imgFileName, new ScreenshotOptions
            {
                FullPage = true
            });

            // Optionally, revert the overflow back to the default if necessary
            await page.EvaluateExpressionAsync(@"document.body.style.overflow = ''");

            Console.WriteLine("capture end");

            // Close the browser
            await browser.CloseAsync();


            #endregion

            await SendLineNotify("APS Notify", imgFileName);



            Console.WriteLine("end");
            //Console.ReadKey();



            /*
            
            #region main
            
            List<DataIN_OUT_Report_BY_TYPE> data_report = SrvReportAPS.getAPSReport();

            DataTable dtReport = new DataTable();
            dtReport.Columns.Add("WCNO", typeof(string));
            dtReport.Columns.Add("PART TYPE", typeof(string));
            dtReport.Columns.Add("PARTNO", typeof(string));
            dtReport.Columns.Add("STOCK", typeof(int));
            #endregion


            if (data_report.Count > 0)
            {
                foreach (DataIN_OUT_Report_BY_TYPE _mainData in data_report)
                {
                    dtReport.Rows.Add(_mainData.wcno, _mainData.partDesc, _mainData.partno, _mainData.bal_stock);
                }
                string directoryPath = @"E:\STOCK_WIP_NOTIFATION\image";

                //string directoryPath = @"D:\Project\2024\APS_STOCK_NOTIFICATION\APS_STOCK_NOTIFICATION\image";
                //SKImage image = ConvertDataTableToImage(dtReport);

                //using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 50))
                //    //using (var stream = File.OpenWrite("D:\\Project\\2024\\APS_STOCK_NOTIFICATION\\APS_STOCK_NOTIFICATION\\image\\Stock_warining.jpg"))

                //using (var stream = File.OpenWrite("D:\\www\\APS_STOCK_WARNING\\image\\Stock_warining.jpg"))
                //{
                //    data.SaveTo(stream);                 
                //}


                string[] imageFiles = Directory.GetFiles(directoryPath, "*.jpg");
                if (imageFiles.Length > 0)
                {
                    string imagePath = imageFiles[0];
                    await SendLineNotify("APS Notify", imagePath);

                }
                else
                {
                    Console.WriteLine("not pass");
                }

            }
            else
            {
                Console.WriteLine("no data");
            }
            */



        }
        public static SKImage ConvertDataTableToImage(DataTable table)
        {
            int cellWidth = 340;  // ¤ÇÒÁ¡ÇéÒ§¢Í§áµèÅÐà«ÅÅì
            int cellHeight = 40;  // ¤ÇÒÁÊÙ§¢Í§áµèÅÐà«ÅÅì
            int imageWidth = table.Columns.Count * cellWidth;
            int imageHeight = (table.Rows.Count + 1) * cellHeight; // ¨Ó¹Ç¹á¶Ç +1 ÊÓËÃÑºËÑÇ¢éÍ

            // ÊÃéÒ§ SKBitmap ÊÓËÃÑºÃÙ»ÀÒ¾
            var bitmap = new SKBitmap(imageWidth, imageHeight);
            using (var canvas = new SKCanvas(bitmap))
            {
                // µÑé§¤èÒÊÕ¾×é¹ËÅÑ§¢Í§ÃÙ»ÀÒ¾
                canvas.Clear(SKColors.White);

                // µÑé§¤èÒÊäµÅì¢Í§µÑÇÍÑ¡ÉÃ
                var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextSize = 22,
                    Style = SKPaintStyle.Fill
                };

                for (int i = 0; i < table.Columns.Count; i++)
                {


                    canvas.DrawRect(i * cellWidth, 0, cellWidth, cellHeight, new SKPaint
                    {
                        Color = SKColors.Black,
                        Style = SKPaintStyle.Stroke
                    });

                    // à¢ÕÂ¹ª×èÍ¤ÍÅÑÁ¹ì
                    canvas.DrawText(table.Columns[i].ColumnName, i * cellWidth + 5, 25, paint);
                }

                // ÇÒ´¢éÍÁÙÅã¹áµèÅÐá¶Ç
                for (int row = 0; row < table.Rows.Count; row++)
                {
                    for (int col = 0; col < table.Columns.Count; col++)
                    {

                        //if (col == 0)
                        //{
                        //    cellWidth = 100;
                        //}
                        //else if (col == 2)
                        //{
                        //    cellWidth = 200;
                        //}
                        //else if (col == 3)
                        //{
                        //    cellWidth = 500;
                        //}
                        //else if (col == 4)
                        //{
                        //    cellWidth = 50;
                        //}

                        // ÇÒ´¢Íºà«ÅÅì
                        canvas.DrawRect(col * cellWidth, (row + 1) * cellHeight, cellWidth, cellHeight, new SKPaint
                        {
                            Color = SKColors.Black,
                            Style = SKPaintStyle.Stroke
                        });

                        // à¢ÕÂ¹¢éÍÁÙÅ¢Í§áµèÅÐà«ÅÅì
                        canvas.DrawText(table.Rows[row][col].ToString(), col * cellWidth + 5, (row + 1) * cellHeight + 25, paint);
                    }
                }
            }

            return SKImage.FromBitmap(bitmap);
        }

        public static async Task SendLineNotify(string message, string? imagePath = "")
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // Add other versions if needed

                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                //HttpClient client = new HttpClient(handler);

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {lineNotifyToken}");

                    using (var form = new MultipartFormDataContent())
                    {
                        form.Add(new StringContent(message), "message");

                        if (imagePath != "")
                        {

                            byte[] imageData = File.ReadAllBytes(imagePath);
                            var imageContent = new ByteArrayContent(imageData);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); 

                            form.Add(imageContent, "imageFile", Path.GetFileName(imagePath));
                        }

                        var response = await client.PostAsync("https://147.92.242.65/api/notify", form);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("success!");
                        }
                        else
                        {
                            Console.WriteLine($"fail: {response.StatusCode}");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"catch: {ex}");
            }
        }




    }

}
