//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusDecHouseContainerPivotValidation
//
//    This class should be used for overriding validation in AutoCusDecHouseContainerPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusDecHouseContainerPivotValidation : AutoCusDecHouseContainerPivotValidation
	{
		public CusDecHouseContainerPivotValidation(AutoCusDecHouseContainerPivot parent)
			: base(parent)
		{
		}

		protected new BasePackingGroup Parent
		{
			get { return (BasePackingGroup)base.Parent; }
		}

		protected override void CheckCR_CEQ_Equipment()
		{
			base.CheckCR_CEQ_Equipment();
			if (Parent.CR_CEQ_Equipment.IsValid && Parent.CR_CO_Container.IsValid)
			{
				Parent.CR_CEQ_EquipmentInfo.AddError(EitherEquipmentOrContainer);
			}
		}

		protected override void CheckCR_CO_Container()
		{
			base.CheckCR_CO_Container();
			if (Parent.Declaration != null && !Parent.Declaration.ContainersRequired && !Parent.CR_CO_Container.IsEmpty)
			{
				Parent.CR_CO_ContainerInfo.AddMessageError(ContainerIsNotRequired);
			}
		}

		public static string ContainerIsNotRequired => Res.GetString("6df1a807-77c2-46ce-9346-3251ef1e1656", "Container is not required.");

		public static string EitherEquipmentOrContainer => Res.GetString("028BA3A1-9514-4E2A-8EBE-F77E19D613FB", "Please select either equipment or container but not both.");
	}
}
