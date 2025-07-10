using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationValidation_IPT : JobDeclarationValidation_Inward
	{
		public JobDeclarationValidation_IPT(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override ICodeDescriptionPairList MessageSubTypeList
		{
			get { return Parent.Lookups.IPTMessageSubTypeList; }
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (!ValidationHelper.IsNonAdValoremDutyRatedGoods(Parent))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportModeInfo, "Inward Transport Mode");
			}
		}

		protected override bool IsInwardCarrierAgentMandatory => Parent.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKT && TransportModeCodeList.TransportIsSeaOrAir(Parent.JE_TransportMode);
	}
}
