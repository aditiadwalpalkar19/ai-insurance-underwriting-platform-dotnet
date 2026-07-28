using Npgsql;

namespace Verity.Insurance.Api.Infrastructure;

public sealed class PostgresDatabase
{
    private readonly string _connectionString;

    public PostgresDatabase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Postgres is missing.");
    }

    public async Task<NpgsqlConnection> OpenAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}