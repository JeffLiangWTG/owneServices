using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NO.Business;

readonly record struct EmmaDocument
{
	public EmmaDocument(string fileName, IeDoc document)
	{
		FileName = Argument.NotNullOrEmpty(fileName, nameof(fileName));
		Document = Argument.NotNull(document, nameof(document));
	}

	public string FileName { get; }
	public IeDoc Document { get; }
	public string DocumentType => Document.DocType;
	public string Description => Document.Description;
	public ZDateTime DateAdded => Document.DateAdded;
	public ZGuid DocumentId => Document.UniqueKey;
}
