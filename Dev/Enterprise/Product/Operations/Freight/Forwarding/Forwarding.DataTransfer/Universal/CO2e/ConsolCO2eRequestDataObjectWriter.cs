using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public sealed class ConsolCO2eRequestDataObjectWriter : CO2eLegBasedRequestDataObjectWriter<ForwardingConsol, Transport>
	{
		public ConsolCO2eRequestDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		public ConsolCO2eRequestDataObjectWriter(IDataWritingManager manager, ICO2eCalculationSupporter hostSupporter) : base(manager, hostSupporter)
		{
		}

		protected override bool UseRoadForFirstAndLastLegs => false;

		protected override DataObjectWriter<Transport, TransportLeg> GetLegDataObjectWriter()
		{
			return new CO2eTransportLegDataObjectWriter(writeManager);
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingConsol;
		}
	}
}
