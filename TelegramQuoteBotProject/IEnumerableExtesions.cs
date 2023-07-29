namespace TelegramQuoteBotProject;


public static class ListExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var shuffledList = new List<T>(source);
        var random = new Random();
        var n = shuffledList.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (shuffledList[k], shuffledList[n]) = (shuffledList[n], shuffledList[k]);
        }
        return shuffledList;
    }
}