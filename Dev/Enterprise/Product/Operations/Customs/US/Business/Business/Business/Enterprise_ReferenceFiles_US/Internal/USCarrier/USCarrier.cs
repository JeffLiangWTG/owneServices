using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Internal
{
	public class USCarrier : AutoUSCarrier
	{
		public USCarrier(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			USC_ModeOfTransportation = TransportModeCodes.Codes.VesselNonContainer;
		}
	}
}
