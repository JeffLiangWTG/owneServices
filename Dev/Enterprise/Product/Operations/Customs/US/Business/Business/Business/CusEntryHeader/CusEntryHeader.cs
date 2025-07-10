using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.Business.EntryNumber;
using Enterprise.Customs.US.Business.FDARecapPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ICusEntryLine = Enterprise.Customs.US.Business.MessageBuilders.ICusEntryLine;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(CusEntryHeader.Schema.UniqueReference), DescriptionProperty(CusEntryHeader.Schema.EntryNumber)]
	[SystemDefinedValues]
	public partial class CusEntryHeader : TypeSafeCusEntryHeader
		, Integration.Customs.US.ICusEntryHeader
		, ICusEntryHeaderMessageAttachee
		, ICargoManifestStatusQueryData
		, ICargoReleaseCusEntryHeader
		, IStatusNeedsRecalculationProvider
		, IDutyDataLineHeader
		, IReconOriginalChargeParent
		, ICustomsChargeEntry
		, IUSCustomsChargeEntry
		, IMessageFailStatusManager
		, IAESTIRMessageAttachee
		, IAESTIRTransportationDetail
		, Accounting.Integration.IAccInvoiceDataProvider
		, IEntrySummaryQueryMessageAttachee
		, ICBPEDIMessageMessageTextNumberPlaceHolderFiller
		, IACECusEntryHeader
		, IACECensusWarningQuery
		, IACECargoReleaseHeader
		, IAESEntry
		, Integration.Customs.ICusAddInfoTypeSupporter
		, Integration.Customs.ICusCodeDataTypeSupporter
		, IBrokerSignatureProvider
		, IEntryHeaderDutyDataProvider
		, ILiquidationProvider
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fEntryStatusCalculator = new ImportEntryStatusCalculator(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Tests ErrorsRecordTest > TestSO20CMTCargoReleaseStatusResponseAction and EntryStatusCalculatorTest > TestDeriveStatus tests fail if removed")]
		readonly ImportEntryStatusCalculator fEntryStatusCalculator;

		#region Constants

		public static class Constants
		{
			public static class FTZZoneStatuses
			{
				public const byte N = 0x08;
				public const byte P = 0x04;
				public const byte ZonesDZ = 0x03;
				public const byte Z = 0x02;
				public const byte D = 0x01;
			}
		}

		#endregion

		#region Loader

		public new class Loader : Customs.Business.CusEntryHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusEntryHeader FindByEntryNumberAndFilerCode(ZGuid companyPK, ZString entryNumber, ZString filerCode, params ZString[] messageCodeTypes)
			{
				CusEntryHeader result = null;
				foreach (ZString messageType in messageCodeTypes)
				{
					result = FindByEntryNumberAndMessageType(companyPK, entryNumber,
						messageType,
						(CusEntryHeader entryHeader) => entryHeader.EntryFilerCode == filerCode);
					if (result != null)
					{
						break;
					}
				}
				return result;
			}

			public CusEntryHeader FindDuplicate(CusEntryHeader entry)
			{
				return FindByEntryNumberAndMessageType(entry.Declaration.JE_GC, entry.EntryNumber,
					entry.CH_MessageType,
					(CusEntryHeader entryHeader) => entryHeader.PK != entry.PK && entryHeader.EntryFilerCode == entry.EntryFilerCode);
			}

			public CusEntryHeader FindByEntryNumberAndMessageType(ZGuid companyPK, ZString entryNumber, ZString messageType)
			{
				return FindByEntryNumberAndMessageType(companyPK, entryNumber, messageType, (CusEntryHeader entryHeader) => true);
			}

			CusEntryHeader FindByEntryNumberAndMessageType(ZGuid companyPK, ZString entryNumber, ZString messageType, Predicate<CusEntryHeader> extraFilter)
			{
				Argument.NotNull(extraFilter, "extraFilter");

				CusEntryHeader result = null;
				ZQuery parentFilter = new ZQuery(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				parentFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_ParentTable, JobDeclarationSchema.Constants.TableName);

				ZQuery entryNumberFilter = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
				entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);

				if (messageType == CusEntryHeaderMessageTypeList.Codes.Export)
				{
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.Export);
				}
				else
				{
					entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				}

				entryNumberFilter.AddToFilter(parentFilter);

				ZQuery cusEntryHeaderFilter = new ZQuery(CusEntryHeaderSchema.CH_MessageType, messageType);
				foreach (CusEntryNumber bizO in Factory.Load<CusEntryNumber>(entryNumberFilter))
				{
					JobDeclaration declaration = null;
					switch (bizO.CE_ParentTable)
					{
						case CusEntryHeaderSchema.Constants.TableName:
							CusEntryHeader entryHeader = Factory.Load(typeof(Customs.Business.CusEntryHeader), bizO.CE_ParentID) as CusEntryHeader;
							if (entryHeader != null && entryHeader.CH_MessageType == messageType)
							{
								declaration = entryHeader.Declaration;
							}
							if (declaration != null &&
								declaration.JE_GC == companyPK &&
								extraFilter(entryHeader))
							{
								result = entryHeader;
							}
							break;
						case JobDeclarationSchema.Constants.TableName:
							declaration = Factory.Load(typeof(BaseJobDeclaration), bizO.CE_ParentID) as JobDeclaration;
							if (declaration != null &&
								declaration.JE_GC == companyPK)
							{
								var entries = (CusEntryHeader[])declaration.ActiveEntryHeaders.Find(cusEntryHeaderFilter);
								if (entries.Length > 0 && extraFilter(entries[0]))
								{
									result = entries[0];
								}
							}
							break;
					}

					if (result != null)
					{
						break;
					}
				}
				return result;
			}

			protected override Customs.Business.CusEntryHeader FindByEntryNumberAndCurrentCompanyCore(string entryNumber)
			{
				ErrorReporter.ReportOnce("FindByEntryNumberAndCurrentCompany Is Not Supported", "This method is not valid for US Customs; please use FindByEntryNumberAndType.");
				return null;
			}

			protected override Customs.Business.CusEntryHeader FindByEntryNumberAndCurrentCompanyCore(string entryNumber, Predicate<Customs.Business.CusEntryHeader> entryHeaderFilter)
			{
				ErrorReporter.ReportOnce("FindByEntryNumberAndCurrentCompany Is Not Supported", "This method is not valid for US Customs; please use FindByEntryNumberAndType.");
				return null;
			}
		}

		#endregion

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : TypeSafeCusEntryHeader.Schema
		{
			public const string JE_DateOfArrival = "JE_DateOfArrival";
			public const string JE_EntryAuthorisationDate = "JE_EntryAuthorisationDate";
			public const string US_SuretyCode = "US_SuretyCode";
			public const string IOROrgPK = "IOROrgPK";
			public const string US_SchDEntry = "US_SchDEntry";
			public const string UniqueReference = "UniqueReference";
			public const string EntryFilerCode = "EntryFilerCode";
			public const string US_SendWithdrawn = "US_SendWithdrawn";
			public const string US_SendReplace = "US_SendReplace";
			public const string EntryType = "EntryType";

			public const int AESTIRCH_BGMReferenceMaxLength = 17;
		}

		#endregion

		#region Export Details

		public ZBool IsForeignTradeZoneRequired
		{
			get { return InbondType == InbondTypeList.Codes.IEForeignTradeZoneWithdrawal || InbondType == InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal; }
		}

		public ZString StateOfOrigin
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_StateOfOrigin : ZString.Empty;
			}
		}

		public ZString ForeignTradeZone
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_ForeignTradeZone : ZString.Empty;
			}
		}

		public ZString PortOfExportInUSFormat
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_SchDExport : ZString.Empty;
			}
		}

		public ZString CountryOfUltimateDestination
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				if (declaration != null)
				{
					if (this.RandomHeader != null && !this.RandomHeader.US_UltimateDestinationCountry.IsEmpty)
					{
						result = this.RandomHeader.US_UltimateDestinationCountry;
					}
					else
					{
						result = declaration.US_RN_NKCountryOfDestination;
					}
				}

				return result;
			}
		}

		public ZString PortOfArrivalInUSFormat
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_SchDArrival : ZString.Empty;
			}
		}

		public ZDateTime ExportDate
		{
			get
			{
				var invoiceLine = (JobComInvoiceLine)this.InvoiceLines.FirstOrDefault();
				return invoiceLine != null ? invoiceLine.US_DateOfExport : ZDateTime.Empty;
			}
		}

		public ZString ModeOfTransport
		{
			//For Reconciliation informal fee calculation, JE_Calc_USTransportMode is stored against US_TransportMode
			get { return Declaration.JE_Calc_USTransportMode; }
		}

		public ZString CarrierCode
		{
			get
			{
				var declaration = this.Declaration;
				return (declaration != null) ? declaration.CarrierCode : ZString.Empty;
			}
		}

		public ZString TransportationReferenceNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(Declaration.US_TransportReference, ABICharacterTypeString.Constants.Special, 30); }
		}

		public ZString ExportingCarrier
		{
			get
			{
				var result = ZString.Empty;
				var declaration = this.Declaration;
				if (declaration != null && declaration.IsExportingCarrierRequired())
				{
					var shippingLine = declaration.ShippingLine;

					if (declaration.IsSea)
					{
						result = declaration.JE_VesselName;
					}
					else if (shippingLine != null)
					{
						if (declaration.IsAir)
						{
							if (shippingLine.MiscServ.Airline != null)
							{
								result = shippingLine.MiscServ.Airline.RM_AirlineName1;
							}
						}
						else
						{
							result = shippingLine.OH_FullNameTruncated;
						}
					}

					if (result.IsEmpty)
					{
						if (declaration.IsUnknownCarrierSCACForExport)
						{
							result = declaration.US_CarrierName;
						}
						else
						{
							var importingCarrier = declaration.ImportingCarrier;
							if (importingCarrier != null)
							{
								result = importingCarrier.UI_Name;
							}
						}
					}
				}
				return result;
			}
		}

		public ZString VesselCountryOfRegistration
		{
			get
			{
				var result = ZString.Empty;
				var declaration = this.Declaration;
				if (declaration != null && declaration.IsSea)
				{
					var vessel = declaration.Vessel;
					if (vessel != null && vessel.CountryOfReg != null)
					{
						result = vessel.RV_RN_NKCountryOfReg;
					}
				}
				return result;
			}
		}

		public ZBool IsTransactionsRelated
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_IsTransactionsRelated : ZBool.False;
			}
		}

		public ZBool IsHazardousCargo
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_IsHazardousCargo : ZBool.False;
			}
		}

		public ZBool IsRoutedTransaction
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_IsRoutedTransaction : ZBool.False;
			}
		}

		public ZString ImportEntryNumber
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_ImportEntryNo : ZString.Empty;
			}
		}

		public ZString InbondType
		{
			get
			{
				var randomHeader = this.RandomHeader;
				return (randomHeader != null) ? randomHeader.US_InbondType : ZString.Empty;
			}
		}

		public ZBool HasMultipleECCN
		{
			get { return MergedLines.HasMultipleECCN; }
		}

		public ZBool HasMultipleLicenseDetails
		{
			get { return MergedLines.HasMultipleLicenseDetails; }
		}

		public ZBool Has98130075Articles
		{
			get { return MergedLines.Has98130075Articles; }
		}

		public ZBool HasPGALinesRequireThreeTimesCustomsValue
		{
			get { return MergedLines.HasPGALinesRequireThreeTimesCustomsValue; }
		}

		public ZBool HasVisaOrQuotaLines
		{
			get { return VisaOrQuotaLinesMerchandiseValue > 0; }
		}

		public ZDecimal VisaOrQuotaLinesMerchandiseValue
		{
			get { return MergedLines.VisaOrQuotaLinesMerchandiseValue; }
		}

		public ZDecimal NonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees
		{
			get { return MergedLines.NonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees; }
		}

		public ZString SEDString
		{
			get
			{
				if (sEDStringCached == null)
				{
					sEDStringCached = new CachedProperty<ZString>(Factory, delegate
					{
						var generator = new AESInputBlockControlGenerator(this);
						generator.AddMessageBlocks(new AESTIRMessageBlockBuilder(this).Build(MessageAction));
						return generator.Serialise(true);
					});
				}
				return sEDStringCached.Value;
			}
		}
		CachedProperty<ZString> sEDStringCached;

		public ZPropertyInfo SEDStringInfo
		{
			get { return GetZPropertyInfo(nameof(SEDString)); }
		}

		public UpdateActionCode MessageAction
		{
			get { return US_SendReplace ? UpdateActionCode.Replace : HasBeenLodgedAtCustoms && !IsCurrentlyWithdrawn ? (US_SendWithdrawn ? UpdateActionCode.Delete : UpdateActionCode.Replace) : UpdateActionCode.Add; }
		}

		IDisposable IAESEntry.SuspendMarkingAsNeedingValidation()
		{
			return SuspendMarkingAsNeedingValidation();
		}

		protected override bool EntryNumber_ReadOnly
		{
			get { return CH_Status != AESDirectCustomsEntryStatus.Codes.NotSent || US_IsDeactivated; }
		}

		[ReadOnlyMember(nameof(US_XTN_ReadOnly))]
		public override ZString US_XTN
		{
			get => base.US_XTN;
			set => base.US_XTN = value;
		}

		bool US_XTN_ReadOnly
		{
			get { return EntryNumber_ReadOnly; }
		}

		public USOrganisation USPPI
		{
			get { return RandomHeader.US_USPPI; }
		}

		public USOrganisation ExportUltimateConsignee
		{
			get { return RandomHeader.US_ExportUltimateConsignee; }
		}

		public USOrganisation IntermediateConsignee
		{
			get { return RandomHeader.US_IntermediateConsignee; }
		}

		public bool US_IsDeactivationHasChanges
		{
			get
			{
				return GetAddInfo().HasChangesSinceLastSaving(USAddInfoSchema.US_IsDeactivated);
			}
		}

		#endregion

		#region Status and Logs

		public bool IsPaidViaStatement
		{
			get
			{
				if (IsFormalEntry)
				{
					CusStatementLine statementLine = new CusStatementLine.Loader(Factory).LoadTop1NotDeleted(EntryNumber, EntryFilerCode, RegistryCompanyPK);
					return statementLine != null && statementLine.IsPaid;
				}
				else
				{
					return false;
				}
			}
		}

		public override bool HasTransactionsWithCustoms
		{
			get { return HasBeenLodgedAtCustoms || IsWaitingForResponse || HasBeenWithdrawn; }
		}

		internal bool IsOKToBeDeactivated
		{
			get { return !HasBeenLodgedAtCustoms && !IsWaitingForResponse && !IsImportEntryStatusRejected && (!HasBeenWithdrawn || IsOKToBeDeactivatedWhenWithdrawn) && (!IsExport || CH_Status.IsEmpty); }
		}

		bool IsOKToBeDeactivatedWhenWithdrawn
		{
			get { return IsCargoRelease || IsBorderCargoRelease || IsACECargoRelease; }
		}

		public override bool HasBeenLodgedAtCustoms
		{
			get { return !IsFTZAdmission && LogManager.HasAClearLog(CusEntryHeaderMessageTypeList.GetMessagesTypesRightFor(CH_MessageType), StatusList); }
		}

		public override bool HasBeenWithdrawn
		{
			get { return !IsFTZAdmission && LogManager.HasAWithdrawnLog; }
		}

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			return !HasBeenWithdrawn;
		}

		public bool IsCurrentlyWithdrawn
		{
			get { return CH_Status == AESDirectCustomsEntryStatus.Codes.DeleteSEDClear; }
		}

		public bool IsCustomsChargeToBeCalculated
		{
			get { return IsFTZAdmission || IsFormalEntry; }
		}

		public bool HasActiveTransactionsWithCustoms
		{
			get { return IsActive && (IsWaitingForResponse || HasBeenLodgedAtCustoms || HasBeenWithdrawn); }
		}

		public bool HasBeenCancelled
		{
			get { return CH_Status == ImportMessageStatusList.Codes.EntrySummaryCanceled; }
		}

		public bool CanSendOriginal
		{
			//For ENS if first original was sent without broker reference, the second original will be rejected.
			//As we send broker reference now, CanSendOriginal is now allowed always
			get
			{
				if (IsFormalEntry)
				{
					return true;
				}
				else if (IsACECargoRelease)
				{
					return Declaration.US_EnableCRL && !HasBeenLodgedAtCustoms;
				}

				return !HasBeenLodgedAtCustoms;
			}
		}

		public bool CanSendWithdrawal
		{
			get
			{
				if (IsACECargoRelease || IsFormalEntry)
				{
					return true;
				}

				return HasBeenLodgedAtCustoms;
			}
		}

		public override bool IsWaitingForResponse
		{
			get
			{
				var entrySummaryEntry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
				var isWaitingForResponseForACECargoRelease = IsACECargoRelease && entrySummaryEntry != null && entrySummaryEntry.IsWaitingForResponse && Declaration.IsCargoReleaseBeingCertified;
				return !IsFTZAdmission && (StatusList.IsWaitingForResponse(CH_Status) || isWaitingForResponseForACECargoRelease);
			}
		}

		bool IsImportEntryStatusRejected
		{
			get { return IsFormalEntry && StatusList.RejectStatusInterested.ToList().Contains(CH_Status); }
		}

		public bool IsRelevantFor(ImportMessageStatusList.MessageType messageType)
		{
			switch (messageType)
			{
				case ImportMessageStatusList.MessageType.BorderCargoRelease:
					return IsBorderCargoRelease;

				case ImportMessageStatusList.MessageType.CargoRelease:
					return IsCargoRelease;

				case ImportMessageStatusList.MessageType.ACECargoRelease:
					return IsACECargoRelease;

				case ImportMessageStatusList.MessageType.Export:
					return IsExport;

				case ImportMessageStatusList.MessageType.EntrySummary:
					return IsFormalEntry;

				case ImportMessageStatusList.MessageType.InBondDeparture:
				case ImportMessageStatusList.MessageType.InBondUpdate:
					return IsInBond;

				case ImportMessageStatusList.MessageType.TemporaryImportationBond:
					return IsTemporaryImportationBond;
			}
			return false;
		}

		public override bool IsActive
		{
			get { return !US_IsDeactivated; }
			set { US_IsDeactivated = !value; }
		}

		public IStatusList StatusList
		{
			get { return (IStatusList)Lookups.MessageStatusList; }
		}

		protected override bool IsStatusClear(string status)
		{
			return !IsFTZAdmission && StatusList.IsStatusClear(status);
		}

		public override bool IsClearedEntry
		{
			get { return IsStatusClear(CH_Status) && !HasBeenWithdrawn; }
		}

		protected override bool HasBeenLodgedAtCustomsForAccIntegration
		{
			get
			{
				return base.HasBeenLodgedAtCustomsForAccIntegration &&
					LogManager.HasAClearLog(new[]
						{
							ImportMessageStatusList.Codes.ClearEntrySummaryOriginal,
							ImportMessageStatusList.Codes.ClearEntrySummaryReplace,
							ImportMessageStatusList.Codes.ClearEntrySummaryDelete
						});
			}
		}

		protected override bool IsChangingToClearStatusForAccIntegration
		{
			get
			{
				return IsInDatabase
					&& !IsStatusClearForAccIntegration((ZString)CH_StatusInfo.OriginalValue)
					&& IsStatusClearForAccIntegration(CH_Status);
			}
		}

		bool IsStatusClearForAccIntegration(ZString status)
		{
			return IsStatusClear(status) && IsStatusNOTWarningForAccIntegration(status);
		}

		bool IsStatusNOTWarningForAccIntegration(ZString status)
		{
			return !DataRegistry.Business.USCustomsDataRegistry.Instance.SuppressAutoBillingDisbursementWhenCensusWarningExists.GetValueWithoutFallback(Declaration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty)
				|| (status != ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings
				&& status != ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings);
		}

		internal StatusLogManager LogManager
		{
			get
			{
				if (fLogManager == null)
				{
					fLogManager = new StatusLogManager(Logs, Branch);
				}
				return fLogManager;
			}
		}
		StatusLogManager fLogManager;

		internal bool HasCargoReleaseBeenCertified
		{
			get { return US_CRLCertStatus == CargoReleaseCertificationStatusList.Codes.Certified; }
		}

		internal bool IsCargoReleaseBeingCertified
		{
			get { return US_CRLCertStatus == CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending; }
		}

		public bool EffectiveHasCargoReleaseBeenCertified
		{
			get
			{
				bool result = HasCargoReleaseBeenCertified;

				if (!result)
				{
					var relatedENSEntry = this.RelatedENSEntry;
					if (relatedENSEntry != null)
					{
						result = relatedENSEntry.HasCargoReleaseBeenCertified;
					}
					else if (IsFormalEntry)
					{
						CusEntryHeader cargoReleaseEntry = RelatedCRLEntry;
						result = cargoReleaseEntry != null && cargoReleaseEntry.HasCargoReleaseBeenCertified;
					}
				}

				return result;
			}
		}

		#endregion

		#region Other methods

		public override bool ShouldPopulateJE_EntrySubmittedDate
		{
			get { return Declaration.JE_EntrySubmittedDate.IsEmpty && (IsFormalEntry || IsExport || IsReconEntry || IsFTZAdmission); }
		}

		public static ZString GetFormmattedFilerCodeAndEntryNumber(ZString entryFilerCode, ZString entryNumber)
		{
			return entryFilerCode.IsEmpty && entryNumber.IsEmpty ? string.Empty : entryFilerCode + "-" + entryNumber.SubstringSafe(0, 7) + "-" + entryNumber.SubstringSafe(7, 1);
		}

		protected override void ResetCachedValues()
		{
			base.ResetCachedValues();
			aDDOrCVDDutyDetailsCalculated = false;
			isTotalCustomsValueOfLinesWithMPFCalculated = false;
			fdaLines = null;
			uniqueCountryOfExport = null;
			uniqueCountryOfOrigin = null;
			uniquePortOfLading = null;
			uniqueAgricultureLicenseNumber = null;
			hasMultipleManufacturerIDsCached = null;
			hasMultiExportDates = null;
		}

		public override void Delete()
		{
			var declaration = Declaration;

			if (declaration != null)
			{
				var hasMessages = Messages.Count > 0;
				if (!declaration.SuspendDeleteEntryLogging && IsInDatabase && !IsDeleted)
				{
					if (!HasBeenWithdrawn && (HasBeenLodgedAtCustoms || IsWaitingForResponse))
					{
						LogEntryHeaderDeletionWhenIsShouldNotBeDeleted(declaration, "Deleting US Customs Pending CusEntryHeader", "A Customs Pending CusEntryHeader was deleted\r\nPlease let Customs Team know\r\n");
					}
					else if (hasMessages && IsExport)
					{
						LogEntryHeaderDeletionWhenIsShouldNotBeDeleted(declaration, "Deleting US Export Customs CusEntryHeader with messages", "An US Export Customs CusEntryHeader with messages was deleted\r\nPlease let Customs Team know\r\n");
					}
				}
				if (hasMessages)
				{
					Messages.DiscardAll(Declaration);
				}
			}

			AESCusDispositions.RemoveAndDeleteAll();

			MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, this, MessageAttacheeActionType.Deleted);
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (GenerateFormalEntryNumberOnSaving)
			{
				FillInFormalEntryNumber();
			}
			else
			{
				FillInInBondNumberIfInBond();
			}
			ReCalculateDeclarationStatus();
		}

		internal void SetAllocateInbondNumberOnSaving()
		{
			allocateInbondNumberOnSaving = true;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (allocateInbondNumberOnSaving)
			{
				FillInInBondNumberIfInBond();
			}
		}
		bool allocateInbondNumberOnSaving;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			allocateInbondNumberOnSaving = false;
		}

		bool GenerateFormalEntryNumberOnSaving
		{
			get
			{
				bool result = false;
				var declaration = this.Declaration;
				if (declaration == null || !declaration.IsImportByExternalBroker)
				{
					result = IsFormalEntry || IsRelatedToENSEntry;
				}

				return result;
			}
		}

		internal void FillInFormalEntryNumber()
		{
			var ensEntry = RelatedENSEntry;
			if (ensEntry != null)
			{
				ensEntry.FillInFormalEntryNumber();
			}
			else if (EntryNumber.IsEmpty)
			{
				ZString entryNumber;
				if (EntryNumberGenerator.TryGetNextEntryNumber(EntryBranch, EntryFilerCode, out entryNumber))
				{
					ZString oldValue = EntryNumber;
					EntryNumber = entryNumber;
					CusEntryNumber.CE_EntryIsSystemGenerated = true;

					if (!Declaration.IsReconMessageType)
					{
						Declaration.SetMasterBillForSouthOriginatedTruckShipment();
					}

					var supporter = Declaration.GetAllocateNumberSupporter();
					if (supporter != null)
					{
						supporter.OnAllocatedNumberSaved();
					}
				}
			}
		}

		internal void FillInInBondNumberIfInBond()
		{
			if (EntryNumber.IsEmpty && IsInBond)
			{
				ZString inbondNumber;
				if (InBondNumberGenerator.TryGetNextInBondNumber(EntryBranch, out inbondNumber))
				{
					EntryNumber = inbondNumber;

					CusEntryNumber.CE_EntryIsSystemGenerated = true;
				}
				InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(EntryBranch);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				GetAddInfo().ResetToOriginalValue(USAddInfoSchema.US_CRLCertStatus);
				Declaration.GetAddInfo().ResetToOriginalValue(USAddInfoSchema.US_AESBGMRefLastUsed);
			}
			else if (publishExportEntryNumber)
			{
				var dataProvider = ObjectFactory.Get<Integration.Customs.US.IUSUniversalCustomsDataObjectProvider>();
				dataProvider.PublishDeclarationUniversalEvent(this.Declaration);
			}
		}

		public override Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryBranchPK();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		Guid GetRegistryBranchPK()
		{
			return base.RegistryBranchPK;
		}

		public override Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryCompanyPK();
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		Guid GetRegistryCompanyPK()
		{
			return base.RegistryCompanyPK;
		}

		[BusinessObjectTestExclude]
		public ZBool US_SendWithdrawn
		{
			get { return fUS_SendWithdrawn && CanSendWithdrawal; }
			set
			{
				if (SetNonPersistentPropertyValue(US_SendWithdrawnInfo, ref fUS_SendWithdrawn, value))
				{
					SEDStringInfo.RefreshBinding();
				}
			}
		}
		ZBool fUS_SendWithdrawn;

		protected bool US_SendWithdrawn_ReadOnly
		{
			get { return !CanSendWithdrawal || US_IsDeactivated; }
		}

		public ZPropertyInfo US_SendWithdrawnInfo
		{
			get { return GetZPropertyInfo(Schema.US_SendWithdrawn); }
		}

		[BusinessObjectTestExclude]
		public ZBool US_SendReplace
		{
			get { return fUS_SendReplace; }
			set
			{
				if (SetNonPersistentPropertyValue(US_SendReplaceInfo, ref fUS_SendReplace, value))
				{
					SEDStringInfo.RefreshBinding();
				}
			}
		}
		ZBool fUS_SendReplace;

		protected bool US_SendReplace_ReadOnly
		{
			get { return HasTransactionsWithCustoms; }
		}

		public ZPropertyInfo US_SendReplaceInfo
		{
			get { return GetZPropertyInfo(Schema.US_SendReplace); }
		}

		#endregion

		#region Calculated Properties

		public ZString US_SchDEntry
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_SchDEntry : ZString.Empty;
			}
		}

		public ZPropertyInfo US_SchDEntryInfo
		{
			get { return GetZPropertyInfo(Schema.US_SchDEntry); }
		}

		public ZString UniqueReference
		{
			get { return IsExport ? CH_BGMReference : (ZString)(EntryFilerCode + EntryNumber); }
		}

		public OrgHeader ImportUltimateConsignee
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.ConsigneeOrgAddress : null;
			}
		}

		public ZString EntryTypeName
		{
			get { return Lookups.US_EntryTypeList.GetDescriptionFromCode(EntryType); }
		}

		public OrgHeader Importer
		{
			get { return (RandomHeader == null) ? null : RandomHeader.Importer; }
		}

		public OrgHeader ImporterOfRecord
		{
			get { return (RandomHeader == null) ? null : RandomHeader.ImporterOfRecord; }
		}

		public bool IsBorderCargoRelease
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease; }
		}

		public bool IsCargoRelease
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.CargoRelease; }
		}

		public bool IsACECargoRelease
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ACECargoRelease; }
		}

		public override bool IsFormalEntry
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.EntrySummary; }
		}

		public bool IsFTZAdmission
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone; }
		}

		public bool IsInBond
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.InBond; }
		}

		public bool IsReconEntry
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconEntry; }
		}

		public bool IsReconImportEntry
		{
			get { return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry; }
		}

		protected override bool IsExportCore() => CH_MessageType == CusEntryHeaderMessageTypeList.Codes.Export;

		/// <summary>
		/// TODO WI00003224
		/// ENS: until Summary appears on a statement or duty is paid
		/// CRL: until Cargo selectivity is issued or parent ENS is finalised
		/// </summary>
		public
