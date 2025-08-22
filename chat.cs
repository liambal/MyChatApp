using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsN2
{
    public partial class chat : Form
    {
        private List<message> messages = new List<message>();
        private string currentUser;

        private static readonly HttpClient client = new HttpClient();
        private static readonly string firebaseBaseUrl = "https://online-chat-usdb-default-rtdb.europe-west1.firebasedatabase.app/";

        private readonly string onlineFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "online.txt");
        private System.Windows.Forms.Timer onlineTimer;

        public chat(string username)
        {
            InitializeComponent();
            currentUser = username;
        }

        private void chat_Load(object sender, EventArgs e)
        {
            label1.Text = $"Logged in as: {currentUser}";
            LoadMessagesFromFirebase(); // 🔥 Load messages from Firebase

            UpdatePresence();
            RefreshOnlineUsersUI();

            onlineTimer = new System.Windows.Forms.Timer();
            onlineTimer.Interval = 5000;
            onlineTimer.Tick += (s, args) =>
            {
                UpdatePresence();
                RefreshOnlineUsersUI();
                LoadMessagesFromFirebase(); // 🔄 Refresh messages every 5 seconds
            };
            onlineTimer.Start();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            message msg = new message
            {
                Sender = currentUser,
                Text = text,
                Timestamp = DateTime.Now
            };

            messages.Add(msg);
            DisplayMessage(msg);
            SaveMessageToFile(msg);
            await SendMessageToFirebase(msg); // 🔥 Send to Firebase

            textBox1.Clear();
        }

        private async Task SendMessageToFirebase(message msg)
        {
            var json = JsonSerializer.Serialize(msg);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync(firebaseBaseUrl + "messages.json", content);
        }

        private async void LoadMessagesFromFirebase()
        {
            try
            {
                var response = await client.GetAsync(firebaseBaseUrl + "messages.json");
                if (!response.IsSuccessStatusCode) return;

                string json = await response.Content.ReadAsStringAsync();
                var firebaseMessages = JsonSerializer.Deserialize<Dictionary<string, message>>(json);

                richTextBox1.Clear();
                if (firebaseMessages != null)
                {
                    var sorted = firebaseMessages.Values.OrderBy(m => m.Timestamp);
                    foreach (var msg in sorted)
                    {
                        DisplayMessage(msg);
                    }
                }
            }
            catch { /* ignore errors for now */ }
        }

        private void DisplayMessage(message msg)
        {
            richTextBox1.SelectionColor = Color.Blue;
            richTextBox1.AppendText($"{msg.Sender} [{msg.Timestamp:HH:mm}]: ");
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.AppendText($"{msg.Text}\n");

            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void SaveMessageToFile(message msg)
        {
            string filePath = "history.txt";
            string line = $"{msg.Sender} [{msg.Timestamp:HH:mm}]: {msg.Text}";
            File.AppendAllText(filePath, line + Environment.NewLine);
        }

        // Online presence methods (unchanged)
        private void UpdatePresence() { /* ... */ }
        private void RemovePresence() { /* ... */ }
        private void RefreshOnlineUsersUI() { /* ... */ }
        private Dictionary<string, long> ReadPresence() { /* ... */ return new Dictionary<string, long>(); }
        private void WritePresence(Dictionary<string, long> dict) { /* ... */ }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (onlineTimer != null)
                {
                    onlineTimer.Stop();
                    onlineTimer.Dispose();
                    onlineTimer = null;
                }
                RemovePresence();
            }
            catch { }
            base.OnFormClosed(e);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { this.Close(); }
    }
}