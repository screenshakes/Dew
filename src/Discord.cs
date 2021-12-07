using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Dew
{
    class Discord
    {
        DiscordSocketClient client;
        IMessageChannel channel;
        Dictionary<string, int> buttons;
        Dictionary<string, string> userInputs;
        List<(ulong, string)> logs;
        IMessage inputMessage;
        Random random;
        ulong userId;

        public Discord(ulong user, ulong channel)
        {
            buttons = new Dictionary<string, int>();
            userInputs = new Dictionary<string, string>();
            logs = new List<(ulong, string)>();
            client = new DiscordSocketClient();

            random = new Random();

            client.Log += Log;
            client.Disconnected += async (e) => Environment.Exit(1);
            client.Ready += () => SetChannel(channel);
            client.Ready += () => OnReady();
            client.ButtonExecuted += ButtonHandler;

            userId = user;
        }

        void SaveLogs(int state)
        {
            if(Settings.LogReactions)
            {
                Directory.CreateDirectory(Program.AbsolutePath + "/Logs");
                string file = "";
                foreach(var log in logs)
                    file += $"{log.Item1} : {log.Item2}{System.Environment.NewLine}";
                
                File.WriteAllText($"{Program.AbsolutePath}/Logs/{state}_logs", file);
            }

            logs.Clear();
        }

        public async Task Start(string token)
        {
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        Task SetChannel(ulong channelId)
        {
            channel = client.GetChannel(channelId) as IMessageChannel;
            return Task.CompletedTask;
        }

        Task Log(LogMessage message)
        {
            Console.WriteLine($"[Discord] {message.ToString()}");
            return Task.CompletedTask;
        }

        public async Task<IMessage> SendGIF(string path)
        {
            var builder = new ComponentBuilder();
            var index = 0;
            foreach(var b in Settings.Buttons)
            {
                if(b.Item2.Contains("\\")) builder.WithButton(" ", index.ToString(), emote : new Emoji(b.Item2), style: ButtonStyle.Secondary);
                else builder.WithButton(b.Item2, index.ToString(), style: ButtonStyle.Secondary);
                ++index;
            }

            var message = await channel.SendFileAsync(path, component: builder.Build());
            return message;
        }

        public async Task ClearMessages()
        {
            var messages = channel.GetMessagesAsync().Flatten();
            await foreach(var message in messages)
            {
                if(message.Author.Id == userId)
                    await message.DeleteAsync();
            }
        }

        public async Task ButtonHandler(SocketMessageComponent component)
        {
            var user = component.User.Id;
            var button = component.Data.CustomId;
            logs.Add((user, button));

            if(userInputs.ContainsKey(button))
            {
                --buttons[userInputs[button]];
                userInputs[button] = button;
            }
            else userInputs.Add(button, button);

            if(!buttons.ContainsKey(button))
                buttons.Add(button, 1);
            else ++buttons[button];

            OnButtonReaction(buttons, (state) => {
                userInputs.Clear();
                buttons.Clear();
                SaveLogs(state);
            });
        }

        public Func<Dictionary<string, int>, Action<int>, Task> OnButtonReaction;
        public Func<Task> OnReady;
    }
}