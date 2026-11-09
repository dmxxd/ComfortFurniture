using MySqlConnector;
using System.Data;

namespace ComfortFurniture;

internal static class Db
{
    private const string ConnectionString =
        "Server=localhost;Port=3306;Database=comfort_furniture;User ID=root;Password=Comfort2026!;CharSet=utf8mb4;";

    public static MySqlConnection Open()
    {
        var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static DataTable Query(
        string sql,
        params MySqlParameter[] parameters)
    {
        using var connection = Open();
        using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddRange(parameters);

        using var adapter = new MySqlDataAdapter(command);
        var table = new DataTable();

        adapter.Fill(table);

        return table;
    }

    public static int Execute(
        string sql,
        params MySqlParameter[] parameters)
    {
        using var connection = Open();
        using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddRange(parameters);

        return command.ExecuteNonQuery();
    }

    public static MySqlParameter P(string name, object? value)
    {
        return new MySqlParameter(name, value ?? DBNull.Value);
    }
}
