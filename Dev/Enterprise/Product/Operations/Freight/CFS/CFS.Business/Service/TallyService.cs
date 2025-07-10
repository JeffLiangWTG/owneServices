using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyService : CFSService
	{
		public TallyService(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Implementation

		protected override Type GetTypeFromPrefix(string prefix)
		{
			if (PrefixToTypeHash == null)
			{
				PrefixToTypeHash = new Dictionary<string, Type>();
				PrefixToTypeHash["JP"] = typeof(PackUnpackDocsAndCartage);
				PrefixToTypeHash["JC"] = typeof(TallyContainer);
			}

			return PrefixToTypeHash[prefix];
		}

		#endregion
	}
}
