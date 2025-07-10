using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class InquirySalesDashboardActivity : SalesDashboardActivity
	{
		public InquirySalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public SalesEnquiry ParentInquiry
		{
			get { return Factory.Load<SalesEnquiry>(VSA_ParentId); }
		}

		public override ZString VSA_ActivityDescription
		{
			get
			{
				var description = base.VSA_ActivityDescription;

				if (string.IsNullOrEmpty(description))
				{
					var inquiryTypeCode = VSA_ActivitySubType;
					if (!string.IsNullOrEmpty(inquiryTypeCode) && ParentInquiry != null)
					{
						var inquiryTypeDescription = ParentInquiry.Lookups.AllEnquiryTypes.GetDescriptionFromCode(inquiryTypeCode);
						if (!string.IsNullOrEmpty(inquiryTypeDescription))
						{
							description = inquiryTypeDescription;
						}
					}
				}

				return description;
			}
		}

		public override ZString OverallActivityDispositionDescription
		{
			get { return ParentInquiry?.OverallDispositionDescription ?? ZString.Empty; }
		}

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks
		{
			get { return tasks ?? (tasks = new SalesDashboardProcessTaskCollectionView(ParentInquiry.WorkflowItems)); }
		}
		SalesDashboardProcessTaskCollectionView tasks;

		#region ISalesRelationActivity

		protected override ISalesRelationActivity ParentActivity
		{
			get { return ParentInquiry; }
		}

		#endregion
	}
}
