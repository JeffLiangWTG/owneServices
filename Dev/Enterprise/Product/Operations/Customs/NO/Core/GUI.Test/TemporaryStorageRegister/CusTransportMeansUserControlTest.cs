using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(CusTransportMeansUserControl))]
sealed class CusTransportMeansUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new CusTransportMeansUserControl();
		AssertEquals("CusTransportMeansUserControl test data source", typeof(CusTransportMeans), control.BindingSource.DataSourceType);
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new CusTransportMeansUserControl();
		_ = control.AssertContainsControl<ZCodeFindBox>("NationalityCodeFindBox", x => x
			.WithBindTo(nameof(CusTransportMeans.TPM_RN_NKTransportNationality))
			.WithShowDescriptionBox(false)
		);
		_ = control.AssertContainsControl<ZTextBox>("TransportIDTextBox", x => x
			.WithBindTo(nameof(CusTransportMeans.TPM_IdentificationNumber))
			.WithCharacterCasing(CharacterCasing.Normal)
		);
	});
}
