using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class PackagingWrapper : IPackaging
{
	public PackagingWrapper(BasePackage package, int sequenceNumeric)
	{
		this.package = Argument.NotNull(package, nameof(package));
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly BasePackage package;

	public string MarksNumbersID => package.CW_MarksAndNos;

	public int QuantityQuantity => (package.InvoiceLinePivotCollection).Cast<InvoiceLinePackagePivot>().FirstOrDefault()?.CHC_NumberOfPacks ?? 0;

	public string TypeCode => package.CW_PackType;

	public int SequenceNumeric { get; }
}
