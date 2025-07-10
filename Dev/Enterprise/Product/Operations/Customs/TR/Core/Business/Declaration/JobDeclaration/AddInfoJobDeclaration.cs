using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class JobDeclaration
	{
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryList))]
		public override ZString JE_ShippingCountry { get => base.JE_ShippingCountry; set => base.JE_ShippingCountry = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryList))]
		public override ZString JE_CountryOfSupply { get => base.JE_CountryOfSupply; set => base.JE_CountryOfSupply = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TradeTypeList))]
		public override ZString JE_TradeType { get => base.JE_TradeType; set => base.JE_TradeType = value; }
	}
}
