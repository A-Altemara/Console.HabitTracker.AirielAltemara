namespace ConsoleHabitTracker;

public class Habit
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? HabitName { get; set; }
    public int Quantity { get; set; }
    public string? Units { get; set; }
}