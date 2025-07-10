using System;
using System.Linq;

namespace Enterprise.Customs.NL.Business;

public class EntrySelectionValidation : AutoEntrySelectionValidation
{
	public EntrySelectionValidation(AutoEntrySelection parent) : base(parent)
	{
	}

	protected override void CheckSelected()
	{
		if (Parent.Declaration.EntrySelections.Where(x => x.Selected).Count() > 1)
		{
			Parent.SelectedInfo.AddError(Res.GetString("DD893400-2F5B-4966-9233-8392EBC4D0E8", "Only 1 entry can be selected."));
		}
	}

	public override Type AutoValidationType => typeof(EntrySelectionValidation);

	new EntrySelection Parent => (EntrySelection)base.Parent;
}
