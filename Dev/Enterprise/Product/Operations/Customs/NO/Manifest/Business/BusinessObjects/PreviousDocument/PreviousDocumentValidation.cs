using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Manifest.Business;

public class PreviousDocumentValidation : CusSupportingInfoValidation
{
	public PreviousDocumentValidation(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	public new PreviousDocument Parent => (PreviousDocument)base.Parent;

	protected override void CheckCSI_Code()
	{
		var parent = Parent;
		base.CheckCSI_Code();
		MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_CodeInfo);
		ListValidation.MessageErrorIfInvalidCode(parent.CSI_CodeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		ValidateCSI_ReferenceNumberForHouseBill();
	}

	void ValidateCSI_ReferenceNumberForHouseBill()
	{
		var parent = Parent;
		if (parent.CSI_Code == PreviousDocumentConstants.Codes.CUDE
			&& parent.Parent is AsycudaBill { IsHouseBill: true }
			&& !HasValidReferenceNumber())
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(
				Res.GetString(
					"4AEAA945-A562-4643-9507-4C44BE0C13D6",
					"Document number for type CUDE requires the following structure: Declarant(9)-Date(8 YYYYMMDD)-Sequence(1-6 long). Example: 123456789-20240928-123"));
		}
	}

	bool HasValidReferenceNumber()
	{
		var referenceNumber = Parent.CSI_ReferenceNumber;
		var isMatch = new Regex(@"^\d{9}-\d{8}-\d{1,6}$").IsMatch(referenceNumber);

		if (!isMatch)
		{
			return false;
		}

		var datePart = referenceNumber.Split('-')[1];
		return ZDateTime.TryParseExact(datePart, out _, "yyyyMMdd");
	}
}
