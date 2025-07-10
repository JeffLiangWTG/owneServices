using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	class BillValidator : Customs.Business.BillValidator
	{
		protected override ZString SpecialFlightTerm => "VARIOUS";
	}
}
