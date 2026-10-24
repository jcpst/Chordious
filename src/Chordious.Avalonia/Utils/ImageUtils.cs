// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.IO;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using SkiaSharp;
using Svg.Skia;

using Chordious.Core.ViewModel;

using Chordious.Avalonia.Resources;

namespace Chordious.Avalonia
{
    public class ImageUtils
    {
        public static AppViewModel AppVM => AppViewModel.Instance;

        #region SVG Rendering

        /// <summary>
        /// Creates the image shown on screen for a diagram. It's drawn as vectors when visible,
        /// so it's cheap to create, small in memory and sharp at any display scale.
        /// </summary>
        public static IImage SvgTextToDisplayImage(string svgText, int width, int height, bool editMode)
        {
            Background background = editMode ? GetEditorRenderBackground() : GetRenderBackground();
            return new SvgDiagramImage(svgText, width, height, background);
        }

        public static SKBitmap SvgTextToSKBitmap(string svgText, int width, int height, float scaleFactor, Background background)
        {
            if (string.IsNullOrEmpty(svgText))
            {
                throw new ArgumentNullException(nameof(svgText));
            }

            if (width <= 0 || width > MaxBitmapDimension)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0 || height > MaxBitmapDimension)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (scaleFactor <= 0 || scaleFactor > GetMaxScaleFactor(width, height))
            {
                throw new ArgumentOutOfRangeException(nameof(scaleFactor));
            }

            using SKSvg svg = new SKSvg();
            SKPicture picture = svg.FromSvg(svgText) ?? throw new ArgumentException("Unable to parse SVG.", nameof(svgText));

            int pixelWidth = (int)Math.Ceiling(width * scaleFactor);
            int pixelHeight = (int)Math.Ceiling(height * scaleFactor);

            SKBitmap bitmap = new SKBitmap(new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul));

            using SKCanvas canvas = new SKCanvas(bitmap);
            DrawBackground(canvas, pixelWidth, pixelHeight, background);

            canvas.Scale(scaleFactor);
            canvas.DrawPicture(picture);
            canvas.Flush();

            return bitmap;
        }

        private static void DrawBackground(SKCanvas canvas, int width, int height, Background background)
        {
            canvas.Clear(background == Background.White ? SKColors.White : SKColors.Transparent);

            if (background == Background.Transparent)
            {
                // Checkerboard, like the WPF version's tiled transparent16.png
                const int Tile = 8;
                using SKPaint light = new SKPaint() { Color = SKColors.White };
                using SKPaint dark = new SKPaint() { Color = new SKColor(0xCC, 0xCC, 0xCC) };
                for (int y = 0; y < height; y += Tile)
                {
                    for (int x = 0; x < width; x += Tile)
                    {
                        canvas.DrawRect(x, y, Tile, Tile, ((x / Tile) + (y / Tile)) % 2 == 0 ? light : dark);
                    }
                }
            }
        }

        public static Bitmap SKBitmapToBitmap(SKBitmap bitmap)
        {
            return new Bitmap(PixelFormat.Bgra8888,
                              AlphaFormat.Premul,
                              bitmap.GetPixels(),
                              new PixelSize(bitmap.Width, bitmap.Height),
                              new Vector(96.0, 96.0),
                              bitmap.RowBytes);
        }

        public static byte[] SKBitmapToPng(SKBitmap bitmap)
        {
            using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public static string BitmapToPngTempFile(SKBitmap bitmap, string title)
        {
            string tempPath = IntegrationUtils.GetTempPathAndCreateIfNecessary();

            string random = Path.GetRandomFileName().Substring(0, 8);

            string filePath = FolderUtils.CleanPath(Path.Combine(tempPath, string.IsNullOrWhiteSpace(title) ? $"chordious.{random}.png" : $"chordious.{FolderUtils.CleanTitle(title)}.{random}.png"));

            File.WriteAllBytes(filePath, SKBitmapToPng(bitmap));

            return filePath;
        }

        #endregion

        #region Export

        public static void ExportImageFile(string svgText, int width, int height, ExportFormat exportFormat, float scaleFactor, string filePath)
        {
            if (string.IsNullOrWhiteSpace(svgText))
            {
                throw new ArgumentNullException(nameof(svgText));
            }

            if (width <= 0 || (exportFormat != ExportFormat.SVG && width > MaxBitmapDimension))
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0 || (exportFormat != ExportFormat.SVG && height > MaxBitmapDimension))
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (scaleFactor <= 0 || (exportFormat != ExportFormat.SVG && scaleFactor > GetMaxScaleFactor(width, height)))
            {
                throw new ArgumentOutOfRangeException(nameof(scaleFactor));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            if (exportFormat == ExportFormat.SVG)
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(svgText);
                sw.Flush();
            }
            else
            {
                // JPG has no alpha channel, so give it a white background
                Background background = exportFormat == ExportFormat.JPG ? Background.White : Background.None;
                SKEncodedImageFormat format = exportFormat == ExportFormat.JPG ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png;

                using SKBitmap bitmap = SvgTextToSKBitmap(svgText, width, height, scaleFactor, background);
                using SKData data = bitmap.Encode(format, 95);
                data.SaveTo(fs);
            }
        }

        #endregion

        #region Limits

        public static float GetMaxScaleFactor(int width, int height)
        {
            if (width <= 0 || width > MaxBitmapDimension)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0 || height > MaxBitmapDimension)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            float maxByDimensionConstraint = MaxBitmapDimension / (float)Math.Max(width, height);

            float maxByMemoryConstraint = (float)Math.Sqrt(MaxBitmapDimension * (MaxBitmapDimension / (width * (double)height)));

            return Math.Min(maxByDimensionConstraint, maxByMemoryConstraint);
        }

        private static readonly int MaxBitmapDimension = 32768;

        #endregion

        #region Settings

        public static Background GetRenderBackground()
        {
            if (Enum.TryParse(AppVM.GetSetting("app.renderbackground"), out Background result))
            {
                return result;
            }

            return Background.None;
        }

        public static Background GetEditorRenderBackground()
        {
            if (Enum.TryParse(AppVM.GetSetting("diagrameditor.renderbackground"), out Background result))
            {
                return result;
            }

            return Background.None;
        }

        public static ObservableCollection<string> GetBackgrounds()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>
            {
                Strings.BackgroundNoneFriendlyValue,
                Strings.BackgroundWhiteFriendlyValue,
                Strings.BackgroundTransparentFriendlyValue
            };

            return collection;
        }

        #endregion
    }

    public enum Background
    {
        None,
        White,
        Transparent
    }

    public enum ExportFormat
    {
        SVG = 0,
        PNG,
        GIF, // Not supported by Skia's encoders; kept so saved settings stay compatible with the WPF build
        JPG
    }
}
