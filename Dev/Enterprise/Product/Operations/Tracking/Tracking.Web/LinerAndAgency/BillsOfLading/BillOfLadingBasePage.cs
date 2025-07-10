using System;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public abstract class BillOfLadingBasePage : LinerAndAgencyBasePage
	{
		protected override string NotFoundLabelText
		{
			get { return Res.GetString("34491434-0d09-4545-8e12-60ddfc1e07c1", "Bill of Lading was not found in the database or you don't have rights to access it."); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (BillOfLading != null &&
			BillOfLading.Destination != null &&
			BillOfLading.Destination.Country != null)
			{
				ICustomsMessageStatusProvider provider = CustomsMessageStatusProviderFactory.New(BillOfLading.Destination.RL_RN_NKCountryCode);
				bool showCustomsData = provider.ShouldShow(BillOfLading);

				CustomsStatusRow.Visible = showCustomsData;
				MessageStatusRow.Visible = showCustomsData;

				if (showCustomsData)
				{
					CustomsStatus.Text = provider.GetCustomsStatus(BillOfLading);
					MessageStatus.Text = provider.GetMessageStatus(BillOfLading);
				}
			}
		}

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			TrackingBillOfLading result = null;
			ZGuid bookingPK = GetGuidFromParameter("Ref");

			if (SiteUser != null && SiteUser.LoggedInOrganisation != null && !bookingPK.IsEmpty)
			{
				ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(TrackingBillOfLading));
				filter.AddToFilter(JobShipmentSchema.PK, bookingPK);
				if (!SiteUser.IsShipmentQuickViewUser)
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBillOfLading>());
				}
				filter.IgnoreActiveFilter = true;
				result = Factory.LoadTop1<TrackingBillOfLading>(filter);
			}
			return result;
		}

		public TrackingBillOfLading BillOfLading
		{
			get { return DataSource as TrackingBillOfLading; }
		}

		#endregion

		#region WebInterfacesHelper

		protected override LinerAndAgencyBaseWebInterfacesHelper GetNewWebInterfacesHelper()
		{
			return new LinerAndAgencyBillOfLadingWebInterfacesHelper(BillOfLading);
		}

		#endregion

		protected HtmlTableRow CustomsStatusRow;
		protected ZTextLabel CustomsStatus;
		protected HtmlTableRow MessageStatusRow;
		protected ZTextLabel MessageStatus;
	}
}
