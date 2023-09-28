using System.Text.Json;

namespace Quiz;

public static class TestRepository
{
    public static List<Test> GetTests()
    {
        var stringTest = File.ReadAllText("C:\\Users\\user\\Documents\\Quiz_TelegramBot\\Quiz\\testlar.json");
        var tests = JsonSerializer.Deserialize<List<Test>>(stringTest);

        return tests;
    }
}