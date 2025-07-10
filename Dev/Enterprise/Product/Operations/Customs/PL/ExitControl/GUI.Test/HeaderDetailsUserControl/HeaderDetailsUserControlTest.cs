using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.ExitControl.GUI.Testing;

sealed class HeaderDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(CusExitHeader), userControl.BindingSource.DataSourceType);
	}

	public void TestBrokerCodeFindBox()
	{
		var brokerCodeFindBox = userControl.FindSingleOrDefault<ZCodeFindBox>("BrokerCodeFindBox");
		CombineAssertions(() =>
		{
			AssertType<ZCodeFindBox>("Type", brokerCodeFindBox);
			AssertEquals("Caption", "Broker", brokerCodeFindBox.CaptionResourceString.Caption);
			AssertEquals("ShortCaption", "Broker", brokerCodeFindBox.CaptionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "Broker", brokerCodeFindBox.CaptionResourceString.MediumCaption);
			AssertEquals("FullDescription", "The broker selected will be the responsible of declarations to Customs in this Job", brokerCodeFindBox.CaptionResourceString.FullDescription);
			AssertEquals("BindTo", nameof(CusExitHeader.CXH_GS_NKCustomsAgent), brokerCodeFindBox.BindTo);
		});
	}

	public void TestCertificateDropEdit()
	{
		var certificateDropEdit = userControl.FindSingleOrDefault<ZDropEdit>("CertificateDropEdit");
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", certificateDropEdit);
			AssertEquals("Caption", "Certificate", certificateDropEdit.CaptionResourceString.Caption);
			AssertEquals("ShortCaption", "Cert.", certificateDropEdit.CaptionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "Certif.", certificateDropEdit.CaptionResourceString.MediumCaption);
			AssertEquals("FullDescription", "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job", certificateDropEdit.CaptionResourceString.FullDescription);
			AssertEquals("BindTo", nameof(CusExitHeader.CXH_CustomsProfile), certificateDropEdit.BindTo);
		});
	}

	public void TestTrainingCheckBox()
	{
		var trainingCheckBox = userControl.FindSingleOrDefault<ZCheckBox>("TrainingCheckBox");
		CombineAssertions(() =>
		{
			AssertType<ZCheckBox>("Type", trainingCheckBox);
			AssertEquals("Caption", "Training Entry", trainingCheckBox.CaptionResourceString.Caption);
			AssertEquals("ShortCaption", "Training Entry", trainingCheckBox.CaptionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "Training Entry", trainingCheckBox.CaptionResourceString.MediumCaption);
			AssertEquals("FullDescription", "When checked the declaration will be sent to Test", trainingCheckBox.CaptionResourceString.FullDescription);
			AssertEquals("BindTo", nameof(CusExitHeader.TrainingEntry), trainingCheckBox.BindTo);
		});
	}

	public void TestStoringFlagCheckBox()
	{
		var storingFlagCheckBox = userControl.FindSingleOrDefault<ZCheckBox>("StoringFlagCheckBox");
		CombineAssertions(() =>
		{
			AssertType<ZCheckBox>("Type", storingFlagCheckBox);
			AssertEquals("Caption", "Storing Flag", storingFlagCheckBox.CaptionResourceString.Caption);
			AssertEquals("BindTo", nameof(CusExitHeader.StoringFlag), storingFlagCheckBox.BindTo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new HeaderDetailsUserControl();
	}
	HeaderDetailsUserControl userControl;

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}
}
