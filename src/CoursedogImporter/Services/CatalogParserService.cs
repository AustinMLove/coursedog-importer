using HtmlAgilityPack;
using CoursedogImporter.Models;

namespace CoursedogImporter.Services;

public class CatalogParserService
{
  public ProgramRequirements ParseFromFile(string filePath)
  {
    if (!File.Exists(filePath))
      throw new FileNotFoundException($"Catalog HTML file not found: {filePath}");

    var doc = new HtmlDocument();
    doc.Load(filePath);

    var requirements = new ProgramRequirements();

    // Extract program title from h1
    var h1 = doc.DocumentNode.SelectSingleNode("//h1");
    if (h1 != null)
      requirements.ProgramTitle = h1.InnerText.Trim();

    // Select all relevant nodes
    var allNodes = doc.DocumentNode.SelectNodes("//h2|//h3|//h4|//ul");
    if (allNodes == null)
      return requirements;
    RequirementSection currentSection = null;
    bool inRequirements = false;
    foreach (var node in allNodes)
    {
      var nodeName = node.Name.ToLower();
      var nodeText = node.InnerText.Trim();
      // h2 controls scope — either starts extraction, stops it, or is skipped
      if (nodeName == "h2")
      {
        if (nodeText.Contains("Degree Requirements") ||
            nodeText.Contains("Certificate Requirements"))
        {
          inRequirements = true;
          continue;
        }
        if (nodeText.Contains("Completion Plan"))
          break;
        continue;
      }
      // Skip everything outside the requirements scope
      if (!inRequirements)
        continue;
      // h3 indicates a new requirement section
      if (nodeName == "h3")
      {
        currentSection = new RequirementSection
        {
          SectionName = nodeText
        };
        requirements.Sections.Add(currentSection);
      }
      // h4 indicates either a credit hour subtotal or an option group subheading
      else if (nodeName == "h4" && currentSection != null)
      {
        if (nodeText.StartsWith("Subtotal"))
        {
          currentSection.Entries.Add(new RequirementEntry
          {
            Type = EntryType.Subtotal,
            Text = nodeText
          });
        }
        else
        {
          currentSection.Entries.Add(new RequirementEntry
          {
            Type = EntryType.SubHeading,
            Text = nodeText
          });
        }
      }
      // ul contains course entries and elective placeholders
      else if (nodeName == "ul" && currentSection != null)
      {
        foreach (var li in node.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
        {
          var liClass = li.GetAttributeValue("class", "");
          if (liClass.Contains("acalog-course"))
          {
            // Course entry — extract text from anchor tag
            var anchor = li.SelectSingleNode(".//a");
            if (anchor != null)
            {
              var text = anchor.InnerText.Trim();
              if (!string.IsNullOrWhiteSpace(text))
                currentSection.Entries.Add(new RequirementEntry
                {
                  Type = EntryType.Course,
                  Text = text
                });
            }
          }
          else if (liClass.Contains("acalog-adhoc-list-item"))
          {
            // Placeholder bullet — elective note within the main list
            var text = li.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
              currentSection.Entries.Add(new RequirementEntry
              {
                Type = EntryType.Placeholder,
                Text = text
              });
          }
        }
      }
    }
    return requirements;
  }

  // Extract course code
  public static string ExtractCourseCode(string fullText)
  {
    var separatorIndex = fullText.IndexOf(" - ");
    if (separatorIndex < 0)
      return fullText;

    return fullText.Substring(0, separatorIndex).Trim();
  }
}
