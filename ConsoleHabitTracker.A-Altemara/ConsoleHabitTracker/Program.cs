using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace ConsoleHabitTracker;

public static class Program
{
    private static readonly string[] DateFormats = { "MM-dd-yyyy", "dd-MM-yyyy" };

    static void Main()
    {
        var continueProgram = true;

        string connectionString = "Data Source = habits.db; Version=3;";
        var habitDb = new HabitdB(connectionString);

        while (continueProgram)
        {
            DisplayMenu();
            var selection = Console.ReadKey().KeyChar;

            switch (selection)
            {
                case '0':
                    Console.WriteLine("\n\nClosing application");
                    continueProgram = false;
                    break;
                case '1':
                    Console.WriteLine("\n\nView All Records");
                    ViewRecords(habitDb);
                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                    break;
                case '2':
                    Console.WriteLine("\n\nAdd a Record");
                    AddNewHabit(habitDb);
                    break;
                case '3':
                    Console.WriteLine("\n\nDelete a Record");
                    ViewRecords(habitDb);
                    DeleteEntry(habitDb);
                    break;
                case '4':
                    Console.WriteLine("\n\nEdit a Record");
                    ViewRecords(habitDb);
                    UpdateEntry(habitDb);
                    break;
                default:
                    Console.WriteLine("\n\nInvalid Selection press enter to try again");
                    Console.ReadLine();
                    break;
            }
        }

        habitDb.CloseConnection();
    }

