using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorThief.ImageSharp.Test
{
    public class Tests
    {
        private const string Path = "./TestImage.jpg";
        private const int ImageSize = 153927;

        [Test]
        [Order(0)]
        public void ReadImageTest()
        {
            using var fs = File.OpenRead(Path);

            Assert.That(fs, Has.Length.EqualTo(ImageSize));
        }

        [Test]
        [Order(1)]
        public void GetPaletteTest()
        {
            using var fs = File.OpenRead(Path);
            var colorThief = new ColorThief();

            var palette = colorThief.GetPalette(Image.Load<Rgba32>(fs));

            Assert.That(palette.Count, Is.EqualTo(5));
        }
    }
}