using ClaimDataManager;

Console.WriteLine("=====================================");
Console.WriteLine("   CONTRACT CLAIM DATA MANAGER");
Console.WriteLine("=====================================\n");

// The context is disposed automatically when the program ends.
using var context = new ClaimContext();
var service = new ClaimService(context);

try
{
    service.Initialise();
}
catch (Exception ex)
{
    Console.WriteLine($"The database could not be prepared: {ex.Message}");
    return;
}

bool running = true;

while (running)
{
    Console.WriteLine("\n1. Add a claim");
    Console.WriteLine("2. View all claims");
    Console.WriteLine("3. Update a claim status");
    Console.WriteLine("4. Delete a claim");
    Console.WriteLine("5. Export claims to a text file");
    Console.WriteLine("6. Count claims using ADO.NET");
    Console.WriteLine("7. Exit");
    Console.Write("\nChoose an option: ");

    string choice = Console.ReadLine() ?? string.Empty;

    try
    {
        switch (choice)
        {
            case "1": service.AddClaim(); break;
            case "2": service.ViewClaims(); break;
            case "3": service.UpdateClaimStatus(); break;
            case "4": service.DeleteClaim(); break;
            case "5": service.ExportReport(); break;
            case "6": service.CountClaimsWithAdoNet(); break;
            case "7":
                running = false;
                Console.WriteLine("Closing the application.");
                break;
            default:
                Console.WriteLine("Please choose a number between 1 and 7.");
                break;
        }
    }
    catch (Exception ex)
    {
        // One handler here keeps the menu alive if any operation fails.
        Console.WriteLine($"Something went wrong: {ex.Message}");
    }
}
