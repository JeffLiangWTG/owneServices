using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusContainerValidation : AutoTWCusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		public JobDeclaration JobDeclaration
		{
			get { return Parent?.Declaration; }
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			if (JobDeclaration != null && JobDeclaration.JE_TransportMode == TransportTypeList.Codes.Sea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CO_FCL_LCL_AIRInfo, Parent.Lookups.CO_FCL_LCL_NCT_List);
			}
		}

		protected override void CheckCO_Seal()
		{
			base.CheckCO_Seal();

			if (Parent.CO_Seal.Length > 17)
			{
				Parent.CO_SealInfo.AddMessageError(Res.GetString("D89A9E01-FA0D-4EA8-8F73-1F451C9B0A07", "The length of Seal Number shouldn't be more than 17."));
			}
		}
	}
}
