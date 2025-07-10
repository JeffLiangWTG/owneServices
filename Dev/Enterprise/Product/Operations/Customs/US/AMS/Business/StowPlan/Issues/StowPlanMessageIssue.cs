using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanMessageIssue : PortMessageIssue
	{
		public StowPlanMessageIssue(ZGuid targetPK, ZString targetCode, ZString text, ZString detail, ZString moreDetail, INotificationType notificationType)
			: base(targetPK, targetCode, text, detail)
		{
			this.notificationType = notificationType;
			this.moreDetail = moreDetail;
		}
		internal readonly INotificationType notificationType;
		readonly ZString moreDetail;

		public ZString ErrorType
		{
			get
			{
				if (this.notificationType == NotificationType.Warning)
				{
					return "Warning";
				}
				else
				{
					return "Message Error";
				}
			}
		}

		public CargoWise.EntityFramework.ZPropertyInfo ErrorTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ErrorType)); }
		}

		[MaxLength(255)]
		public ZString MoreDetail
		{
			get { return this.moreDetail; }
		}

		public CargoWise.EntityFramework.ZPropertyInfo MoreDetailInfo
		{
			get { return GetZPropertyInfo(nameof(MoreDetail)); }
		}
	}
}
