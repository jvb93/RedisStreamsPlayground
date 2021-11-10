using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RedisChat
{
    public partial class Form1 : Form
    {
        private readonly ConnectionMultiplexer _redisMux;
        private IDatabase _db;
        private readonly RedisOptions _redisOptions;
        public Form1(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _redisOptions = serviceProvider.GetRequiredService<RedisOptions>();
            _redisMux = serviceProvider.GetRequiredService<ConnectionMultiplexer>();
           
        }

        private string lastRead = "0-0";

        private void Form1_Load(object sender, EventArgs e)
        {
            _db = _redisMux.GetDatabase();

            try
            {
                _db.StreamCreateConsumerGroup(_redisOptions.StreamName, "ChatGroup");

            }
            catch (RedisServerException ex)
            {

            }

            var messages = _db.StreamRead(_redisOptions.StreamName, lastRead);
            foreach (var streamEntry in messages)
            {
                chatList.AppendText($"[{streamEntry.Values.First().Name}]: {streamEntry.Values.First().Value}\r\n");
                lastRead = streamEntry.Id;
            }

            inputBox.Focus();

            var ts = new ThreadStart(Listen);
            var thread = new Thread(ts);
            thread.Start();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text))
            {
                return;
            }
            _db.StreamAddAsync(_redisOptions.StreamName, usernameBox.Text, inputBox.Text);
            inputBox.Clear();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            SendMessage(); 
        }

        private void Listen()
        {
            while (true)
            {
                var messages = _db.StreamRead(_redisOptions.StreamName, lastRead);
                foreach (var streamEntry in messages)
                {
                    chatList.Invoke((MethodInvoker)(() =>
                    {
                        chatList.AppendText($"[{streamEntry.Values.First().Name}]: {streamEntry.Values.First().Value}\r\n");
                    }));
                   
                    lastRead = streamEntry.Id;
                }
                Thread.Sleep(500);
            }
        }

        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
               SendMessage();
            }
        }
    }
}
