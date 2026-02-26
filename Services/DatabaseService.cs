using Microsoft.Data.Sqlite;
public class DatabaseService
{
    private readonly SqliteConnection _connection;
    public DatabaseService(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        CreateTables();
    }
    private void CreateTables()
    {
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                id INTEGER PRIMARY KEY NOT NULL,
                username TEXT,
                name TEXT,
                ban BOOL NOT NULL DEFAULT FALSE
            );

            CREATE TABLE IF NOT EXISTS Admins (
                id INTEGER PRIMARY KEY NOT NULL,
                tag TEXT,
                greeting TEXT,
                farewell TEXT,
                mainAdmin BOOL DEFAULT FALSE
            );

            CREATE TABLE IF NOT EXISTS Dialogues (
                topicId INTEGER UNIQUE NOT NULL,
                userId INTEGER UNIQUE NOT NULL
            );
        ";
        cmd.ExecuteNonQuery();
    }
    
    public void InsertOrIgnoreUser(long id)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Users (id) VALUES (@id)";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void UpsertUser(long id, string? username, string? name)
    {
        BotUser? botUser = GetBotUser(id);
        if (botUser == null)
        {
            InsertOrIgnoreUser(id);
            botUser = GetBotUser(id);
        }
        if (botUser != null && (botUser.Username != username || botUser.Name != name))
        {
            botUser.Username = username;
            botUser.Name = name;
            UpdateInfoUser(botUser);
        }
    }
    public BotUser? GetBotUser(long id)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BotUser
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Username = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
                Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
                Ban = reader.GetBoolean(reader.GetOrdinal("ban")),
            };
        }
        return null;
    }
    public void UpdateInfoUser(BotUser botUser)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "UPDATE Users SET id = @id, username = @username, name = @name, ban = @ban WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", botUser.Id);
        cmd.Parameters.AddWithValue("@username", botUser.Username ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@name", botUser.Name ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ban", botUser.Ban);
        cmd.ExecuteNonQuery();
    }
    
    public void InsertOrIgnoreAdmin(long id)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Admins (id) VALUES (@id)";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void UpdateAdmin(Admin admin)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "UPDATE Admins SET tag = @tag, greeting = @greeting, farewell = @farewell, mainAdmin = @mainAdmin WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", admin.Id);
        cmd.Parameters.AddWithValue("@tag", admin.Tag ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@greeting", admin.Greeting ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@farewell", admin.Farewell ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@mainAdmin", admin.MainAdmin);
        cmd.ExecuteNonQuery();
    }
    public Admin? GetAdmin(long id)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Admins WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Admin
            {
                Id = reader.GetInt64(reader.GetOrdinal("id")),
                Tag = reader.IsDBNull(reader.GetOrdinal("tag")) ? null : reader.GetString(reader.GetOrdinal("tag")),
                Greeting = reader.IsDBNull(reader.GetOrdinal("greeting")) ? null : reader.GetString(reader.GetOrdinal("greeting")),
                Farewell = reader.IsDBNull(reader.GetOrdinal("farewell")) ? null : reader.GetString(reader.GetOrdinal("farewell")),
                MainAdmin = reader.GetBoolean(reader.GetOrdinal("mainAdmin"))
            };
        }
        return null;
    }

    
    public bool IsInDialogue(long userId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Dialogues WHERE userId = @userId";
        cmd.Parameters.AddWithValue("@userId", userId);
        long count = (long)(cmd.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    public void InsertDialogue(long topicId, long userId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT INTO Dialogues (topicId, userId) VALUES (@topicId, @userId)";
        cmd.Parameters.AddWithValue("@topicId", topicId);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteDialogueByTopicId(long topicId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Dialogues WHERE topicId = @topicId";
        cmd.Parameters.AddWithValue("@topicId", topicId);
        cmd.ExecuteNonQuery();
    }

    public int? GetTopicIdByUserId(long userId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT topicId FROM Dialogues WHERE userId = @userId";
        cmd.Parameters.AddWithValue("@userId", userId);
        object? result = cmd.ExecuteScalar();
        return result == null ? null : Convert.ToInt32(result);
    }

    public long? GetUserIdByTopicId(int? topicId)
    {
        if (topicId == null) return null;
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT userId FROM Dialogues WHERE topicId = @topicId";
        cmd.Parameters.AddWithValue("@topicId", topicId);
        object? result = cmd.ExecuteScalar();
        return result == null ? null : Convert.ToInt64(result);
    }

}