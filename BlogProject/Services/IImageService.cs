using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.Services
{
    public interface IImageService
    {
        //converts image into a byte array. Prevents malicious content from being uploaded into our database.
        //interacting with inputted files
        Task<byte[]> EncodeImageAsync(IFormFile file);

        //used for default images stored within project
        Task<byte[]> EncodeImageAsync(string fileName);

        string DecodeImage(byte[] data, string type);

        //file type
        string ContentType(IFormFile file);

        //file size
        int Size(IFormFile file);

    }
}
