using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClaimDataManager;

/// <summary>
/// Holds all data-access and reporting logic for the application.
/// </summary>
public class ClaimService
{
    private readonly ClaimContext _context;

    public ClaimService(ClaimContext context) => _context = context;

    /// <summary>
    /// Creates the database from the model and seeds it once.
    /// </summary>
    public void Initialise()
    {
        _context.Database.EnsureCreated();

        // Only seed when the table is empty, otherwise the samples
        // would be duplicated on every run.
        if (_context.Claims.Any()) return;

        _context.Claims.AddRange(
            new Claim
            {
                LecturerName = "A. Phewa",
                ModuleCode = "PROG6212",
                HoursWorked = 40,
                HourlyRate = 350m,
                ClaimMonth = "August 2025",
                Status = "Approved"
            },
            new Claim
            {
                LecturerName = "T. Mokoena",
                ModuleCode = "DATA6222",
                HoursWorked = 25,
                HourlyRate = 400m,
                ClaimMonth = "September 2025",
                Status = "Pending"
            });

        _context.SaveChanges();
        Console.WriteLine("Sample claim records were added to the new database.\n");
    }

    public void AddClaim()
    {
        Console.WriteLine("\n--- Add a new claim ---");

        var claim = new Claim
        {
            LecturerName = ReadText("Lecturer name: "),
            ModuleCode = ReadText("Module code: "),
            HoursWorked = ReadHours(),
            HourlyRate = ReadRate(),
            ClaimMonth = ReadText("Claim month (e.g. October 2025): "),
            Status = "Draft"   // every new claim starts as a draft
        };

        _context.Claims.Add(claim);
        _context.SaveChanges();

        Console.WriteLine($"Claim {claim.ClaimId} saved with a total of R{claim.TotalAmount:N2}.");
    }

    public void ViewClaims()
    {
        // AsNoTracking is used because this is a read-only listing,
        // EF Core does not need to watch these objects for changes.
        var claims = _context.Claims.AsNoTracking().ToList();

        if (claims.Count == 0)
        {
            Console.WriteLine("\nThere are no claims to display.");
            return;
        }

        Console.WriteLine("\n--- All claim records ---");
        Console.WriteLine($"{"ID",-4}{"Lecturer",-18}{"Module",-12}{"Hours",-8}{"Rate",-10}{"Month",-18}{"Status",-12}{"Total",-12}");
        Console.WriteLine(new string('-', 94));

        foreach (var c in claims)
        {
            Console.WriteLine($"{c.ClaimId,-4}{c.LecturerName,-18}{c.ModuleCode,-12}" +
                              $"{c.HoursWorked,-8}{c.HourlyRate,-10:N2}{c.ClaimMonth,-18}" +
                              $"{c.Status,-12}{c.TotalAmount,-12:N2}");
        }
    }

    public void UpdateClaimStatus()
    {
        var claim = FindClaim("Enter the ID of the claim to update: ");
        if (claim is null) return;

        Console.WriteLine($"Current status: {claim.Status}");
        var status = ReadText("New status (Draft / Pending / Approved / Rejected): ");

        claim.Status = status;
        _context.SaveChanges();   // EF Core tracked the change, so this writes the update

        Console.WriteLine($"Claim {claim.ClaimId} is now marked as {claim.Status}.");
    }

    public void DeleteClaim()
    {
        var claim = FindClaim("Enter the ID of the claim to delete: ");
        if (claim is null) return;

        Console.Write($"Delete the claim for {claim.LecturerName}? (y/n): ");
        if (!string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Deletion cancelled.");
            return;
        }

        _context.Claims.Remove(claim);
        _context.SaveChanges();
        Console.WriteLine("The claim was deleted.");
    }

    /// <summary>
    /// Writes every claim to Reports/claim_summary.txt and reads it back.
    /// </summary>
    public void ExportReport()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Reports");
        Directory.CreateDirectory(folder);   // safe to call even if it exists

        var file = Path.Combine(folder, "claim_summary.txt");
        var claims = _context.Claims.AsNoTracking().ToList();

        // The using scope closes and flushes the writer automatically,
        // even if an exception is thrown while writing.
        using (var writer = new StreamWriter(file))
        {
            writer.WriteLine("CONTRACT CLAIM SUMMARY");
            writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            writer.WriteLine(new string('=', 60));

            foreach (var c in claims)
            {
                writer.WriteLine($"Claim ID   : {c.ClaimId}");
                writer.WriteLine($"Lecturer   : {c.LecturerName}");
                writer.WriteLine($"Module     : {c.ModuleCode}");
                writer.WriteLine($"Month      : {c.ClaimMonth}");
                writer.WriteLine($"Hours      : {c.HoursWorked}");
                writer.WriteLine($"Rate       : R{c.HourlyRate:N2}");
                writer.WriteLine($"Total      : R{c.TotalAmount:N2}");
                writer.WriteLine($"Status     : {c.Status}");
                writer.WriteLine(new string('-', 60));
            }

            writer.WriteLine($"Total claims exported: {claims.Count}");
        }

        Console.WriteLine($"\nReport written to: {file}\n");

        // Read the finished file back so the export can be verified.
        Console.WriteLine(File.ReadAllText(file));
    }

    /// <summary>
    /// Runs a direct SQL count against the database without EF Core.
    /// </summary>
    public void CountClaimsWithAdoNet()
    {
        using var connection = new SqliteConnection($"Data Source={ClaimContext.DbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Claims";

        // ExecuteScalar returns the single value in the first column of the first row.
        var count = Convert.ToInt32(command.ExecuteScalar());

        Console.WriteLine($"\nADO.NET direct query result: {count} claim record(s) in the database.");
    }

    private Claim? FindClaim(string prompt)
    {
        Console.Write(prompt);

        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("That is not a valid ID.");
            return null;
        }

        var claim = _context.Claims.Find(id);
        if (claim is null) Console.WriteLine($"No claim was found with ID {id}.");

        return claim;
    }

    private static string ReadText(string prompt)
    {
        string? input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                Console.WriteLine("This field cannot be left empty.");
        }
        while (string.IsNullOrWhiteSpace(input));

        return input.Trim();
    }

    private static double ReadHours()
    {
        while (true)
        {
            Console.Write("Hours worked (1 to 160): ");

            if (double.TryParse(Console.ReadLine(), out double hours) && hours is >= 1 and <= 160)
                return hours;

            Console.WriteLine("Hours must be a number between 1 and 160.");
        }
    }

    private static decimal ReadRate()
    {
        while (true)
        {
            Console.Write("Hourly rate (greater than 0): ");

            if (decimal.TryParse(Console.ReadLine(), out decimal rate) && rate > 0)
                return rate;

            Console.WriteLine("The hourly rate must be greater than zero.");
        }
    }
}