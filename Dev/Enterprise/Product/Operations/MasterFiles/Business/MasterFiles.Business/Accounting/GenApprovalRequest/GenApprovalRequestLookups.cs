//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenApprovalRequestLookups
//
//    This class should be used for overriding collections in AutoGenApprovalRequestLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GenApprovalRequestLookups : AutoGenApprovalRequestLookups
	{
		public GenApprovalRequestLookups(AutoGenApprovalRequest parent) : base(parent)
		{
		}

		public virtual GlbStaffCollection SystemCreateUserList
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public virtual ReadOnlyCodeDescriptionPairList ReasonCodeList => GenApprovalRequestHelper.GetReasonCodeList((GenApprovalRequest)Parent);

		public static ReadOnlyCodeDescriptionPairList ReasonCodeDescriptionList(GenApprovalRequest genApprovalRequest)
		{
			var list = new CodeDescriptionPairList();
			var reasonCodeList = GenApprovalRequestHelper.GetReasonCodeList(genApprovalRequest);

			foreach (var code in reasonCodeList.GetAllCodes())
			{
				list.AddPair(code, string.Format(CultureInfo.InvariantCulture, "{0} - {1}", code, reasonCodeList.GetDescriptionFromCode(code)));
			}

			return list;
		}

		public virtual CodeDescriptionPairList ApprovalStatusList
		{
			get
			{
				var list = ApprovalStatusCodeDescriptionList;
				list.RemoveCode(Constants.GenApprovalRequestApprovalStatus.Error);

				return list;
			}
		}

		public static CodeDescriptionPairList ApprovalStatusCodeDescriptionList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Requested, ResString.GetMultilingualString("d29b3485-f1ec-4e34-885b-8b83c06a3313", "Requested"));
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Cancelled, ResString.GetMultilingualString("4795d8fe-dd07-4dea-9f93-978b0d1eb1be", "Canceled"));
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Rejected, ResString.GetMultilingualString("0fcb0957-d63f-4765-a0d3-fdc6fd91503b", "Rejected"));
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Approved, ResString.GetMultilingualString("eb9982a1-e951-4617-879b-f35752951f14", "Approved"));
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Posted, ResString.GetMultilingualString("1b21c965-a6e3-4c94-9297-7bc6b1b74745", "Posted"));
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Error, ResString.GetMultilingualString("A3C30B8C-9CC6-4228-ABCF-8C182C34C8F2", "Validation Errors"));

				return list;
			}
		}
	}
}
