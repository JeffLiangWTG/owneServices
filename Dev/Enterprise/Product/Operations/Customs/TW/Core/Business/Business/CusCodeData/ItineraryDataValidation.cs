using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ItineraryDataValidation : CusCodeDataValidation
	{
		public ItineraryDataValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected new ItineraryData Parent => (ItineraryData)base.Parent;

		protected override void CheckCY_CodeList()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);
		}
	}
}
