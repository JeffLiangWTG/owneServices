using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public partial class OneOffQuoteStatusesCodeList : CodeDescriptionPairList
	{
		public bool IsOpenStatus(string status)
		{
			return OpenStatuses.Contains(status);
		}

		public IList<string> OpenStatuses
		{
			get
			{
				if (openStatuses == null)
				{
					openStatuses = new List<string>();
					openStatuses.Add(Codes.Active);
					openStatuses.Add(Codes.Finalized);
					openStatuses.Add(Codes.Approved);
					openStatuses.Add(Codes.Booked);
				}
				return openStatuses;
			}
		}
		IList<string> openStatuses;
	}

	public class OneOffQuoteSalesDashboardActivity : SalesDashboardActivity
	{
		public OneOffQuoteSalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessObject ParentQuote
		{
			get
			{
				if (quotedBooking == null)
				{
					var builder = ObjectFactory.Get<IQuotedBookingBuilder>();
					if (builder != null)
					{
						var parent = Factory.Load<RateOneOffShipment>(VSA_ParentId);
						quotedBooking = builder.Load(Factory, parent.TT_TH);
					}
				}
				return quotedBooking;
			}
		}
		BusinessObject quotedBooking;

		public override ZString OverallActivityDispositionDescription => (oneOffQuoteStatuses ?? (oneOffQuoteStatuses = new OneOffQuoteStatusesCodeList())).IsOpenStatus(VSA_ActivityStatus)
			? OrgSalesCallOverallDispositionList.Descriptions.Open
			: OrgSalesCallOverallDispositionList.Descriptions.Closed;
		OneOffQuoteStatusesCodeList oneOffQuoteStatuses;

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks => tasks
			?? (tasks = new SalesDashboardProcessTaskCollectionView((ParentQuote as IWorkflowProvider)?.WorkflowItems));
		SalesDashboardProcessTaskCollectionView tasks;

		protected override ISalesRelationActivity ParentActivity => ParentQuote as ISalesRelationActivity;
	}
}
