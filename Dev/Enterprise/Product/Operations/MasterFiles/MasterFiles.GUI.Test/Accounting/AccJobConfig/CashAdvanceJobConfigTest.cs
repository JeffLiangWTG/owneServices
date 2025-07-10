using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CashAdvanceJobConfig))]
	internal class CashAdvanceJobConfigTest : BasherTest
	{
		[RequiresSTA]
		public void TestChargeCodeMultiSelectIsDisabledWhenJobConfigIsReadOnly()
		{
			AssertChargeCodeMultiSelectControlEditability(true);
		}

		[RequiresSTA]
		public void TestChargeCodeMultiSelectIsEnabledWhenJobConfigIsNotReadOnly()
		{
			AssertChargeCodeMultiSelectControlEditability(false);
		}

		void AssertChargeCodeMultiSelectControlEditability(bool jobConfigIsReadOnly)
		{
			using (var form = CreateFormForTest())
			{
				var companyForTest = ((GlbCompany)form.DataSource);
				var configForTest = Factory.New<DummyCashAdvanceConfig>();
				configForTest.CAC_GC = GlbCompany.CurrentCompany.PK;
				configForTest.CAC_Ledger = LedgerTypes.AccountsReceivable;
				configForTest.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Export;
				configForTest.CAC_TransportMode = Core.Constants.TransportModes.Sea;
				configForTest.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
				if (jobConfigIsReadOnly)
				{
					configForTest.SetReadOnlyForTest();
					Assert(configForTest.ReadOnly);
				}
				else
				{
					Assert(!configForTest.ReadOnly);
				}
				companyForTest.CashAdvanceConfigurations.Add(configForTest);

				form.Show();

				var jobConfigGrid = form.FindSingle<ZGrid>("jobConfigGrid");
				AssertEquals(configForTest.PK, jobConfigGrid.GetCurrentPK());

				var chargeCodeMultiSelectUserControl = form.FindSingle<CashAdvanceChargeCodeMultiSelectUserControl>("chargeCodeMultiSelectUserControl1");
				Assert(chargeCodeMultiSelectUserControl.Visible);
				AssertEquals(jobConfigIsReadOnly, !chargeCodeMultiSelectUserControl.Enabled);
			}
		}

		class DummyCashAdvanceConfig : AccCashAdvanceDefaultingConfiguration
		{
			public DummyCashAdvanceConfig(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			bool isReadOnlyForTest;

			public void SetReadOnlyForTest() => isReadOnlyForTest = true;

			public override bool ReadOnly
			{
				get => isReadOnlyForTest;
				set => base.ReadOnly = value;
			}
		}

		[RequiresSTA]
		public void TestBindingOfChargeCodeMultiSelectUserControlOnLoad()
		{
			using (var form = CreateFormForTest())
			{
				var companyForTest = ((GlbCompany)form.DataSource);
				var companyLevelConfig1 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, defaultingOption: CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);
				companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig1);
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				var chargeCodePivot = companyLevelConfig1.ChargeCodes.AddNew();
				chargeCodePivot.JCT_ParentId = chargeCode.PK;

				form.Show();

				var jobConfigGrid = form.FindSingle<ZGrid>("jobConfigGrid");
				AssertEquals(companyLevelConfig1.PK, jobConfigGrid.GetCurrentPK());

				var chargeCodesGroupBox = form.FindSingle<ZGroupBox>("ChargeCodesGroupBox");
				Assert(chargeCodesGroupBox.Visible);

				var multiSelectControl = form.FindSingle<CashAdvanceChargeCodeMultiSelectUserControl>("chargeCodeMultiSelectUserControl1");
				AssertNotNull(multiSelectControl.CurrentDataItem);
			}
		}

		public void TestBindingOfChargeCodeMultiSelectUserControlWhenUserSelectJobConfigRecord()
		{
			using (var form = CreateFormForTest())
			{
				var companyForTest = ((GlbCompany)form.DataSource);
				var companyLevelConfig1 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, defaultingOption: CashAdvanceDefaultingOption.All);
				var companyLevelConfig2 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, ledger: LedgerTypes.AccountsReceivable, defaultingOption: CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);
				companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig1);
				companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig2);

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				var chargeCodePivot = companyLevelConfig2.ChargeCodes.AddNew();
				chargeCodePivot.JCT_ParentId = chargeCode.PK;

				form.Show();

				var jobConfigGrid = form.FindSingle<ZGrid>("jobConfigGrid");
				AssertEquals(companyLevelConfig1.PK, jobConfigGrid.GetCurrentPK());

				var chargeCodesGroupBox = form.FindSingle<ZGroupBox>("ChargeCodesGroupBox");
				Assert(!chargeCodesGroupBox.Visible);

				var multiSelectControl = form.FindSingle<CashAdvanceChargeCodeMultiSelectUserControl>("chargeCodeMultiSelectUserControl1");
				AssertEquals(companyLevelConfig1.PK, ((AccCashAdvanceDefaultingConfiguration)jobConfigGrid.GetCurrent()).PK);
				AssertEquals(companyLevelConfig1.PK, ((AccCashAdvanceDefaultingConfiguration)multiSelectControl.CurrentDataItem).PK);

				jobConfigGrid.SelectSingleElementByPK(companyLevelConfig2.PK);
				AssertEquals(companyLevelConfig2.PK, ((AccCashAdvanceDefaultingConfiguration)jobConfigGrid.GetCurrent()).PK);
				Assert(chargeCodesGroupBox.Visible);
				AssertEquals(companyLevelConfig2.PK, ((AccCashAdvanceDefaultingConfiguration)multiSelectControl.CurrentDataItem).PK);
			}
		}

		public void TestChargeGroupAndChargeCodeGridVisibilityOnLoad()
		{
			AssertChargeGroupAndChargeCodeGridVisibilityOnLoad(CashAdvanceDefaultingOption.All);
			AssertChargeGroupAndChargeCodeGridVisibilityOnLoad(CashAdvanceDefaultingOption.None);
			AssertChargeGroupAndChargeCodeGridVisibilityOnLoad(CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);

			void AssertChargeGroupAndChargeCodeGridVisibilityOnLoad(ZString defaultingOptionOfFirstJobConfig)
			{
				using (var form = CreateFormForTest())
				{
					var companyForTest = ((GlbCompany)form.DataSource);
					var companyLevelConfig1 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, defaultingOption: defaultingOptionOfFirstJobConfig);
					companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig1);

					form.Show();

					var jobConfigGrid = form.FindSingle<ZGrid>("jobConfigGrid");
					AssertEquals(companyLevelConfig1.PK, jobConfigGrid.GetCurrentPK());
					AssertEquals(defaultingOptionOfFirstJobConfig, ((AccCashAdvanceDefaultingConfiguration)jobConfigGrid.GetCurrent()).CAC_DefaultingOption);

					var chargeGroupsGroupBox = form.FindSingle<ZGroupBox>("ChargeGroupsGroupBox");
					var chargeCodesGroupBox = form.FindSingle<ZGroupBox>("ChargeCodesGroupBox");
					var shouldGroupBoxesBeVisible = defaultingOptionOfFirstJobConfig == CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

					AssertEquals(shouldGroupBoxesBeVisible, chargeGroupsGroupBox.Visible);
					AssertEquals(shouldGroupBoxesBeVisible, chargeCodesGroupBox.Visible);
				}
			}
		}

		public void TestChargeGroupAndChargeCodeGridVisibilityWhenUserSelectJobConfigRecord()
		{
			AssertChargeGroupAndChargeCodeGridVisibilityOnLoadWhenUserSelectJobConfigRecord(CashAdvanceDefaultingOption.All);
			AssertChargeGroupAndChargeCodeGridVisibilityOnLoadWhenUserSelectJobConfigRecord(CashAdvanceDefaultingOption.None);
			AssertChargeGroupAndChargeCodeGridVisibilityOnLoadWhenUserSelectJobConfigRecord(CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);

			void AssertChargeGroupAndChargeCodeGridVisibilityOnLoadWhenUserSelectJobConfigRecord(ZString defaultingOptionOfSelectedJobConfig)
			{
				using (var form = CreateFormForTest())
				{
					var companyForTest = ((GlbCompany)form.DataSource);
					var companyLevelConfig1 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, defaultingOption: CashAdvanceDefaultingOption.All);
					var companyLevelConfig2 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, ledger: LedgerTypes.AccountsReceivable, defaultingOption: defaultingOptionOfSelectedJobConfig);
					companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig1);
					companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig2);

					form.Show();

					var jobConfigGrid = form.FindSingle<ZGrid>("jobConfigGrid");
					jobConfigGrid.SelectSingleElementByPK(companyLevelConfig2.PK);
					AssertEquals(companyLevelConfig2.PK, jobConfigGrid.GetCurrentPK());
					AssertEquals(defaultingOptionOfSelectedJobConfig, ((AccCashAdvanceDefaultingConfiguration)jobConfigGrid.GetCurrent()).CAC_DefaultingOption);

					var chargeGroupsGroupBox = form.FindSingle<ZGroupBox>("ChargeGroupsGroupBox");
					var chargeCodesGroupBox = form.FindSingle<ZGroupBox>("ChargeCodesGroupBox");
					var shouldGroupBoxesBeVisible = defaultingOptionOfSelectedJobConfig == CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

					AssertEquals(shouldGroupBoxesBeVisible, chargeGroupsGroupBox.Visible);
					AssertEquals(shouldGroupBoxesBeVisible, chargeGroupsGroupBox.Visible);
				}
			}
		}

		public void TestChargeGroupAndChargeCodeGridVisibilityWhenUserChangeDefaultingOption()
		{
			using (var form = CreateFormForTest())
			{
				var companyForTest = ((GlbCompany)form.DataSource);
				var companyLevelConfig1 = CreateCashAdvanceConfig(companyForTest.PK, ZGuid.Empty, defaultingOption: CashAdvanceDefaultingOption.All);
				companyForTest.CashAdvanceConfigurations.Add(companyLevelConfig1);

				form.Show();

				var jobConfigGrid = form.FindSingle<ZGrid>("jobConfigGrid");
				AssertEquals(companyLevelConfig1.PK, jobConfigGrid.GetCurrentPK());
				AssertEquals(CashAdvanceDefaultingOption.All, ((AccCashAdvanceDefaultingConfiguration)jobConfigGrid.GetCurrent()).CAC_DefaultingOption);

				var chargeGroupsGroupBox = form.FindSingle<ZGroupBox>("ChargeGroupsGroupBox");
				var chargeCodesGroupBox = form.FindSingle<ZGroupBox>("ChargeCodesGroupBox");

				Assert(!chargeGroupsGroupBox.Visible);
				Assert(!chargeGroupsGroupBox.Visible);

				companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

				Assert(chargeGroupsGroupBox.Visible);
				Assert(chargeGroupsGroupBox.Visible);

				companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.None;

				Assert(!chargeGroupsGroupBox.Visible);
				Assert(!chargeGroupsGroupBox.Visible);

				companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

				Assert(chargeGroupsGroupBox.Visible);
				Assert(chargeGroupsGroupBox.Visible);

				companyLevelConfig1.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;

				Assert(!chargeGroupsGroupBox.Visible);
				Assert(!chargeGroupsGroupBox.Visible);
			}
		}

		AccCashAdvanceDefaultingConfiguration CreateCashAdvanceConfig(ZGuid parentCompanyPk, ZGuid parentPK, string parentTableCode = "", string ledger = "", string defaultingOption = "ALL")
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			config.CAC_GC = parentCompanyPk.IsEmpty ? GlbCompany.CurrentCompany.PK : parentCompanyPk;
			config.CAC_Ledger = ledger;
			config.CAC_ParentTableCode = parentTableCode;
			config.CAC_ParentId = parentPK;
			config.CAC_ServiceDirection = "IMP";
			config.CAC_TransportMode = "SEA";
			config.CAC_DefaultingOption = defaultingOption;
			return config;
		}

		#region Implementation

		public override Form GetFormToBash() => CreateFormForTest();

		ZChildForm CreateFormForTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Hungary;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true

			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new CashAdvanceJobConfig();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(company, nameof(GlbCompany.CashAdvanceConfigurations));
			return form;
		}

		#endregion
	}
}
