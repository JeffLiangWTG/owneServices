using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FWSHeader : AutoFWSHeader, ICusAddInfoTypeSupporter, IFWSHeader, IPGADataCorrection, ICanDelete, ICustomsBrokerDetails, IAESFWS, ICusDispositionParent
	{
		public FWSHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoFWSHeader.Schema
		{
			public const string FWSExporterOrgPK = "FWSExporterOrgPK";
			public const string FWSImporterOrgPK = "FWSImporterOrgPK";
			public const string US_ProcessingCodeDesc = "US_ProcessingCodeDesc";
			public const string US_ProductTypeDesc = "US_ProductTypeDesc";
			public const string US_HighSeaAreaDesc = "US_HighSeaAreaDesc";
			public const string US_HybridDesc = "US_HybridDesc";
			public const string US_WildlifeCategoryCodeDesc = "US_WildlifeCategoryCodeDesc";
			public const string US_WildlifeDescriptionCodeDesc = "US_WildlifeDescriptionCodeDesc";
			public const string US_WildlifeSourceDesc = "US_WildlifeSourceDesc";
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#region Flags

		public bool IsHybrid
		{
			get { return US_Hybrid == FWSHybridTypeList.Codes.Intergeneric || US_Hybrid == FWSHybridTypeList.Codes.Interspecific; }
		}

		public bool IsFromHighSeas
		{
			get { return US_SpeciesOrigin == FWSConstants.HighSeas; }
		}

		public ZBool IsExport
		{
			get
			{
				if (isExportCached == null)
				{
					isExportCached = new CachedProperty<ZBool>(Factory, () =>
					{
						var parent = Parent;
						return parent != null && ((parent.TablePrefix == JobComInvoiceLineSchema.Constants.Prefix && ((JobComInvoiceLine)parent).IsExport) || (parent.TablePrefix == CusClassPartPivotSchema.Constants.Prefix && ((CusClassPartPivot)parent).IsExportTariff));
					});
				}
				return isExportCached.Value;
			}
		}
		CachedProperty<ZBool> isExportCached;

		#endregion

		public JobComInvoiceLine InvoiceLine
		{
			get { return B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? Parent as JobComInvoiceLine : null; }
		}

		public CusClassPartPivot Pivot
		{
			get { return B7_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix ? Parent as CusClassPartPivot : null; }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.InvoiceHeader : null;
			}
		}

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		#region Override Properties

		#region US_LineNo

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_LineNo", Caption = "Line No.")]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						base.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}

		#endregion

		#region US_IsDocSubmitted

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_IsDocSubmitted", Caption = "Is Doc Submitted?")]
		public override ZBool US_IsDocSubmitted
		{
			get { return base.US_IsDocSubmitted; }
			set { base.US_IsDocSubmitted = value; }
		}

		#endregion

		#region US_ProcessingCode

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ProcessingCode", Caption = "Processing Code")]
		public override ZString US_ProcessingCode
		{
			get { return base.US_ProcessingCode; }
			set
			{
				var hasChange = US_ProcessingCode != value;
				base.US_ProcessingCode = value;
				if (hasChange && !IsCopying)
				{
					if (InvoiceLine != null)
					{
						InvoiceLine.RefreshInvoiceLinesWithPGAIndicators();
					}

					if (FWSProcessingCodeList.IsLDS(US_ProcessingCode))
					{
						US_InvCurrPGAValue = ZDecimal.Zero;
					}
				}
			}
		}

		#endregion

		#region US_ProcessingCodeDesc

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ProcessingCodeDesc", Caption = "Processing Code Description")]
		public ZString US_ProcessingCodeDesc
		{
			get { return AddInfoLookups.ProcessingCodes.GetDescriptionFromCode(US_ProcessingCode); }
		}

		public ZPropertyInfo US_ProcessingCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProcessingCodeDesc); }
		}

		#endregion

		#region US_ProductType

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ProductType", Caption = "GUP ID Type")]
		public override ZString US_ProductType
		{
			get { return base.US_ProductType; }
			set
			{
				var oldValue = US_ProductType;
				base.US_ProductType = value;
				if (!IsCopying && oldValue != US_ProductType && US_ProductType.IsEmpty)
				{
					if (!US_ProductNumber.IsEmpty && US_ProductNumber_ReadOnly)
					{
						US_ProductNumber = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region US_ProductTypeDesc

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ProductTypeDesc", Caption = "GUP ID Type Description")]
		public ZString US_ProductTypeDesc
		{
			get { return AddInfoLookups.ProductTypes.GetDescriptionFromCode(US_ProductType); }
		}

		public ZPropertyInfo US_ProductTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProductTypeDesc); }
		}

		#endregion

		#region US_ProductNumber

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ProductNumber", Caption = "GUP ID")]
		public override ZString US_ProductNumber
		{
			get { return base.US_ProductNumber; }
			set { base.US_ProductNumber = value; }
		}

		bool US_ProductNumber_ReadOnly
		{
			get { return US_ProductType.IsEmpty; }
		}

		#endregion

		#region US_TaxonomicSerialNumber

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_TaxonomicSerialNumber", Caption = "Taxonomic Serial Number")]
		public override ZString US_TaxonomicSerialNumber
		{
			get { return base.US_TaxonomicSerialNumber; }
			set { base.US_TaxonomicSerialNumber = value; }
		}

		#endregion

		#region US_SpeciesOrigin

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_SpeciesOrigin", Caption = "Species Origin")]
		public override ZString US_SpeciesOrigin
		{
			get { return base.US_SpeciesOrigin; }
			set
			{
				var oldValue = US_SpeciesOrigin;
				base.US_SpeciesOrigin = value;
				if (!IsCopying && oldValue != US_SpeciesOrigin)
				{
					if (US_SpeciesOrigin != Core.Constants.CountryCodes.UnitedStates)
					{
						US_USState = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region US_Hybrid

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_Hybrid", Caption = "Hybrid")]
		public override ZString US_Hybrid
		{
			get { return base.US_Hybrid; }
			set
			{
				var oldValue = US_Hybrid;
				base.US_Hybrid = value;
				if (!IsCopying && oldValue != US_Hybrid && US_Hybrid.IsEmpty)
				{
					if (!US_Scientific2GenusName.IsEmpty && US_Scientific2GenusName_ReadOnly)
					{
						US_Scientific2GenusName = ZString.Empty;
					}
					if (!US_Scientific2SpeciesName.IsEmpty && US_Scientific2SpeciesName_ReadOnly)
					{
						US_Scientific2SpeciesName = ZString.Empty;
					}
					if (!US_Scientific2SubSpeciesName.IsEmpty && US_Scientific2SubSpeciesName_ReadOnly)
					{
						US_Scientific2SubSpeciesName = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region US_HybridDesc

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_HybridDesc", Caption = "Hybrid Description")]
		public ZString US_HybridDesc
		{
			get { return AddInfoLookups.HybridTypes.GetDescriptionFromCode(US_Hybrid); }
		}

		public ZPropertyInfo US_HybridDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_HybridDesc); }
		}

		#endregion

		#region US_ScientificGenusName

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ScientificGenusName", Caption = "Scientific Genus Name", MediumCaption = "Genus Name")]
		public override ZString US_ScientificGenusName
		{
			get { return base.US_ScientificGenusName; }
			set { base.US_ScientificGenusName = value; }
		}

		#endregion

		#region US_ScientificSpeciesName

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ScientificSpeciesName", Caption = "Scientific Species Name", MediumCaption = "Species Name")]
		public override ZString US_ScientificSpeciesName
		{
			get { return base.US_ScientificSpeciesName; }
			set { base.US_ScientificSpeciesName = value; }
		}

		#endregion

		#region US_Scientific2GenusName

		[ReadOnlyMember(nameof(US_Scientific2GenusName_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_Scientific2GenusName", Caption = "2nd Scientific Genus Name", MediumCaption = "2nd Genus Name")]
		public override ZString US_Scientific2GenusName
		{
			get { return base.US_Scientific2GenusName; }
			set { base.US_Scientific2GenusName = value; }
		}

		bool US_Scientific2GenusName_ReadOnly
		{
			get { return !IsHybrid; }
		}

		#endregion

		#region US_Scientific2SpeciesName

		[ReadOnlyMember(nameof(US_Scientific2SpeciesName_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_Scientific2SpeciesName", Caption = "2nd Scientific Species Name", MediumCaption = "2nd Species Name")]
		public override ZString US_Scientific2SpeciesName
		{
			get { return base.US_Scientific2SpeciesName; }
			set { base.US_Scientific2SpeciesName = value; }
		}

		bool US_Scientific2SpeciesName_ReadOnly
		{
			get { return !IsHybrid; }
		}

		#endregion

		#region US_Scientific2SubSpeciesName

		[ReadOnlyMember(nameof(US_Scientific2SubSpeciesName_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_Scientific2SubSpeciesName", Caption = "Sub Species", MediumCaption = "Sub Species")]
		public override ZString US_Scientific2SubSpeciesName
		{
			get { return base.US_Scientific2SubSpeciesName; }
			set { base.US_Scientific2SubSpeciesName = value; }
		}

		bool US_Scientific2SubSpeciesName_ReadOnly
		{
			get { return !IsHybrid; }
		}

		#endregion

		#region US_ScientificSubSpeciesName

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ScientificSubSpeciesName", Caption = "Sub Species", MediumCaption = "Sub Species")]
		public override ZString US_ScientificSubSpeciesName
		{
			get { return base.US_ScientificSubSpeciesName; }
			set { base.US_ScientificSubSpeciesName = value; }
		}

		#endregion

		#region US_WildlifeCategoryCode

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_WildlifeCategoryCode", Caption = "Wildlife Category Code", MediumCaption = "Wildlife Cat.")]
		public override ZString US_WildlifeCategoryCode
		{
			get { return base.US_WildlifeCategoryCode; }
			set { base.US_WildlifeCategoryCode = value; }
		}

		#endregion

		#region US_WildlifeCategoryCodeDesc

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_WildlifeCategoryCodeDesc", Caption = "Wildlife Category Code Description")]
		public ZString US_WildlifeCategoryCodeDesc
		{
			get { return AddInfoLookups.WildlifeCategoryCodes.GetDescriptionFromCode(US_WildlifeCategoryCode); }
		}

		public ZPropertyInfo US_WildlifeCategoryCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_WildlifeCategoryCodeDesc); }
		}

		#endregion

		#region US_WildlifeDescriptionCode

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_WildlifeDescriptionCode", Caption = "Wildlife Description Code", MediumCaption = "Wildlife Desc.")]
		public override ZString US_WildlifeDescriptionCode
		{
			get { return base.US_WildlifeDescriptionCode; }
			set { base.US_WildlifeDescriptionCode = value; }
		}

		#endregion

		#region US_WildlifeDescriptionCodeDesc

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_WildlifeDescriptionCodeDesc", Caption = "Wildlife Description Code Description")]
		public ZString US_WildlifeDescriptionCodeDesc
		{
			get { return AddInfoLookups.WildlifeDescriptionCodes.GetDescriptionFromCode(US_WildlifeDescriptionCode); }
		}

		public ZPropertyInfo US_WildlifeDescriptionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_WildlifeDescriptionCodeDesc); }
		}

		#endregion

		#region US_CommoditySpecificName

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_CommoditySpecificName", Caption = "Commodity Specific Name", MediumCaption = "Commodity Name")]
		public override ZString US_CommoditySpecificName
		{
			get { return base.US_CommoditySpecificName; }
			set { base.US_CommoditySpecificName = value; }
		}

		#endregion

		#region US_CommodityGeneralName

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_CommodityGeneralName", Caption = "Commodity General Name", MediumCaption = "General Name")]
		public override ZString US_CommodityGeneralName
		{
			get { return base.US_CommodityGeneralName; }
			set { base.US_CommodityGeneralName = value; }
		}

		#endregion

		#region US_IsLiveVenomous

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_IsLiveVenomous", Caption = "Is Live Venomous", MediumCaption = "Is Venomous")]
		public override ZBool US_IsLiveVenomous
		{
			get { return base.US_IsLiveVenomous; }
			set { base.US_IsLiveVenomous = value; }
		}

		#endregion

		#region US_CartonQty

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_CartonQty", Caption = "Carton Qty", MediumCaption = "Carton Qty")]
		public override ZShort US_CartonQty
		{
			get { return base.US_CartonQty; }
			set { base.US_CartonQty = value; }
		}

		#endregion

		#region US_OA_FWSImporterAddress

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_OA_FWSImporterAddress", Caption = "FWS Importer Address")]
		public override ZGuid US_OA_FWSImporterAddress
		{
			get { return base.US_OA_FWSImporterAddress; }
			set { base.US_OA_FWSImporterAddress = value; }
		}

		#endregion

		#region FWSImporterOrgPK
		[List(nameof(AddInfoLookups) + "." + nameof(USFWSHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|FWSImporterOrgPK", Caption = "FWS Importer")]
		public ZGuid FWSImporterOrgPK
		{
			get { return US_OA_FWSImporterAddress_ZAddress.OrgPK; }
			set { US_OA_FWSImporterAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo FWSImporterOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FWSImporterOrgPK, x => US_OA_FWSImporterAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region US_OA_FWSExporterAddress

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_OA_FWSExporterAddress", Caption = "FWS Exporter Address")]
		public override ZGuid US_OA_FWSExporterAddress
		{
			get { return base.US_OA_FWSExporterAddress; }
			set { base.US_OA_FWSExporterAddress = value; }
		}

		#endregion

		#region FWSExporterOrgPK
		[List(nameof(AddInfoLookups) + "." + nameof(USFWSHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|FWSExporterOrgPK", Caption = "FWS Exporter")]
		public ZGuid FWSExporterOrgPK
		{
			get { return US_OA_FWSExporterAddress_ZAddress.OrgPK; }
			set { US_OA_FWSExporterAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo FWSExporterOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FWSExporterOrgPK, x => US_OA_FWSExporterAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region US_TrackingStatus

		[ReadOnly(true)]
		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set { base.US_TrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_TrackingStatusDesc", Caption = "Message Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		#endregion

		#region US_NetCommodity

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_NetCommodity", Caption = "Net Commodity")]
		public override ZDecimal US_NetCommodity
		{
			get { return base.US_NetCommodity; }
			set { base.US_NetCommodity = value; }
		}

		#endregion

		#region US_NetCommodityUQ

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_NetCommodityUQ", Caption = "Net Commodity UQ")]
		public override ZString US_NetCommodityUQ
		{
			get { return base.US_NetCommodityUQ; }
			set { base.US_NetCommodityUQ = value; }
		}

		#endregion

		#region US_FIRMS

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_FIRMS", Caption = "Inspection Location")]
		public override ZString US_FIRMS
		{
			get { return base.US_FIRMS; }
			set { base.US_FIRMS = value; }
		}

		#endregion

		#region US_Value

		[ReadOnlyMember(nameof(US_Value_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_Value", Caption = "Value", FullDescription = "The value associated with the FWS data in whole dollars.")]
		public override ZDecimal US_Value
		{
			get { return base.US_Value; }
			set
			{
				try
				{
					suspendTrackingStatusChange = true;
					base.US_Value = value;
				}

				finally
				{
					suspendTrackingStatusChange = false;
				}
			}
		}
		bool suspendTrackingStatusChange;

		bool US_Value_ReadOnly
		{
			get { return true; }
		}

		public ZString PGAValue
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLine = InvoiceLine;

				if (invoiceLine == null || invoiceLine.IsOGAValueUpToDate)
				{
					result = US_Value.ToString(0);
				}
				else
				{
					result = "...";
				}

				return result;
			}
		}

		public ZPropertyInfo PGAValueInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PGAValue), (x) => US_ValueInfo); }
		}

		public override ZDecimal US_InvCurrPGAValue
		{
			get { return base.US_InvCurrPGAValue; }
			set
			{
				base.US_InvCurrPGAValue = value;

				var invoiceLine = InvoiceLine;

				if (invoiceLine != null && invoiceLine.Declaration != null)
				{
					invoiceLine.Declaration.MarkApportionmentDirty();
				}
			}
		}

		#endregion

		#region US_RemarksText

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_RemarksText", Caption = "Remarks", FullDescription = "Use to provide package marking/labeling information.")]
		public override ZString US_RemarksText
		{
			get { return base.US_RemarksText; }
			set { base.US_RemarksText = value; }
		}

		#endregion

		#region US_WildlifeSource

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_WildlifeSource", Caption = "Wildlife Source")]
		public override ZString US_WildlifeSource
		{
			get { return base.US_WildlifeSource; }
			set { base.US_WildlifeSource = value; }
		}

		#endregion

		#region US_WildlifeSourceDesc

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_WildlifeSourceDesc", Caption = "Wildlife Source Description")]
		public ZString US_WildlifeSourceDesc
		{
			get { return AddInfoLookups.WildlifeSources.GetDescriptionFromCode(US_WildlifeSource); }
		}

		public ZPropertyInfo US_WildlifeSourceDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_WildlifeSourceDesc); }
		}

		#endregion

		#region US_IntendedUseCode

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_IntendedUseCode", Caption = "Intended Use Code", MediumCaption = "Intended Use Code")]
		public override ZString US_IntendedUseCode
		{
			get { return base.US_IntendedUseCode; }
			set { base.US_IntendedUseCode = value; }
		}

		#endregion

		#region USState

		[ReadOnlyMember(nameof(US_USState_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_USState", Caption = "State", MediumCaption = "State")]
		public override ZString US_USState
		{
			get { return base.US_USState; }
			set { base.US_USState = value; }
		}

		bool US_USState_ReadOnly
		{
			get { return US_SpeciesOrigin != Core.Constants.CountryCodes.UnitedStates; }
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_PurposeCode", Caption = "Purpose Code")]
		public override ZString US_PurposeCode
		{
			get { return base.US_PurposeCode; }
			set { base.US_PurposeCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_CertificationCode", Caption = "Certification Code", FullDescription = "Exemption Certification Code", ShortCaption = "Cert. Code")]
		public override ZString US_CertificationCode
		{
			get { return base.US_CertificationCode; }
			set { base.US_CertificationCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FWSHeader|US_ConfirmationNum", Caption = "eDecs Confirmation", FullDescription = "eDecs Confirmation Number")]
		public override ZString US_ConfirmationNum
		{
			get { return base.US_ConfirmationNum; }
			set
			{
				try
				{
					suspendTrackingStatusChange = true;
					base.US_ConfirmationNum = value;
				}
				finally
				{
					suspendTrackingStatusChange = false;
				}
			}
		}

		#endregion

		#region New Properties

		public ZBool HasExportData
		{
			get
			{
				return !IsDeleted && (!US_ConfirmationNum.IsEmpty || !US_TaxonomicSerialNumber.IsEmpty || !US_PurposeCode.IsEmpty || !US_WildlifeDescriptionCode.IsEmpty || !US_WildlifeSource.IsEmpty
						|| !US_WildlifeCategoryCode.IsEmpty || !US_CertificationCode.IsEmpty || !US_SpeciesOrigin.IsEmpty || !US_USState.IsEmpty);
			}
		}

		#endregion

		#region Related Objects

		[ChildEditable(true)]
		public FWSLicenseCollection Licenses
		{
			get
			{
				if (licenses == null)
				{
					licenses = new FWSLicenseCollection(this);
					licenses.Load();
					RegisterEditableChildObject(licenses);
				}
				return licenses;
			}
		}
		FWSLicenseCollection licenses;

		#endregion

		#region Override Methods

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			Licenses.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		#region Implementation

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(FWSHeader header)
				: base(header)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		protected override bool IsDataEmpty
		{
			get
			{
				return base.IsDataEmpty &&
					Licenses.Count == 0;
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (FWSHeader)base.CloneInternal(args);
			result.US_LineNo = ZInt.Zero;
			foreach (FWSLicense license in Licenses)
			{
				result.Licenses.Add((FWSLicense)license.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(FWSLicense), false)));
			}
			return result;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USFWSLicense, typeof(FWSLicense));
			return result;
		}

		#endregion

		#region IFWSHeader Members

		ZInt IFWSHeader.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IFWSHeader.ProcessingCode
		{
			get { return US_ProcessingCode; }
		}

		ZBool IFWSHeader.IsDocSubmitted
		{
			get { return US_IsDocSubmitted; }
		}

		ZString IFWSHeader.ProductType
		{
			get { return US_ProductType; }
		}

		ZString IFWSHeader.ProductNumber
		{
			get { return US_ProductNumber; }
		}

		ZString IFWSHeader.ScientificGenusName
		{
			get { return US_ScientificGenusName; }
		}

		ZString IFWSHeader.ScientificSpeciesName
		{
			get { return US_ScientificSpeciesName; }
		}

		ZString IFWSHeader.ScientificSpeciesCode
		{
			get { return US_WildlifeCategoryCode; }
		}

		ZString IFWSHeader.FWSDescriptionCode
		{
			get { return US_WildlifeDescriptionCode; }
		}

		ZString IFWSHeader.Scientific2GenusName
		{
			get { return IsHybrid ? US_Scientific2GenusName : ZString.Empty; }
		}

		ZString IFWSHeader.Scientific2SpeciesName
		{
			get { return IsHybrid ? US_Scientific2SpeciesName : ZString.Empty; }
		}

		ZString IFWSHeader.SourceCountryCode
		{
			get { return US_SpeciesOrigin; }
		}

		ZString IFWSHeader.CommodityQualifierCode
		{
			get { return US_WildlifeSource; }
		}

		ZString IFWSHeader.Hybrid
		{
			get { return US_Hybrid; }
		}

		IEnumerable<IFWSLicense> IFWSHeader.Licenses
		{
			get { return Licenses.Cast<IFWSLicense>(); }
		}

		ZString IFWSHeader.CommoditySpecificName
		{
			get { return US_CommoditySpecificName; }
		}

		ZString IFWSHeader.ScientificSubSpeciesName
		{
			get { return US_ScientificSubSpeciesName; }
		}

		ZString IFWSHeader.Scientific2SubSpeciesName
		{
			get { return US_Scientific2SubSpeciesName; }
		}

		ZString IFWSHeader.CommodityGeneralName
		{
			get { return US_CommodityGeneralName; }
		}

		ZDate IFWSHeader.ArrivalDate
		{
			get
			{
				var result = ZDate.Empty;
				if (InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration)
				{
					result = declaration.JE_DateOfArrival.Date;
				}
				return result;
			}
		}

		ZString IFWSHeader.IsLiveVenomous
		{
			get { return US_IsLiveVenomous ? "Y" : "N"; }
		}

		ZShort IFWSHeader.CartonQty
		{
			get { return US_CartonQty; }
		}

		IPGAContactDetails IFWSHeader.FWSImporter
		{
			get { return OrgHeaderWrapper.New(FWSImporterAddress); }
		}

		public ZString FWSImporterFWE
		{
			get
			{
				return GetFWEFromOrganization(FWSImporterAddress?.Header);
			}
		}

		ZString GetFWEFromOrganization(OrgHeader orgHeader)
		{
			return orgHeader?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
		}

		IPGAContactDetails IFWSHeader.FWSForeignExporter
		{
			get { return OrgHeaderWrapper.New(FWSExporterAddress); }
		}

		ZString IFWSHeader.FWSForeignExporterDUNS
		{
			get
			{
				var exporterAddress = FWSExporterAddress;
				var exporter = exporterAddress == null ? null : exporterAddress.Header;
				return exporter == null ? ZString.Empty : exporter.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedStates);
			}
		}

		ZString IFWSHeader.DeclarationCode
		{
			get { return FWSProcessingCodeList.IsEDS(US_ProcessingCode) ? (ZString)DeclarationCodeList.Codes.FW3 : ZString.Empty; }
		}

		ZDate IFWSHeader.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_FWSSignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_FWSSignDate = value;
				}
			}
		}

		ZString IFWSHeader.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString IFWSHeader.RemarksText
		{
			get { return US_RemarksText; }
		}

		ZDecimal IFWSHeader.PGALineValue
		{
			get { return US_Value; }
		}

		IEnumerable<ZString> IFWSHeader.ContainerNumbers
		{
			get
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null)
				{
					foreach (var containerPivot in invoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>())
					{
						yield return containerPivot.ContainerNumber;
					}
				}
			}
		}

		ZString IFWSHeader.NetCommodityUQ
		{
			get { return US_NetCommodityUQ; }
		}

		ZDecimal IFWSHeader.NetCommodityQty
		{
			get { return US_NetCommodity; }
		}

		ZString IFWSHeader.FIRMS
		{
			get { return US_FIRMS; }
		}

		ICustomsBrokerDetails IFWSHeader.ContactDetails
		{
			get { return this; }
		}

		ICustomsBrokerDetails IFWSHeader.BrokerDetails
		{
			get { return FWSProcessingCodeList.IsEDS(US_ProcessingCode) ? InvoiceLine : null; }
		}

		ZString IFWSHeader.FilerAccountNumber
		{
			get
			{
				var result = ZString.Empty;

				if (InvoiceLine?.Declaration?.Branch is GlbBranch branch)
				{
					result = GetFWEFromOrganization(branch.OrgProxy);

					if (result.IsEmpty && branch.Company is GlbCompany company && branch.GB_OH_OrgProxy != company.GC_OH_OrgProxy)
					{
						result = GetFWEFromOrganization(company.OrgProxy);
					}
				}

				return result;
			}
		}

		#endregion

		#region ICustomsBrokerDetails Members

		IAddressDetails ICustomsBrokerDetails.Address
		{
			get { return ((IPGAContactDetails)OrgHeaderWrapper.New(FWSImporterAddress))?.CompanyAddress; }
		}

		ZString ICustomsBrokerDetails.ContactName
		{
			get { return ZString.Empty; }
		}

		ZString ICustomsBrokerDetails.ContactPhone
		{
			get { return ZString.Empty; }
		}

		ZString ICustomsBrokerDetails.ContactEmail
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IPGADataCorrection

		IPGADataCorrection PGADataCorrection
		{
			get { return this; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return suspendTrackingStatusChange || Data.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_FWSInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_FWSDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return new[] { CusContainer.Schema.CO_ContainerNumber };
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.JE_DateOfArrival };
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return PGADataCorrection.PGALinesCanBeDeleted(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return PGADataChangeTracker.ReasonForNotAbleToDelete; }
		}

		#endregion

		#region IAESFWS Members

		ZString IAESFWS.EDecsConfirmation
		{
			get { return US_ConfirmationNum; }
		}

		ZString IAESFWS.TaxonomicSerialNumber
		{
			get { return US_TaxonomicSerialNumber; }
		}

		ZString IAESFWS.PurposeCode
		{
			get { return US_PurposeCode; }
		}

		ZString IAESFWS.DescriptionCode
		{
			get { return US_WildlifeDescriptionCode; }
		}

		ZString IAESFWS.SpeciesOrigin
		{
			get { return US_SpeciesOrigin; }
		}

		ZString IAESFWS.SourceCode
		{
			get
			{
				var result = ZString.Empty;

				switch (US_WildlifeSource)
				{
					case FWSWildlifeSourceList.Codes.W:
					case FWSWildlifeSourceList.Codes.R:
					case FWSWildlifeSourceList.Codes.F:
					case FWSWildlifeSourceList.Codes.C:
					case FWSWildlifeSourceList.Codes.I:
					case FWSWildlifeSourceList.Codes.D:
					case FWSWildlifeSourceList.Codes.X:
						result = US_WildlifeSource;
						break;
					case FWSWildlifeSourceList.Codes.P2:
						result = "O";
						break;
					case FWSWildlifeSourceList.Codes.U6:
						result = "U";
						break;
					case FWSWildlifeSourceList.Codes.DOM:
						result = "J";
						break;
				}

				return result;
			}
		}

		ZString IAESFWS.ExemptionCertification
		{
			get { return US_CertificationCode; }
		}

		ZString IAESFWS.WildlifeCategoryCode
		{
			get { return US_WildlifeCategoryCode; }
		}

		ZString IAESFWS.StateCode
		{
			get { return US_USState; }
		}

		ZString IAESFWS.CommercialDescription
		{
			get { return InvoiceLine != null ? InvoiceLine.JI_Description.Left(70) : ZString.Empty; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.FWS; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString Status
		{
			get { return this.GetStatus(); }
		}

		public ZString StatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(Status); }
		}

		public ZDateTime StatusDate
		{
			get { return this.GetStatusDate(); }
		}

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}

		#endregion
	}
}
