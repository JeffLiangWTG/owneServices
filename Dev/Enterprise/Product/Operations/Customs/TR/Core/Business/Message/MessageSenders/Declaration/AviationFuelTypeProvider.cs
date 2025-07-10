using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class AviationFuelTypeProvider : IAviationFuelTypes
	{
		public AviationFuelTypeProvider(AviationFuelType aviationFuelType)
		{
			AviationFuelType = Argument.NotNull(aviationFuelType, nameof(aviationFuelType));
		}
		AviationFuelType AviationFuelType { get; }

		public string TaxId => AviationFuelType.CSI_ReferenceNumber2;
		public string InvoiceDate => AviationFuelType.CSI_DateOfIssue.ToISO8601ShortDateString();
		public string InvoiceNumber => AviationFuelType.CSI_ReferenceNumber;
		public string TotalInvoiceAmount => AviationFuelType.CSI_Value.ToString();
		public string FuelType => AviationFuelType.CSI_Description;
	}
}
