using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(VehicleRegistrationAndNationalityUserControl))]
sealed class VehicleRegistrationAndNationalityUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new VehicleRegistrationAndNationalityUserControl();
		AssertEquals("NOVehicleRegistrationAndNationalityUserControl test data source", typeof(ASYCUDA.Business.AsycudaManifestHeader), control.BindingSource.DataSourceType);
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new VehicleRegistrationAndNationalityUserControl();
		_ = control.AssertThisControl(x => x
			.WithCaptionRenderingEnabled(true)
		);
		_ = control.AssertContainsControl<ZTextBox>("VehicleRegistrationTextBox", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.AMA_VehicleRegistration))
		);
		_ = control.AssertContainsControl<ZCodeFindBox>("VehicleNationalityCodeFindBox", x => x
			.WithBindTo(nameof(AsycudaManifestHeader.AMA_RN_NKConveyanceNationality))
		);
	});
}
