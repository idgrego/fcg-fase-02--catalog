namespace FCG02.CatalogAPI.Domain.Entities;

public class UserLibrary
{
    private readonly List<LibraryItem> _items = [];

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<LibraryItem> Items => _items.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public UserLibrary() { }
    public UserLibrary(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        Id = Guid.CreateVersion7();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddGame(Guid gameId, string gameTitle, decimal pricePaid)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException("GameId cannot be empty", nameof(gameId));
        if (string.IsNullOrWhiteSpace(gameTitle))
            throw new ArgumentException("Game title cannot be empty", nameof(gameTitle));
        if (pricePaid < 0)
            throw new ArgumentException("Price paid cannot be negative", nameof(pricePaid));
        if (HasGame(gameId))
            throw new InvalidOperationException($"Game '{gameTitle}' is already in user library");
        var item = new LibraryItem(
            GameId: gameId,
            GameTitle: gameTitle.Trim(),
            PricePaid: decimal.Round(pricePaid, 2),
            AcquiredAt: DateTime.UtcNow
        );
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasGame(Guid gameId)
    {
        return _items.Any(i => i.GameId == gameId);
    }
}