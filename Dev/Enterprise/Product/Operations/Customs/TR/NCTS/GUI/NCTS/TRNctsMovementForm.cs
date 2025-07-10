using System;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.TR.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class TRNctsMovementForm : NctsMovementForm
	{
		public TRNctsMovementForm(NctsHeader nctsMovement)
			: base(nctsMovement)
		{
			InitializeComponent();
			SetTabAtIndex();
		}

		protected override Type GetDeclarationDetailsUserControlType()
		{
			return typeof(TRDeclarationDetailsTabUserControl);
		}

		void SetTabAtIndex()
		{
			if (BusinessEntity.IsDepartureMovement)
			{
				this.MainTabControl.TabPages.Insert(this.ManifestsToOpenTabPage, 2);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.ManifestsToOpenTabPage.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		protected override Type GetGoodsItemUserControlType()
		{
			return typeof(TRNctsGoodsItemsUserControl);
		}

		protected override EU.NCTS.GUI.NctsMessagingMenu GetNctsMessageMenu() => new NctsMessagingMenu(this);

		public new NctsHeader BusinessEntity => (NctsHeader)base.BusinessEntity;

		protected override Type GetSecurityUserControlType() => typeof(SecurityTabUserControl);

		protected override Type GetMessagesUserControlType() => typeof(MessagesTabUserControl);
	}
}
