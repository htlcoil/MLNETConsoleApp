using Microsoft.ML.Data;

namespace MLNETConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //itrate through all the images in a specific folder    
            //var files = Directory.GetFiles(@"C:\Users\webma\Downloads\coins");
            //foreach (var file in files)

            //{

            var files = Directory.GetFiles(@"C:\Dev\ml.net\coins","*.jpg");
            // Create single instance of sample data from first line of dataset for model input.
            foreach (var imageFile in files)
            {               
                var image = MLImage.CreateFromFile(imageFile);//@"C:\Users\webma\Downloads\101055.jpg");//CreateFromFile(@"C:\Dev\ml.net\coins\db90e9b7-ido_63_jpg.rf.276e83b12a8fe5f532d60262a1c3aa36.jpg");
                MLCoinsModel.ModelInput sampleData = new MLCoinsModel.ModelInput()
                {
                    Image = image,
                };
                // Make a single prediction on the sample data and print results.
                var predictionResult = MLCoinsModel.Predict(sampleData);
                Console.WriteLine("\n\nPredicted Boxes:\n");
                if (predictionResult.PredictedBoundingBoxes == null)
                {
                    Console.WriteLine("No Predicted Bounding Boxes");
                    return;
                }
                var boxes =
                    predictionResult.PredictedBoundingBoxes.Chunk(4)
                        .Select(x => new { XTop = x[0], YTop = x[1], XBottom = x[2], YBottom = x[3] })
                        .Zip(predictionResult.Score, (a, b) => new { Box = a, Score = b });

                foreach (var item in boxes)
                {
                    Console.WriteLine($"XTop: {item.Box.XTop},YTop: {item.Box.YTop},XBottom: {item.Box.XBottom},YBottom: {item.Box.YBottom}, Score: {item.Score}");
                }
                int totals = 0;
                foreach (var lable in predictionResult.PredictedLabel)
                {
                    totals += Convert.ToInt32(lable);
                    Console.WriteLine($"Lable: {lable}");
                }
                Console.WriteLine($"Total sum of coins in the image: {totals}"); 
            }
        }
    }
}
