// See https://aka.ms/new-console-template for more information

using System.Text.Json;

const string filePath = "tasks.json";
const string initialMessage = "Type 'help' to see commands, 'exit' to quit.";
var tasks = LoadTasks();

Console.WriteLine("Task CLI");
Console.WriteLine(initialMessage);

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input))
        continue;

    var splitCount = input.Contains("add") || input.Contains("delete") ? 2 : 3;
    
    var parts =  input.Split(' ', splitCount, StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();
    
    switch (command)
    {
        case "add":
            AddTask(parts, tasks);
            break;
        case "update":
            UpdateTaskDescription(parts, tasks);
            break;
        case "delete":
            DeleteTask(parts[1], tasks);
            break;
        case "mark-in-progress":
            UpdateTaskStatus(parts, tasks);
            break;
        case "mark-done":
            UpdateTaskStatus(parts, tasks);
            break;
        case "list":
            ListTask(parts, tasks);
            break;
        case "help":
            ShowHelp();
            break;
        case "exit":
            SaveTasks(tasks);
            Console.WriteLine("Goodbye!");
            return;
        default:
            Console.WriteLine($"Unknown command. {initialMessage}");
            break;
    }
}

static void ListTask(string[] parts, List<TaskItem> tasks)
{
    foreach (var task in tasks)
    {
        if (!task.Deleted)
        {
            if (parts.Length == 1)
            {
                Console.WriteLine($"ID: {task.Id} - Description: {task.Description} - Status: {task.Status}");
            }
            else
            {
                if (task.Status == parts[1])
                {
                    Console.WriteLine($"ID: {task.Id} - Description: {task.Description} - Status: {task.Status}");
                }
            }
        }
    }
}

static void ShowHelp()
{
    Console.WriteLine("""
    Commands:
        add <description>
        update <id> <new description>
        delete <id>
        mark-in-progress <id>
        mark-done <id>
        list
        list done
        list todo
        list in-progress
        help
        exit
    """);
}

static void DeleteTask(string taskId, List<TaskItem> tasks)
{
    if (string.IsNullOrWhiteSpace(taskId) || !int.TryParse(taskId, out var id))
    {
        Console.WriteLine("Usage: delete <id>");
        return;
    }
    
    var task = tasks.Where(t => !t.Deleted).FirstOrDefault(t => t.Id == id);
    if (task == null)
    {
        Console.WriteLine($"Task with ID {id} not found.");
        return;
    }
    
    task.Deleted = true;
    Console.WriteLine($"Task with ID {id} has been deleted.");
}

static void UpdateTaskStatus(string[] parts, List<TaskItem> tasks)
{
    if (parts.Length < 2 || !int.TryParse(parts[1], out var id))
    {
        Console.WriteLine("Usage: mark-in-progress | mark-done <id>");
        return;
    }
    
    var task = tasks.Where(t => !t.Deleted).FirstOrDefault(t => t.Id == id);
    if (task == null)
    {
        Console.WriteLine($"Task with ID {id} not found.");
        return;
    }
    
    task.Status = parts[0].Replace("mark-","");
    task.UpdatedAt = DateTime.Now;
    Console.WriteLine($"Task status updated: ID {task.Id}");
}

static void UpdateTaskDescription(string[] parts, List<TaskItem> tasks)
{
    if (parts.Length < 3 || !int.TryParse(parts[1], out var id))
    {
        Console.WriteLine("Usage: update <id> <new description>");
        return;
    }
    
    var task = tasks.Where(t => !t.Deleted).FirstOrDefault(t => t.Id == id);
    if (task == null)
    {
        Console.WriteLine($"Task with ID {id} not found.");
        return;
    }
    
    task.Description = parts[2];
    task.UpdatedAt = DateTime.Now;
    Console.WriteLine($"Task description updated: ID {task.Id}");
}

static void AddTask(string[] parts, List<TaskItem> tasks)
{
    var newId = tasks.Count == 0 ? 1 : tasks.Max(t => t.Id) + 1;
    
    var task = new TaskItem()
    {
        Id = newId,
        Description = parts[1],
        CreatedAt = DateTime.Now,
    };
    
    tasks.Add(task);
    Console.WriteLine($"Task added: ID {task.Id}");
}

static List<TaskItem> LoadTasks()
{
    if (!File.Exists(filePath))
        return new List<TaskItem>();
    
    var json = File.ReadAllText(filePath);
    return JsonSerializer.Deserialize<List<TaskItem>>(json)
        ?? new List<TaskItem>();
}

static void SaveTasks(List<TaskItem> tasks)
{
    var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
    {
        WriteIndented = true
    });
    
    File.WriteAllText(filePath, json);
}

public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; } =  string.Empty;
    public string Status { get; set; } =  "todo";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Deleted { get; set; }
}

