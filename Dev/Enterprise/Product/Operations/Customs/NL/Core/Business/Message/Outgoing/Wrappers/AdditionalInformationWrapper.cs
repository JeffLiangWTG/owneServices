using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class AdditionalInformationWrapper : IAdditionalInformation
{
	public AdditionalInformationWrapper(CusSupportingInfo additionalInformation, int sequenceNumeric)
	{
		additionalInfo = Argument.NotNull(additionalInformation, nameof(additionalInformation));
		SequenceNumeric = sequenceNumeric;
	}

	public AdditionalInformationWrapper(string reasonForInvalidation)
	{
		this.reasonForInvalidation = reasonForInvalidation;
		SequenceNumeric = 1;
	}

	readonly CusSupportingInfo additionalInfo;
	readonly string reasonForInvalidation;

	public string StatementCode => additionalInfo?.CSI_Code;

	public string StatementDescription => additionalInfo?.CSI_Description ?? reasonForInvalidation;

	public string CCQualifierCode => null;

	public string StatementTypeCode => ResponseStatementTypes.Codes.ReasonForAnInvalidation;

	public IReadOnlyCollection<IPointer> Pointers => Array.Empty<IPointer>();

	public int SequenceNumeric { get; }
}
