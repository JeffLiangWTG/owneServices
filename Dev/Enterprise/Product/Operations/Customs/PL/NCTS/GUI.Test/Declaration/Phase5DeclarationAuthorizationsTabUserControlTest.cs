using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.Customs.EU.Business.AutoCusAuthorizationUsage.Schema;
using NctsHeader = Enterprise.Customs.PL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class Phase5DeclarationAuthorizationsTabUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
	}

	public void TestGridDockStyle()
	{
		AssertEquals("Dock", DockStyle.Fill, userControl.AuthorisationsGrid.Dock);
	}

	public void TestGridBindingMember()
	{
		AssertEquals("BindingMember", "MovementHeader.CusAuthorizationUsages", userControl.AuthorisationsGrid.GetBindingMember());
	}

	public void TestAvailableColumns()
	{
		AssertSequencesEqual("Columns", new[] { AGC_Code, AGC_Number, AGC_OH_Owner, AGC_Location },
			 userControl.AuthorisationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	public void TestColumnsWidth()
	{
		var authorisationsGrid = userControl.AuthorisationsGrid;
		CombineAssertions(() =>
		{
			AssertEquals("AGC_Code", 50, authorisationsGrid.GetColumnStyle(AGC_Code).Width);
			AssertEquals("AGC_Number", 130, authorisationsGrid.GetColumnStyle(AGC_Number).Width);
			AssertEquals("AGC_OH_Owner", 120, authorisationsGrid.GetColumnStyle(AGC_OH_Owner).Width);
			AssertEquals("AGC_Location", 100, authorisationsGrid.GetColumnStyle(AGC_Location).Width);
		});
	}

	public void TestLocationFindBoxColumnStyle_PopupSelected()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Number = "1234";
		authorisationHeader.CPH_Type = "ACE";
		authorisationHeader.CPH_OH_PermitHolder = org.PK;
		var rule = authorisationHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = "LOC";
		rule.CPR_ValueFrom = "WAW";

		Factory.Save();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var usage = header.MovementHeader.CusAuthorizationUsages.AddNew();
		using (var form = new ZForm(header))
		{
			form.Controls.Add(userControl);
			userControl.Dock = DockStyle.Fill;
			form.Show();
			var authorisationsGrid = userControl.AuthorisationsGrid;
			authorisationsGrid.Select(0);
			var locationColumnStyle = (ZCodeFindBoxColumnStyle)authorisationsGrid.Columns[AGC_Location].ColumnStyle;
			var locationFindBox = (ZGridFindBox)locationColumnStyle.EditControl;
			var findBox = (IFindBox)locationFindBox;
			var findBoxPopupForm = (EmbeddedModulePopup)findBox.PopupForm;
			var methodHookControlEvents = typeof(ZCodeFindBoxColumnStyle).GetMethod("HookControlEvents", BindingFlags.NonPublic | BindingFlags.Instance);
			methodHookControlEvents.Invoke(locationColumnStyle, null);
			findBoxPopupForm.Module_ForTest.ModuleDecisionProvider.HandleFindBoxOKButton(new[] { rule });

			CombineAssertions(() =>
			{
				AssertEquals("Code has been set", "ACE", usage.AGC_Code);
				AssertEquals("Number has been set", "1234", usage.AGC_Number);
				AssertEquals("Owner has been set", org.PK, usage.AGC_OH_Owner);
				AssertEquals("Location has been set", "WAW", usage.AGC_Location);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new Phase5DeclarationAuthorizationsTabUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	Phase5DeclarationAuthorizationsTabUserControl userControl;
}
