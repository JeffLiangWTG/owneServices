using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public sealed class AIMMessageChooser : MessageChooser
	{
		public AIMMessageChooser(ASYCUDA.Business.AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, ZString messageType)
			: base(header, items, true)
		{
			MessageType = messageType;
		}

		public ZString MessageType { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RequestCode = AIMFreightStatusRequestCodes.Codes.RequestAllInformationForSingleBill;
		}

		public bool IsSending => MessageType == AIMMessageSubTypes.FRI || MessageType == AIMMessageSubTypes.FXI;

		public bool IsChanging => MessageType == AIMMessageSubTypes.FRC || MessageType == AIMMessageSubTypes.FXC;

		public bool IsCancelling => MessageType == AIMMessageSubTypes.FRX || MessageType == AIMMessageSubTypes.FXX;

		public bool IsChangeOrCancellation => IsChanging || IsCancelling;

		public bool IsFreightStatusQuery => MessageType == AIMMessageSubTypes.FSQ;

		public bool IsManifestMessage { get; set; }

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AIMMessageChooserLookups.ReasonList))]
		public ZString Reason
		{
			get { return reason; }
			set
			{
				if (reason != value)
				{
					CheckMaximumLength(ReasonInfo, value);
					reason = value;

					Validation.ValidateReason();
					ReasonInfo.RefreshBinding();
				}
			}
		}
		ZString reason;

		public ZPropertyInfo ReasonInfo { get { return GetZPropertyInfo(nameof(Reason)); } }

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AIMMessageChooserLookups.FreightStatusRequestCodeList))]
		public ZString RequestCode
		{
			get { return requestCode; }
			set
			{
				if (requestCode != value)
				{
					CheckMaximumLength(ReasonInfo, value);
					requestCode = value;

					Validation.ValidateRequestCode();
					RequestCodeInfo.RefreshBinding();
				}
			}
		}
		ZString requestCode;

		public ZPropertyInfo RequestCodeInfo { get { return GetZPropertyInfo(nameof(RequestCode)); } }

		#region Override

		public new AIMMessageChooserLookups Lookups => (AIMMessageChooserLookups)base.Lookups;

		public new AIMMessageChooserValidation Validation => (AIMMessageChooserValidation)base.Validation;

		protected override MessageChooserLookups GetNewLookups()
		{
			return new AIMMessageChooserLookups(Header.Factory, this);
		}

		protected override MessageChooserValidation GetNewValidation()
		{
			return new AIMMessageChooserValidation(this);
		}

		protected override MessageChooserItemCollection CreateNewMessageChooserItemCollection()
		{
			return new AIMMessageChooserItemCollection();
		}

		#endregion
	}
}
