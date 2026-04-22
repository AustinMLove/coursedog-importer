namespace CoursedogImporter.Models;

// Represents the requisitesFreeform block in the PUT request body
public class RequisitesFreeform
{
  public bool ShowInCatalog { get; set; } = true;
  public string Value { get; set; }
}

// Wraps the freeform block in the requisites container
public class RequisitesUpdate
{
  public RequisitesFreeform RequisitesFreeform { get; set; }
}

// Top-level PUT request body shape
public class ProgramUpdateRequest
{
  public RequisitesUpdate Requisites { get; set; }
}
