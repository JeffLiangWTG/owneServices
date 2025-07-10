using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class TransportSailingAdditionalValidation : JobSailingValidation
	{
		public TransportSailingAdditionalValidation(JobSailing parent, Transport transport)
			: base(parent)
		{
			this.transport = transport;
		}

		readonly Transport transport;

		protected override void CheckJX_DepotCutOff()
		{
			base.CheckJX_DepotCutOff();
			if (transport != null && !transport.IsDeleted)
			{
				ConsolTransportValidationHelper.CheckDepotCutOff(Parent.JX_DepotCutOffInfo, transport);
			}
		}
	}
}
