using MLCoinsModel_ConsoleApp1;
using Microsoft.ML.Data;
List<PredictionResult> predictionResults = new List<PredictionResult>();
// Create single instance of sample data from first line of dataset for model input.
//var image = MLImage.CreateFromFile(@"C:\Users\webma\Downloads\101055.jpg");//CreateFromFile(@"C:\Dev\ml.net\coins\db90e9b7-ido_63_jpg.rf.276e83b12a8fe5f532d60262a1c3aa36.jpg");
string? coinsImagesFolder = string.Empty;
Console.WriteLine("Coins Detection with ML.NET");
while (string.IsNullOrEmpty(coinsImagesFolder) || !Directory.Exists(coinsImagesFolder))
{
    Console.WriteLine("Enter the path to directory where the coins are:");
    coinsImagesFolder = Console.ReadLine();
    if (!Directory.Exists(coinsImagesFolder))
    {
        Console.WriteLine("The directory does not exist. Please enter a valid directory path.");
    }
}
var files = Directory.GetFiles(coinsImagesFolder, "*.jpg");//(@"C:\Dev\ml.net\coins", "*.jpg");
// Create single instance of sample data from first line of dataset for model input.
Console.WriteLine($"Predicting {files.Count()} images in the folder");
int totalDetectedImages = 0;
int totalNotDetectedImages = 0;
foreach (var imageFile in files)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"Predicting image: {imageFile}");
    var image = MLImage.CreateFromFile(imageFile);
    MLCoinsModel.ModelInput sampleData = new MLCoinsModel.ModelInput()
    {
        Image = image,
    };
    // Make a single prediction on the sample data and print results.
    var predictionResult = MLCoinsModel.Predict(sampleData);
    PredictionResult currentPredictionResult = new PredictionResult();
    currentPredictionResult.ImagePath = imageFile;
    Console.WriteLine("\n\nPredicted Boxes:\n");
    if (predictionResult.PredictedBoundingBoxes == null)
    {
        currentPredictionResult.CoinsFound = false;
        predictionResults.Add(currentPredictionResult);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No Predicted Bounding Boxes");
        totalNotDetectedImages++;
        //return;
        continue;
    }
    totalDetectedImages++;
    currentPredictionResult.CoinsFound = true;
    currentPredictionResult.BoundingBoxes = new List<string>();
    currentPredictionResult.DetectedCoins = new List<string>();
    currentPredictionResult.BoundingBoxesList = new List<BoundingBox>();
    var boxes =
        predictionResult.PredictedBoundingBoxes.Chunk(4)
            .Select(x => new { XTop = x[0], YTop = x[1], XBottom = x[2], YBottom = x[3] })
            .Zip(predictionResult.Score, (a, b) => new { Box = a, Score = b });
    foreach (var item in boxes)
    {
        currentPredictionResult.BoundingBoxesList.Add(new BoundingBox
        {
            XTop = item.Box.XTop,
            YTop = item.Box.YTop,
            XBottom = item.Box.XBottom,
            YBottom = item.Box.YBottom
        });
        currentPredictionResult.BoundingBoxes.Add($"XTop: {item.Box.XTop},YTop: {item.Box.YTop},XBottom: {item.Box.XBottom},YBottom: {item.Box.YBottom}, Score: {item.Score}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"XTop: {item.Box.XTop},YTop: {item.Box.YTop},XBottom: {item.Box.XBottom},YBottom: {item.Box.YBottom}, Score: {item.Score}");
    }
    int totals = 0;
    foreach (var lable in predictionResult.PredictedLabel)
    {
        currentPredictionResult.DetectedCoins.Add(lable);
        totals += Convert.ToInt32(lable);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Lable: {lable}");
    }
    currentPredictionResult.TotalSum = totals;
    Console.WriteLine($"Total sum of coins in the image: {totals}");
    predictionResults.Add(currentPredictionResult);
}
Console.ForegroundColor = ConsoleColor.White;
string pdfReportPath = Path.Combine(coinsImagesFolder, $"coins-detection-results-{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}.pdf");
PDFGenerator.CreatePdfReport(predictionResults, $@"{pdfReportPath}");
Console.WriteLine($"Total detected images: {totalDetectedImages} from {files.Count()}");
Console.WriteLine($"Total not detected images: {totalNotDetectedImages} from {files.Count()}");
Console.WriteLine($"PDF report generated at: {pdfReportPath}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();