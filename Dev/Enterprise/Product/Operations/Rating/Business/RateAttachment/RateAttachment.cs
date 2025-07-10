using System.Diagnostics;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay("Attachment: {CurrentAttachment.TS_AttachmentName}")]
	public class RateAttachment : AutoRateAttachment, IRateAttachment
	{
		public RateAttachment(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row) { }

		#region Schema

		public new abstract class Schema : AutoRateAttachment.Schema
		{
			public const string TA_RateAttachmentName = "TA_RateAttachmentName";
			public const string TA_IsMandatory = "TA_IsMandatory";
			public const string TA_IsDefault = "TA_IsDefault";
		}

		#endregion

		#region Current Attachment

		public RateAttachmentSet CurrentAttachment
		{
			get
			{
				if (TA_TS.IsValid && fCurrentAttachment == null)
				{
					fCurrentAttachment = Factory.Load<RateAttachmentSet>(TA_TS);
				}
				return fCurrentAttachment;
			}
		}

		RateAttachmentSet fCurrentAttachment;

		public RatingHeader Header
		{
			get { return Factory.Load<RatingHeader>(TA_TH); }
		}

		#endregion

		#region Properties

		#region TA_RateAttachmentName

		public ZString TA_RateAttachmentName => (CurrentAttachment != null) ? CurrentAttachment.TS_AttachmentNameMultilingual : ZString.Empty;

		public ZPropertyInfo TA_RateAttachmentNameInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(RateAttachment.Schema.TA_RateAttachmentName); }
		}

		#endregion

		#region TA_IsMandatory

		public ZBool TA_IsMandatory
		{
			get { return (CurrentAttachment != null) ? CurrentAttachment.TS_IsMandatory : ZBool.False; }
		}

		public ZPropertyInfo TA_IsMandatoryInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(RateAttachment.Schema.TA_IsMandatory); }
		}

		#endregion

		#region TA_IsDefault

		public ZBool TA_IsDefault
		{
			get { return (CurrentAttachment != null) ? CurrentAttachment.TS_IsDefault : ZBool.False; }
		}

		public ZPropertyInfo TA_IsDefaultInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(RateAttachment.Schema.TA_IsDefault); }
		}

		#endregion

		#region TA_IsCoverPage

		public ZBool TA_IsCoverPage
		{
			get { return (CurrentAttachment != null && CurrentAttachment.TS_IsCoverPage); }
		}

		public ZPropertyInfo TA_IsCoverPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TA_IsCoverPage)); }
		}

		#endregion

		#region TA_IsStandardPricingPage

		public ZBool TA_IsStandardPricingPage
		{
			get { return (CurrentAttachment != null && CurrentAttachment.TS_IsStandardPricingPage); }
		}

		public ZPropertyInfo TA_IsStandardPricingPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TA_IsStandardPricingPage)); }
		}

		#endregion

		#region TA_IsOneOffPricingPage

		public ZBool TA_IsOneOffPricingPage
		{
			get { return (CurrentAttachment != null && CurrentAttachment.TS_IsOneOffPricingPage); }
		}

		public ZPropertyInfo TA_IsOneOffPricingPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TA_IsOneOffPricingPage)); }
		}

		#endregion

		#region TA_IsTrailingPage

		public ZBool TA_IsTrailingPage
		{
			get { return (CurrentAttachment != null && CurrentAttachment.TS_IsTrailingPage); }
		}

		public ZPropertyInfo TA_IsTrailingPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TA_IsTrailingPage)); }
		}

		#endregion

		#region TA_TS

		[RelatedBusinessObject("CurrentAttachment")]
		public override ZGuid TA_TS
		{
			get { return base.TA_TS; }
			set { base.TA_TS = value; }
		}

		#endregion

		#region TA_TH

		[RelatedBusinessObject("Header")]
		public override ZGuid TA_TH
		{
			get { return base.TA_TH; }
			set { base.TA_TH = value; }
		}

		#endregion

		#region Image

		public bool IsImage
		{
			get { return CurrentAttachment?.IsImage ?? false; }
		}

		public Image Image
		{
			get { return CurrentAttachment?.Image; }
		}

		#endregion

		#endregion

		#region IRateAttachment Members

		ZInt IRateAttachment.Sequence
		{
			get { return ((IRateAttachment)CurrentAttachment).Sequence; }
		}

		ZString IRateAttachment.TemplateType
		{
			get { return ((IRateAttachment)CurrentAttachment).TemplateType; }
		}

		#endregion
	}
}

