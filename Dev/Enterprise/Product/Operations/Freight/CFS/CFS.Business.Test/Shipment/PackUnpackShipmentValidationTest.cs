using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class PackUnpackShipmentValidationTest : TestCaseWithFactory
	{
		#region TestValidateJS_ActualVolume

		public void TestValidateJS_ActualVolume()
		{
			PackLine pack = Shipment.OuterPackLines.AddNew();
			//Pack.JL_ActualVolume = 1;
			Shipment.JS_ActualVolume = 1;

			Shipment.Validation.ValidateJS_ActualVolume();
			Assert("default values are equal", !Shipment.JS_ActualVolumeInfo.HasWarnings());

			Shipment.JS_ActualVolume++;
			Assert("default values are equal", !Shipment.JS_ActualVolumeInfo.HasWarnings());
		}

		#endregion

		#region TestValidateJS_ActualWeight2

		public void TestValidateJS_ActualWeight2()
		{
			PackLine pack = Shipment.OuterPackLines.AddNew();
			//Pack.JL_ActualWeight = 1;
			Shipment.JS_ActualWeight = 1;

			Shipment.Validation.ValidateJS_ActualWeight();
			Assert("equal", !Shipment.JS_ActualWeightInfo.HasWarnings());

			Shipment.JS_ActualWeight++;
			Assert("equal", !Shipment.JS_ActualWeightInfo.HasWarnings());
		}

		#endregion

		#region TestValidateJS_OuterPackPackType

		public void TestValidateJS_OuterPackPackType()
		{
			Shipment.JS_F3_NKPackType = string.Empty;
			Assert(Shipment.JS_F3_NKPackTypeInfo.HasErrors());

			Shipment.JS_F3_NKPackType = "CTN";
			Assert(!Shipment.JS_F3_NKPackTypeInfo.HasErrors());

			Shipment.JS_F3_NKPackType = "OOO";
			Assert(Shipment.JS_F3_NKPackTypeInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_UnitOfVolume

		public void TestValidateJS_UnitOfVolume()
		{
			Shipment.JS_UnitOfVolume = string.Empty;
			Assert(Shipment.JS_UnitOfVolumeInfo.HasErrors());

			Shipment.JS_UnitOfVolume = "M3";
			Assert(!Shipment.JS_UnitOfVolumeInfo.HasErrors());

			Shipment.JS_UnitOfVolume = "OO";
			Assert(Shipment.JS_UnitOfVolumeInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_UnitOfWeight

		public void TestValidateJS_UnitOfWeight()
		{
			Shipment.JS_UnitOfWeight = string.Empty;
			//Assert(Shipment.JS_UnitOfWeightInfo.HasErrors());

			Shipment.JS_UnitOfWeight = "KG";
			Assert(!Shipment.JS_UnitOfWeightInfo.HasErrors());

			Shipment.JS_UnitOfWeight = "OO";
			Assert(Shipment.JS_UnitOfWeightInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_ActualWeight

		public void TestValidateJS_ActualWeight()
		{
			Shipment.JS_ActualWeight = (ZDecimal)1.00;
			Shipment.JS_ActualWeight = (ZDecimal)0.00;
			Assert(Shipment.JS_ActualWeightInfo.HasWarnings());

			Shipment.JS_ActualWeight = (ZDecimal)(-1.00);
			Assert(Shipment.JS_ActualWeightInfo.HasErrors());

			Shipment.JS_ActualWeight = (ZDecimal)1.00;
			Assert(!Shipment.JS_ActualWeightInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_MarksAndNumbers

		public void TestValidateJS_MarksAndNumbers()
		{
			Shipment.JS_OuterPacks = 0;
			Shipment.JS_MarksAndNumbers = "Marks And Numbers";
			AssertNoErrors("JS_MarksAndNumbersInfo.HasErrors()", Shipment.JS_MarksAndNumbersInfo);

			Shipment.JS_MarksAndNumbers = "";
			AssertEquals("JS_MarksAndNumbersInfo.HasErrors()", true, Shipment.JS_MarksAndNumbersInfo.HasErrors());

			Shipment.JS_OuterPacks = 1;
			Shipment.JS_MarksAndNumbers = "Marks And Numbers";
			AssertNoErrors("JS_MarksAndNumbersInfo.HasErrors()", Shipment.JS_MarksAndNumbersInfo);

			Shipment.JS_MarksAndNumbers = "";
			AssertNoErrors("JS_MarksAndNumbersInfo.HasErrors()", Shipment.JS_MarksAndNumbersInfo);
		}

		#endregion

		#region PackUnpackShipment

		public void TestPackUnpackShipment()
		{
			Shipment.OuterPackLines.AddNew().JL_PackageCount = 5;
			Shipment.JS_OuterPacks = 5;
			AssertNoWarnings(Shipment.JS_OuterPacksInfo);
			Shipment.AllowSurplusPacks = false;
			Shipment.JS_OuterPacks = 0;
			AssertNoErrors(Shipment.JS_OuterPacksInfo);
			AssertHasWarningContaining(Shipment.JS_OuterPacksInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region Imlementation

		protected PackUnpackShipment Shipment;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<PackUnpackShipment>();
		}

		#endregion
	}
}
