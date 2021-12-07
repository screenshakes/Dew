using System;
using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using SK.Libretro;
using SK.Libretro.Utilities;

namespace Dew
{
    sealed class Libretro
    {
        LibretroWrapper wrapper;
        GraphicsProcessor renderer;
        InputProcessor inputs;
        Image<Rgba32> output;
        public int state;

        public Libretro(string core, string gameDirectory, string game)
        {
            InitializeLogs();

            var mainDirectory = Path.Combine(Program.AbsolutePath, "libretro");
            wrapper = new LibretroWrapper(Program.Platform, mainDirectory);

            if(!wrapper.StartGame(core, Path.Combine(Program.AbsolutePath, gameDirectory), game))
            {
            
            }

            InitializeGraphics();
            InitializeInputs();

            if(File.Exists(Program.AbsolutePath + "/state"))
                state = int.Parse(File.ReadAllText(Program.AbsolutePath + "/state"));
            else state = -1;
        }

        public void Update()
        {
            wrapper.Update();
        }

        public string Simulate(int duration, uint input, Func<int, bool> pressFunction)
        {
            while(state > -1 && !LoadState(state))
            {
                Console.WriteLine("Could not load save :" + state);
                --state;
            }

            inputs.Clear();

            for(int i = 0; i < duration + Settings.GIFFrameSkip; ++i)
            {
                renderer.Record = i >= Settings.GIFFrameSkip && i % Settings.GIFBlankSkip == 0;
                SetInput(input, pressFunction.Invoke(i));
                Update();
            }

            if(Settings.DestroyGIFs)
            {
                var gif = Program.AbsolutePath + $"/GIFs/{state}.gif";
                if(File.Exists(gif)) File.Delete(gif);
            }

            if(Settings.DestroyStates) 
            {
                var statePath = Program.AbsolutePath + $"/libretro/saves/{Settings.Core}/{Settings.RomName}/save_{state}.state";
                if(File.Exists(statePath)) File.Delete(statePath);
            }

            ++state;
            SaveState(state);

            if(state.ToString()?.Length > 0) File.WriteAllText(Program.AbsolutePath + "/state", state.ToString());
            
            Directory.CreateDirectory(Program.AbsolutePath + "/GIFs");
            var path = Program.AbsolutePath + $"/GIFs/{state}.gif";

            if(Settings.Upscale > 1) output.Mutate(x => x.Resize(output.Width * Settings.Upscale, output.Height * Settings.Upscale, Settings.UpscaleAlgorithm));
            output.SaveAsGif(path);

            output = null;

            return path;
        }

        void InitializeLogs()
        {
            Logger.Instance.AddDebughandler(Log);
            Logger.Instance.AddInfoHandler(Log);
            Logger.Instance.AddWarningHandler(Log);
            Logger.Instance.AddErrorhandler(Log);
            Logger.Instance.AddExceptionHandler(Console.Error.WriteLine);
        }

        void Log(string message)
        {
            Console.WriteLine($"[Libretro] {message}");
        }

        void InitializeGraphics()
        {
            renderer = new GraphicsProcessor(wrapper.Game.VideoWidth, wrapper.Game.VideoHeight, Callback);
            wrapper.ActivateGraphics(renderer);
        }

        void InitializeInputs()
        {
            inputs = new InputProcessor();
            wrapper.ActivateInput(inputs);
        }

        void Callback(Image<Rgba32> output)
        {
            output.Frames[0].Metadata.GetGifMetadata().FrameDelay = 4;
            if(this.output == null) this.output = output;
            else this.output.Frames.AddFrame(output.Frames[0]);
        }

        void SetInput(uint input, bool down)
        {
            if(input < 16) inputs.Buttons[input] = down;
        }

        string SaveState(int index)
        {
            if (wrapper.SaveState(index, out string savePath))
                return savePath;

            return null;
        }

        bool LoadState(int index)
        {
            return wrapper.LoadState(index);
        }
    }
}