    private static void UpdateEntry(HabitdB habitDb)
    {
        Console.WriteLine("Enter the record ID would you like to edit, or E to exit");
        var idToEdit = Console.ReadLine()?.ToLower();
        var sanitizedIdToEdit = SanitizeNullOrWhiteSpace(idToEdit);
        if (IsExit(sanitizedIdToEdit)) return;

        while (!habitDb.CheckEntryExists(sanitizedIdToEdit))
        {
            Console.WriteLine("invalid entry please try again or press E to exit");
            idToEdit = Console.ReadLine()?.ToLower();
            sanitizedIdToEdit = SanitizeNullOrWhiteSpace(idToEdit);
            if (IsExit(sanitizedIdToEdit)) return;
        }

        Console.WriteLine("What part of the entry would you like to edit:");
        Console.WriteLine("\tEdit the Habit Date select 0 and press enter");
        Console.WriteLine("\tEdit the Habit Name select 1 and press enter");
        Console.WriteLine("\tEdit the Habit Quantity select 2 and press enter");
        Console.WriteLine("\tEdit the Habit Units select 3 and press enter");
        var selectColumnToEdit = Console.ReadLine()?.ToLower();

        while (string.IsNullOrWhiteSpace(selectColumnToEdit) ||
               !Regex.IsMatch(selectColumnToEdit, "^[eE0123]$"))
        {
            if (selectColumnToEdit?.ToLower() == "e")
            {
                Console.WriteLine("Exiting Update Option, press enter to continue");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("invalid entry please try again or press E to exit");
            selectColumnToEdit = Console.ReadLine()?.ToLower();
        }

        var habit = habitDb.GetHabit(sanitizedIdToEdit);
        switch (selectColumnToEdit)
        {
            case "0":
                Console.WriteLine("\nEditing the Habit Date");
                Console.WriteLine("Enter the new Date, MM-DD-YYYY");
                var newHabitDate = Console.ReadLine();
                var sanitizedNewDate = SanitizeNullOrWhiteSpace(newHabitDate);
                if (IsExit(sanitizedNewDate)) return;
                var newDate = SanitizeDate(sanitizedNewDate).ToString(DateFormats[0]);
                habit.Date = DateOnly.Parse(newDate);
                break;
            case "1":
                Console.WriteLine("\nEditing the Habit Name");
                Console.WriteLine("Enter the new name");
                var newHabitName = Console.ReadLine();
                var sanitizedNewHabit = SanitizeNullOrWhiteSpace(newHabitName);
                if (IsExit(sanitizedNewHabit)) return;
                habit.HabitName = sanitizedNewHabit;
                break;
            case "2":
                Console.WriteLine("\nEditing the Habit Quantity");
                Console.WriteLine("Enter the new Quantity");
                var newQuantity = Console.ReadLine();
                if (IsExit(newQuantity ?? "")) return;
                var sanitizedQuantity = SanitizeQuantity(newQuantity);
                habit.Quantity = sanitizedQuantity;
                break;
            case "3":
                Console.WriteLine("\nEditing the Habit Units");
                Console.WriteLine("Enter the new Unit");
                var newUnits = Console.ReadLine();
                var sanitizedNewUnits = SanitizeNullOrWhiteSpace(newUnits);
                if (IsExit(sanitizedNewUnits)) return;
                habit.Units = sanitizedNewUnits;
                break;
        }

        habitDb.UpdateHabit(habit);
        Console.WriteLine("Record updated, press enter to continue");
        Console.ReadLine();
    }

    private static void DeleteEntry(HabitdB habitDb)
    {
        Console.WriteLine("Enter the record ID would you like to delete, or E to exit");
        var id = Console.ReadLine()?.ToLower();

        while (string.IsNullOrWhiteSpace(id) || !habitDb.CheckEntryExists(id) || id == "e")
        {
            if (id == "e")
            {
                Console.WriteLine("Exiting Delete Option, press enter to continue");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("invalid entry please try again or press E to exit");
            id = Console.ReadLine()?.ToLower();
        }

        if (habitDb.DeleteHabit(id))
        {
            Console.WriteLine("Record deleted successfully, press any key to continue");
        }
        else
        {
            Console.WriteLine("Failed to delete record, press any key to continue");
        }

        Console.ReadKey();
    }

    private static void ViewRecords(HabitdB habitDb)
    {
        var habits = habitDb.GetAllRecords();
        foreach (var habit in habits)
        {
            Console.WriteLine($"Id: {habit.Id,-4} " +
                              $"Date: {habit.Date,-20} " +
                              $"HabitName: {habit.HabitName,-20} " +
                              $"Quantity: {habit.Quantity,-10} " +
                              $"Units: {habit.Units,-10}");
        }
    }

    private static void AddNewHabit(HabitdB habitDb)
    {
        Console.WriteLine("Enter Date completed mm-dd-yyyy, or type 'E' to exit");
        string? dateEntry = Console.ReadLine();
        string date = SanitizeNullOrWhiteSpace(dateEntry);
        if (IsExit(date)) return;
        string? dateValue = SanitizeDate(dateEntry).ToString(DateFormats[0]);

        Console.WriteLine("Enter Habit Name, or type 'E' to exit");
        string? habitNameEntry = Console.ReadLine();
        string habitName = SanitizeNullOrWhiteSpace(habitNameEntry);
        if (IsExit(habitName)) return;

        Console.WriteLine("Enter Quantity complete");
        string? quantityEntry = Console.ReadLine();
        var quantity = SanitizeQuantity(quantityEntry);

        Console.WriteLine("Enter type of Units tracked");
        string? unitsEntry = Console.ReadLine();
        string units = SanitizeNullOrWhiteSpace(unitsEntry);
        if (IsExit(units)) return;

        var newHabit = new Habit
        {
            Date = DateOnly.Parse(dateValue),
            HabitName = habitName,
            Quantity = quantity,
            Units = units
        };

        habitDb.AddHabit(newHabit);
        Console.WriteLine("New entry added, press enter to continue");
        Console.ReadLine();
    }

    private static DateOnly SanitizeDate(string? dateEntry)
    {
        while (true)
        {
            try
            {
                // Try to parse the date using the specified formats
                if (DateOnly.TryParseExact(dateEntry, DateFormats, null, System.Globalization.DateTimeStyles.None, out DateOnly dateValue))
                {
                    Console.WriteLine("Converted date to convention");
                    return dateValue;
                }
                else
                {
                    throw new FormatException("Invalid date format.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to convert date, please try again.");
                Console.WriteLine("Enter Date completed (mm-dd-yyyy or dd-mm-yyyy), or type 'E' to exit");
                string? newDateEntry = Console.ReadLine();
                dateEntry = SanitizeNullOrWhiteSpace(newDateEntry);
                if (IsExit(dateEntry)) return DateOnly.MinValue;
            }
        }
    }

    private static int SanitizeQuantity(string? entry)
    {
        while (true)
        {
            if (int.TryParse(entry, out int validQuantity) && validQuantity > 0)
            {
                return validQuantity;
            }

            Console.WriteLine("Invalid Entry please enter a numerical quantity");
            entry = Console.ReadLine();
        }
    }

    private static bool IsExit(string entry)
    {
        if (entry.ToLower() == "e")
        {
            Console.WriteLine("Exiting to main menu, press enter to continue");
            Console.ReadLine();
            return true;
        }

        return false;
    }

    private static string SanitizeNullOrWhiteSpace(string? entryName)
    {
        while (string.IsNullOrWhiteSpace(entryName))
        {
            Console.WriteLine("invalid entry please try again or press E to exit");
            entryName = Console.ReadLine()?.ToLower();
        }

        return entryName;
    }

    private static void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("What do you want to do?\n");
        Console.WriteLine("Type 0 to Close Application");
        Console.WriteLine("Type 1 to View all Records");
        Console.WriteLine("Type 2 to Add a record");
        Console.WriteLine("Type 3 to Delete a record");
        Console.WriteLine("Type 4 to Edit a record");
    }
}
