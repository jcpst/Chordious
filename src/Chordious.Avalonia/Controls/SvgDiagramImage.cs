// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

using SkiaSharp;
using Svg.Skia;

namespace Chordious.Avalonia
{
    /// <summary>
    /// A diagram image that keeps the parsed SVG as a (small) vector picture and draws it with Skia
    /// when it's actually rendered. Compared to pre-rendering a bitmap per diagram, this makes
    /// creating finder results nearly free, uses a few KB instead of ~1 MB per diagram, and stays
    /// sharp at any display scale.
    /// </summary>
    public sealed class SvgDiagramImage : IImage
    {
        private readonly SKSvg _svg; // Owns the picture, so keep it alive with this image
        private readonly SKPicture _picture;
        private readonly Background _background;

        public Size Size { get; }

        public SvgDiagramImage(string svgText, int width, int height, Background background)
        {
            if (string.IsNullOrEmpty(svgText))
            {
                throw new ArgumentNullException(nameof(svgText));
            }

            _svg = new SKSvg();
            _picture = _svg.FromSvg(svgText) ?? throw new ArgumentException("Unable to parse SVG.", nameof(svgText));
            _background = background;
            Size = new Size(width, height);
        }

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            context.Custom(new DrawOperation(this, sourceRect, destRect));
        }

        private void Render(SKCanvas canvas, Rect sourceRect, Rect destRect)
        {
            int saveCount = canvas.Save();
            try
            {
                canvas.ClipRect(ToSKRect(destRect));

                // Map diagram coordinates (sourceRect) onto the destination
                canvas.Translate((float)destRect.X, (float)destRect.Y);
                canvas.Scale((float)(destRect.Width / sourceRect.Width), (float)(destRect.Height / sourceRect.Height));
                canvas.Translate((float)-sourceRect.X, (float)-sourceRect.Y);

                SKRect bounds = new SKRect(0, 0, (float)Size.Width, (float)Size.Height);
                if (_background == Background.White)
                {
                    using SKPaint white = new SKPaint() { Color = SKColors.White };
                    canvas.DrawRect(bounds, white);
                }
                else if (_background == Background.Transparent)
                {
                    using SKPaint checker = new SKPaint() { Shader = CheckerShader };
                    canvas.DrawRect(bounds, checker);
                }

                canvas.DrawPicture(_picture);
            }
            finally
            {
                canvas.RestoreToCount(saveCount);
            }
        }

        private static SKRect ToSKRect(Rect rect) => new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);

        /// <summary>
        /// The light/dark checkerboard shown behind diagrams with a "transparent" background.
        /// </summary>
        private static SKShader CheckerShader => _checkerShader ??= CreateCheckerShader();
        private static SKShader _checkerShader;

        private static SKShader CreateCheckerShader()
        {
            const int Tile = 8;
            using SKBitmap bitmap = new SKBitmap(Tile * 2, Tile * 2);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                using SKPaint dark = new SKPaint() { Color = new SKColor(0xCC, 0xCC, 0xCC) };
                canvas.DrawRect(Tile, 0, Tile, Tile, dark);
                canvas.DrawRect(0, Tile, Tile, Tile, dark);
            }
            return SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
        }

        private sealed class DrawOperation : ICustomDrawOperation
        {
            private readonly SvgDiagramImage _image;
            private readonly Rect _sourceRect;

            public Rect Bounds { get; }

            public DrawOperation(SvgDiagramImage image, Rect sourceRect, Rect destRect)
            {
                _image = image;
                _sourceRect = sourceRect;
                Bounds = destRect;
            }

            public void Render(ImmediateDrawingContext context)
            {
                if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is ISkiaSharpApiLeaseFeature leaseFeature)
                {
                    using ISkiaSharpApiLease lease = leaseFeature.Lease();
                    _image.Render(lease.SkCanvas, _sourceRect, Bounds);
                }
            }

            public bool HitTest(Point p) => Bounds.Contains(p);

            public bool Equals(ICustomDrawOperation other)
            {
                return other is DrawOperation op && op._image == _image && op._sourceRect == _sourceRect && op.Bounds == Bounds;
            }

            public void Dispose() { }
        }
    }
}
