using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class PackagingProvider : IPackaging
{
	readonly NctsPackage package;
	readonly bool isInPhase5TransitionPeriod;

	public PackagingProvider(int sequenceNumber, NctsPackage package, bool isInPhase5TransitionPeriod)
	{
		this.package = Argument.NotNull(package, nameof(package));
		SequenceNumber = sequenceNumber;
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}

	public int SequenceNumber { get; }

	public string TypeOfPackages => package.B5_UnitType;

	public int? NumberOfPackages => CachedValueHelper.GetValue(ref numberOfPackages, () => package.IsBulk ? null : (int)package.B5_UnitCount);
	CachedValue<int?> numberOfPackages;

	public string ShippingMarks => package.B5_MarksAndNumbers;

	public int ShippingMarksMaxLength => CachedValueHelper.GetValue(ref shippingMarksMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 42;
		var maxLengthOutsideTransitionPeriod = 512;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> shippingMarksMaxLength;
}
