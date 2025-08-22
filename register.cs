using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    public partial class register : Form
    {
        List<User> userList = new List<User>();
        string filePath = "users.json";

        private static readonly HttpClient client = new HttpClient();
        private static readonly string firebaseBaseUrl = "https://online-chat-usdb-default-rtdb.europe-west1.firebasedatabase.app/";

        public register()
        {
            InitializeComponent();
        }

        private void register_Load(object sender, EventArgs e)
        {
            LoadUsersFromFile();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            if (User.IsUsernameTaken(userList, username))
            {
                MessageBox.Show("Username is taken. Please pick a new username");
                return;
            }

            string hashedPassword = ComputeSha256Hash(password);
            User newUser = new User(username, hashedPassword);
            userList.Add(newUser);
            SaveUsersToFile();
            SendUserToFirebase(newUser); // 🔥 Send to Firebase

            textBox1.Clear();
            textBox2.Clear();
            textBox1.Focus();

            MessageBox.Show("Account has been created.");
        }

        private async void SendUserToFirebase(User user)
        {
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await client.PostAsync(firebaseBaseUrl + "users.json", content);
        }

        private void SaveUsersToFile()
        {
            string json = JsonSerializer.Serialize(userList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        private void LoadUsersFromFile()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                userList = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
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

        private void button2_Click(object sender, EventArgs e)
        {
            menu menuForm = new menu();
            menuForm.Show();
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
    }
}