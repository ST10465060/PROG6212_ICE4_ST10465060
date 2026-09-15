namespace ClaimDataManager;

/// <summary>
/// Represents a single lecturer claim record stored in the database.
/// </summary>
public class Claim
{
    // EF Core treats a property named "ClaimId" as the primary key by convention.
    public int ClaimId { get; set; }

    public string LecturerName { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;

    public double HoursWorked { get; set; }
    public decimal HourlyRate { get; set; }

    public string ClaimMonth { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Read-only calculated value. It is not stored in the database,
    /// it is worked out from the hours and the rate every time it is read.
    /// </summary>
    public decimal TotalAmount => (decimal)HoursWorked * HourlyRate;
}
