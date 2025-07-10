
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
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
			base.CheckCO_ContainerNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_ContainerNumberInfo, "Container Number");
		}

		protected override void CheckCO_FCL_LCL_AIR()
		{
			base.CheckCO_FCL_LCL_AIR();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_FCL_LCL_AIRInfo, "Container Mode");
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_FCL_LCL_AIRInfo, Parent.Lookups.CO_FCL_LCL_NCT_List, (NoResString)"Please enter a valid Container Mode");
		}

		protected override void CheckCO_Seal()
		{
		}

		protected override void CheckCO_ContainerSize()
		{
			base.CheckCO_ContainerSize();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_ContainerSizeInfo, "Container Size");
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_ContainerSizeInfo, Parent.Lookups.ContainerSizeList);

			if (Parent.CO_ContainerSize == "0")
			{
				Parent.CO_ContainerSizeInfo.AddMessageError("Please enter a Container Size");
			}
		}

		protected override void CheckCO_Weight()
		{
			base.CheckCO_Weight();
			if (Parent.CO_Weight <= 0)
			{
				Parent.CO_WeightInfo.AddWarning("Container Weight has not been entered.");
			}
		}

		protected override void CheckCO_WeightUQ()
		{
			base.CheckCO_WeightUQ();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_WeightUQInfo, "Container Weight UQ");
			ListValidation.MessageErrorIfInvalidCode(Parent.CO_WeightUQInfo, Parent.Lookups.WeightUnits, (NoResString)"Please enter a valid Container Weight UQ");
		}
	}
}
