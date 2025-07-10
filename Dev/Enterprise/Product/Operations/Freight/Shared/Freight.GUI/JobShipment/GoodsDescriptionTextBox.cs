using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class GoodsDescriptionTextBox : ZTextBoxWithDetailsOnNote
	{
		public GoodsDescriptionTextBox()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				NoteTypeDescription = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			}
		}

		protected override void HookPopup(ZStmNotePopupForm notePopupForm)
		{
			base.HookPopup(notePopupForm);
			notePopupForm.LeadingComment = Res.GetString("Freight|GoodsDescriptionTextBoxThisPopupAllowsYou", "This popup allows you to enter the detailed goods description. Changing the detailed goods description will not affect the short description.");
			notePopupForm.NoteHasChangesChanged += notePopupForm_NoteHasChangesChanged;
		}

		void notePopupForm_NoteHasChangesChanged()
		{
			var parent = this.DataSource as Business.IHaveDetailedGoodsDescription;
			if (parent != null)
			{
				parent.NotifyChanged();
			}
		}
	}
}
