﻿namespace ConsoleHabitTracker;

/// <summary>
/// Entry point for the Habit Tracker console application.
/// Provides a menu-driven interface for managing habit records.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main method that starts the application.
    /// Displays a menu and allows the user to view, add, edit, or delete habit records.
    /// </summary>
    static void Main()
    {
        var continueProgram = true;

        string connectionString = "Data Source = habits.db; Version=3;";
        var habitDb = new HabitDb(connectionString);

        while (continueProgram)
        {
            Menu.DisplayMainMenu();
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
                    NewHabit(habitDb);
                    break;
                case '3':
                    Console.WriteLine("\n\nDelete a Record");
                    ViewRecords(habitDb);
                    DeleteEntry(habitDb);
                    break;
                case '4':
                    Console.WriteLine("\n\nEdit a Record");
                    UpdateHabit(habitDb);
                    break;
                default:
                    Console.WriteLine("\n\nInvalid Selection, press enter to try again");
                    Console.ReadLine();
                    break;
            }
        }
    }

    /// <summary>
    /// Deletes a habit record from the database.
    /// Prompts the user to select a record to delete.
    /// </summary>
    /// <param name="habitDb">The database connection to use.</param>
    private static void DeleteEntry(HabitDb habitDb)
    {
        var habits = ViewRecords(habitDb);
        var habitId = Menu.GetValidHabitId(habits);
        if (habitId is null)
        {
            return;
        }

        if (habitDb.DeleteHabit(habitId))
        {
            Console.WriteLine("Record deleted successfully, press any key to continue");
        }
        else
        {
            Console.WriteLine("Failed to delete record, press any key to continue");
        }

        Console.ReadKey();
    }

    /// <summary>
    /// Retrieves and displays all habit records from the database.
    /// </summary>
    /// <param name="habitDb">The database connection to use.</param>
    /// <returns>A list of all habit records in the database.</returns>
    private static List<Habit> ViewRecords(HabitDb habitDb)
    {
        var habits = habitDb.GetAllRecords();
        Menu.DisplayAllRecords(habits);
        return habits;
    }

    /// <summary>
    /// Prompts the user to create a new habit record and adds it to the database.
    /// </summary>
    /// <param name="habitDb">The database connection to use.</param>
    private static void NewHabit(HabitDb habitDb)
    {
        var newHabit = Menu.AddNewHabit();
        if (newHabit == null)
        {
            return;
        }
        habitDb.AddHabit(newHabit);
        Console.WriteLine("New entry added, press enter to continue");
        Console.ReadLine();
    }

    /// <summary>
    /// Prompts the user to update an existing habit record in the database.
    /// Displays the current records and allows the user to select a record to edit.
    /// </summary>
    /// <param name="habitDb">The database connection to use.</param>
    private static void UpdateHabit(HabitDb habitDb)
    {
        var habits = ViewRecords(habitDb);
        var habitIdString = Menu.GetValidHabitId(habits);
        if (habitIdString is null)
        {
            return;
        }

        var habitId = Convert.ToInt32(habitIdString);
        var habit = habits.First(h => h.Id == habitId);
        var updatedHabit = Menu.UpdateEntry(habit);
        if (updatedHabit is null)
        {
            return;
        }

        var success = habitDb.UpdateHabit(updatedHabit);
        if (success)
        {
            Console.WriteLine("Record updated, press enter to continue");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Unable to update record, press enter to continue");
            Console.ReadLine();
        }
    }
}
