using System.Text;
using CoursedogImporter.Models;

namespace CoursedogImporter.Services;

public class RequirementBlockBuilder
{
  // Builds HTML string from all requirement sections
  // This becomes the value of requisitesFreeform.value in the PUT payload

  public string BuildFreeformHtml(ProgramRequirements requirements)
  {
    var sb = new StringBuilder();

    foreach (var section in requirements.Sections)
    {
      // Use section name to create h1 header
      sb.AppendLine($"<h1>{section.SectionName}</h1>");

      foreach (var entry in section.Entries)
      {
        switch (entry.Type)
        {
          case EntryType.Course:
            sb.AppendLine(BuildCourseLink(entry.Text));
            break;

          case EntryType.SubHeading:
            sb.AppendLine($"<h2>{entry.Text}</h2>");
            break;

          case EntryType.Placeholder:
            sb.AppendLine($"<p>{entry.Text}</p>");
            break;

          case EntryType.Subtotal:
            sb.AppendLine($"<blockquote><p><strong>{entry.Text}</strong></p></blockquote>");
            break;

          case EntryType.Narrative:
            sb.AppendLine($"<p>{entry.Text}</p>");
            break;
        }
      }
    }

    return sb.ToString();
  }

  // Builds a Coursedog internal link from course text string

  private string BuildCourseLink(string fullText)
  {
    // Extract and format course code
    var courseCode = CatalogParserService.ExtractCourseCode(fullText);
    var hrefCode = courseCode.Replace(" ", "");

    // Return internal Coursedog embedded course link
    return $"<p><a href=\"/courses/{hrefCode}\" " +
           $"class=\"custom-link\" " +
           $"data-course-id=\"{hrefCode}\">" +
           $"{fullText}</a></p>";
  }
}
