using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		protected override void CheckCO_ContainerNumber()
		{
			if (!IsValidContainerNumberForZA(Parent.CO_ContainerNumber))
			{
				Parent.CO_ContainerNumberInfo.AddWarning(ValidationConstants.Containers.ContainerNotISO);
			}

			base.CheckCO_ContainerNumber();
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				if (declaration.IsImport)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CO_FCL_LCL_AIRInfo, Parent.Lookups.CO_FCL_LCL_NCT_List);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.CO_FCL_LCL_AIRInfo, Parent.Lookups.CO_FCL_LCL_NCT_List);
				}
				if (declaration.JE_ContainerMode == Core.Constants.ContainerModes.Containerised)
				{
					var currentContainerMode = Parent.CO_FCL_LCL_AIR;
					if (currentContainerMode != Core.Constants.ContainerModes.Empty && declaration.CusContainers
						.Cast<CusContainer>().Any(x => x.PK != Parent.PK && !x.CO_FCL_LCL_AIR.IsEmpty && x.CO_FCL_LCL_AIR != Constants.ContainerModes.Empty && x.CO_FCL_LCL_AIR != currentContainerMode))
					{
						Parent.CO_FCL_LCL_AIRInfo.AddError("Modes for all containers need to match, or be EMP");
					}
				}
			}
		}

		const char ValidFourthContainerCharacter = 'U';

		public static bool IsValidContainerNumberForZA(ZString containerNumber)
		{
			return ContainerNumberValidation.IsValidContainerNumber(containerNumber) && containerNumber[3] == ValidFourthContainerCharacter;
		}
	}
}
