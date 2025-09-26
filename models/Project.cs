namespace AppExtractor;

public class Project
{
  public int ReferenceId { get; set; }
  public string? ApplicationNumber { get; set; }
  public string Unit { get; set; }
  public string? StatusMoreInfo { get; set; }
  public string Status { get; set; }
  public string StatusDate { get; set; }
  public string Address { get; set; }
  public string? ApplicationType { get; set; }
  public string? AssignedStaff { get; set; }
  public string SysRef { get; set; }
  public string? Archived { get; set; }


  public Project(string text, string isArchived)
  {
    var parts = text.Split("|");

    // Part 1 Address Removes last 5 numbers
    Address = parts[0].Substring(0, parts[0].Length - 5).TrimEnd();

    // Part 2 ReferenceId
    ReferenceId = int.Parse(parts[1].Trim());

    // Part 3 Status
    Status = parts[2].Trim();

    // Part 4 ApplicationNumber
    if (!String.IsNullOrWhiteSpace(parts[3]))
    {
      ApplicationNumber = parts[3].Trim();
    }

    // Part 6 SysRef
    SysRef = parts[6].Trim();

    // Part 9 StatusDate
    StatusDate = parts[8].Trim();

    // Part 10 Unit
    Unit = parts[9].Trim();

    Archived = isArchived;
  }

}
