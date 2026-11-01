using FCG02.CatalogAPI.Domain.Enums;

namespace FCG02.CatalogAPI.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public GameCategory Category { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // construtor sem parâmetros exigido por serializadores/mapedores BSON
    public Game() { }

    public Game(
        string title,
        string description,
        decimal price,
        GameCategory category,
        IEnumerable<String>? tags = null)
    {
        this.UpdateDetails(title, description, price, category, tags);
        Id = Guid.CreateVersion7();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public void UpdateDetails(
        string title,
        string description,
        decimal price,
        GameCategory category,
        IEnumerable<string>? tags = null)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        ValidatePrice(price);

        Title = title.Trim();
        Description = description.Trim();
        Price = decimal.Round(price, 2);
        Category = category;
        Tags = tags?.Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList() ?? [];
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    private void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
    }

    private void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));

        if (description.Trim().Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(description));
    }

    private void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        if (title.Trim().Length > 150)
            throw new ArgumentException("Title cannot exceed 150 characters.", nameof(title));
    }
}