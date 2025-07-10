using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class AdditionalInformationProvider : IAdditionalInformation
{
	public AdditionalInformationProvider(CusSupportingInfo cusSupportingInfo)
	{
		this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
	}
	readonly CusSupportingInfo cusSupportingInfo;

	public int SequenceNumeric => cusSupportingInfo.CSI_LineNo;

	public string StatementCode => cusSupportingInfo.CSI_Code;

	public string StatementDescription => cusSupportingInfo.CSI_Description;

	public string StatementTypeCode => null;

	public IReadOnlyCollection<IPointer> Pointers => Array.Empty<IPointer>();
}
