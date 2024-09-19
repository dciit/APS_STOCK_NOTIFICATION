using APS_STOCK_NOTIFICATION.Controller;
using APS_STOCK_NOTIFICATION.Model;
using SkiaSharp;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace APS_STOCK_NOTIFICATION
{
    internal class Program
    {

        private static readonly string lineNotifyToken = "9VWUsTW1DwYrwYLCGafYFo1848adYWaIufNmU4K2fFP";
        static ReportAPSController SrvReportAPS = new ReportAPSController();
        
        static async Task Main(string[] args)
        {
            List<DataIN_OUT_Report_ALL> data_report = SrvReportAPS.getAPSReport();


            DataTable dt = new DataTable();
            dt.Columns.Add("WCNO", typeof(string));
            dt.Columns.Add("PART TYPE", typeof(string));
            dt.Columns.Add("PARTNO", typeof(string));
            dt.Columns.Add("STOCK", typeof(int));


            if (data_report.Count > 0)
            {
                foreach (DataIN_OUT_Report_ALL _mainData in data_report)
                {
                    foreach (DataIN_OUT_Report_BY_TYPE _subData in _mainData.reportAll)
                    {
                        //string message = "";
                        //message = "\nWCNO: " + _subData.wcno + "\n";
                        //message += "PART TYPE: " + _mainData.part_type + "\n";
                        //message += "PARTNO: " + _subData.partno + " " + _subData.cm + "\n";               
                        //message += "STOCK: " + _subData.bal_stock + "\n";
                        //await SendLineNotify(message);



                        dt.Rows.Add(_subData.wcno, _mainData.part_type, _subData.partno + " " + _subData.cm, _subData.bal_stock);



                    }
                }

                string directoryPath = @"D:\www\APS_STOCK_WARNING\image";
                SKImage image = ConvertDataTableToImage(dt);

                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite("D:\\www\\APS_STOCK_WARNING\\image\\Stock_warining.jpg"))
                {
                    data.SaveTo(stream);

                }


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
                    // ÇÒ´¢Íºà«ÅÅì
                    //if (i == 0)
                    //{
                    //    cellWidth = 150;
                    //}
                    //else if (i == 2)
                    //{
                    //    cellWidth = 200;
                    //}
                    //else if (i == 3)
                    //{
                    //    cellWidth = 400;
                    //}
                    //else if (i == 4)
                    //{
                    //    cellWidth = 50;
                    //}

                    canvas.DrawRect(i * cellWidth, 0, cellWidth, cellWidth, new SKPaint
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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {lineNotifyToken}");

                    using (var form = new MultipartFormDataContent())
                    {
                        form.Add(new StringContent(message), "message");

                        if (imagePath != "") {

                            byte[] imageData = File.ReadAllBytes(imagePath);
                            var imageContent = new ByteArrayContent(imageData);
                            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");  // ËÒ¡à»ç¹ä¿ÅìÍ×è¹àªè¹ PNG ãËéà»ÅÕèÂ¹à»ç¹ "image/png"

                            form.Add(imageContent, "imageFile", Path.GetFileName(imagePath));
                        }

                       var response = await client.PostAsync("https://notify-api.line.me/api/notify", form);

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
