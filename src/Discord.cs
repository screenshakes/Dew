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
        Dictionary<ulong, string> userInputs;
        List<(ulong, string)> logs;
        IMessage inputMessage;
        Random random;
        ulong userId;

        public Discord(ulong user, ulong channel)
        {
            buttons = new Dictionary<string, int>();
            userInputs = new Dictionary<ulong, string>();
            logs = new List<(ulong, string)>();
            client = new DiscordSocketClient();

            random = new Random();

            client.Log += Log;
            client.Disconnected += async (e) => Environment.Exit(1);
            client.Ready += () => SetChannel(channel);
            client.Ready += () => OnReady();
            client.Ready += async () => {
                inputMessage = await this.channel.GetMessageAsync(Settings.InputMessage);
                await AddButtons(inputMessage);
            };
            client.ReactionAdded += async (m, c, r) => {
                if(r.UserId != userId && m.Id == Settings.InputMessage)
                {
                    logs.Add((r.UserId, r.Emote.Name));

                    if(userInputs.ContainsKey(r.UserId))
                    {
                        --buttons[userInputs[r.UserId]];
                        userInputs[r.UserId] = r.Emote.Name;
                    }
                    else userInputs.Add(r.UserId, r.Emote.Name);

                    if(!buttons.ContainsKey(r.Emote.Name))
                        buttons.Add(r.Emote.Name, 1);
                    else ++buttons[r.Emote.Name];

                    OnButtonReaction(buttons, (state) => {
                        userInputs.Clear();
                        buttons.Clear();
                        SaveLogs(state);
                    });
                }
            };

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
            var message = await channel.SendFileAsync(path);
            return message;
        }

        public async Task AddButtons(IMessage message)
        {
            await message.RemoveAllReactionsAsync();
            foreach(var b in Settings.Buttons)
            {
                await Task.Delay(250);
                if(b.Item2.Contains("<")) await message.AddReactionAsync(Emote.Parse(b.Item2));
                else await message.AddReactionAsync(new Emoji(b.Item2));
            }
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

        public async Task SendMessage(string message)
        {
            await channel.SendMessageAsync(message);
        }

        public Func<Dictionary<string, int>, Action<int>, Task> OnButtonReaction;
        public Func<Task> OnReady;
    }
}