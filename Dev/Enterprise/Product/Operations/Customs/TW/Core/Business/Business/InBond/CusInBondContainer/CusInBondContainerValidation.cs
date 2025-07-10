using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondContainerValidation : AutoTWCusInBondContainerValidation
	{
		public CusInBondContainerValidation(CusInBondContainer parent)
			: base(parent)
		{
		}

		protected new CusInBondContainer Parent => (CusInBondContainer)base.Parent;

		protected override void CheckBC_ContainerNum()
		{
			base.CheckBC_ContainerNum();

			var targetInfo = Parent.BC_ContainerNumInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			var containerNumber = Parent.BC_ContainerNum;
			var containerPK = Parent.PK;
			if (!containerNumber.IsEmpty)
			{
				if (Parent.MoveHeader?.InBondMoveDetail?.Containers?.Any(x => x.BC_ContainerNum == containerNumber && x.PK != containerPK) ?? false)
				{
					targetInfo.AddMessageError(ValidationConstants.CusInBondContainer.ContainerNumberAlreadyExists);
				}
				if (!containerNumber.IsLettersAndNumbersOnlyOrEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.CusInBondContainer.ContainerNumberAlphanumericCharactersOnly);
				}
				else
				{
					ContainerNumberValidation.WarnIfInvalid(Parent.BC_ContainerNumInfo);
				}
			}
		}

		protected override void CheckBC_Seal1()
		{
			base.CheckBC_Seal1();

			var seal = Parent.BC_Seal1;
			if (!seal.IsEmpty && !seal.IsLettersAndNumbersOnlyOrEmpty)
			{
				var targetInfo = Parent.BC_Seal1Info;
				targetInfo.AddMessageError(ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckBC_RC()
		{
			base.CheckBC_RC();

			var targetInfo = Parent.BC_RCInfo;
			ListValidation.ErrorIfInvalidPK(targetInfo);
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
		}

		protected override void CheckBC_Mode()
		{
			base.CheckBC_Mode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BC_ModeInfo, Parent.Lookups.ModeList);
		}
	}
}
