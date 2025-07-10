using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CommodityGroupForm : ZChildForm
	{
		public CommodityGroupForm(CommodityGroupViewModelCollection collection)
			: base(collection)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;
		public override string FormCaption => ResString.GetMultilingualString("8603C10B-5131-4043-938E-FE64296DBA62", "Commodity Groups");

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CommodityGroupsGrid?.Dispose();
				OKButton?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
