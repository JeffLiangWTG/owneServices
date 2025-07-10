using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(GoodsRegistrationNumberUserControl))]
sealed class GoodsRegistrationNumberUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new GoodsRegistrationNumberUserControl();
		AssertEquals("SumARegisterDetailsHeaderUserControl test data source", typeof(CusTempStorageRegHeader), control.BindingSource.DataSourceType);
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new GoodsRegistrationNumberUserControl();
		_ = control.AssertContainsControl<ZButton>("GenerateGoodsNumberButton", x => x
			.WithCaption("Select goods number")
		);
		_ = control.AssertContainsControl<ZTextBox>("GoodsNumberTextBox", x => x
			.WithBindTo(nameof(CusTempStorageRegHeader.SRH_Reference))
		);
	});

	public void TestGenerateGoodsNumberButton_Click()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		using var form = new ZForm(header);
		using var control = new GoodsRegistrationNumberUserControl();
		form.Controls.Add(control);
		form.Show();

		var button = control.FindSingle<ZButton>("GenerateGoodsNumberButton");
		button.PerformClick();
		AssertType<GenerateGoodsRegistrationNumberForm>(ZFormModaliser.LastFormShownDialogForTest);
	}

	public void TestEnableAndDisableControls()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = ZString.Empty;

		using var form = new ZForm(header);
		using var control = new GoodsRegistrationNumberUserControl();
		form.Controls.Add(control);
		form.Show();

		var button = control.FindSingle<ZButton>("GenerateGoodsNumberButton");
		AssertEquals("When SRH_Reference is empty", true, button.Enabled);

		header.SRH_Reference = "NO1234";
		AssertEquals("When SRH_Reference is not empty", false, button.Enabled);
	}
}
