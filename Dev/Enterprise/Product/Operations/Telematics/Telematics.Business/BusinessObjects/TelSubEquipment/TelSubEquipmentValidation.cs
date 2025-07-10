using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class TelSubEquipmentValidation : AutoTelSubEquipmentValidation
	{
		public TelSubEquipmentValidation(AutoTelSubEquipment parent) : base(parent)
		{
		}

		protected override void CheckTSE_Type()
		{
			base.CheckTSE_Type();
			if (!Parent.Lookups.TelSubEquipmentTypeList.ContainsCode(Parent.TSE_Type))
			{
				ListValidation.ErrorIfInvalidCode(Parent.TSE_TypeInfo);
			}
		}
	}
}
