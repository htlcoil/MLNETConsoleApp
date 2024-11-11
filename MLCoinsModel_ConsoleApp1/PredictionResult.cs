using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.Data;
using SkiaSharp;
using System.Drawing;

namespace MLCoinsModel_ConsoleApp1
{
    public class PredictionResult
    {
        public string? ImagePath { get; set; }
        public bool CoinsFound { get; set; }
        public List<string>? BoundingBoxes { get; set; }
        public List<BoundingBox> BoundingBoxesList { get; set; }
        public List<string>? DetectedCoins { get; set; }
        public decimal TotalSum { get; set; }
    }

    public class PDFGenerator
    {
        public static void GeneratePDF(List<PredictionResult> predictionResults, string outputFilePath)
        {
            Document doc = new Document();
            PdfWriter.GetInstance(doc, new FileStream(outputFilePath, FileMode.Create));
            doc.Open();
            foreach (var predictionResult in predictionResults)
            {
                doc.Add(new Paragraph($"Image Path: {predictionResult.ImagePath}"));
                doc.Add(new Paragraph($"Coins Found: {predictionResult.CoinsFound}"));
                if (predictionResult.CoinsFound)
                {
                    doc.Add(new Paragraph("Bounding Boxes:"));
                    foreach (var box in predictionResult.BoundingBoxes)
                    {
                        doc.Add(new Paragraph(box));
                    }
                    doc.Add(new Paragraph("Detected Coins:"));
                    foreach (var coin in predictionResult.DetectedCoins)
                    {
                        doc.Add(new Paragraph(coin));
                    }
                    doc.Add(new Paragraph($"Total Sum: {predictionResult.TotalSum}"));
                }
                doc.Add(new Paragraph("\n"));
            }
            doc.Close();
        }

        public static void CreatePdfReport(List<PredictionResult> results, string outputPath)
        {
            using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, stream);
                document.Open();

                PdfPTable table = new PdfPTable(6);
                table.AddCell("Image");
                table.AddCell("Image Path");
                table.AddCell("Coins Found");
                table.AddCell("Bounding Boxes");
                table.AddCell("Detected Coins");
                table.AddCell("Total Sum");
                table.HeaderRows = 1;
                table.WidthPercentage = 100;
                foreach (PredictionResult result in results)
                {
                    // add the acctual image to the pdf
                    var image = result.CoinsFound? result.DrawBoundingBox() : iTextSharp.text.Image.GetInstance(result.ImagePath);
                    table.AddCell(image);
                    table.AddCell(result.ImagePath);
                    table.AddCell(result.CoinsFound ? "Yes" : "No");
                    table.AddCell(result.CoinsFound ? string.Join(", ", result.BoundingBoxes) : string.Empty);
                    table.AddCell(result.CoinsFound ? string.Join(", ", result.DetectedCoins) : string.Empty);
                    table.AddCell(result.CoinsFound ? result.TotalSum.ToString() : string.Empty);
                }

                document.Add(table);
                document.Close();
            }
        }
       
    }

    //create a new class to represent the Bounding Box on an image
    public class BoundingBox
    {
        public float XTop { get; set; }
        public float YTop { get; set; }
        public float XBottom { get; set; }
        public float YBottom { get; set; }
    }
    public static class Extensions
    {        
        public static iTextSharp.text.Image DrawBoundingBox(this PredictionResult predictionResult)
        {
            //draw bounding box on the image in predictionResult.ImagePath and return it as a new iTextSharp.text.Image 
            using (var bitmap = new Bitmap(predictionResult.ImagePath))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var pen = new Pen(System.Drawing.Color.Red, 2);
                    foreach (var box in predictionResult.BoundingBoxesList)
                    {
                        graphics.DrawRectangle(pen, box.XTop, box.YTop, box.XBottom - box.XTop, box.YBottom - box.YTop);
                    }
                }
                //convert the bitmap to byte array
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    var image = iTextSharp.text.Image.GetInstance(ms.GetBuffer());
                    return image;
                }                
            }
        }        

    }
}
