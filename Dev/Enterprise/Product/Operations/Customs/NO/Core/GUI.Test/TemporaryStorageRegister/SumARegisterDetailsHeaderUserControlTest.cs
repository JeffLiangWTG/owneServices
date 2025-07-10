using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(SumARegisterDetailsHeaderUserControl))]
sealed class SumARegisterDetailsHeaderUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new SumARegisterDetailsHeaderUserControl();
		AssertEquals("SumARegisterDetailsHeaderUserControl test data source", typeof(CusTempStorageRegHeader), control.BindingSource.DataSourceType);
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new SumARegisterDetailsHeaderUserControl();
		_ = control.AssertContainsControl<CusTransportMeansUserControl>("TrasportMeansUserControl", x => x
			.WithBindTo(nameof(CusTempStorageRegHeader.TransportMeans))
		);
		_ = control.AssertContainsControl<ZLabel>("UnloadingRemarksLabel", x => x
			.WithCaption("Unloading Remarks")
		);
		_ = control.AssertContainsControl<ZTextBox>("UnloadingRemarksTextBox", x => x
			.WithBindTo(nameof(CusTempStorageRegHeader.UnloadingRemarks))
			.WithMultiline(true)
		);
		_ = control.AssertContainsControl<GoodsRegistrationNumberUserControl>("GoodsNumberUserControl", x => x
			.WithBindTo(".")
		);
	});
}
