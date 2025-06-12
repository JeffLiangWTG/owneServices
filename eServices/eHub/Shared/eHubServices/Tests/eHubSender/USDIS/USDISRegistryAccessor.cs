using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.USDIS
{
	public class USDISRegistryAccessor:IRegistryAccessor
	{
		#region IRegistryAccessor Members

		public void InsertMessageReference(string clientId, string applicationCode, string messageReference)
		{
			throw new NotImplementedException();
		}

		public string ResolveMessageReference(string reference, string applicationCode)
		{
			throw new NotImplementedException();
		}

		public bool SelectRegistryIsProd(string clientId, string applicationCode, string name)
		{
			return clientId.EndsWith("PROD");
		}

		public string SelectRegistryValue(string clientId, string applicationCode, string name)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
