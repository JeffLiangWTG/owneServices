using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class GovernmentProcedureWrapper : IGovernmentProcedure
	{
		public GovernmentProcedureWrapper(ZString currentCode)
			: this(currentCode, ZString.Empty)
		{
		}

		public GovernmentProcedureWrapper(ZString currentCode, ZString transportTypeCode)
			: this(currentCode, transportTypeCode, ZString.Empty)
		{
		}

		public GovernmentProcedureWrapper(ZString currentCode, ZString transportTypeCode, ZString description)
		{
			this.currentCode = currentCode;
			this.transportTypeCode = transportTypeCode;
			this.description = description;
		}

		readonly ZString currentCode;
		readonly ZString transportTypeCode;
		readonly ZString description;

		ZString IGovernmentProcedure.CurrentCode => currentCode;

		ZString IGovernmentProcedure.TransportTypeCode => transportTypeCode;

		ZString IGovernmentProcedure.Description => description;
	}
}
