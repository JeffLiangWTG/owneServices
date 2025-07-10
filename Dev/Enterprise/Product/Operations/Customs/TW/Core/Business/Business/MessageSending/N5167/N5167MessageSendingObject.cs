using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.N5167
{
	public partial class N5167MessageSendingObject : MessageSendingObject
	{
		public N5167MessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			Action = ActionCodeList.Codes.Create;
		}

		public override CodeDescriptionPairList ActionList
		{
			get
			{
				return Factory.GetCachedValue("IEAActionCodeList", () =>
				{
					var list = new ActionCodeList();
					list.RemoveCode(ActionCodeList.Codes.Update);
					list.RemoveCode(ActionCodeList.Codes.Delete);
					return list;
				});
			}
		}

		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new N5167MessageSendingObjectValidation(this);
		}

		[BusinessObjectTestExclude]
		public override ZString MessageType { get => MessageTypeList.Codes.IEA; }

		public override ZString GetMessageOwner()
		{
			var buyerOrg = Header.Declaration?.Buyer;
			var result = ZString.Empty;
			if (buyerOrg != null)
			{
				result = GetCustomsRegNo(buyerOrg, OrgCusCode.CodeTypes.VATCode);
				if (result.IsEmpty)
				{
					result = GetCustomsRegNo(buyerOrg, OrgCusCode.CodeTypes.PassportID);
				}

				if (result.IsEmpty)
				{
					result = GetCustomsRegNo(buyerOrg, OrgCusCode.TaiwanCodeTypes.PID);
				}
			}

			return result;
		}

		public override ZString FriendlyNameForMessageManager => Res.GetString("3A2C5754-CC7D-4114-B138-79F633F17AD4", "Send Goods Examination Application");

		public override ZString SerializeToMessageString()
		{
			return new N5167MessageBuilder().SerializeToMessageString(this);
		}
	}
}
