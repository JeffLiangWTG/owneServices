using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusAuthorisationForm))]
	sealed class CusAuthorisationFormTest : ZFormBasherTest
	{
		public void TestAuthorisationNumberCharacterCasing()
		{
			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var authorisationNumberTextBox = form.AuthorisationNumberZTextBox;
				AssertEquals(CharacterCasing.Upper, authorisationNumberTextBox.CharacterCasing);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var gbAuthorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				gbAuthorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				using (var form = new CusAuthorisationForm(gbAuthorisation))
				{
					form.Show();
					AssertEquals(CharacterCasing.Normal, form.AuthorisationNumberZTextBox.CharacterCasing);

					gbAuthorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
					AssertEquals(CharacterCasing.Upper, form.AuthorisationNumberZTextBox.CharacterCasing);
				}
			}
		}

		public void TestAdHocVisible()
		{
			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var adHocCheckBox = form.AdHocCheckBox;
				AssertEquals(false, adHocCheckBox.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var frAuthorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				using (var form = new CusAuthorisationForm(frAuthorisation))
				{
					form.Show();
					var adHocCheckBox = form.AdHocCheckBox;
					AssertEquals(true, adHocCheckBox.Visible);
				}
			}
		}

		public void TestFormCaption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var frAuthorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				using (var form = new CusAuthorisationForm(frAuthorisation))
				{
					frAuthorisation.CPH_IsAdHoc = true;
					AssertEquals(form.FormCaption, "Temporary Authorization");
					frAuthorisation.CPH_IsAdHoc = false;
					AssertEquals(form.FormCaption, "Authorization");
				}
			}
		}

		public void TestReloadNumberRangesTabPage()
		{
			var erAuthorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();

			using (var form = new CusAuthorisationForm(erAuthorisation))
			{
				form.Show();
				var numberRangesTabPage = form.NumberRangesTabPage;

				erAuthorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals(false, numberRangesTabPage.TabVisible);

				erAuthorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
				AssertEquals(false, numberRangesTabPage.TabVisible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var frAuthorisation = Factory.NewWithValidTestData<CusAuthorisationHeader>();

				using (var form = new CusAuthorisationForm(frAuthorisation))
				{
					form.Show();
					var numberRangesTabPage = form.NumberRangesTabPage;

					frAuthorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
					AssertEquals(false, numberRangesTabPage.TabVisible);

					frAuthorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
					AssertEquals(true, numberRangesTabPage.TabVisible);
				}
			}
		}

		public void TestAuthorizationAddress()
		{
			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var authorizationAddressControl = form.FindSingle<ZAddressControl>("AuthorizationAddressControl");
				AssertEquals(false, authorizationAddressControl.ShowAddress);
			}
		}

		public void TestAuthorisationDescription()
		{
			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var authorizationDescriptionControl = form.FindSingle<ZTextBox>("AuthorisationDescriptionZTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", "CPH_PermitDescription", authorizationDescriptionControl.BindTo);
				});
			}
		}

		public void TestAuthorisationRuleGrid()
		{
			(string ColumnName, string ColumnCaption, int ColumnWidth, Type columnStyleType)[] orderedColumnDetails = {
				(CusAuthorisationRule.Schema.CPR_RuleCode, "Rule Code", ControlDpiScalingHelper.ScaleToCurrentDpiX(72), typeof(ZDropEditColumnStyle)),
				(CusAuthorisationRule.Schema.CPR_ValueFrom, "Value", ControlDpiScalingHelper.ScaleToCurrentDpiX(114), typeof(ZMultiControlColumnStyle)),
				(CusAuthorisationRule.Schema.CPR_Description, "Description", ControlDpiScalingHelper.ScaleToCurrentDpiX(280), typeof(ZMultiControlColumnStyle))
			};

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var groupBox = form.FindSingle<ZGroupBox>("AuthorisationRuleGroupBox");
				var grid = form.FindSingle<ZGrid>("AuthorisationRuleGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Grid caption", "Authorization Rules", groupBox.CaptionResourceString.Caption);
					foreach (var (columnName, columnCaption, columnWidth, columnStyleType) in orderedColumnDetails)
					{
						var columnStyle = grid.GetColumnStyle(columnName);
						AssertEquals($"{columnName} caption", columnCaption, grid.GetColumnCaption(columnName));
						AssertEquals($"{columnName} width", columnWidth, columnStyle.Width);
						AssertEquals($"{columnName} type", columnStyleType, columnStyle.ColumnStyleType);
						AssertEquals($"{columnName} not sortable", false, columnStyle.IsSortable);
					}
				});
			}
		}

		public void TestLinkedAuthorisationRuleGrid()
		{
			(string ColumnName, string ColumnCaption, int ColumnWidth, Type columnStyleType)[] orderedColumnDetails = {
				(LinkedCusAuthorisationRule.Schema.CPR_RuleCode, "Rule Code", ControlDpiScalingHelper.ScaleToCurrentDpiX(72), typeof(ZDropEditColumnStyle)),
				(LinkedCusAuthorisationRule.Schema.CPR_ValueFrom, "Value", ControlDpiScalingHelper.ScaleToCurrentDpiX(114), typeof(ZMultiControlColumnStyle)),
				(LinkedCusAuthorisationRule.Schema.CPR_Description, "Description", ControlDpiScalingHelper.ScaleToCurrentDpiX(280), typeof(ZTextBoxColumnStyle))
			};

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var groupBox = form.FindSingle<ZGroupBox>("LinkedAuthorisationRuleGroupBox");
				var grid = form.FindSingle<ZGrid>("LinkedAuthorisationRuleGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Grid caption", "Linked Rules", groupBox.CaptionResourceString.Caption);
					foreach (var (columnName, columnCaption, columnWidth, columnStyleType) in orderedColumnDetails)
					{
						var columnStyle = grid.GetColumnStyle(columnName);
						AssertEquals($"{columnName} caption", columnCaption, grid.GetColumnCaption(columnName));
						AssertEquals($"{columnName} width", columnWidth, columnStyle.Width);
						AssertEquals($"{columnName} type", columnStyleType, columnStyle.ColumnStyleType);
					}
				});
			}
		}

		public void TestLinkedRulesGridNotVisibleAfterLoad()
		{
			var cusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule.CPR_RuleCode = "USE";

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var linkedRulesGroupBox = form.FindSingle<ZGroupBox>("LinkedAuthorisationRuleGroupBox");

				CombineAssertions(() =>
				{
					AssertEquals("After loaded, LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
					cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
					AssertEquals("Changed rule code to LOC, LinkedAuthorisationRuleGrid is visible", true, linkedRulesGroupBox.Visible);
				});
			}
		}

		public void TestLinkedRulesGridVisibleAfterLoad()
		{
			var cusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var linkedRulesGroupBox = form.FindSingle<ZGroupBox>("LinkedAuthorisationRuleGroupBox");

				CombineAssertions(() =>
				{
					AssertEquals("After loaded, LinkedAuthorisationRuleGrid is visible", true, linkedRulesGroupBox.Visible);
					cusAuthorisationRule.CPR_RuleCode = "USE";
					AssertEquals("Changed rule code to LOC, LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
				});
			}
		}

		public void TestAuthorisationRuleGrid_ChangeRow()
		{
			var cusAuthorisationRule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule1.CPR_RuleCode = "USE";
			var cusAuthorisationRule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			var cusAuthorisationRule3 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var linkedRulesGroupBox = form.FindSingle<ZGroupBox>("LinkedAuthorisationRuleGroupBox");
				var authorisationRulesGrid = form.FindSingle<ZGrid>("AuthorisationRuleGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Current is 1st row, LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
					authorisationRulesGrid.CurrentRowIndex = 1;
					AssertEquals("Change to 2nd row, LinkedAuthorisationRuleGrid visible", true, linkedRulesGroupBox.Visible);
					authorisationRulesGrid.CurrentRowIndex = 2;
					AssertEquals("Change to 3nd row (empty rule code), LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
					cusAuthorisationRule3.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
					AssertEquals("Entered rule code LOC, LinkedAuthorisationRuleGrid is now visible", true, linkedRulesGroupBox.Visible);
				});
			}
		}

		public void TestAuthorisationRuleGrid_DeleteRow()
		{
			var cusAuthorisationRule1 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule1.CPR_RuleCode = "USE";
			var cusAuthorisationRule2 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			cusAuthorisationRule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			var cusAuthorisationRule3 = cusAuthorisationHeader.CusAuthorisationRules.AddNew();

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var linkedRulesGroupBox = form.FindSingle<ZGroupBox>("LinkedAuthorisationRuleGroupBox");
				var authorisationRulesGrid = form.FindSingle<ZGrid>("AuthorisationRuleGrid");
				authorisationRulesGrid.CurrentRowIndex = 2;

				CombineAssertions(() =>
				{
					AssertEquals("Current is 3rd row, LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
					cusAuthorisationRule3.Delete();
					AssertEquals("Deleted 3rd row, Current is 2nd row, LinkedAuthorisationRuleGrid visible", true, linkedRulesGroupBox.Visible);
					cusAuthorisationRule2.Delete();
					AssertEquals("Deleted 2nd row, Current is 1st row, LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
					cusAuthorisationRule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
					AssertEquals("Updated rule code to LOC, LinkedAuthorisationRuleGrid visible", true, linkedRulesGroupBox.Visible);
					cusAuthorisationRule1.Delete();
					AssertEquals("Deleted 1st row, No authorisation rules, LinkedAuthorisationRuleGrid not visible", false, linkedRulesGroupBox.Visible);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestBinding()
		{
			var underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
			TestBind(underbond, ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestBindingWithBindPrepend()
		{
			DummyBizoWithUnderbondCollection dummy = Factory.New<DummyBizoWithUnderbondCollection>();
			TestBind(dummy, "Underbonds.");
		}

		public void TestGridId()
		{
			using (var control = new CusOutturnUserControl())
			{
				AssertEquals("GridLayoutDGu8fdRQgEtBCLuEeuYwuA==", control.OutturnsGrid.GridId);
			}
		}

		public void TestSetBindPrepend()
		{
			using (CusOutturnUserControl control = new CusOutturnUserControl())
			{
				control.InitializeComponent();
				control.SetBindPrepend("XXX.");
				Assert("StatusTextBox.BindTo prepended", control.StatusTextBox.BindTo.StartsWith("XXX."));
				Assert("DateTimeOfOutturnDateEdit.BindTo prepended", control.DateTimeOfOutturnDateEdit.BindTo.StartsWith("XXX."));
				Assert("DateTimeOfUnloadDateEdit.BindTo prepended", control.DateTimeOfUnloadDateEdit.BindTo.StartsWith("XXX."));
				Assert("zGrid1.BindTo prepended", control.OutturnsGrid.BindTo.StartsWith("XXX."));
			}
		}

		public void TestSearchOfOutturns()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.GetIsUnderbondForSeaShipmentResult = false;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };

			CusOutturn outturnLine1 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine2 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine3 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine4 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine5 = underbond1.Outturns.AddNew();
			CusOutturn outturnLine6 = underbond1.Outturns.AddNew();

			outturnLine1.C5_HouseBill = "Default";
			outturnLine2.C5_HouseBill = "CuckooSqueaker";
			outturnLine3.C5_HouseBill = "Default";
			outturnLine4.C5_HouseBill = "Default";
			outturnLine5.C5_HouseBill = "Gibbiceps";
			outturnLine6.C5_HouseBill = "Default";

			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);

			using (ZForm form = new ZForm(dummy))
			{
				using (CusUnderbondUserControl underbondControl = new CusUnderbondUserControl())
				{
					underbondControl.DisableVisibilityCheck();
					underbondControl.SetBindPrepend("");
					underbondControl.Parent = form;

					underbondControl.SetDataBinding(collection, "");
					form.Show();
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;

					underbondControl.DetailsUserControl.MainTabControl.SelectedTab = underbondControl.DetailsUserControl.OutturnTabPage;

					CusOutturnUserControl outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;

					outturnControl.OutturnSearchTextBox.Text = "Cuckoo";
					AssertEquals(1, outturnControl.OutturnsGrid.CurrentRowIndex);

					outturnControl.OutturnSearchTextBox.Text = "Gibb";
					AssertEquals(4, outturnControl.OutturnsGrid.CurrentRowIndex);
				}
			}
		}

		public void TestSeaOnlyControlsVisibility()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.GetIsUnderbondForSeaShipmentResult = false;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.GetIsUnderbondForSeaShipmentResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };

			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);

			using (ZForm form = new ZForm(dummy))
			{
				using (CusUnderbondUserControl underbondControl = new CusUnderbondUserControl())
				{
					underbondControl.DisableVisibilityCheck();
					underbondControl.SetBindPrepend("");
					form.Controls.Add(underbondControl);
					underbondControl.SetDataBinding(collection, "");
					form.Show();
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;

					underbondControl.DetailsUserControl.MainTabControl.SelectedTab = underbondControl.DetailsUserControl.OutturnTabPage;

					CusOutturnUserControl outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;

					AssertSeaControlsVisibility(outturnControl, false);

					underbondControl.UnderbondsGrid.CurrentRowIndex = 1;
					outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;
					AssertSeaControlsVisibility(outturnControl, true);

					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;
					AssertSeaControlsVisibility(outturnControl, false);
				}
			}
		}

		public void TestSeaColumns()
		{
			using (CusOutturnUserControl control = new CusOutturnUserControl())
			{
				List<string> seaColumns = new List<string>(control.SeaColumns);
				AssertEquals(10, seaColumns.Count);
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_SealIntactIndicator));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_ContainerNumber));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_HouseBill));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_MasterBill));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_OuterPackUnits));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_CargoReceiptDate));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_CargoUnpackDate));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_ReceiptOnlyIndicator));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_CargoType));
				Assert(seaColumns.Contains(CusOutturnSchema.Constants.C5_PackagesUnits));
			}
		}

		public void TestRemoveColumnFromGrid()
		{
			ICusUnderbondDependentCollectionParent dummyWithUnderbonds = Factory.New<DummyBizoWithUnderbondCollection>();
			TestHelperCusUnderbond underbond1 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond1.GetIsUnderbondForSeaShipmentResult = false;
			TestHelperCusUnderbond underbond2 = (TestHelperCusUnderbond)dummyWithUnderbonds.Underbonds.AddNew(typeof(TestHelperCusUnderbond));
			underbond2.GetIsUnderbondForSeaShipmentResult = true;
			DummyCusUnderbondUnionCollectionParent dummy = Factory.New<DummyCusUnderbondUnionCollectionParent>();
			dummy.AllPossibleCollectionProviders = new ICusUnderbondDependentCollectionParent[] { dummyWithUnderbonds };

			CusUnderbondUnionCollectionParentCollection collection = new CusUnderbondUnionCollectionParentCollection(Factory, typeof(DummyCusUnderbondUnionCollectionParent));
			collection.Add(dummy);

			using (ZForm form = new ZForm(dummy))
			{
				using (CusUnderbondUserControl underbondControl = new CusUnderbondUserControl())
				{
					underbondControl.DisableVisibilityCheck();
					underbondControl.SetBindPrepend("");
					underbondControl.Parent = form;

					underbondControl.SetDataBinding(collection, "");
					form.Show();
					underbondControl.UnderbondsGrid.CurrentRowIndex = 0;

					underbondControl.DetailsUserControl.MainTabControl.SelectedTab = underbondControl.DetailsUserControl.OutturnTabPage;

					CusOutturnUserControl outturnControl = underbondControl.DetailsUserControl.OutturnUserControl;

					AssertNotNull(outturnControl.OutturnsGrid.Columns[CusOutturnSchema.Constants.C5_OuterPacks]);
					underbondControl.UnderbondsGrid.CurrentRowIndex = 1;
					outturnControl.RemoveColumnFromGrid(CusOutturnSchema.Constants.C5_OuterPacks);
					AssertNull(outturnControl.OutturnsGrid.Columns[CusOutturnSchema.Constants.C5_OuterPacks]);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			Factory.Save();
			return new CusAuthorisationForm(cusAuthorisationHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		}
		CusAuthorisationHeader cusAuthorisationHeader;

		void TestBind(BusinessObject bizo, ZString bindPrepend)
		{
			using (ZForm form = new ZForm(bizo))
			{
				using (CusOutturnUserControl control = new CusOutturnUserControl())
				{
					control.Parent = form;
					if (!bindPrepend.IsEmpty)
					{
						control.SetBindPrepend(bindPrepend);
					}

					form.Show();
					control.SetDataBinding(bizo, "");
				}
			}
		}

		void AssertSeaControlsVisibility(CusOutturnUserControl outturnControl, bool expectedVisibility)
		{
			AssertEquals("DateTimeOfUnloadDateEdit.Visible", expectedVisibility, outturnControl.DateTimeOfUnloadDateEdit.Visible);
			AssertEquals("DateTimeOfUnloadLabel.Visible", expectedVisibility, outturnControl.DateTimeOfUnloadLabel.Visible);
			foreach (string column in outturnControl.SeaColumns)
			{
				if (column != CusOutturnSchema.Constants.C5_ReceiptOnlyIndicator)
				{
					AssertEquals(column + " column", expectedVisibility, outturnControl.OutturnsGrid.Columns[column] != null);
				}
				else
				{
					AssertEquals(false, outturnControl.OutturnsGrid.Columns[column] != null);
				}
			}
		}

		sealed class TestHelperCusUnderbond : CusUnderbond
		{
			public TestHelperCusUnderbond(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool GetIsUnderbondForSeaShipmentResult;

			protected override bool GetIsUnderbondForSeaShipment() => GetIsUnderbondForSeaShipmentResult;

			protected override bool GetCanDoOutturn() => true;

			protected override TypeLoaderCollection GetParentLoaders()
			{
				var result = base.GetParentLoaders();
				result.Add(new TypeLoader(typeof(DummyBizoWithUnderbondCollection)));
				return result;
			}
		}
	}
}
