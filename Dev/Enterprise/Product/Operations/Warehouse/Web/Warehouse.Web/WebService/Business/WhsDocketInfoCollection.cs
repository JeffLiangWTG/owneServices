using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsDocketInfoCollection : DataObjectInfoCollection<WhsDocketInfo>
	{
		#region Constructors

		public WhsDocketInfoCollection(IEnumerable<WhsDocket> docketCollection, bool shouldCreateDocketLines = true)
		{
			if (docketCollection.Any())
			{
				JobDocAddressFetchHintsHelper.AddFetchHints(docketCollection.Select(d => d.PK), docketCollection.First().Factory);
			}

			foreach (var docket in docketCollection)
			{
				var docketInfo = new WhsDocketInfo(docket, shouldCreateDocketLines);
				Add(docketInfo);
			}
		}

		public WhsDocketInfoCollection()
			: base()
		{
		}

		#endregion
	}
}
