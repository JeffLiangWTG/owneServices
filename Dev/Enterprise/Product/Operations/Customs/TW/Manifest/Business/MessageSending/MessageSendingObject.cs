using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class MessageSendingObject : Customs.Business.BaseMessageSendingObject
	{
		public MessageSendingObject(AsycudaBill bill) : base(bill.Factory)
		{
			Bill = bill;
		}

		public static class Schema
		{
			#region SuppressResourceStringsCheckRegion Reason = Schema
			public const string MessageStatus = "MessageStatus";
			public const string BillNumber = "BillNumber";
			public const string Action = "Action";
			public const string BagNumber = "BagNumber";
			#endregion
		}

		public AsycudaBill Bill { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
			Action = ActionCodeList.Codes.New;
		}

		#region Action

		ZString action;
		[List(nameof(ActionList))]
		[ResourceStringData("ED4D4EE4-4BE6-4B3F-BE27-D5C0C5FFE995", Caption = "Action")]
		public ZString Action
		{
			get => action;
			set
			{
				CheckMaximumLength(ActionInfo, value);
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
			}
		}

		public ZPropertyInfo ActionInfo => GetZPropertyInfo(Schema.Action);

		public CodeDescriptionPairList ActionList => Factory.GetCachedValue<ActionCodeList>();

		#endregion

		#region BillNumber

		[ResourceStringData("AF3517F5-B800-416B-9C7C-43F76C896A4D", Caption = "Bill Number")]
		public ZString BillNumber => Bill.ABL_BillNumber;
		#endregion

		#region BagNumber

		[ReadOnly(true)]
		[ResourceStringData("3346E2BE-DCE8-4F07-B8A9-BF4EA53FECE1", Caption = "Bag Number")]
		public ZString BagNumber => Bill.BagNumber;
		#endregion
	}
}
