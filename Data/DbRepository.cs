using System;
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriangleDbRepository
{
    public class DbRepository
    {
        private readonly IDbConnection _dbConnection;

        public DbRepository(IConfiguration config)
        {
            _dbConnection = new SqliteConnection(config.GetConnectionString("DefaultConnection"));
        }

        public void OpenConnection()
        {
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
        }

        public void CloseConnection()
        {
            _dbConnection.Close();
        }

        public async Task<IEnumerable<T>> GetRecordsAsync<T>(string query, object parameters)
        {
            try
            {
                OpenConnection();
                IEnumerable<T> records = await _dbConnection.QueryAsync<T>(query, parameters, commandType: CommandType.Text);
                CloseConnection();
                return records;
            }
            catch (Exception)
            {
                CloseConnection();
                throw;
            }
        }

        public async Task<bool> SaveDataAsync(string query, object parameters)
        {
            try
            {
                OpenConnection();
                int records = await _dbConnection.ExecuteAsync(query, parameters, commandType: CommandType.Text);
                CloseConnection();
                return records > 0;
            }
            catch (Exception)
            {
                CloseConnection();
                throw;
            }
        }

        public async Task<int> InsertReturnId(string query, object parameters)
        {
            try
            {
                OpenConnection();
                int results = await _dbConnection.ExecuteAsync(query, parameters, commandType: CommandType.Text);

                if (results > 0)
                {
                    int Id = _dbConnection.Query<int>("SELECT last_insert_rowid()").FirstOrDefault();
                    CloseConnection();
                    return Id;
                }
                CloseConnection();
                return 0;
            }
            catch (Exception)
            {
                CloseConnection();
                throw;
            }
        }

        // Additional methods

        public async Task<int> AddUserAsync(string username, string passwordHash, string googleId = null)
        {
            string query = "INSERT INTO Users (Username, PasswordHash, GoogleId) VALUES (@Username, @PasswordHash, @GoogleId)";
            var parameters = new { Username = username, PasswordHash = passwordHash, GoogleId = googleId };
            return await InsertReturnId(query, parameters);
        }

        public async Task<bool> UpdateUserProgressAsync(int userId, int level, int stars)
        {
            string query = $"UPDATE Progress SET Level{level} = @Stars WHERE UserId = @UserId";
            var parameters = new { Stars = stars, UserId = userId };
            return await SaveDataAsync(query, parameters);
        }

        public async Task<Progress> GetUserProgressAsync(int userId)
        {
            string query = "SELECT * FROM Progress WHERE UserId = @UserId";
            var parameters = new { UserId = userId };
            var result = await GetRecordsAsync<Progress>(query, parameters);
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<Avatar>> GetAvatarsAsync()
        {
            string query = "SELECT * FROM Avatars";
            return await GetRecordsAsync<Avatar>(query, null);
        }
    }

    // Data models
    public class Progress
    {
        public int UserId { get; set; }
        public int Level1 { get; set; }
        public int Level2 { get; set; }
        public int Level3 { get; set; }
        public int Level4 { get; set; }
        public int Level5 { get; set; }
        public int Level6 { get; set; }
        public int Level7 { get; set; }
        public int Level8 { get; set; }
        public int Level9 { get; set; }
        public int Level10 { get; set; }
    }

    public class Avatar
    {
        public int AvatarId { get; set; }
        public string AvatarName { get; set; }
        public string CssStyle { get; set; }
        public int StarsRequired { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string GoogleId { get; set; }
        public int Stars { get; set; }
        public int CurrentAvatar { get; set; }
    }
}
