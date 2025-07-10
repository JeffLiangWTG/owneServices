using System;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolStmNoteValidation : StmNoteValidation
	{
		public ForwardingConsolStmNoteValidation(ForwardingConsolStmNote parent) : base(parent)
		{
		}

		public new StmNote Parent => (ForwardingConsolStmNote)base.Parent;

		protected override void CheckST_Description()
		{
			if (string.Compare(Parent.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.Description, StringComparison.OrdinalIgnoreCase) == 0 && !Parent.IsInDatabase)
			{
				var message = Res.GetString("69654055-E97C-406B-8B8C-EC72C6FB63A1", "Original Bill Notes is reserved for system use and cannot be manually entered.");
				Parent.ST_DescriptionInfo.AddError(message);
			}

			if (!Parent.ST_DescriptionInfo.HasErrors())
			{
				base.CheckST_Description();
			}
		}
	}
}
