using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing
{
	sealed class DummyChild : NonPersistentBusinessObject
	{
		public DummyChild(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZInt ChildPositiveInt;
		public ZDate ChildZDate;

		public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}
