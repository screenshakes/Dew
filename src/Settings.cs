using System;
using SK.Libretro;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Dew
{
    static class Settings
    {
        public const int GIFDuration = 120; // In frames
        public const int GIFFrameSkip = 0; // How many frames should be skipped at the beginning of the gif
        public const int GIFBlankSkip = 4; // How often should frames be added to the gif (once every 2, 3, 4, ...)
        public const int VoteDuration = 2500; // In milliseconds
        public const int AutoPlayDuration = 0; // Number of simulations that should be done after an input even if no new input is sent

        public const ulong BotId = 0;
        public const ulong ChannelId = 0;

        public const string Core = "";
        public const string RomName = "";

        public const int Upscale = 2;
        public static readonly IResampler UpscaleAlgorithm = KnownResamplers.NearestNeighbor;
        public static int KeepEveryNthState = 1; // Set to 1 to keep every state
        public static int KeepEveryNthGIF = 1; // Set to 1 to keep every GIF
        public static bool LogReactions = false;
        
        public static readonly (uint, string, Func<int, bool>)[] Buttons = new (uint, string, Func<int, bool>)[]
        {
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_A,      "\U0001F170", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_B,      "\U0001F171", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_UP,     "\U0001F53C", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_DOWN,   "\U0001F53D", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_LEFT,   "\U000025C0", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_RIGHT,  "\U000025B6", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_START,  "\U00002795", (frame) => frame <= 16),
            (LibretroHeader.RETRO_DEVICE_ID_JOYPAD_SELECT, "\U00002796", (frame) => frame <= 16),
            (256, "Skip", (frame) => false) // Skip
        };
    }
}