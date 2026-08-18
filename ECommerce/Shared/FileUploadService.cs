namespace ECommerce.Shared
{
    public interface IFileService
    {
        Task<string> UploadFileWithBase64Async(string base64Data, string Extension);
    }
    public class FileUploadService: IFileService
    {
        public async Task<string> UploadFileWithBase64Async(string base64Data, string Extension)
        {
            byte[] imageBytes = [];
            if (!string.IsNullOrEmpty(base64Data))
            {
                if (base64Data.Contains("base64,"))
                {
                    base64Data = base64Data.Substring(base64Data.IndexOf("base64,") + 7);
                    imageBytes = Convert.FromBase64String(base64Data);
                }
            }
            var FileName = Guid.NewGuid().ToString() + '.' + Extension;
            if (imageBytes != null)
            {
                return await UploadFileAsync(imageBytes, FileName);
            }
            return string.Empty;
        }
        public async Task<string> UploadFileAsync(byte[] file, string FileName)
        {
            //var CompanyName = await GetCompanyDetails(Code);
            string ReturnPath = string.Empty;
            if (file != null)
            {
                var streamData = new MemoryStream(file);
                var UploadFolder = Path.Combine( "D://Images");
                //var FileName = Guid.NewGuid().ToString() + "_";
                var FullPath = Path.Combine(UploadFolder, FileName);
                if (!Directory.Exists(UploadFolder))
                {
                    Directory.CreateDirectory(UploadFolder);
                }
                using (var fileStream = new FileStream(FullPath, FileMode.Create))
                {
                    streamData.CopyTo(fileStream);
                }
                ReturnPath = Path.Combine("Images", FileName);
                return ReturnPath;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
