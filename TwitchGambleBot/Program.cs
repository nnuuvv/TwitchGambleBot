using System;
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
            
            Bot bot = new Bot();
            DateTime lastGamble = DateTime.Now;
            
            Thread.Sleep(5000);

            double delay = double.Parse(File.ReadAllLines(Directory.GetCurrentDirectory() + @"/Data/temp_pointstorage.txt")[2]);

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(delay);
            var timer = new System.Threading.Timer((e) =>
            {
                if (lastGamble.Subtract(DateTime.Now) > TimeSpan.FromMinutes(delay*2))
                {
                    bot = new Bot();
                    Thread.Sleep(5000);
                }
                lastGamble = bot.Gamble();   
            }, null, startTimeSpan, periodTimeSpan);
            
            while (true)
            {
                string cur = Console.ReadLine();
                if (cur.StartsWith("say"))
                {
                    bot.Client_SendMessage(cur.Remove(0, 3).ToString());
                }
            }
        }
    }

    class Bot
    {
        TwitchClient client;
        
        private long points { get; set; }

        private static readonly string credPath =
            Directory.GetCurrentDirectory() + @"/Data/gamblebot.txt";
        private static readonly string storagePath =
            Directory.GetCurrentDirectory() + @"/Data/temp_pointstorage.txt";
        private static readonly string[] _creds = File.ReadAllLines(credPath)[0].Trim().ToLower().Split(",");
        private static readonly string _channelToBot = File.ReadAllLines(credPath)[1];
        
        ConnectionCredentials credentials = new ConnectionCredentials(_creds[0],_creds[1]);
        public Bot()
        {
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
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;

            Console.WriteLine("Attempting to Connect...");
            client.Connect();
        }

        private void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            Console.WriteLine("Attempting to Reconnect...");
            client.Reconnect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            GetPointsFromMessage(e.Data);
        }
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }
  
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(e.Channel, "!points");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            
        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            
        }
        
        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {

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
                Console.WriteLine("points: " + points);
            }

            if (message.Contains("Rolled") && message.Contains(_creds[0]) && message.Contains("Points"))
            {//!gamble
                Console.WriteLine("Roll msg: " + message);
                
                var items = message.Split(" ");
                points = long.Parse(items[items.Length - 2].Trim());
                Console.WriteLine(DateTime.Now.ToLocalTime() + ": points after gamble: " + String.Format("{0:n}", points));
                
                string pointHistory = File.ReadAllText(storagePath);
                File.WriteAllText(storagePath, pointHistory + ":" + points.ToString());
            }
        }

        public DateTime Gamble()
        {
            
            Client_SendMessage("!points");
            Thread.Sleep(5000);
            if (points > 1000) ;
            Client_SendMessage("!gamble " + Math.Floor(points*0.1));
            
            return DateTime.Now;
        }

    }
}