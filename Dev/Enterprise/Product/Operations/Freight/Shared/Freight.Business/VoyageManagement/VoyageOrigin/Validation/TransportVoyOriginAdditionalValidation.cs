using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class TransportVoyOriginAdditionalValidation : JobVoyOriginValidation
	{
		public TransportVoyOriginAdditionalValidation(AutoJobVoyOrigin parent, Transport transport)
			: base(parent)
		{
			this.transport = transport;
		}

		readonly Transport transport;

		protected override void CheckJA_CutOff()
		{
			base.CheckJA_CutOff();
			if (transport != null && !transport.IsDeleted)
			{
				ConsolTransportValidationHelper.CheckTerminalCutOff(Parent.JA_CutOffInfo, transport);
			}
		}

		protected override void CheckJA_RL_NKPortOfLoading()
		{
			base.CheckJA_RL_NKPortOfLoading();
			if (transport != null && !transport.IsDeleted && transport.JW_IsLinked)
			{
				ConsolTransportValidationHelper.CheckLoadPort(Parent.JA_RL_NKPortOfLoadingInfo, transport);
			}
		}

		protected override void CheckJA_E_DEP()
		{
			base.CheckJA_E_DEP();
			if (transport != null && !transport.IsDeleted && transport.JW_IsLinked)
			{
				ConsolTransportValidationHelper.CheckETD(Parent.JA_E_DEPInfo, transport);
			}
		}

		protected override void CheckJA_A_DEP()
		{
			base.CheckJA_A_DEP();
			if (transport != null && !transport.IsDeleted && transport.JW_IsLinked)
			{
				ConsolTransportValidationHelper.CheckATD(Parent.JA_A_DEPInfo, transport);
			}
		}
	}
}
