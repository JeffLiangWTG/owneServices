using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class TWMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		public TWMessageCollection(BusinessObject master)
			: base(master)
		{
		}

		public new TWMessage this[int index]
		{
			get { return (TWMessage)base[index]; }
		}

		public new TWMessage AddNew()
		{
			return (TWMessage)base.AddNew();
		}
	}
}
