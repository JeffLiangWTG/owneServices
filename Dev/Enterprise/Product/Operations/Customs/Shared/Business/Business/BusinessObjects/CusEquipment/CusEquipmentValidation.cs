//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEquipmentValidation
//
//    This class should be used for overriding validation in AutoCusEquipmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	using CargoWise.EntityFramework;

	public class CusEquipmentValidation : AutoCusEquipmentValidation
	{
		public CusEquipmentValidation(AutoCusEquipment parent) : base(parent)
		{
		}

		protected new CusEquipment Parent => (CusEquipment)base.Parent;

		protected override void CheckCEQ_IdentificationNumber()
		{
			base.CheckCEQ_IdentificationNumber();
			if (Parent.CEQ_IdentificationNumber.IsEmpty && (Parent.Declaration?.EquipmentsRequired ?? false))
			{
				var info = Parent.CEQ_IdentificationNumberInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
		}
	}
}
