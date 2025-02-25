using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using Telegram.Bot;

[ApiController]
[Route("api/users")]
public class EndpointsManager : ControllerBase
{
    private readonly string _connectionString = "Server=localhost;Database=weatherbot;User Id=root;Password=yourpassword;";
    private readonly ITelegramBotClient _botClient;

    public EndpointsManager(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(int userId)
    {
        using IDbConnection db = new MySqlConnection(_connectionString);

        string userQuery = "SELECT * FROM users WHERE TelegramId = @UserId";
        var user = await db.QueryFirstOrDefaultAsync<User>(userQuery, new { UserId = userId });

        if (user == null)
            return NotFound(new { Message = "Користувача не знайдено" });

        string historyQuery = "SELECT * FROM WeatherRequests WHERE UserId = @UserId ORDER BY RequestDate DESC";
        var history = await db.QueryAsync<WeatherRequest>(historyQuery, new { UserId = user.Id });

        return Ok(new { User = user, History = history });
    }

    [HttpPost("sendWeatherToAll")]
    public async Task<IActionResult> SendWeather([FromBody] SendWeatherRequest request)
    {
        using IDbConnection db = new MySqlConnection(_connectionString);

        IEnumerable<User> users;

        if (request.UserId != null)
        {
            users = await db.QueryAsync<User>("SELECT * FROM users WHERE TelegramId = @UserId", new { UserId = request.UserId });
        }
        else
        {
            users = await db.QueryAsync<User>("SELECT * FROM users");
        }

        foreach (var user in users)
        {
            await _botClient.SendMessage(user.TelegramId, request.Message);
        }

        return Ok(new { Message = "Повідомлення надіслано!" });
    }
}

public class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string UserName { get; set; }
}

public class WeatherRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string City { get; set; }
    public string WeatherInfo { get; set; }
    public DateTime RequestDate { get; set; }
}

public class SendWeatherRequest
{
    public long? UserId { get; set; }
    public string Message { get; set; }
}
