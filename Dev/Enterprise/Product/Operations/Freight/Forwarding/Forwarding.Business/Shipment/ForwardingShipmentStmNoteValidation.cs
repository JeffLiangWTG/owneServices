using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentStmNoteValidation : StmNoteValidation
	{
		public ForwardingShipmentStmNoteValidation(ForwardingShipmentStmNote parent) : base(parent)
		{
		}

		public new StmNote Parent => (ForwardingShipmentStmNote)base.Parent;

		#region CheckST_NoteText

		protected override void CheckST_NoteText()
		{
			if (!Parent.ST_IsTextOnly
				|| !Parent.ST_NoteText.IsEmpty
				|| (Parent.ST_Description != PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description
					&& Parent.ST_Description != PredefinedNoteTypes.Instance.MarksAndNumbers.Description))
			{
				base.CheckST_NoteText();
			}
		}

		#endregion

		#region CheckST_NoteText

		protected override void CheckST_Description()
		{
			if (string.Compare(Parent.ST_Description, PredefinedNoteTypes.Instance.OriginalBillNotes.Description, StringComparison.OrdinalIgnoreCase) == 0 && !Parent.IsInDatabase)
			{
				var message = Res.GetString("79D6C5AB-DA47-4566-A005-3C02BC330E53", "Original Bill Notes is reserved for system use and cannot be manually entered.");
				Parent.ST_DescriptionInfo.AddError(message);
				return;
			}

			base.CheckST_Description();
		}

		#endregion
	}
}
