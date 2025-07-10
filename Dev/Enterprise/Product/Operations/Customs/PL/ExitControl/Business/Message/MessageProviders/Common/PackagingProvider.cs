using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class PackagingProvider(EU.ExitControl.Business.CusExitConsignmentPackage exitPackage) : IPackaging
{
	readonly EU.ExitControl.Business.CusExitConsignmentPackage package = Argument.NotNull(exitPackage, nameof(exitPackage));

	public int SequenceNumber => package.CXP_Sequence;

	public string TypeOfPackages => package.CXP_PackageType;

	public int? NumberOfPackages => package.CXP_Quantity;

	public string ShippingMarks => package.CXP_MarksAndNumbers;
}
