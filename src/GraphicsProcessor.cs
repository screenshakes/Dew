using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SK.Libretro;

namespace Dew
{
    sealed class GraphicsProcessor : IGraphicsProcessor
    {
        public Image<Rgba32> Output { get; private set; }
        public bool Record;
        Action<Image<Rgba32>> onRender;

        public GraphicsProcessor(int width, int height, Action<Image<Rgba32>> onRender)
        {
            Output = new Image<Rgba32>(width, height);
            this.onRender = onRender;
        }

        public void DeInit()
        {
            Output = null;
        }

        public unsafe void ProcessFrame0RGB1555(ushort* data, int width, int height, int pitch)
        {
            if(!Record) return;

            pitch /= sizeof(ushort);

            Output = new Image<Rgba32>(width, height);
            ushort* line = data;
            for(int y = 0; y < Output.Height; ++y)
            {
                Span<Rgba32> pixelRowSpan = Output.GetPixelRowSpan(y);
                for(int x = 0; x < Output.Width; ++x)
                    pixelRowSpan[x] = ARGB1555toRGBA32(line[x]);

                line += pitch;
            }

            onRender(Output);
        }

        public unsafe void ProcessFrameXRGB8888(uint* data, int width, int height, int pitch)
        {
            if(!Record) return;
            
            pitch /= sizeof(uint);

            Output = new Image<Rgba32>(width, height);
            for(int y = 0; y < Output.Height; ++y)
            {
                Span<Rgba32> pixelRowSpan = Output.GetPixelRowSpan(y);
                for(int x = 0; x < Output.Width; ++x)
                    pixelRowSpan[x] = XRGB8888toRGBA32(data[x]);

                data += pitch;
            }

            onRender(Output);
        }

        public unsafe void ProcessFrameRGB565(ushort* data, int width, int height, int pitch)
        {
             if(!Record) return;
             
            pitch /= sizeof(ushort);

            Output = new Image<Rgba32>(width, height);
            ushort* line = data;
            for(int y = 0; y < Output.Height; ++y)
            {
                Span<Rgba32> pixelRowSpan = Output.GetPixelRowSpan(y);
                for(int x = 0; x < Output.Width; ++x)
                    pixelRowSpan[x] = RGB565toRGBA32(line[x]);

                line += pitch;
            }

            onRender(Output);
        }

        Rgba32 XRGB8888toRGBA32(uint packed)
        {
            uint a = packed       >> 24;
            uint r = packed << 8  >> 24;
            uint g = packed << 16 >> 24;
            uint b = packed << 24 >> 24;
            return new Rgba32(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        Rgba32 ARGB1555toRGBA32(ushort packed)
        {
            uint a   = (uint)packed & 0x8000;
            uint r   = (uint)packed & 0x7C00;
            uint g   = (uint)packed & 0x03E0;
            uint b   = (uint)packed & 0x1F;
            uint rgb = (r << 9) | (g << 6) | (b << 3);
            return new Rgba32(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        Rgba32 RGB565toRGBA32(ushort packed)
        {
            uint r = ((uint)packed >> 11) & 0x1f;
            uint g = ((uint)packed >> 5) & 0x3f;
            uint b = ((uint)packed >> 0) & 0x1f;
            r      = (r << 3) | (r >> 2);
            g      = (g << 2) | (g >> 4);
            b      = (b << 3) | (b >> 2);

            return new Rgba32(r / 255f, g / 255f, b / 255f, 1);
        }
    }
}
