using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolStmNote : StmNote
	{
		public ForwardingConsolStmNote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override StmNoteValidation GetNewValidation()
		{
			return new ForwardingConsolStmNoteValidation(this);
		}
	}
}
