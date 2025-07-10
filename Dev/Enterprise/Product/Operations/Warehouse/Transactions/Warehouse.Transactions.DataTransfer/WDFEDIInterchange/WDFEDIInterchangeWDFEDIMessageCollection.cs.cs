using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WDFEDIInterchangeWDFEDIMessageCollection : EDIInterchangeEDIMessageCollection
	{
		#region Constructors

		public WDFEDIInterchangeWDFEDIMessageCollection(WDFEDIInterchange master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		#endregion

		#region Overrides

		public virtual new WDFEDIMessage AddNew()
		{
			return (WDFEDIMessage)base.AddNew();
		}

		public virtual new WDFEDIMessage AddNew(Type bizoType)
		{
			return (WDFEDIMessage)base.AddNew(bizoType);
		}

		public new WDFEDIMessage this[int index]
		{
			get { return (WDFEDIMessage)Elements[index]; }
		}

		#endregion
	}
}
