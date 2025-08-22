using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsN2
{
    public partial class login : Form
    {
        private Form _previousForm;
        private List<User> userList = new List<User>();
        private string filePath = "users.json"; // local backup

        private static readonly HttpClient client = new HttpClient();
        private static readonly string firebaseBaseUrl = "https://online-chat-usdb-default-rtdb.europe-west1.firebasedatabase.app/";

        public login(Form previousForm)
        {
            InitializeComponent();
            _previousForm = previousForm;
            LoadUsersFromFile();
        }

        // Load users from local file, supporting both new and old formats
        private void LoadUsersFromFile()
        {
            userList = new List<User>();

            if (!File.Exists(filePath)) return;

            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                // Try new format
                var current = JsonSerializer.Deserialize<List<User>>(json);
                if (current != null && current.Count > 0)
                {
                    userList = current;
                    return;
                }
            }
            catch { }

            try
            {
                // Try old format
                var legacy = JsonSerializer.Deserialize<List<LegacyUser>>(json);
                if (legacy != null)
                {
                    foreach (var lu in legacy)
                    {
                        if (!string.IsNullOrWhiteSpace(lu?.Username) && !string.IsNullOrWhiteSpace(lu?.PasswordHash))
                            userList.Add(new User(lu.Username, lu.PasswordHash));
                    }
                }
            }
            catch { }
        }

        // Legacy user class for backward compatibility
        private class LegacyUser
        {
            public string Username { get; set; }
            public string PasswordHash { get; set; }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            string hashedInput = ComputeSha256Hash(password);

            // Try Firebase first
            bool firebaseSuccess = await CheckFirebaseCredentials(username, hashedInput);

            if (firebaseSuccess)
            {
                MessageBox.Show("Login successful via Firebase!");
                OpenChat(username);
                return;
            }

            // Fallback to local file
            User foundUser = userList.FirstOrDefault(u => u.GetUsername() == username);

            if (foundUser == null || foundUser.GetPassword() != hashedInput)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            MessageBox.Show("Login successful (local file).");
            OpenChat(username);
        }

        // Check Firebase for both new and old formats
        private async Task<bool> CheckFirebaseCredentials(string username, string hashedPassword)
        {
            try
            {
                var response = await client.GetAsync(firebaseBaseUrl + "users.json");
                if (!response.IsSuccessStatusCode) return false;

                string json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json) || json == "null") return false;

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;

                foreach (var kv in doc.RootElement.EnumerateObject())
                {
                    var obj = kv.Value;
                    if (obj.ValueKind != JsonValueKind.Object) continue;

                    // Try both naming styles
                    string u =
                        obj.TryGetProperty("username", out var u1) ? u1.GetString() :
                        obj.TryGetProperty("Username", out var u2) ? u2.GetString() : null;

                    string p =
                        obj.TryGetProperty("password", out var p1) ? p1.GetString() :
                        obj.TryGetProperty("PasswordHash", out var p2) ? p2.GetString() : null;

                    if (u != null && p != null &&
                        string.Equals(u, username, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p, hashedPassword, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to Firebase: " + ex.Message);
            }

            return false;
        }

        private void OpenChat(string username)
        {
            chat chatForm = new chat(username);
            chatForm.FormClosed += (s, args) => this.Show();
            chatForm.Show();
            this.Hide();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (_previousForm != null)
            {
                _previousForm.Show();
            }
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
    }
}