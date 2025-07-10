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
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.MasterFiles.Business.OrgConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[GlowInterfaceReference("")]
	[GlowDataDefinition("IUSACEFDA")]
	public class ACEFDA : CusAddInfo<USACEFDAAddInfo>,
		IDocAddresses,
		IACEPriorNoticeLine,
		Integration.Customs.US.IUSACEFDA,
		ICusAddInfoTypeSupporter,
		ICusCodeDataTypeSupporter,
		IPGADataCorrection,
		ICanDelete,
		ICusDispositionParent
	{
		public ACEFDA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<USACEFDAAddInfo>.Schema
		{
			public const string US_BrandName = USACEFDAAddInfoSchema.Constants.US_BrandName;
			public const string US_Description = USACEFDAAddInfoSchema.Constants.US_Description;
			public const string US_FDAForcePN = USACEFDAAddInfoSchema.Constants.US_FDAForcePN;
			public const string US_IntendedUseCode = USACEFDAAddInfoSchema.Constants.US_IntendedUseCode;
			public const string US_IntendedUseDescr = USACEFDAAddInfoSchema.Constants.US_IntendedUseDescr;
			public const string US_LineNo = USACEFDAAddInfoSchema.Constants.US_LineNo;
			public const string US_PNC = USACEFDAAddInfoSchema.Constants.US_PNC;
			public const string US_PND = USACEFDAAddInfoSchema.Constants.US_PND;
			public const string US_ProdCountry = USACEFDAAddInfoSchema.Constants.US_ProdCountry;
			public const string US_ProducerType = USACEFDAAddInfoSchema.Constants.US_ProducerType;
			public const string US_ProductCode = USACEFDAAddInfoSchema.Constants.US_ProductCode;
			public const string US_ProgramCode = USACEFDAAddInfoSchema.Constants.US_ProgramCode;
			public const string US_ProcessingCode = USACEFDAAddInfoSchema.Constants.US_ProcessingCode;
			public const string US_RefusedCountry = USACEFDAAddInfoSchema.Constants.US_RefusedCountry;
			public const string US_ShipmentCountry = USACEFDAAddInfoSchema.Constants.US_ShipmentCountry;
			public const string US_SourceCountry = USACEFDAAddInfoSchema.Constants.US_SourceCountry;
			public const string US_ManufacturerAddress = USACEFDAAddInfoSchema.Constants.US_ManufacturerAddress;
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string US_Qty1 = USACEFDAAddInfoSchema.Constants.US_Qty1;
			public const string US_UQ1 = USACEFDAAddInfoSchema.Constants.US_UQ1;
			public const string US_Qty2 = USACEFDAAddInfoSchema.Constants.US_Qty2;
			public const string US_UQ2 = USACEFDAAddInfoSchema.Constants.US_UQ2;
			public const string US_Qty3 = USACEFDAAddInfoSchema.Constants.US_Qty3;
			public const string US_UQ3 = USACEFDAAddInfoSchema.Constants.US_UQ3;
			public const string US_Qty4 = USACEFDAAddInfoSchema.Constants.US_Qty4;
			public const string US_UQ4 = USACEFDAAddInfoSchema.Constants.US_UQ4;
			public const string US_Qty5 = USACEFDAAddInfoSchema.Constants.US_Qty5;
			public const string US_UQ5 = USACEFDAAddInfoSchema.Constants.US_UQ5;
			public const string US_Qty6 = USACEFDAAddInfoSchema.Constants.US_Qty6;
			public const string US_UQ6 = USACEFDAAddInfoSchema.Constants.US_UQ6;
			public const string US_ContainerDimType = USACEFDAAddInfoSchema.Constants.US_ContainerDimType;
			public const string US_DimUQ = USACEFDAAddInfoSchema.Constants.US_DimUQ;
			public const string US_DeliverToPartyAddress = USACEFDAAddInfoSchema.Constants.US_DeliverToPartyAddress;
			public const string DeliverToPartyOrgPK = "DeliverToPartyOrgPK";
			public const string US_FDAImporterAddress = USACEFDAAddInfoSchema.Constants.US_FDAImporterAddress;
			public const string FDAImporterOrgPK = "FDAImporterOrgPK";
			public const string US_FSVPImporterAddress = USACEFDAAddInfoSchema.Constants.US_FSVPImporterAddress;
			public const string FSVPImporterOrgPK = "FSVPImporterOrgPK";
			public const string US_ProducerAddress = USACEFDAAddInfoSchema.Constants.US_ProducerAddress;
			public const string ProducerOrgPK = "ProducerOrgPK";
			public const string US_ItemIdentityNumber = USACEFDAAddInfoSchema.Constants.US_ItemIdentityNumber;
			public const string US_ItemIdentityNumberQualifier = USACEFDAAddInfoSchema.Constants.US_ItemIdentityNumberQualifier;
			public const string US_PackageTrackCode = USACEFDAAddInfoSchema.Constants.US_PackageTrackCode;
			public const string US_PackageTrackNumber = USACEFDAAddInfoSchema.Constants.US_PackageTrackNumber;
			public const string US_Remarks = USACEFDAAddInfoSchema.Constants.US_Remarks;
			public const string US_OwnerAddress = USACEFDAAddInfoSchema.Constants.US_OwnerAddress;
			public const string OwnerOrgPK = "OwnerOrgPK";
			public const string US_LocationOfGoodsAddress = USACEFDAAddInfoSchema.Constants.US_LocationOfGoodsAddress;
			public const string LocationOfGoodsOrgPK = "LocationOfGoodsOrgPK";
			public const string US_FME = USACEFDAAddInfoSchema.Constants.US_FME;
			public const string US_PFR = USFDAAddInfoSchema.Constants.US_PFR;
			public const string US_CanDim1 = USACEFDAAddInfoSchema.Constants.US_CanDim1;
			public const string US_CanDim1_16th = "US_CanDim1_16th";
			public const string US_CanDim1Inch = "US_CanDim1Inch";
			public const string US_CanDim2 = USACEFDAAddInfoSchema.Constants.US_CanDim2;
			public const string US_CanDim2_16th = "US_CanDim2_16th";
			public const string US_CanDim2Inch = "US_CanDim2Inch";
			public const string US_CanDim3 = USACEFDAAddInfoSchema.Constants.US_CanDim3;
			public const string US_CanDim3_16th = "US_CanDim3_16th";
			public const string US_CanDim3Inch = "US_CanDim3Inch";
			public const string US_OA_ShipperAddress = USACEFDAAddInfoSchema.Constants.US_OA_ShipperAddress;
			public const string ShipperOrgPK = "ShipperOrgPK";
			public const string US_InvCurrValue = USACEFDAAddInfoSchema.Constants.US_InvCurrValue;
			public const string US_TotalValue = USACEFDAAddInfoSchema.Constants.US_TotalValue;
			public const string US_TotalUSDValue = "US_TotalUSDValue";
			public const string US_UnitValue = USACEFDAAddInfoSchema.Constants.US_UnitValue;
			public const string US_TrackingStatus = USACEFDAAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
			public const string US_IsPNCFromMsg = USACEFDAAddInfoSchema.Constants.US_IsPNCFromMsg;
		}

		#endregion

		#region AddInfo Properties

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		internal bool FDAPriorNoticeAndAdmissibilityReviewMayBeRequired
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null &&
					(
						invoiceLine.ImportTariff != null && invoiceLine.ImportTariff.ACEFDAPriorNoticeAndAdmissibilityReviewMayBeRequired ||
						invoiceLine.ImportSupTariff != null && invoiceLine.ImportSupTariff.ACEFDAPriorNoticeAndAdmissibilityReviewMayBeRequired
					);
			}
		}

		bool FDAPriorNoticeAndAdmissibilityReviewRequired
		{
			get
			{
				return InvoiceLine != null &&
					(
						InvoiceLine.ImportTariff != null && InvoiceLine.ImportTariff.ACEFDAPriorNoticeAndAdmissibilityReviewRequired ||
						InvoiceLine.ImportSupTariff != null && InvoiceLine.ImportSupTariff.ACEFDAPriorNoticeAndAdmissibilityReviewRequired
					);
			}
		}

		#region Qty Running Total

		public ZString FDAQtyRunningTotal
		{
			get
			{
				if (!fdaQtyRunningTotal.HasValue)
				{
					var calculatedQty = GetCalculatedRunningBaseQty();
					fdaQtyRunningTotal = ZString.Empty;

					if (!calculatedQty.IsEmpty)
					{
						fdaQtyRunningTotal = calculatedQty > maxDisplaySize ? "Large Qty - please check" : "Total " + calculatedQty.ToString(2) + " " + US_UQ1;
					}
				}
				return fdaQtyRunningTotal.Value;
			}
		}
		ZString? fdaQtyRunningTotal;
		readonly ZDecimal maxDisplaySize = 99999999999999.99m;

		void ReCalculateRunningQty()
		{
			fdaQtyRunningTotal = null;
		}

		internal ZDecimal GetCalculatedRunningBaseQty()
		{
			var result = ZDecimal.Zero;

			if (US_Qty1 > 0)
			{
				try
				{
					result = GetQtyMultiplier(US_Qty1) * GetQtyMultiplier(US_Qty2) * GetQtyMultiplier(US_Qty3) * GetQtyMultiplier(US_Qty4) * GetQtyMultiplier(US_Qty5) * GetQtyMultiplier(US_Qty6);
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

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.ProgramCodeList))]
		public ZString US_ProgramCode
		{
			get { return AddInfo.US_ProgramCode; }
			set
			{
				var hasChanges = AddInfo.US_ProgramCode != value;
				AddInfo.US_ProgramCode = value;
				if (hasChanges && !IsCopying)
				{
					this.US_ItemIdentityNumber = ZString.Empty;
					this.US_ItemIdentityNumberQualifier = ZString.Empty;
					this.US_ProcessingCode = ZString.Empty;
					this.US_IntendedUseCode = ZString.Empty;

					if (US_ProgramCode == FDAProgramCodeList.Codes.FOO)
					{
						US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals;
					}

					if (US_ProducerType.IsEmpty && US_ProgramCode != FDAProgramCodeList.Codes.FOO && US_ProgramCode != FDAProgramCodeList.Codes.TOB)
					{
						US_ProducerType = ProducerFirmTypeList.Codes.M;
					}

					if (US_ProgramCode == FDAProgramCodeList.Codes.TOB && US_ProducerAddress.IsEmpty && InvoiceLine != null)
					{
						US_ProducerAddress = InvoiceLine.JI_OA_ManufacturerAddress;
					}

					if (US_ProgramCode != FDAProgramCodeList.Codes.TOB && US_PFR.IsEmpty)
					{
						SetFoodFacilityRegNumber();
					}

					if (US_ProgramCode != FDAProgramCodeList.Codes.TOB)
					{
						SetManufacturerDefaults();
					}

					DefaultShipperAddress();
					DefaultDeliverToPartyAddress();
					DefaultFSVPImporter();
					DefaultFDAImporterAddress();
					DefaultManufacturer();
					DefaultGoodsOwnerDocAddress();
					DefaultInitialImporterDocAddress();
					DefaultFDAIfRequiredForPriorNotice();
					DefaultAOCForDRU_804_080012();
				}

				DefaultAOCForVeterinary();
			}
		}

		public ZPropertyInfo US_ProgramCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProgramCode, x => AddInfo.US_ProgramCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.ProcessingCodeList))]
		public ZString US_ProcessingCode
		{
			get { return AddInfo.US_ProcessingCode; }
			set
			{
				var hasChanges = AddInfo.US_ProcessingCode != value;
				AddInfo.US_ProcessingCode = value;
				if (hasChanges && !IsCopying)
				{
					DefaultFSVPImporter();
					DefaultAOCForDRU_804_080012();
				}
			}
		}

		void DefaultAffirmationOfComplianceCode()
		{
			if (IsBiologics)
			{
				DefaultAOCForBiologic_ALG_BDP_CGT_VAC();
				DefaultAOCForBiologic_BLO_BLD();
				DefaultAOCForBiologic_BBA_PVE();
				DefaultAOCForBiologic_HCT();
				DefaultAOCForBiologic_BLD();
			}
			DefaultAOCForDRU_804_080012();
		}

		void DefaultAOCForDRU_804_080012()
		{
			if (US_ProgramCode == FDAProgramCodeList.Codes.DRU
				&& US_ProcessingCode == FDAProcessingCodeList.Codes.DRU_804
				&& US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._080012
				&& !AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.FSR))
			{
				var regNo = InvoiceLine?.Seller?.Header?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.USACodeTypes.FDAForeignSellerRegistrationNumber, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
				if (!regNo.IsEmpty)
				{
					AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.FSR, regNo);
				}
			}
		}

		void DefaultAOCForBiologic_BLD()
		{
			if (US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_BLD)
			{
				if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.IND) &&
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180009)
				{
					AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.IND;
				}
			}
		}

		void DefaultAOCForBiologic_ALG_BDP_CGT_VAC()
		{
			if (US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_ALG ||
				US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_BDP ||
				US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_CGT ||
				US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_VAC)
			{
				if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.IND) &&
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180009)
				{
					AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.IND;
				}

				if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.IFE) &&
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._970000)
				{
					AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
				}

				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._080000 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180016 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._155000 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150007)
				{
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.BLN))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
					}

					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.STN))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.STN;
					}
				}
			}
		}

		void DefaultAOCForBiologic_BLO_BLD()
		{
			if (US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_BLO ||
				US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_BLD)
			{
				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._080000 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180016 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._155000 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150007)
				{
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.BLN))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
					}
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.STN))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.STN;
					}
				}

				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._970000 &&
					!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.IFE))
				{
					AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
				}
			}
		}

		void DefaultAOCForBiologic_BBA_PVE()
		{
			if (US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_BBA ||
				US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_PVE)
			{
				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._180009)
				{
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.IND))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.IND;
					}
				}

				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._080000)
				{
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.DA))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.DA;
					}

					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.REG))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
					}
				}

				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._155000 ||
					US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._150007)
				{
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.BLN))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
					}

					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.STN))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.STN;
					}
				}

				if (US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._970000)
				{
					if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.IFE))
					{
						AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
					}
				}
			}
		}

		void DefaultAOCForBiologic_HCT()
		{
			if (US_ProcessingCode == FDAProcessingCodeList.Codes.BIO_HCT &&
				US_IntendedUseCode == FDAIntendedUseCodesHelper.Codes._082000)
			{
				if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.HCT))
				{
					AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.HCT;
				}

				if (!AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.HRN))
				{
					AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.HRN;
				}
			}
		}

		void DefaultAOCForVeterinary()
		{
			if (US_ProgramCode == FDAProgramCodeList.Codes.VME && !AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.REG))
			{
				AffirmationCodes.AddNew().CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			}
		}

		public ZPropertyInfo US_ProcessingCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProcessingCode, x => AddInfo.US_ProcessingCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.IntendedUseCodeList))]
		public ZString US_IntendedUseCode
		{
			get { return AddInfo.US_IntendedUseCode; }
			set
			{
				AddInfo.US_IntendedUseCode = value;
				DefaultAffirmationOfComplianceCode();
			}
		}

		public ZPropertyInfo US_IntendedUseCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseCode, x => AddInfo.US_IntendedUseCodeInfo); }
		}

		[MaxLength(21)]
		public ZString US_IntendedUseDescr
		{
			get { return AddInfo.US_IntendedUseDescr; }
			set { AddInfo.US_IntendedUseDescr = value; }
		}

		public ZPropertyInfo US_IntendedUseDescrInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseDescr, x => AddInfo.US_IntendedUseDescrInfo); }
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

		public ZString US_Description
		{
			get { return AddInfo.US_Description; }
			set { AddInfo.US_Description = value; }
		}

		public ZPropertyInfo US_DescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Description, x => AddInfo.US_DescriptionInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAProductsList))]
		public ZString US_ProductCode
		{
			get { return AddInfo.US_ProductCode; }
			set
			{
				var hasChanges = US_ProductCode != value;
				AddInfo.US_ProductCode = value;

				if (!IsCopying && hasChanges)
				{
					US_FDAForcePN = ShouldForcePriorNotice;
					DefaultFSVPImporter();
				}
			}
		}

		public ZPropertyInfo US_ProductCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductCode, x => AddInfo.US_ProductCodeInfo); }
		}

		public ZString US_PackageTrackCode
		{
			get { return AddInfo.US_PackageTrackCode; }
			set { AddInfo.US_PackageTrackCode = value; }
		}

		public ZPropertyInfo US_PackageTrackCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PackageTrackCode, x => AddInfo.US_PackageTrackCodeInfo); }
		}

		public ZString US_PackageTrackNumber
		{
			get { return AddInfo.US_PackageTrackNumber; }
			set { AddInfo.US_PackageTrackNumber = value; }
		}

		public ZPropertyInfo US_PackageTrackNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PackageTrackNumber, x => AddInfo.US_PackageTrackNumberInfo); }
		}

		public ZString US_Remarks
		{
			get { return AddInfo.US_Remarks; }
			set { AddInfo.US_Remarks = value; }
		}

		public ZPropertyInfo US_RemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Remarks, x => AddInfo.US_RemarksInfo); }
		}

		public ZBool US_FDAForcePN
		{
			get { return AddInfo.US_FDAForcePN; }
			set
			{
				var hasChanged = US_FDAForcePN != value;
				AddInfo.US_FDAForcePN = value;
				if (hasChanged && !IsCopying)
				{
					DefaultFSVPImporter();
					DefaultFDAIfRequiredForPriorNotice();
				}
			}
		}

		public ZPropertyInfo US_FDAForcePNInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FDAForcePN, x => AddInfo.US_FDAForcePNInfo); }
		}

		public ZString US_PNC
		{
			get { return AddInfo.US_PNC; }
			set
			{
				var oldValue = US_PNC;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						var usedToBePriorNotice = IsPriorNotice;
						AddInfo.US_PNC = value;
						US_IsPNCFromMsg = false;

						if (!US_PNC.IsEmpty && usedToBePriorNotice && !IsPriorNotice && US_ProgramCode == FDAProgramCodeList.Codes.FOO && US_ProducerType != ProducerFirmTypeList.Codes.M)
						{
							US_ProducerType = ProducerFirmTypeList.Codes.M;
						}
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}

		public ZPropertyInfo US_PNCInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PNC, x => AddInfo.US_PNCInfo); }
		}

		public ZBool US_PND
		{
			get { return AddInfo.US_PND; }
			set
			{
				var oldValue = US_PND;
				AddInfo.US_PND = value;
				if (oldValue != value && !IsCopying)
				{
					DefaultFDAIfRequiredForPriorNotice();
				}
			}
		}

		public ZPropertyInfo US_PNDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PND, x => AddInfo.US_PNDInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.ProducerFirmTypes))]
		public ZString US_ProducerType
		{
			get { return AddInfo.US_ProducerType; }
			set
			{
				var oldValue = US_ProducerType;
				var hasChanged = oldValue != value;
				AddInfo.US_ProducerType = value;
				if (hasChanged)
				{
					SetFoodFacilityRegNumber();
				}
			}
		}

		public ZPropertyInfo US_ProducerTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProducerType, x => AddInfo.US_ProducerTypeInfo); }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public ACEAffirmationCodeCollection AffirmationCodes
		{
			get
			{
				if (affirmationCodes == null)
				{
					affirmationCodes = new ACEAffirmationCodeCollection(this);
					affirmationCodes.Load();
					RegisterEditableChildObject(affirmationCodes);
				}

				return affirmationCodes;
			}
		}
		ACEAffirmationCodeCollection affirmationCodes;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public PG04ConstituentElementCollection ProductConstituentElements
		{
			get
			{
				if (productConstituentElements == null)
				{
					productConstituentElements = new PG04ConstituentElementCollection(this);
					productConstituentElements.Load();
					RegisterEditableChildObject(productConstituentElements);
				}
				return productConstituentElements;
			}
		}
		PG04ConstituentElementCollection productConstituentElements;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public LotCollection Lots
		{
			get
			{
				if (lots == null)
				{
					lots = new LotCollection(this);
					lots.Load();
					RegisterEditableChildObject(lots);
				}
				return lots;
			}
		}
		LotCollection lots;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public FDALicenseCollection Licenses
		{
			get
			{
				if (licenses == null)
				{
					licenses = new FDALicenseCollection(this);
					licenses.Load();
					RegisterEditableChildObject(licenses);
				}
				return licenses;
			}
		}
		FDALicenseCollection licenses;

		#region US_ManufacturerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_ManufacturerAddress_ZAddress
		{
			get
			{
				if (manufacturerAddress_ZAddress == null)
				{
					manufacturerAddress_ZAddress = GetNewUS_ManufacturerAddress_ZAddress();
					manufacturerAddress_ZAddress.IsOrgVisible = true;
					manufacturerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return manufacturerAddress_ZAddress;
			}
		}
		ZAddress manufacturerAddress_ZAddress;

		public void RefreshUS_ManufacturerAddress_ZAddress()
		{
			manufacturerAddress_ZAddress = null;
		}

		protected ZAddress GetNewUS_ManufacturerAddress_ZAddress()
		{
			return new ZAddress(US_ManufacturerAddressInfo);
		}

		[List(nameof(US_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_ManufacturerAddress
		{
			get
			{
				var result = AddInfo.US_ManufacturerAddress;
				var invoiceLine = InvoiceLine;
				if (result.IsEmpty && invoiceLine != null && US_ProgramCode != FDAProgramCodeList.Codes.TOB)//For TOB, this field is used as a laboratory and there is another field called Manufacturer
				{
					result = invoiceLine.JI_OA_ManufacturerAddress;
				}

				return result;
			}
			set
			{
				AddInfo.US_ManufacturerAddress = value;
				SetManufacturerDefaults();
			}
		}

		void SetManufacturerDefaults()
		{
			SetFoodFacilityRegNumber(true);
			if (ManufacturerWrapper != null)
			{
				if (US_ProducerType.IsEmpty)
				{
					US_ProducerType = ManufacturerWrapper.ZO_ProducerFirmType;
				}

				if (US_FME.IsEmpty)
				{
					US_FME = ManufacturerWrapper.ZO_MFRRegExempt;
				}
			}
		}

		internal void SetFoodFacilityRegNumber(bool ignoreEmptry = false)
		{
			if (ManufacturerAddress != null && InvoiceLine != null && (ignoreEmptry || US_PFR.IsEmpty))
			{
				US_PFR = ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, Core.Constants.CountryCodes.UnitedStates).Left(US_PFRInfo.MaxLength);
			}
		}

		public OrgCusCodeForFDA ManufacturerNumber
		{
			get { return OrgCusCodeForFDA.FindCustomsNumberAndID(EnableDocAddressForFDA ? ManufacturerDocAddress : ManufacturerAddress, US_ProgramCode); }
		}

		public ZPropertyInfo US_ManufacturerAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ManufacturerAddress, x => AddInfo.US_ManufacturerAddressInfo); }
		}

		public OrgAddress ManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(US_ManufacturerAddress); }
		}

		internal OrgHeaderWrapper ManufacturerWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (ManufacturerAddress != null)
				{
					result = OrgHeaderWrapper.New(ManufacturerAddress);
				}
				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Consignors))]
		public ZGuid ManufacturerOrgPK
		{
			get { return US_ManufacturerAddress_ZAddress.OrgPK; }
			set { US_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => US_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}

		public ACEFDAJobDocAddress ManufacturerDocAddress
		{
			get
			{
				if (manufacturerDocAddress == null || manufacturerDocAddress.IsDeleted || manufacturerDocAddress.E2_AddressType != DocAddressTypes.Codes.Manufacturer)
				{
					manufacturerDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
				}
				return manufacturerDocAddress;
			}
		}
		ACEFDAJobDocAddress manufacturerDocAddress;

		public ACEFDAJobDocAddress LaboratoryDocAddress
		{
			get
			{
				if (laboratoryDocAddress == null || laboratoryDocAddress.IsDeleted || laboratoryDocAddress.E2_AddressType != DocAddressTypes.Codes.Laboratory)
				{
					laboratoryDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.Laboratory);
				}
				return laboratoryDocAddress;
			}
		}
		ACEFDAJobDocAddress laboratoryDocAddress;

		#endregion

		#region US_DeliverToPartyAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_DeliverToPartyAddress_ZAddress
		{
			get
			{
				if (deliverToPartyAddress_ZAddress == null)
				{
					deliverToPartyAddress_ZAddress = GetNewUS_DeliverToPartyAddress_ZAddress();
					deliverToPartyAddress_ZAddress.IsOrgVisible = true;
					deliverToPartyAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return deliverToPartyAddress_ZAddress;
			}
		}
		ZAddress deliverToPartyAddress_ZAddress;

		protected ZAddress GetNewUS_DeliverToPartyAddress_ZAddress()
		{
			return new ZAddress(US_DeliverToPartyAddressInfo);
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.DeliverToPartyAddressList))]
		public ZGuid US_DeliverToPartyAddress
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => AddInfo.US_DeliverToPartyAddress.IsEmpty ? DeliverToPartyAddressToMatchAgainst : AddInfo.US_DeliverToPartyAddress,
					() => DeliverToPartyDocAddress,
					(x) => x.E2_OA_Address);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => AddInfo.US_DeliverToPartyAddress = value,
					() => DeliverToPartyDocAddress ?? (deliverToPartyDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.GoodsDeliveredTo)),
					(x) => x.E2_OA_Address = value);
			}
		}

		internal void RefreshDeliverToPartyZAddress()
		{
			deliverToPartyAddress_ZAddress = null;
		}

		ZGuid DeliverToPartyAddressToMatchAgainst
		{
			get
			{
				var result = ZGuid.Empty;
				if (InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.ShipToParty != null)
				{
					result = invoiceLine.JI_OA_ShipToPartyAddress;
				}
				return result;
			}
		}

		public OrgCusCodeForFDA DeliveryPartyNumber
		{
			get { return OrgCusCodeForFDA.FindCustomsNumberAndID(EnableDocAddressForFDA ? DeliverToPartyDocAddress : DeliverToPartyAddress, US_ProgramCode); }
		}

		public ZPropertyInfo US_DeliverToPartyAddressInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.US_DeliverToPartyAddress,
					x => GetDataFromZAddressOrJobDocAddress(
						() => AddInfo.US_DeliverToPartyAddressInfo,
						() => DeliverToPartyDocAddress,
						(x) => x.E2_OA_AddressInfo,
						() => GetZPropertyInfo(Schema.US_DeliverToPartyAddress)));
			}
		}

		public OrgAddress DeliverToPartyAddress
		{
			get { return Factory.Load<OrgAddress>(US_DeliverToPartyAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Consignees))]
		public ZGuid DeliverToPartyOrgPK
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => US_DeliverToPartyAddress_ZAddress.OrgPK,
					() => DeliverToPartyDocAddress,
					(x) => x.OrganisationPK);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => US_DeliverToPartyAddress_ZAddress.OrgPK = value,
					() => DeliverToPartyDocAddress ?? (deliverToPartyDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.GoodsDeliveredTo)),
					(x) => SetValueToOrgPKOfJobDocAddress(x, value));
			}
		}

		public ZPropertyInfo DeliverToPartyOrgPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.DeliverToPartyOrgPK,
					x => GetDataFromZAddressOrJobDocAddress(
						() => US_DeliverToPartyAddress_ZAddress.OrgPKInfo,
						() => DeliverToPartyDocAddress,
						(x) => x.OrganisationPKInfo,
						() => GetZPropertyInfo(Schema.DeliverToPartyOrgPK)));
			}
		}

		public ACEFDAJobDocAddress DeliverToPartyDocAddress
		{
			get
			{
				if (deliverToPartyDocAddress == null || deliverToPartyDocAddress.IsDeleted || deliverToPartyDocAddress.E2_AddressType != DocAddressTypes.Codes.GoodsDeliveredTo)
				{
					deliverToPartyDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.GoodsDeliveredTo);
				}
				return deliverToPartyDocAddress;
			}
		}
		ACEFDAJobDocAddress deliverToPartyDocAddress;

		void DefaultDeliverToPartyAddress()
		{
			if (US_DeliverToPartyAddress.IsEmpty)
			{
				var addressToMatchAgainst = DeliverToPartyAddressToMatchAgainst;
				if (addressToMatchAgainst.IsValid)
				{
					US_DeliverToPartyAddress = addressToMatchAgainst;
				}
			}
		}

		#endregion

		#region US_FDAImporterAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_FDAImporterAddress_ZAddress
		{
			get
			{
				if (fdaImporterAddress_ZAddress == null)
				{
					fdaImporterAddress_ZAddress = GetNewUS_FDAImporterAddress_ZAddress();
					fdaImporterAddress_ZAddress.IsOrgVisible = true;
					fdaImporterAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return fdaImporterAddress_ZAddress;
			}
		}
		ZAddress fdaImporterAddress_ZAddress;

		protected ZAddress GetNewUS_FDAImporterAddress_ZAddress()
		{
			return new ZAddress(US_FDAImporterAddressInfo);
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAImporterAddressList))]
		public ZGuid US_FDAImporterAddress
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => AddInfo.US_FDAImporterAddress,
					() => FDAImporterDocAddress,
					(x) => x.E2_OA_Address);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => AddInfo.US_FDAImporterAddress = value,
					() => FDAImporterDocAddress ?? (fdaImporterDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.ImporterDocumentaryAddress)),
					(x) => x.E2_OA_Address = value);
			}
		}

		ZGuid FDAImporterToMatchAgainst => InvoiceLine?.Declaration?.IOR?.PK ?? ZGuid.Empty;

		public OrgCusCodeForFDA FDAImporterNumber
		{
			get { return OrgCusCodeForFDA.FindCustomsNumberAndID(EnableDocAddressForFDA ? FDAImporterDocAddress : FDAImporterAddress, US_ProgramCode); }
		}

		public ZPropertyInfo US_FDAImporterAddressInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.US_FDAImporterAddress,
					x => GetDataFromZAddressOrJobDocAddress(
						() => AddInfo.US_FDAImporterAddressInfo,
						() => FDAImporterDocAddress,
						(x) => x.E2_OA_AddressInfo,
						() => GetZPropertyInfo(Schema.US_FDAImporterAddress)));
			}
		}

		public OrgAddress FDAImporterAddress
		{
			get { return Factory.Load<OrgAddress>(US_FDAImporterAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Organizations))]
		public ZGuid FDAImporterOrgPK
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => US_FDAImporterAddress_ZAddress.OrgPK,
					() => FDAImporterDocAddress,
					(x) => x.OrganisationPK);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => US_FDAImporterAddress_ZAddress.OrgPK = value,
					() => FDAImporterDocAddress ?? (fdaImporterDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.ImporterDocumentaryAddress)),
					(x) => SetValueToOrgPKOfJobDocAddress(x, value));
			}
		}

		public ZPropertyInfo FDAImporterOrgPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.FDAImporterOrgPK,
					x => GetDataFromZAddressOrJobDocAddress(
						() => US_FDAImporterAddress_ZAddress.OrgPKInfo,
						() => FDAImporterDocAddress,
						(x) => x.OrganisationPKInfo,
						() => GetZPropertyInfo(Schema.FDAImporterOrgPK)));
			}
		}

		public ACEFDAJobDocAddress FDAImporterDocAddress
		{
			get
			{
				if (fdaImporterDocAddress == null || fdaImporterDocAddress.IsDeleted || fdaImporterDocAddress.E2_AddressType != DocAddressTypes.Codes.ImporterDocumentaryAddress)
				{
					fdaImporterDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.ImporterDocumentaryAddress);
				}
				return fdaImporterDocAddress;
			}
		}
		ACEFDAJobDocAddress fdaImporterDocAddress;

		void DefaultFDAImporterAddress()
		{
			if (FDAImporterOrgPK.IsEmpty && !IsOverriddenJobDocAddress(() => FDAImporterDocAddress))
			{
				var orgPKToMatchAgainst = FDAImporterToMatchAgainst;
				if (orgPKToMatchAgainst.IsValid)
				{
					FDAImporterOrgPK = orgPKToMatchAgainst;
				}
			}
		}

		#endregion

		#region US_FSVPImporterAddress_ZAdress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_FSVPImporterAddress_ZAddress
		{
			get
			{
				if (fsvpImporterAddress_ZAddress == null)
				{
					fsvpImporterAddress_ZAddress = GetNewUS_FSVPImporterAddress_ZAddress();
					fsvpImporterAddress_ZAddress.IsOrgVisible = true;
					fsvpImporterAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return fsvpImporterAddress_ZAddress;
			}
		}
		ZAddress fsvpImporterAddress_ZAddress;

		protected ZAddress GetNewUS_FSVPImporterAddress_ZAddress()
		{
			return new ZAddress(US_FSVPImporterAddressInfo);
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FSVPImporterAddressList))]
		public ZGuid US_FSVPImporterAddress
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => AddInfo.US_FSVPImporterAddress,
					() => FSVPImporterDocAddress,
					(x) => x.E2_OA_Address);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => AddInfo.US_FSVPImporterAddress = value,
					() => FSVPImporterDocAddress ?? (fsvpImporterDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.FSVPImporter)),
					(x) => x.E2_OA_Address = value);
			}
		}

		ZGuid FSVPImporterToMatchAgainst
		{
			get
			{
				var result = ZGuid.Empty;
				if (IsFSVPImpRequired && !US_ProductCode.IsEmpty && !US_ProcessingCode.IsEmpty)
				{
					var importerOfRecord = InvoiceLine?.Declaration?.IOR;
					if (importerOfRecord != null && importerOfRecord.PK != OrgHeader.UnmatchedOrganisationPK &&
						importerOfRecord.Addresses.OfType<IAddressDetails>().Any(addressDetail => addressDetail.Country == Core.Constants.CountryCodes.UnitedStates))
					{
						result = importerOfRecord.PK;
					}
				}
				return result;
			}
		}

		public OrgCusCodeForFDA FSVPImporterNumber
		{
			get { return OrgCusCodeForFDA.FindCustomsNumberAndIDForFSVP(EnableDocAddressForFDA ? FSVPImporterDocAddress : FSVPImporterAddress); }
		}

		public ZPropertyInfo US_FSVPImporterAddressInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.US_FSVPImporterAddress,
					x => GetDataFromZAddressOrJobDocAddress(
						() => AddInfo.US_FSVPImporterAddressInfo,
						() => FSVPImporterDocAddress,
						(x) => x.E2_OA_AddressInfo,
						() => GetZPropertyInfo(Schema.US_FSVPImporterAddress)));
			}
		}

		public OrgAddress FSVPImporterAddress
		{
			get { return Factory.Load<OrgAddress>(US_FSVPImporterAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Organizations))]
		public ZGuid FSVPImporterOrgPK
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => US_FSVPImporterAddress_ZAddress.OrgPK,
					() => FSVPImporterDocAddress,
					(x) => x.OrganisationPK);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => US_FSVPImporterAddress_ZAddress.OrgPK = value,
					() => FSVPImporterDocAddress ?? (fsvpImporterDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.FSVPImporter)),
					(x) => SetValueToOrgPKOfJobDocAddress(x, value));
			}
		}

		public ZPropertyInfo FSVPImporterOrgPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.FSVPImporterOrgPK,
					x => GetDataFromZAddressOrJobDocAddress(
						() => US_FSVPImporterAddress_ZAddress.OrgPKInfo,
						() => FSVPImporterDocAddress,
						(x) => x.OrganisationPKInfo,
						() => GetZPropertyInfo(Schema.FSVPImporterOrgPK)));
			}
		}

		public ACEFDAJobDocAddress FSVPImporterDocAddress
		{
			get
			{
				if (fsvpImporterDocAddress == null || fsvpImporterDocAddress.IsDeleted || fsvpImporterDocAddress.E2_AddressType != DocAddressTypes.Codes.FSVPImporter)
				{
					fsvpImporterDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.FSVPImporter);
				}
				return fsvpImporterDocAddress;
			}
		}
		ACEFDAJobDocAddress fsvpImporterDocAddress;

		void DefaultFSVPImporter()
		{
			if (FSVPImporterOrgPK.IsEmpty && !IsOverriddenJobDocAddress(() => FSVPImporterDocAddress))
			{
				var orgPKToMatchAgainst = FSVPImporterToMatchAgainst;
				if (orgPKToMatchAgainst.IsValid)
				{
					FSVPImporterOrgPK = orgPKToMatchAgainst;
				}
			}
		}

		#endregion

		#region US_ProducerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_ProducerAddress_ZAddress
		{
			get
			{
				if (producerAddress_ZAddress == null)
				{
					producerAddress_ZAddress = GetNewUS_ProducerAddress_ZAddress();
					producerAddress_ZAddress.IsOrgVisible = true;
					producerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return producerAddress_ZAddress;
			}
		}
		ZAddress producerAddress_ZAddress;

		protected ZAddress GetNewUS_ProducerAddress_ZAddress()
		{
			return new ZAddress(US_ProducerAddressInfo);
		}

		[List(nameof(US_ProducerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[BusinessObjectTestExclude]
		public ZGuid US_ProducerAddress
		{
			get
			{
				var jobDocAddressSupplier = ProducerRelevantDocAddressSupplier;
				return jobDocAddressSupplier == null ? ZGuid.Empty : GetDataFromZAddressOrJobDocAddress(() => AddInfo.US_ProducerAddress, jobDocAddressSupplier, (x) => x.E2_OA_Address);
			}
			set
			{
				var jobDocAddressSupplier = ProducerRelevantDocAddressSupplierAndCreator;
				if (jobDocAddressSupplier == null)
				{
					AddInfo.US_ProducerAddress = value;
				}
				else
				{
					SetValueToZAddressOrJobDocAddress(() => AddInfo.US_ProducerAddress = value, jobDocAddressSupplier, (x) => x.E2_OA_Address = value);
				}
			}
		}

		Func<JobDocAddress> ProducerRelevantDocAddressSupplier => US_ProgramCode.ToString() switch
		{
			FDAProgramCodeList.Codes.DEV => () => InitialImporterDocAddress,
			FDAProgramCodeList.Codes.DRU => () => SponsorDocAddress,
			FDAProgramCodeList.Codes.TOB => () => ManufacturerDocAddress,
			_ => null
		};

		Func<JobDocAddress> ProducerRelevantDocAddressSupplierAndCreator => US_ProgramCode.ToString() switch
		{
			FDAProgramCodeList.Codes.DEV => () => InitialImporterDocAddress ?? (initialImporterDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.InitialImporter)),
			FDAProgramCodeList.Codes.DRU => () => SponsorDocAddress ?? (sponsorDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.Sponsor)),
			FDAProgramCodeList.Codes.TOB => () => ManufacturerDocAddress ?? (manufacturerDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer)),
			_ => null
		};

		public bool IsUS_ProducerAddressRelevant
		{
			get { return US_ProgramCode == FDAProgramCodeList.Codes.DEV || US_ProgramCode == FDAProgramCodeList.Codes.TOB || US_ProgramCode == FDAProgramCodeList.Codes.DRU; }
		}

		public OrgCusCodeForFDA ProducerNumber
		{
			get
			{
				IDocAddress address = null;
				if (EnableDocAddressForFDA)
				{
					var producerRelevantDocAddressSupplier = ProducerRelevantDocAddressSupplier;
					address = producerRelevantDocAddressSupplier == null ? null : producerRelevantDocAddressSupplier();
				}
				else
				{
					address = ProducerAddress;
				}
				return OrgCusCodeForFDA.FindCustomsNumberAndID(address, US_ProgramCode);
			}
		}

		public ZPropertyInfo US_ProducerAddressInfo
		{
			get
			{
				var jobDocAddressSupplier = ProducerRelevantDocAddressSupplier;
				if (jobDocAddressSupplier == null)
				{
					return GetWrappedZPropertyInfo(Schema.US_ProducerAddress, x => AddInfo.US_ProducerAddressInfo);
				}
				else
				{
					return GetWrappedZPropertyInfo(Schema.US_ProducerAddress, x => GetDataFromZAddressOrJobDocAddress(
					() => AddInfo.US_ProducerAddressInfo, ProducerRelevantDocAddressSupplierAndCreator, (x) => x.E2_OA_AddressInfo, () => GetZPropertyInfo(Schema.US_ProducerAddress)));
				}
			}
		}

		public OrgAddress ProducerAddress
		{
			get { return Factory.Load<OrgAddress>(US_ProducerAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Organizations))]
		public ZGuid ProducerOrgPK
		{
			get
			{
				var jobDocAddressSupplier = ProducerRelevantDocAddressSupplier;
				return jobDocAddressSupplier == null ? US_ProducerAddress_ZAddress.OrgPK : GetDataFromZAddressOrJobDocAddress(() => US_ProducerAddress_ZAddress.OrgPK, jobDocAddressSupplier, (x) => x.OrganisationPK);
			}
			set
			{
				var jobDocAddressSupplier = ProducerRelevantDocAddressSupplierAndCreator;
				if (jobDocAddressSupplier == null)
				{
					US_ProducerAddress_ZAddress.OrgPK = value;
				}
				else
				{
					SetValueToZAddressOrJobDocAddress(() => US_ProducerAddress_ZAddress.OrgPK = value, jobDocAddressSupplier, (x) => SetValueToOrgPKOfJobDocAddress(x, value));
				}
			}
		}

		public ZPropertyInfo ProducerOrgPKInfo
		{
			get
			{
				var jobDocAddressSupplier = ProducerRelevantDocAddressSupplier;
				if (jobDocAddressSupplier == null)
				{
					return GetWrappedZPropertyInfo(Schema.ProducerOrgPK, x => US_ProducerAddress_ZAddress.OrgPKInfo);
				}
				else
				{
					return GetWrappedZPropertyInfo(Schema.ProducerOrgPK, x => GetDataFromZAddressOrJobDocAddress(() => US_ProducerAddress_ZAddress.OrgPKInfo, jobDocAddressSupplier, (x) => x.OrganisationPKInfo, () => GetZPropertyInfo(Schema.ProducerOrgPK)));
				}
			}
		}

		#endregion

		#region InitialImporterDocAddress

		public ACEFDAJobDocAddress InitialImporterDocAddress
		{
			get
			{
				if (initialImporterDocAddress == null || initialImporterDocAddress.IsDeleted || initialImporterDocAddress.E2_AddressType != DocAddressTypes.Codes.InitialImporter)
				{
					initialImporterDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.InitialImporter);
				}
				return initialImporterDocAddress;
			}
		}
		ACEFDAJobDocAddress initialImporterDocAddress;

		void DefaultInitialImporterDocAddress()
		{
			if (US_ProducerAddress.IsEmpty && US_ProgramCode == FDAProgramCodeList.Codes.DEV && !IsOverriddenJobDocAddress(() => InitialImporterDocAddress))
			{
				var orgPKToMatchAgainst = InvoiceLine?.Declaration?.IOR?.PK ?? ZGuid.Empty;
				if (orgPKToMatchAgainst.IsValid)
				{
					ProducerOrgPK = orgPKToMatchAgainst;
				}
			}
		}

		#endregion

		#region SponsorDocAddress

		public ACEFDAJobDocAddress SponsorDocAddress
		{
			get
			{
				if (sponsorDocAddress == null || sponsorDocAddress.IsDeleted || sponsorDocAddress.E2_AddressType != DocAddressTypes.Codes.Sponsor)
				{
					sponsorDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.Sponsor);
				}
				return sponsorDocAddress;
			}
		}
		ACEFDAJobDocAddress sponsorDocAddress;

		#endregion

		#region US_OwnerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OwnerAddress_ZAddress
		{
			get
			{
				if (ownerAddress_ZAddress == null)
				{
					ownerAddress_ZAddress = GetNewUS_OwnerAddress_ZAddress();
					ownerAddress_ZAddress.IsOrgVisible = true;
					ownerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return ownerAddress_ZAddress;
			}
		}
		ZAddress ownerAddress_ZAddress;

		protected ZAddress GetNewUS_OwnerAddress_ZAddress()
		{
			return new ZAddress(US_OwnerAddressInfo);
		}

		[List(nameof(US_OwnerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OwnerAddress
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => AddInfo.US_OwnerAddress,
					() => GoodsOwnerDocAddress,
					(x) => x.E2_OA_Address);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => AddInfo.US_OwnerAddress = value,
					() => GoodsOwnerDocAddress ?? (goodsOwnerDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.GoodsOwner)),
					(x) => x.E2_OA_Address = value);
			}
		}

		public OrgCusCodeForFDA OwnerNumber
		{
			get { return OrgCusCodeForFDA.FindCustomsNumberAndID(EnableDocAddressForFDA ? GoodsOwnerDocAddress : OwnerAddress, US_ProgramCode); }
		}

		public ZPropertyInfo US_OwnerAddressInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.US_OwnerAddress,
					x => GetDataFromZAddressOrJobDocAddress(
						() => AddInfo.US_OwnerAddressInfo,
						() => GoodsOwnerDocAddress,
						(x) => x.E2_OA_AddressInfo,
						() => GetZPropertyInfo(Schema.US_OwnerAddress)));
			}
		}

		public OrgAddress OwnerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OwnerAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Organizations))]
		public ZGuid OwnerOrgPK
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => US_OwnerAddress_ZAddress.OrgPK,
					() => GoodsOwnerDocAddress,
					(x) => x.OrganisationPK);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => US_OwnerAddress_ZAddress.OrgPK = value,
					() => GoodsOwnerDocAddress ?? (goodsOwnerDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.GoodsOwner)),
					(x) => SetValueToOrgPKOfJobDocAddress(x, value));
			}
		}

		public ZPropertyInfo OwnerOrgPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.OwnerOrgPK,
					x => GetDataFromZAddressOrJobDocAddress(
						() => US_OwnerAddress_ZAddress.OrgPKInfo,
						() => GoodsOwnerDocAddress,
						(x) => x.OrganisationPKInfo,
						() => GetZPropertyInfo(Schema.OwnerOrgPK)));
			}
		}

		public ACEFDAJobDocAddress GoodsOwnerDocAddress
		{
			get
			{
				if (goodsOwnerDocAddress == null || goodsOwnerDocAddress.IsDeleted || goodsOwnerDocAddress.E2_AddressType != DocAddressTypes.Codes.GoodsOwner)
				{
					goodsOwnerDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.GoodsOwner);
				}
				return goodsOwnerDocAddress;
			}
		}
		ACEFDAJobDocAddress goodsOwnerDocAddress;

		void DefaultGoodsOwnerDocAddress()
		{
			if (US_OwnerAddress.IsEmpty && IsPriorNotice && !IsOverriddenJobDocAddress(() => GoodsOwnerDocAddress))
			{
				var orgPKToMatchAgainst = InvoiceLine?.Declaration?.IOR?.PK ?? ZGuid.Empty;
				if (orgPKToMatchAgainst.IsValid)
				{
					OwnerOrgPK = orgPKToMatchAgainst;
				}
			}
		}

		#endregion

		#region US_LocationOfGoodsAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_LocationOfGoodsAddress_ZAddress
		{
			get
			{
				if (locationOfGoodsAddress_ZAddress == null)
				{
					locationOfGoodsAddress_ZAddress = GetNewUS_LocationOfGoodsAddress_ZAddress();
					locationOfGoodsAddress_ZAddress.IsOrgVisible = true;
					locationOfGoodsAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return locationOfGoodsAddress_ZAddress;
			}
		}
		ZAddress locationOfGoodsAddress_ZAddress;

		protected ZAddress GetNewUS_LocationOfGoodsAddress_ZAddress()
		{
			return new ZAddress(US_LocationOfGoodsAddressInfo);
		}

		[List(nameof(US_LocationOfGoodsAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_LocationOfGoodsAddress
		{
			get { return AddInfo.US_LocationOfGoodsAddress; }
			set { AddInfo.US_LocationOfGoodsAddress = value; }
		}

		public OrgCusCodeForFDA LocationOfGoodsNumber
		{
			get { return OrgCusCodeForFDA.FindCustomsNumberAndID(EnableDocAddressForFDA ? LocationOfGoodsDocAddress : LocationOfGoodsAddress, US_ProgramCode); }
		}

		public ZPropertyInfo US_LocationOfGoodsAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LocationOfGoodsAddress, x => AddInfo.US_LocationOfGoodsAddressInfo); }
		}

		public OrgAddress LocationOfGoodsAddress
		{
			get { return Factory.Load<OrgAddress>(US_LocationOfGoodsAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Organizations))]
		public ZGuid LocationOfGoodsOrgPK
		{
			get { return US_LocationOfGoodsAddress_ZAddress.OrgPK; }
			set { US_LocationOfGoodsAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo LocationOfGoodsOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.LocationOfGoodsOrgPK, x => US_LocationOfGoodsAddress_ZAddress.OrgPKInfo); }
		}

		public ACEFDAJobDocAddress LocationOfGoodsDocAddress
		{
			get
			{
				if (locationOfGoodsDocAddress == null || locationOfGoodsDocAddress.IsDeleted || locationOfGoodsDocAddress.E2_AddressType != DocAddressTypes.Codes.GoodsLocation)
				{
					locationOfGoodsDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.GoodsLocation);
				}
				return locationOfGoodsDocAddress;
			}
		}
		ACEFDAJobDocAddress locationOfGoodsDocAddress;

		#endregion

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
					shipperAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return shipperAddress_ZAddress;
			}
		}
		ZAddress shipperAddress_ZAddress;

		protected ZAddress GetNewUS_OA_ShipperAddress_ZAddress()
		{
			return new ZAddress(US_OA_ShipperAddressInfo);
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.ShipperAddressList))]
		public ZGuid US_OA_ShipperAddress
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => AddInfo.US_OA_ShipperAddress.IsEmpty ? ShipperAddressToMatchAgainst : AddInfo.US_OA_ShipperAddress,
					() => ShipperDocAddress,
					(x) => x.E2_OA_Address);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => AddInfo.US_OA_ShipperAddress = value,
					() => ShipperDocAddress ?? (shipperDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.Shipper)),
					(x) => x.E2_OA_Address = value);
			}
		}

		ZGuid ShipperAddressToMatchAgainst => InvoiceLine?.InvoiceHeader?.SupplierAddress?.PK ?? ZGuid.Empty;

		public void RefreshUS_OA_ShipperAddress_ZAddress()
		{
			shipperAddress_ZAddress = null;
		}

		public ZPropertyInfo US_OA_ShipperAddressInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.US_OA_ShipperAddress,
					x => GetDataFromZAddressOrJobDocAddress(
						() => AddInfo.US_OA_ShipperAddressInfo,
						() => ShipperDocAddress,
						(x) => x.E2_OA_AddressInfo,
						() => GetZPropertyInfo(Schema.US_OA_ShipperAddress)));
			}
		}

		public OrgAddress ShipperAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ShipperAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.Organizations))]
		public ZGuid ShipperOrgPK
		{
			get
			{
				return GetDataFromZAddressOrJobDocAddress(
					() => US_OA_ShipperAddress_ZAddress.OrgPK,
					() => ShipperDocAddress,
					(x) => x.OrganisationPK);
			}
			set
			{
				SetValueToZAddressOrJobDocAddress(
					() => US_OA_ShipperAddress_ZAddress.OrgPK = value,
					() => ShipperDocAddress ?? (shipperDocAddress = DocAddresses.CreateWithAddressType(DocAddressType.Shipper)),
					(x) => SetValueToOrgPKOfJobDocAddress(x, value));
			}
		}

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.ShipperOrgPK,
					x => GetDataFromZAddressOrJobDocAddress(
						() => US_OA_ShipperAddress_ZAddress.OrgPKInfo,
						() => ShipperDocAddress,
						(x) => x.OrganisationPKInfo,
						() => GetZPropertyInfo(Schema.ShipperOrgPK)));
			}
		}

		public ACEFDAJobDocAddress ShipperDocAddress
		{
			get
			{
				if (shipperDocAddress == null || shipperDocAddress.IsDeleted || shipperDocAddress.E2_AddressType != DocAddressTypes.Codes.Shipper)
				{
					shipperDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.Shipper);
				}
				return shipperDocAddress;
			}
		}
		ACEFDAJobDocAddress shipperDocAddress;

		void DefaultShipperAddress()
		{
			if (US_OA_ShipperAddress.IsEmpty)
			{
				var addressToMatchAgainst = ShipperAddressToMatchAgainst;
				if (addressToMatchAgainst.IsValid)
				{
					US_OA_ShipperAddress = addressToMatchAgainst;
				}
			}
		}

		public void SetFDADefaultValueWhenCopyingFromProduct()
		{
			SetFoodFacilityRegNumber();

			DefaultShipperAddress();
			DefaultDeliverToPartyAddress();
			DefaultFSVPImporter();
			DefaultFDAImporterAddress();
			DefaultManufacturer();
			DefaultGoodsOwnerDocAddress();
			DefaultInitialImporterDocAddress();
		}

		#endregion

		public ZDecimal US_Qty1
		{
			get { return AddInfo.US_Qty1; }
			set
			{
				AddInfo.US_Qty1 = value;
				ReCalculateRunningQty();
			}
		}

		public ZPropertyInfo US_Qty1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Qty1, x => AddInfo.US_Qty1Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDABaseUQs))]
		public ZString US_UQ1
		{
			get { return AddInfo.US_UQ1; }
			set
			{
				AddInfo.US_UQ1 = value;
				ReCalculateRunningQty();
				SetAgainWhenUQChanged();
			}
		}

		public ZPropertyInfo US_UQ1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ1, x => AddInfo.US_UQ1Info); }
		}

		public ZDecimal US_Qty2
		{
			get { return AddInfo.US_Qty2; }
			set
			{
				AddInfo.US_Qty2 = value;
				ReCalculateRunningQty();
			}
		}

		public ZPropertyInfo US_Qty2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Qty2, x => AddInfo.US_Qty2Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAUQs))]
		public ZString US_UQ2
		{
			get { return AddInfo.US_UQ2; }
			set
			{
				AddInfo.US_UQ2 = value;
				ReCalculateRunningQty();
				SetAgainWhenUQChanged();
			}
		}

		public ZPropertyInfo US_UQ2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ2, x => AddInfo.US_UQ2Info); }
		}

		public ZDecimal US_Qty3
		{
			get { return AddInfo.US_Qty3; }
			set
			{
				AddInfo.US_Qty3 = value;
				ReCalculateRunningQty();
			}
		}

		public ZPropertyInfo US_Qty3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Qty3, x => AddInfo.US_Qty3Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAUQs))]
		public ZString US_UQ3
		{
			get { return AddInfo.US_UQ3; }
			set
			{
				AddInfo.US_UQ3 = value;
				ReCalculateRunningQty();
				SetAgainWhenUQChanged();
			}
		}

		public ZPropertyInfo US_UQ3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ3, x => AddInfo.US_UQ3Info); }
		}

		public ZDecimal US_Qty4
		{
			get { return AddInfo.US_Qty4; }
			set
			{
				AddInfo.US_Qty4 = value;
				ReCalculateRunningQty();
			}
		}

		public ZPropertyInfo US_Qty4Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Qty4, x => AddInfo.US_Qty4Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAUQs))]
		public ZString US_UQ4
		{
			get { return AddInfo.US_UQ4; }
			set
			{
				AddInfo.US_UQ4 = value;
				ReCalculateRunningQty();
				SetAgainWhenUQChanged();
			}
		}

		public ZPropertyInfo US_UQ4Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ4, x => AddInfo.US_UQ4Info); }
		}

		public ZDecimal US_Qty5
		{
			get { return AddInfo.US_Qty5; }
			set
			{
				AddInfo.US_Qty5 = value;
				ReCalculateRunningQty();
			}
		}

		public ZPropertyInfo US_Qty5Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Qty5, x => AddInfo.US_Qty5Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAUQs))]
		public ZString US_UQ5
		{
			get { return AddInfo.US_UQ5; }
			set
			{
				AddInfo.US_UQ5 = value;
				ReCalculateRunningQty();
				SetAgainWhenUQChanged();
			}
		}

		public ZPropertyInfo US_UQ5Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ5, x => AddInfo.US_UQ5Info); }
		}

		public ZDecimal US_Qty6
		{
			get { return AddInfo.US_Qty6; }
			set
			{
				AddInfo.US_Qty6 = value;
				ReCalculateRunningQty();
			}
		}

		public ZPropertyInfo US_Qty6Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Qty6, x => AddInfo.US_Qty6Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FDAUQs))]
		public ZString US_UQ6
		{
			get { return AddInfo.US_UQ6; }
			set
			{
				AddInfo.US_UQ6 = value;
				ReCalculateRunningQty();
				SetAgainWhenUQChanged();
			}
		}

		public ZPropertyInfo US_UQ6Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UQ6, x => AddInfo.US_UQ6Info); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.USCCountryList))]
		public ZString US_ProdCountry
		{
			get
			{
				var result = AddInfo.US_ProdCountry;
				if (result.IsEmpty)
				{
					var invoiceLine = InvoiceLine;
					result = invoiceLine != null ? invoiceLine.US_UC_NKCountryOfOrigin : ZString.Empty;
					if (result.StartsWith("X", StringComparison.CurrentCultureIgnoreCase))
					{
						result = Core.Constants.CountryCodes.Canada;
					}
				}
				return result;
			}
			set { AddInfo.US_ProdCountry = value; }
		}

		public ZPropertyInfo US_ProdCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProdCountry, x => AddInfo.US_ProdCountryInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.USCCountryList))]
		public ZString US_ShipmentCountry
		{
			get
			{
				var result = AddInfo.US_ShipmentCountry;
				if (result.IsEmpty && IsPriorNotice && InvoiceLine != null)
				{
					result = InvoiceLine.US_UC_NKCountryOfExport;
					if (result.StartsWith("X", StringComparison.CurrentCultureIgnoreCase))
					{
						result = Core.Constants.CountryCodes.Canada;
					}
				}
				return result;
			}
			set { AddInfo.US_ShipmentCountry = value; }
		}

		public ZPropertyInfo US_ShipmentCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ShipmentCountry, x => AddInfo.US_ShipmentCountryInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.USCCountryList))]
		public ZString US_SourceCountry
		{
			get { return AddInfo.US_SourceCountry; }
			set { AddInfo.US_SourceCountry = value; }
		}

		public ZPropertyInfo US_SourceCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_SourceCountry, x => AddInfo.US_SourceCountryInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.USCCountryList))]
		public ZString US_RefusedCountry
		{
			get { return AddInfo.US_RefusedCountry; }
			set { AddInfo.US_RefusedCountry = value; }
		}

		public ZPropertyInfo US_RefusedCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RefusedCountry, x => AddInfo.US_RefusedCountryInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.CylindricalRectangularList))]
		[ReadOnlyMember(nameof(US_ContainerDimType_ReadOnly))]
		public ZString US_ContainerDimType
		{
			get { return AddInfo.US_ContainerDimType; }
			set { AddInfo.US_ContainerDimType = value; }
		}

		public ZPropertyInfo US_ContainerDimTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ContainerDimType, x => AddInfo.US_ContainerDimTypeInfo); }
		}

		public ZString US_ItemIdentityNumber
		{
			get { return AddInfo.US_ItemIdentityNumber; }
			set { AddInfo.US_ItemIdentityNumber = value; }
		}

		public ZPropertyInfo US_ItemIdentityNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ItemIdentityNumber, x => AddInfo.US_ItemIdentityNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.IdentityNumberQualifierList))]
		public ZString US_ItemIdentityNumberQualifier
		{
			get { return AddInfo.US_ItemIdentityNumberQualifier; }
			set { AddInfo.US_ItemIdentityNumberQualifier = value; }
		}

		public ZPropertyInfo US_ItemIdentityNumberQualifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ItemIdentityNumberQualifier, x => AddInfo.US_ItemIdentityNumberQualifierInfo); }
		}

		bool US_ContainerDimType_ReadOnly
		{
			get { return IsBiologics; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.DimensionUQs))]
		[ReadOnlyMember(nameof(US_DimUQ_ReadOnly))]
		public ZString US_DimUQ
		{
			get { return AddInfo.US_DimUQ; }
			set { AddInfo.US_DimUQ = value; }
		}

		public ZPropertyInfo US_DimUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DimUQ, x => AddInfo.US_DimUQInfo); }
		}

		bool US_DimUQ_ReadOnly
		{
			get { return IsBiologics; }
		}

		public ZDecimal US_CanDim1
		{
			get { return AddInfo.US_CanDim1; }
			set { AddInfo.US_CanDim1 = value; }
		}

		public ZPropertyInfo US_CanDim1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CanDim1, x => AddInfo.US_CanDim1Info); }
		}

		public ZDecimal US_CanDim2
		{
			get { return AddInfo.US_CanDim2; }
			set { AddInfo.US_CanDim2 = value; }
		}

		public ZPropertyInfo US_CanDim2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CanDim2, x => AddInfo.US_CanDim2Info); }
		}

		public ZDecimal US_CanDim3
		{
			get { return AddInfo.US_CanDim3; }
			set { AddInfo.US_CanDim3 = value; }
		}

		public ZPropertyInfo US_CanDim3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CanDim3, x => AddInfo.US_CanDim3Info); }
		}

		public bool IsBiologics
		{
			get { return US_ProgramCode == FDAProgramCodeList.Codes.BIO; }
		}

		internal bool IsProductConstituentElementRequired
		{
			get
			{
				var isRequiredForVME = US_ProcessingCode == FDAProcessingCodeList.Codes.VME_ADR;
				return IsACECargoReleaseValidationModeOrStandAlonePriorNotice && US_ProgramCode == FDAProgramCodeList.Codes.VME && isRequiredForVME;
			}
		}

		public ZBool IsPriorNotice
		{
			get { return IsFDAPriorNoticeRequired && US_PNC.IsEmpty; }
		}

		ZBool IsFDAPriorNoticeRequired
		{
			get
			{
				return IsFood
						 && !US_PND
						 && (FDAPriorNoticeAndAdmissibilityReviewMayBeRequired || FDAPriorNoticeAndAdmissibilityReviewRequired || US_FDAForcePN)
						 && !IsConsumptionFTZ
						 && GoodsFromFTZ.IsEmpty;
			}
		}

		void DefaultFDAIfRequiredForPriorNotice()
		{
			if (InvoiceLine != null)
			{
				InvoiceLine.DefaultFDADateIfRequired();
			}
		}

		ZString GoodsFromFTZ
		{
			get
			{
				var declaration = InvoiceLine?.Declaration;
				return declaration != null && declaration.US_EntryType == EntryTypeList.Codes.Warehouse ? declaration.US_GoodsFromFTZ : ZString.Empty;
			}
		}

		internal ZBool IsPriorNoticeForAutoRating
		{
			get { return IsFDAPriorNoticeRequired && (US_PNC.IsEmpty || US_IsPNCFromMsg); }
		}

		public bool IsFood
		{
			get { return US_ProgramCode == FDAProgramCodeList.Codes.FOO; }
		}

		public bool IsFSVPImpRequired
		{
			get
			{
				return IsFood && IndustryCode != "16"
					&& IndustryCode != "32"
					&& !AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.FSX)
					&& !AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.RNE)
					&& (US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_NSF
						|| US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_PRO
						|| US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_ADD
						|| US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_DSU
						|| US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_FEE);
			}
		}

		internal bool HasPNCorPND
		{
			get { return !US_PNC.IsEmpty || US_PND; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USACEFDAAddInfoLookups.FoodFacilityRegistrationExemptionCodes))]
		public ZString US_FME
		{
			get { return AddInfo.US_FME; }
			set { AddInfo.US_FME = value; }
		}

		public ZPropertyInfo US_FMEInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FME, x => AddInfo.US_FMEInfo); }
		}

		public ZString US_PFR
		{
			get { return AddInfo.US_PFR; }
			set { AddInfo.US_PFR = value; }
		}

		public ZPropertyInfo US_PFRInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PFR, x => AddInfo.US_PFRInfo); }
		}

		public ZInt US_CanDim1Inch
		{
			get { return US_CanDim1.Truncate(0).ToZInt(); }
			set
			{
				US_CanDim1 = ZDecimal.ParseSafe(value.ToString() + "." + US_CanDim1_16th.ToString().PadLeft(2, '0'), 0);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_CanDim1Inch();
				}
				US_CanDim1InchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_CanDim1InchInfo
		{
			get { return GetZPropertyInfo(Schema.US_CanDim1Inch); }
		}

		public ZPropertyInfo US_CanDim1_16thInfo
		{
			get { return GetZPropertyInfo(nameof(US_CanDim1_16th)); }
		}

		public ZInt US_CanDim1_16th
		{
			get { return GetDecimalPoints(US_CanDim1Info); }
			set
			{
				UpdateDimensionFromRepresentationValue(value, US_CanDim1Info);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_CanDim1_16th();
				}
				US_CanDim1_16thInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_CanDim2_16thInfo
		{
			get { return GetZPropertyInfo(nameof(US_CanDim2_16th)); }
		}

		public ZInt US_CanDim2_16th
		{
			get { return GetDecimalPoints(US_CanDim2Info); }
			set
			{
				UpdateDimensionFromRepresentationValue(value, US_CanDim2Info);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_CanDim2_16th();
				}
				US_CanDim2_16thInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_CanDim3_16thInfo
		{
			get { return GetZPropertyInfo(nameof(US_CanDim3_16th)); }
		}

		public ZInt US_CanDim3_16th
		{
			get { return GetDecimalPoints(US_CanDim3Info); }
			set
			{
				UpdateDimensionFromRepresentationValue(value, US_CanDim3Info);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_CanDim3_16th();
				}
				US_CanDim3_16thInfo.RefreshBinding();
			}
		}

		ZInt GetDecimalPoints(ZPropertyInfo propertyInfo)
		{
			var value = (ZDecimal)propertyInfo.Value;
			var result = ZInt.Zero;
			if (value > 0)
			{
				var splitted = value.ToString().Split('.');
				return ZInt.ParseSafe((splitted.Length > 1 ? splitted[1].PadRight(2, '0') : ""), 0);
			}
			return result;
		}

		void UpdateDimensionFromRepresentationValue(ZInt reprValue, ZPropertyInfo propertyInfo)
		{
			var dimension = (ZDecimal)propertyInfo.Value;

			var beforeStop = dimension.Truncate(0).ToString().TrimEnd('.');
			var afterStop = reprValue.ToString();
			propertyInfo.Value = ZDecimal.ParseSafe(beforeStop + "." + afterStop.PadLeft(2, '0'), 0);
		}

		public ZInt US_CanDim2Inch
		{
			get { return US_CanDim2.Truncate(0).ToZInt(); }
			set
			{
				US_CanDim2 = ZDecimal.ParseSafe(value.ToString() + "." + US_CanDim2_16th.ToString().PadLeft(2, '0'), 0);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_CanDim2Inch();
				}
				US_CanDim2InchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_CanDim2InchInfo
		{
			get { return GetZPropertyInfo(Schema.US_CanDim2Inch); }
		}

		public ZInt US_CanDim3Inch
		{
			get { return US_CanDim3.Truncate(0).ToZInt(); }
			set
			{
				US_CanDim3 = ZDecimal.ParseSafe(value.ToString() + "." + US_CanDim3_16th.ToString().PadLeft(2, '0'), 0);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_CanDim3Inch();
				}
				US_CanDim3InchInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_CanDim3InchInfo
		{
			get { return GetZPropertyInfo(Schema.US_CanDim3Inch); }
		}

		public ZDecimal US_InvCurrValue
		{
			get { return AddInfo.US_InvCurrValue; }
			set
			{
				var oldValue = US_InvCurrValue;
				AddInfo.US_InvCurrValue = value;
				if (!IsCopying && oldValue != US_InvCurrValue)
				{
					InvoiceLine?.Declaration?.MarkApportionmentDirty();
				}
			}
		}

		public ZPropertyInfo US_InvCurrValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InvCurrValue, x => AddInfo.US_InvCurrValueInfo); }
		}

		public ZDecimal US_TotalValue
		{
			get { return AddInfo.US_TotalValue; }
			set { AddInfo.US_TotalValue = value; }
		}

		public ZPropertyInfo US_TotalValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TotalValue, x => AddInfo.US_TotalValueInfo); }
		}

		public ZString US_TotalUSDValue
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLine = InvoiceLine;

				if (invoiceLine == null || invoiceLine.IsOGAValueUpToDate)
				{
					result = US_TotalValue.ToString(0);
				}
				else
				{
					result = "...";
				}

				return result;
			}
		}

		public ZPropertyInfo US_TotalUSDValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TotalUSDValue, x => US_TotalValueInfo); }
		}

		public ZDecimal US_UnitValue
		{
			get { return AddInfo.US_UnitValue; }
			set { AddInfo.US_UnitValue = value; }
		}

		internal ZDecimal? GetDefaultUnitValueFromBaseQty()
		{
			ZDecimal? result = null;
			var totalQty = GetCalculatedRunningBaseQty().Round(2);

			if (!totalQty.IsEmpty)
			{
				result = new ZDecimal(US_InvCurrValue / totalQty).Round(2);
			}
			return result;
		}

		public ZPropertyInfo US_UnitValueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnitValue, x => AddInfo.US_UnitValueInfo); }
		}

		[ReadOnly(true)]
		public ZString US_TrackingStatus
		{
			get { return AddInfo.US_TrackingStatus; }
			set
			{
				var oldValue = US_TrackingStatus;
				AddInfo.US_TrackingStatus = value;
				var newValue = US_TrackingStatus;
				if (!IsCopying && oldValue != newValue && PGATrackingStatusList.IsDeletedOrBeingDeleted(newValue) != PGATrackingStatusList.IsDeletedOrBeingDeleted(oldValue))
				{
					InvoiceLine?.Declaration?.MarkApportionmentDirty();
				}
			}
		}

		public ZPropertyInfo US_TrackingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TrackingStatus, x => AddInfo.US_TrackingStatusInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.ACEFDA|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		public ZBool US_IsPNCFromMsg
		{
			get { return AddInfo.US_IsPNCFromMsg; }
			set
			{
				var oldValue = US_IsPNCFromMsg;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						AddInfo.US_IsPNCFromMsg = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}

		public ZPropertyInfo US_IsPNCFromMsgInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IsPNCFromMsg, x => AddInfo.US_IsPNCFromMsgInfo); }
		}

		#endregion

		#region New Properties

		public ZBool ShouldForcePriorNotice
		{
			get
			{
				var result = false;

				if (!FDAPriorNoticeAndAdmissibilityReviewMayBeRequired && !FDAPriorNoticeAndAdmissibilityReviewRequired && !IsConsumptionFTZ)
				{
					var industryCode = IndustryCode;
					var classCode = ClassCode;
					var subClassCode = SubClassCode;
					var intIndustryCode = ZInt.ParseSafe(industryCode, 0);
					var industryCodeAndClassCode = industryCode + classCode;

					result = new ZString("07,09,69,70,71,72").OccurrencesIgnoringCase(industryCode) == 1
						|| intIndustryCode.IsInRange(2, 5)
						|| intIndustryCode.IsInRange(12, 18)
						|| intIndustryCode.IsInRange(20, 42)
						|| intIndustryCode.IsInRange(45, 47)
						|| new ZString("52D,50C,50D,50E,50F,50G,50L").OccurrencesIgnoringCase(industryCodeAndClassCode) == 1
						|| (industryCode == "54" && new ZString("A,B,C,L,M,Y").OccurrencesIgnoringCase(subClassCode) == 1);
				}

				return result;
			}
		}

		public ZString IndustryCode
		{
			get { return US_ProductCode.SubstringSafe(0, 2); }
		}

		public ZString ClassCode
		{
			get { return US_ProductCode.SubstringSafe(2, 1); }
		}

		public ZString SubClassCode
		{
			get { return US_ProductCode.SubstringSafe(3, 1); }
		}

		public ZString PIC
		{
			get { return US_ProductCode.SubstringSafe(4, 1); }
		}

		public ZString GroupCode
		{
			get { return US_ProductCode.SubstringSafe(5, 2); }
		}

		public ZBool IsLACF
		{
			get { return (ZInt.ParseSafe(IndustryCode, 0).IsInRange(2, 39) || IndustryCode == "41" || IndustryCode == "71" || IndustryCode == "72") && (PIC == "F" || PIC == "E"); }
		}

		public ZBool IsAcidified
		{
			get { return (ZInt.ParseSafe(IndustryCode, 0).IsInRange(2, 39) || IndustryCode == "41" || IndustryCode == "71" || IndustryCode == "72") && PIC == "I"; }
		}

		bool IsConsumptionFTZ
		{
			get
			{
				var declaration = InvoiceLine != null ? InvoiceLine.Declaration : null;
				return declaration != null && declaration.IsConsumptionFTZ;
			}
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "ACEFDA"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (ACEFDA)base.CloneInternal(args);
			result.US_PND = false;
			result.US_FDAForcePN = false;
			result.US_PNC = ZString.Empty;

			result.CloneChildren(this, args);

			return result;
		}

		internal void CloneChildren(ACEFDA previousLine, BusinessObjectCloneArgs args = null)
		{
			foreach (var affirmationCode in previousLine.AffirmationCodes)
			{
				AffirmationCodes.Add((ACEAffirmationCode)affirmationCode.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(ACEAffirmationCode), false)));
			}

			foreach (Lot lot in previousLine.Lots)
			{
				Lots.Add((Lot)lot.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(Lot), false)));
			}

			foreach (ConstituentElement productConstituentElement in previousLine.ProductConstituentElements)
			{
				ProductConstituentElements.Add((ConstituentElement)productConstituentElement.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(ConstituentElement), false)));
			}

			foreach (FDALicense license in previousLine.Licenses)
			{
				Licenses.Add((FDALicense)license.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(FDALicense), false)));
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				PGADataCorrection.UnRegisterTrackerIfNeeded();
			}
			AffirmationCodes.RemoveAndDeleteAll();
			Lots.RemoveAndDeleteAll();
			Licenses.RemoveAndDeleteAll();
			ProductConstituentElements.RemoveAndDeleteAll();
			DocAddresses.RemoveAndDeleteAll();
			base.Delete();
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USLot, typeof(Lot));
			result.Add(CusAddInfoTypeAttribute.Codes.USFDALicense, typeof(FDALicense));
			result.Add(CusAddInfoTypeAttribute.Codes.USSCI, typeof(ScientificData));
			result.Add(CusAddInfoTypeAttribute.Codes.USPGA, typeof(ConstituentElement));
			return result;
		}

		internal void SetFDAQuantityDefaultsByInvoice()
		{
			var fdaQtyHelper = GetLastFDAQty(false);
			if (fdaQtyHelper.LastFDAQtyInfo != null)
			{
				var dataProvider = new UnitConverterDataProviderWithoutProduct(InvoiceLine);
				var unitConverter = new UnitConverter(dataProvider);

				var defaultValue = unitConverter.Convert(InvoiceLine.JI_InvoiceQuantity, InvoiceLine.JI_InvoiceUQ, fdaQtyHelper.LastFDAUQ);

				if (!fdaQtyHelper.IsBaseFDAQty)
				{
					var baseQuantity = unitConverter.Convert(InvoiceLine.JI_InvoiceQuantity, InvoiceLine.JI_InvoiceUQ, US_UQ1);
					var calculatedRunningBaseQty = GetCalculatedRunningBaseQty();
					if (baseQuantity != 0 && calculatedRunningBaseQty != 0)
					{
						defaultValue = baseQuantity / calculatedRunningBaseQty;
					}
				}
				if (defaultValue != 0 && (fdaQtyHelper.IsBaseFDAQty || fdaQtyHelper.LastFDAQtyInfo.Value.IsEmpty))
				{
					fdaQtyHelper.LastFDAQtyInfo.Value = defaultValue;
				}
			}
		}

		public class UnitConverterDataProviderWithoutProduct : IUnitConverterDataProvider
		{
			public UnitConverterDataProviderWithoutProduct(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
			}

			readonly IUnitConverterDataProvider invoiceLine;

			public MasterFiles.Business.OrgSupplierPart Product
			{
				get
				{
					return null;
				}
			}

			public ZString CountryCode
			{
				get
				{
					return invoiceLine.CountryCode;
				}
			}

			public ZGuid SupplierFK
			{
				get
				{
					return invoiceLine.SupplierFK;
				}
			}

			public BusinessObjectFactory Factory
			{
				get
				{
					return invoiceLine.Factory;
				}
			}

			public IEnumerable<IUnitConverter> GetUnitConversionFactorsFromProductUnits()
			{
				return Enumerable.Empty<IUnitConverter>();
			}

			public bool ProductHasSpecificUnitConversions
			{
				get { return false; }
			}

			ZString IUnitConverterDataProvider.Type
			{
				get { return RPTypeList.Codes.CommercialInvoice; }
			}
		}

		void SetAgainWhenUQChanged()
		{
			if (InvoiceLine != null && InvoiceLine.ACE_FDALines.Count == 1 && InvoiceLine.Part != null && InvoiceLine.JI_InvoiceQuantity != 0)
			{
				SetFDAQuantityDefaultsByInvoice();
			}
		}

		internal FDAQtyHelper GetLastFDAQty(bool hasQty)
		{
			var result = new FDAQtyHelper();
			bool getLastQty = false;
			getLastQty = result.GetLastQty(US_Qty6, US_UQ6, US_Qty6Info, hasQty);
			if (!getLastQty)
			{
				getLastQty = result.GetLastQty(US_Qty5, US_UQ5, US_Qty5Info, hasQty);
			}
			if (!getLastQty)
			{
				getLastQty = result.GetLastQty(US_Qty4, US_UQ4, US_Qty4Info, hasQty);
			}
			if (!getLastQty)
			{
				getLastQty = result.GetLastQty(US_Qty3, US_UQ3, US_Qty3Info, hasQty);
			}
			if (!getLastQty)
			{
				getLastQty = result.GetLastQty(US_Qty2, US_UQ2, US_Qty2Info, hasQty);
			}
			if (!getLastQty && !US_UQ1.IsEmpty)
			{
				result.LastFDAQtyInfo = US_Qty1Info;
				result.LastFDAUQ = US_UQ1;
				result.IsBaseFDAQty = true;
			}
			return result;
		}

		public void SetFDADefaultValueForBaseQty()
		{
			if (InvoiceLine != null && InvoiceLine.Part == null && InvoiceLine.RequiresPriorNoticeReporting() && !HasPNCorPND && AddInfoLookups.FDABaseUQs.ContainsCode(InvoiceLine.JI_InvoiceUQ) && US_Qty1.IsEmpty)
			{
				US_UQ1 = InvoiceLine.JI_InvoiceUQ;
				US_Qty1 = InvoiceLine.JI_InvoiceQuantity;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USACEFDAAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USACEFDAAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USACEFDAAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USACEFDAAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USACEFDAAddInfo fAddInfo;

		public new ACEFDAValidation Validation
		{
			get { return (ACEFDAValidation)base.Validation; }
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new ACEFDAValidation(this);
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		Integration.Customs.US.IUSACEFDAAddInfo Integration.Customs.US.IUSACEFDA.AddInfo
		{
			get { return AddInfo; }
		}

		internal bool IsACECargoReleaseValidationModeOrStandAlonePriorNotice
		{
			get
			{
				var declaration = InvoiceLine != null ? InvoiceLine.Declaration : null;
				return declaration != null && declaration.CanHavePGAFDA && (declaration.IsPGAValidationOn() || declaration.IsStandAlonePriorNoticeMode);
			}
		}

		internal bool IsACECargoReleaseValidationMode
		{
			get
			{
				var declaration = this.InvoiceLine != null ? InvoiceLine.Declaration : null;
				return declaration != null && declaration.CanHavePGAFDA && declaration.IsPGAValidationOn();
			}
		}

		internal bool IsACEStandalonePNWithoutENSAndCRL
		{
			get
			{
				var declaration = this.InvoiceLine != null ? InvoiceLine.Declaration : null;
				return declaration != null && declaration.IsACEStandalonePNWithoutENSAndCRL;
			}
		}

		#endregion

		#region IFDAData Members

		ZString IACEPriorNoticeLine.CommercialDescription
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.JI_Description : ZString.Empty;
			}
		}

		IEnumerable<KeyValuePair<IPGAContactDetails, OrgCusCodeForFDA>> IFDAData.ActiveIngredientProducers
		{
			get
			{
				var result = ProductConstituentElements.Cast<ConstituentElement>().Where(x => x.ProducerAddress != null).Select(x => x.ProducerAddress).Distinct();

				foreach (OrgAddress address in result)
				{
					yield return new KeyValuePair<IPGAContactDetails, OrgCusCodeForFDA>(OrgHeaderWrapper.New(address), OrgCusCodeForFDA.FindCustomsNumberAndID(address, US_ProgramCode));
				}
			}
		}

		ZInt IFDAData.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IFDAData.ProgramCode { get { return US_ProgramCode; } }

		ZString IFDAData.ProcessingCode { get { return US_ProcessingCode; } }

		ZString IFDAData.IntendedUseCode { get { return US_IntendedUseCode; } }

		ZString IFDAData.IntendedUseDescription { get { return US_IntendedUseDescr.Left(21); } }

		ZString IFDAData.BrandName { get { return US_BrandName; } }

		ZString IFDAData.ItemIdentityNumber { get { return US_ItemIdentityNumber; } }

		ZString IFDAData.ItemIdentityNumberQualifier { get { return US_ItemIdentityNumberQualifier; } }

		ZString IFDAData.Description { get { return US_Description; } }

		ZString IFDAData.ProductCode { get { return US_ProductCode; } }

		ZString IFDAData.ProductionGrowthCountry { get { return US_ProdCountry; } }

		ZString IFDAData.ProductionGrowthCountryQualifier
		{
			get
			{
				var result = SourceTypeCodesList.Codes.CountryOfProduction;

				if (US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_NSF)
				{
					result = SourceTypeCodesList.Codes.PlaceOfGrowth;
				}
				else if (US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_FEE)
				{
					if (US_ProducerType == ProducerFirmTypeList.Codes.M)
					{
						result = SourceTypeCodesList.Codes.CountryOfProduction;
					}
					else
					{
						result = SourceTypeCodesList.Codes.PlaceOfGrowth;
					}
				}
				return result;
			}
		}

		ZString IFDAData.ShipmentCountry
		{
			get { return IsPriorNotice ? US_ShipmentCountry : ZString.Empty; }
		}

		ZString IFDAData.SourceCountry { get { return US_SourceCountry; } }

		ZString IFDAData.RefusalCountry { get { return US_RefusedCountry; } }

		IEnumerable<IConstituentElement> IFDAData.ProductConstituentElements
		{
			get
			{
				foreach (IConstituentElement element in ProductConstituentElements)
				{
					yield return element;
				}
			}
		}

		IEnumerable<KeyValuePair<ZString, ZString>> IFDAData.AffirmationOfCompliance
		{
			get
			{
				var affirmationCodesCombined = new AffirmationCodesCombined(this);

				foreach (var element in affirmationCodesCombined.AoCs)
				{
					yield return new KeyValuePair<ZString, ZString>(element.Key, element.Value);
				}
			}
		}

		IEnumerable<IFDALot> IFDAData.Lots
		{
			get
			{
				foreach (IFDALot lot in Lots)
				{
					yield return lot;
				}
			}
		}

		public OrgCusCodeForFDA ShipperNumber
		{
			get
			{
				return OrgCusCodeForFDA.FindCustomsNumberAndID(EnableDocAddressForFDA ? ShipperDocAddress : ShipperAddress, US_ProgramCode);
			}
		}

		IPGAContactDetails IFDAData.ShipperContact
		{
			get
			{
				return EnableDocAddressForFDA ? ShipperDocAddress : (ShipperAddress != null ? OrgHeaderWrapper.New(ShipperAddress) : null);
			}
		}

		OrgCusCodeForFDA IFDAData.SubmitterNumber
		{
			get
			{
				var invoiceLine = InvoiceLine;
				var address = invoiceLine != null && invoiceLine.Declaration.FDASubmitter != null ? invoiceLine.Declaration.FDASubmitter.MainAddress : null;
				return OrgCusCodeForFDA.FindCustomsNumberAndID(address, US_ProgramCode);
			}
		}

		IPGAContactDetails IFDAData.SubmitterContact
		{
			get
			{
				var invoiceLine = InvoiceLine;
				var orgHeader = invoiceLine != null && invoiceLine.Declaration != null ? invoiceLine.Declaration.FDASubmitter : null;
				return orgHeader != null ? OrgHeaderWrapper.New(orgHeader) : null;
			}
		}

		OrgCusCodeForFDA IFDAData.TransmitterNumber
		{
			get
			{
				OrgAddress address = null;
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null)
				{
					address = invoiceLine.Declaration.BrokerCustomsAddressOfRecord;
					if (address == null)
					{
						var orgProxy = invoiceLine.Declaration.Branch.OrgProxy;
						if (orgProxy != null)
						{
							if (orgProxy.MainAddress != null)
							{
								address = orgProxy.MainAddress;
							}
						}
					}
				}
				return OrgCusCodeForFDA.FindCustomsNumberAndID(address, US_ProgramCode);
			}
		}

		ICustomsBrokerDetails IFDAData.Transmitter
		{
			get { return InvoiceLine; }
		}

		bool IFDAData.IsSubmitterRelevant
		{
			get { return IsPriorNotice || US_ProgramCode == FDAProgramCodeList.Codes.TOB; }
		}

		ZString IFDAData.SubmitterType
		{
			get { return US_ProgramCode == FDAProgramCodeList.Codes.TOB ? EntityRoleCodeList.Codes.Submitter : EntityRoleCodeList.Codes.PNSubmitter; }
		}

		bool IFDAData.IsTransmitterRelevant
		{
			get { return IsPriorNotice; }
		}

		IPGAContactDetails IFDAData.ManufacturerContact
		{
			get { return EnableDocAddressForFDA ? ManufacturerDocAddress : ManufacturerWrapper; }
		}

		ZString IFDAData.FirmType
		{
			get
			{
				var result = ZString.Empty;
				if (US_ProgramCode == FDAProgramCodeList.Codes.FOO && US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_CCW)
				{
					result = EntityRoleCodeList.Codes.ManufacturerOfGoods;
				}
				else
				{
					switch (US_ProducerType)
					{
						case ProducerFirmTypeList.Codes.C:
							result = EntityRoleCodeList.Codes.FDAConsolidator;
							break;
						case ProducerFirmTypeList.Codes.G:
							result = EntityRoleCodeList.Codes.CropGrower;
							break;
						case "I":
							result = EntityRoleCodeList.Codes.IndependentThirdPartyLaboratory;
							break;
						case "L":
							result = EntityRoleCodeList.Codes.Laboratory;
							break;
						default:
							result = EntityRoleCodeList.Codes.ManufacturerOfGoods;
							break;
					}
				}

				return result;
			}
		}

		IPGAContactDetails IFDAData.DeliveryPartyContact
		{
			get
			{
				IPGAContactDetails result = EnableDocAddressForFDA ? DeliverToPartyDocAddress : (DeliverToPartyAddress != null ? OrgHeaderWrapper.New(DeliverToPartyAddress) : null);

				if (result == null)
				{
					var invoiceLine = InvoiceLine;
					result = invoiceLine != null ? OrgHeaderWrapper.New(invoiceLine.ConsigneeAddress) : null;
				}
				return result;
			}
		}

		ZString IFDAData.DeliveryPartyRoleCode
		{
			get { return IsPriorNotice && US_ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW ? EntityRoleCodeList.Codes.UltimateConsignee : EntityRoleCodeList.Codes.DeliveryParty; }
		}

		ZDate IFDAData.InspectionDate
		{
			get
			{
				var invoiceLine = InvoiceLine;
				var arrivalDate = invoiceLine != null && invoiceLine.Declaration != null ? invoiceLine.Declaration.US_FDAADTA : ZDate.Empty;
				return arrivalDate.IsValid ? arrivalDate.Date : ZDate.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		ZString IFDAData.InspectionTime
		{
			get
			{
				var invoiceLine = InvoiceLine;
				var arrivalDate = invoiceLine != null && invoiceLine.Declaration != null ? invoiceLine.Declaration.US_FDAADTA : ZDate.Empty;
				return arrivalDate.IsValid ? arrivalDate.ToString("HHmm") : "";
			}
		}

		ZString IFDAData.ArrivalLocation
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLine = InvoiceLine;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;

				if (declaration != null)
				{
					result = IsPriorNotice ? declaration.US_SchDArrival : declaration.US_SchDEntry;
				}

				return result;
			}
		}

		List<FDAQtyUQPair> IFDAData.OrderedQtyUQs
		{
			get
			{
				var result = new List<FDAQtyUQPair>();

				if (!US_Qty6.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty6, US_UQ6));
				}

				if (!US_Qty5.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty5, US_UQ5));
				}

				if (!US_Qty4.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty4, US_UQ4));
				}

				if (!US_Qty3.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty3, US_UQ3));
				}

				if (!US_Qty2.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty2, US_UQ2));
				}

				if (!US_Qty1.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty1, US_UQ1));
				}

				return result;
			}
		}

		IPGAContactDetails IFDAData.FDAImporterContact
		{
			get { return EnableDocAddressForFDA ? FDAImporterDocAddress : (FDAImporterAddress != null ? OrgHeaderWrapper.New(FDAImporterAddress) : null); }
		}

		IPGAContactDetails IFDAData.FSVPImporterContact
		{
			get
			{
				return EnableDocAddressForFDA ? FSVPImporterDocAddress : (FSVPImporterAddress != null ? OrgHeaderWrapper.New(FSVPImporterAddress, new string[] { ContactAllocationType.USFSV, ContactAllocationType.USPGA }) : null);
			}
		}

		IPGAContactDetails IFDAData.ProducerContact
		{
			get { return EnableDocAddressForFDA && US_ProgramCode == FDAProgramCodeList.Codes.TOB ? ManufacturerDocAddress : (ProducerAddress != null ? OrgHeaderWrapper.New(ProducerAddress) : null); }
		}

		ZString IFDAData.InitialImporterRoleCode
		{
			get
			{
				return US_ProgramCode == FDAProgramCodeList.Codes.DEV ? EntityRoleCodeList.Codes.DeviceInitialImporter :
					US_ProgramCode == FDAProgramCodeList.Codes.TOB ? EntityRoleCodeList.Codes.ManufacturerOfGoods :
					US_ProgramCode == FDAProgramCodeList.Codes.DRU ? EntityRoleCodeList.Codes.Sponsor : string.Empty;
			}
		}

		IPGAContactDetails IFDAData.OwnerContact
		{
			get { return EnableDocAddressForFDA ? GoodsOwnerDocAddress : (OwnerAddress != null ? OrgHeaderWrapper.New(OwnerAddress) : null); }
		}

		IPGAContactDetails IFDAData.LocationOfGoodsContact
		{
			get { return EnableDocAddressForFDA ? LocationOfGoodsDocAddress : (LocationOfGoodsAddress != null ? OrgHeaderWrapper.New(LocationOfGoodsAddress) : null); }
		}

		ZString IFDAData.CanDimensions1 { get { return GetCanDimensionsFormatted(US_CanDim1); } }

		ZString IFDAData.CanDimensions2 { get { return GetCanDimensionsFormatted(US_CanDim2); } }

		ZString IFDAData.CanDimensions3 { get { return GetCanDimensionsFormatted(US_CanDim3); } }

		ZString GetCanDimensionsFormatted(ZDecimal value)
		{
			var result = ZString.Empty;
			if (value > ZDecimal.Zero)
			{
				var values = value.ToString().Split('.');
				var firstValue = values[0];
				var secondValue = values.Length > 1 ? values[1].PadRight(2, '0') : "00";

				result = firstValue.PadLeft(2, ' ') + secondValue;
			}
			return result;
		}

		ZString IFDAData.PackageTrackingNumberCode { get { return US_PackageTrackCode; } }
		ZString IFDAData.PackageTrackingNumber { get { return US_PackageTrackNumber; } }
		ZString IFDAData.Remarks { get { return US_Remarks; } }

		IEnumerable<ZString> IFDAData.ContainerNumbers
		{
			get
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null && US_ProgramCode != FDAProgramCodeList.Codes.VME)
				{
					foreach (CusContainerInvoiceLinePivot containerPivot in invoiceLine.ContainersPivot)
					{
						if (!containerPivot.ContainerNumber.IsEmpty)
						{
							yield return containerPivot.ContainerNumber;
						}
					}
				}
			}
		}

		IEnumerable<IFDALicense> IFDAData.Licenses
		{
			get
			{
				foreach (IFDALicense license in Licenses)
				{
					yield return license;
				}
			}
		}

		ZString IFDAData.PNConfirmationNumber { get { return US_PNC; } }

		ZDecimal IFDAData.PGALineValue
		{
			get { return US_TotalValue; }
		}

		ZDecimal IFDAData.UnitValue { get { return US_UnitValue; } }

		ZString IFDAData.GoodsFromFTZ
		{
			get { return GoodsFromFTZ; }
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(ACEFDA fda)
				: base(fda)
			{
			}

			protected new ACEFDA BusinessObject
			{
				get { return (ACEFDA)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
				var genPivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				Factory.AddFetchHint(typeof(FDARelatedContainersGenPivot), new ZQuery(GenPivotSchema.XX_RelationType, FDARelatedContainersGenPivot.RelationType), genPivotQuery);
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, BusinessObject.PK);
				Factory.AddFetchHint(OrgCusCodeSchema.OK_OA_PremisesAddress, BusinessObject.US_ManufacturerAddress);
				Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.US_ManufacturerAddress_ZAddress.OrgPK);
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.AffirmationCode, typeof(ACEAffirmationCode));
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
			return new[] { JobComInvoiceLine.Schema.US_FDAIndicator };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_FDADisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[]
			{
				JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, JobComInvoiceLine.Schema.US_SetInd,
				JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin, JobComInvoiceLine.Schema.US_ZoneStatus
			};
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[]
			{
				JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress, JobComInvoiceHeader.Schema.US_FDAContactName, JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress,
				JobComInvoiceHeader.Schema.US_UC_NKCountryOfOrigin, JobComInvoiceHeader.Schema.US_ZoneStatus, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress
			};
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return new[] { CusContainer.Schema.CO_ContainerNumber };
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[]
			{
				JobDeclaration.Schema.US_FDAContactName, JobDeclaration.Schema.JE_OH_FDASubmitter, JobDeclaration.Schema.JE_OA_ManufacturerAddress, JobDeclaration.Schema.US_FDAADTA,
				JobDeclaration.Schema.US_SchDEntry, JobDeclaration.Schema.US_EntryType, JobDeclaration.Schema.US_EstimatedEntryDate,
				JobDeclaration.Schema.US_EntryDate, JobDeclaration.Schema.JE_OH_Supplier, JobDeclaration.Schema.JE_OA_ConsigneeAddress
			};
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

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.FDA; }
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

		#region JobDocAddress

		internal bool EnableDocAddressForFDA => USCustomsDataRegistry.Instance.EnableDocAddressForFDA.Value;

		public T GetDataFromZAddressOrJobDocAddress<T>(Func<T> getDataFromZAddress, Func<JobDocAddress> getJobDocAddress, Func<JobDocAddress, T> getDataFromJobDocAddress, Func<T> getDefaultValue = null)
		{
			if (EnableDocAddressForFDA)
			{
				var result = default(T);
				var docAddress = getJobDocAddress();
				if (docAddress != null)
				{
					result = getDataFromJobDocAddress(docAddress);
				}
				if (result == null && getDefaultValue != null)
				{
					result = getDefaultValue();
				}

				return result;
			}
			else
			{
				return getDataFromZAddress();
			}
		}

		void SetValueToZAddressOrJobDocAddress(Action setValueToZAddress, Func<JobDocAddress> getDocAddress, Action<JobDocAddress> setValueToJobDocAddress)
		{
			if (EnableDocAddressForFDA)
			{
				var docAddress = getDocAddress();
				setValueToJobDocAddress(docAddress);
			}
			else
			{
				setValueToZAddress();
			}
		}

		void SetValueToOrgPKOfJobDocAddress(JobDocAddress docAddress, ZGuid value)
		{
			if (docAddress != null)
			{
				docAddress.OrganisationPK = value;
				if (value.IsEmpty)
				{
					DocAddresses.RemoveAndDelete(docAddress);
				}
			}
		}

		bool IsOverriddenJobDocAddress(Func<JobDocAddress> getJobDocAddress)
		{
			var result = false;
			if (EnableDocAddressForFDA)
			{
				var jobDocAddress = getJobDocAddress();
				result = jobDocAddress?.E2_AddressOverride ?? false;
			}
			return result;
		}

		[ChildEditable(true)]
		public ACEFDAJobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new ACEFDAJobDocAddressDependentCollection(this);
					docAddresses.Load();
					docAddresses.CountChanged += DocAddresses_CountChanged;
					RegisterEditableChildObject(docAddresses);
				}
				return docAddresses;
			}
		}

		void DocAddresses_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			AddInfoValidation.ValidateUS_ProgramCode();
		}

		ACEFDAJobDocAddressDependentCollection docAddresses;

		public OrgHeaderCollection DocAddressOrganizations
		{
			get
			{
				switch (DocAddressCurrentType)
				{
					case DocAddressTypes.Codes.GoodsDeliveredTo:
						return AddInfoLookups.Consignees;
					case DocAddressTypes.Codes.Consolidator:
					case DocAddressTypes.Codes.Grower:
					case DocAddressTypes.Codes.Manufacturer:
						return AddInfoLookups.Consignors;
					default:
						return AddInfoLookups.Organizations;
				}
			}
		}

		public ZString DocAddressCurrentType { get; set; }

		void DefaultManufacturer()
		{
			if (EnableDocAddressForFDA && InvoiceLine is JobComInvoiceLine invoiceLine && !IsOverriddenJobDocAddress(() => ManufacturerDocAddress))
			{
				CreateOrUpdateDocAddress(DocAddressType.Manufacturer, invoiceLine.JI_OA_ManufacturerAddress);
			}
		}

		void CreateOrUpdateDocAddress(DocAddressType docAddressType, ZGuid addressPK)
		{
			if (addressPK.IsValid)
			{
				var docAddress = DocAddresses.FindOrCreateWithDocAddressType(docAddressType);
				if (addressPK != docAddress.E2_OA_Address)
				{
					docAddress.E2_OA_Address = addressPK;
				}
			}
			else
			{
				DocAddresses.RemoveDocAddressViaDocAddressType(docAddressType);
			}
		}

		#endregion

		#region IDocAddresses

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new[]
				{
					DocAddressType.Shipper,
					DocAddressType.GoodsDeliveredTo,
					DocAddressType.ImporterDocumentaryAddress,
					DocAddressType.FSVPImporter,
					DocAddressType.GoodsOwner,
					DocAddressType.Manufacturer,
					DocAddressType.GoodsLocation,
					DocAddressType.InitialImporter,
					DocAddressType.Sponsor,
					DocAddressType.Consolidator,
					DocAddressType.Grower,
					DocAddressType.Laboratory,
					DocAddressType.ThirdPartyLaboratory
				};
			}
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return HumanReadableName; }
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			if (EnableDocAddressForFDA)
			{
				return InvoiceLine == null ? new ACEFDAJobDocAddressValidation(addressToValidate, this) : new ACEFDAJobDocAddressForInvoiceLineValidation(addressToValidate, this);
			}
			return new JobDocAddressValidation(addressToValidate);
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Enterprise.Environment.Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return new JobDocAddressRequirement();
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			var orgHeader = docAddress.Organisation;
			if (orgHeader != null)
			{
				switch (docAddress.E2_AddressType)
				{
					case DocAddressTypes.Codes.GoodsOwner:
					case DocAddressTypes.Codes.GoodsLocation:
						docAddress.E2_Contact = orgHeader?.GetContactDetails(new string[] { ContactAllocationType.USPGA })?.Name ?? ZString.Empty;
						break;
					case DocAddressTypes.Codes.Consolidator:
					case DocAddressTypes.Codes.Grower:
					case DocAddressTypes.Codes.Laboratory:
						docAddress.E2_OA_Address = DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(orgHeader);
						break;
					case DocAddressTypes.Codes.Shipper:
					case DocAddressTypes.Codes.Manufacturer:
					case DocAddressTypes.Codes.GoodsDeliveredTo:
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					case DocAddressTypes.Codes.InitialImporter:
						docAddress.E2_Contact = orgHeader?.GetContactDetails(new string[] { ContactAllocationType.USPGA })?.Name ?? ZString.Empty;
						docAddress.E2_OA_Address = DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(orgHeader);
						break;
					case DocAddressTypes.Codes.FSVPImporter:
						docAddress.E2_Contact = orgHeader?.GetContactDetails(new string[] { ContactAllocationType.USFSV, ContactAllocationType.USPGA })?.Name ?? ZString.Empty;
						docAddress.E2_OA_Address = DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(orgHeader);
						break;
					default:
						break;
				}
			}
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion
	}

	class AffirmationCodesCombined
	{
		public AffirmationCodesCombined(ACEFDA fda)
		{
			this.fDA = fda;
		}
		readonly ACEFDA fDA;

		internal Dictionary<ZString, ZString> AoCs
		{
			get
			{
				var result = new Dictionary<ZString, ZString>();
				foreach (ACEAffirmationCode element in fDA.AffirmationCodes)
				{
					if (!result.ContainsKey(element.CY_Code))
					{
						result.Add(element.CY_Code, element.CY_Data);
					}
				}

				if (fDA.IsPriorNotice)
				{
					var invoiceLine = fDA.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					if (declaration != null)
					{
						if (declaration.JE_TransportMode == TransportTypeList.Codes.Sea && !declaration.JE_VesselName.IsEmpty &&
							!result.ContainsKey(ACE_AffirmationOfComplianceList.Codes.VES))
						{
							result.Add(ACE_AffirmationOfComplianceList.Codes.VES, declaration.JE_VesselName.Left(declaration.IsFTZAdmission ? JobDeclaration.Schema.FTZVesselNameLength : JobDeclaration.Schema.VesselNameLength));
						}

						if (!declaration.JE_VoyageFlightNo.IsEmpty && !result.ContainsKey(ACE_AffirmationOfComplianceList.Codes.VFT))
						{
							result.Add(ACE_AffirmationOfComplianceList.Codes.VFT, declaration.JE_VoyageFlightNo);
						}
					}

					if (!fDA.US_PFR.IsEmpty)
					{
						var affirmationCode = fDA.US_ProducerType.IsEmpty ? ACE_AffirmationOfComplianceList.Codes.PFR : string.Empty;
						switch (fDA.US_ProducerType)
						{
							case ProducerFirmTypeList.Codes.C:
								affirmationCode = ACE_AffirmationOfComplianceList.Codes.CFR;
								break;
							case ProducerFirmTypeList.Codes.G:
								affirmationCode = ACE_AffirmationOfComplianceList.Codes.GFR;
								break;
							case ProducerFirmTypeList.Codes.M:
								affirmationCode = ACE_AffirmationOfComplianceList.Codes.PFR;
								break;
						}

						if (!string.IsNullOrEmpty(affirmationCode) && !result.ContainsKey(affirmationCode))
						{
							result.Add(affirmationCode, fDA.US_PFR);
						}
					}
				}

				if (!fDA.US_FME.IsEmpty && !result.ContainsKey(ACE_AffirmationOfComplianceList.Codes.FME))
				{
					result.Add(ACE_AffirmationOfComplianceList.Codes.FME, fDA.US_FME);
				}
				return result;
			}
		}
	}

	public class FDAQtyHelper
	{
		internal ZPropertyInfo LastFDAQtyInfo
		{
			get;
			set;
		}

		internal ZString LastFDAUQ
		{
			get;
			set;
		}

		internal bool IsBaseFDAQty
		{
			get;
			set;
		}

		public bool GetLastQty(ZDecimal qty, ZString uQ, ZPropertyInfo info, bool hasQty)
		{
			if (!uQ.IsEmpty)
			{
				if ((qty == 0 && !hasQty) || (qty != 0 && hasQty))
				{
					LastFDAQtyInfo = info;
					LastFDAUQ = uQ;
					return true;
				}
				return true;
			}
			return false;
		}
	}
}
