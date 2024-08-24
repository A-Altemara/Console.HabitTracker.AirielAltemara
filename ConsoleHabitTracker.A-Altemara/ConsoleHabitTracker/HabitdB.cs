using System.Data;
using System.Data.SQLite;

namespace ConsoleHabitTracker;

public class HabitdB
{
    private readonly Random Random = new();
    private readonly SQLiteConnection _connection;

    public HabitdB(string connectionString)
    {
        _connection = new SQLiteConnection(connectionString);

        _connection.Open();
        if (_connection.State != ConnectionState.Open)
        {
            Console.WriteLine("Failed to connect to the database.");
            return;
        }

        Console.WriteLine("Connected to the database.");

        string checkTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='habitsTable';";
        var tableExists = false;

        using (SQLiteCommand command = new(checkTableQuery, _connection))
        {
            using SQLiteDataReader reader = command.ExecuteReader();
            tableExists = reader.HasRows;
        }

        // if table doesn't exit Create a table

        if (!tableExists)
        {
            string createTableQuery = "CREATE TABLE habitsTable " +
                                      "(Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                      "Date TEXT NOT NULL , " +
                                      "HabitName TEXT NOT NULL, " +
                                      "Quantity INTEGER, " +
                                      "Units TEXT);";

            using (SQLiteCommand command = new(createTableQuery, _connection))
            {
                command.ExecuteNonQuery();
            }

            CreateAndPopulateData();
        }
    }

    public void CloseConnection()
    {
        _connection.Close();
    }

    public void CreateAndPopulateData()
    {
        List<Habit> prepopulatedData = new();
        int counter = 100;
        while (counter > 0)
        {
            DateOnly randomDate = GenerateRandomDate(new DateOnly(2020, 1, 1), new DateOnly(2024, 7, 31));
            string randomActivity = GenerateRandomActivity();
            int randomDuration = Random.Next(1, 60);
            string randomUnits = GenerateRandomUnits();

            // Create an Habit object and add it to the list
            var entry = new Habit
            {
                Date = randomDate,
                HabitName = randomActivity,
                Quantity = randomDuration,
                Units = randomUnits
            };
            prepopulatedData.Add(entry);
            counter--;
        }

        foreach (var entry in prepopulatedData)
        {
            string insertQuery = "INSERT INTO habitsTable (Date, HabitName, Quantity, Units) " +
                                 "VALUES (@date, @habitName, @quantity, @units);";

            using SQLiteCommand command = new(insertQuery, _connection);
            command.Parameters.AddWithValue("@date", entry.Date.ToString("MM-dd-yyyy"));
            command.Parameters.AddWithValue("@habitName", entry.HabitName);
            command.Parameters.AddWithValue("@quantity", entry.Quantity);
            command.Parameters.AddWithValue("@units", entry.Units);

            command.ExecuteNonQuery();
        }
    }

    public IEnumerable<Habit> GetAllRecords()
    {
        var habits = new List<Habit>();
        string selectDataQuery = "SELECT * FROM habitsTable;";
        using (SQLiteCommand command = new(selectDataQuery, _connection))
        {
            using SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var habit = new Habit
                {
                    Id = reader.GetInt32(0),
                    Date = DateOnly.Parse(reader.GetString(1)),
                    HabitName = reader.GetString(2),
                    Quantity = reader.GetInt32(3),
                    Units = reader.GetString(4)
                };
                habits.Add(habit);
            }
        }

        return habits;
    }

    public void AddHabit(Habit habit)
    {
        string insertDataQuery = "INSERT INTO habitsTable (Date, HabitName, Quantity, Units) VALUES " +
                                 "(@date, @habitName, @quantity, @units);";

        using SQLiteCommand command = new(insertDataQuery, _connection);
        command.Parameters.AddWithValue("@date", habit.Date.ToString("MM-dd-yyyy"));
        command.Parameters.AddWithValue("@habitName", habit.HabitName);
        command.Parameters.AddWithValue("@quantity", habit.Quantity);
        command.Parameters.AddWithValue("@units", habit.Units);

        command.ExecuteNonQuery();
    }

    public bool DeleteHabit(string id)
    {
        string deleteQuery = "DELETE FROM habitsTable WHERE Id = @id;";

        using SQLiteCommand command = new(deleteQuery, _connection);
        command.Parameters.AddWithValue("@id", id);

        try
        {
            command.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool CheckEntryExists(string id)
    {
        if (!int.TryParse(id, out _))
        {
            return false;
        }

        string query = "SELECT COUNT(*) FROM habitsTable WHERE Id = @id;";

        using SQLiteCommand command = new(query, _connection);
        command.Parameters.AddWithValue("@id", id);

        // ExecuteScalar returns the first column of the first row in the result set
        int count = Convert.ToInt32(command.ExecuteScalar());

        return count > 0;
    }

    public Habit GetHabit(string id)
    {
        string query = "SELECT * FROM habitsTable WHERE Id = @id;";

        using SQLiteCommand command = new(query, _connection);
        command.Parameters.AddWithValue("@id", id);

        using SQLiteDataReader reader = command.ExecuteReader();
        reader.Read();

        return new Habit
        {
            Id = reader.GetInt32(0),
            Date = DateOnly.Parse(reader.GetString(1)),
            HabitName = reader.GetString(2),
            Quantity = reader.GetInt32(3),
            Units = reader.GetString(4)
        };
    }

    public bool UpdateHabit(Habit habit)
    {
        string updateQuery = "UPDATE habitsTable SET Date = @date, HabitName = @habitName, Quantity = @quantity, " +
                             "Units = @units WHERE Id = @id;";

        using SQLiteCommand command = new(updateQuery, _connection);
        command.Parameters.AddWithValue("@date", habit.Date.ToString("MM-dd-yyyy"));
        command.Parameters.AddWithValue("@habitName", habit.HabitName);
        command.Parameters.AddWithValue("@quantity", habit.Quantity);
        command.Parameters.AddWithValue("@units", habit.Units);
        command.Parameters.AddWithValue("@id", habit.Id);

        try
        {
            command.ExecuteNonQuery();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateRandomActivity()
    {
        string[] possibleActivities = ["swimming", "running", "walking", "cycling", "working", "cooking", "coding", "reading"];
        int chosenEntry = Random.Next(0, 8);
        return possibleActivities[chosenEntry];
    }

    private string GenerateRandomUnits()
    {
        string[] possibleUnits = ["minutes", "hours", "miles", "kilometers"];
        int chosenEntry = Random.Next(0, 4);
        return possibleUnits[chosenEntry];
    }

    private DateOnly GenerateRandomDate(DateOnly startDate, DateOnly endDate)
    {
        // Calculate the total number of days between the start and end dates
        int totalDays = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;

        // Generate a random number of days to add to the start date
        int randomDays = Random.Next(0, totalDays + 1);

        // Return the new random date
        return startDate.AddDays(randomDays);
    }
}