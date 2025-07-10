using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public abstract class ImpExpMessageInterpreter<TDataProvider>(CusEntryHeader entryHeader)
	: MessageHtmlInterpreterBase<CusEntryHeader, TDataProvider>(entryHeader)
	where TDataProvider : class
{
	protected CusEntryHeader EntryHeader => LinkedObject;
	protected BusinessObjectFactory Factory => EntryHeader.Factory;
	protected JobDeclaration Declaration => EntryHeader.Declaration;
}
