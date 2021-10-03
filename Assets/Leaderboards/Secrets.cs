public class Secrets
{
    public static string redditApiKey = "-rHEdO1MXyDpyA";

    public static long GetVerificationNumber(string name, long score, long secondary)
    {
        return (score % 456 + (long)name.Length * secondary * 123) % 79971;
    }
}