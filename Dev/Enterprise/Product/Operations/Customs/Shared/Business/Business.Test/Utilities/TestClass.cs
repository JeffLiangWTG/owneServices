using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestClass : DummyBusinessObject
	{
		public TestClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		public ZString TC_String1 { get; set; }

		public ZPropertyInfo TC_String1Info
		{
			get { return GetZPropertyInfo(nameof(TC_String1)); }
		}

		public ZWrappedPropertyInfo WrappedString1Info
		{
			get { return GetWrappedZPropertyInfo(nameof(TC_String1), x => TC_String1Info); }
		}

		[IsNAddInfoField]
		public ZString TC_String2 { get; set; }

		public ZPropertyInfo TC_String2Info
		{
			get { return GetZPropertyInfo(nameof(TC_String2)); }
		}

		public ZWrappedPropertyInfo WrappedString2Info
		{
			get { return GetWrappedZPropertyInfo(nameof(TC_String2), x => TC_String2Info); }
		}
	}
}
