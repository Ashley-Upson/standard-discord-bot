string searchTerm = "Brokers";
var results = new TheStandard().SearchAsync(searchTerm).GetAwaiter().GetResult();

foreach (var result in results)
{
    Console.WriteLine(result.Title);
    Console.WriteLine($" - {result.Link}");
}

Console.ReadKey();
