//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffUOMViewValidation
//
//    This class should be used for overriding validation in AutoTariffUOMViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class TariffUOMViewValidation : AutoTariffUOMViewValidation
	{
		public TariffUOMViewValidation(AutoTariffUOMView parent) : base(parent)
		{
		}

		protected new TariffUOMView Parent => (TariffUOMView)base.Parent;

		protected override void CheckZZ8_Type()
		{
			base.CheckZZ8_Type();

			var parent = Parent;
			if (!parent.ZZ8_IsSystem)
			{
				var type = parent.ZZ8_Type;
				var propertyInfo = parent.ZZ8_TypeInfo;
				var unitsOfMeasure = parent.CusTariff.UnitsOfMeasure;

				ListValidation.ErrorIfInvalidCode(propertyInfo);

				if (type == UOMTypeList.Codes.CU2 && !unitsOfMeasure.Any(x => x.ZZ8_Type == UOMTypeList.Codes.CU1))
				{
					propertyInfo.AddError(Res.GetString("CFCBA59B-B8A6-4639-9BE4-FC46FBBD4B79", "There has to be a CU1 UOM"));
				}
				else if (type == UOMTypeList.Codes.CU3 && !unitsOfMeasure.Any(x => x.ZZ8_Type == UOMTypeList.Codes.CU2))
				{
					propertyInfo.AddError(Res.GetString("903D7F88-0EBD-44D0-B668-09AC262B450C", "There has to be a CU2 UOM"));
				}
			}
		}

		protected override void CheckZZ8_UOM()
		{
			base.CheckZZ8_UOM();

			var parent = Parent;
			if (!parent.ZZ8_IsSystem)
			{
				var uom = parent.ZZ8_UOM;
				var propertyInfo = parent.ZZ8_UOMInfo;
				if (parent.Lookups.UOMList.Count > 0)
				{
					ListValidation.ErrorIfInvalidCode(propertyInfo);
				}

				if (!uom.IsEmpty && parent.CusTariff.UnitsOfMeasure.Any(x => x.ZZ8_UOM == uom && x.PK != parent.PK))
				{
					propertyInfo.AddError(Res.GetString("93B42727-D288-4B8C-AA35-7AB8F0615331", "Unit must be unique"));
				}
			}
		}
	}
}
