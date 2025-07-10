using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class BounceBackEmailProcessResult : NonPersistentBusinessObject
	{
		public BounceBackEmailProcessResult() { }

		public bool IsSuccess { get; set; }
		public ZGuid MailItemPK { get; set; }
		public ZDateTime MailItemReceivedTimeUtc { get; set; }
		public ZString MailItemSubject { get; set; }
		public ZString FromAddress { get; set; }
		public ZString SentTimeText { get; set; }
		public ZGuid SenderStaffID { get; set; }
		public GlbStaff SenderStaff { get; set; }
		public BusinessObject BusinessEntity { get; set; }
		public ZGuid BusinessEntityID { get; set; }
		public ZString BusinessEntityTableCode { get; set; }
		public bool BusinessEntityInDatabase { get; set; }
		public ZString DocumentName { get; set; }
		public ZString JobNumber { get; set; }
		public ZString BounceReasonCode { get; set; }
		public ZString BouncedRecipients { get; set; }
		public ZString DiagnositicInfo { get; set; }
	}
}
