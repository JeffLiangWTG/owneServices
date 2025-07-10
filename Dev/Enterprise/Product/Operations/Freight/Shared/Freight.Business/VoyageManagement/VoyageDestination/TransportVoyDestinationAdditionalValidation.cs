using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class TransportVoyDestinationAdditionalValidation : JobVoyDestinationValidation
	{
		public TransportVoyDestinationAdditionalValidation(AutoJobVoyDestination parent, Transport transport)
			: base(parent)
		{
			this.transport = transport;
		}

		readonly Transport transport;

		protected override void CheckJB_RL_NKPortOfDischarge()
		{
			base.CheckJB_RL_NKPortOfDischarge();
			if (transport != null && !transport.IsDeleted && transport.JW_IsLinked)
			{
				ConsolTransportValidationHelper.CheckDiscPort(Parent.JB_RL_NKPortOfDischargeInfo, transport);
			}
		}

		protected override void CheckJB_E_ARV()
		{
			base.CheckJB_E_ARV();
			if (transport != null && !transport.IsDeleted && transport.JW_IsLinked)
			{
				ConsolTransportValidationHelper.CheckETA(Parent.JB_E_ARVInfo, transport);
			}
		}

		protected override void CheckJB_A_ARV()
		{
			base.CheckJB_A_ARV();
			if (transport != null && !transport.IsDeleted && transport.JW_IsLinked)
			{
				ConsolTransportValidationHelper.CheckATA(Parent.JB_A_ARVInfo, transport);
			}
		}
	}
}
