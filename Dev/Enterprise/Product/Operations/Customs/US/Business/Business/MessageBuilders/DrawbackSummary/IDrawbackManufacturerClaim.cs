using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IDrawbackManufactureClaim
	{
		ZString CertificateOfManufactureNumber { get; }
		ZString CertificateOfManufacturePort { get; }
		ZDecimal DrawbackClaimDuty { get; }
		ZDecimal DrawbackClaimTax { get; }
		ZDecimal DrawbackManufactureQuantity { get; }
		ZString DrawbackManufactureUnitOfMeasure { get; }
		ZString DescriptionForBlock41 { get; }
	}
}
