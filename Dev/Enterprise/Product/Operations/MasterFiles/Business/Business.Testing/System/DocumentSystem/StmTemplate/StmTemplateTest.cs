using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmTemplate))]
	sealed class StmTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsDocBuilderStyle()
		{
			Action<ZBool, ZString> assertIsDocBuilderStyle = (expected, templateName) =>
			{
				var template = (StmTemplate)GetNewBusinessObject();
				template.SO_Name = templateName;

				var message = string.Format("The template [{0}] {1} be a DocBuilder template.", template.SO_Name, expected ? "should" : "should not");

				AssertEquals(message, expected, template.IsDocBuilderStyle);
			};

			assertIsDocBuilderStyle(ZBool.True, "System Document Elements");
			assertIsDocBuilderStyle(ZBool.True, "System Document Elements [DE-DE]");
			assertIsDocBuilderStyle(ZBool.False, "System Document Elements [NL-NL");
			assertIsDocBuilderStyle(ZBool.False, "System Document Elements FRA]");
			assertIsDocBuilderStyle(ZBool.False, "System Document Elements ZH-TW");

			assertIsDocBuilderStyle(ZBool.True, "Customized Document Elements");
			assertIsDocBuilderStyle(ZBool.True, "Customized Document Elements [ZH-TW]");
			assertIsDocBuilderStyle(ZBool.False, "Customized Document Elements [FRA");
			assertIsDocBuilderStyle(ZBool.False, "Customized Document Elements NL-NL]");
			assertIsDocBuilderStyle(ZBool.False, "Customized Document Elements DE-DE");

			assertIsDocBuilderStyle(ZBool.False, "Pre-Alert");
			assertIsDocBuilderStyle(ZBool.False, "Pre-Alert [DE-DE]");
		}

		public void TestIsClientSpecificDocBuilderStyle()
		{
			StmTemplate systemTemplate = Factory.New<StmTemplate>();
			systemTemplate.SO_Name = Enterprise.Core.Constants.SectionRepositoryTemplateNames.User + " [DE-DE]";
			Assert("Not identified as DocBuilder template.", systemTemplate.IsClientSpecificDocBuilderStyle);
			systemTemplate.SO_Name = Enterprise.Core.Constants.SectionRepositoryTemplateNames.User;
			Assert("Not identified as DocBuilder template.", systemTemplate.IsClientSpecificDocBuilderStyle);
			systemTemplate.SO_Name = "My" + Enterprise.Core.Constants.SectionRepositoryTemplateNames.User + " [DE-DE]";
			Assert("Not identified as non DocBuilder template.", !systemTemplate.IsClientSpecificDocBuilderStyle);
		}

		public void TestIsDefaultLanguageClientSpecificDocBuilderStyle()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = Constants.SectionRepositoryTemplateNames.System;
			Assert(!template.IsDefaultLanguageClientSpecificDocBuilderStyle);
			template.SO_Name = Constants.SectionRepositoryTemplateNames.User;
			Assert(template.IsDefaultLanguageClientSpecificDocBuilderStyle);
			template.SO_Name = Constants.SectionRepositoryTemplateNames.User + " [ZH-CN]";
			Assert(!template.IsDefaultLanguageClientSpecificDocBuilderStyle);
		}

		public void TestIsSystemDocBuilderStyle()
		{
			StmTemplate systemTemplate = Factory.New<StmTemplate>();
			systemTemplate.SO_Name = Enterprise.Core.Constants.SectionRepositoryTemplateNames.System + " [DE-DE]";
			Assert("Not identified as DocBuilder template.", systemTemplate.IsSystemDocBuilderStyle);
			systemTemplate.SO_Name = Enterprise.Core.Constants.SectionRepositoryTemplateNames.System;
			Assert("Not identified as DocBuilder template.", systemTemplate.IsSystemDocBuilderStyle);
			systemTemplate.SO_Name = "My" + Enterprise.Core.Constants.SectionRepositoryTemplateNames.System + " [DE-DE]";
			Assert("Not identified as DocBuilder template.", !systemTemplate.IsSystemDocBuilderStyle);
		}

		public void TestSetDefaultValues()
		{
			var template = Factory.New<StmTemplate>();
			AssertEquals("default values for SO_TemplateType", StmTemplateTypes.Codes.Document, template.SO_TemplateType);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDataVersionsIsLoggedWithAuditLog()
		{
			using var adminConnection = Db.NewAdminConnection();
			var factory = new BusinessObjectFactory(adminConnection);
			var auditLogsHelperForTesting = new AuditLogsHelperForTesting(factory, StmTemplateSchema.Instance);

			var template = factory.NewWithValidTestData<StmTemplate>();
			template.SO_Name = "Mack";
			factory.Save();

			template.SO_Name = "McQueen";
			factory.Save();

			var auditEventCollection = auditLogsHelperForTesting.GetAuditLogCollection(template);

			AssertEquals("VersionLog count should be 2", 2, auditEventCollection.Count);
		}
	}
}
