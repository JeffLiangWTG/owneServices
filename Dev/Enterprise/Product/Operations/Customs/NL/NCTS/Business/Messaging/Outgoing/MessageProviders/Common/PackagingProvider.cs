using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NL.NCTS.Business;

public class PackagingProvider : INCTSPackaging
{
	public PackagingProvider(NctsPackage package, int sequence, bool statusIsNew = true)
	{
		this.package = Argument.NotNull(package, nameof(package));
		SequenceNumeric = sequence;
		this.statusIsNew = statusIsNew;
	}
	protected readonly NctsPackage package;
	protected readonly bool statusIsNew;

	public int SequenceNumeric { get; }

	public string TypeOfPackages => statusIsNew ? package.B5_UnitType : string.Empty;

	public virtual int? NumberOfPackages
		=> (TypeOfPackages.In(RefCusCodeUnPackedPackageUnitType.Unpacked, RefCusCodeUnPackedPackageUnitType.UnpackedMultiple, RefCusCodeUnPackedPackageUnitType.UnpackedSingle)
		&& package.B5_UnitCount.IsEmpty) || !statusIsNew
		? null
		: package.B5_UnitCount.ToZInt();

	public string ShippingMarks => statusIsNew ? package.B5_MarksAndNumbers : string.Empty;
}
