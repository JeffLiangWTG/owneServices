using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class NHTSADocument : CusAddInfo<USNHTSADocumentAddInfo>, INHTSADocument
	{
		public NHTSADocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<USNHTSADocumentAddInfo>.Schema
		{
			public const string US_NHTDocumentType = USNHTSADocumentAddInfoSchema.Constants.US_NHTDocumentType;
			public const string US_NHTDocumentDesc = USNHTSADocumentAddInfoSchema.Constants.US_NHTDocumentDesc;
			public const string US_NHTDocumentOwner = USNHTSADocumentAddInfoSchema.Constants.US_NHTDocumentOwner;
		}

		#endregion

		#region Related

		public NHTSAHeader Header
		{
			get { return Factory.Load<NHTSAHeader>(B7_ParentID); }
		}

		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADocumentAddInfoLookups.DocumentTypes))]
		public ZString US_NHTDocumentType
		{
			get { return AddInfo.US_NHTDocumentType; }
			set { AddInfo.US_NHTDocumentType = value; }
		}

		public bool US_NHTDocumentType_ReadOnly
		{
			get { return ShouldDocumentTypeBeReadOnly; }
		}

		public ZPropertyInfo US_NHTDocumentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDocumentType, x => AddInfo.US_NHTDocumentTypeInfo); }
		}

		public ZString US_NHTDocumentDesc
		{
			get { return AddInfoLookups.DocumentTypes.GetDescriptionFromCode(US_NHTDocumentType); }
		}

		public bool US_NHTDocumentDesc_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo US_NHTDocumentDescInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDocumentDesc, x => AddInfo.US_NHTDocumentDescInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADocumentAddInfoLookups.OrganizationTypes))]
		public ZString US_NHTDocumentOwner
		{
			get { return AddInfo.US_NHTDocumentOwner; }
			set
			{
				var hasChanges = AddInfo.US_NHTDocumentOwner != value;
				AddInfo.US_NHTDocumentOwner = value;
				if (hasChanges && !IsCopying && Header != null)
				{
					var invoiceLine = Header.InvoiceLine;
					if (invoiceLine != null)
					{
						invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
					}
				}
			}
		}

		public ZPropertyInfo US_NHTDocumentOwnerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDocumentOwner, x => AddInfo.US_NHTDocumentOwnerInfo); }
		}

		#endregion

		#region New Properties

		ZBool IsHS7DeclarationForm
		{
			get { return US_NHTDocumentType == NHTSADocumentTypeList.Codes._946; }
		}

		ZBool HasMultipleHS7DeclarationDocument
		{
			get { return Header.NHTSADocuments.OfType<NHTSADocument>().Count(x => x.IsHS7DeclarationForm) > 1; }
		}

		public bool ShouldDocumentTypeBeReadOnly
		{
			get { return IsHS7DeclarationForm && !HasMultipleHS7DeclarationDocument; }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "NHTSADocument"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (NHTSADocument)base.CloneInternal(args);
			return result;
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && (!IsHS7DeclarationForm || HasMultipleHS7DeclarationDocument); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("CC66C07D-364A-4CF3-AA92-42B72A1649FE", "You can't delete HS-7 declaration form (Code 946), as it used to declare the lawful basis for the importation of a motor vehicle or equipment item."); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USNHTSADocumentAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USNHTSADocumentAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USNHTSADocumentAddInfo fAddInfo;

		protected void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		public USNHTSADocumentAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		#endregion

		#region INHTSADocument Members

		ZString INHTSADocument.DocumentType
		{
			get { return US_NHTDocumentType; }
		}

		ZString INHTSADocument.OwnerCode
		{
			get { return US_NHTDocumentOwner; }
		}

		#endregion
	}
}
