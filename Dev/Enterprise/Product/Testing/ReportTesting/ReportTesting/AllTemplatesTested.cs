using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ReportTesting
{
	public class AllTemplatesTested : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAllTemplatesTested()
		{
			var filter = new ZQuery(StmTemplateSchema.SO_DataContext, nameof(Core.Constants.DataContext.None));
			filter.AddToFilter(StmTemplateSchema.SO_IsSystemDefined, true);

			var templates = new StmTemplateCollection(Factory);
			templates.Load(filter);

			var templateNamesFromDb = new List<string>();
			var templateNamesFromTemplateNameAttribute = new List<string>();

			foreach (StmTemplate template in templates)
			{
				templateNamesFromDb.Add(template.SO_Name);
			}

			foreach (var typeInAssembly in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (typeof(TemplateTestCase).IsAssignableFrom(typeInAssembly))
				{
					var attribute = typeInAssembly.GetCustomAttribute<TemplateNameAttribute>(inherit: false);

					if (attribute != null)
					{
						templateNamesFromTemplateNameAttribute.Add(attribute.TemplateName);
					}
				}
			}

			var reportScriptsAssembly = Assembly.Load("Enterprise.Build.Database.Script");
			if (reportScriptsAssembly == null)
			{
				Assert("You have done something unspeakable to assembly Enterprise.Build.Database.Script and it cannot be loaded", false);
			}

			var templatesTestedMultipleTimes = templateNamesFromTemplateNameAttribute
				.GroupBy(s => s)
				.Where(group => group.Count() > 1)
				.Select(group => group.Key)
				.OrderBy(s => s)
				.ToArray();

			var untestedTemplates = templateNamesFromDb
				.Except(templateNamesFromTemplateNameAttribute)
				.Except(suppressedTemplates)
				.OrderBy(s => s)
				.ToArray();

			var superfluousSuppressions = suppressedTemplates
				.Except(templateNamesFromDb.Except(templateNamesFromTemplateNameAttribute))
				.OrderBy(s => s)
				.ToArray();

			CombineAssertions(() =>
			{
				if (untestedTemplates.Length > 0)
				{
					string failures = string.Join(System.Environment.NewLine, untestedTemplates);
					Fail("These templates need to be tested using the super awesome and oh-so-useful 'TemplateTestCase' class. Or, add it to the suppression list (but make sure you test it using a ReportFunctionalTestCase class). The parameter of this attribute should be the name of the template, from SO_Name."
						+ System.Environment.NewLine + failures);
				}

				if (templatesTestedMultipleTimes.Length > 0)
				{
					string failures = string.Join(System.Environment.NewLine, templatesTestedMultipleTimes);
					Fail("These templates are tested in more than one class. Please test each template only once."
						+ System.Environment.NewLine + failures);
				}
				if (superfluousSuppressions.Length > 0)
				{
					string failures = string.Join(System.Environment.NewLine, superfluousSuppressions);
					Fail("These suppressions do not correspond to existing templates or the template has a test and so the suppression should be removed."
						+ System.Environment.NewLine + failures);
				}
			});
		}

		public void TestExtractDocumentXmlFromEmbeddedResource()
		{
			using var locator = new ClientSpecificDocumentsEmbeddedResourceLocator();
			{
				AssertNoExceptionThrown(() =>
				{
					var fileName = locator.ExtractClientDocumentXmlEmbeddedResourceToTempFile("EDI");
					AssertNotNull(fileName);
				});
			}
		}

		readonly IEnumerable<string> suppressedTemplates = new List<string>()
		{
			"FR Declaration Report",
			"FR Invoice Line Report",
			"FR Invoice Report",
			"FR VAT Report",
			"FR IST Accounting Report",
			"EU Temporary Storage Report",
			"Organization - Global Credit Profile Report",
			"Statement of Account",
		};
	}
}
