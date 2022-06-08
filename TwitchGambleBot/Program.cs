using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Client.Models.Internal;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;




namespace TwitchBot
{
    
    class Program
    {
        static void Main(string[] args)
        {
            bool endOnJp = false;
            AutoGamble(endOnJp);
            
            /*var bot = new Bot(endOnJp);
            while (true)
            {
                var cur = Console.ReadLine();
                if (cur != null && cur.StartsWith("say"))
                {
                    bot.Client_SendMessage(cur.Remove(0, 3).ToString());
                }
            }*/
        }
        
        static void AutoGamble(bool endOnJp)
        {
            var bot = new Bot(endOnJp);

            Thread.Sleep(5000);

            double delay = 5.1;

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(delay);
            var timer = new System.Threading.Timer((e) =>
            {
                if (bot == null)
                {
                    NewBot();
                    Console.WriteLine("--starting bot");
                }
                bot.Gamble();
                Console.WriteLine("--gambling");
                DisposeBot();
                Console.WriteLine("--disposing bot");
            }, null, startTimeSpan, periodTimeSpan);
            
            while (true)
            {
                var cur = Console.ReadLine();
                if (cur != null && cur.StartsWith("say"))
                {
                    NewBot();
                    bot.Client_SendMessage(cur.Remove(0, 3).ToString());
                    DisposeBot();
                }
            }

            void NewBot()
            {
                bot = new Bot(endOnJp);
                Thread.Sleep(5000);
            }

            void DisposeBot()
            {
                Thread.Sleep(5000);
                bot = null;
            }
        }
    }


    class Bot
    {
        TwitchClient client;
        private bool _endOnJp;
        private long points { get; set; }

        private static readonly string credPath =
            Directory.GetCurrentDirectory() + @"/Data/gamblebot.txt";
        private static readonly string storagePath =
            Directory.GetCurrentDirectory() + @"/Data/temp_pointstorage.txt";
        private static readonly string[] _creds = File.ReadAllLines(credPath)[0].Trim().ToLower().Split(",");
        private static readonly string _channelToBot = File.ReadAllLines(credPath)[1];
        
        ConnectionCredentials credentials = new ConnectionCredentials(_creds[0],_creds[1]);
        public Bot(bool endOnJp)
        {
            _endOnJp = endOnJp;
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 100000000,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, _channelToBot);
            
            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnDisconnected += Client_OnDisconnected;

            Console.WriteLine("Attempting to Connect...");
            client.Connect();
        }


        private void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected: " + e.ToString());
            Console.WriteLine("Attempting to Reconnect...");
            client.Reconnect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            GetPointsFromMessage(e.Data);
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine(e.BotUsername + " joined channel: " + e.Channel);
        }
        

        public void Client_SendMessage(string message)
        {
            try
            {
                if (client.IsInitialized)
                {
                    if (client.IsConnected)
                    {
                        client.SendMessage(_channelToBot, message);
                    }
                    else
                    {
                        Console.WriteLine("Attempting to Reconnect...");
                        client.Reconnect();
                    }
                }
                else
                {
                    Console.WriteLine("Reinitializing");
                    client.Initialize(credentials, _channelToBot);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        

        private void GetPointsFromMessage(string message)
        {
            if (message.Contains(_creds[0]) && message.Contains("Points:"))
            {//!points
                var items = message.Split(":");
                points = long.Parse(items.Last().Trim());
                Console.WriteLine("points: " + String.Format("{0:n}", points));
            }

            if (message.Contains("Rolled") && message.Contains(_creds[0]) && message.Contains("Points"))
            {//!gamble
                //Console.WriteLine("Roll msg: " + message);
                
                var items = message.Split(" ");
                points = long.Parse(items[items.Length - 2].Trim());

                if(int.Parse(items[6].Replace(".", "")) == 100 && _endOnJp)
                    Environment.Exit(0);
                
                
                Console.WriteLine(DateTime.Now.ToLocalTime() + ": points after gamble: " + String.Format("{0:n}", points));
                
                string pointHistory = File.ReadAllText(storagePath);
                File.WriteAllText(storagePath, pointHistory + ":" + points.ToString());
            }
        }

        public void Gamble()
        {
            Client_SendMessage("!points");
            Thread.Sleep(5000);
            if (points > 1000)
            {
                Client_SendMessage("!gamble " + Math.Floor(points*0.1).ToString("F0", CultureInfo.InvariantCulture));
            }
        }
    }
}