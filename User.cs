using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace WinFormsN2
{
    public class User
    {
        // Match Firebase JSON keys exactly
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string PasswordHash { get; set; }

        // Parameterless constructor for JSON deserialization
        public User()
        {
            Username = string.Empty;
            PasswordHash = string.Empty;
        }

        public User(string username, string passwordHash)
        {
            Username = username ?? string.Empty;
            PasswordHash = passwordHash ?? string.Empty;
        }

        public string GetUsername() => Username ?? string.Empty;

        public string GetPassword() => PasswordHash ?? string.Empty; // returns hash

        public void SetUsername(string username) => Username = username ?? string.Empty;

        public void SetPassword(string passwordHash) => PasswordHash = passwordHash ?? string.Empty;

        public static bool IsUsernameTaken(List<User> users, string username) =>
            users != null && users.Any(u =>
                !string.IsNullOrEmpty(u?.Username) &&
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}