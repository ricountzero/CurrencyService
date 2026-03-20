namespace UserService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public List<Guid> FavoriteCurrencyIds { get; private set; } = new();

    private User() { }

    public static User Create(string name, string hashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Password = hashedPassword
        };
    }

    public static User Reconstitute(Guid id, string name, string hashedPassword)
    {
        return new User
        {
            Id = id,
            Name = name,
            Password = hashedPassword
        };
    }

    public void AddFavorite(Guid currencyId)
    {
        if (!FavoriteCurrencyIds.Contains(currencyId))
            FavoriteCurrencyIds.Add(currencyId);
    }

    public void RemoveFavorite(Guid currencyId)
    {
        FavoriteCurrencyIds.Remove(currencyId);
    }
}
