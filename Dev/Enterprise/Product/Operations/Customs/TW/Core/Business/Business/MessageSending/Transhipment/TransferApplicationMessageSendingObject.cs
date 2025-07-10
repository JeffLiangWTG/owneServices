using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.N5301
{
	public partial class N5301MessageSendingObject : TranshipmentMessageSendingObject
	{
		public N5301MessageSendingObject(CusInBondHeader header)
			: base(header, MessageTypeList.Codes.TRA)
		{
		}

		public override CodeDescriptionPairList ActionList
		{
			get
			{
				return Factory.GetCachedValue("ActionList",
				delegate
				{
					var list = new ActionCodeList();
					list.RemoveCode(ActionCodeList.Codes.Update);
					list.RemoveCode(ActionCodeList.Codes.Delete);
					return list;
				});
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Action = ActionCodeList.Codes.Create;
		}

		public override ZString GetMessageOwner()
		{
			return Applicant?.ID ?? ZString.Empty;
		}
	}
}
