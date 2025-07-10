using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class Human : XmlSerializableNonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string Name = "Name";
			public const string DOB = "DOB";
		}

		public Human(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[MaxLength(50)]
		public ZString Name
		{
			get { return name; }
			set { name = value; }
		}
		ZString name;

		public ZDate DOB
		{
			get { return dob; }
			set { dob = value; }
		}
		ZDate dob;
	}
}
