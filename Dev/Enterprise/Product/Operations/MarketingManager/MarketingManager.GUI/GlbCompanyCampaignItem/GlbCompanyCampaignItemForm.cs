using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MarketingManager.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class GlbCompanyCampaignItemForm : ZArchitecture.GUI.ZTemplateForm, Enterprise.Integration.MarketingManager.GUI.IGlbCompanyCampaignItemForm
	{
		public GlbCompanyCampaignItemForm(GlbCompanyCampaignItem businessEntity) : base(businessEntity)
		{
		}

		public GlbCompanyCampaignItemForm()
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DisableNewAction();
			if (BusinessEntity.HasChanges)
			{
				DisplayMode = ODisplayMode.Edit;
			}
		}

		public new GlbCompanyCampaignItem BusinessEntity
		{
			get { return (GlbCompanyCampaignItem)base.BusinessEntity; }
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return Res.GetString("24550941-8d37-48c9-b2fd-0e543cfabccb", "Campaign Sent To {0}", BusinessEntity != null ? BusinessEntity.ContactName : ZString.Empty); }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

	}
}
