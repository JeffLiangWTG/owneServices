using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryInstructionDetailGuaranteesUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new EntryInstructionDetailGuaranteesUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull("GuaranteesGrid", control.FindSingleOrDefault<ZGrid>("GuaranteesGrid"));
			});
		}
	}

	public void TestColumns()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		using (var form = new ZForm(declaration))
		{
			using (var control = new ImportEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var guaranteesGrid = (ZGrid)form.Controls.Find("GuaranteesGrid", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("GuaranteesGrid should be visible", true, guaranteesGrid.Visible);

					AssertNotNull("PW_BondType", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_BondType));
					AssertNotNull("PW_BondNumber", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_BondNumber));
					AssertNotNull("PW_HolderIdentification", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_HolderIdentification));
					AssertNotNull("PW_Password", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_Password));
					AssertNotNull("PW_BondAmount", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_BondAmount));
					AssertNotNull("PW_RX_NKCurrency", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_RX_NKCurrency));

					AssertNotNull("PW_CPH_Guarantee", FindColumnByName(guaranteesGrid, GuaranteeBondDetail.Schema.PW_CPH_Guarantee));

					var bondTypeStyle = guaranteesGrid.GetColumnStyle(GuaranteeBondDetail.Schema.PW_BondType);
					Assert("bondTypeStyle", bondTypeStyle is ZDropEditColumnStyleInfo);
				});
			}
		}
	}

	public void TestColumnsEnabledDisabled()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entry = declaration.CustomsEntryInstructions.AddNew();
		var guarantee = entry.Guarantees.AddNew();

		using (var form = new ZForm(declaration))
		{
			using (var control = new ImportEntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var guaranteesGrid = (ZGrid)form.Controls.Find("GuaranteesGrid", true).Single();
				guaranteesGrid.Select(0);

				CombineAssertions(() =>
				{
					var row = (GuaranteeBondDetail)guaranteesGrid.GetFirstSelectedRow();
					AssertEquals("PW_BondType empty - PW_CPH_Guarantee", true, row.PW_CPH_GuaranteeInfo.ReadOnly);
					AssertEquals("PW_BondType empty - PW_BondNumber", false, row.PW_BondNumberInfo.ReadOnly);
					AssertEquals("PW_BondType empty - PW_HolderIdentification", true, row.PW_HolderIdentificationInfo.ReadOnly);
					AssertEquals("PW_BondType empty - PW_Password", true, row.PW_PasswordInfo.ReadOnly);

					guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Individual;
					row = (GuaranteeBondDetail)guaranteesGrid.GetFirstSelectedRow();
					AssertEquals("PW_BondType Individual - PW_CPH_Guarantee", true, row.PW_CPH_GuaranteeInfo.ReadOnly);
					AssertEquals("PW_BondType Individual - PW_BondNumber", false, row.PW_BondNumberInfo.ReadOnly);
					AssertEquals("PW_BondType Individual - PW_HolderIdentification", true, row.PW_HolderIdentificationInfo.ReadOnly);
					AssertEquals("PW_BondType Individual - PW_Password", true, row.PW_PasswordInfo.ReadOnly);

					guarantee.PW_BondType = GuaranteeBondTypeList.Codes.Comprehensive;
					row = (GuaranteeBondDetail)guaranteesGrid.GetFirstSelectedRow();
					AssertEquals("PW_BondType Comprehensive - PW_CPH_Guarantee", false, row.PW_CPH_GuaranteeInfo.ReadOnly);
					AssertEquals("PW_BondType Comprehensive - PW_BondNumber", true, row.PW_BondNumberInfo.ReadOnly);
					AssertEquals("PW_BondType Comprehensive - PW_HolderIdentification", true, row.PW_HolderIdentificationInfo.ReadOnly);
					AssertEquals("PW_BondType Comprehensive - PW_Password", true, row.PW_PasswordInfo.ReadOnly);
				});
			}
		}
	}

	ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
		=> grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
}
