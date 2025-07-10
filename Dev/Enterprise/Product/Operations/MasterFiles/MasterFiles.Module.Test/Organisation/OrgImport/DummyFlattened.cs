using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyFlattened : NonPersistentBusinessObject
	{
		public DummyFlattened() { }

		public DummyFlattened(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString FlatHeaderString { get; set; }
		public ZDate FlatHeaderDate { get; set; }
		public ZInt FlatChild1Int { get; set; }
		public ZDate FlatChild1Date { get; set; }
		public ZInt FlatChild2Int { get; set; }
		public ZDate FlatChild2Date { get; set; }
		public ZString HomeAddress_OH_Code { get; set; }
		public ZString HomeAddress_E2_Address1 { get; set; }
		public ZString HomeAddress_E2_Address2 { get; set; }
		public ZString HomeAddress_E2_City { get; set; }
		public ZString HomeAddress_E2_State { get; set; }
		public ZString HomeAddress_E2_Postcode { get; set; }
		public ZString XX_HeaderString { get; set; }
		public ZDate XX_HeaderDate { get; set; }

		public ZString Child_ChildString { get; set; }

		public static class Schema
		{
			public const string FlatHeaderString = "FlatHeaderString";
			public const string FlatHeaderDate = "FlatHeaderDate";
			public const string FlatChild1Int = "FlatChild1Int";
			public const string FlatChild1Date = "FlatChild1Date";
			public const string FlatChild2Int = "FlatChild2Int";
			public const string FlatChild2Date = "FlatChild2Date";
		}

		public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}
