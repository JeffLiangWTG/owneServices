using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class CountryReferenceModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestDetach()
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();

			var countryRef1 = Factory.NewWithValidTestData<UNDGCountryReference>();
			var countryRef2 = Factory.NewWithValidTestData<UNDGCountryReference>();

			undgSubstance.UNDGCountryReferences.Add(countryRef1);
			undgSubstance.UNDGCountryReferences.Add(countryRef2);

			countryRef2.DCR_Code = "ZZ";
			countryRef2.DCR_Type = "PSA";
			countryRef2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;

			(undgSubstance.UNDGCountryReferences.GetRelationshipBusinessObject(countryRef2) as UNDGCountryReferencePivot).DCP_IsSystem = true;

			Factory.Save();

			using (ZForm form = new ZForm())
			{
				var grid = CreateGrid();
				form.Controls.Add(grid);
				grid.InnerGrid.SetDataBinding(undgSubstance, "UNDGCountryReferences");
				grid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ClickButton(grid, ZModuleButtonGrid.Buttons.Detach);
				CombineAssertions("Should be detached", () =>
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Are you sure you want to detach the selected records?\r\nAll unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, undgSubstance.UNDGCountryReferences.Count);
				});

				grid.InnerGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ClickButton(grid, ZModuleButtonGrid.Buttons.Detach);
				CombineAssertions("Should NOT be detached", () =>
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("System maintained Country/Region Reference/s (SG, PSA) cannot be detached.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, undgSubstance.UNDGCountryReferences.Count);
				});
			}
		}

		public void TestIFilterModuleExtraNotificationProvider()
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();

			var countryRef1 = Factory.NewWithValidTestData<UNDGCountryReference>();
			var countryRef2 = Factory.NewWithValidTestData<UNDGCountryReference>();

			countryRef2.DCR_Code = "ZZ";
			countryRef2.DCR_Type = "PSA";
			countryRef2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;

			var notificationProvider = undgSubstance.Lookups.UNDGCountryReferences as IFilterModuleExtraNotificationProvider;

			AssertNull("Attach is allowed, no notification", notificationProvider.GetExtraNotification(countryRef1));

			var notification = notificationProvider.GetExtraNotification(countryRef2);
			CombineAssertions("Attach is not allowed", () =>
			{
				AssertEquals("error notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
				AssertEquals("error message", "System maintained Country/Region Reference/s (SG, PSA) cannot be attached.", notification.Message);
			});
		}

		public void TestIFilterModuleExtraNotificationProvider_SecurityRight()
		{
			var previousSecurityValue = Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed;
			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = false;
			try
			{
				var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();

				var countryRef1 = Factory.NewWithValidTestData<UNDGCountryReference>();
				countryRef1.DCR_Code = "1234";
				countryRef1.DCR_Type = "ICPE";
				countryRef1.DCR_RN_NKCountry = Core.Constants.CountryCodes.France;

				var notificationProvider = undgSubstance.Lookups.UNDGCountryReferences as IFilterModuleExtraNotificationProvider;
				var notification = notificationProvider.GetExtraNotification(countryRef1);
				AssertNotNull("No security rights, attach not allowed", notification);
				AssertEquals("No security rights", "You do not have the security rights to attach Country/Region Regulations.", notification.Message);
			}
			finally
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = previousSecurityValue;
			}
		}

		void ClickButton(CountryReferencesModuleButtonGrid grid, string button)
		{
			var toolStrip = grid.Controls.Find("toolStrip", true)[0] as ZToolStrip;
			toolStrip.Items.Find(button, true)[0].PerformClick();
		}

		CountryReferencesModuleButtonGrid CreateGrid()
		{
			var grid = new CountryReferencesModuleButtonGrid();
			grid.BindToGridList = "UNDGCountryReferences";
			grid.BindToFindBoxList = "Lookups+UNDGCountryReferences";
			var columnStyle = new ZCalcEditColumnStyleInfo();
			columnStyle.ColumnName = "DCR_RN_NKCountry";
			grid.InnerGrid.ColumnStyles.Add(columnStyle);
			return grid;
		}

		public void TestCountryReferencesModuleButtonGrid()
		{
			var dgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			dgSubstance.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E2;

			using (var countryReferencesGrid = new CountryReferencesModuleButtonGrid())
			{
				AssertEquals("New MUST BE disabled", false, countryReferencesGrid.ShowNewButton);
				AssertEquals("Edit MUST BE disabled", false, countryReferencesGrid.ShowEditButton);
				AssertEquals("Attach should be enabled", true, countryReferencesGrid.ShowAttachButton);
				AssertEquals("Detach should be enabled", true, countryReferencesGrid.ShowDetachButton);
			}
		}
	}

	[TestedType(typeof(CountryReferencesModuleButtonGrid))]
	class CountryReferenceModuleButtonGridBaseTest : ZModuleButtonGridTestBase
	{
	}
}
