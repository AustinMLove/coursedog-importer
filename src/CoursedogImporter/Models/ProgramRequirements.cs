namespace CoursedogImporter.Models;

// Type of each entry within a requirement section
public enum EntryType
{
  Course,
  Placeholder,
  SubHeading,
  Subtotal,
  Narrative
}

// Single entry within a requirement section
public class RequirementEntry
{
  public EntryType Type { get; set; }

  public string Text { get; set; }
}

// Represents single course record 
public class CourseEntry
{
  public string FullText { get; set; }

  public string CourseCode { get; set; }

  public string CourseName { get; set; }

  public string CreditHours { get; set; }
}

// Single requirement section and entried within it 
public class RequirementSection
{
  public string SectionName { get; set; }

  public List<RequirementEntry> Entries { get; set; } = new();
}

// Top-level container for all extracted requirement data
public class ProgramRequirements
{
  public string ProgramTitle { get; set; }

  public List<RequirementSection> Sections { get; set; } = new();
}
