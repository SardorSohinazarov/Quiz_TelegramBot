using Quiz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Program
{
    public static int TestNumber;
    private static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient("6666617530:AAEc5I4KUCpYe1JHw2KM4g0AD9GGSvxDxb0");

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        cts.Cancel();

        //Har qanday o'zgarish shu method 
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(botClient, update, cancellationToken),
                UpdateType.CallbackQuery => HandleCallBackQueryAsync(botClient, update, cancellationToken),
                //Yana update larni davom ettirib tutishingiz mumkin
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Xato:{ex.Message}");
            }
        }

        //Polling errorlarni shu ushledi
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }

    //CallBackQuery ni ustida amallar bajaradi
    private static async Task HandleCallBackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callBack = update.CallbackQuery;

        var tests = TestRepository.GetTests();
        Console.WriteLine(TestNumber);
        var test = tests[TestNumber - 1];
        var nextTest = tests[TestNumber];


        await CheckAnswerAsync(test,botClient, callBack, cancellationToken);
        TestNumber++;
        await SendNextQuestion(nextTest, botClient, update, cancellationToken);
    }

    //Keyingi testni botga tashlab beradi
    private static async Task SendNextQuestion(Test nextTest, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            //row
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.A}", 
                    callbackData: "A"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.B}", 
                    callbackData: "B"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.C}", 
                    callbackData: "C"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.D}", 
                    callbackData: "D"),
            }
        });

        if(update.Message is null )
        {
            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                text: $"{nextTest.Question}",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $"{nextTest.Question}",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
        }
    }

    //Callback queryda kelgan javobni to'g'ri yoki xatoligini tekshiradi
    private static async Task CheckAnswerAsync(Test test, ITelegramBotClient botClient, CallbackQuery? callBack, CancellationToken cancellationToken)
    {
        if(callBack.Data == test.CorrectAnswer)
        {
            await botClient.SendTextMessageAsync(
                chatId: callBack.From.Id,
                text: $"To'g'ri malades",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: callBack.From.Id,
                text: $"Xato javob",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }

    private static async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        //betda update.Messageni null emasligini tekshirib,
        //null bo'lmasa message degan o'zgaruvchiga qiymatni olib beryapti
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;
        
        Console.WriteLine($"Received a '{messageText}' message in chat {update.Message.Chat.Id}.");

        if(messageText == "/start")
        {
            await botClient.SendTextMessageAsync(
                chatId:update.Message.Chat.Id,
                text:$"Assalomu alaykum <b> {update.Message.From.FirstName} </b>",
                parseMode: ParseMode.Html,
                cancellationToken:cancellationToken);
        }

        TestNumber = 1;
        var tests = TestRepository.GetTests();
        await SendNextQuestion(tests[0],botClient,update,cancellationToken);
    }
}