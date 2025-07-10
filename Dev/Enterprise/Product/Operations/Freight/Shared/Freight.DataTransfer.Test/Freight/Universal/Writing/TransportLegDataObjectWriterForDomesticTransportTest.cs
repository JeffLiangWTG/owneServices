using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class TransportLegDataObjectWriterForDomesticTransportTest : TransportLegDataObjectWriterTest
	{
		IDtbBookingConsolidation consol;

		protected override Transport GetTransport()
		{
			consol = Factory.New<IDtbBookingConsolidation>();
			return new TransportCollection((ITransportParentCommon)consol).AddNew();
		}

		public new void TestGreenhouseGasEmission()
		{
			var transportBO = GetTransport();
			SetupTransportLeg(transportBO, false);
			transportBO.SetCO2ePerTonneInKg(1);
			transportBO.SetCO2eDistanceInKM(2);
			transportBO.SetCO2ePerTEUInKg(3);
			transportBO.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var writer = new TransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, transportBO)), consol as BusinessObject);
			var transportLeg = writer.GetDataObject(transportBO);

			AssertNull("transportLeg.GreenhouseGasEmission", transportLeg.GreenhouseGasEmission);
		}
	}
}
