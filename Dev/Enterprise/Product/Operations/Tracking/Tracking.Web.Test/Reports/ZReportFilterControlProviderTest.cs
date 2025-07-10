using System;
using System.Reflection;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ZReportFilterControlProviderTest : TestCaseWithFactory
	{
		public void TestSetWebUserRestrictingValues()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TEST";
			testContact.OC_Email = "TEST@TEST.COM";
			testContact.SetHashedPassword("TEST");
			testContact.OC_WebAccessEnabled = true;
			Factory.Save();

			OrgContactWebUser testWebUser = new OrgContactWebUser();
			testWebUser.Login(testOrg.OH_Code, testContact.OC_Email, testContact.PasswordForTesting);
			Assert(testWebUser.IsLoggedIn);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_IsPublished, true);
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Client - Order Lines");

			ReportCommand[] commands = Factory.Load<ReportCommand>(filter);
			Assert("Should found a report command for test", commands.Length > 0);
			ReportCommand command = commands[0];

			ReportPrintSet printset = new ReportPrintSet(command);
			DocumentPack pack = printset[0];

			using (Report report = pack.GetFirstReport())
			{
				AssertNotNull("Should be a report for test", report);
				report.PrepareForRender();

				FilterField clientFilterField = null;
				foreach (FilterField field in report.FilterCollection)
				{
					if (field.DisplayName == "Client")
					{
						clientFilterField = field;
					}
				}
				AssertNotNull(clientFilterField);
				report.ColumnHeadingManager.SaveToFilterField = clientFilterField.DisplayName;
				AssertNotNull(report.LinkedLookupField);
				report.LinkedLookupField.Value = Guid.Empty;
				AssertEquals(Guid.Empty, report.LinkedLookupField.Value);

				ZReportFilterControlProvider.SetWebUserRestrictingValues(report, testWebUser);
				AssertEquals(testWebUser.CurrentOrg.ToGuid(), report.LinkedLookupField.Value);
			}
		}

		[ExpectNoExceptions()]
		public void TestSetWebUserRestrictingValuesNoClientLinkWithForceWebPublish()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TEST";
			testContact.OC_Email = "TEST@TEST.COM";
			testContact.SetHashedPassword("TEST");
			testContact.OC_WebAccessEnabled = true;
			Factory.Save();

			OrgContactWebUser testWebUser = new OrgContactWebUser();
			testWebUser.Login(testOrg.OH_Code, testContact.OC_Email, testContact.PasswordForTesting);
			Assert(testWebUser.IsLoggedIn);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_IsPublished, true);
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Client - Order Lines");

			ReportCommand[] commands = Factory.Load<ReportCommand>(filter);
			Assert("Should found a report command for test", commands.Length > 0);
			ReportCommand command = commands[0];

			command.Documents[0].Template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[ForceWebPublish]
{A}-[#SectionBody]
{A}-[#EndOfReport]");

			ReportPrintSet printset = new ReportPrintSet(command);
			DocumentPack pack = printset[0];

			using (Report report = pack.GetFirstReport())
			{
				AssertNotNull("Should be a report for test", report);
				report.PrepareForRender();

				AssertNull(report.LinkedLookupField);
				Assert(report.ForceWebPublish);

				ZReportFilterControlProvider.SetWebUserRestrictingValues(report, testWebUser);
			}
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Report Client - Order Line Report that has no link to the client is not supported on Web")]
		public void TestSetWebUserRestrictingValuesNoClientLinkWithoutForceWebPublish()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TEST";
			testContact.OC_Email = "TEST@TEST.COM";
			testContact.SetHashedPassword("TEST");
			testContact.OC_WebAccessEnabled = true;
			Factory.Save();

			OrgContactWebUser testWebUser = new OrgContactWebUser();
			testWebUser.Login(testOrg.OH_Code, testContact.OC_Email, testContact.PasswordForTesting);
			Assert(testWebUser.IsLoggedIn);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_IsPublished, true);
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Client - Order Lines");

			ReportCommand[] commands = Factory.Load<ReportCommand>(filter);
			Assert("Should found a report command for test", commands.Length > 0);
			ReportCommand command = commands[0];

			command.Documents[0].Template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[#EndOfReport]");

			ReportPrintSet printset = new ReportPrintSet(command);
			DocumentPack pack = printset[0];

			using (Report report = pack.GetFirstReport())
			{
				AssertNotNull("Should be a report for test", report);
				report.PrepareForRender();

				AssertNull(report.LinkedLookupField);
				Assert(!report.ForceWebPublish);

				ZReportFilterControlProvider.SetWebUserRestrictingValues(report, testWebUser);
			}
		}

		public void TestCreateFilterControls()
		{
			FilterField field = new MultipleChoice(Factory);
			Control[] results = ZReportFilterControlProvider.CreateFilterControls(field);
			AssertEquals(1, results.Length);
			AssertEquals(typeof(ZFilterStripDropDownList), results[0].GetType());
			Assert(((ZFilterStripDropDownList)results[0]).IsDescriptionsList);

			field = new CodeListMultipleChoice(Factory);
			results = ZReportFilterControlProvider.CreateFilterControls(field);
			AssertEquals(1, results.Length);
			AssertEquals(typeof(ZFilterStripDropDownList), results[0].GetType());
			Assert(((ZFilterStripDropDownList)results[0]).IsDescriptionsList);
		}

		public void TestShouldBeHiddenOnWeb()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_IsPublished, true);
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Client - Order Lines");

			ReportCommand[] commands = Factory.Load<ReportCommand>(filter);
			Assert("Should found a report command for test", commands.Length > 0);
			ReportCommand command = commands[0];

			ReportPrintSet printset = new ReportPrintSet(command);
			DocumentPack pack = printset[0];
			using (Report report = pack.GetFirstReport())
			{
				AssertNotNull("Should be a report for test", report);
				report.PrepareForRender(); // populates the collection of filters

				bool hidden = false;
				foreach (FilterField field in report.FilterCollection)
				{
					if (ZReportFilterControlProvider.ShouldBeHiddenOnWeb(report, field))
					{
						AssertEquals("Should be Client", "Client", field.DisplayName);
						hidden = true;
					}
				}
				Assert("There should be a hidden field", hidden);
			}
		}

		#region TestIsSupportedOnWeb

		public void TestIsSupportedOnWeb()
		{
			FilterField[] supportedFields = new FilterField[]
							{
								new TextField(Factory),
								new NumberField(Factory),
								new DateField(Factory),
								new DateRangeField(Factory),
								new DateTimeOffsetField(Factory),
								new DateTimeOffsetRangeField(Factory),
								new MultipleChoice(Factory),
								new CodeListMultipleChoice(Factory),
								AssignModuleID(new CodeLookupField(Factory)),
								AssignModuleID(new LookupField(Factory)),
								new OptionGroup(Factory),
							};

			AssertIsSupportedOnWeb(supportedFields, true, "{0} is supported on Web");

			FilterField[] notSupportedFields = new FilterField[]
							{
										new CodeLookupField(Factory),
										new LookupField(Factory),
							};

			AssertIsSupportedOnWeb(notSupportedFields, false, "{0} with undefined ModuleID is not supported on Web");
		}

		void AssertIsSupportedOnWeb(FilterField[] array, bool expectedSupportedOnWebValue, string messageTemplate)
		{
			foreach (FilterField field in array)
			{
				string message = string.Format(messageTemplate, field.GetType().Name);
				AssertEquals(message, expectedSupportedOnWebValue, ZReportFilterControlProvider.IsSupportedOnWeb(field));
			}
		}

		FilterField AssignModuleID(LookupFilterFieldBase field)
		{
			FieldInfo fieldInfo = field.GetType().GetField("fCollectionProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			AssertNotNull(fieldInfo);

			fieldInfo.SetValue(field, CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation"));

			return field;
		}

		#endregion
	}
}
