namespace Enterprise.Customs.NL.Module;

public class EntryHeaderFilterBusinessObject : EU.Module.EntryHeaderFilterBusinessObject
{
	protected override Customs.Module.EntryHeaderFilterLookups GetNewLookups()
	{
		return new EntryHeaderFilterLookups(this);
	}
}
