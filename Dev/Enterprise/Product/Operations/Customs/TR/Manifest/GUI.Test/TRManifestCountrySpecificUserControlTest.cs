using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	public class TRManifestCountrySpecificUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(AsycudaManifestHeader), control.BindingSource.DataSourceType);
		}

		public void TestRegistrationDateEdit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Long, control.RegistrationDateEdit.DateTimeFormat);
				AssertType<ZDateEdit>("Type", control.RegistrationDateEdit);
			});
		}

		public void TestTransportTypeDropEdit()
		{
			AssertType<ZDropEdit>("Type", control.TransportTypeDropEdit);
		}

		public void TestTempStorageDueDateEdit()
		{
			AssertType<ZDateEdit>("Type", control.TempStorageDueDateEdit);
		}

		public void TestTempStorageStartDateEdit()
		{
			AssertType<ZDateEdit>("Type", control.TempStorageStartDateEdit);
		}

		public void TestInspectionClerkTextBox()
		{
			AssertType<ZTextBox>("Type", control.InspectionClerkTextBox);
		}

		public void TestInternalInspectionNoTextBox()
		{
			AssertType<ZTextBox>("Type", control.InternalInspectionNoTextBox);
		}

		public void TestManifestDescriptionTextBox()
		{
			AssertType<ZTextBox>("Type", control.ManifestDescriptionTextBox);
		}

		public void TestTIRNumberTextBox()
		{
			AssertType<ZTextBox>("Type", control.TIRNumberTextBox);
		}

		public void TestPresentationCustomsOfficeDropEdit()
		{
			AssertType<ZDropEdit>("Type", control.PresentationCustomsOfficeDropEdit);
		}

		public void TestGlobalManifestStampDutyValueCalcEdit()
		{
			AssertType<ZCalcEdit>("Type", control.GlobalManifestStampDutyValueCalcEdit);
		}

		public void TestMasterBillStampDutyValueCalcEdit()
		{
			AssertType<ZCalcEdit>("Type", control.MasterBillStampDutyValueCalcEdit);
		}

		public void TestTotalStampDutyValueCalcEdit()
		{
			AssertType<ZCalcEdit>("Type", control.TotalStampDutyValueCalcEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TRManifestCountrySpecificUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TRManifestCountrySpecificUserControl control;
	}
}
