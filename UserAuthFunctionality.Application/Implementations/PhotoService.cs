using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using UserAuthFunctionality.Application.Exceptions;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Application.Settings;

namespace UserAuthFunctionality.Application.Implementations
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }
        public async Task DeletePhotoAsync(string imageUrl)
        {
            string publicId = await ExtractPublicIdFromUrl(imageUrl);
            var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
            var result = await _cloudinary.DestroyAsync(deletionParams);
            if (result.Result != "ok")
                throw new CustomException(500, "Image", "Image delete error");
        }
        public async Task<string> UploadPhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var uploadResult = new ImageUploadResult();

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill")
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl.ToString();
        }
        private async Task<string> ExtractPublicIdFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string[] segments = uri.Segments;

            string lastSegment = segments[^1];
            string publicId = lastSegment.Substring(0, lastSegment.LastIndexOf('.'));

            return publicId;
        }
    }

}
