using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

class DataBase
{
    private readonly string _connectionString = "Server=localhost;Database=weatherbot;User Id=Your UserID;Password=Your Password;";

    public async Task<int> AddUserAsync(long telegramId, string userName)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            string sql = @"
                INSERT INTO Users (TelegramId, UserName) 
                SELECT @TelegramId, @UserName
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE TelegramId = @TelegramId);
                SELECT LAST_INSERT_ID();";

            var result = await db.ExecuteScalarAsync<int>(sql, new { TelegramId = telegramId, UserName = userName });

            if (result == 0)
            {
                string selectSql = "SELECT Id FROM Users WHERE TelegramId = @TelegramId";
                return await db.ExecuteScalarAsync<int>(selectSql, new { TelegramId = telegramId });
            }

            return result;
        }
    }

    public async Task SaveWeatherRequestAsync(int userId, string city, string weatherInfo)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            string sql = "INSERT INTO WeatherRequests (UserId, City, WeatherInfo) VALUES (@UserId, @City, @WeatherInfo)";
            await db.ExecuteAsync(sql, new { UserId = userId, City = city, WeatherInfo = weatherInfo });
        }
    }

    public async Task<IEnumerable<dynamic>> GetWeatherHistoryAsync(int userId)
    {
        using (IDbConnection db = new MySqlConnection(_connectionString))
        {
            string sql = "SELECT City, WeatherInfo, RequestedAt FROM WeatherRequests WHERE UserId = @UserId ORDER BY RequestedAt DESC";
            return await db.QueryAsync(sql, new { UserId = userId });
        }
    }
}
