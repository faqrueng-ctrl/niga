using Microsoft.EntityFrameworkCore;

namespace RecipeBook.ApplicationData;

public class RecipeBookContext : DbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeImage> RecipeImages => Set<RecipeImage>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<LikeRecipe> LikeRecipes => Set<LikeRecipe>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=RecipeBookDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeIngredient>().HasKey(x => new { x.RecipeId, x.IngredientId });
        modelBuilder.Entity<LikeRecipe>().HasKey(x => new { x.AuthorId, x.RecipeId });

        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.Category)
            .WithMany(c => c.Recipes)
            .HasForeignKey(r => r.CategoryId);

        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.Author)
            .WithMany(a => a.Recipes)
            .HasForeignKey(r => r.AuthorId);

        modelBuilder.Entity<RecipeImage>()
            .HasOne(i => i.Recipe)
            .WithMany(r => r.Images)
            .HasForeignKey(i => i.RecipeId);

        modelBuilder.Entity<RecipeStep>()
            .HasOne(s => s.Recipe)
            .WithMany(r => r.Steps)
            .HasForeignKey(s => s.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.RecipeIngredients)
            .HasForeignKey(ri => ri.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany(i => i.RecipeIngredients)
            .HasForeignKey(ri => ri.IngredientId);

        modelBuilder.Entity<LikeRecipe>()
            .HasOne(lr => lr.Author)
            .WithMany(a => a.LikeRecipes)
            .HasForeignKey(lr => lr.AuthorId);

        modelBuilder.Entity<LikeRecipe>()
            .HasOne(lr => lr.Recipe)
            .WithMany(r => r.LikeRecipes)
            .HasForeignKey(lr => lr.RecipeId);
    }
}

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}

public class Author
{
    public int AuthorId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int ExperienceYears { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<LikeRecipe> LikeRecipes { get; set; } = new List<LikeRecipe>();
}

public class Recipe
{
    public int RecipeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CookingTimeMinutes { get; set; }
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public Category? Category { get; set; }
    public Author? Author { get; set; }
    public ICollection<RecipeImage> Images { get; set; } = new List<RecipeImage>();
    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<LikeRecipe> LikeRecipes { get; set; } = new List<LikeRecipe>();
}

public class RecipeImage
{
    public int RecipeImageId { get; set; }
    public int RecipeId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public Recipe? Recipe { get; set; }
}

public class RecipeStep
{
    public int RecipeStepId { get; set; }
    public int RecipeId { get; set; }
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public Recipe? Recipe { get; set; }
}

public class Ingredient
{
    public int IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}

public class RecipeIngredient
{
    public int RecipeId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Recipe? Recipe { get; set; }
    public Ingredient? Ingredient { get; set; }
}

public class LikeRecipe
{
    public int AuthorId { get; set; }
    public int RecipeId { get; set; }
    public Author? Author { get; set; }
    public Recipe? Recipe { get; set; }
}
