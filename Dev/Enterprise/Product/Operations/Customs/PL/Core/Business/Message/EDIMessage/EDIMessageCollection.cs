using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class EDIMessageCollection : Messaging.Business.EDIMessageCollection
{
	public EDIMessageCollection(BusinessObject master)
		: base(master)
	{
	}

	public EDIMessageCollection(BusinessObject master, ZQuery additionalFilter)
		: base(master, additionalFilter)
	{
	}

	public new EDIMessage this[int index] => (EDIMessage)base[index];

	public new EDIMessage AddNew() => (EDIMessage)base.AddNew();
}
