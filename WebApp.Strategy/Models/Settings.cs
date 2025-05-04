namespace WebApp.Strategy.Models
{
    public class Settings
    {
        public static string claimDataBaseType = "databasetype";

        public EDatabaseType DatabaseType;

        public EDatabaseType GetDefaultDatabaseType => EDatabaseType.SqlServer;
    }
}
