using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallDivot : AutoCarrierVoyagePortCallDivot
	{
		public CarrierVoyagePortCallDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CarrierVoyage Voyage => Factory.Load<CarrierVoyage>(CVP_CVO_Voyage);

		[RelatedBusinessObject(nameof(Voyage))]
		public override ZGuid CVP_CVO_Voyage
		{
			get => base.CVP_CVO_Voyage;
			set => base.CVP_CVO_Voyage = value;
		}

		public CarrierVoyagePortCall PortCall => Factory.Load<CarrierVoyagePortCall>(CVP_CPO_PortCall);

		[RelatedBusinessObject(nameof(PortCall))]
		public override ZGuid CVP_CPO_PortCall
		{
			get => base.CVP_CPO_PortCall;
			set => base.CVP_CPO_PortCall = value;
		}
	}
}
