using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class Engine : XmlSerializableNonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string Data = "Data";
			public const string CylindersCount = "CylindersCount";
		}

		public Engine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZBlob Data
		{
			get { return data; }
			set { data = value; }
		}
		ZBlob data;

		[XmlElement("Cylinders")]
		public ZShort CylindersCount
		{
			get { return cylindersCount; }
			set { cylindersCount = value; }
		}
		ZShort cylindersCount;
	}
}
