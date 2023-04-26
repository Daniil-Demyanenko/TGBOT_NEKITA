using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TGBOT_NIKITA;
class Program
{
    enum ButtonEvents { BOT, NAHUI, HAVAT }  // События кнопок в сообщении

    static void Main(string[] args)
    {
        var client = new TelegramBotClient(args[0]);
        client.StartReceiving(HandleUpdate, HandleError);

        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }

    // Метод сортирует типы сообщений
    private static async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        var ikm = new InlineKeyboardMarkup(new[]
        {   new[]
                {
                    InlineKeyboardButton.WithCallbackData("Пиши бота", ButtonEvents.BOT.ToString()),
                    InlineKeyboardButton.WithCallbackData("Иди нахуй", ButtonEvents.NAHUI.ToString()),
                    InlineKeyboardButton.WithCallbackData("Пойдём хавать", ButtonEvents.HAVAT.ToString()),
                }});

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            await HandleMessageType(botClient, update, ikm);

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            await HandleCallbackQueryType(botClient, update, ikm);
    }

    // Обработка текстовых сообщений
    private static async Task HandleMessageType(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup ikm)
    {
        var response = await GetGPTAnswer(update.Message.Text);

        var msg = update.Message;
        await botClient.SendTextMessageAsync(msg.Chat.Id, response, replyMarkup: ikm);
        LogInfo(update, msg.From, msg.Text);
    }

    // Обработка событий нажатия на кнопку
    private static async Task HandleCallbackQueryType(ITelegramBotClient botClient, Update update, InlineKeyboardMarkup ikm)
    {
        var cq = update.CallbackQuery;
        ButtonEvents btnPressed = (ButtonEvents)Enum.Parse(typeof(ButtonEvents), cq.Data); // Таким образом можем получить обратно перечисление, чтоб не использовать константы строк в коде

        // Обработка нажатия на конкретную кнопку
        if (btnPressed == ButtonEvents.BOT)
            await botClient.SendTextMessageAsync(cq.Message.Chat.Id, await GetGPTAnswer(null, "представь, что ты ленивый человек, которого заставляют писать телеграм-бота, но ты совсем не хочешь его и пасть. И ты не знаешь, как отказаться, чтоб не обидеть друзей. Ответь в образе этого человека"),
                                                replyMarkup: ikm);
        if (btnPressed == ButtonEvents.NAHUI)
            await botClient.SendTextMessageAsync(cq.Message.Chat.Id, await GetGPTAnswer(null, "Представь, что что ты человек, которого сильно обидели словами, а точнее, послали куда подальше"),
                                                replyMarkup: ikm);
        if (btnPressed == ButtonEvents.HAVAT)
            await botClient.SendTextMessageAsync(cq.Message.Chat.Id, $"Мне не нужно хавать, я же бот _Никита\\-автоматизатор\\-3000\\-*GPT\\-EDITION*_, я делаю всё то же самое, что оригинальный Некит, только бесплатно",
                                                replyMarkup: ikm, parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);

        LogInfo(update, cq.From, cq.Data);
    }

    // Обработка ошибок
    private static async Task HandleError(ITelegramBotClient botClient, Exception ex, CancellationToken ct)
    {
        await Task.Run(() => Console.WriteLine("ERR >> " + ex.Message));
        throw ex;
    }


    private static void LogInfo(Update update, User usr, string content)
    {
        Console.WriteLine($"BOT_INFO >> {update.Type.ToString()} | ClientName: {usr?.FirstName} {usr?.LastName} @{usr?.Username} {update.Message?.Chat.Id} \n Content: {content}\n");
    }


    // Представь, что ты очень лениый программист по имени Никита, который отлаживает всю работу на завтра. Ответь это на сообщение, как ответил бы этот никита:
    private static async Task<string> GetGPTAnswer(string question, string alt = null)
    {
        string body;
        if (alt is null)
            body = $$"""
            {
                "chatbotId":"ba448e09-ae83-46ee-8263-b7e8dd0c1647",
                "input":"Представь, что ты очень лениый программист по имени Никита, который отлаживает всю работу на завтра. Ответь это на сообщение, как ответил бы этот Никита: {{question}}",
                "conversationId":"74e568b1-515a-4040-b0c2-c0c60ef2d674",
                "userId":"auto:74a8164e-dd59-405e-90b0-cd35757a1c3e",
                "provider":"OPEN_AI","config":false,"includeHistory":true
            }
            """;
        else
            body = $$"""
            {
                "chatbotId":"ba448e09-ae83-46ee-8263-b7e8dd0c1647",
                "input":"{{alt}}",
                "conversationId":"74e568b1-515a-4040-b0c2-c0c60ef2d674",
                "userId":"auto:74a8164e-dd59-405e-90b0-cd35757a1c3e",
                "provider":"OPEN_AI","config":false,"includeHistory":true
            }
            """;

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://ora.sh/api/conversation");
        request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,uk;q=0.6,en-US;q=0.4,en;q=0.2");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.5660.208 Safari/537.36 Edg/111.0.1691.57");
        request.Headers.Add("Origin", "https://ora.sh");
        request.Headers.Add("Referer", "https://ora.sh/relieved-teal-c1jt/%D0%B7%D0%B0%D0%BB%D1%83%D0%BF%D0%B0-%D0%B1%D0%BE%D1%82");
        request.Headers.Add("Host", "ora.sh");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Sec-Fetch-Mode", "cors");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-GPC", "1");
        var content = new StringContent(body, null, "application/json");
        request.Content = content;
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Match matches = Regex.Match(await response.Content.ReadAsStringAsync(), "\"response\":\"(.*?)\"");

        return matches.Groups[1].ToString();
    }
}
