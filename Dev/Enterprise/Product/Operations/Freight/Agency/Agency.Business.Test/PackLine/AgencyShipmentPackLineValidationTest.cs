using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentPackLineValidationTest : BaseFreightTest
	{
		public void TestValidateJL_JC()
		{
			const string invalid = "Enter a valid Container.";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AgencyShipmentContainer bookedContainer = shipment.BookedContainers.AddNew();
			AgencyShipmentContainer realContainer = shipment.RealContainers.AddNew();
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = ZGuid.Invalid;
			AssertHasError(packline.JL_JCInfo, invalid);
			packline.JL_JC = bookedContainer.PK;
			AssertNoNotifications(packline.JL_JCInfo);
		}

		public void TestValidateJL_PackageCount()
		{
			const string error = "Package count must be greater than 0";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			AssertNoError("JL_PackageCountInfo should has NO errors", packline.JL_PackageCountInfo, error);
			packline.JL_PackageCount = -8;
			AssertHasError("JL_PackageCountInfo should has error", packline.JL_PackageCountInfo, error);
			packline.JL_PackageCount = 0;
			AssertHasError("JL_PackageCountInfo should has error", packline.JL_PackageCountInfo, error);
			packline.JL_PackageCount = 5;
			AssertNoError("JL_PackageCountInfo should has NO errors", packline.JL_PackageCountInfo, error);
		}

		#region JL_HarmonisedCode
		public void TestCheckJL_HarmonisedCode()
		{
			var errorMessage = "Invalid Harmonized Code. Only numeric characters and dots are allowed.";
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "123ABC";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);
			packLine.JL_HarmonisedCode = "123A$#";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);
			packLine.JL_HarmonisedCode = "......";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);
			packLine.JL_HarmonisedCode = ".123";
			AssertHasError(packLine.JL_HarmonisedCodeInfo, errorMessage);
			packLine.JL_HarmonisedCode = string.Empty;
			AssertNoErrors(packLine.JL_HarmonisedCodeInfo);
			packLine.JL_HarmonisedCode = "13.3.1";
			AssertNoErrors(packLine.JL_HarmonisedCodeInfo);
			var dbPackline = shipment.OuterPackLines.AddNew();
			using (dbPackline.SuspendValidationTesting())
			{
				dbPackline.JL_HarmonisedCode = "123ABC";
				Factory.Save();
			}

			dbPackline.Validation.ValidateJL_HarmonisedCode();
			AssertHasWarning(dbPackline.JL_HarmonisedCodeInfo, errorMessage);
		}
		#endregion
	}
}
