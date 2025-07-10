using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageForTest : CommonCartage
	{
		public CartageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public StmNoteContexts GetNoteContextsForRelatedNotes()
		{
			return NoteContextsForRelatedNotes;
		}
	}
}
