using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassService : CFSService
	{
		public GatePassService(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Implementation

		protected override Type GetTypeFromPrefix(string prefix)
		{
			if (PrefixToTypeHash == null)
			{
				PrefixToTypeHash = new Dictionary<string, Type>();
				PrefixToTypeHash["JP"] = typeof(GatePassDocsAndCartage);
				PrefixToTypeHash["JC"] = typeof(GatePassContainer);
			}

			return PrefixToTypeHash[prefix];
		}

		#endregion
	}
}
