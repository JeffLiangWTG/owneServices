using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IUTBDocumentDataProvider
{
	#region Fields for UTB

	#region General
	ZDateTime DateOfIssue { get; }
	ZDateTime ObjectionDateTime { get; }
	#endregion

	#region Declaration general details
	IAddressDocumentInformation Agent { get; }
	IAddressDocumentInformation Declarant { get; }
	ZString MovementReferenceNumber { get; }
	ZDecimal TotalDutiesAndTaxes { get; }
	#endregion

	#region Line Details
	IEnumerable<IEntryLineDetails> EntryLineDetails { get; }
	#endregion
	#endregion
}
