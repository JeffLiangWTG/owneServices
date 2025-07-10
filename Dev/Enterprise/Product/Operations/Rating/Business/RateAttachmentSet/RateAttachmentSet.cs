using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[AllowAllObjectsToBeLoaded]
	[DebuggerDisplay("Attachment Set: {TS_AttachmentName}")]
	public class RateAttachmentSet : AutoRateAttachmentSet, IRateAttachment
	{
		public new class Schema : AutoRateAttachmentSet.Schema
		{
			public const string IsSetReadonly = "IsSetReadonly";
		}

		public RateAttachmentSet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TS_IsSystemDefined = false;
			TS_IsClientSpecific = false;
		}

		public override void Delete()
		{
			var attachments = Factory.Load<RateAttachment>(new ZQuery(RateAttachmentSchema.TA_TS, PK));

			foreach (var attachment in attachments)
			{
				attachment.Delete();
			}

			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("04ef1c9f-b62a-4a88-91f0-4519a1f26fdf", "{0} Quotation Document Attachment", TS_AttachmentNameMultilingual); }
		}

		#endregion

		#region Properties

		[List("Lookups.Companies")]
		[ReadOnlyMember(Schema.IsSetReadonly)]
		public override ZGuid TS_GC
		{
			get { return base.TS_GC; }
			set { base.TS_GC = value; }
		}

		[ReadOnly(true)]
		public override ZBool TS_IsSystemDefined
		{
			get { return base.TS_IsSystemDefined; }
			set
			{
				base.TS_IsSystemDefined = value;

				TS_AttachmentNameInfo.RefreshBinding();
				TS_SUInfo.RefreshBinding();
				TS_TemplateTypeInfo.RefreshBinding();
			}
		}

		[ReadOnly(true)]
		public override ZBool TS_IsClientSpecific
		{
			get { return base.TS_IsClientSpecific; }
			set
			{
				base.TS_IsClientSpecific = value;

				TS_AttachmentNameInfo.RefreshBinding();
				TS_SUInfo.RefreshBinding();
				TS_TemplateTypeInfo.RefreshBinding();
			}
		}

		public override ZBool TS_IsMandatory
		{
			get { return base.TS_IsMandatory; }
			set
			{
				base.TS_IsMandatory = value;
				if (TS_IsMandatory)
				{
					TS_IsDefault = true;
				}
			}
		}

		[ReadOnlyMember(Schema.IsSetReadonly)]
		[List("Lookups.TemplateTypes")]
		public override ZString TS_TemplateType
		{
			get { return base.TS_TemplateType; }
			set
			{
				base.TS_TemplateType = value;

				Validation.ValidateTS_Sequence();
				Validation.ValidateTS_IsMandatory();

				if (!IsCopying)
				{
					BumpSequenceNumber();
				}
			}
		}

		public override ZShort TS_Sequence
		{
			get { return base.TS_Sequence; }
			set
			{
				base.TS_Sequence = value;
				if (!IsCopying)
				{
					BumpSequenceNumber();
				}
			}
		}

		public ZBool TS_IsCoverPage
		{
			get { return TS_TemplateType == RatingConstants.DocTemplateTypes.CoverPage; }
		}
		public ZPropertyInfo TS_IsCoverPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TS_IsCoverPage)); }
		}

		public ZBool TS_IsStandardPricingPage
		{
			get
			{
				return TS_TemplateType == RatingConstants.DocTemplateTypes.StandardPricingPage ||
					TS_TemplateType == RatingConstants.DocTemplateTypes.TableFormatPricingPage;
			}
		}
		public ZPropertyInfo TS_IsStandardPricingPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TS_IsStandardPricingPage)); }
		}

		public ZBool TS_IsOneOffPricingPage
		{
			get { return TS_TemplateType == RatingConstants.DocTemplateTypes.OneOffPricingPage; }
		}
		public ZPropertyInfo TS_IsOneOffPricingPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TS_IsOneOffPricingPage)); }
		}

		public ZBool TS_IsTrailingPage
		{
			get { return TS_TemplateType == RatingConstants.DocTemplateTypes.TrailingPage; }
		}
		public ZPropertyInfo TS_IsTrailingPageInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TS_IsTrailingPage)); }
		}

		[TranslatableDataField(Schema.TableName, Schema.TS_AttachmentName, DataXmlFilePaths.Documents, Type = typeof(RateAttachmentSet), Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(Schema.IsSetReadonly)]
		public override ZString TS_AttachmentName
		{
			get => base.TS_AttachmentName;
			set => base.TS_AttachmentName = value;
		}

		public MultilingualString TS_AttachmentNameMultilingual => GetMultilingual(TS_AttachmentNameInfo);

		[ReadOnlyMember(Schema.IsSetReadonly)]
		[List("Lookups.Documents")]
		public override ZGuid TS_SU
		{
			get { return base.TS_SU; }
			set { base.TS_SU = value; }
		}

		public override StmMenuItem MenuItem
		{
			get { return Command; }
		}

		public DocumentCommand Command
		{
			get { return Factory.Load<DocumentCommand>(TS_SU); }
		}

		public bool IsSetReadonly
		{
			get { return TS_IsSystemDefined || TS_IsClientSpecific; }
		}

		#region Image

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		public bool IsImage
		{
			get
			{
				if (TS_IsTrailingPage && TS_AttachmentName.Contains((NoResString)"Trailing Page ")) // Hard-coded constant
				{
					var indexString = TS_AttachmentName.Substring(TS_AttachmentName.LastIndexOf(' ') + 1);
					return ZInt.CanParse(indexString);
				}
				else
				{
					return false;
				}
			}
		}

		public Image Image
		{
			get
			{
				var index = GetImageIndex();
				if (index >= 0 && index <= 9)
				{
					return Factory.GetCachedValue("QuoteTermsAndConditionsPage" + index,
						delegate
						{
							return Env.Registry.Rating.GetQuoteTermsAndConditionsPage(index);
						});
				}
				else
				{
					return NoPreviewImage;
				}
			}
		}

		public bool HasImage
		{
			get
			{
				var index = GetImageIndex();
				return Env.Registry.Rating.IsQuoteTermsAndConditionsPagesValueSet(index);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		int GetImageIndex()
		{
			return IsImage ? ZInt.Parse(TS_AttachmentName.Substring(TS_AttachmentName.LastIndexOf(' ') + 1)) - 1 : -1;
		}

#if DEBUG
		internal
#endif
 Image NoPreviewImage
		{
			get
			{
				if (fNoPreviewImage == null)
				{
					fNoPreviewImage = new Bitmap(352, 440);
					using (var imageGraphics = Graphics.FromImage(fNoPreviewImage))
					using (var font = new Font("Arial", 16))
					using (var brush = new SolidBrush(Color.Red))
					using (var drawFormat = new StringFormat())
					{
						drawFormat.Alignment = StringAlignment.Center;
						drawFormat.LineAlignment = StringAlignment.Center;
						imageGraphics.DrawString(Res.GetString("e87e4383-34ce-47e3-8f5c-8a83e0d33e7c", "No preview available for\r\n{0}", TS_AttachmentNameMultilingual), font, brush, new RectangleF(0, 0, 352, 440), drawFormat);
					}
				}

				return fNoPreviewImage;
			}
		}

		Image fNoPreviewImage;

		#endregion

		#endregion

		#region IRateAttachment Members

		ZInt IRateAttachment.Sequence
		{
			get { return TS_Sequence; }
		}

		ZString IRateAttachment.TemplateType
		{
			get { return TS_TemplateType; }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !TS_IsSystemDefined && !TS_IsClientSpecific; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("9aa11a72-ab4d-4583-bc1c-b4540275a179", "Cannot delete a system defined document."); }
		}

		#endregion

		#region Implementation

		void BumpSequenceNumber()
		{
			var filter = new ZQuery();
			filter.AddToFilter(RateAttachmentSetSchema.TS_Sequence, TS_Sequence);
			filter.AddToFilter(RateAttachmentSetSchema.PK, SQLComparisonOperator.NotEqual, PK);
			filter.AddToFilter(RateAttachmentSetSchema.TS_TemplateType, TS_TemplateType);

			var attachments = Factory.Load<RateAttachmentSet>(filter);

			foreach (var next in attachments)
			{
				next.TS_Sequence += (ZShort)attachments.Length;
			}
		}

		#endregion
	}
}

