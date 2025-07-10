using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(GenerateGoodsRegistrationNumberForm))]
sealed class GenerateGoodsRegistrationNumberFormTest : ZFormBasherTest
{
	public void TestFormProperties()
	{
		using var form = new GenerateGoodsRegistrationNumberForm(goodsNumberGenerator);
		CombineAssertions(() =>
		{
			AssertEquals("FormHeading", "Create Goods Number for given Date and Customs Warehouse ID", form.FormHeading);
			AssertEquals("FormBorderStyle", FormBorderStyle.FixedDialog, form.FormBorderStyle);
		});
	}

	public void TestCancelFormButton_Click()
	{
		using var form = new GenerateGoodsRegistrationNumberForm(goodsNumberGenerator);
		form.Show();
		form.CancelButton.PerformClick();

		CombineAssertions(() =>
		{
			AssertEquals("Visible", false, form.Visible);
			AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
		});
	}

	public void TestCreateNextFormButton_Click()
	{
		goodsNumberGenerator.WarehouseAuthorisationId = "09123";
		goodsNumberGenerator.GoodsRegistrationDate = new(2024, 08, 23);

		using var form = new GenerateGoodsRegistrationNumberForm(goodsNumberGenerator);
		form.Show();
		form.CreateNextButton.PerformClick();

		CombineAssertions(() =>
		{
			AssertEquals("SRH_Reference", "2024082309123001", header.SRH_Reference);
			AssertEquals("Visible", false, form.Visible);
			AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
		});
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var form = new GenerateGoodsRegistrationNumberForm(goodsNumberGenerator);
		AssertEquals(nameof(GoodsRegistrationNumberGeneratorObject.GoodsRegistrationDate), form.GoodsRegistrationZDateEdit.BindTo);
		AssertEquals(nameof(GoodsRegistrationNumberGeneratorObject.WarehouseAuthorisationId), form.WarehouseAuthorizationZDropEdit.BindTo);
	});

	protected override Form GetFormToBashCore() => new GenerateGoodsRegistrationNumberForm(goodsNumberGenerator);

	protected override bool ShouldTestFormIsFullyTranslatable => false;

	protected override void SetUp()
	{
		base.SetUp();
		CreateOrgHeaderWithAuthorization();
		header = Factory.New<CusTempStorageRegHeader>();
		goodsNumberGenerator = new GoodsRegistrationNumberGeneratorObject(header, Factory);
	}
	CusTempStorageRegHeader header;
	GoodsRegistrationNumberGeneratorObject goodsNumberGenerator;

	void CreateOrgHeaderWithAuthorization()
	{
		var factory = Factory;
		var declarant = factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "CH123";
		var appliesTo = factory.NewWithValidTestData<OrgHeader>();
		appliesTo.OH_Code = "APT01";

		var authorization = factory.New<Customs.Business.CusAuthorisationHeader>();
		authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
		authorization.CPH_OH_PermitHolder = declarant.PK;
		authorization.CPH_Number = "09123";
		authorization.CPH_PermitDescription = "DESC123";
		authorization.CPH_OA_AppliesTo = appliesTo.MainAddress.PK;
		authorization.CPH_StartDate = ZDate.Today.AddDays(-10);
	}
}
