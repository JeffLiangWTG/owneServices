using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class PreviousDocumentProvider : IPreviousDocument
{
	public PreviousDocumentProvider(int sequenceNumber, PreviousDocument previousDocument, bool isInPhase5TransitionPeriod)
	{
		SequenceNumber = sequenceNumber.ToString();
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}
	readonly PreviousDocument previousDocument;
	readonly bool isInPhase5TransitionPeriod;

	public string GoodsItemNumber => CachedValueHelper.GetValue(ref goodsItemNumber, () => previousDocument.CSI_ItemNumber.IsEmpty ? null : previousDocument.CSI_ItemNumber.ToString());
	CachedValue<string> goodsItemNumber;

	public string TypeOfPackages => previousDocument.CSI_UnitOfQuantity2;

	public string NumberOfPackages => ((int)previousDocument.CSI_Quantity2).ToString();

	public string MeasurementUnitAndQualifier => CachedValueHelper.GetValue(ref measurementUnitAndQualifier, () => IsRuleC0298 ? null : previousDocument.CSI_UnitOfQuantity);
	CachedValue<string> measurementUnitAndQualifier;

	public decimal? Quantity => previousDocument.CSI_Quantity;

	public string ComplementOfInformation => previousDocument.CSI_ReferenceNumber2;

	public int ComplementOfInformationMaxLength => CachedValueHelper.GetValue(ref complementOfInformationMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 26;
		var maxLengthOutsideTransitionPeriod = 35;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> complementOfInformationMaxLength;

	public string SequenceNumber { get; }

	public string DocumentType => previousDocument.CSI_Code;

	public string ReferenceNumber => previousDocument.Validation is IRuleG0321Checker validationChecker && validationChecker.CheckRuleG0321() ? "0" : previousDocument.CSI_ReferenceNumber.ToString();

	public int ReferenceNumberMaxLength => CachedValueHelper.GetValue(ref referenceNumberMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 35;
		var maxLengthOutsideTransitionPeriod = 70;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> referenceNumberMaxLength;

	bool IsRuleC0298 => previousDocument.CSI_Quantity.IsEmpty;
}
