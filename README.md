# ColorThief.ImageSharp

### The library to resolve color palette for [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) images.

### This is a ported project of [Color Thief](https://github.com/KSemenenko/ColorThief)

## How to use

### Get the dominant color from an image
```cs
var colorThief = new ColorThief();
colorThief.GetColor(sourceImage);
```

### Build a color palette from an image

In this example, we build an 8 color palette.

```cs
var colorThief = new ColorThief();
colorThief.GetPalette(sourceImage, 8);
```

#### You can see more examples for loading an image and get the palette of it in `ColorThief.ImageSharp.Test`
