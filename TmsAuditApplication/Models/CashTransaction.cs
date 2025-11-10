

using iTextSharp.text;
using Microsoft.Extensions.Options;

namespace VrsAuditApplication.Models
{

    public class DeclareCashupModel
    {
        public string BagNumber { get; set; }
        public string SelectedShift { get; set; }
        public string UserName { get; set; }
    }

    public class FastTagTransaction
    {
        public string Tolltransactionnumber { get; set; }       // 44929402
        public string ActTransactionID { get; set; }            // 536537062407251844417
        public string PlateNumber { get; set; }                 // MH02CE5902
        public string ShiftNumber { get; set; }                 // NULL
        public string Laneid { get; set; }                      // 6
        public string DateTime { get; set; }                    // 2025-07-24 18:44:41.733
        public string Tagid { get; set; }                       // 34161FA820328EE8344A6FA0
        public string BankClass { get; set; }                   // N/A
        public string BankClassID { get; set; }                 // (empty)
        public string AVCClass { get; set; }                    // CAR
        public string AVCClassID { get; set; }                  // 1
        public string TagClass { get; set; }                    // Car / Jeep / Van
        public string Amount { get; set; }                      // 60.00
        public string FareType { get; set; }                    // FULL
        public string TCR { get; set; }                         // (empty)
    }

    public class ManualFastTagTransaction
    {
        public string Tolltransactionnumber { get; set; }
        public string ActTransactionID { get; set; }
        public string PlateNumber { get; set; }
        public string ShiftNumber { get; set; }
        public string Laneid { get; set; }
        public string DateTime { get; set; }
        public string Tagid { get; set; }
        public string AVCClass { get; set; }
        public string AVCClassID { get; set; }
        public string TagClass { get; set; }
        public string Amount { get; set; }
        public string FareType { get; set; }
        public string TCR { get; set; }
        public bool IsValidated { get; set; }
    }



    public class NonFastTagTransaction
    {
        public long TollTransactionNumber { get; set; }
        public int ShiftNumber { get; set; }
        public int LaneID { get; set; }
        public int MeansID { get; set; }
        public DateTime DateTime { get; set; }
        public long ReceiptNumber { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public int TCClass { get; set; }
        public int AVCClass { get; set; }
        public int PaymentMeansType { get; set; }
        public string PaymentMode { get; set; }
        public int ExemptionID { get; set; }
        public string PlateNumber { get; set; }
        public bool Simulation { get; set; }
        public int AccountID { get; set; }
        public int FareID { get; set; }
        public int SWBWeight { get; set; }
        public decimal SWBAmount { get; set; }
        public int? WimWeight { get; set; }
        public string IsUnderWeight { get; set; }
        public string TCR { get; set; }
        public string ReferenceID { get; set; }
        public int UserID { get; set; }
        public string ValidatorClass { get; set; }
        public string WimTCID { get; set; }
        public string Subclass { get; set; }
        public string TowVehicle { get; set; }
        public string BagNumber { get; set; }
    }

    public class MediaPathsOptions
    {
        public string ImagefolderPath { get; set; }
        public string VideoFolderPath { get; set; }
    }

    public class MediaPaths
    {
        public string VideoPath { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>();
    }

    public class MediaHelper
    {
        private readonly MediaPathsOptions _options;
        private readonly string logFile;

        public MediaHelper(IOptions<MediaPathsOptions> options)
        {
            _options = options.Value;
            //logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaHelperLog.txt");
        }

        public MediaPaths ShowAllMedia(string transactionNumber, string shiftNumber)
        {
            MediaPaths media = new MediaPaths();

            try
            {
                //Log($"Start processing transaction: {transactionNumber} {shiftNumber}");

                // ✅ Assign paths directly here
                string imageRoot = @"E:\Plaza\Download\NPRImages";
                string cchImageRoot = @"E:\Plaza\Download\CCHImages";
                string videoRoot = @"E:\Plaza\Download\Images";

                if (string.IsNullOrEmpty(imageRoot) || string.IsNullOrEmpty(videoRoot))
                {
                    //Log($"Image {imageRoot} or video {videoRoot} root path is not configured properly.");
                    return media;
                }

                string imageFile1 = ShowLaneICSImage(imageRoot, transactionNumber, shiftNumber, "dummy.jpg");
                string imageFile2 = ShowLaneLPICImage(imageRoot, transactionNumber, shiftNumber, "dummy.jpg");
                string imageFile3 = ShowScitaImage(cchImageRoot, transactionNumber, shiftNumber, "dummy.jpg");
                string videoFile = PlayLaneVideo(videoRoot, transactionNumber, shiftNumber);

                media.ImagePaths.Add($"/Plaza/Download/NPRImages/{imageFile1}");
                media.ImagePaths.Add($"/Plaza/Download/NPRImages/{imageFile2}");
                media.ImagePaths.Add($"/Plaza/Download/CCHImages/{imageFile3}");
                media.VideoPath = $"Plaza/Download/Images/{videoFile}";

                //Log($"Checking image: {imageFile1}");


            }
            catch (Exception ex)
            {
                //Log($"Exception: {ex.Message}\n{ex.StackTrace}");
            }

            return media;
        }


        public string ShowLaneICSImage(string imageRoot, string transactionNumber, string shiftNumber, string dummyImage)
        {
            string imageFile = $"{shiftNumber}/{transactionNumber}A.jpg";
            //Log($"Checking imageFile: {imageFile}");
            string imagePath = Path.Combine(imageRoot, imageFile);
            if (File.Exists(imagePath))
            {
                //Log($"Checking imagePath: {imagePath}");
                return imageFile;
            }
            else
            {
                return dummyImage; // Return dummy image if not found
            }

        }

        public string ShowLaneLPICImage(string imageRoot, string transactionNumber, string shiftNumber, string dummyImage)
        {
            string imageFile = $"{shiftNumber}/{transactionNumber}B.jpg";
            string imagePath = Path.Combine(imageRoot, imageFile);
            if (File.Exists(imagePath))
            {
                //Log($"Checking imagePath: {imagePath}");
                return imageFile;
            }
            else
            {
                return dummyImage; // Return dummy image if not found
            }
        }

        public string ShowScitaImage(string cchImageRoot, string transactionNumber, string shiftNumber, string dummyImage)
        {
            string imageFile = $"{transactionNumber}B.jpg";
            string imagePath = Path.Combine(cchImageRoot, imageFile);
            if (File.Exists(imagePath))
            {
                //Log($"Checking imagePath: {imagePath}");
                return imageFile;
            }
            else
            {
                return dummyImage; // Return dummy image if not found
            }
        }

        public string PlayLaneVideo(string videoRoot, string transactionNumber, string shiftNumber)
        {
            string videoFile = $"{shiftNumber}/{transactionNumber}.mp4";
            string videoPath = Path.Combine(videoRoot, videoFile);
            if (File.Exists(videoPath))
            {
                //Log($"Checking videoPath: {videoPath}");
                return videoFile;
            }
            else
            {
                return ""; // Return dummy video if not found
            }
        }

        //private void Log(string message)
        //{
        //    File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}\n");
        //}
    }


    public class CashTransaction
    {

    }

}
