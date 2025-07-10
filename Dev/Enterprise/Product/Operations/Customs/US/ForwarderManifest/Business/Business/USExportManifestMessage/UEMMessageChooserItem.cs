using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMMessageChooserItem : MessageChooserItem
	{
		public UEMMessageChooserItem(MessageChooser chooser, ISelectionItem item, bool showStatus)
			: base(chooser, item, showStatus)
		{
		}

		public new UEMMessageChooser Chooser => (UEMMessageChooser)base.Chooser;

		SplitBillSelectionItem SplitBillSelectionItem => (SplitBillSelectionItem)BizO;

		public ZInt ManifestQty => SplitBillSelectionItem.Bill.ABL_ManifestQty;

		#region ActionType

		[List(nameof(BillOfLadingActionType))]
		public ZString ActionType
		{
			get { return SplitBillSelectionItem.ActionType; }
			set
			{
				var old = ActionType;
				if (old != value)
				{
					SplitBillSelectionItem.ActionType = value;
					SplitBillSelectionItem.SetMessage();
					ActionTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ActionTypeInfo => GetZPropertyInfo(nameof(ActionType));

		#endregion

		#region Meaasge

		[ReadOnly(true)]
		public ZString Message
		{
			get { return SplitBillSelectionItem.Message; }
			set
			{
				var old = Message;
				if (old != value)
				{
					SplitBillSelectionItem.Message = value;
					MessageInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo MessageInfo => GetZPropertyInfo(nameof(Message));

		#endregion

		#region lookup

		public CodeDescriptionPairList BillOfLadingActionType => Chooser.Header.Factory.GetCachedValue<USExportBillOfLadingActionCodeType>();

		#endregion
	}
}
