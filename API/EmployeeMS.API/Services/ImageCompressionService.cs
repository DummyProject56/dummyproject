using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
public static class ImageCompressionService
{
    private const int TargetBytes = 1024 * 1024;

    public static async Task<byte[]> CompressAsync(Stream stream)
    {
        using var image = await Image.LoadAsync(stream);

        image.Mutate(x => x.AutoOrient());

        for (int width = Math.Min(image.Width, 1920); width >= 320; width = (int)(width * 0.8))
        {
            using var resized = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(width, 0),
                Mode = ResizeMode.Max
            }));

            for (int quality = 85; quality >= 35; quality -= 10)
            {
                using var output = new MemoryStream();

                await resized.SaveAsJpegAsync(output, new JpegEncoder { Quality = quality });

                if (output.Length <= TargetBytes)
                    return output.ToArray();
            }
        }

        throw new InvalidOperationException("Image cannot be compressed below 1 MB.");
    }

}
