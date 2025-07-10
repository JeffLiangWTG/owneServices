using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportSingleLineEntryLookups : SingleLineEntryLookups
{
	public ImportSingleLineEntryLookups(ImportSingleLineEntry parent) : base(parent)
	{
	}

	public RefCountryCollection CountryOfOrigins => new RefCountryCollection(Factory);

	public virtual CodeDescriptionPairList PreviousDocumentCodes => new ImportPreviousDocumentCodeList(Factory);
}