#if DEBUG
 virtual //mocking
#endif
			bool IsElectronicAmendmentAllowed
		{
			get { return true; }
		}

		public bool IsTemporaryImportationBond
		{
			get { return IsFormalEntry && EntryType == EntryTypeList.Codes.TemporaryImportationBond; }
		}

		public bool IsExhibition
		{
			get { return IsFormalEntry && EntryType == EntryTypeList.Codes.PermanentExhibition; }
		}

		public bool IsEBondMessageAttacher
		{
			get
			{
				var result = false;

				if (IsFormalEntry)
				{
					var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USeBond);
					query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
					query.AddToFilter(EDIMessageSchema.EM_IsActive, true);

					result = Factory.LoadTop1<EBondEDIMessage>(query) != null;
				}

				return result;
			}
		}

		public ZDecimal ExhibitionEstimatedDutiesIfEntryHadBeenForConsumption
		{
			get
			{
				var result = ZDecimal.Zero;

				foreach (CusEntryLine line in MergedLines)
				{
					if (line.ParentLine != null)
					{
						result = result + line.ParentChildLineDuty;
					}
					else
					{
						result = result + line.DutyAmount;
					}
				}

				return result;
			}
		}

		public bool MayRequireWarehousing
		{
			get { return IsFormalEntry && EntryTypeList.MayRequireWarehousing(EntryType); }
		}

		public bool IsExWarehouseEntryType
		{
			get { return IsFormalEntry && EntryTypeList.IsExWarehouseType(EntryType); }
		}

		public bool IsConsumptionFTZEntryType
		{
			get
			{
				var result = false;
				if (IsFormalEntry)
				{
					var declaration = Declaration;
					result = declaration != null && declaration.IsConsumptionFTZ;
				}
				return result;
			}
		}

		public bool IsQuotaOrVisaEntryType
		{
			get { return IsFormalEntry && EntryTypeList.IsQuotaVisa(EntryType); }
		}

		public ZDecimal TotalCustomsValueOfLinesWithMPF
		{
			get
			{
				if (!isTotalCustomsValueOfLinesWithMPFCalculated)
				{
					isTotalCustomsValueOfLinesWithMPFCalculated = true;

					fTotalCustomsValueOfLinesWithMPF = 0m;
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) > 0m)
						{
							fTotalCustomsValueOfLinesWithMPF += invoiceLine.JI_CustomsValue;//CusEntryLine.CL_CustomsValue is rounded
						}
					}
				}
				return fTotalCustomsValueOfLinesWithMPF;
			}
		}
		ZDecimal fTotalCustomsValueOfLinesWithMPF;
		bool isTotalCustomsValueOfLinesWithMPFCalculated;

		public ZDateTime JE_DateOfArrival
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.JE_DateOfArrival : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JE_DateOfArrivalInfo
		{
			get { return GetZPropertyInfo(Schema.JE_DateOfArrival); }
		}

		public ZDateTime JE_EntryAuthorisationDate
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.JE_EntryAuthorisationDate : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo JE_EntryAuthorisationDateInfo
		{
			get { return GetZPropertyInfo(Schema.JE_EntryAuthorisationDate); }
		}

		public ZString US_SuretyCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_SuretyCode : ZString.Empty;
			}
		}

		public ZPropertyInfo US_SuretyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_SuretyCode); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoCusEntryHeaderLookups.Consignees))]
		public ZGuid IOROrgPK
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.IOROrgPK : ZGuid.Empty;
			}
		}

		public ZPropertyInfo IOROrgPKInfo
		{
			get { return GetZPropertyInfo(Schema.IOROrgPK); }
		}

		public ZBool HasMixedRelationshipIndicators
		{
			get
			{
				if (hasMixedRelationshipIndicatorsCached == null)
				{
					hasMixedRelationshipIndicatorsCached = new CachedProperty<ZBool>(Factory, delegate
					{
						var savedRelationship = "";

						foreach (var line in EntryLines)
						{
							var randomLine = line.RandomLine;
							if (randomLine.US_TransactionsRelated != savedRelationship && !string.IsNullOrEmpty(savedRelationship))
							{
								return true;
							}

							if (randomLine.IsVParentLine)
							{
								foreach (var child in randomLine.ChildVLines)
								{
									if (child.US_TransactionsRelated != savedRelationship && !string.IsNullOrEmpty(savedRelationship))
									{
										return true;
									}
									savedRelationship = child.US_TransactionsRelated;
								}
							}
							savedRelationship = randomLine.US_TransactionsRelated;
						}
						return false;
					});
				}
				return hasMixedRelationshipIndicatorsCached.Value;
			}
		}
		CachedProperty<ZBool> hasMixedRelationshipIndicatorsCached;

		public ZDateTime LiquidationDate
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.LiquidationDate : ZDateTime.Empty;
			}
		}

		public ZString LiquidationType
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.LiquidationType : ZString.Empty;
			}
		}

		#endregion

		#region Related Objects

		public PSCReasonCusCodeData PSCReasonCodes
		{
			get { return pscReasonCodes ?? (pscReasonCodes = new PSCReasonCusCodeData.Loader(Factory).Load(this)); }
		}
		PSCReasonCusCodeData pscReasonCodes;

		public PSCExplanationCusAddInfo PSCExplanation
		{
			get { return pscExplanation ?? (pscExplanation = new PSCExplanationCusAddInfo.Loader(Factory).Load(this)); }
		}
		PSCExplanationCusAddInfo pscExplanation;

		public bool IsRelatedTo(CusEntryHeader anotherEntry)
		{
			return RelatedENSEntry == anotherEntry || RelatedCRLEntry == anotherEntry || RelatedBCREntry == anotherEntry;
		}

		public CusEntryHeader RelatedENSEntry
		{
			get { return (IsRelatedToENSEntry) ? Factory.Load<CusEntryHeader>(CH_CH_PrimeEntry) : null; }
		}

		public CusEntryHeader RelatedBCREntry
		{
			get { return GetFirstRelatedEntry(CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease); }
		}

		public CusEntryHeader RelatedCRLEntry
		{
			get { return GetFirstRelatedEntry(new ZString[] { CusEntryHeaderMessageTypeList.Codes.CargoRelease, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease }); }
		}

		CusEntryHeader GetFirstRelatedEntry(params ZString[] messageCodeTypes)
		{
			CusEntryHeader result = null;
			var declaration = this.Declaration;
			if (IsFormalEntry && declaration != null)
			{
				ZQuery query = new ZQuery(CusEntryHeaderSchema.CH_CH_PrimeEntry, PK);
				query.AddToFilter(CusEntryHeaderSchema.CH_MessageType, messageCodeTypes);
				CusEntryHeader[] entries = declaration.CustomsEntryHeaders.Find(query);
				if (entries.Length > 0)
				{
					result = entries[0];
				}
			}
			return result;
		}

		public ReconOriginalEntryHeader ReconOriginalEntry
		{
			get { return reconOriginalEntry; }
			set
			{
				if (reconOriginalEntry != null && reconOriginalEntry != value)
				{
					ErrorReporter.ReportOnce("ReconOriginalEntryHeader in CusEntryHeader", "You are not supposed to set ReconOriginalEntry more than once.");
				}
				reconOriginalEntry = value;
			}
		}
		ReconOriginalEntryHeader reconOriginalEntry;

		#endregion

		#region Collections

		IEnumerable<IBillDetails> ICusEntryHeader.LowestBillDetails
		{
			get { return Declaration.GetLowestBillDetails(); }
		}

		public LowestBillCollection<Bill, JobDeclaration> LowestBillDetails => (LowestBillCollection<Bill, JobDeclaration>)Declaration.LowestBills;

		[ChildEditable(true)]
		public US7501DocPrintingCollection US7501DocPrintingData
		{
			get
			{
				if (us7501DocPrintingData == null)
				{
					us7501DocPrintingData = new US7501DocPrintingCollection(this);
					us7501DocPrintingData.Load();
					RegisterEditableChildObject(us7501DocPrintingData);
				}
				return us7501DocPrintingData;
			}
		}
		US7501DocPrintingCollection us7501DocPrintingData;

		#endregion

		#region Override

		public ZString US_PaymentType
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_PaymentType : ZString.Empty;
			}
		}

		public ZDateTime US_PreliminaryStatementPrintDate
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_PreliminaryStatementPrintDate : ZDateTime.Empty;
			}
		}

		public ZString US_PeriodicStatementMM
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_PeriodicStatementMM : ZString.Empty;
			}
		}

		public ZString US_ClientBranchDesignation
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_ClientBranchDesignation : ZString.Empty;
			}
		}

		[BusinessObjectTestExclude]
		public override ZDateTime CH_EntryReleaseDate
		{
			get { return ZDateTime.Empty; }
			set
			{
				ErrorReporter.ReportOnce("CH_EntryReleaseDate", "CH_EntryReleaseDate should not be used in US Customs. For setting Release Date please use Declaration > JE_EntryAuthorisationDate.");
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(US_IsDeactivated_ReadOnly))]
		public override ZBool US_IsDeactivated
		{
			get { return base.US_IsDeactivated; }
			set
			{
				var oldValue = US_IsDeactivated;
				base.US_IsDeactivated = value;
				//this is an AddInfo proxy property and ListChanged would not be fired as addinfo is serialised and set to the value onSaving only
				//manual rebuilding is necessary
				Declaration.ActiveEntryHeaders.Rebuild();

				if (US_IsDeactivated)
				{
					if (HasBeenWithdrawn)
					{
						Messages.DiscardAll(this);
					}
					else if (IsOKToBeDeactivated)
					{
						Delete();
					}
					else
					{
						HasAESTIRMessageThatNeedsWithdrawn = !Declaration.IsAESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspended && IsAESTIRMessagingMode && !IsWaitingForResponse && CanSendWithdrawal;
					}
				}
				if (oldValue != US_IsDeactivated)
				{
					ActiveBusinessObjectCollection<CusEntryHeader>.RefreshAll(Factory);
				}
			}
		}

		internal bool HasAESTIRMessageThatNeedsWithdrawn
		{
			get;
			private set;
		}

		internal void ClearAESTIRMessageThatNeedToBeWithdrawnFlag()
		{
			HasAESTIRMessageThatNeedsWithdrawn = false;
		}

		bool US_IsDeactivated_ReadOnly
		{
			get { return true; }
		}

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked
		{
			//If this entry is created without invoice lines at the first place
			get
			{
				bool result = true;

				if (IsInBond)
				{
					var declaration = this.Declaration;
					result = declaration == null || declaration.InvoiceLines.Count > 0;
				}
				else if (IsReconImportEntry || IsReconEntry)
				{
					result = false;
				}

				return result;
			}
		}

		public override ZGuid CH_JE
		{
			get { return base.CH_JE; }
			set
			{
				bool hasChanges = base.CH_JE != value;
				base.CH_JE = value;
				if (!IsCopying && hasChanges)
				{
					var declaration = this.Declaration;
					if (declaration != null)
					{
						declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		internal ZString EntryNumberTypeInternal
		{
			get { return EntryNumberType; }
		}

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			return !StatusList.IsStatusClear(originalStatus) && StatusList.IsStatusClear(newStatus);
		}

		protected override ZString EntryNumberType
		{
			get { return IsRelatedToENSEntry || IsReconEntry ? (ZString)CusEntryHeaderMessageTypeList.Codes.EntrySummary : CH_MessageType; }
		}

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			CusEntryNumber result = null;
			JobDeclaration declaration = Declaration;
			if (declaration != null)
			{
				switch (EntryNumberType)
				{
					case CusEntryHeaderMessageTypeList.Codes.EntrySummary:
						result = declaration.ENSEntryNumber;
						break;
					case CusEntryHeaderMessageTypeList.Codes.InBond:
						result = declaration.INBEntryNumber;
						break;
					default:
						result = CusEntryNumber.Load(this, EntryNumberType, Core.Constants.CountryCodes.UnitedStates);
						break;
				}
			}
			return result;
		}

		protected override CusEntryNumber CreateCusEntryNumber()
		{
			CusEntryNumber result = null;
			JobDeclaration declaration = Declaration;
			if (declaration != null)
			{
				BusinessObject bizObj = this;
				switch (EntryNumberType)
				{
					case CusEntryHeaderMessageTypeList.Codes.EntrySummary:
					case CusEntryHeaderMessageTypeList.Codes.InBond:
						bizObj = declaration;
						break;
				}
				result = CusEntryNumber.New(bizObj, EntryNumberType, Core.Constants.CountryCodes.UnitedStates);
				result.CE_EntryIsSystemGenerated = true;
			}
			return result;
		}

		public override ZString CH_Status
		{
			get { return base.CH_Status; }
			set
			{
				ZString oldValue = base.CH_Status;
				base.CH_Status = value;
				var declaration = this.Declaration;
				if (!IsCopying && declaration != null)
				{
					if (oldValue != CH_Status)
					{
						if (ShouldAddLogForMessageStatus)
						{
							if (IsExport)
							{
								LogManager.AddALogIfNecessary(oldValue, CH_Status, StatusList);
							}
							else
							{
								if (StatusList.IsStatusClear(CH_Status))
								{
									LogManager.AddAClearLogIfNecessary(oldValue, CH_Status, StatusList);

									if (IsReconEntry && declaration != null)
									{
										declaration.LogCustomsClearedIfNeeded();
									}

									AddOrUpdatePaymentAmountToCustomsFromMessage();
								}
								else
								{
									LogManager.AddARejectLogIfNecessary(oldValue, CH_Status, StatusList);
								}
							}

							SetAsNotRequireToBeReportToCustoms();
						}

						if (!IsExport)
						{
							if (StatusList.IsWithdrawnStatus(CH_Status))
							{
								if (declaration.US_PaymentDueDate.IsValid)
								{
									declaration.US_PaymentDueDate = ZDateTime.Empty;
								}

								AddOrUpdatePaymentAmountToCustomsFromMessage();

								if (IsACECargoRelease)
								{
									declaration.CloseRelatedPermits();
								}
							}
							else if (StatusList.IsStatusClear(CH_Status))
							{
								if (IsFormalEntry)
								{
									var preliminaryStatementPD = LastENSAcceptedMessageBlock?.PreliminaryStatementPrintDate ?? ZDate.Empty;
									if (preliminaryStatementPD.IsValid)
									{
										declaration.US_PSDAccepted = LastENSAcceptedMessageBlock.PreliminaryStatementPrintDate;
									}
									else
									{
										declaration.US_PSDAccepted = declaration.US_PreliminaryStatementPrintDate;
									}

									if (declaration.JE_EntryAuthorisationDate.IsValid)
									{
										declaration.PopulatePaymentDueDateIfNeeded();
									}

									declaration.CloseRelatedPermits();
									MarkActionsTaken(this);
								}

								if (IsFormalEntry || IsBorderCargoRelease || IsCargoRelease)
								{
									declaration.TrySendStatementUpdate();
								}

								if (IsACECargoRelease)
								{
									ClearBillHolds();
								}
							}

							if (ShouldClearDutyAndMPFCalcDate)
							{
								ClearDutyAndMPFCalcDate();
							}
						}
					}
				}
			}
		}

		void MarkActionsTaken(CusEntryHeader cusEntryHeader)
		{
			var declaration = cusEntryHeader.Declaration;
			var errorsRecords = declaration.ENSStatusNotifications.Cast<ErrorsRecord>().Where(r => !r.message.ActionAuthorised && r.message.EM_ActionStatus == EM_ActionStatusList.Codes.Incomplete);
			foreach (var errorsRecord in errorsRecords)
			{
				errorsRecord.message.Logs.AddNew(Events.Authorised, StatusList.GetDescriptionFromCode(cusEntryHeader.CH_Status));
			}
			declaration.ReCalculateENSAction();
		}

		bool ShouldAddLogForMessageStatus
		{
			get
			{
				return CH_MessageType == CusEntryHeaderMessageTypeList.Codes.Export
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.CargoRelease
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ACECargoRelease
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.EntrySummary
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.InBond
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconEntry
					|| CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone;
			}
		}

		bool ShouldClearDutyAndMPFCalcDate
		{
			get
			{
				return CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal
					|| CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryReplace
					|| CH_Status == ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			}
		}

		void ClearDutyAndMPFCalcDate()
		{
			var oldDutyCalcDate = US_DutyCalcDate;
			US_DutyCalcDate = ZDateTime.Empty;
			Logs.AddNew(Events.ResetEntryMessageItemFunction, ZString.Format("Duty Calc date {0} was cleared for {1}", oldDutyCalcDate, StatusList.GetDescriptionFromCode(CH_Status)));

			US_MPFCalcDate = ZDateTime.Empty;
		}

		public void AddOrUpdatePaymentAmountToCustomsFromMessage()
		{
			if (IsFormalEntry)
			{
				var existingPayment = EntryPayInfos.Count > 0 ? EntryPayInfos[0] : null;

				if (HasBeenWithdrawn)
				{
					if (existingPayment != null)
					{
						existingPayment.Delete();
					}
				}
				else
				{
					var applicationIdentifier = IsACE ? ACEApplicationIdentifierCodeList.Codes.EntrySummary : ApplicationIdentifierCodeList.Codes.EntrySummary;
					MQEDIMessage lastENS = (MQEDIMessage)Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, applicationIdentifier, EDIMessage.Direction.Transmit);

					if (lastENS != null && lastENS.RelatedMessage != null && lastENS.RelatedMessage.IsENSCleared)
					{
						var entryPayment = existingPayment ?? EntryPayInfos.AddNew();

						entryPayment.C9_PaymentAmount = lastENS.TotalENSAmountDue;
					}
				}
			}
		}

		void ClearBillHolds()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				declaration.Bills.Cast<Bill>().ForEach(x => x.CU_MessageStatus = ZString.Empty);
			}
		}

		public override ZString CH_MessageType
		{
			get { return base.CH_MessageType; }
			set
			{
				ZString oldValue = CH_MessageType;
				base.CH_MessageType = value;
				if (!IsCopying && oldValue != CH_MessageType)
				{
					Charges.MarkAsNeedingValidation();
					Declaration?.InvoiceLines?.MarkAsNeedingValidation();
				}
			}
		}

		//Get used to populate CH_TotalPaid
		public override ZDecimal TotalAmountPayable
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (!EntryTypeList.IsNothingPayable(EntryType))
				{
					if (EntryTypeList.IsOnlyHMFPayable(EntryType))
					{
						result = HMFAmountForEntry;
					}
					else
					{
						result = TotalPayableIncludingDeferredTax - (IsTaxDeferred ? TotalEstimatedTax : ZDecimal.Zero);
					}
				}

				return result;
			}
		}

		public ZDecimal TotalPayableIncludingDeferredTax
		{
			get
			{
				ZDecimal result = 0m;

				var entry = (ICusEntryHeader)this;
				result += entry.TotalAntidumpingDuty;
				result += entry.TotalCountervailingDuty;
				result += entry.TotalEstimatedDuty;
				result += entry.TotalEstimatedTax;
				result += entry.GrandTotalFee;
				result += entry.GrandTotalOtherRevenueAmount;
				return result;
			}
		}

		public ZDecimal TotalAmountForBondCalculationUse
		{
			get { return TotalPayableIncludingDeferredTax; }
		}

		public override ZString EntryNumber
		{
			get
			{
				CusEntryHeader relatedENSEntry = RelatedENSEntry;
				if (relatedENSEntry != null)
				{
					return relatedENSEntry.EntryNumber;
				}
				return base.EntryNumber;
			}
			set
			{
				if (RelatedENSEntry != null)
				{
					throw new InvalidOperationException("EntryNumber for entries related to ens should not be settable. The number should be retrieved from ens entries");
				}

				publishExportEntryNumber = !value.IsEmpty && IsExport && base.EntryNumber != value;
				base.EntryNumber = value;
			}
		}
		bool publishExportEntryNumber;

		public bool IsRelatedToENSEntryWithoutENSEntry
		{
			get { return IsRelatedToENSEntry && RelatedENSEntry == null; }
		}

		public bool IsRelatedToENSEntry
		{
			get { return IsBorderCargoRelease || IsCargoRelease || IsACECargoRelease; }
		}

		public bool IsAESTIRMessagingMode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && IsExport;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusEntryHeaderFetchStrategy(this);
		}

		public int CH_BGMReference_MaxLength
		{
			get { return IsAESTIRMessagingMode ? Schema.AESTIRCH_BGMReferenceMaxLength : Schema.CH_BGMReferenceMaxLength; }
		}

		protected bool CH_BGMReference_ReadOnly
		{
			get { return IsExport && !US_SendReplace || HasTransactionsWithCustoms; }
		}

		public override ZDateTime US_CollectionDate
		{
			get { return base.US_CollectionDate; }
			set
			{
				var oldValue = US_CollectionDate;
				base.US_CollectionDate = value;

				if (oldValue != US_CollectionDate)
				{
					var declaration = Declaration;
					declaration.US_DeferredTaxDueDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(declaration);
				}
			}
		}

		#endregion

		#region Accounting Integration

		protected override EntryChargeTypeList GetEntryChargeTypeList()
		{
			return Factory.GetCachedValue<Registry.Business.Customs.US.EntryChargeTypeList>();
		}

		public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPaymentCode, ILogger logger)
		{
			return IsPaidByBroker && EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(Declaration.US_EntryType, feeCode);
		}

		protected override ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPayment)
		{
			ZDecimal result = 0m;

			if (EntryTypeList.IsCustomsChargeIncludedInTotalAmountDue(Declaration.US_EntryType, chargeTypeElement.Code))
			{
				if (Registry.Business.Customs.US.EntryChargeTypeList.IsFeeType(chargeTypeElement.Code))
				{
					result = Charges.GetAmount(chargeTypeElement.Code);//as fees persist against CusEntryHeader as well as CusEntryLine
				}
				else if (chargeTypeElement.Code == Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable)
				{
					if (!IsTaxDeferred)
					{
						result = TotalEstimatedTax;
					}
				}
				else
				{
					result = base.GetTotalChargeValueFor(chargeTypeElement, methodOfPayment);//for duty/ADD/CVD
				}
			}
			else if (chargeTypeElement.Code == Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred && !MayRequireWarehousing)
			{
				if (IsTaxDeferred)
				{
					result = TotalEstimatedTax;
				}
			}

			if (result < 0m)//For recon, refundable amount by Customs
			{
				result = 0m;//not billable for the importer
			}

			return result;
		}

		public bool IsPaidByBroker
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsPaidByBroker;
			}
		}

		#endregion

		#region Implementation

		protected override bool IsInwardBondedWarehousingEnabledCore
		{
			get { return Declaration?.IsWarehouseEntryType ?? false; }
		}

		protected override bool IsOutwardBondedWarehousingEnabledCore
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.IsExWarehouseEntryType || declaration.IsENSFormalImportAndConsumptionFTZ);
			}
		}

		protected override void AddExtraRequiredFieldsMessageError(ZStringBuilder messageErrors, bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			base.AddExtraRequiredFieldsMessageError(messageErrors, checkProduct, checkQuantity, checkEntryDetails);
			var declaration = Declaration;
			if (declaration != null)
			{
				if (declaration.IsInwardBondedWarehousingEnabled)
				{
					var whsWarehouse = WarehouseAddress.GetWhsWarehouse();
					if (whsWarehouse != null)
					{
						if (declaration.IsFTZAdmission)
						{
							if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.FreeTradeZone)
							{
								messageErrors.Append(JobDeclaration.OnlyFTZProductWarehouseTypeCanBeUsedForFTZAdmission(declaration.TermNameForBondedWarehouse));
							}
						}
						else
						{
							if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.Product)
							{
								messageErrors.Append(JobDeclaration.OnlyWarehouseWithProductWarehouseTypeCanBeUsedForInward(declaration.TermNameForBondedWarehouse));
							}
						}
					}
				}
				else if (declaration.IsOutwardBondedWarehousingEnabled)
				{
					var whsWarehouse = WarehouseAddress.GetWhsWarehouse();
					if (whsWarehouse != null)
					{
						if (declaration.IsConsumptionFTZ)
						{
							if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.FreeTradeZone)
							{
								messageErrors.Append(JobDeclaration.OnlyWarehouseWithFTZProductWarehouseTypeCanBeUsedForFTZOutward(declaration.TermNameForBondedWarehouse));
							}
						}
						else
						{
							if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.Product)
							{
								messageErrors.Append(JobDeclaration.OnlyWarehouseWithProductWarehouseTypeCanBeUsedForOutward(declaration.TermNameForBondedWarehouse));
							}
						}
					}
				}

				if (checkQuantity && declaration.HasABondedWarehousingLineWithoutWHSPackDetails)
				{
					messageErrors.Append(JobDeclaration.InvoiceLineMarkedForBondedWarehousingLineRequiresWHSPackDetails(declaration.TermNameForBondedWarehouse));
				}
				if (checkEntryDetails)
				{
					if (declaration.IsImport)
					{
						if (declaration.US_EnableENS && EntryTypeList.IsExWarehouseType(declaration.US_EntryType) && (declaration.US_WHSEntryFilerCode.IsEmpty || declaration.US_WHSEntryNumber.IsEmpty))
						{
							messageErrors.Append(JobDeclaration.WarehouseEntryFilerCodeAndNumberAreRequiredForBondedWarehousing(declaration.TermNameForBondedWarehouse));
						}
						if (IsFTZAdmission && FTZAdmissionNumber.IsEmpty)
						{
							messageErrors.Append(JobDeclaration.FTZAdmissionControlNumberComponentIsRequiredForBondedWarehousing(declaration.TermNameForBondedWarehouse));
						}
					}
				}
			}
		}

		protected override string GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage()
		{
			string result;
			var declaration = Declaration;
			if (declaration != null && (declaration.IsExWarehouseEntryType || declaration.IsImportByExternalBroker))
			{
				result = JobDeclaration.BondedWarehousingLineRequiresWHSEntryLine(declaration.TermNameForBondedWarehouse);
			}
			else if (declaration != null && declaration.IsENSFormalImportAndConsumptionFTZ)
			{
				result = JobDeclaration.BondedWarehousingLineRequiresWHSEntryNumberAndWHSEntryLine(declaration.TermNameForBondedWarehouse);
			}
			else
			{
				result = base.GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage();
			}
			return result;
		}

		void LogEntryHeaderDeletionWhenIsShouldNotBeDeleted(JobDeclaration declaration, string key, string messagePrefix)
		{
			var note = Factory.New<HiddenStmNote>();
			note.ST_ParentID = declaration.PK;
			note.ST_Table = declaration.TableName;
			note.ST_Description = "Deleted Entry Header"; // Debugging Data
			var dataBuilder = new ZStringBuilder("Entry Header"); // Debugging Data
			dataBuilder.Append(GetData(this));
			dataBuilder.Append("");
			dataBuilder.Append("MergeLines:"); // Debugging Data
			foreach (CusEntryLine mergedLine in MergedLines)
			{
				dataBuilder.Append("");
				dataBuilder.Append(GetData(mergedLine));
			}

			dataBuilder.Append("");
			dataBuilder.Append("CallStack:"); // Debugging Data
			dataBuilder.Append(new System.Diagnostics.StackTrace(true).ToString());

			note.ST_NoteDataAsText = dataBuilder.ToStringWithNewLineBetweenAppends();
			ErrorReporter.ReportOnce(key, messagePrefix + note.ST_NoteDataAsText); // DN - 9/11/11
		}

		ZString GetData(BusinessObject bizObj)
		{
			var row = ((IBusinessObjectInternals)bizObj).Row;
			var table = row.Table;
			var dataBuilder = new ZStringBuilder();
			foreach (DataColumn column in table.Columns)
			{
				var data = row[column] ?? "NULL";
				dataBuilder.Append(column.ColumnName + "=" + data.ToString());
			}
			return dataBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected override bool IsCustomsClearedEventSupported
		{
			get { return IsExport; }
		}

		protected override bool IsCustomsImpedimentReceivedEventSupported
		{
			get { return false; }
		}

		protected override CurrencyConverter CurrencyConverterCore
		{
			get { return currencyConverter ?? (currencyConverter = new CurrencyConverterWithDataProvider(Factory, Declaration)); }
		}
		CurrencyConverter currencyConverter;

		void SetAsNotRequireToBeReportToCustoms()
		{
			if (IsExport && IsWaitingForResponse)
			{
				US_ShouldBeReportToCustoms = false;
			}
		}

		void ReCalculateDeclarationStatus()
		{
			var declaration = this.Declaration;
			if (declaration != null && (!IsInDatabase || IsCH_StatusChanged || IsCH_EntryStatusChanged))
			{
				declaration.DeriveDeclarationStatus();
			}
		}

		public void SetTIBExpiryDate()
		{
			var dateOfImportation = ((ICusEntryHeader)this).DateOfImportation;

			if (dateOfImportation.IsValid)
			{
				if (Has98130075Articles)
				{
					US_TIBExpiryDate = dateOfImportation.AddMonths(6);
				}
				else
				{
					US_TIBExpiryDate = dateOfImportation.AddYears(1);
				}
			}
		}

		bool IsCH_EntryStatusChanged
		{
			get { return !CH_EntryStatus.Equals(CH_EntryStatusInfo.OriginalValue); }
		}

		bool IsCH_StatusChanged
		{
			get { return !CH_Status.Equals(CH_StatusInfo.OriginalValue); }
		}

		#endregion

		#region ICusEntryHeader Members

		public bool IsRemoteLocationFiling
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsRemoteLocationFiling;
			}
		}

		public ZBool IsElectronicInvoicing
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsElectronicInvoicing;
			}
		}

		ZBool ICusEntryHeader.IsInvoiceByRequest
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.US_IsInvoiceByRequest;
			}
		}

		IEnumerable<IContainer> ICusEntryHeader.Containers
		{
			get { return new TypedEnumerable<IContainer>(Declaration.CusContainers); }
		}

		IAddressDetails ICusEntryHeader.UltimateConsignee
		{
			get { return Declaration.ConsigneeOrgAddress; }
		}

		public ZString PreparerDistrictPort
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_PreparerDistrictPort : ZString.Empty;
			}
		}

		ZString ICusEntryHeader.PreparerFilerCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && IsRemoteLocationFiling ? declaration.US_EntryFilerCode : ZString.Empty;
			}
		}

		/// To be used with remote entry filing only.
		ZString ICusEntryHeader.PreparerOfficeCode
		{
			get { return Declaration.US_PreparerOfficeCode; }
		}

		ZBool ICusEntryHeader.IsSplitShipment
		{
			get { return Declaration.IsSplitShipment; }
		}

		ZString ICusEntryHeader.SplitShipmentReleaseCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration.IsACECargoCertificationMode ? declaration.US_SESplitRel : ZString.Empty;
			}
		}

		public ZString DistrictPortOfEntry
		{
			get { return US_SchDEntry; }
		}

		public ZString ImporterOfRecordNumber
		{
			get { return Declaration.ImporterOfRecordNumber; }
		}

		public ZString ImporterOfRecordNumberForDocument => Declaration.ImporterOfRecordNumberForDocument;

		ZString ICusEntryHeader.UltimateConsigneeNumber
		{
			get { return Declaration.UltimateConsigneeCustomsClientNumber; }
		}

		public ZString CBPF4811ReferenceNumber
		{
			get { return Declaration.CBPF4811ReferenceNumber; }
		}

		ZBool ICusEntryHeader.LiveEntry
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes;
			}
		}

		ZString ICusEntryHeader.MissingDocumentCodes
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_MissingDocument1 + declaration.US_MissingDocument2 : string.Empty;
			}
		}

		ZString IHeaderCommon.BondType
		{
			get
			{
				ZString result = Declaration != null ? Declaration.US_BondType : ZString.Empty;
				return result.IsEmpty ? (ZString)BondTypeList.Codes.NoBondRequired : result;
			}
		}

		ZString ICusEntryHeader.SuretyCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_SuretyCode : ZString.Empty;
			}
		}

		ZString ICusEntryHeader.StateOfDestination
		{
			get { return US_DestinationState; }
		}

		ZBool ICusEntryHeader.OGALineReleaseIndicator
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.US_OGALineReleaseIndicator == YesNoDefaultList.Codes.Yes;
			}
		}

		ZString ICusEntryHeader.ImportingVesselName
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.VesselName : ZString.Empty;
			}
		}

		public ZString ImportFTZNumber
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;

				if (declaration != null && declaration.IsConsumptionFTZ)
				{
					result = declaration.US_FTZNo;
				}
				return result;
			}
		}

		ZString IHeaderCommon.ModeOfTransportationCode
		{
			get { return ModeOfTransport; }
		}

		ZString ICusEntryHeader.DistrictPortOfUnlading
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_SchDArrival : ZString.Empty;
			}
		}

		ZDate ICusEntryHeader.DateOfImportation
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.JE_DateOfArrival.Date : ZDate.Empty;
			}
		}

		//If ZString.Empty is sent, users will get a rejection if they send the second original after the first original is successfully lodged.
		//See comments in CanSendOriginal
		public ZString BrokerReferenceNumber
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.BrokerReferenceNumberCore : ZString.Empty;
			}
		}

		ZString ICusEntryHeader.ClientBranchDesignation
		{
			get { return US_ClientBranchDesignation; }
		}

		ZString ICusEntryHeader.VoyageNumber
		{
			get { return Declaration.VoyageFlightNumberForMessaging; }
		}

		ZDate ICusEntryHeader.EstimatedDateOfArrival
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_EntryDate.Date : ZDate.Empty;
			}
		}

		ZString ICusEntryHeader.LocationOfGoods
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;

				if (declaration != null)
				{
					result = declaration.WarehouseAddressFirmsCode;

					if (result.IsEmpty)
					{
						result = declaration.US_US_NKLocationOfGoods;
					}
				}

				return result;
			}
		}

		public ZBool NAFTAReconciliation
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.US_NAFTAReconIndicator;
			}
		}

		public ZString OtherReconciliationIndicator
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? ReconIssueCodeList.ConvertToENSOtherIssueCode(declaration.US_OtherReconIndicator) : string.Empty;
			}
		}

		ZDecimal ICusEntryHeader.BondAmount
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_BondAmount : ZDecimal.Zero;
			}
		}

		ZString ICusEntryHeader.BondProducerAccountNumber
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsSingleTransactionBond ? declaration.US_BondProducerAccNo : ZString.Empty;
			}
		}

		ZString ICusEntryHeader.EntryFilerCodeOfWarehouseEntry
		{
			get { return ShouldSendWarehouseDetails ? Declaration.US_WHSEntryFilerCode : ZString.Empty; }
		}

		ZString ICusEntryHeader.WarehouseEntryNumber
		{
			get { return ShouldSendWarehouseDetails ? Declaration.US_WHSEntryNumber : ZString.Empty; }
		}

		bool ShouldSendWarehouseDetails
		{
			get
			{
				var declaration = Declaration;
				return declaration != null &&
					(!IsACE && declaration.IsExWarehouse || IsACE && EntryTypeList.IsExWarehouseOrReWarehouseType(declaration.US_EntryType));
			}
		}

		ZString ICusEntryHeader.DistrictPortCodeOfWarehouseEntry
		{
			get { return ShouldSendWarehouseDetails ? Declaration.US_WHSDistrictPortCode : ZString.Empty; }
		}

		ZBool ICusEntryHeader.FinalWarehouseIndicator
		{
			//TODO: Figure out how/where to implement
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsExWarehouse && declaration.US_IsFinalWHS;
			}
		}

		ZString ICusEntryHeader.ConsolidatedInformalIndicator
		{
			//TODO: Figure out how/where to implement
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_ConsolidatedInformalIndicator : ZString.Empty;
			}
		}

		ZBool ICusEntryHeader.IsACECargoReleaseCertification
		{
			get { return Declaration.IsACECargoCertificationMode; }
		}

		ZString ICusEntryHeader.DesignatedExamPort
		{
			get
			{
				var declaration = this.Declaration;
				return (declaration != null) ? declaration.US_SchDExam : ZString.Empty;
			}
		}

		ZString ICusEntryHeader.CarrierCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.CarrierCodeForEntrySummary : ZString.Empty;
			}
		}

		ZDecimal ICusEntryHeader.InformalFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		ZDecimal ICusEntryHeader.DutiableMailFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		ZDecimal ICusEntryHeader.ManualSurcharge
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		ZDecimal ICusEntryHeader.BondedADDDuty
		{
			get
			{
				CalculateADDOrCVDDutyDetailsIfNecessary();
				return fBondedADDDuty;
			}
		}
		ZDecimal fBondedADDDuty;

		ZBool ICusEntryHeader.BondedADDIndicator
		{
			get
			{
				CalculateADDOrCVDDutyDetailsIfNecessary();
				return fBondedADDIndicator;
			}
		}
		ZBool fBondedADDIndicator;

		ZDecimal ICusEntryHeader.PayableADDDuty
		{
			get
			{
				CalculateADDOrCVDDutyDetailsIfNecessary();
				return fPayableADDDuty;
			}
		}
		ZDecimal fPayableADDDuty;

		ZBool ICusEntryHeader.BondedCVDIndicator
		{
			get
			{
				CalculateADDOrCVDDutyDetailsIfNecessary();
				return fBondedCVDIndicator;
			}
		}
		ZBool fBondedCVDIndicator;

		ZDecimal ICusEntryHeader.BondedCVDDuty
		{
			get
			{
				CalculateADDOrCVDDutyDetailsIfNecessary();
				return fBondedCVDDuty;
			}
		}
		ZDecimal fBondedCVDDuty;

		ZDecimal ICusEntryHeader.PayableCVDDuty
		{
			get
			{
				CalculateADDOrCVDDutyDetailsIfNecessary();
				return fPayableCVDDuty;
			}
		}
		bool aDDOrCVDDutyDetailsCalculated;
		ZDecimal fPayableCVDDuty;

		void CalculateADDOrCVDDutyDetailsIfNecessary()
		{
			if (!aDDOrCVDDutyDetailsCalculated)
			{
				aDDOrCVDDutyDetailsCalculated = true;

				fBondedCVDIndicator = false;
				fBondedADDIndicator = false;
				fBondedADDDuty = 0m;
				fBondedCVDDuty = 0m;
				fPayableADDDuty = 0m;
				fPayableCVDDuty = 0m;

				//A parent CusEntryLine.CountervailingDuty returns a secondary tariff line's one
				//therefore it should loop through only parent or normal lines
				foreach (ICusEntryLine entryLine in EntryLines)
				{
					if (entryLine.BondedCountervailingDuty)//is there a bonded CVD line?
					{
						fBondedCVDIndicator = true;
						fBondedCVDDuty += entryLine.CountervailingDuty;
					}
					else
					{
						fPayableCVDDuty += entryLine.CountervailingDuty;
					}

					if (entryLine.BondedAntidumpingDuty)
					{
						fBondedADDIndicator = true;
						fBondedADDDuty += entryLine.AntidumpingDuty;
					}
					else
					{
						fPayableADDDuty += entryLine.AntidumpingDuty;
					}

					if (entryLine is CusEntryLine xvvLine && xvvLine.IsVChildLine)
					{
						foreach (ICusEntryLine child in xvvLine.ChildVLines)
						{
							if (child.BondedCountervailingDuty)
							{
								fBondedCVDIndicator = true;
								fBondedCVDDuty += child.CountervailingDuty;
							}
							else
							{
								fPayableCVDDuty += child.CountervailingDuty;
							}

							if (child.BondedAntidumpingDuty)
							{
								fBondedADDIndicator = true;
								fBondedADDDuty += child.AntidumpingDuty;
							}
							else
							{
								fPayableADDDuty += child.AntidumpingDuty;
							}
						}
					}
				}
			}
		}

		ZString ICusEntryHeader.ADDCVDSuretyCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_ADDCVDSuretyCode : ZString.Empty;
			}
		}

		IEnumerable<ICusEntryLine> ICusEntryHeader.EntryLines
		{
			get { return new TypedEnumerable<ICusEntryLine>(EntryLines); }
		}

		/// <summary>
		/// Returns only the top level merged lines (not secondary lines)
		/// </summary>
		public IEnumerable<CusEntryLine> EntryLines
		{
			get
			{
				foreach (CusEntryLine entryLine in MergedLines)
				{
					if (!entryLine.IsVChildLine)
					{
						if (!entryLine.IsSecondaryTariffLine)
						{
							yield return entryLine;
						}
					}
				}
			}
		}

		public ZInt TotalNonSecondaryInvoiceLinesCount
		{
			get
			{
				ZInt result = 0;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (!invoiceLine.IsSecondaryTariffLine)
					{
						result++;
					}
				}
				return result;
			}
		}

		IEnumerable<IFee> ICusEntryHeader.Fees
		{
			get
			{
				foreach (IFee fee in Charges)
				{
					if (!CusFeeCodeConstants.IsExciseTax(fee.Code))
					{
						yield return fee;
					}
				}
			}
		}

		bool ICusEntryHeader.BuildEmpty89EvenIfNoFeeExists
		{
			get
			{
				//no customs fees payable
				if (Charges.GetTotalCustomsFeeAmount() == 0m)
				{
					foreach (CusEntryLine entryLine in MergedLines)
					{
						//HMF less than $3 is exempt if no other fee exists. However 89 should report the fee
						if (entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF) > 0m ||
							new List<ZString>(entryLine.GetRequiredFees()).Count > 0)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		ZDecimal ICusEntryHeader.TotalEstimatedDuty
		{
			get { return TotalDutyAmount; }
		}

		public ZDecimal TotalEstimatedTax
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (IFee fee in Charges)
				{
					if (CusFeeCodeConstants.IsExciseTax(fee.Code))
					{
						result += fee.Amount;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalIRTTaxes
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (IFee fee in Charges)
				{
					if (CusFeeCodeConstants.IsExciseTax(fee.Code))
					{
						result += fee.Amount;
					}
				}
				return result;
			}
		}

		ZString ICusEntryHeader.DeferredTaxIndicator
		{
			get
			{
				var declaration = this.Declaration;
				return declaration == null || declaration.IsBulkLiquorTaxDeferred ? ZString.Empty : declaration.US_TaxDeferIndicator;
			}
		}

		public bool IsTaxDeferred
		{
			get
			{
				ZString taxDeferIndicator = ((ICusEntryHeader)this).DeferredTaxIndicator;
				return taxDeferIndicator == TaxDeferIndicatorList.Codes.DeferredTax ||
					taxDeferIndicator == TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			}
		}

		public ZDecimal TotalCountervailingDuty
		{
			get
			{
				ICusEntryHeader entry = this;
				return entry.BondedCVDDuty + entry.PayableCVDDuty;
			}
		}

		public ZDecimal TotalAntidumpingDuty
		{
			get
			{
				ICusEntryHeader entry = this;
				return entry.BondedADDDuty + entry.PayableADDDuty;
			}
		}

		public ZDecimal GrandTotalFee
		{
			get
			{
				ZDecimal result = 0m;
				foreach (IFee fee in ((ICusEntryHeader)this).Fees.Where(x => !Enterprise.Registry.Business.Customs.US.EntryChargeTypeList.IsOtherRevenueAmountChargeCode(x.Code)))
				{
					result += fee.Amount;
				}
				return result;
			}
		}

		ZDecimal ICusEntryHeader.GrandTotalOtherRevenueAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (IFee fee in ((ICusEntryHeader)this).Fees.Where(x => Enterprise.Registry.Business.Customs.US.EntryChargeTypeList.IsOtherRevenueAmountChargeCode(x.Code)))
				{
					result += fee.Amount;
				}
				return result;
			}
		}

		ZDecimal IHeaderCommon.TotalValueOfEntrySummary
		{
			get
			{
				var result = TransactionValue;
				if (result == ZDecimal.Zero)
				{
					var declaration = Declaration;
					if (declaration != null && declaration.IsEstimatedEnteredValueRequired)
					{
						result = declaration.US_EstEnteredValue;
					}
				}
				return result;
			}
		}

		protected override ZDecimal TransactionValueCore
		{
			get { return TotalEnteredValue; }
		}

		public ZDecimal DistilledSpiritsTax
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits); }
		}

		public ZDecimal OtherExciseTax
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise); }
		}

		public ZDecimal TobaccoTax
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco); }
		}

		public ZDecimal WinesTax
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines); }
		}

		public ZDecimal InformalFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal DutiableMailFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal BeefFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal BlueberryFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal CottonFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal AvocadoFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal HoneyFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal LimesFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal MangoFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal MushroomFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal RaspberryFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal PorkFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal PotatoFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal SoftwoodLumberFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal SugarFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal SorghumFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal WatermelonFee
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public GlbBranch EntryBranch
		{
			get { return Declaration?.Branch; }
		}

		ZString ICusEntryHeader.PaymentTypeIndicator
		{
			get { return US_PaymentType; }
		}

		ZDate ICusEntryHeader.PreliminaryStatementPrintDate
		{
			get { return US_PreliminaryStatementPrintDate.Date; }
		}

		ZString ICusEntryHeader.PeriodicStatementMonth
		{
			get { return US_PeriodicStatementMM; }
		}

		ZBool ICusEntryHeader.IsNonAMS
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.US_NonAMS;
			}
		}

		ZBool ICusEntryHeader.IsPerishable
		{
			get
			{
				return InvoiceHeaders.Any(x => x.HasInvoiceLinesWithPerishableCommodity);
			}
		}

		ZBool ICusEntryHeader.IsSelfCertification
		{
			get
			{
				return InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.US_DisclaimSanctions || x.HasFishingInformations || x.HasMiningInformations);
			}
		}

		#endregion

		#region ICargoManifestStatusQueryData Members

		CargoManifestQueryActionType ICargoManifestStatusQueryData.QueryActionType
		{
			get { return CargoManifestQueryActionType.Entry; }
		}

		string ICargoManifestStatusQueryData.TableCode
		{
			get { return CusEntryHeaderSchema.Constants.Prefix; }
		}

		ZString ICargoManifestStatusQueryData.HumanFriendlyReference
		{
			get { return ((IMessageAttacheeInDeclaration)this).HumanFriendlyReference; }
		}

		ZString ICargoManifestStatusQueryData.JobReferenceNumber
		{
			get { return ((IMessageAttacheeInDeclaration)this).JobReferenceNumber; }
		}

		ZString ICargoManifestStatusQueryData.EntryOrInBondNumber
		{
			get { return EntryNumber; }
		}

		ZString ICargoManifestStatusQueryData.MasterAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.HouseAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillIssuerCode
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		bool ICargoManifestStatusQueryData.HasPGAData => Declaration?.PGAFlags.HasInvoiceLinesWithPGA ?? false;

		void ICargoManifestStatusQueryData.LinkToMessage(EDIMessage message)
		{
			Messages.Add(message);
		}

		ZGuid ICargoManifestStatusQueryData.MessageAttacheePK
		{
			get { return PK; }
		}

		ZBool ICargoManifestStatusQueryData.IsRelevantFor(ZString actionCode)
		{
			return actionCode.IsEmpty || actionCode == CargoManifestStatusQueryActionList.Codes.Entry;
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return Declaration; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		#region IMessageAttachee

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return RegistryCompanyPK; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return Declaration?.TransportMode ?? ZString.Empty; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Declaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Declaration.JE_DeclarationReference + " / " + GetFormmattedFilerCodeAndEntryNumber(EntryFilerCode, EntryNumber); }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Declaration?.Logs; }
		}

		public ZString ProcessingDistrictPort
		{
			get { return IsBorderCargoRelease ? DistrictPortOfEntry : Declaration.ProcessingDistrictPort; }
		}

		public ZString ProcessingOfficeCode
		{
			get { return Declaration.ProcessingOfficeCode; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return CH_EntryStatus; }
		}

		public AENS10 LastENSAcceptedMessageBlock
		{
			get
			{
				AENS10 ens10block = null;
				EDIMessage outGoingMessage = PGADispositionProviderExtensionMethods.GetLastENSClearedMessage(Declaration);
				if (outGoingMessage != null)
				{
					foreach (var block in outGoingMessage.MessageBlock.MessageBlocks)
					{
						ens10block = block as AENS10;
						if (ens10block != null)
						{
							break;
						}
					}
				}
				return ens10block;
			}
		}

		public ASESE10 LastCRAcceptedMessageBlock
		{
			get
			{
				ASESE10 se10block = null;
				var outGoingMessage = PGADispositionProviderExtensionMethods.GetLastCRClearedMessage(Declaration);
				if (outGoingMessage != null)
				{
					foreach (var block in outGoingMessage.MessageBlock.MessageBlocks)
					{
						se10block = block as ASESE10;
						if (se10block != null)
						{
							break;
						}
					}
				}

				return se10block;
			}
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return CH_JE; }
		}

		ValidationModes IMessageAttacheeInDeclaration.ValidationModes
		{
			get { return Declaration?.ValidationModes ?? ValidationModes.None; }
			set
			{
				var declaration = this.Declaration;
				if (declaration != null)
				{
					declaration.ValidationModes = value;
				}
			}
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return EntryNumber; }
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return Declaration?.JE_EntryAuthorisationDate ?? ZDateTime.Empty; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return CH_Status; }
			set { CH_Status = value; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get
			{
				MessageAttacheeRecordType result;

				if (IsInBond)
				{
					result = MessageAttacheeRecordType.InBondEntry;
				}
				else if (IsCargoRelease)
				{
					result = MessageAttacheeRecordType.CargoRelease;
				}
				else if (IsBorderCargoRelease)
				{
					result = MessageAttacheeRecordType.BorderCargoRelease;
				}
				else if (IsACECargoRelease)
				{
					result = MessageAttacheeRecordType.SimplifiedEntry;
				}
				else
				{
					result = MessageAttacheeRecordType.Entry;
				}

				return result;
			}
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get
			{
				var result = MessageAttacheeRecordTypeDescriptions.Entry;

				switch (((IMessageAttacheeInDeclaration)this).RecordType)
				{
					case MessageAttacheeRecordType.CargoRelease:
					case MessageAttacheeRecordType.BorderCargoRelease:
						result = MessageAttacheeRecordTypeDescriptions.CargoRelease;
						break;
					case MessageAttacheeRecordType.InBondEntry:
						result = MessageAttacheeRecordTypeDescriptions.InBond;
						break;
					case MessageAttacheeRecordType.SimplifiedEntry:
						result = MessageAttacheeRecordTypeDescriptions.SimplifiedEntry;
						break;
				}

				return result;
			}
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			return Notifications;
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.JE_DeclarationReference : ZString.Empty;
			}
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return US_TIBExpiryDate; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return US_TIBNumOfExtensions; }
		}

		#endregion

		#region ICargoReleaseCusEntryHeader Members

		ZString ICargoReleaseCusEntryHeader.EntryDateElectionCode
		{
			get
			{
				var declaration = this.Declaration;
				return declaration == null || declaration.IsNonWeeklyEstimateFilingDate ? ZString.Empty : declaration.US_EntryDateElectionCode;
			}
		}

		ZDate ICargoReleaseCusEntryHeader.PresentationDate
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_PresentationDate.Date : ZDate.Empty;
			}
		}

		ZBool ICargoReleaseCusEntryHeader.IsConsigneeNameAddressUsed
		{
			get { return US_UseConsigneeNameAddress; }
		}

		public IAddressDetails EffectiveUltimateConsigneeWrapperAddressDetails
		{
			get { return ImportUltimateConsignee.GetCustomsAddressDetailsFallingBackToMainAddress(); }
		}

		public IAddressDetails ImporterWrapperAddressDetails
		{
			get
			{
				var organization = ImporterOfRecord ?? Importer;
				return organization.GetCustomsAddressDetailsFallingBackToMainAddress();
			}
		}

		ZString ICargoReleaseCusEntryHeader.UltimateConsigneeNumber
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				if (declaration != null && !declaration.HasLineLevelUltimateConsignees)
				{
					result = OrgHeaderWrapper.GetCustomsRelatedCode(declaration.ConsigneeOrgAddress, OrgMatchedCustomsRegNoType.ECN);//Encrypted number is accepted for Cargo Release.
				}

				return result;
			}
		}

		IEnumerable<ICargoReleaseCusEntryLine> ICargoReleaseCusEntryHeader.EntryLines
		{
			get { return new TypedEnumerable<ICargoReleaseCusEntryLine>(EntryLines); }
		}

		#endregion

		#region EntrySummary Document Fields

		public ZString FormattedEntryNumber
		{
			get { return GetFormmattedFilerCodeAndEntryNumber(EntryFilerCode, EntryNumber); }
		}

		public ZString EntryType
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_EntryType : ZString.Empty;
			}
		}

		public ZDate EstimatedEntryDate
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_EstimatedEntryDate.Date : ZDate.Empty;
			}
		}

		public ZString EntryFilerCode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && !declaration.IsExport ? declaration.US_EntryFilerCode : ZString.Empty;
			}
		}

		public ZDecimal MPFAmountForEntry
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		public ZDecimal HMFAmountForEntry
		{
			get { return Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal TotalEnteredValue
		{
			get
			{
				var result = ZDecimal.Zero;

				foreach (var entryLine in EntryLines)
				{
					if (!entryLine.IsSetXLine)
					{
						result += GetCustomsValueFromSingleEntryLine(entryLine);
						if (entryLine.IsVParentLine)
						{
							result += entryLine.ChildVLines.Sum(x => GetCustomsValueFromSingleEntryLine(x));
						}
					}
				}

				return result;
			}
		}

		public static ZDecimal GetCustomsValueFromSingleEntryLine(IEntryLine entryLine)
		{
			var result = ZDecimal.Zero;
			result += entryLine.RoundedCustomsValue;
			result += entryLine.ChildSecondaryEntryLines.Sum(x => x.RoundedCustomsValue);
			return result;
		}

		#endregion

		#region Entry Immediate Delivery Doc Fields

		public FlattenEntryLineAndBillForImmediateDeliveryCollection BillAndTariffLines
		{
			get { return billAndTariffLines ?? (billAndTariffLines = new FlattenEntryLineAndBillForImmediateDeliveryCollection(this)); }
		}
		FlattenEntryLineAndBillForImmediateDeliveryCollection billAndTariffLines;

		internal void ResetBillAndTariffLinesCache()
		{
			billAndTariffLines = null;
		}

		public ZString AdditionalBills
		{
			get { return BillAndTariffLines.ITBLAWBToPrint > 8 ? "Additional Bills" : ""; }
		}

		public ZString AdditionalTariffs
		{
			get { return BillAndTariffLines.UniqueTariffLines > 8 ? "ADD'L" : ""; }
		}

		#region 3461 First Page Line Details

		#region IT No / BL / AWB Code

		ZString GetItBlAwbCodeForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].ItBlAwbCode : ZString.Empty;
		}

		public ZString ItBlAwbCode1
		{
			get { return GetItBlAwbCodeForLine(0); }
		}

		public ZString ItBlAwbCode2
		{
			get { return GetItBlAwbCodeForLine(1); }
		}

		public ZString ItBlAwbCode3
		{
			get { return GetItBlAwbCodeForLine(2); }
		}

		public ZString ItBlAwbCode4
		{
			get { return GetItBlAwbCodeForLine(3); }
		}

		public ZString ItBlAwbCode5
		{
			get { return GetItBlAwbCodeForLine(4); }
		}

		public ZString ItBlAwbCode6
		{
			get { return GetItBlAwbCodeForLine(5); }
		}

		public ZString ItBlAwbCode7
		{
			get { return GetItBlAwbCodeForLine(6); }
		}

		public ZString ItBlAwbCode8
		{
			get { return GetItBlAwbCodeForLine(7); }
		}

		#endregion

		#region Port Of Lading

		ZString GetPortOfLadingForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].PortOfLading : ZString.Empty;
		}

		public ZString PortOfLading1_3461
		{
			get { return GetPortOfLadingForLine(0); }
		}

		public ZString PortOfLading2_3461
		{
			get { return GetPortOfLadingForLine(1); }
		}

		public ZString PortOfLading3_3461
		{
			get { return GetPortOfLadingForLine(2); }
		}

		public ZString PortOfLading4_3461
		{
			get { return GetPortOfLadingForLine(3); }
		}

		public ZString PortOfLading5_3461
		{
			get { return GetPortOfLadingForLine(4); }
		}

		public ZString PortOfLading6_3461
		{
			get { return GetPortOfLadingForLine(5); }
		}

		public ZString PortOfLading7_3461
		{
			get { return GetPortOfLadingForLine(6); }
		}

		public ZString PortOfLading8_3461
		{
			get { return GetPortOfLadingForLine(7); }
		}

		#endregion

		#region IT / BL / AWB Number

		ZString GetItBlAwbNumberForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].ItBlAwbNumber : ZString.Empty;
		}

		public ZString ItBlAwbNumber1
		{
			get { return GetItBlAwbNumberForLine(0); }
		}

		public ZString ItBlAwbNumber2
		{
			get { return GetItBlAwbNumberForLine(1); }
		}

		public ZString ItBlAwbNumber3
		{
			get { return GetItBlAwbNumberForLine(2); }
		}

		public ZString ItBlAwbNumber4
		{
			get { return GetItBlAwbNumberForLine(3); }
		}

		public ZString ItBlAwbNumber5
		{
			get { return GetItBlAwbNumberForLine(4); }
		}

		public ZString ItBlAwbNumber6
		{
			get { return GetItBlAwbNumberForLine(5); }
		}

		public ZString ItBlAwbNumber7
		{
			get { return GetItBlAwbNumberForLine(6); }
		}

		public ZString ItBlAwbNumber8
		{
			get { return GetItBlAwbNumberForLine(7); }
		}

		#endregion

		#region Manifest Quantity

		ZString GetManifestQuantityAndUQForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].ManifestQuantityAndUQ : ZString.Empty;
		}

		public ZString ManifestQuantityAndUQ1_3461
		{
			get { return GetManifestQuantityAndUQForLine(0); }
		}

		public ZString ManifestQuantityAndUQ2_3461
		{
			get { return GetManifestQuantityAndUQForLine(1); }
		}

		public ZString ManifestQuantityAndUQ3_3461
		{
			get { return GetManifestQuantityAndUQForLine(2); }
		}

		public ZString ManifestQuantityAndUQ4_3461
		{
			get { return GetManifestQuantityAndUQForLine(3); }
		}

		public ZString ManifestQuantityAndUQ5_3461
		{
			get { return GetManifestQuantityAndUQForLine(4); }
		}

		public ZString ManifestQuantityAndUQ6_3461
		{
			get { return GetManifestQuantityAndUQForLine(5); }
		}

		public ZString ManifestQuantityAndUQ7_3461
		{
			get { return GetManifestQuantityAndUQForLine(6); }
		}

		public ZString ManifestQuantityAndUQ8_3461
		{
			get { return GetManifestQuantityAndUQForLine(7); }
		}

		#endregion

		#region Tariff

		ZString GetTariffForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].Tariff : ZString.Empty;
		}

		public ZString Tariff1_3461
		{
			get { return GetTariffForLine(0); }
		}

		public ZString Tariff2_3461
		{
			get { return GetTariffForLine(1); }
		}

		public ZString Tariff3_3461
		{
			get { return GetTariffForLine(2); }
		}

		public ZString Tariff4_3461
		{
			get { return GetTariffForLine(3); }
		}

		public ZString Tariff5_3461
		{
			get { return GetTariffForLine(4); }
		}

		public ZString Tariff6_3461
		{
			get { return GetTariffForLine(5); }
		}

		public ZString Tariff7_3461
		{
			get { return GetTariffForLine(6); }
		}

		public ZString Tariff8_3461
		{
			get { return GetTariffForLine(7); }
		}

		#endregion

		#region CountryOfOrigin

		ZString GetCountryOfOriginForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].CountryOfOrigin : ZString.Empty;
		}

		public ZString CountryOfOrigin1_3461
		{
			get { return GetCountryOfOriginForLine(0); }
		}

		public ZString CountryOfOrigin2_3461
		{
			get { return GetCountryOfOriginForLine(1); }
		}

		public ZString CountryOfOrigin3_3461
		{
			get { return GetCountryOfOriginForLine(2); }
		}

		public ZString CountryOfOrigin4_3461
		{
			get { return GetCountryOfOriginForLine(3); }
		}

		public ZString CountryOfOrigin5_3461
		{
			get { return GetCountryOfOriginForLine(4); }
		}

		public ZString CountryOfOrigin6_3461
		{
			get { return GetCountryOfOriginForLine(5); }
		}

		public ZString CountryOfOrigin7_3461
		{
			get { return GetCountryOfOriginForLine(6); }
		}

		public ZString CountryOfOrigin8_3461
		{
			get { return GetCountryOfOriginForLine(7); }
		}

		#endregion

		#region ManufacturerID

		ZString GetManufacturerIDForLine(int lineCount)
		{
			return BillAndTariffLines.Count > lineCount ? BillAndTariffLines[lineCount].ManufacturerID : ZString.Empty;
		}

		public ZString ManufacturerID1_3461
		{
			get { return GetManufacturerIDForLine(0); }
		}

		public ZString ManufacturerID2_3461
		{
			get { return GetManufacturerIDForLine(1); }
		}

		public ZString ManufacturerID3_3461
		{
			get { return GetManufacturerIDForLine(2); }
		}

		public ZString ManufacturerID4_3461
		{
			get { return GetManufacturerIDForLine(3); }
		}

		public ZString ManufacturerID5_3461
		{
			get { return GetManufacturerIDForLine(4); }
		}

		public ZString ManufacturerID6_3461
		{
			get { return GetManufacturerIDForLine(5); }
		}

		public ZString ManufacturerID7_3461
		{
			get { return GetManufacturerIDForLine(6); }
		}

		public ZString ManufacturerID8_3461
		{
			get { return GetManufacturerIDForLine(7); }
		}

		#endregion

		#endregion

		public ZString BranchName
		{
			get { return BranchIAddresDetails.CompanyName; }
		}

		public ZString BranchAddress
		{
			get
			{
				return (BranchIAddresDetails.AddressLine1 + ", " + BranchIAddresDetails.AddressLine2 + " " + CityAndStateAndPostCode).Trim();
			}
		}

		public ZString BranchStreetAddress
		{
			get
			{
				return BranchIAddresDetails.AddressLine2.IsEmpty ? BranchIAddresDetails.AddressLine1.ToString() : BranchIAddresDetails.AddressLine1 + ", " + BranchIAddresDetails.AddressLine2;
			}
		}

		public ZString CityAndStateAndPostCode
		{
			get
			{
				return BranchIAddresDetails.State.IsEmpty ? BranchIAddresDetails.City + " " + BranchIAddresDetails.PostCode : BranchIAddresDetails.City + ", " + BranchIAddresDetails.State + " " + BranchIAddresDetails.PostCode;
			}
		}

		public ZString BranchPhone
		{
			get
			{
				var branchAddresDetails = BranchIAddresDetails;
				var result = "PHONE: " + branchAddresDetails.Phone;

				if (!branchAddresDetails.Fax.IsEmpty)
				{
					result += "  FAX: " + branchAddresDetails.Fax;
				}

				return result.Trim();
			}
		}

		public ZString Box27PhoneFax
		{
			get { return BranchPhone.SubstringSafe(7); }
		}

		IAddressDetails BranchIAddresDetails
		{
			get { return Declaration.BranchIAddressDetails; }
		}

		public ZString StatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;

				var dispositionDate = declaration.GetLatestDispositionDate();
				declaration.DispositionCodes.Sort(new DispositionDataByCodeComparer());

				foreach (DispositionData disposition in declaration.DispositionCodes)
				{
					if (disposition.US_DispositionDate == dispositionDate)
					{
						if (disposition.US_Code != CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate)
						{
							result += StatusDescriptionList.GetDescriptionFromCode(disposition.US_Code) + "\n";
						}
					}
				}

				return result.TrimEnd();
			}
		}

		//this should be the FDA Status Description
		public ZString FDAStatusDescription
		{
			get { return Declaration.FDAStatusDescription; }
		}

		CodeDescriptionPairList StatusDescriptionList
		{
			get { return new CargoReleaseProcessingResultList(); }
		}

		public ZString ElectedEntryDate
		{
			get
			{
				var electedEntryDateTime = ElectedEntryDateTime;
				return electedEntryDateTime.IsValid ? electedEntryDateTime.ToString("MM/dd/yyyy") : "";
			}
		}

		ZDateTime ElectedEntryDateTime
		{
			get
			{
				var result = ZDateTime.Empty;
				var declaration = Declaration;

				if (declaration.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.PresentationDate ||
					declaration.IsWeeklyEstimateFilingDate)
				{
					result = declaration.US_PresentationDate;
				}
				else if (declaration.US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.ArrivalDate)
				{
					result = declaration.US_EntryDate;
				}
				else if (!declaration.JE_EntryAuthorisationDate.IsEmpty)
				{
					result = declaration.JE_EntryAuthorisationDate;
				}
				else if (!declaration.US_EstimatedEntryDate.IsEmpty)
				{
					result = declaration.US_EstimatedEntryDate;
				}

				return result;
			}
		}

		public ZString BrokerImporterFileNumber
		{
			get
			{
				var declaration = Declaration;
				var result = declaration.JE_DeclarationReference;

				if (!declaration.JE_OwnerRef.IsEmpty)
				{
					result = result + "/" + declaration.JE_OwnerRef;
				}

				return result;
			}
		}

		public ZString DeclarantName
		{
			get
			{
				var signatory = this.Signatory;
				return signatory != null ? signatory.GS_FullName : ZString.Empty;
			}
		}

		public ZString BoxNumber
		{
			get
			{
				var result = ZString.Empty;
				var fallbackForAll = ZString.Empty;

				var declaration = Declaration;
				if (declaration != null)
				{
					var boxNoCollection = USCustomsDataRegistry.Instance.BoxNumbers.GetValueWithoutFallback(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty);
					foreach (BoxNumber boxNo in boxNoCollection)
					{
						if (boxNo.TransportMode == BoxNoTransportModeList.Codes.ALL)
						{
							fallbackForAll = boxNo.BoxNo;
						}

						if (boxNo.TransportMode == declaration.TransportMode)
						{
							result = boxNo.BoxNo;
							break;
						}
					}

					if (result.IsEmpty)
					{
						result = fallbackForAll;
					}
				}

				return result;
			}
		}

		public ZString SingleTransBond
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond ? "X " + declaration.US_SuretyCode : "" : "";
			}
		}

		public ZString VesselCodeOrFTZNumber
		{
			get
			{
				var declaration = this.Declaration;
				return declaration.IsConsumptionFTZ ? declaration.JE_MasterBill : declaration.JE_VesselName;
			}
		}

		public ZString EffectiveUltimateConsigneeCustomsRegNo
		{
			get
			{
				var declaration = Declaration;

				return ImportUltimateConsignee != null && ImporterOfRecord != null &&
					(declaration.IsConsumptionFTZ || ImportUltimateConsignee.OH_Code != ImporterOfRecord.OH_Code)
					? declaration.UltimateConsigneeCustomsClientNumber.ToString()
					: USConstants.Same;
			}
		}

		public ZString EffectiveUltimateConsigneeAddressLine2
		{
			get
			{
				var result = ZString.Empty;

				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;
				if (ultConsigneeAddressDetails != null)
				{
					result = ultConsigneeAddressDetails.AddressLine2.IsEmpty ? EUCCityStatePostCodeCountry : ultConsigneeAddressDetails.AddressLine2;
				}

				return result;
			}
		}

		public ZString EffectiveUltimateConsigneeCityStatePostCodeCountry
		{
			get
			{
				var result = ZString.Empty;

				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;
				if (ultConsigneeAddressDetails != null && !ultConsigneeAddressDetails.AddressLine2.IsEmpty)
				{
					result = EUCCityStatePostCodeCountry;
				}

				return result;
			}
		}

		ZString EUCCityStatePostCodeCountry
		{
			get
			{
				var builder = new ZStringBuilder();
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;
				if (ultConsigneeAddressDetails != null)
				{
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.City);
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.State);
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.PostCode);
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.Country);
				}
				return builder.ToStringWithDelimiterBetweenAppends("   ");
			}
		}

		public ZString ImporterAddressLine2
		{
			get
			{
				var result = ZString.Empty;

				var importerAddressDetails = ImporterWrapperAddressDetails;
				if (importerAddressDetails != null)
				{
					result = importerAddressDetails.AddressLine2.IsEmpty ? ImporterCityStatePostCodeCountryCalculated : importerAddressDetails.AddressLine2;
				}

				return result;
			}
		}

		public ZString ImporterCityStatePostCodeCountry
		{
			get
			{
				var result = ZString.Empty;

				var importerAddressDetails = ImporterWrapperAddressDetails;
				if (importerAddressDetails != null && !importerAddressDetails.AddressLine2.IsEmpty)
				{
					result = ImporterCityStatePostCodeCountryCalculated;
				}

				return result;
			}
		}

		ZString ImporterCityStatePostCodeCountryCalculated
		{
			get
			{
				var result = ZString.Empty;

				var importerAddressDetails = ImporterWrapperAddressDetails;
				if (importerAddressDetails != null)
				{
					result = importerAddressDetails.City + "   " +
							 importerAddressDetails.State + "   " +
							 importerAddressDetails.PostCode + "   " +
							 importerAddressDetails.Country;
					result = result.TrimStart();
				}

				return result;
			}
		}

		public ZZRefCusCodeListCombined LocationOfGoods
		{
			get
			{
				return this.Declaration?.LocationOfGoods;
			}
		}

		public ZString FIRMSAddress
		{
			get
			{
				return this.Declaration?.FIRMSAddress ?? ZString.Empty;
			}
		}

		public ZString FIRMSCity
		{
			get
			{
				return this.Declaration?.FIRMSCity ?? ZString.Empty;
			}
		}

		public ZString FIRMSState
		{
			get
			{
				return this.Declaration?.FIRMSState ?? ZString.Empty;
			}
		}

		public ZString FIRMSZIPCode
		{
			get
			{
				return this.Declaration?.FIRMSZIPCode ?? ZString.Empty;
			}
		}

		public ZBool IsAttorneyInFact
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && USCustomsDataRegistry.Instance.IsAttorneyInFact.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			}
		}

		bool HasCurrentElectronicRelease
		{
			get
			{
				JobDeclaration declaration = Declaration;
				return declaration != null && declaration.HasCurrentElectronicRelease;
			}
		}

		public ZString ElectronicEntryReleaseNotificationLine1
		{
			get { return HasCurrentElectronicRelease ? "ELECTRONIC ENTRY RELEASE NOTIFICATION. PORT OF " + US_SchDEntry + ". I certify" : ""; }
		}

		public ZString ElectronicEntryReleaseNotificationLine2
		{
			get { return HasCurrentElectronicRelease ? "that proper release for this cargo has been received from U.S. Customs." : ""; }
		}

		public ZString ElectronicEntryReleaseNotificationLine3
		{
			get
			{
				var result = ZString.Empty;

				if (HasCurrentElectronicRelease)
				{
					var declaration = Declaration;
					result = "Release Date: " + declaration.JE_EntryAuthorisationDate.ToString("MM/dd/yy HH:mm") + "   " + BranchName;
				}

				return result;
			}
		}

		public ZString ElectronicEntryReleaseNotificationLine4
		{
			get { return HasCurrentElectronicRelease ? "Signature: ____________________________________________________" : ""; }
		}

		public ZString EntryNumberFormattedForImmediateDeliveryDoc
		{
			get { return EntryNumber.SubstringSafe(0, 1) == "0" ? EntryNumber.SubstringSafe(1) : EntryNumber; }
		}

		public ZString ABICertifiedText
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.HasCargoReleaseBeenCertified ? "ABI Certified" : "";
			}
		}

		public ZDateTime ArrivalDate
		{
			get { return Declaration.US_EntryDate; }
		}

		public ZDateTime CertificationDate
		{
			get { return ZDateTime.Today; }
		}

		public ZString Box29ExamSite
		{
			get
			{
				var result = ZString.Empty;
				var centralizedExamSite = Declaration.US_US_NKCentralizedExamSite;
				if (!centralizedExamSite.IsEmpty)
				{
					result = "Examination Site: " + centralizedExamSite;

					var firms = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, centralizedExamSite, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);

					if (firms != null)
					{
						result = result + " " + firms.ZZD_Description;
					}
				}

				return result;
			}
		}

		public ZString PPQ505
		{
			get { return MergedLines.HasLaceyActData ? "PPQ 505-ABI" : ""; }
		}

		public ZString Box29OtherData
		{
			get
			{
				var result = new StringBuilder();
				int box29CharsUsed = 0;

				if (!PPQ505.IsEmpty)
				{
					result.Append(PPQ505);
					result.Append("\r\n");
					box29CharsUsed = box29CharsUsed + 50;
				}

				var declaration = Declaration;
				if (declaration.IORWrapper != null && !declaration.IORWrapper.CTPAT.IsEmpty)
				{
					result.Append("CTPAT CERTIFIED\r\n");
					box29CharsUsed = box29CharsUsed + 50;
				}

				var box29Text = declaration.US_Box29Text;
				if (!box29Text.IsEmpty)
				{
					result.Append(box29Text + "\r\n\r\n");
					var upperBox29 = box29Text.ToUpper();
					int largeFontedLettersAdjustment = upperBox29.Occurrences("G") + upperBox29.Occurrences("M") + upperBox29.Occurrences("Q") + upperBox29.Occurrences("W");
					box29CharsUsed = box29CharsUsed + box29Text.Length + largeFontedLettersAdjustment + 50;
					box29CharsUsed = box29CharsUsed + (box29Text.Occurrences("\r\n") * 50); // adjustment for additional carriage returns
				}

				continuationPageContainers = new ArrayList();

				if (declaration.US_Box29IncludeContainers)
				{
					if (declaration.CusContainers != null)
					{
						int totalContainerSpaceRequired = declaration.CusContainers.Count * 13;
						bool printContainersOnContinuationPage = (box29CharsUsed + totalContainerSpaceRequired) > box29CharsAvailable;

						if (printContainersOnContinuationPage)
						{
							continuationPageContainers = new ArrayList();
						}
						else
						{
							result.Append("Containers: ");
						}

						if (printContainersOnContinuationPage)
						{
							foreach (CusContainer container in declaration.CusContainers)
							{
								continuationPageContainers.Add(container.CO_ContainerNumber);
							}
						}
						else
						{
							foreach (CusContainer container in declaration.CusContainers)
							{
								result.Append(container.CO_ContainerNumber);
								result.Append(", ");
							}

							result.Remove(result.Length - 2, 2);    //remove the last ', '
						}
					}
				}

				return result.ToString();
			}
		}

		const int box29CharsAvailable = 600;
		ArrayList continuationPageContainers = new ArrayList();

		public ContainerOverflowForImmediateDeliveryCollection ContainersContinued
		{
			get
			{
				var result = new ContainerOverflowForImmediateDeliveryCollection(Factory);
				string checkBox29Data = Box29OtherData;

				if (continuationPageContainers.Count > 0 && Declaration.US_Box29IncludeContainers)
				{
					result = new ContainerOverflowForImmediateDeliveryCollection(continuationPageContainers, Factory);
				}

				return result;
			}
		}

		#endregion

		#region PPQ Form 368 Notice Of Arrival Fields

		public ZString PPQForm368MarksBillOfLadingAndContainerNumber
		{
			get { return Declaration.US_PPQForm368Box13A; }
		}

		public ZString PPQForm368QuantityAndNetWeight
		{
			get { return Declaration.US_PPQForm368Box13B; }
		}

		public ZString PPQForm368Commodity
		{
			get { return Declaration.US_PPQForm368Box13C; }
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		Enterprise.Messaging.Business.EDIMessageCollection IStatusNeedsRecalculationProvider.Messages
		{
			get { return Messages; }
		}

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get { return (IsFormalEntry || IsCargoRelease || IsBorderCargoRelease) && Messages.HasChanges; }
		}

		#endregion

		#region IDutyDataLineHeader Members

		IEnumerable<IEntryLineOrInvoiceLineDutyData> IDutyDataLineHeader.DutyDataLines
		{
			get
			{
				foreach (CusEntryLine entryLine in MergedLines)
				{
					yield return new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine);
				}
			}
		}

		IFees IDutyDataLineHeader.FeeAndCharges
		{
			get { return Charges; }
		}

		void IDutyDataLineHeader.DeleteDetachedEntryLines()
		{
			foreach (CusEntryLine entryLine in MergedLines.ToArray())
			{
				if (entryLine.InvoiceLines.Count == 0)
				{
					entryLine.Delete();
				}
			}
		}

		bool IDutyDataLineHeader.IsDutiableMailFeeApplicable
		{
			get
			{
				return ((ICusEntryHeader)this).ModeOfTransportationCode == TransportModeCodes.Codes.Mail &&
					TotalDutyAmount > 0;
			}
		}

		public bool IsACE
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsACE;
			}
		}
		/// <summary>
		/// This is to determine whether to make Entry HMF exempt or not
		/// </summary>
		public bool IsHMFDeMinimisApplicable
		{
			get { return TotalDutyAmount == 0 && TotalEstimatedTax == 0 && TotalAntidumpingDuty == 0m && TotalCountervailingDuty == 0m; }
		}

		bool IDutyDataLineHeader.IsInformalFeeApplicable
		{
			get
			{
				ICusEntryHeader iEntryHeader = this;
				return InformalFeeApplicableCalculator.IsApplicable(iEntryHeader.EntryType, iEntryHeader.ModeOfTransportationCode, iEntryHeader.DistrictPortOfEntry);
			}
		}

		bool IDutyDataLineHeader.IsHMFApplicable
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.US_IsHMFApplicable == YesNoDefaultList.Codes.Yes;
			}
		}

		public ZDateTime DateForMPFCalculation
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.DateForMPFCalculation : ZDateTime.Today;
			}
		}

		ZDateTime IDutyDataLineHeader.DateForFeeCalculation
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.DateForFeeCalculation : ZDateTime.Today;
			}
		}

		void IDutyDataLineHeader.UpdateAfterHMFDeMinimusRuleApplied()
		{
			if (IsACE)
			{
				foreach (CusEntryLine entryLine in MergedLines)
				{
					entryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, 0m);
				}
			}
		}

		bool IDutyDataLineHeader.DoesMPFSurchargeApply
		{
			get
			{
				var filer = USCustomsDataRegistry.Instance.EntryFiler.GetValueWithoutFallback(RegistryCompanyPK, Guid.Empty, Guid.Empty);
				return filer != null && !filer.IsABICertified;
			}
		}

		bool IDutyDataLineHeader.AreDutyFeeKnownAndImported
		{
			get { return false; }
		}

		bool IDutyDataLineHeader.IsCottonFeeDeMinimusApplicable
		{
			get
			{
				var declaration = this.Declaration;
				return declaration == null || declaration.IsCottonFeeDeMinimusApplicable();
			}
		}

		BusinessObjectFactory IDutyDataLineHeader.Factory
		{
			get { return this.Factory; }
		}

		ZDecimal? IDutyDataLineHeader.OverridenTotalMPFPayable => ((IDutyDataLineHeaderProvider)Declaration).OverridenTotalMPFPayable;

		bool IDutyDataLineHeader.CalculateChangedLinesOnly => false;

		ZDecimal IDutyDataLineHeader.OriginalTotalCV => 0m;

		void IDutyDataLineHeader.OnCalculating()
		{
		}

		#endregion

		#region SupportsCloneCore
		protected override bool SupportsCloneCore()
		{
			return true;
		}
		#endregion

		public ZString InBondCarrier
		{
			get
			{
				var result = string.Empty;
				var declaration = this.Declaration;
				var orgDelivery = declaration.DeliveryOrPickupCartageCo;
				if (orgDelivery != null && declaration.IsFTZAdmission && !declaration.JE_PrimaryITNumber.IsEmpty && !declaration.US_ITDate.IsEmpty)
				{
					result = orgDelivery.OH_FullName;
				}
				return result;
			}
		}

		public ZString PrimaryITNumber
		{
			get
			{
				var declaration = this.Declaration;
				return (declaration.JE_PrimaryITNumber == JobDeclaration.Constants.Multiple) ? (ZString)USConstants.MultipleValueIndicator : declaration.JE_PrimaryITNumber;
			}
		}

		public ZString UniqueAgricultureLicenseNumber
		{
			get
			{
				if (!uniqueAgricultureLicenseNumber.HasValue)
				{
					uniqueAgricultureLicenseNumber = UniqueValueCalculator.GetUniqueValue(MergedLines.Cast<ICusEntryLine>(), x => x.AgricultureLicenseNumber, (ZString)USConstants.SeeAttachedIndicator);
				}
				return uniqueAgricultureLicenseNumber.Value;
			}
		}
		ZString? uniqueAgricultureLicenseNumber;

		public ZString UniqueCountryOfOrigin
		{
			get
			{
				if (!uniqueCountryOfOrigin.HasValue)
				{
					uniqueCountryOfOrigin = UniqueValueCalculator.GetUniqueValue(MergedLines.Cast<ICusEntryLine>(), x => x.CountryOfOrigin, (ZString)USConstants.MultipleValueIndicator);
				}
				return uniqueCountryOfOrigin.Value;
			}
		}
		ZString? uniqueCountryOfOrigin;

		public ZString UniqueCountryOfExport
		{
			get
			{
				if (!uniqueCountryOfExport.HasValue)
				{
					uniqueCountryOfExport = UniqueValueCalculator.GetUniqueValue(MergedLines.Cast<ICusEntryLine>(), x => x.CountryOfExport, (ZString)USConstants.MultipleValueIndicator);
				}

				return uniqueCountryOfExport.Value;
			}
		}
		ZString? uniqueCountryOfExport;

		public ZString UniquePortOfLading
		{
			get
			{
				if (!uniquePortOfLading.HasValue)
				{
					uniquePortOfLading = ZString.Empty;

					if (Declaration.JE_TransportMode == TransportTypeList.Codes.Sea)
					{
						uniquePortOfLading = UniqueValueCalculator.GetUniqueValue(MergedLines.Cast<ICusEntryLine>(), x => x.PortOfLading, (ZString)USConstants.MultipleValueIndicator);
					}
				}
				return uniquePortOfLading.Value;
			}
		}
		ZString? uniquePortOfLading;

		public ZBool HasMultipleManufacturerIDs
		{
			get
			{
				if (!hasMultipleManufacturerIDsCached.HasValue)
				{
					hasMultipleManufacturerIDsCached = UniqueValueCalculator.HasMultiValues(MergedLines.Cast<ICusEntryLine>(), x => x.ManufacturerSupplierCode);
				}
				return hasMultipleManufacturerIDsCached.Value;
			}
		}
		ZBool? hasMultipleManufacturerIDsCached;

		public bool HasMultiExportDates
		{
			get
			{
				if (!hasMultiExportDates.HasValue)
				{
					hasMultiExportDates = UniqueValueCalculator.HasMultiValues(MergedLines.Cast<CusEntryLine>(), x => x.ExportDate);
				}
				return hasMultiExportDates.Value;
			}
		}
		bool? hasMultiExportDates;

		#region IReconOriginalChargeParent Members

		CodeDescriptionPairList IReconOriginalChargeParent.FeeAndChargeList
		{
			get => CusFeeCodeConstants.GetReconEntryHeaderFeeChargeCodeList(Factory);
		}

		bool IReconOriginalChargeParent.DefaultValueForOverridenForNewChild
		{
			get
			{
				var entry = this.ReconOriginalEntry;
				return entry != null && (!entry.ShouldDutiesFeesBeCalculated || entry.US_R_ChangedLinesOnly);
			}
		}

		USCTariff IReconOriginalChargeParent.Tariff
		{
			get { return null; }
		}

		void IReconOriginalChargeParent.SynchroniseOnNonCommittedAdded(ReconEntryOriginalCharge charge)
		{
			if (charge != null && !charge.CY_Code.IsEmpty)
			{
				ZString chargeType = charge.CY_Code;
				var existingCharge = Charges[chargeType];
				if (existingCharge == null)
				{
					Charges.AddNew(chargeType);
				}
			}
		}

		bool IReconOriginalChargeParent.ShouldCalculateOrigDuty
		{
			get
			{
				var entry = this.ReconOriginalEntry;
				return entry != null ? entry.US_R_CalcOrigDuty : ZBool.False;
			}
		}

		bool IReconOriginalChargeParent.MonthlyFiling
		{
			get
			{
				var entry = this.ReconOriginalEntry;
				return entry != null ? entry.US_R_MonthlyFiling : ZBool.False;
			}
		}

		void IReconOriginalChargeParent.UpdateChargeDetails()
		{
			if (reconOriginalEntry != null)
			{
				reconOriginalEntry.RefreshEntryWithTotalOriginalCustomsFees();
			}
		}

		BusinessObject IReconOriginalChargeParent.ParentAsBusinessObject
		{
			get { return this; }
		}

		#endregion

		#region IUSCustomsChargeEntry Members

		string Accounting.Integration.IAccInvoiceDataProvider.EntryWithdrawnStatusTerm
		{
			get { return "deleted"; }
		}

		ZString ICustomsChargeEntry.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		ZString IUSCustomsChargeEntry.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IUSCustomsChargeEntry.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IUSCustomsChargeEntry.PaymentType
		{
			get { return US_PaymentType; }
		}

		ZDate IUSCustomsChargeEntry.DueDate
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_PaymentDueDate.Date : ZDate.Empty;
			}
		}

		bool IUSCustomsChargeEntry.IsPaidByImporter
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsPaidByImporter;
			}
		}

		bool ICustomsChargeEntry.EntryReferenceInChargeDescSupported
		{
			get { return false; }
		}

		#endregion

		#region IMessageFailStatusManager Members

		bool IMessageFailStatusManager.IsMessageTypeSupported(ZString messageType)
		{
			return true;
		}

		void IMessageFailStatusManager.SetFailStatus(MQEDIMessage message)
		{
			switch (message.EM_MessageType)
			{
				case ACEApplicationIdentifierCodeList.Codes.InbondTransaction:
					new InBondMessageStatusCalculator(this).CalculateStatus(message, ABIResponseStatus.Rejected);
					break;
				case ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAdd:
					CH_Status = ImportMessageStatusList.Codes.ErrorConsigneeNameAddressAdd;
					break;
				case ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions:
					new CargoReleaseMessageStatusCalculator(this).CalculateStatus(message, ABIResponseStatus.Rejected);
					break;
				case ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse:
					new EntrySummaryMessageStatusCalculator(this).CalculateStatus(message, ABIResponseStatus.Rejected);
					break;
			}
		}

		#endregion

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			IMessageResponseNotificator notificator = Declaration;
			return notificator != null ? notificator.GetFallbackEmailAddressRecipient() : ZString.Empty;
		}

		#endregion

		#region IAESTIRMessageAttachee Members
		// SC1 Record
		ZString IAESTIRMessageAttachee.RelatedCompanyIndicator
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.RelatedCompanyIndicator;
				}
				else
				{
					return IsTransactionsRelated ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
				}
			}
		}

		ZString IAESTIRMessageAttachee.ModeOfTransportationCodeMOT
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.ModeOfTransportationCodeMOT;
				}
				else
				{
					return ModeOfTransport;
				}
			}
		}

		ZString IAESTIRMessageAttachee.CountryOfUltimateDestinationCode
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.CountryOfUltimateDestinationCode;
				}
				else
				{
					return CountryOfUltimateDestination;
				}
			}
		}

		ZString IAESTIRMessageAttachee.USStateOfOriginCode
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.USStateOfOriginCode;
				}
				else
				{
					return StateOfOrigin;
				}
			}
		}

		ZString IAESTIRMessageAttachee.CarrierIDSCACIATA
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.CarrierIDSCACIATA;
				}
				else
				{
					return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(CarrierCode, ABICharacterTypeString.Constants.Special, 4);
				}
			}
		}

		ZString IAESTIRMessageAttachee.ShipmentReferenceNumber
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.ShipmentReferenceNumber;
				}
				else
				{
					return CH_BGMReference;
				}
			}
		}

		ZString IAESTIRMessageAttachee.ConveyanceNameCarrierName
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.ConveyanceNameCarrierName;
				}
				else
				{
					return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(ExportingCarrier, ABICharacterTypeString.Constants.Special, 23);
				}
			}
		}

		ZString IAESTIRMessageAttachee.FilingOptionIndicator
		{
			get
			{
				var sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.FilingOptionIndicator;
				}
				else
				{
					var declaration = Declaration;
					return declaration != null ? declaration.US_CommodityFilingOption.SubstringSafe(0, 1) : ZString.Empty;
				}
			}
		}

		ZString IAESTIRMessageAttachee.AEIFilingType
		{
			get
			{
				var sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.AEIFilingType;
				}
				else
				{
					var declaration = Declaration;
					return declaration != null ? declaration.US_CommodityFilingOption.SubstringSafe(1, 1) : ZString.Empty;
				}
			}
		}

		ZString IAESTIRMessageAttachee.PortOfUnladingCode
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.PortOfUnladingCode;
				}
				else
				{
					return PortOfArrivalInUSFormat;
				}
			}
		}

		ZString IAESTIRMessageAttachee.PortOfExportationCode
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.PortOfExportationCode;
				}
				else
				{
					return PortOfExportInUSFormat;
				}
			}
		}

		ZDate IAESTIRMessageAttachee.EstimatedDateOfExport
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.EstimatedDateOfExport;
				}
				else
				{
					return ExportDate.Date;
				}
			}
		}

		ZString IAESTIRMessageAttachee.HazardousMaterialIndicatorHAZMAT
		{
			get
			{
				AESCommShipSC1XP sc1Record = ShouldDataComesFromLastClearMessage ? SC1RecordFromLatestClearMessage : null;
				if (sc1Record != null)
				{
					return sc1Record.HazardousMaterialIndicatorHAZMAT;
				}
				else
				{
					return IsHazardousCargo ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
				}
			}
		}

		AESCommShipSC1XP SC1RecordFromLatestClearMessage
		{
			get
			{
				if (sC1RecordFromLatestClearMessageCached == null)
				{
					sC1RecordFromLatestClearMessageCached = new CachedProperty<AESCommShipSC1XP>(Factory,
						delegate
						{
							AESCommShipSC1XP result = null;
							var latestClearMessage = LatestClearMessage;
							if (latestClearMessage != null)
							{
								result = latestClearMessage.MessageBlock.MessageBlocks.OfType<AESCommShipSC1XP>().FirstOrDefault();
							}
							return result;
						});
				}
				return sC1RecordFromLatestClearMessageCached.Value;
			}
		}
		CachedProperty<AESCommShipSC1XP> sC1RecordFromLatestClearMessageCached;

		// SC2 Record
		ZString IAESTIRMessageAttachee.InbondCode
		{
			get { return InbondType; }
		}

		ZString IAESTIRMessageAttachee.EntryNumber
		{
			get { return ImportEntryNumber; }
		}

		ZString IAESTIRMessageAttachee.ForeignTradeZoneIdentifier
		{
			get { return ForeignTradeZone; }
		}

		ZString IAESTIRMessageAttachee.RoutedExportTransactionIndicator
		{
			get { return IsRoutedTransaction ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No; }
		}

		ZString IAESTIRMessageAttachee.OriginalITN
		{
			get { return Declaration.US_OriginalITNNumber; }
		}

		// SC3 Records
		IEnumerable<IAESTIRTransportationDetail> IAESTIRMessageAttachee.TransportationDetails
		{
			get
			{
				if (aESTransportationDetailsCached == null)
				{
					aESTransportationDetailsCached = new CachedProperty<IEnumerable<IAESTIRTransportationDetail>>(Factory, delegate
					{
						List<IAESTIRTransportationDetail> result = new List<IAESTIRTransportationDetail>();
						if (!TransportationReferenceNumber.IsEmpty)
						{
							result.Add(this);
						}
						Dictionary<ZGuid, IAESTIRTransportationDetail> containers = new Dictionary<ZGuid, IAESTIRTransportationDetail>();

						foreach (CusContainer container in Containers)
						{
							if (container != null && !containers.ContainsKey(container.PK))
							{
								containers.Add(container.PK, container);
							}
						}
						result.AddRange(containers.Values);
						return result.ToArray();
					});
				}
				return aESTransportationDetailsCached.Value;
			}
		}
		CachedProperty<IEnumerable<IAESTIRTransportationDetail>> aESTransportationDetailsCached;

		// Parties
		IAESTIRParty IAESTIRMessageAttachee.USPPI
		{
			get
			{
				if (ShouldDataComesFromLastClearMessage)
				{
					return USPPIFromLatestClearMessage;
				}
				else
				{
					return AESTIRParty.New(USPPI, new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.USACodeTypes.ForeignRegistrationNumber,
						OrgCusCode.CodeTypes.PassportID,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem }, RandomHeader.SupplierPickupAddress);
				}
			}
		}

		bool ShouldDataComesFromLastClearMessage
		{
			get
			{
				if (shouldDataComesFromLastClearMessageCached == null)
				{
					shouldDataComesFromLastClearMessageCached = new CachedProperty<bool>(Factory, () => US_IsDeactivated && RandomHeader == null && HasBeenLodgedAtCustoms && !HasBeenWithdrawn);
				}
				return shouldDataComesFromLastClearMessageCached.Value;
			}
		}
		CachedProperty<bool> shouldDataComesFromLastClearMessageCached;

		IAESTIRParty USPPIFromLatestClearMessage
		{
			get
			{
				if (uSPPIFromLatestClearMessageCached == null)
				{
					uSPPIFromLatestClearMessageCached = new CachedProperty<IAESTIRParty>(Factory,
						delegate
						{
							IAESTIRParty result = null;
							var latestClearMessage = LatestClearMessage;
							if (latestClearMessage != null)
							{
								var usppiRecord = latestClearMessage.MessageBlock.B as IAESControlMessageBlockB;
								if (usppiRecord != null)
								{
									result = new USPPIFromBBlock(usppiRecord);
								}
							}
							return result;
						});
				}
				return uSPPIFromLatestClearMessageCached.Value;
			}
		}
		CachedProperty<IAESTIRParty> uSPPIFromLatestClearMessageCached;

		class USPPIFromBBlock : IAESTIRParty
		{
			public USPPIFromBBlock(IAESControlMessageBlockB bBlock)
			{
				this.bBlock = bBlock;
			}
			readonly IAESControlMessageBlockB bBlock;

			#region IAESTIRParty Members

			ZString IAESTIRParty.PartyID => bBlock.USPPIID;

			ZString IAESTIRParty.PartyIDType => bBlock.USPPIIDType;

			ZString IAESTIRParty.PartyName => bBlock.USPPIName;

			ZString IAESTIRParty.ContactFirstName => ZString.Empty;

			ZString IAESTIRParty.ContactMiddleInitial => ZString.Empty;

			ZString IAESTIRParty.ContactLastName => ZString.Empty;

			ZString IAESTIRParty.AddressLine1 => ZString.Empty;

			ZString IAESTIRParty.AddressLine2 => ZString.Empty;

			ZString IAESTIRParty.ContactPhoneNumber => ZString.Empty;

			ZString IAESTIRParty.City => ZString.Empty;

			ZString IAESTIRParty.StateCode => ZString.Empty;

			ZString IAESTIRParty.CountryCode => ZString.Empty;

			ZString IAESTIRParty.PostalCode => ZString.Empty;

			#endregion
		}

		MQEDIMessage LatestClearMessage
		{
			get
			{
				if (latestClearMessageCached == null)
				{
					latestClearMessageCached = new CachedProperty<MQEDIMessage>(Factory,
						delegate
						{
							MQEDIMessage result = null;
							if (Messages.Count > 0)
							{
								result = Messages.OfType<MQEDIMessage>().Where(x => !x.IsTransmitMessage
									&& x.MessageBlock.MessageBlocks.FirstOrDefault(y => (y is AESCommShipES1XT && ((AESCommShipES1XT)y).FinalDispositionIndicator == USConstants.AESRejectionDisposition) || (y is AESCommWarnES1XN && ((AESCommWarnES1XN)y).FinalDispositionIndicator == USConstants.AESRejectionDisposition)) == null)
									.OrderByDescending(x => x.EM_SystemCreateTimeUtc)
									.FirstOrDefault();
							}
							return result;
						});
				}
				return latestClearMessageCached.Value;
			}
		}
		CachedProperty<MQEDIMessage> latestClearMessageCached;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public EDIMessage[] ApprovedTIBExtensionMessages
		{
			get
			{
				if (approvedTIBExtensionMessagesCached == null)
				{
					approvedTIBExtensionMessagesCached = new CachedProperty<EDIMessage[]>(Factory,
						delegate
						{
							return Messages.OfType<EDIMessage>().Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse
							&& x.EM_Status == MQEDIMessage.Status.Received && x.EM_ReceiveTransmit == EDIMessage.Direction.Receive && x.HasApprovedTIBExtension()).ToArray();
						});
				}
				return approvedTIBExtensionMessagesCached.Value;
			}
		}
		CachedProperty<EDIMessage[]> approvedTIBExtensionMessagesCached;

		ZString IAESTIRMessageAttachee.USPPIIRSNumber
		{
			get
			{
				ZString result = ZString.Empty;
				IAESTIRParty party = ((IAESTIRMessageAttachee)this).USPPI;
				if (party != null && party.PartyIDType == AESConstants.IDTypes.DUNS)
				{
					var docAddress = USPPI.USOrganisationDocAddress;
					var organisation = USPPI.Organisation;
					if (docAddress != null && organisation != null && !docAddress.E2_AddressOverride)
					{
						result = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
					}
				}
				return result;
			}
		}

		ZString IAESTIRMessageAttachee.USPPIIRSIDType
		{
			get
			{
				ZString number = ((IAESTIRMessageAttachee)this).USPPIIRSNumber;
				return number.IsEmpty ? string.Empty : AESConstants.IDTypes.EmployerIdentificationNumber;
			}
		}

		IAESTIRParty IAESTIRMessageAttachee.ForwardingAgent
		{
			get
			{
				return AESTIRParty.New(Declaration.Forwarder, ContactType.ExportFreightAgent, Declaration.JE_TransportMode, new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem
					});
			}
		}

		IAESTIRParty IAESTIRMessageAttachee.UltimateConsignee
		{
			get
			{
				return AESTIRParty.New(ExportUltimateConsignee, new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem
					});
			}
		}

		ZString IAESTIRMessageAttachee.UltimateConsigneeType
		{
			get { return RandomHeader.US_UltimateConsigneeType; }
		}

		ZBool IAESTIRMessageAttachee.IsSoldEnRoute
		{
			get { return Declaration.IsSoldEnRoute; }
		}

		ZString IAESTIRMessageAttachee.CityOfFirstPortOfCall
		{
			get { return Declaration.US_FirstPortOfCallCity; }
		}

		ZString IAESTIRMessageAttachee.CountryOfFirstPortOfCall
		{
			get { return Declaration.US_RN_NKFirstPortOfCallCountry; }
		}

		IAESTIRParty IAESTIRMessageAttachee.IntermediateConsignee
		{
			get
			{
				return AESTIRParty.New(IntermediateConsignee, new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem
					});
			}
		}

		// Commodity Line Items
		IEnumerable<IAESTIRCommodityLineItem> IAESTIRMessageAttachee.CommodityLineItems
		{
			get { return new TypedEnumerable<IAESTIRCommodityLineItem>(MergedLines); }
		}

		#endregion

		#region IAESTIRTransportationDetail
		ZString IAESTIRTransportationDetail.EquipmentNumber
		{
			get { return ZString.Empty; }
		}

		ZString IAESTIRTransportationDetail.SealNumber
		{
			get { return ZString.Empty; }
		}

		ZString IAESTIRTransportationDetail.TransportationReferenceNumber
		{
			get { return TransportationReferenceNumber; }
		}
		#endregion

		#region ICBPEDIMessageMessageTextNumberPlaceHolderFiller Members

		string ICBPEDIMessageMessageTextNumberPlaceHolderFiller.Fill(CBPEDIMessage cbpMessage)
		{
			var information = string.Empty;
			var message = (MQEDIMessage)cbpMessage;

			if (IsInBond)
			{
				FillInInBondNumberIfInBond();
				CusEntryNumber.FillMessagePlaceHolder(cbpMessage, MQEDIMessage.InBondNumberPlaceHolder, EntryNumber.PadRight(12));
				information = string.Format(CultureInfo.InvariantCulture, "{0}CusEntryHeader is InBond, and InBondNumberPlaceHolder {1} is replaced with Entry Number {2}. ",
					information, MQEDIMessage.InBondNumberPlaceHolder, EntryNumber);
			}
			else if (IsReconEntry)
			{
				FillInFormalEntryNumber();

				CusEntryNumber.FillMessagePlaceHolder(cbpMessage, MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, EntryFilerCode + EntryNumber);
				CusEntryNumber.FillMessagePlaceHolder(cbpMessage, MQEDIMessage.USEntryNumberPlaceHolder, EntryNumber);
				information = string.Format(CultureInfo.InvariantCulture, "{0}CusEntryHeader is ReconEntry, and USEntryFilerEntryNumberPlaceHolder {1} is replaced with EntryFilerCode + EntryNumber {2}, and USEntryNumberPlaceHolder {3} is replaced with Entry Number {4}. ",
					information, MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, EntryFilerCode + EntryNumber, MQEDIMessage.USEntryNumberPlaceHolder, EntryNumber);
			}
			else
			{
				if (GenerateFormalEntryNumberOnSaving)
				{
					FillInFormalEntryNumber();
				}

				var entryNumber = EntryNumber;
				if (entryNumber.IsEmpty && message.IsBIRDTransaction)
				{
					entryNumber = " ".PadLeft(MQEDIMessage.USEntryNumberPlaceHolder.Length);
				}

				if (!EntryNumber.IsEmpty)
				{
					CusEntryNumber.FillMessagePlaceHolder(cbpMessage, MQEDIMessage.USEntryNumberPlaceHolder, entryNumber.PadRight(MQEDIMessage.USEntryNumberPlaceHolder.Length));
					information = string.Format(CultureInfo.InvariantCulture, "{0}Entry Number is not empty, and USEntryNumberPlaceHolder {1} is replaced with Entry Number {2}. ",
						information, MQEDIMessage.USEntryNumberPlaceHolder, entryNumber);
				}
				else
				{
					message.EM_MessageText = message.EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, entryNumber.PadRight(MQEDIMessage.USEntryNumberPlaceHolder.Length));
					information = string.Format(CultureInfo.InvariantCulture, "{0}Entry Number is empty, and USEntryNumberPlaceHolder {1} is replaced with Entry Number {2}. ",
						information, MQEDIMessage.USEntryNumberPlaceHolder, entryNumber);
				}
			}
			return information;
		}

		#endregion

		#region IACECusEntryHeader Members

		public ZBool KnownImporterIndicator
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.KnownImporterIndicator;
			}
		}

		public ZBool IsExpressConsignment
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsExpressConsignment;
			}
		}

		public ZBool IsDomesticCargo
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.US_DomesticCargo;
			}
		}

		ZString IACECusEntryHeader.DesignationCode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.US_BondDesignationCode : ZString.Empty;
			}
		}

		ZBool IACECusEntryHeader.IsPaid
		{
			get { return Declaration.US_Paid == YesNoDefaultList.Codes.Yes; }
		}

		ZString IACECusEntryHeader.NotifyPartyNumber
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(Declaration.NotifyParty, OrgMatchedCustomsRegNoType.EIN); }
		}

		ZString IACECusEntryHeader.LloydsCode
		{
			get
			{
				var declaration = this.Declaration;
				var vessel = declaration.IsSea ? declaration.Vessel : null;
				return vessel != null ? vessel.RV_LloydsNumber : ZString.Empty;
			}
		}

		ZDate IACECusEntryHeader.ITDate
		{
			get
			{
				var declaration = this.Declaration;
				return !declaration.US_NonAMS ? declaration.US_ITDate.Date : ZDate.Empty;
			}
		}

		ZBool IACECusEntryHeader.Consolidated
		{
			get { return Declaration.US_ConsolACE; }
		}

		ZBool IACECusEntryHeader.BondWaivedOrNoBond
		{
			get
			{
				var declaration = this.Declaration;
				return !declaration.US_BondWaiverCode.IsEmpty || declaration.IsWithoutBondType;
			}
		}

		ZString IACECusEntryHeader.BondWaiverReasonCode
		{
			get { return Declaration.US_BondWaiverCode; }
		}

		ZBool IACECusEntryHeader.ContinuousBondSuperseded
		{
			get { return Declaration.US_BondSuperseding; }
		}

		ZString IACECusEntryHeader.ADDCVDBondType
		{
			get { return Declaration.US_BondType2; }
		}

		ZDecimal IACECusEntryHeader.ADDCVDSingleTransactionBondAmount
		{
			get { return Declaration.US_BondAmount2; }
		}

		ZString IACECusEntryHeader.ADDCVDSingleTransactionBondAccNo
		{
			get { return Declaration.US_BondProducerAccNo2; }
		}

		IEnumerable<IACECusEntryLine> IACECusEntryHeader.EntryLines
		{
			get { return new TypedEnumerable<IACECusEntryLine>(EntryLines); }
		}

		IEnumerable<ZString> IACECusEntryHeader.ConsolidatedReleaseEntryNumbers
		{
			get
			{
				var releaseEntryNumbersList = new List<ZString>();
				foreach (var invoice in InvoiceHeaders)
				{
					var releaseEntryNumber = invoice.US_ReleaseEntryNumber;
					if (!releaseEntryNumber.IsEmpty && !releaseEntryNumbersList.Contains(releaseEntryNumber))
					{
						releaseEntryNumbersList.Add(releaseEntryNumber);
					}
				}

				return releaseEntryNumbersList;
			}
		}

		ZBool IACECusEntryHeader.IsPSC
		{
			get { return Declaration.US_PSC; }
		}

		ZBool IACECusEntryHeader.AcceleratedLiqReqIndicator
		{
			get { return Declaration.US_AccLiqReq; }
		}

		ZString IACECusEntryHeader.PGAExpeditedReleaseIndicator => Declaration?.PGAExpeditedInEntrySummaryIndicator ?? ZString.Empty;

		public IReadOnlyCollection<IPGADataCorrection> PGADataCorrections => Factory.GetValue(ref cachedPGADataCorrections,
			() =>
			{
				return MergedLines.Cast<CusEntryLine>().SelectMany(x => x.PGADataCorrections).ToArray();
			});
		CachedProperty<IReadOnlyCollection<IPGADataCorrection>> cachedPGADataCorrections;

		#endregion

		#region IACECargoReleaseHeader Members

		ZDate IACECargoReleaseHeader.ElectedEntryDate
		{
			get { return !Declaration.US_EntryDateElectionCode.IsEmpty && !Declaration.IsNonWeeklyEstimateFilingDate ? ElectedEntryDateTime.Date : ZDate.Empty; }
		}

		ZString IACECargoReleaseHeader.EntryDateElectionCode
		{
			get { return !Declaration.IsNonWeeklyEstimateFilingDate ? Declaration.US_EntryDateElectionCode : ZString.Empty; }
		}

		ZString IACECargoReleaseHeader.ElectedExamSite
		{
			get { return Declaration.US_US_NKCentralizedExamSite; }
		}

		ZString IACECargoReleaseHeader.GeneralOrderNumber
		{
			get { return Declaration.US_GeneralOrderNo; }
		}

		ZString IACECargoReleaseHeader.PortOfUnlading
		{
			get { return Declaration.US_SchDArrival; }
		}

		ZString IACECargoReleaseHeader.CBPBondedWarehouseFIRMS
		{
			get
			{
				var result = ZString.Empty;
				if (EntryTypeList.IsWarehouseType(EntryType))
				{
					result = Declaration.WarehouseAddressFirmsCode;
				}

				return result;
			}
		}

		ZString IACECargoReleaseHeader.DeclarationReferenceNumber
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		ZString IACECargoReleaseHeader.ImporterOfRecordType
		{
			get { return ACECargoReleaseData.GetCustomsNumberType(ImporterOfRecordNumber); }
		}

		ZString IACECargoReleaseHeader.ADDCVDBondType
		{
			get { return Declaration.US_BondType2; }
		}

		ZDecimal IACECargoReleaseHeader.ADDCVDSingleTransactionBondAmount
		{
			get { return Declaration.US_BondAmount2; }
		}

		ZString IACECargoReleaseHeader.ADDCVDSingleTransactionBondAccNo
		{
			get { return Declaration.US_BondProducerAccNo2; }
		}

		ZString IACECargoReleaseHeader.ConsolidatedFilterCodeAndEntryNumber
		{
			get
			{
				if (consolidatedFilterCodeAndEntryNumberCached == null)
				{
					consolidatedFilterCodeAndEntryNumberCached = new CachedValue<ZString>(() =>
					{
						var result = ZString.Empty;
						var declaration = Declaration;
						if (declaration != null && !declaration.US_ConsolidatedJobNumber.IsEmpty)
						{
							var consolidatedDeclaration = GetConsolidatedJobDeclaration(declaration.US_ConsolidatedJobNumber, declaration.CompanyPK);
							if (consolidatedDeclaration != null)
							{
								var filterCode = consolidatedDeclaration.US_EntryFilerCode;
								var entryNumber = consolidatedDeclaration.DecEntryNumber;
								if (!filterCode.IsEmpty && !entryNumber.IsEmpty)
								{
									result = filterCode + entryNumber;
								}
							}
						}
						return result;
					});
				}
				return consolidatedFilterCodeAndEntryNumberCached.Value;
			}
		}
		CachedValue<ZString> consolidatedFilterCodeAndEntryNumberCached;

		JobDeclaration GetConsolidatedJobDeclaration(ZString reference, ZGuid companyPK)
		{
			var filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, reference);
			filter.AddToFilter(JobDeclarationSchema.JE_GC, companyPK);

			return Factory.LoadTop1<JobDeclaration>(filter);
		}

		IEnumerable<ISimplifiedEntryOrganisationDetails> IACECargoReleaseHeader.Entities
		{
			get
			{
				var result = new List<ISimplifiedEntryOrganisationDetails>();
				var randomHeader = this.RandomHeader;
				if (randomHeader != null)
				{
					var firstEntity = randomHeader.JZ_OA_ManufacturerAddress;
					var isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_ManufacturerAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.ManufacturerAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_ManufacturerAddressInfo, EntityCodeList.Codes.ManufacturerSupplier);
					}

					firstEntity = randomHeader.JZ_OA_ConsigneeAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_ConsigneeAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.ConsigneeAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_ConsigneeAddressInfo, EntityCodeList.Codes.Consignee);
					}

					firstEntity = randomHeader.JZ_OA_SoldToPartyAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_SoldToPartyAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.SoldToPartyAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_SoldToPartyAddressInfo, EntityCodeList.Codes.BuyingParty);
					}

					firstEntity = randomHeader.JZ_OA_SellerAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_SellerAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.SellerAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_SellerAddressInfo, EntityCodeList.Codes.SellingParty);
					}

					firstEntity = randomHeader.JZ_OA_ShipToPartyAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_ShipToPartyAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.ShipToPartyAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_ShipToPartyAddressInfo, EntityCodeList.Codes.ShipToParty);
					}

					firstEntity = randomHeader.JZ_OA_ExporterAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_ExporterAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.ExporterAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_ExporterAddressInfo, EntityCodeList.Codes.Exporter);
					}

					firstEntity = randomHeader.JZ_OA_ShipperAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_ShipperAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.ShipperAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_ShipperAddressInfo, EntityCodeList.Codes.Shipper);
					}

					firstEntity = randomHeader.JZ_OA_DistributorAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_DistributorAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.DistributorAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_DistributorAddressInfo, EntityCodeList.Codes.Distributor);
					}

					firstEntity = randomHeader.JZ_OA_PackagerAddress;
					isTheSamePartyForAllInvoices = InvoiceHeaders.All(x => x.JZ_OA_PackagerAddress == firstEntity);
					if (isTheSamePartyForAllInvoices && randomHeader.PackagerAddress != null)
					{
						ACECargoReleaseData.AddEntity(result, randomHeader.JZ_OA_PackagerAddressInfo, EntityCodeList.Codes.Packager);
					}
				}
				return result;
			}
		}

		ZString IACECargoReleaseHeader.CurrentFirmsCodeForWarehousingEntry
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null ? declaration.US_US_NKLocationOfGoods : ZString.Empty;
			}
		}

		ZBool IACECargoReleaseHeader.ImmediateDelivery
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.US_ImmediateDelivery;
			}
		}

		ZString IACECargoReleaseHeader.RailReferenceNumber
		{
			get
			{
				return Declaration.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN).FirstOrDefault();
			}
		}

		ZDate IACECargoReleaseHeader.EstimatedDateOfArrivalForEntryType86
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsLowValue ? declaration.JE_DateOfArrival.Date : ZDate.Empty;
			}
		}

		#endregion

		#region IEntrySummaryQueryMessageAttachee Members

		Guid IQueryMessageAttachee.CompanyPK
		{
			get { return Declaration.RegistryCompanyPK; }
		}

		ZDateTime IQueryMessageAttachee.DateFrom
		{
			get { return ZDate.Empty; }
		}

		ZDateTime IQueryMessageAttachee.DateTo
		{
			get { return ZDate.Empty; }
		}

		IEnumerable<(ZString, ZString)> IQueryMessageAttachee.EntryFilerCodesAndNumbers
		{
			get
			{
				yield return (EntryFilerCode, EntryNumber);
			}
		}

		ZString IEntrySummaryQueryMessageAttachee.CriteriaCode
		{
			get { return ZString.Empty; }
		}

		ZBool IEntrySummaryQueryMessageAttachee.ConsumptionEntrySummaries
		{
			get { return ZBool.False; }
		}

		ZBool IEntrySummaryQueryMessageAttachee.FTAReconSummaries
		{
			get { return ZBool.False; }
		}

		ZBool IEntrySummaryQueryMessageAttachee.OtherReconSummaries
		{
			get { return ZBool.False; }
		}

		ZBool IEntrySummaryQueryMessageAttachee.DrawbackSummaries
		{
			get { return ZBool.False; }
		}

		ZBool IEntrySummaryQueryMessageAttachee.NAFTADutyDeferralSummaries
		{
			get { return ZBool.False; }
		}

		ZString IEntrySummaryQueryMessageAttachee.CollectionBillInformationCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PGA Recap Document Properties

		public FDARecapLineCollection FDARecapLines
		{
			get { return fdaLines ?? (fdaLines = new FDARecapLineCollection(this)); }
		}
		FDARecapLineCollection fdaLines;

		public ZString VoyageNumber
		{
			get { return ((ICusEntryHeader)this).VoyageNumber; }
		}

		public ZString VesselName
		{
			get { return ((ICusEntryHeader)this).ImportingVesselName; }
		}

		public ZDateTime ReleaseDate
		{
			get { return ((IMessageAttacheeInDeclaration)this).ReleaseDate; }
		}

		public ZString FDAStatus
		{
			get
			{
				var declaration = Declaration;
				return !declaration.FDAStatus.IsEmpty ? (declaration.FDAStatus + "-" + declaration.FDAStatusDescription) : string.Empty;
			}
		}

		public ZString BrokerName
		{
			get
			{
				var entryBranch = this.EntryBranch;
				return entryBranch != null ? entryBranch.Company.GC_Name : ZString.Empty;
			}
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.US7501DocPrinting, typeof(US7501DocPrinting));
			result.Add(CusCodeDataTypeList.Codes.PSCReasonCodes, typeof(PSCExplanationCusAddInfo));
			result.Add(CusCodeDataTypeList.Codes.AutoSendACEMessage, typeof(AutoSendMessageCusAddInfo));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.ReconRefundedCharge, typeof(ReconRefundedCharge));
			result.Add(CusCodeDataTypeList.Codes.PSCReasonCodes, typeof(PSCReasonCusCodeData));
			result.Add(CusCodeDataTypeList.Codes.ReconEntryOriginalCharge, typeof(ReconEntryOriginalCharge));
			return result;
		}

		#endregion

		#region FTZ214 Document Print Members

		public ZString FTZAdmissionNumber
		{
			get
			{
				var result = ZString.Empty;
				if (IsFTZAdmission)
				{
					var declaration = Declaration;
					if (declaration != null)
					{
						result = declaration.FTZAdmissionNumber;
					}
				}
				return result;
			}
		}

		public ZString CBP214ExpirationDate => new RefSysConfig.Loader(Factory).GetStringValue("CBP214ED");

		public ZString CBP214RevisionDateForFirstPage => new RefSysConfig.Loader(Factory).GetStringValue("CBP214FRD");

		public ZString CBP214RevisionDateForContinuationPage => new RefSysConfig.Loader(Factory).GetStringValue("CBP214CRD");

		public ZDateTime FTZAdmissionStatusDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				var disposition = AdmissionStatusData;

				if (disposition != null)
				{
					result = disposition.US_DispositionDate;
				}

				return result;
			}
		}

		public ZString FTZAdmissionStatusMessage
		{
			get
			{
				ZString result = ZString.Empty;
				var disposition = AdmissionStatusData;

				if (disposition != null)
				{
					result = ZString.Format("{0}  {1}", disposition.US_Code, disposition.DispositionCodeDesc);
				}

				return result;
			}
		}

		DispositionData AdmissionStatusData
		{
			get
			{
				if (admissionStatusData == null)
				{
					var dispositionCodes = Declaration.FTZDispositionCodes;
					dispositionCodes.Sort(DispositionData.Schema.US_DispositionDate, System.ComponentModel.ListSortDirection.Descending);
					admissionStatusData = dispositionCodes.OfType<DispositionData>().FirstOrDefault(x => x.US_Code == DispositionList.Codes.B4 || x.US_Code == DispositionList.Codes.B5);
				}
				return admissionStatusData;
			}
		}
		DispositionData admissionStatusData;

		JobDocAddress FTZWarehouseDocAddress
		{
			get
			{
				return Declaration?.WarehouseDocAddress;
			}
		}

		ZBool HasFTZWarehouseDocAddress
		{
			get
			{
				return FTZWarehouseDocAddress?.Organisation != null;
			}
		}

		public ZString FTZZone
		{
			get { return Declaration.FTZZoneID.SubstringSafe(0, 9); }
		}

		public ZString FTZCompanyName
		{
			get
			{
				var name = LocationOfGoods?.ZZD_Description ?? ZString.Empty;
				return HasFTZWarehouseDocAddress ? FTZWarehouseDocAddress.Organisation.OH_FullName : name;
			}
		}

		public ZString FTZAddress
		{
			get
			{
				return HasFTZWarehouseDocAddress ? new ZString(FTZWarehouseDocAddress.E2_Address1 + " " + FTZWarehouseDocAddress.E2_Address2) : FIRMSAddress;
			}
		}

		public ZString FTZCity
		{
			get
			{
				return HasFTZWarehouseDocAddress ? FTZWarehouseDocAddress.E2_City : FIRMSCity;
			}
		}

		public ZString FTZState
		{
			get
			{
				return HasFTZWarehouseDocAddress ? FTZWarehouseDocAddress.E2_State : FIRMSState;
			}
		}

		public ZString FTZZIPCode
		{
			get
			{
				return HasFTZWarehouseDocAddress ? FTZWarehouseDocAddress.E2_Postcode : FIRMSZIPCode;
			}
		}

		public ZString FTZPortCode
		{
			get { return US_SchDEntry; }
		}

		public ZString FTZImportingVesselOtherCarrier
		{
			get
			{
				var result = ZString.Empty;
				var declaration = this.Declaration;
				if (declaration != null)
				{
					if (FTZIsNotDomesticNorZone)
					{
						switch (declaration.JE_TransportMode)
						{
							case TransportTypeList.Codes.Sea:
								result = declaration.JE_VesselName;
								RefVessel vessel = declaration.Vessel;
								if (vessel != null)
								{
									ZString regCountry = vessel.RV_RN_NKCountryOfReg;
									if (!regCountry.IsEmpty)
									{
										result += ", " + regCountry;
									}
								}
								break;
							case TransportTypeList.Codes.Air:
								var importingCarrier = declaration.ImportingCarrier;
								if (importingCarrier != null)
								{
									result = importingCarrier.UI_Name;
								}
								break;
							case TransportTypeList.Codes.Mail:
								result = "MAIL";
								break;
							case TransportTypeList.Codes.Rail:
								result = "RAILROAD";
								break;
							case TransportTypeList.Codes.Truck:
								result = "TRUCK";
								break;
							case TransportTypeList.Codes.FixedTransportInstallations:
								result = "PIPELINE";
								break;
							default:
								result = "OTHER";
								break;
						}
					}
					else if (FTZIsZoneTransfer)
					{
						result = "ZONE TRANSFER";
					}
				}
				return result;
			}
		}

		public ZDateTime FTZExportDate
		{
			get { return ExportDate; }
		}

		public ZDateTime FTZImportDate
		{
			get { return Declaration.JE_DateOfArrival; }
		}

		public ZString FTZPortOfUnlading
		{
			get { return ArrivalPort; }
		}

		ZString ArrivalPort
		{
			get
			{
				var result = ZString.Empty;
				var port = Declaration.SchDArrivalPort;

				if (port != null)
				{
					result = port.PortCode + " " + port.PortName;
				}

				return result;
			}
		}

		public ZString FTZForeignPortOfLading
		{
			get
			{
				var result = ZString.Empty;
				var port = Declaration.SchDLoadingPort;
				if (FTZHasMultipleForeignPortsOfLading)
				{
					result = "MULTI (SEE ITEM 15)";
				}
				else if (port != null)
				{
					result = port.PortCode + " " + port.PortName;
				}

				return result;
			}
		}

		public bool FTZHasMultipleForeignPortsOfLading
		{
			get { return (InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.US_SchDLoading).Distinct().Count() > 1); }
		}

		public ZString FTZBillOfLading
		{
			get { return (Declaration.JE_TransportMode == TransportTypeList.Codes.Mail) ? ZString.Empty : ZString.Format("{0}{1}", Declaration.JE_MasterBillIssuerSCAC, Declaration.JE_MasterBill); }
		}

		public ZDateTime FTZITDate
		{
			get
			{
				return !(PrimaryITNumber.IsEmpty || PrimaryITNumber == (ZString)USConstants.MultipleValueIndicator) ?
						Declaration.US_ITDate : ZDateTime.Empty;
			}
		}

		public ZString FTZITFromPort
		{
			get { return !PrimaryITNumber.IsEmpty ? ArrivalPort : ZString.Empty; }
		}

		public ZDecimal FTZTotalHMF
		{
			get
			{
				ZDecimal total = 0.0m;

				foreach (CusEntryLine line in EntryLines)
				{
					total += line.HMFAmount;

					if (line.IsVParentLine)
					{
						total += line.ChildVLines.Sum(x => x.HMFAmount);
					}
				}

				return total;
			}
		}

		public ZString FTZBox18TotalQuantity
		{
			get
			{
				var groupByUnit = EntryLines.Where(x => (!x.CustomsUnitQty.IsEmpty & x.CustomsQuantity > 0))
					.GroupBy(x => x.CustomsUnitQty)
					.Select(x => (unit: x.Key, sumQty: x.Sum(q => q.CustomsQuantity)));

				var strBuilder = new StringBuilder();
				foreach (var oneProduct in groupByUnit)
				{
					strBuilder.AppendLine(((ZDecimal)oneProduct.sumQty).ToString(0) + " " + oneProduct.unit);
				}
				return strBuilder.ToString();
			}
		}

		public ZString FTZBox19TotalGrossWeight
		{
			get
			{
				var groupByUnit = EntryLines.Where(x => (!x.EffectiveGrossWeight.Unit.IsEmpty & x.EffectiveGrossWeight.Amount > 0))
					.GroupBy(x => x.EffectiveGrossWeight.Unit)
					.Select(x => (unit: x.Key, sumQty: x.Sum(q => q.EffectiveGrossWeight.Amount.Round(0))));

				var strBuilder = new StringBuilder();
				foreach (var oneProduct in groupByUnit)
				{
					strBuilder.AppendLine(((ZDecimal)oneProduct.sumQty).ToString(0) + " " + oneProduct.unit);
				}
				return strBuilder.ToString();
			}
		}

		public ZDecimal FTZBox20TotalAggrCharges => EntryLines.Sum(x => x.FTZCustomsValue);

		public ZString FTZWarehousePackageTotalQuantityAndUnit
		{
			get
			{
				var groupByUnit = EntryLines.Where(x => (!x.FTZWhsPkgConvertedInvoiceUnit.IsEmpty & x.FTZWhsPkgQty > 0))
					.GroupBy(x => x.FTZWhsPkgConvertedInvoiceUnit)
					.Select(x => (unit: x.Key, sumQty: x.Sum(q => q.FTZWhsPkgQty)));

				var strBuilder = new StringBuilder();
				foreach (var oneProduct in groupByUnit)
				{
					strBuilder.AppendLine(((ZDecimal)oneProduct.sumQty).ToString(0) + " " + oneProduct.unit);
				}
				return strBuilder.ToString();
			}
		}

		public void CalculateFTZZoneStatuses()
		{
			activeZoneStatuses = 0x00;
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				switch (line.US_ZoneStatus)
				{
					case ZoneStatusList.Codes.NonPrivilegedForeign:
						activeZoneStatuses |= Constants.FTZZoneStatuses.N;
						break;
					case ZoneStatusList.Codes.PrivilegedForeign:
						activeZoneStatuses |= Constants.FTZZoneStatuses.P;
						break;
					case ZoneStatusList.Codes.ZoneRestricted:
						activeZoneStatuses |= Constants.FTZZoneStatuses.Z;
						break;
					case ZoneStatusList.Codes.Domestic:
						activeZoneStatuses |= Constants.FTZZoneStatuses.D;
						break;
				}
			}
		}
		byte activeZoneStatuses;

		public bool FTZPrintZones
		{
			get
			{
				return (activeZoneStatuses != 0x00 &&
						((activeZoneStatuses ^ Constants.FTZZoneStatuses.N) != 0x00) && ((activeZoneStatuses ^ Constants.FTZZoneStatuses.P) != 0x00) &&
						((activeZoneStatuses ^ Constants.FTZZoneStatuses.Z) != 0x00) && ((activeZoneStatuses ^ Constants.FTZZoneStatuses.D) != 0x00));
			}
		}

		bool FTZIsDomesticZoneStatusOnly
		{
			get { return (((activeZoneStatuses ^ Constants.FTZZoneStatuses.D) == 0x00)); }
		}

		bool FTZIsZoneRestrictedStatusOnly
		{
			get { return (((activeZoneStatuses ^ Constants.FTZZoneStatuses.Z) == 0x00)); }
		}

		bool FTZIsZoneRestrictedAndDomestic
		{
			get { return (((activeZoneStatuses ^ Constants.FTZZoneStatuses.ZonesDZ) == 0x00)); }
		}

		bool FTZIsZoneTransfer
		{
			get { return Declaration.US_F_AdmissionType == FTZAdmissionTypeCodeList.Codes.ZoneToZone; }
		}

		bool FTZIsNotDomesticNorZone
		{
			get { return !FTZIsDomesticZoneStatusOnly && !FTZIsZoneRestrictedStatusOnly && !FTZIsZoneRestrictedAndDomestic && !FTZIsZoneTransfer; }
		}

		public ZString FTZZoneStatusN
		{
			get { return ((activeZoneStatuses & Constants.FTZZoneStatuses.N) == Constants.FTZZoneStatuses.N) ? "x" : ""; }
		}

		public ZString FTZZoneStatusP
		{
			get { return ((activeZoneStatuses & Constants.FTZZoneStatuses.P) == Constants.FTZZoneStatuses.P) ? "x" : ""; }
		}

		public ZString FTZZoneStatusZ
		{
			get { return ((activeZoneStatuses & Constants.FTZZoneStatuses.Z) == Constants.FTZZoneStatuses.Z) ? "x" : ""; }
		}

		public ZString FTZZoneStatusD
		{
			get { return ((activeZoneStatuses & Constants.FTZZoneStatuses.D) == Constants.FTZZoneStatuses.D) ? "x" : ""; }
		}

		public ZString FTZApplicantFirmName
		{
			get
			{
				var importerOfRecord = this.ImporterOfRecord;
				return (importerOfRecord != null) ? importerOfRecord.OH_FullName : ZString.Empty;
			}
		}

		public Image FTZBrokerSignatureImage
		{
			get { return BrokerProvider.BrokerSignature; }
		}

		public ZBool IsFTZOrgBroker => Declaration.WarehouseDocAddress?.Organisation?.OH_IsBroker ?? false;
		public ZBool IsConcurrenceClearedAndIsFTZOrgBroker => IsConcurrenceCleared && IsFTZOrgBroker;

		public Image ConcurrenceFTZBrokerSignatureImage
		{ get { return IsConcurrenceClearedAndIsFTZOrgBroker ? FTZBrokerSignatureImage : null; } }

		public Image PTTFTZBrokerSignatureImage
		{
			get { return IsPTTClearedAndValid ? FTZBrokerSignatureImage : null; }
		}

		public ZString FTZCartman => FTZPTTCarrier != null ? FTZPTTCarrier.OH_FullName : ZString.Empty;

		public OrgHeader FTZPTTCarrier => Declaration is JobDeclaration declaration && declaration.IsFTZAdmission ? declaration.DeliveryOrPickupCartageCo : null;

		public ZString FTZBrokerTitle
		{
			get
			{
				var brokerName = Declaration.BranchIAddressDetails.CompanyName;
				return brokerName + (IsAttorneyInFact ? " AS ATTY-IN-FACT" : "");
			}
		}

		public ZString FTZDeclarantName
		{
			get
			{
				var signatory = this.Signatory;
				return signatory != null ? signatory.GS_FullName : ZString.Empty;
			}
		}

		public ZDateTime FTZDeclarationDate
		{
			get { return Declaration.AdmissionMsgLastAcceptedDate; }
		}

		public ZBool IsPTTWithoutException
		{
			get { return Declaration.US_F_PTTWOExc; }
		}

		public ZBool IsFTZPTTStatusClearPermitToTransfer => Declaration.IsFTZPTTStatusClearPermitToTransfer;

		public ZBool IsConcurrenceCleared
		{
			get { return Declaration.IsConcurrenceCleared; }
		}

		public ZBool IsPTTClearedAndValid
		{
			get { return Declaration.IsPTTClearedAndValid; }
		}

		public FTZ214EntryLineCollection FTZ214PrintCollection
		{
			get { return Declaration.FTZ214PrintCollection; }
		}

		public ZDecimal FTZTotalEnteredValue
		{
			get
			{
				ZDecimal result = 0.0m;
				foreach (CusEntryLine line in EntryLines)
				{
					if (!line.IsSetXLine)
					{
						result += line.CL_CustomsValue;
						if (line.IsVParentLine)
						{
							result += line.ChildVLines.Sum(x => x.CL_CustomsValue);
						}
					}
				}
				return result;
			}
		}

		public ZDecimal FTZTotalAggregateCharges
		{
			get
			{
				ZDecimal result = 0.0m;
				foreach (CusEntryLine line in EntryLines)
				{
					if (!line.IsSetXLine)
					{
						result += line.Charges;
						if (line.IsVParentLine)
						{
							result += line.ChildVLines.Sum(x => x.Charges);
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region IBrokerSignatureProvider Members

		Guid IBrokerSignatureProvider.RegistryBranchPK
		{
			get { return Declaration.RegistryBranchPK; }
		}

		Guid IBrokerSignatureProvider.RegistryCompanyPK
		{
			get { return Declaration.RegistryCompanyPK; }
		}

		bool IBrokerSignatureProvider.HasCurrentElectronicRelease
		{
			get { return Declaration.HasCurrentElectronicRelease; }
		}

		GlbStaff IBrokerSignatureProvider.CusAgent
		{
			get { return Declaration.CusAgent; }
		}

		BusinessObjectFactory IBrokerSignatureProvider.Factory
		{
			get { return Factory; }
		}

		#endregion

		#region IEntryHeaderDutyDataProvider Members
		IEnumerable<IInvoiceLineDutyDataProvider> IEntryHeaderDutyDataProvider.InvoiceLines => InvoiceLines.Cast<IInvoiceLineDutyDataProvider>();
		IEnumerable<IEntryLineDutyDataProvider> IEntryHeaderDutyDataProvider.EntryLines => MergedLines.Cast<IEntryLineDutyDataProvider>();
		#endregion

		#region ILiquidationProvider Members

		JobDeclaration ILiquidationProvider.Declaration => Declaration;

		void ILiquidationProvider.SetAnticipatedLiquidatedDuty(ZDecimal dutyAmount)
		{
			US_ALDuty = dutyAmount;
		}

		void ILiquidationProvider.SetAnticipatedLiquidationDate(ZDateTime dateTime)
		{
			US_ALDate = dateTime;
		}

		#endregion
	}
}
