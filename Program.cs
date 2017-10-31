using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace SlackBot
{
    static class Program
    {
        static string _Download;
        static string _Upload;

        static void Main()
        {
            _Download = Environment.GetEnvironmentVariable("SlackCats_URL_Download");

            if (System.String.IsNullOrEmpty(_Download))
            {
                Console.WriteLine("Environmental Variable Not Defined: SlackCats_URL_Download");
                Console.Read();

                return;
            }

            _Upload = Environment.GetEnvironmentVariable("SlackCats_URL_Upload");

            if (System.String.IsNullOrEmpty(_Upload))
            {
                Console.WriteLine("Environmental Variable Not Defined: SlackCats_URL_Upload");
                Console.Read();

                return;
            }

            var thread = new Thread(new ThreadStart(Meow));

            thread.Start();

            Console.WriteLine("Press any key to quit.");
            Console.WriteLine();
            Console.Read();

            Environment.Exit(0);
        }

        static void Meow()
        {
            var client = new WebClient();
            var hash = new HashSet<string>();

            while (true)
            {
                var json = client.DownloadString(_Download);

                var data = JsonConvert.DeserializeObject<History>(json);

                if(data == null)
                {
                    Console.WriteLine("NO DATA!");
                    continue;
                }

                if (!data.ok)
                {
                    Console.WriteLine("BAD DATA!");
                    continue;
                }

                System.Array.Reverse(data.messages);

                for (int i = 0; i < data.messages.Length; i++)
                {
                    var ts = data.messages[i].ts;

                    if (hash.Contains(ts))
                    {
                        continue;
                    }

                    hash.Add(ts);

                    var text = data.messages[i].text;
                    var user = data.messages[i].user;

                    Console.WriteLine("{0} {1}", user, text);

                    if (!Regex.IsMatch(text, "m+e+o*w+", RegexOptions.IgnoreCase))
                    {
                        continue;
                    }

                    json = JsonConvert.SerializeObject(new
                    {
                        text = ":cat:",
                        username = "SlackMeower",
                        icon_emoji = ":cat:"
                    });

                    client.UploadString(_Upload, "POST", json);
                }

                Thread.Sleep(5000);
            }
        }

        // Chat History JSON Structure

        class History
        {
            public bool ok;
            public Message[] messages;
        }

        class Message
        {
            public string ts;
            public string text;
            public string user;
        }
    }
}