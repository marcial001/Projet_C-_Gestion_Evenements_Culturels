namespace GestionEvenements
{
    public static class CurrentUser
    {
        public static int Id { get; set; } = -1;
        public static string Login { get; set; } = "";
        public static string Nom { get; set; } = "";
        public static string Role { get; set; } = "";
    }
}