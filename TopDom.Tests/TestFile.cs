using Microsoft.AspNetCore.Http;

namespace TopDom.Tests;

public static class TestFile
{
    public static IFormFile Create(string contentType, long length, string fileName = "test.jpg")
    {
        var stream = new MemoryStream(new byte[Math.Max(length, 1)]);
        return new FormFile(stream, 0, length, "imageFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}