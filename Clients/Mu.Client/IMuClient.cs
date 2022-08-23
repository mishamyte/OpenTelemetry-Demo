namespace Mu.Client;

public interface IMuClient
{
    Task<Bar> GetBar();
}