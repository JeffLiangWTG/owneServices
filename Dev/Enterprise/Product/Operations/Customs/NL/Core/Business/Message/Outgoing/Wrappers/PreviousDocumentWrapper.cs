using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class PreviousDocumentWrapper : IPreviousDocument
{
	public PreviousDocumentWrapper(PreviousDocument previousDocument, int sequenceNumeric)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly PreviousDocument previousDocument;

	public string Id => previousDocument.CSI_ReferenceNumber;
	
	public string TypeCode => previousDocument.CSI_Code;

	public int? LineNumeric => ShouldMapLineNumeric ? previousDocument.CSI_LineNo : null;
	bool ShouldMapLineNumeric => !((previousDocument.ImportExportParent?.IsExport ?? false)
		&& previousDocument.CSI_LineNo == 0
		&& previousDocument.CSI_Code != NLConstants.PreviousDocumentTypes.C651 && previousDocument.CSI_Code != NLConstants.PreviousDocumentTypes.N705);

	public string CcQualifierCode => null;

	public IWriteOff WriteOff => null;

	public int SequenceNumeric { get; }
}
