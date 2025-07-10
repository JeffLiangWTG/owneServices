using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business;

public class EDIMessageCollection : Messaging.Business.EDIMessageCollection
{
	public EDIMessageCollection(BusinessObject master)
		: base(master)
	{
	}

	public new NLEDIMessage this[int index]
	{
		get { return (NLEDIMessage)base[index]; }
	}

	public new NLEDIMessage AddNew()
	{
		return (NLEDIMessage)base.AddNew();
	}
}
