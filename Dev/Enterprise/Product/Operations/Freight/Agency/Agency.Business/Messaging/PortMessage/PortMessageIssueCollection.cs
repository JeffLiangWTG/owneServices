using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageIssueCollection : NonPersistentBusinessObjectCollection<PortMessageIssue>
	{
		public PortMessageIssue AddNew(ZGuid targetPK, ZString targetCode, ZString text, ZString detail)
		{
			PortMessageIssue issue = new PortMessageIssue(targetPK, targetCode, text, detail);
			Add(issue);
			return issue;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}



