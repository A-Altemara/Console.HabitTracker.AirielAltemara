using System.Data;
using System.Data.SQLite;

namespace ConsoleHabitTracker;

/// <summary>
/// Represents a database handler for habit tracking, using SQLite as the database engine.
/// </summary>
public class HabitDb
{
    private readonly Random _random = new();
    private readonly SQLiteConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="HabitDb"/> class and connects to the specified SQLite database.
    /// If the "habitsTable" table does not exist, it creates the table and pre-populates it with random data.
    /// </summary>
    /// <param name="connectionString">The connection string for the SQLite database.</param>
    public HabitDb(string connectionString)
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
        bool tableExists;

        using (SQLiteCommand command = new(checkTableQuery, _connection))
        {
            using SQLiteDataReader reader = command.ExecuteReader();
            tableExists = reader.HasRows;
        }

        // If the table doesn't exist, create it
        if (!tableExists)
        {
            string createTableQuery = "CREATE TABLE habitsTable " +
                                      "(Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                      "Date TEXT NOT NULL, " +
                                      "HabitName TEXT NOT NULL, " +
                                      "Quantity INTEGER, " +
                                      "Units TEXT);";

            using (SQLiteCommand command = new(createTableQuery, _connection))
            {
                command.ExecuteNonQuery();
            }

            GenerateAndPopulateData();
        }
    }

    /// <summary>
    /// Closes the connection to the database.
    /// </summary>
    public void CloseConnection()
    {
        _connection.Close();
    }

    /// <summary>
    /// Generates random data and populates the "habitsTable" table with pre-populated data.
    /// </summary>
    private void GenerateAndPopulateData()
    {
        List<Habit> prepopulatedData = new();
        int counter = 100;
        while (counter > 0)
        {
            DateOnly randomDate = GenerateRandomDate(new DateOnly(2020, 1, 1), new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
            string randomActivity = GenerateRandomActivity();
            int randomDuration = _random.Next(1, 60);
            string randomUnits = GenerateRandomUnits();

            // Create a Habit object and add it to the list
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

    /// <summary>
    /// Retrieves all records from the "habitsTable" table.
    /// </summary>
    /// <returns>A list of all <see cref="Habit"/> records from the database.</returns>
    public List<Habit> GetAllRecords()
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
                    Date = DateOnly.ParseExact(reader.GetString(1), "MM-dd-yyyy"),
                    HabitName = reader.GetString(2),
                    Quantity = reader.GetInt32(3),
                    Units = reader.GetString(4)
                };
                habits.Add(habit);
            }
        }

        return habits;
    }

    /// <summary>
    /// Adds a new habit record to the "habitsTable" table.
    /// </summary>
    /// <param name="habit">The habit record to add.</param>
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

    /// <summary>
    /// Deletes a habit record from the "habitsTable" table by its ID.
    /// </summary>
    /// <param name="id">The ID of the habit record to delete.</param>
    /// <returns><c>true</c> if the record was deleted successfully; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Updates an existing habit record in the "habitsTable" table.
    /// </summary>
    /// <param name="habit">The habit record to update.</param>
    /// <returns><c>true</c> if the record was updated successfully; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Generates a random activity name from a predefined list.
    /// </summary>
    /// <returns>A randomly chosen activity name.</returns>
    private string GenerateRandomActivity()
    {
        string[] possibleActivities =
            { "swimming", "running", "walking", "cycling", "working", "cooking", "coding", "reading" };
        int chosenEntry = _random.Next(0, 8);
        return possibleActivities[chosenEntry];
    }

    /// <summary>
    /// Generates a random unit of measurement from a predefined list.
    /// </summary>
    /// <returns>A randomly chosen unit of measurement.</returns>
    private string GenerateRandomUnits()
    {
        string[] possibleUnits = { "minutes", "hours", "miles", "kilometers" };
        int chosenEntry = _random.Next(0, 4);
        return possibleUnits[chosenEntry];
    }

    /// <summary>
    /// Generates a random date within a specified range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A randomly chosen date within the specified range.</returns>
    private DateOnly GenerateRandomDate(DateOnly startDate, DateOnly endDate)
    {
        // Calculate the total number of days between the start and end dates
        int totalDays = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;

        // Generate a random number of days to add to the start date
        int randomDays = _random.Next(0, totalDays + 1);

        // Return the new random date
        return startDate.AddDays(randomDays);
    }
}