using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Declaration;

public sealed class SingleLineEntryProvider : ISingleLineEntryProvider
{
	public SingleLineEntry GetSingleLineEntry(EU.Business.Declaration.JobDeclaration declaration) => new SingleLineEntry(declaration);
	public ZString DefaultCPCCode(EU.Business.Declaration.JobDeclaration declaration) => declaration.IsExport ? (ZString)"1000" : declaration.IsImport ? (ZString)"4000" : ZString.Empty;
}
