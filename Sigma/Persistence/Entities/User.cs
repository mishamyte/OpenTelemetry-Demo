namespace Sigma.Persistence.Entities;

public class User
{
    public User(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    private User() {}

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;
}