using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AgriTrade.Backend.Services
{
    public interface ITelegramService
    {
        Task<bool> SendMessageAsync(string message);
        Task<bool> TestConnectionAsync();
    }

    public class TelegramService : ITelegramService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TelegramService> _logger;
        private TelegramBotClient? _client;

        public TelegramService(IConfiguration config, ILogger<TelegramService> logger)
        {
            _config = config;
            _logger = logger;

            var token = _config["Telegram:BotToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _client = new TelegramBotClient(token);
            }
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            var chatId = _config["Telegram:ChatId"];

            if (_client == null || string.IsNullOrEmpty(chatId))
            {
                _logger.LogWarning("Telegram not configured. Message: {Message}", message);
                return false;
            }

            try
            {
                await _client.SendMessageAsync(
                    chatId: chatId,
                    text: message,
                    parseMode: ParseMode.Html
                );
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Telegram message");
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (_client == null)
                return false;

            try
            {
                var me = await _client.GetMeAsync();
                return me != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
