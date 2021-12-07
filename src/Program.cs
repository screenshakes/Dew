using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using SK.Libretro;

namespace Dew
{
    static class Program
    {
        static Discord discord;
        static Libretro libretro;
        static bool waitingForVote;
        static int autoPlay;

        static void Main(string[] args) => Program.MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            Platform = GetPlatform();
            AbsolutePath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
            
            discord = new Discord(Settings.BotId, Settings.ChannelId);
            discord.OnButtonReaction += OnButtonReaction;
            discord.OnReady += () => Simulate(0, (frame) => false);
            
            libretro = new Libretro(Settings.Core, "",  Settings.RomName);

            Bans = new HashSet<ulong>();
            if(File.Exists(Path.Combine(AbsolutePath, "bans")))
            foreach(var l in File.ReadAllLines(Path.Combine(AbsolutePath, "bans")))
                Bans.Add(ulong.Parse(l));

            await discord.Start(File.ReadAllText(Path.Combine(AbsolutePath, "token")));
            
            while(true)
            {
                await Task.Delay(500);
                GC.Collect();
            }
        }

        async static Task Simulate(uint input, Func<int, bool> pressFunction)
        {
            await discord.ClearMessages();
            var gif = libretro.Simulate(Settings.GIFDuration, input, pressFunction);
            var task = discord.SendGIF(gif);
            while(await Task.WhenAny(task, Task.Delay(5000)) != task);
        }

        async static Task OnButtonReaction(Dictionary<string, int> buttons, Action<int> callback)
        {
            if(!waitingForVote)
            {
                waitingForVote = true;
                if(buttons.Count > 0) autoPlay = Settings.AutoPlayDuration;

                await Task.Delay(Settings.VoteDuration);

                var maxCount = 0;
                var button = "";

                foreach(var b in buttons)
                    if(b.Value > maxCount)
                    {
                        maxCount = b.Value;
                        button = b.Key;
                    }

                if(button != "")
                {
                    var input = Settings.Buttons[int.Parse(button)];
                    await Simulate(input.Item1, input.Item3);
                        waitingForVote = false;
                }

                if(waitingForVote)
                {
                    await Simulate(0, (frame) => false);
                    waitingForVote = false;
                }

                callback.Invoke(libretro.state);

                if(autoPlay > 0)
                {
                    --autoPlay;
                    await OnButtonReaction(buttons, callback);
                }
            }
        }

        static LibretroTargetPlatform GetPlatform()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return LibretroTargetPlatform.WindowsPlayer;
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return LibretroTargetPlatform.LinuxPlayer;
            else return LibretroTargetPlatform.OSXPlayer;
        }

        public static LibretroTargetPlatform Platform { get; private set; }
        public static string AbsolutePath { get; private set; }
        public static HashSet<ulong> Bans { get; private set; }
    }
}
