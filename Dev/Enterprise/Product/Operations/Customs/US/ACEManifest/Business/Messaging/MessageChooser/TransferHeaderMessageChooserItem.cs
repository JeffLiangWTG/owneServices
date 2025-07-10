using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.AIM.Messaging;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class TransferHeaderMessageChooserItem : MessageChooserItem
	{
		public TransferHeaderMessageChooserItem(TransferHeaderMessageChooser chooser, ISelectionItem item, bool showStatus) : base(chooser, item, showStatus)
		{
		}

		TransferHeaderSelectionItem TransferHeaderSelectionItem => (TransferHeaderSelectionItem)BizO;
		public AsycudaTransferHeader TransferHeader => TransferHeaderSelectionItem.TransferHeader;
		AsycudaArrivalHeader ArrivalHeader => TransferHeader.ArrivalHeader;
		public new TransferHeaderMessageChooser Chooser => (TransferHeaderMessageChooser)base.Chooser;

		[ResourceStringData("34D6EC62-9F8F-495F-9253-D66D5607BCF7", Caption = "Send?")]
		public override ZBool Checked
		{
			get => base.Checked;
			set
			{
				base.Checked = value;
			}
		}

		public ZString VoyageFlightNo => ArrivalHeader.ATH_VoyageFlightNo;
		public ZString ArrivalReference => ArrivalHeader.ATH_Reference;
		public ZDateTime ETAAtDischargePort => ArrivalHeader.ATH_ETAAtDischargePort;

		public ZString DestinationPort => TransferHeader.ATF_RL_NKDestinationPortCode;
		public ZString TransferType => TransferHeader.ATF_TransferType;
		public ZString InBondCarrier => TransferHeader.InBondCarrierOrg?.OH_Code ?? ZString.Empty;
		public ZString InBondCarrierID => TransferHeader.ATF_CarrierID;
		public ZString OnwardCarrier => TransferHeader.ATF_OnwardCarrier;
		public ZString BondedPremises => TransferHeader.DestinationWarehouseOrg?.OH_Code ?? ZString.Empty;
		public ZString BondedPremisesID => TransferHeader.ATF_DestinationWarehouseID;

		[List(nameof(ArrivalStatusList))]
		public ZString StatusCode
		{
			get { return TransferHeaderSelectionItem.StatusCode; }
			set
			{
				var old = StatusCode;
				if (old != value)
				{
					TransferHeaderSelectionItem.StatusCode = value;
					StatusCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo StatusCodeInfo => GetZPropertyInfo(nameof(StatusCode));

		#region lookup
		public CodeDescriptionPairList ArrivalStatusList => TransferHeader.Factory.GetCachedValue<AIMArrivalStatusCodes>();
		#endregion

		#region Validation

		public new TransferHeaderMessageChooserItemValidation Validation => (TransferHeaderMessageChooserItemValidation)base.Validation;

		protected override MessageChooserItemValidation GetNewValidation() => new TransferHeaderMessageChooserItemValidation(this);

		#endregion
	}
}
