using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESPackagingProvider : IPackaging
{
	public AESPackagingProvider(InvoiceLinePackagePivot[] packagePivots, JobDeclaration declaration, int sequenceIndex)
	{
		this.packagePivots = Argument.NotNull(packagePivots, nameof(packagePivots));
		randomPackagePivot = packagePivots.First();
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		this.sequenceIndex = sequenceIndex;
	}

	readonly InvoiceLinePackagePivot[] packagePivots;
	readonly InvoiceLinePackagePivot randomPackagePivot;
	readonly JobDeclaration declaration;
	readonly int sequenceIndex;

	public int SequenceNumber => sequenceIndex;

	public string TypeOfPackages => CachedValueHelper.GetValue(ref typeOfPackages, () => randomPackagePivot.Package.CW_PackType);
	CachedValue<string> typeOfPackages;

	public int? NumberOfPackages => CachedValueHelper.GetValue(ref numberOfPackages, () => CheckRuleC0060 ? null : CalculateNumberOfPackages());
	CachedValue<int?> numberOfPackages;

	public string ShippingMarks => CachedValueHelper.GetValue(ref shippingMarks, () => MessageProviderHelper.ReturnNullIfEmpty(randomPackagePivot.Package.CW_MarksAndNos));
	CachedValue<string> shippingMarks;

	bool CheckRuleC0060 => PackageHelper.IsBulkCode(TypeOfPackages, declaration.Factory);

	int? CalculateNumberOfPackages()
	{
		var result = 0;
		var hasMatch = false;

		foreach (var packagePivot in packagePivots)
		{
			var package = packagePivot.Package;
			if (package.CW_PackType == TypeOfPackages
				&& package.CW_MarksAndNos == ShippingMarks)
			{
				if (!hasMatch)
				{
					result = 0;
					hasMatch = true;
				}
				result += packagePivot.CHC_NumberOfPacks;
			}
		}

		return result;
	}
}
