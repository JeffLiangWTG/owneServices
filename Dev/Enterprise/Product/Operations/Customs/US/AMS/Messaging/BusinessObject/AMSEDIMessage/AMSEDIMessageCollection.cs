using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSEDIMessageCollection : US.Messaging.Business.CBPEDIMessageCollection
	{
		public AMSEDIMessageCollection(BusinessObject master)
			: base(master, AMSEDIMessage.AMSFilter)
		{
		}

		public new AMSEDIMessage this[int index]
		{
			get { return (AMSEDIMessage)base[index]; }
		}

		public new AMSEDIMessage AddNew()
		{
			return (AMSEDIMessage)base.AddNew();
		}

		public new AMSEDIMessage AddNew(Type bizOType)
		{
			return (AMSEDIMessage)base.AddNew(bizOType);
		}
	}
}
