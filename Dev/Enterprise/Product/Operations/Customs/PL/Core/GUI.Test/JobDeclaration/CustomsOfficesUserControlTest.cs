using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class PLCustomsOfficesUserControlTest : TestCaseWithFactory
{
	public void TestColumns()
	{
		using (var control = new PLCustomsOfficesUserControl())
		{
			var grid = control.FindSingle<ZGrid>("CustomsOfficesGrid");
			var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName);
			AssertContainsExactElementsInExactOrder(new[]
			{
				EuOfficeCode.Schema.CY_Code,
				EuOfficeCode.Schema.CY_Data,
				EuOfficeCode.Schema.CY_OfficeDescription,
			}, columns);
		}
	}

	public void TestControlVisibility()
	{
		var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
		using var form = new ZForm(declaration);
		using var control = new PLCustomsOfficesUserControl();
		form.Controls.Add(control);
		form.Show();
		control.HandleDeclarationControlVisibilityChanged();

		CombineAssertions(() =>
		{
			Assert("CustomsOfficeFindBox Invisible", !GetControl("CustomsOfficeFindBox").Visible);
			declaration.CustomsOfficeRequirementHelper.SetMainOffice(new());
			control.HandleDeclarationControlVisibilityChanged();
			Assert("CustomsOfficeFindBox Visible", GetControl("CustomsOfficeFindBox").Visible);

			Assert("AdditionalRequiredOfficeFindBox Invisible", !GetControl("AdditionalRequiredOfficeFindBox").Visible);
			declaration.CustomsOfficeRequirementHelper.SetAdditionalOffice(new());
			control.HandleDeclarationControlVisibilityChanged();
			Assert("AdditionalRequiredOfficeFindBox Visible", GetControl("AdditionalRequiredOfficeFindBox").Visible);

			Assert("CustomsOffices Invisible", !GetControl("CustomsOfficesGrid").Visible);
			declaration.CustomsOfficeRequirementHelper.SetOtherRequirements([new()]);
			control.HandleDeclarationControlVisibilityChanged();
			Assert("CustomsOffices Visible", GetControl("CustomsOfficesGrid").Visible);

			Assert("PresentationStartDateEdit Invisible", !GetControl("PresentationStartDateEdit").Visible);
			declaration.CustomsOfficeRequirementHelper.SetIsPresentationStartDateEditVisible(true);
			control.HandleDeclarationControlVisibilityChanged();
			Assert("PresentationStartDateEdit Visible", GetControl("PresentationStartDateEdit").Visible);
		});
		return;

		Control GetControl(string key) => control.Controls.Find(key, true).Single();
	}

	public class JobDeclarationForCustomsOfficeRequirementTest(BusinessObjectFactory factory, DataRow row) : JobDeclaration(factory, row)
	{
		protected override JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new CustomsOfficeRequirementHelperForTest(this);

		public new CustomsOfficeRequirementHelperForTest CustomsOfficeRequirementHelper => (CustomsOfficeRequirementHelperForTest)base.CustomsOfficeRequirementHelper;

		protected override AutologState AutoLoggingState => AutologState.NotLogged;
	}

	public class CustomsOfficeRequirementHelperForTest(JobDeclarationForCustomsOfficeRequirementTest declaration) : Customs.PL.Business.JobDeclarationCustomsOfficeRequirementHelper(declaration)
	{
		public void SetMainOffice(CustomsOfficeRequirement value)
		{
			this.mainOffice = value;
		}

		protected override CustomsOfficeRequirement GetMainOffice() => mainOffice;
		CustomsOfficeRequirement mainOffice;

		public void SetAdditionalOffice(CustomsOfficeRequirement value)
		{
			additionalOffice = value;
		}

		public override CustomsOfficeRequirement AdditionalOffice => additionalOffice;
		CustomsOfficeRequirement additionalOffice;

		public void SetOtherRequirements(IEnumerable<CustomsOfficeRequirement> value)
		{
			otherRequirements = value;
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => otherRequirements;
		IEnumerable<CustomsOfficeRequirement> otherRequirements = [];

		public void SetIsPresentationStartDateEditVisible(bool value)
		{
			isPresentationStartDateEditVisible = value;
		}

		public override bool IsPresentationStartDateEditVisible => isPresentationStartDateEditVisible;
		bool isPresentationStartDateEditVisible;
	}
}
