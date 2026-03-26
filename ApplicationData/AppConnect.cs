namespace RecipeBook.ApplicationData;

public static class AppConnect
{
    public static RecipeBookContext Model { get; } = new();
    public static Author? CurrentAuthor { get; set; }
}
