using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.GBCustoms.TL.BT.Helpers
{
	[Serializable, XmlRoot("TypedPolling", Namespace = "http://schemas.microsoft.com/Sql/2008/05/TypedPolling/GBCTLPolling")]
	public class TypedPolling
	{
		[XmlArray("TypedPollingResultSet0")]
		[XmlArrayItem("TypedPollingResultSet0")]
		public List<TypedPollingResultSet0> TypedPollingResultSet0 { get; set; }
	}

	[Serializable]
	public class TypedPollingResultSet0
	{
		public string Provider { get; set; }
		public string Subscriber { get; set; }
		public string CredentialsKey { get; set; }
	}

	[Serializable]
	public class TypedPollingArray
	{
		public TypedPollingResultSet0[] PollingResults { get; set; }
	}
}
