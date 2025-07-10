using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobDeclarationForTest : BaseJobDeclaration
	{
		public BaseJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		new internal StmNoteContexts NoteContextsForRelatedNotes => base.NoteContextsForRelatedNotes;
		new internal WarehouseInvoiceLink fWarehouseInvoiceLink => base.fWarehouseInvoiceLink;
		new internal bool shouldOverrideNotes => base.shouldOverrideNotes;
		new internal virtual bool SupportsBondedWarehousingCore => base.SupportsBondedWarehousingCore;

		public static new BaseJobDeclarationForTest New(BusinessObjectFactory factory)
		{
			return factory.New<BaseJobDeclarationForTest>();
		}
	}
}
