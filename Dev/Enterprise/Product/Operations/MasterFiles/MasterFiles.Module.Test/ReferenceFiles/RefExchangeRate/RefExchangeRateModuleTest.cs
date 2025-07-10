using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefExchangeRateModule))]
	sealed class RefExchangeRateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ExchangeRate;

		protected override string GetIsSystemDefinedDefaultProperty() => ZString.Empty;

		public void TestToolbarButtons()
		{
			using (RefExchangeRateModule module = (RefExchangeRateModule)ZModuleFactory.Instance.Create(ModuleIDs.ExchangeRate))
			{
				AssertNotNull("Ex Rate Update button should exist", module.ToolBarButtons.FindByText("Ex Rate Update"));
			}
		}

		public void TestExRateButtonShowsForm()
		{
			bool originalSec = Env.Security.CurrenciesModify.IsAllowed;
			Env.Security.CurrenciesModify.IsAllowed = false;

			try
			{
				using (RefExchangeRateModule module = (RefExchangeRateModule)ZModuleFactory.Instance.Create(ModuleIDs.ExchangeRate))
				{
					ZToolBarButton button = (ZToolBarButton)module.ToolBarButtons.FindByText("Ex Rate Update");
					AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						button.PerformClick();
						AssertEquals("ExRate Update Form error shown as security denied",
							"You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\nMaintain -> References -> Currencies -> Edit",
							UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
			finally
			{
				Env.Security.CurrenciesModify.IsAllowed = originalSec;
			}
		}

		[RequiresSTA]
		public void TestReproduceWI00512551()
		{
			var rateID = Guid.NewGuid();
			var sqlInsertRefExchangeRate = $@"
INSERT INTO RefDatabase_RefExchangeRateZZ
 ([ZZN_PK], [ZZN_ExRateType], [ZZN_StartDate], [ZZN_EndDate], [ZZN_Rate], [ZZN_RX_NKExCurrency], [ZZN_RN_NKCountry], [ZZN_AsPublished])
VALUES('{rateID}', 'CUS', GETDATE(), DATEADD(day, 1, GETDATE()), 4.8, 'CNY', '{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}', 1);
";

			Db.Connection.ExecuteNonQuery(sqlInsertRefExchangeRate);
			var factory = new BusinessObjectFactory();

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			factory.Save();

			rateID = (Guid)Db.Connection.ExecuteScalar($@"SELECT uuid = CONVERT(uniqueidentifier, HASHBYTES('SHA2_256', CONCAT(ZZN_PK, '{GlbCompany.CurrentCompany.GC_Code}'))) FROM RefDatabase_RefExchangeRateZZ
WHERE ZZN_PK = '{rateID}'");

			using (var moduleForm = new ZForm())
			using (var module = (RefExchangeRateModule)ZModuleFactory.Instance.Create(ModuleIDs.ExchangeRate))
			{
				var filterControl = module.EmbeddedControl;
				moduleForm.Controls.Add(filterControl);
				moduleForm.Show();

				var tempFilePath = Temp.GetTempFileNameWithExtension("xml");
				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectSingleElement(module.GridCollection.Find(new ZQuery(RefExchangeRateSchema.PK, rateID))[0]);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;
				UnitTestUserNotification.Instance.ThrowOnWarningAndError = true;
				var exportNativeXmlMenuItem = (ZMenuItem)typeof(RefExchangeRateModule)
					.GetProperty("ExportNativeXmlMenuItem", BindingFlags.Instance | BindingFlags.NonPublic)
					.GetValue(module);
				try
				{
					AssertNoExceptionThrown(exportNativeXmlMenuItem.PerformClick);
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}
			}
		}

		public void TestFilterOnMultiRowResult()
		{
			var table = new DataTable();
			table.Columns.Add(new DataColumn("ID", typeof(int)));
			table.Columns.Add(new DataColumn(RefExchangeRateSchema.RE_GC.Name, typeof(Guid)));

			using (var testClass = new RefExchangeRateModuleForTest())
			{
				CombineAssertions(() =>
				{
					AssertNull("Empty DataTable", testClass.FilterOnMultiRowResultExploded(table));

					table.Rows.Add(1, new Guid());
					var dataRow = testClass.FilterOnMultiRowResultExploded(table);
					AssertNull("1 row but unmatched, should return null", dataRow);

					table.Rows.Add(2, Env.CurrentCompanyPK);
					dataRow = testClass.FilterOnMultiRowResultExploded(table);
					AssertEquals("1 matched row", 2, dataRow.Field<int>("ID"));

					table.Rows.Add(3, new Guid());
					dataRow = testClass.FilterOnMultiRowResultExploded(table);
					AssertEquals("1 matched with unmatched", 2, dataRow.Field<int>("ID"));

					table.Rows.Add(4, Env.CurrentCompanyPK);
					AssertExceptionThrown<InvalidOperationException>("exception expected when multi matched rows", () => testClass.FilterOnMultiRowResultExploded(table));
				});
			}
		}
	}
}
