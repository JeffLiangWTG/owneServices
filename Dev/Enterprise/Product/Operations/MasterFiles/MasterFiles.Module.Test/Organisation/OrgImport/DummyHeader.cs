using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	internal class DummyHeader : NonPersistentBusinessObject
	{
		public DummyHeader() { }

		public DummyHeader(BusinessObjectFactory factory)
			: base(factory)
		{
			ChildrenType1 = new DummyChildCollection(factory);
			ChildrenType2 = new DummyChildCollection(factory);
		}

		public ZString HeaderString;
		public ZDate HeaderDate;
		public ZString XX_HeaderString { get; set; }
		public ZDate XX_HeaderDate { get; set; }

		public bool HasBeenSetup { get; set; }

		public DummyChildCollection ChildrenType1;
		public DummyChildCollection ChildrenType2;

		public ZString ChildString { get; set; }

		public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}
