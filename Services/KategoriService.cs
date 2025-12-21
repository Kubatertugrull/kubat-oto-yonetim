using MySqlConnector;
using Services;

namespace Services;

public class KategoriService
{
    private readonly DatabaseService _dbService;

    public KategoriService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<object>> GetKategorilerAsync()
    {
        await using var connection = await _dbService.GetConnectionAsync();
        var cmd = new MySqlCommand("SELECT * FROM kategoriler ORDER BY KategoriAdi", connection);

        var result = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                kategoriID = reader.GetInt32("KategoriID"),
                kategoriAdi = reader.GetString("KategoriAdi")
            });
        }
        return result;
    }
}

