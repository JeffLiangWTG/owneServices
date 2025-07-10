using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

public class NOEDIMessageCollection : EDIMessageCollection
{
	public NOEDIMessageCollection(BusinessObject master) : base(master)
	{
	}

	public new NOEDIMessage this[int index] => (NOEDIMessage)base[index];

	public new NOEDIMessage AddNew() => (NOEDIMessage)base.AddNew();
}
