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
	[GlowInterfaceReference("")]
	public class Pesticide : CusAddInfo<USPSTAddInfo>, IPSTData, ICusAddInfoTypeSupporter, Integration.Customs.US.IPesticide, IPGADataCorrection, ICanDelete, ICusDispositionParent
	{
		public Pesticide(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<USPSTAddInfo>.Schema
		{
			public const string US_LineNo = USPSTAddInfoSchema.Constants.US_LineNo;
			public const string US_IntendedUseCode = USPSTAddInfoSchema.Constants.US_IntendedUseCode;
			public const string US_IntendedUseDesc = USPSTAddInfoSchema.Constants.US_IntendedUseDesc;
			public const string US_ProductType = USPSTAddInfoSchema.Constants.US_ProductType;
			public const string US_UnregReasonCode = USPSTAddInfoSchema.Constants.US_UnregReasonCode;
			public const string US_UnregReasonRemarks = USPSTAddInfoSchema.Constants.US_UnregReasonRemarks;
			public const string US_BrandName = USPSTAddInfoSchema.Constants.US_BrandName;
			public const string US_LPCONumber = USPSTAddInfoSchema.Constants.US_LPCONumber;
			public const string US_ProducerEstNo = USPSTAddInfoSchema.Constants.US_ProducerEstNo;
			public const string US_ProducerEstNoForeign = USPSTAddInfoSchema.Constants.US_ProducerEstNoForeign;
			public const string US_OA_ExaminationLocation = USPSTAddInfoSchema.Constants.US_OA_ExaminationLocation;
			public const string US_NoOfUnit1 = USPSTAddInfoSchema.Constants.US_NoOfUnit1;
			public const string US_UQ1 = USPSTAddInfoSchema.Constants.US_UQ1;
			public const string US_NoOfUnit2 = USPSTAddInfoSchema.Constants.US_NoOfUnit2;
			public const string US_UQ2 = USPSTAddInfoSchema.Constants.US_UQ2;
			public const string US_NoOfUnit3 = USPSTAddInfoSchema.Constants.US_NoOfUnit3;
			public const string US_UQ3 = USPSTAddInfoSchema.Constants.US_UQ3;
			public const string US_NoOfUnit4 = USPSTAddInfoSchema.Constants.US_NoOfUnit4;
			public const string US_UQ4 = USPSTAddInfoSchema.Constants.US_UQ4;
			public const string US_NoOfUnit5 = USPSTAddInfoSchema.Constants.US_NoOfUnit5;
			public const string US_UQ5 = USPSTAddInfoSchema.Constants.US_UQ5;
			public const string US_NoOfUnit6 = USPSTAddInfoSchema.Constants.US_NoOfUnit6;
			public const string US_UQ6 = USPSTAddInfoSchema.Constants.US_UQ6;
			public const string US_NetWeight = USPSTAddInfoSchema.Constants.US_NetWeight;
			public const string US_WeightUQ = USPSTAddInfoSchema.Constants.US_WeightUQ;
			public const string US_CertifyingIndividual = USPSTAddInfoSchema.Constants.US_CertifyingIndividual;
			public const string US_NotifyParty = USPSTAddInfoSchema.Constants.US_NotifyParty;
			public const string US_CBIIndicator = USPSTAddInfoSchema.Constants.US_CBIIndicator;
			public const string US_ConfidentialityRemarks = USPSTAddInfoSchema.Constants.US_ConfidentialityRemarks;
			public const string US_PSTLabelsSent = USPSTAddInfoSchema.Constants.US_PSTLabelsSent;
			public const string US_OA_ShipperAddress = USPSTAddInfoSchema.Constants.US_OA_ShipperAddress;
			public const string ShipperOrgPK = "ShipperOrgPK";
			public const string ExaminationLocationOrgPK = "ExaminationLocationOrgPK";
			public const string US_TrackingStatus = USPSTAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#endregion

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.InvoiceHeader : null;
			}
		}

		public JobDeclaration Declaration
		{
			get { return InvoiceLine != null ? InvoiceLine.Declaration : null; }
		}

		#endregion

		#region AddInfo Properties

		public ZInt US_LineNo
		{
			get { return AddInfo.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						AddInfo.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		public bool US_LineNo_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo US_LineNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LineNo, x => AddInfo.US_LineNoInfo); }
		}

		public ZBool US_CBIIndicator
		{
			get { return AddInfo.US_CBIIndicator; }
			set { AddInfo.US_CBIIndicator = value; }
		}

		public ZPropertyInfo US_CBIIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CBIIndicator, x => AddInfo.US_CBIIndicatorInfo); }
		}

		public ZBool US_PSTLabelsSent
		{
			get { return AddInfo.US_PSTLabelsSent; }
			set { AddInfo.US_PSTLabelsSent = value; }
		}

		public ZPropertyInfo US_PSTLabelsSentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PSTLabelsSent, x => AddInfo.US_PSTLabelsSentInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.IntendedUseCodeList))]
		[ReadOnlyMember(nameof(US_IntendedUseCode_ReadOnly))]
		public ZString US_IntendedUseCode
		{
			get { return AddInfo.US_IntendedUseCode; }
			set
			{
				var oldValue = US_IntendedUseCode;
				AddInfo.US_IntendedUseCode = value;
				if (!IsCopying && oldValue != US_IntendedUseCode)
				{
					if (US_IntendedUseDesc_ReadOnly && !US_IntendedUseDesc.IsEmpty)
					{
						US_IntendedUseDesc = ZString.Empty;
					}
				}
			}
		}

		bool US_IntendedUseCode_ReadOnly
		{
			get { return US_ProductType != PSTProductTypeList.Codes.PS3; }
		}

		public ZPropertyInfo US_IntendedUseCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseCode, x => AddInfo.US_IntendedUseCodeInfo); }
		}

		public ZString US_IntendedUseDesc
		{
			get { return AddInfo.US_IntendedUseDesc; }
			set { AddInfo.US_IntendedUseDesc = value; }
		}

		public ZPropertyInfo US_IntendedUseDescInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseDesc, x => AddInfo.US_IntendedUseDescInfo); }
		}

		bool US_IntendedUseDesc_ReadOnly
		{
			get { return US_IntendedUseCode != IntendedUseCodesList.Codes.ForOtherUse; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.ProductTypeList))]
		public ZString US_ProductType
		{
			get { return AddInfo.US_ProductType; }
			set
			{
				var oldValue = US_ProductType;
				AddInfo.US_ProductType = value;

				if (!IsCopying && oldValue != value && US_ProductType != PSTProductTypeList.Codes.PS3)
				{
					US_IntendedUseCode = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo US_ProductTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductType, x => AddInfo.US_ProductTypeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.ReasonCodeList))]
		public ZString US_UnregReasonCode
		{
			get { return AddInfo.US_UnregReasonCode; }
			set { AddInfo.US_UnregReasonCode = value; }
		}

		public ZPropertyInfo US_UnregReasonCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnregReasonCode, x => AddInfo.US_UnregReasonCodeInfo); }
		}

		public ZString US_UnregReasonRemarks
		{
			get { return AddInfo.US_UnregReasonRemarks; }
			set { AddInfo.US_UnregReasonRemarks = value; }
		}

		public ZPropertyInfo US_UnregReasonRemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnregReasonRemarks, x => AddInfo.US_UnregReasonRemarksInfo); }
		}

		public ZString US_ConfidentialityRemarks
		{
			get { return AddInfo.US_ConfidentialityRemarks; }
			set { AddInfo.US_ConfidentialityRemarks = value; }
		}

		public ZPropertyInfo US_ConfidentialityRemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ConfidentialityRemarks, x => AddInfo.US_ConfidentialityRemarksInfo); }
		}

		public ZString US_BrandName
		{
			get { return AddInfo.US_BrandName; }
			set { AddInfo.US_BrandName = value; }
		}

		public ZPropertyInfo US_BrandNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BrandName, x => AddInfo.US_BrandNameInfo); }
		}

		public ZString US_LPCONumber
		{
			get { return AddInfo.US_LPCONumber; }
			set { AddInfo.US_LPCONumber = value; }
		}

		public ZPropertyInfo US_LPCONumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LPCONumber, x => AddInfo.US_LPCONumberInfo); }
		}

		public ZString US_ProducerEstNo
		{
			get { return AddInfo.US_ProducerEstNo; }
			set { AddInfo.US_ProducerEstNo = value; }
		}

		public ZPropertyInfo US_ProducerEstNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProducerEstNo, x => AddInfo.US_ProducerEstNoInfo); }
		}

		public ZString US_ProducerEstNoForeign
		{
			get { return AddInfo.US_ProducerEstNoForeign; }
			set { AddInfo.US_ProducerEstNoForeign = value; }
		}

		public ZPropertyInfo US_ProducerEstNoForeignInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProducerEstNoForeign, x => AddInfo.US_ProducerEstNoForeignInfo); }
		}

		public OrgHeaderWrapper ExaminationLocationWrapper
		{
			get { return OrgHeaderWrapper.New(ExaminationLocationAddress); }
		}

		#region Unit 1

		public ZDecimal US_NoOfUnit1
		{
			get { return AddInfo.US_NoOfUnit1; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_NoOfUnit1 = value;
			}
		}

		public ZPropertyInfo US_NoOfUnit1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoOfUnit1, x => AddInfo.US_NoOfUnit1Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.OuterPackingTypes))]
		public ZString US_UQ1
		{
			get { return AddInfo.US_UQ1; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_UQ1 = value;
			}
		}

		public ZPropertyInfo US_UQ1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ1, x => AddInfo.US_UQ1Info); }
		}

		#endregion

		#region Unit 2

		public ZDecimal US_NoOfUnit2
		{
			get { return AddInfo.US_NoOfUnit2; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_NoOfUnit2 = value;
			}
		}

		public ZPropertyInfo US_NoOfUnit2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoOfUnit2, x => AddInfo.US_NoOfUnit2Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.InnerPackingTypes))]
		public ZString US_UQ2
		{
			get { return AddInfo.US_UQ2; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_UQ2 = value;
			}
		}

		public ZPropertyInfo US_UQ2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ2, x => AddInfo.US_UQ2Info); }
		}

		#endregion

		#region Unit 3

		public ZDecimal US_NoOfUnit3
		{
			get { return AddInfo.US_NoOfUnit3; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_NoOfUnit3 = value;
			}
		}

		public ZPropertyInfo US_NoOfUnit3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoOfUnit3, x => AddInfo.US_NoOfUnit3Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.InnerPackingTypes))]
		public ZString US_UQ3
		{
			get { return AddInfo.US_UQ3; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_UQ3 = value;
			}
		}

		public ZPropertyInfo US_UQ3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ3, x => AddInfo.US_UQ3Info); }
		}

		#endregion

		#region Unit 4

		public ZDecimal US_NoOfUnit4
		{
			get { return AddInfo.US_NoOfUnit4; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_NoOfUnit4 = value;
			}
		}

		public ZPropertyInfo US_NoOfUnit4Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoOfUnit4, x => AddInfo.US_NoOfUnit4Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.InnerPackingTypes))]
		public ZString US_UQ4
		{
			get { return AddInfo.US_UQ4; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_UQ4 = value;
			}
		}

		public ZPropertyInfo US_UQ4Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ4, x => AddInfo.US_UQ4Info); }
		}

		#endregion

		#region Unit 5

		public ZDecimal US_NoOfUnit5
		{
			get { return AddInfo.US_NoOfUnit5; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_NoOfUnit5 = value;
			}
		}

		public ZPropertyInfo US_NoOfUnit5Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoOfUnit5, x => AddInfo.US_NoOfUnit5Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.InnerPackingTypes))]
		public ZString US_UQ5
		{
			get { return AddInfo.US_UQ5; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_UQ5 = value;
			}
		}

		public ZPropertyInfo US_UQ5Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ5, x => AddInfo.US_UQ5Info); }
		}

		#endregion

		#region Unit 6

		public ZDecimal US_NoOfUnit6
		{
			get { return AddInfo.US_NoOfUnit6; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_NoOfUnit6 = value;
			}
		}

		public ZPropertyInfo US_NoOfUnit6Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoOfUnit6, x => AddInfo.US_NoOfUnit6Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.InnerPackingTypes))]
		public ZString US_UQ6
		{
			get { return AddInfo.US_UQ6; }
			set
			{
				ReCalculateRunningQty();
				AddInfo.US_UQ6 = value;
			}
		}

		public ZPropertyInfo US_UQ6Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ6, x => AddInfo.US_UQ6Info); }
		}

		#endregion

		[MeasureUnit(Schema.US_WeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_NetWeight
		{
			get { return AddInfo.US_NetWeight; }
			set { AddInfo.US_NetWeight = value; }
		}

		public ZPropertyInfo US_NetWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeight, x => AddInfo.US_NetWeightInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.PSTWeightUQList))]
		public ZString US_WeightUQ
		{
			get { return AddInfo.US_WeightUQ; }
			set { AddInfo.US_WeightUQ = value; }
		}

		public ZPropertyInfo US_WeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_WeightUQ, x => AddInfo.US_WeightUQInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.PSTCertifyingIndividualList))]
		public ZString US_CertifyingIndividual
		{
			get { return AddInfo.US_CertifyingIndividual; }
			set { AddInfo.US_CertifyingIndividual = value; }
		}

		public ZPropertyInfo US_CertifyingIndividualInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertifyingIndividual, x => AddInfo.US_CertifyingIndividualInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.NotifyPartyList))]
		public ZString US_NotifyParty
		{
			get { return AddInfo.US_NotifyParty; }
			set { AddInfo.US_NotifyParty = value; }
		}

		public ZPropertyInfo US_NotifyPartyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NotifyParty, x => AddInfo.US_NotifyPartyInfo); }
		}

		[ChildEditable(true)]
		public PesticideLineCollection PesticideLines
		{
			get
			{
				if (pesticideLines == null)
				{
					pesticideLines = new PesticideLineCollection(this);
					pesticideLines.Load();
					RegisterEditableChildObject(pesticideLines);
				}
				return pesticideLines;
			}
		}
		PesticideLineCollection pesticideLines;

		#region US_OA_ShipperAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ShipperAddress_ZAddress
		{
			get
			{
				if (shipperAddress_ZAddress == null)
				{
					shipperAddress_ZAddress = GetNewUS_OA_ShipperAddress_ZAddress();
					shipperAddress_ZAddress.IsOrgVisible = true;
					shipperAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return shipperAddress_ZAddress;
			}
		}
		ZAddress shipperAddress_ZAddress;

		protected ZAddress GetNewUS_OA_ShipperAddress_ZAddress()
		{
			return new ZAddress(US_OA_ShipperAddressInfo);
		}

		[List(nameof(US_OA_ShipperAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ShipperAddress
		{
			get
			{
				var result = AddInfo.US_OA_ShipperAddress;
				var invoiceLine = InvoiceLine;
				var invoice = invoiceLine != null ? invoiceLine.InvoiceHeader : null;
				if (result.IsEmpty && invoice != null && invoice.Seller != null)
				{
					result = invoice.Seller.MainAddress.PK;
				}
				return result;
			}
			set
			{
				AddInfo.US_OA_ShipperAddress = value;
			}
		}

		public void RefreshUS_OA_ShipperAddress_ZAddress()
		{
			shipperAddress_ZAddress = null;
		}

		public ZPropertyInfo US_OA_ShipperAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_ShipperAddress, x => AddInfo.US_OA_ShipperAddressInfo); }
		}

		public OrgAddress ShipperAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ShipperAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.Organizations))]
		public ZGuid ShipperOrgPK
		{
			get { return US_OA_ShipperAddress_ZAddress.OrgPK; }
			set { US_OA_ShipperAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipperOrgPK, x => US_OA_ShipperAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_ExaminationLocation_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ExaminationLocation_ZAddress
		{
			get
			{
				if (examinationLocation_ZAddress == null)
				{
					examinationLocation_ZAddress = GetNewUS_OA_ExaminationLocation_ZAddress();
					examinationLocation_ZAddress.IsOrgVisible = true;
					examinationLocation_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return examinationLocation_ZAddress;
			}
		}
		ZAddress examinationLocation_ZAddress;

		protected ZAddress GetNewUS_OA_ExaminationLocation_ZAddress()
		{
			return new ZAddress(US_OA_ExaminationLocationInfo);
		}

		[List(nameof(US_OA_ExaminationLocation_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ExaminationLocation
		{
			get { return AddInfo.US_OA_ExaminationLocation; }
			set { AddInfo.US_OA_ExaminationLocation = value; }
		}

		public ZPropertyInfo US_OA_ExaminationLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_ExaminationLocation, x => AddInfo.US_OA_ExaminationLocationInfo); }
		}

		public OrgAddress ExaminationLocationAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ExaminationLocation); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTAddInfoLookups.Organizations))]
		public ZGuid ExaminationLocationOrgPK
		{
			get { return US_OA_ExaminationLocation_ZAddress.OrgPK; }
			set { US_OA_ExaminationLocation_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ExaminationLocationOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ExaminationLocationOrgPK, x => US_OA_ExaminationLocation_ZAddress.OrgPKInfo); }
		}

		#endregion

		[ReadOnly(true)]
		public ZString US_TrackingStatus
		{
			get { return AddInfo.US_TrackingStatus; }
			set { AddInfo.US_TrackingStatus = value; }
		}

		public ZPropertyInfo US_TrackingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TrackingStatus, x => AddInfo.US_TrackingStatusInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.Pesticide|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		#endregion

		#region Qty Running Total

		public ZString PSTQtyRunningTotal
		{
			get
			{
				if (!pstQtyRunningTotal.HasValue)
				{
					ZDecimal calculatedQty = GetCalculatedRunningBaseQty();
					pstQtyRunningTotal = ZString.Empty;

					var innermostUQ = US_UQ6 != ZString.Empty ? US_UQ6
										: US_UQ5 != ZString.Empty ? US_UQ5
											: US_UQ4 != ZString.Empty ? US_UQ4
												: US_UQ3 != ZString.Empty ? US_UQ3
													: US_UQ2 != ZString.Empty ? US_UQ2
														: US_UQ1;

					if (!calculatedQty.IsEmpty)
					{
						pstQtyRunningTotal = calculatedQty > maxDisplaySize ? "Large Qty - please check" : "Total " + calculatedQty.ToString(2) + " " + innermostUQ;
					}
				}

				return pstQtyRunningTotal.Value;
			}
		}
		ZString? pstQtyRunningTotal;
		readonly ZDecimal maxDisplaySize = 99999999999999.99m;

		void ReCalculateRunningQty()
		{
			pstQtyRunningTotal = null;
		}

		ZDecimal GetCalculatedRunningBaseQty()
		{
			ZDecimal result = 0m;

			if (US_NoOfUnit1 + US_NoOfUnit2 + US_NoOfUnit3 + US_NoOfUnit4 + US_NoOfUnit5 + US_NoOfUnit6 > 0)
			{
				try
				{
					result = GetQtyMultiplier(US_NoOfUnit1) * GetQtyMultiplier(US_NoOfUnit2) * GetQtyMultiplier(US_NoOfUnit3) * GetQtyMultiplier(US_NoOfUnit4) * GetQtyMultiplier(US_NoOfUnit5) * GetQtyMultiplier(US_NoOfUnit6);
				}
				catch (OverflowException)
				{
					ZDecimal forceQtyTooBigWarning = maxDisplaySize + 1;
					result = forceQtyTooBigWarning;
				}
			}

			return result;
		}

		ZDecimal GetQtyMultiplier(ZDecimal value)
		{
			return value == 0 ? 1 : value;
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "Pesticide"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (Pesticide)base.CloneInternal(args);
			result.US_PSTLabelsSent = ZBool.False;

			foreach (PesticideLine line in PesticideLines)
			{
				result.PesticideLines.Add((PesticideLine)line.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(PesticideLine), false)));
			}
			return result;
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			PesticideLines.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USPSTAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USPSTAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USPSTAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USPSTAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USPSTAddInfo fAddInfo;

		public void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		Integration.Customs.US.IPesticideAddInfo Integration.Customs.US.IPesticide.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IPSTData Members

		ZInt IPSTData.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IPSTData.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString IPSTData.IntendedUseDescription
		{
			get { return US_IntendedUseDesc; }
		}

		ZString IPSTData.ProductType
		{
			get { return US_ProductType; }
		}

		ZString IPSTData.ReasonCode
		{
			get { return US_UnregReasonCode; }
		}

		ZString IPSTData.ReasonRemarks
		{
			get { return US_UnregReasonRemarks; }
		}

		ZString IPSTData.BrandName
		{
			get { return US_BrandName; }
		}

		ZString IPSTData.LPCONumber
		{
			get { return US_LPCONumber; }
		}

		ZString IPSTData.ProducerEstNo
		{
			get { return US_ProducerEstNo; }
		}

		ZString IPSTData.ProducerEstForNo
		{
			get { return US_ProducerEstNoForeign; }
		}

		ICustomsBrokerDetails IPSTData.BrokerDetails
		{
			get { return InvoiceLine; }
		}

		IPGAContactDetails IPSTData.ImporterDetails
		{
			get { return Declaration.IORWrapper; }
		}

		IPGAContactDetails IPSTData.CarrierDetails
		{
			get { return OrgHeaderWrapper.New(Declaration.ShippingLine, OrgHeaderWrapper.GetAddressForPSTCarrier); }
		}

		IPGAContactDetails IPSTData.ShipperDetails
		{
			get
			{
				if (ShipperAddress != null)
				{
					return OrgHeaderWrapper.New(ShipperAddress);
				}
				return null;
			}
		}

		IPGAContactDetails IPSTData.ExaminationLocationDetails
		{
			get { return OrgHeaderWrapper.New(ExaminationLocationAddress); }
		}

		ZDecimal IPSTData.Quantity1
		{
			get { return US_NoOfUnit1; }
		}

		ZString IPSTData.UQ1
		{
			get { return US_UQ1; }
		}

		ZDecimal IPSTData.Quantity2
		{
			get { return US_NoOfUnit2; }
		}

		ZString IPSTData.UQ2
		{
			get { return US_UQ2; }
		}

		ZDecimal IPSTData.Quantity3
		{
			get { return US_NoOfUnit3; }
		}

		ZString IPSTData.UQ3
		{
			get { return US_UQ3; }
		}

		ZDecimal IPSTData.Quantity4
		{
			get { return US_NoOfUnit4; }
		}

		ZString IPSTData.UQ4
		{
			get { return US_UQ4; }
		}

		ZDecimal IPSTData.Quantity5
		{
			get { return US_NoOfUnit5; }
		}

		ZString IPSTData.UQ5
		{
			get { return US_UQ5; }
		}

		ZDecimal IPSTData.Quantity6
		{
			get { return US_NoOfUnit6; }
		}

		ZString IPSTData.UQ6
		{
			get { return US_UQ6; }
		}

		ZDecimal IPSTData.NetWeight
		{
			get { return US_NetWeight; }
		}

		ZString IPSTData.WeightUQ
		{
			get { return US_WeightUQ; }
		}

		IEnumerable<IPSTLine> IPSTData.Lines
		{
			get { return PesticideLines.Cast<IPSTLine>(); }
		}

		ZString IPSTData.CertifyingIndividual
		{
			get { return US_CertifyingIndividual; }
		}

		ZDate IPSTData.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_PSTSignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_PSTSignDate = value;
				}
			}
		}

		ZString IPSTData.DeclarationCertificate
		{
			get
			{
				return ((IPSTData)this).CertifySignatureDate.IsValid ? "Y" : "";
			}
		}

		ZString IPSTData.NotifyParty
		{
			get { return US_NotifyParty; }
		}

		ZBool IPSTData.ConfidentialInfoIncluded
		{
			get { return US_CBIIndicator; }
		}

		ZString IPSTData.ConfidentialityRemarks
		{
			get { return US_ConfidentialityRemarks; }
		}

		ZBool IPSTData.IsPSTLabelsSent
		{
			get { return US_PSTLabelsSent; }
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
			result.Add(CusAddInfoTypeAttribute.Codes.USPesticideLine, typeof(PesticideLine));
			return result;
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
			get { return suspendTrackingStatusChange || AddInfo.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_PSTIndicator };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_PSTDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_PSTDisclaimProgram };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.US_FDAContactName, JobComInvoiceHeader.Schema.US_FDAContactPhoneNo, JobComInvoiceHeader.Schema.US_FDAContactEmail };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.JE_OH_ShippingLine, JobDeclaration.Schema.US_FDAContactName, JobDeclaration.Schema.US_FDAContactPhoneNo, JobDeclaration.Schema.US_FDAContactEmail };
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

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(Pesticide pesticide)
				: base(pesticide)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.EPA; }
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
