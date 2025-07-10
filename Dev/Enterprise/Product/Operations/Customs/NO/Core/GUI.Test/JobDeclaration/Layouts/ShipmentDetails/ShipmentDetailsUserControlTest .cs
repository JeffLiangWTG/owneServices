using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class ShipmentDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new ShipmentDetailsLayoutsUserControl())
			{
				AssertEquals("ShipmentDetailsUserControl test data source", typeof(JobDeclaration), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new ShipmentDetailsLayoutsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Destination - Visible", true, control.ShipmentDetailsFinalDestinationUserControl.Visible);
					AssertEquals("Origin - Visible", true, control.ShipmentDetailsOriginUserControl.Visible);
					AssertEquals("GoodsLocation - Visible", true, control.ShipmentDetailsGoodsLocationUserControl.Visible);
					AssertEquals("Weight and Volume - Visible", true, control.ShipmentDetailsWeightAndVolumeUserControl.Visible);
				});
			}
		}

		public void TestNoOfUnits()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = new ShipmentDetailsLayouts().Layout;
			using (var control = new Customs.GUI.ShipmentDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					AssertEquals("Number of pieces", true, layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, declaration));

					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
					AssertEquals("Number of pieces", true, layout.IsVisible(Customs.GUI.ShipmentDetailsControlBag.Instance.TotalNoOfPiecesCalcEdit, declaration));
				});
			}
		}
	}
}
