using System.Text.RegularExpressions;

namespace ConsoleHabitTracker;

public static class Menu
{
    private static readonly string[] DateFormats = { "MM-dd-yyyy", "dd-MM-yyyy" };

    public static void DisplayMainMenu()
    {
        Console.Clear();
        Console.WriteLine("What do you want to do?\n");
        Console.WriteLine("Type 0 to Close Application");
        Console.WriteLine("Type 1 to View all Records");
        Console.WriteLine("Type 2 to Add a record");
        Console.WriteLine("Type 3 to Delete a record");
        Console.WriteLine("Type 4 to Edit a record");
    }

    private static void DisplayEditMenu()
    {
        Console.Clear();
        Console.WriteLine("What part of the entry would you like to edit:");
        Console.WriteLine("\tEdit the Habit Date select 0 and press enter");
        Console.WriteLine("\tEdit the Habit Name select 1 and press enter");
        Console.WriteLine("\tEdit the Habit Quantity select 2 and press enter");
        Console.WriteLine("\tEdit the Habit Units select 3 and press enter");
    }

    public static void DisplayAllRecords(IEnumerable<Habit> habits)
    {
        foreach (var habit in habits)
        {
            Console.WriteLine($"Id: {habit.Id,-4} " +
                              $"Date: {habit.Date,-20} " +
                              $"HabitName: {habit.HabitName,-20} " +
                              $"Quantity: {habit.Quantity,-10} " +
                              $"Units: {habit.Units,-10}");
        }
    }

    

    public static Habit? AddNewHabit()
    {
        Console.WriteLine("Enter Date completed mm-dd-yyyy, or type 'E' to exit");
        string? dateEntry = Console.ReadLine();
        string date = Menu.SanitizeNullOrWhiteSpace(dateEntry);
        if (Menu.IsExit(date)) return null;
        string? dateValue = SanitizeDate(dateEntry).ToString(DateFormats[0]);

        Console.WriteLine("Enter Habit Name, or type 'E' to exit");
        string? habitNameEntry = Console.ReadLine();
        string habitName = Menu.SanitizeNullOrWhiteSpace(habitNameEntry);
        if (Menu.IsExit(habitName)) return null;

        Console.WriteLine("Enter Quantity complete");
        string? quantityEntry = Console.ReadLine();
        var quantity = Menu.SanitizeQuantity(quantityEntry);

        Console.WriteLine("Enter type of Units tracked");
        string? unitsEntry = Console.ReadLine();
        string units = Menu.SanitizeNullOrWhiteSpace(unitsEntry);
        if (Menu.IsExit(units)) return null;

        var newHabit = new Habit
        {
            Date = DateOnly.Parse(dateValue),
            HabitName = habitName,
            Quantity = quantity,
            Units = units
        };

        return newHabit;
        
    }

    public static string? GetValidHabitId(IEnumerable<Habit> habits)
    {
        var habitIdHash = habits.Select(h => h.Id.ToString()).ToHashSet();
        Console.WriteLine("Enter the record ID or E to exit");
        var id = Console.ReadLine()?.ToLower();

        while (string.IsNullOrWhiteSpace(id) || !habitIdHash.Contains(id) || id == "e")
        {
            if (id == "e")
            {
                Console.WriteLine("Exiting to main menu, press enter to continue");
                Console.ReadLine();
                return null;
            }

            Console.WriteLine("invalid entry please try again or press E to exit");
            id = Console.ReadLine()?.ToLower();
        }

        return id;
    }
    
    public static Habit? UpdateEntry(Habit habit)
    {
        Menu.DisplayEditMenu();
        var selectColumnToEdit = Console.ReadLine()?.ToLower();
        while (string.IsNullOrWhiteSpace(selectColumnToEdit) ||
               !Regex.IsMatch(selectColumnToEdit, "^[eE0123]$"))
        {
            Console.WriteLine("invalid entry please try again or press E to exit");
            selectColumnToEdit = Console.ReadLine()?.ToLower();
        }
        
        if (selectColumnToEdit.ToLower() == "e")
        {
            Console.WriteLine("Exiting, press enter to continue");
            Console.ReadLine();
            return null;
        }

        switch (selectColumnToEdit)
        {
            case "0":
                Console.WriteLine("\nEditing the Habit Date");
                Console.WriteLine("Enter the new Date, MM-DD-YYYY");
                var newHabitDate = Console.ReadLine();
                var sanitizedNewDate = SanitizeNullOrWhiteSpace(newHabitDate);
                if (Menu.IsExit(sanitizedNewDate)) return null;
                var newDate = SanitizeDate(sanitizedNewDate).ToString(DateFormats[0]);
                habit.Date = DateOnly.Parse(newDate);
                break;
            case "1":
                Console.WriteLine("\nEditing the Habit Name");
                Console.WriteLine("Enter the new name");
                var newHabitName = Console.ReadLine();
                var sanitizedNewHabit = SanitizeNullOrWhiteSpace(newHabitName);
                if (Menu.IsExit(sanitizedNewHabit)) return null;
                habit.HabitName = sanitizedNewHabit;
                break;
            case "2":
                Console.WriteLine("\nEditing the Habit Quantity");
                Console.WriteLine("Enter the new Quantity");
                var newQuantity = Console.ReadLine();
                if (Menu.IsExit(newQuantity ?? "")) return null;
                var sanitizedQuantity = SanitizeQuantity(newQuantity);
                habit.Quantity = sanitizedQuantity;
                break;
            case "3":
                Console.WriteLine("\nEditing the Habit Units");
                Console.WriteLine("Enter the new Unit");
                var newUnits = Console.ReadLine();
                var sanitizedNewUnits = SanitizeNullOrWhiteSpace(newUnits);
                if (Menu.IsExit(sanitizedNewUnits)) return null;
                habit.Units = sanitizedNewUnits;
                break;
        }

        return habit;
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

    private static DateOnly SanitizeDate(string? dateEntry)
    {
        while (true)
        {
            try
            {
                // Try to parse the date using the specified formats
                if (DateOnly.TryParseExact(dateEntry, DateFormats, null, System.Globalization.DateTimeStyles.None,
                        out DateOnly dateValue))
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
                dateEntry = Menu.SanitizeNullOrWhiteSpace(newDateEntry);
                if (Menu.IsExit(dateEntry)) return DateOnly.MinValue;
            }
        }
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
}