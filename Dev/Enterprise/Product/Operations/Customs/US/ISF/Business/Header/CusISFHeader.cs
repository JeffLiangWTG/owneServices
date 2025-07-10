using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.US.ISF;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	[CodeProperty(CusISFHeader.Schema.BF_JobReference)]
	[DescriptionProperty(nameof(CusISFHeader.HumanReadableName))]
	[UserDefinedValues]
	[SingleObjectAroundARow]
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.USImporterSecurityFiling)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.USCusISFHeader)]
	public class CusISFHeader : AutoCusISFHeader,
		ICusISFHeader,
		IDocAddresses,
		IImporterSecurityFiling,
		IJobInvoicingPlugIn,
		ITemplateCopyable,
		IEDocsProvider,
		IHaveRequiredDocuments,
		IRoutingSupport,
		ITransportParent,
		IBillGenerationSupport,
		IWorkflowProvider,
		IMessageFailStatusManager,
		IJobDocAddressOverrideSupporter,
		ICustomFieldProvider,
		IBranchProvider,
		IUniversalXMLNoteParent,
		IUSDISHost,
		IDISHostProvider,
		IValidateForCustomsMessagingSupporter,
		IRelatedJob,
		ICusISFAutoSendingMessageSupporter
	{
		#region Schema

		public new class Schema : AutoCusISFHeader.Schema
		{
			public const string BF_EntryNumber = "BF_EntryNumber";
			public const string BF_MasterBill = "BF_MasterBill";
			public const string BF_SuretyCode = "BF_SuretyCode";
			public const string BF_OceanBill = "BF_OceanBill";
			public const string BF_HouseBill = "BF_HouseBill";
			public const string BF_CustomsStatusDescription = "BF_CustomsStatusDescription";
			public const string BF_BondReferenceNumber = "BF_BondReferenceNumber";
			public const string CustomAttribute1 = "CustomAttribute1";
			public const string CustomAttribute2 = "CustomAttribute2";
			public const string BF_ImporterFullName = "BF_ImporterFullName";
			public const string BF_BillStatus = "BF_BillStatus";
			public const string BF_BillStatusDescription = "BF_BillStatusDescription";
			public const string DiscardedCustomsReference = "DiscardedCustomsReference";
			public const string BF_FirstMatchedDate = "BF_FirstMatchedDate";
			public const string DISStatus = "DISStatus";
			public const string DISStatusDescription = "DISStatusDescription";
			public const string DeclarationJobNumber = "DeclarationJobNumber";
		}

		#endregion

		public CusISFHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			BF_CustomsStatusInfo.ValueChanged += BF_CustomsStatusInfo_ValueChanged;
		}

		public static readonly CusISFHeaderTypeDecider TypeDecider = new CusISFHeaderTypeDecider();

		#region Properties

		public ZString DISStatus
		{
			get
			{
				if (disStatus == null)
				{
					disStatus = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;

						var flattenedAddInfos = RequiredDocuments.Cast<JobRequiredDocument>().SelectMany(x => x.AddInfos).Cast<JobRequiredDocumentAddInfo>();
						var statusList = flattenedAddInfos.Where(x => x.EX_ApplicationCode == Core.Constants.Customs.DocumentImageSystemIDs.US_DIS && !x.EX_Status.IsEmpty).Select(x => x.EX_Status).Distinct().ToArray();

						if (statusList.Length > 0)
						{
							result = statusList.Length == 1 ? statusList[0].ToString() : Common.US.DIS.StatusList.Codes.MUL;
						}
						return result;
					});
				}
				return disStatus.Value;
			}
		}
		CachedProperty<ZString> disStatus;

		public ZPropertyInfo DISStatusInfo
		{
			get { return GetZPropertyInfo(Schema.DISStatus); }
		}

		public ZString DISStatusDescription
		{
			get { return Factory.GetCachedValue<Common.US.DIS.StatusList>().GetDescriptionFromCode(DISStatus); }
		}

		public ZPropertyInfo DISStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.DISStatusDescription); }
		}

		#region BF_ConsigneeFullName

		[ReadOnlyMember(nameof(BF_ConsigneeFullName_ReadOnly))]
		public override ZString BF_ConsigneeFullName
		{
			get { return base.BF_ConsigneeFullName; }
			set { base.BF_ConsigneeFullName = value; }
		}

		protected bool BF_ConsigneeFullName_ReadOnly
		{
			get { return BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.Passport && BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.SocialSecurity; }
		}

		#endregion

		#region BF_ShipmentSubType

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ShipmentSubTypeList))]
		public override ZString BF_ShipmentSubType
		{
			get { return base.BF_ShipmentSubType; }
			set { base.BF_ShipmentSubType = value; }
		}

		#endregion

		#region BF_EstimatedValue

		[DecimalPlaces(0)]
		public override ZDecimal BF_EstimatedValue
		{
			get { return base.BF_EstimatedValue; }
			set { base.BF_EstimatedValue = value.Round(0); }
		}

		#endregion

		#region BF_EstimatedQuantityUQ

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.PackingingUnitList))]
		public override ZString BF_EstimatedQuantityUQ
		{
			get { return base.BF_EstimatedQuantityUQ; }
			set { base.BF_EstimatedQuantityUQ = value; }
		}

		#endregion

		#region BF_EstimatedWeightUQ

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.WeightUQList))]
		public override ZString BF_EstimatedWeightUQ
		{
			get { return base.BF_EstimatedWeightUQ; }
			set { base.BF_EstimatedWeightUQ = value; }
		}

		#endregion

		#region BF_EstimatedWeight

		[MeasureUnit(Schema.BF_EstimatedWeightUQ, MeasureUnitType.Weight)]
		public override ZInt BF_EstimatedWeight
		{
			get { return base.BF_EstimatedWeight; }
			set { base.BF_EstimatedWeight = value; }
		}

		#endregion

		public ZWeight EstimatedWeight
		{
			get { return new ZWeight((decimal)BF_EstimatedWeight, BF_EstimatedWeightUQ); }
		}

		[ReadOnly(true)]
		public override ZString BF_JobReference
		{
			get { return base.BF_JobReference; }
			set { base.BF_JobReference = value; }
		}

		[ReadOnlyMember(nameof(BF_GB_ReadOnly))]
		public override ZGuid BF_GB
		{
			get { return base.BF_GB; }
			set
			{
				var oldValue = BF_GB;
				base.BF_GB = value;
				if (!IsCopying && oldValue != BF_GB)
				{
					BF_NumOfHarmChars = ISFRegistry.Instance.ImporterSecurityFilingNoOfHTSDigits.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				}
			}
		}

		protected bool BF_GB_ReadOnly
		{
			get { return !BF_CustomsReference.IsEmpty || (!BF_CustomsStatus.IsEmpty && BF_CustomsStatus != MessageStatusList.Codes.NotSentISF) || Messages.Find(new ZQuery(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded)).Length > 0; }
		}

		[ReadOnlyMember(nameof(BF_CustomsReference_ReadOnly))]
		public override ZString BF_CustomsReference
		{
			get => base.BF_CustomsReference;
			set => base.BF_CustomsReference = value;
		}

		bool BF_CustomsReference_ReadOnly
		{
			get
			{
				if (!isBF_CustomsReferenceReadOnlyCached.HasValue)
				{
					isBF_CustomsReferenceReadOnlyCached = !BF_CustomsStatus.IsEmpty &&
							BF_CustomsStatus != MessageStatusList.Codes.NotSentISF &&
							(BF_CustomsStatus == MessageStatusList.Codes.AwaitingISFAdd ||
							BF_CustomsStatus == MessageStatusList.Codes.AwaitingISFDelete ||
							BF_CustomsStatus == MessageStatusList.Codes.AwaitingISFReplace ||
							BF_CustomsStatus == MessageStatusList.Codes.ClearISFAdd ||
							BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete ||
							BF_CustomsStatus == MessageStatusList.Codes.ClearISFReplace ||
							BF_CustomsStatus == MessageStatusList.Codes.ClearWithWarningISFAdd ||
							BF_CustomsStatus == MessageStatusList.Codes.ClearWithWarningISFDelete ||
							BF_CustomsStatus == MessageStatusList.Codes.ClearWithWarningISFReplace ||
							HasAClearMessage());
				}
				return isBF_CustomsReferenceReadOnlyCached.Value;
			}
		}
		bool? isBF_CustomsReferenceReadOnlyCached;

		bool HasAClearMessage()
		{
			return Messages.OfType<EDIMessage>().Any(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse &&
				   x.GetMessageBlocks<ISFSF90>().Any(y => y.MessageTypeCode == ISFMessageStatus.Codes.Accepted ||
																y.MessageTypeCode == ISFMessageStatus.Codes.AcceptedWithWarning ||
																y.MessageTypeCode == ISFMessageStatus.Codes.RecordAcceptedWithWarning));
		}

		internal void ClearBF_CustomsReference_ReadOnlyCache()
		{
			isBF_CustomsReferenceReadOnlyCached = null;
		}

		void BF_CustomsStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			isBF_CustomsReferenceReadOnlyCached = null;
		}

		[ReadOnly(true)]
		public override ZDateTime BF_FirstAcceptedDate
		{
			get { return base.BF_FirstAcceptedDate; }
			set { base.BF_FirstAcceptedDate = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime BF_LastAcceptedDate
		{
			get { return base.BF_LastAcceptedDate; }
			set { base.BF_LastAcceptedDate = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.EntryTypes))]
		public override ZString BF_EntryType
		{
			get { return base.BF_EntryType; }
			set
			{
				ZString oldValue = BF_EntryType;
				base.BF_EntryType = value;
				if (!IsCopying && oldValue != BF_EntryType)
				{
					Lines.MarkAsNeedingValidation();
					DocAddresses.MarkAsNeedingValidation();
					if (IsISF5Entry)
					{
						BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ImporterCodeTypes))]
		public override ZString BF_ImporterCodeType
		{
			get { return base.BF_ImporterCodeType; }
			set
			{
				ZString oldValue = BF_ImporterCodeType;
				base.BF_ImporterCodeType = value;
				if (!IsCopying && oldValue != BF_ImporterCodeType)
				{
					if ((oldValue == ImporterCodeTypeList.Codes.Passport || oldValue == ImporterCodeTypeList.Codes.SocialSecurity)
						&& BF_ImporterCodeType != ImporterCodeTypeList.Codes.SocialSecurity && BF_ImporterCodeType != ImporterCodeTypeList.Codes.Passport)
					{
						BF_ImporterFullName = ZString.Empty;
						BF_DateOfBirth = ZDateTime.Empty;
					}

					if (oldValue == ConsigneeCodeTypeList.Codes.Passport)
					{
						BF_CountryOfIssue = ZString.Empty;
					}
					if (BF_ConsigneeCode == BF_ImporterCode && Lookups.ConsigneeCodeTypes.ContainsCode(BF_ImporterCodeType))
					{
						BF_ConsigneeCodeType = BF_ImporterCodeType;
					}
					UpdateBF_OH_ImporterIfNeeded();
					if (BF_ImporterCodeType == ImporterCodeTypeList.Codes.SCAC && BF_ImporterCode.Length > 4)
					{
						BF_ImporterCode = BF_ImporterCode.Left(4);
					}
					else
					{
						BF_ImporterCode = ZString.Empty;
					}
					UpdateBondHolderIfNeeded();
					UpdateImporterPassportIssueCountryIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.YesNoDefaultList))]
		public override ZString BF_SendEquipment
		{
			get { return base.BF_SendEquipment; }
			set { base.BF_SendEquipment = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.MergeStyleList))]
		public override ZString BF_LineMergeStyle
		{
			get { return base.BF_LineMergeStyle; }
			set { base.BF_LineMergeStyle = value; }
		}

		public override ZGuid BF_OH_Importer
		{
			get { return base.BF_OH_Importer; }
			set
			{
				ZGuid oldValue = BF_OH_Importer;
				base.BF_OH_Importer = value;
				if (!IsCopying && oldValue != BF_OH_Importer)
				{
					UpdateImporterCodeDetailsIfNeeded();
					UpdateBondDetailsFromImporterIfNeeded();
					var linesArray = Lines.ToArray();
					foreach (var line in linesArray)
					{
						if (!line.IsDeleted)
						{
							line.PartSyncManager.Refresh();
						}
					}
					Lines.MarkAsNeedingValidation();
				}
			}
		}

		public ZString ImporterName
		{
			get { return Importer != null ? Importer.OH_FullNameTruncated : ZString.Empty; }
		}

		public override ZString BF_ImporterCode
		{
			get { return base.BF_ImporterCode; }
			set
			{
				ZString oldValue = BF_ImporterCode;
				base.BF_ImporterCode = value;
				if (!IsCopying && oldValue != BF_ImporterCode)
				{
					UpdateConsingeeDetailsFromImporter();
					UpdateBondHolderIfNeeded();
					UpdateBF_OH_ImporterIfNeeded();
					UpdateImporterPassportIssueCountryIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ImporterCodes))]
		[MaxLength(CusISFHeader.Schema.BF_ImporterCodeMaxLength)]
		[ReadOnlyMember(nameof(ImporterCodeForDisplay_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.ISF.Business.CusISFHeader|ImporterCodeForDisplay", Caption = "Importer Identification")]
		public ZString ImporterCodeForDisplay
		{
			get
			{
				var importerCode = BF_ImporterCode;
				if (BF_ImporterCodeType == ImporterCodeTypeList.Codes.SocialSecurity && !OrgDetailsViewPersonalInformationIsAllow)
				{
					return SocialSecurityNumberValidator.SSNWithMask;
				}
				return importerCode;
			}
			set { BF_ImporterCode = value; }
		}

		bool ImporterCodeForDisplay_ReadOnly
		{
			get { return BF_ImporterCodeType == ConsigneeCodeTypeList.Codes.SocialSecurity && !OrgDetailsViewPersonalInformationIsAllow; }
		}

		public ZWrappedPropertyInfo ImporterCodeForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ImporterCodeForDisplay), x => BF_ImporterCodeInfo); }
		}

		bool OrgDetailsViewPersonalInformationIsAllow => Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;

		void UpdateImporterPassportIssueCountryIfNeeded()
		{
			if (BF_ImporterCodeType == ImporterCodeTypeList.Codes.Passport && !BF_ImporterCode.IsEmpty)
			{
				var result = GetPassportCountry(BF_ImporterCode);
				if (!result.IsEmpty)
				{
					BF_CountryOfIssue = result;
				}
			}
		}

		ZString GetPassportCountry(ZString passportNo)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, ImporterCodeTypeList.Codes.Passport);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, passportNo);
			var passport = Factory.LoadTop1<OrgCusCode>(query);
			return passport != null ? passport.OK_RN_NKCodeCountry : ZString.Empty;
		}

		protected int BF_ImporterCode_MaxLength
		{
			get { return BF_ImporterCodeType == ImporterCodeTypeList.Codes.SCAC ? 4 : Schema.BF_ImporterCodeMaxLength; }
		}

		public bool IsISF10Entry
		{
			get { return SubmissionTypeList.IsISF10Entry(BF_EntryType); }
		}

		public bool IsISF5Entry
		{
			get { return SubmissionTypeList.IsISF5Entry(BF_EntryType); }
		}

		public bool IsBondDataRequired
		{
			get
			{
				return ShipmentTypeList.IsBondDataRequired(BF_ShipmentType)
					&& !SubmissionTypeList.IsLateEntry(BF_EntryType);
			}
		}

		public override ZString BF_RL_NKPlaceOfDelivery
		{
			get { return base.BF_RL_NKPlaceOfDelivery; }
			set
			{
				base.BF_RL_NKPlaceOfDelivery = value;
				RequiredDocuments.SetAllDocumentsReceivedEventLogger();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ShipmentTypes))]
		public override ZString BF_ShipmentType
		{
			get { return base.BF_ShipmentType; }
			set
			{
				ZString oldValue = BF_ShipmentType;
				base.BF_ShipmentType = value;
				if (!IsCopying && oldValue != BF_ShipmentType)
				{
					if (IsBondDataRequired)
					{
						UpdateBondHolderIfNeeded();
						UpdateBondDetailsFromImporterIfNeeded();
					}
					else
					{
						BF_BondNumberOrHolder = ZString.Empty;
						BF_BondActivityCode = ZString.Empty;
						BF_BondType = ZString.Empty;
						BF_SuretyCode = ZString.Empty;
						BF_BondReferenceNumber = ZString.Empty;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.TransportModes))]
		public override ZString BF_TransportMode
		{
			get { return base.BF_TransportMode; }
			set { base.BF_TransportMode = value; }
		}

		public override ZDateTime BF_DateOfBirth
		{
			get { return base.BF_DateOfBirth; }
			set
			{
				ZDateTime oldValue = BF_DateOfBirth;
				base.BF_DateOfBirth = value;
				if (!IsCopying && oldValue != BF_DateOfBirth)
				{
					UpdateConsigneePassportDetailsFromImporterIfNeeded();
				}
			}
		}

		protected bool BF_DateOfBirth_ReadOnly
		{
			get { return BF_ImporterCodeType != ImporterCodeTypeList.Codes.Passport && BF_ImporterCodeType != ImporterCodeTypeList.Codes.SocialSecurity; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.Countries))]
		public override ZString BF_CountryOfIssue
		{
			get { return base.BF_CountryOfIssue; }
			set
			{
				ZString oldValue = BF_CountryOfIssue;
				base.BF_CountryOfIssue = value;
				if (!IsCopying && oldValue != BF_CountryOfIssue)
				{
					UpdateConsigneePassportDetailsFromImporterIfNeeded();
				}
			}
		}

		protected bool BF_CountryOfIssue_ReadOnly
		{
			get { return BF_ImporterCodeType != ImporterCodeTypeList.Codes.Passport; }
		}

		#region Importer Full Name
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_ImporterFullName
		{
			get
			{
				CusISFBill bill = ImporterFullName;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.FullNameOfISFImporter, BF_ImporterFullNameInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_ImporterFullName();
				}
			}
		}

		public CusISFBill ImporterFullName
		{
			get
			{
				if (importerFullName == null || importerFullName.IsDeleted || importerFullName.BB_BillType != BillTypeList.Codes.FullNameOfISFImporter)
				{
					importerFullName = GetFirstElementFromReferences(BillTypeList.Codes.FullNameOfISFImporter);
				}
				return importerFullName;
			}
		}
		CusISFBill importerFullName;

		public ZPropertyInfo BF_ImporterFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.BF_ImporterFullName); }
		}

		protected bool BF_ImporterFullName_ReadOnly
		{
			get { return BF_ImporterCodeType != ImporterCodeTypeList.Codes.Passport && BF_ImporterCodeType != ImporterCodeTypeList.Codes.SocialSecurity; }
		}

		[ResourceStringData("Enterprise.Customs.US.ISF.Business.CusISFHeader|DiscardedCustomsReference", Caption = "Discarded Customs Reference")]
		public ZString DiscardedCustomsReference
		{
			get
			{
				if (discardedCustomsReferenceCached == null)
				{
					discardedCustomsReferenceCached = new CachedProperty<ZString>(Factory, delegate
					{
						var listOfUniquedDiscardedRefs = new List<ZString>();
						foreach (EDIMessage message in Messages)
						{
							if (message.EM_Status == EDIMessage.Status.Discarded && !message.EM_ApplicationReference.IsEmpty && !listOfUniquedDiscardedRefs.Contains(message.EM_ApplicationReference))
							{
								listOfUniquedDiscardedRefs.Add(message.EM_ApplicationReference);
							}
						}
						var result = ZString.Empty;
						if (listOfUniquedDiscardedRefs.Count > 0)
						{
							listOfUniquedDiscardedRefs.Sort();
							result = new ZStringBuilder(listOfUniquedDiscardedRefs).ToStringWithDelimiterBetweenAppends(", ");
						}
						return result;
					});
				}

				return discardedCustomsReferenceCached.Value;
			}
		}
		CachedProperty<ZString> discardedCustomsReferenceCached;

		public ZPropertyInfo DiscardedCustomsReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.DiscardedCustomsReference); }
		}

		public ZString BF_BillStatus
		{
			get
			{
				if (bF_BillStatusCached == null)
				{
					bF_BillStatusCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						var hasBlank = false;
						foreach (CusISFBill bill in ReferenceDatas)
						{
							if (bill.IsLowestBill)
							{
								var billStatus = bill.BB_CustomsStatus;
								if (billStatus.IsEmpty)
								{
									hasBlank = true;
								}
								else
								{
									if (result.IsEmpty)
									{
										result = billStatus;
									}
									else if (result != billStatus)
									{
										result = Common.US.ISF.ISFStatusHelper.Multiple;
										break;
									}
								}
							}
						}
						if (!result.IsEmpty && hasBlank && result != Common.US.ISF.ISFStatusHelper.Multiple)
						{
							result += ",No Status";
						}
						return result;
					}
					);
				}

				return bF_BillStatusCached.Value;
			}
		}
		CachedProperty<ZString> bF_BillStatusCached;

		public ZPropertyInfo BF_BillStatusInfo
		{
			get { return GetZPropertyInfo(Schema.BF_BillStatus); }
		}

		public ZDateTime BF_FirstMatchedDate
		{
			get
			{
				if (bF_FirstMatchedDateCached == null)
				{
					bF_FirstMatchedDateCached = new CachedProperty<ZDateTime>(Factory, delegate
					{
						var earlierDate = ReferenceDatas.Where(x => x.IsLowestBill && !x.BB_FirstMatchedDate.IsEmpty).Select(x => x.BB_FirstMatchedDate).Distinct().OrderBy(x => x).FirstOrDefault();
						return earlierDate != ZDateTime.Empty ? earlierDate : ZDateTime.Empty;
					}
					);
				}

				return bF_FirstMatchedDateCached.Value;
			}
		}
		CachedProperty<ZDateTime> bF_FirstMatchedDateCached;

		public ZPropertyInfo BF_FirstMatchedDateInfo
		{
			get { return GetZPropertyInfo(Schema.BF_FirstMatchedDate); }
		}

		public ZString BF_BillStatusDescription
		{
			get
			{
				if (bF_BillStatusDescriptionCached == null)
				{
					bF_BillStatusDescriptionCached = new CachedProperty<ZString>(Factory, delegate
					{
						ZString result;
						var status = BF_BillStatus;
						var statuses = status.Split(',');
						var extraMessage = "";
						if (statuses.Length > 1 && statuses[1].EqualsIgnoringCase("No Status"))
						{
							status = statuses[0];
							extraMessage = " Also there is a bill without any status.";
						}
						if (status == Common.US.ISF.ISFStatusHelper.Multiple)
						{
							result = MultipleBillsWithDifferentStatuses;
						}
						else
						{
							result = Factory.GetCachedValue<DispositionCodeList>().GetDescriptionFromCode(status);
						}
						return result + extraMessage;
					});
				}
				return bF_BillStatusDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> bF_BillStatusDescriptionCached;

		public ZPropertyInfo BF_BillStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BF_BillStatusDescription); }
		}

		public static string MultipleBillsWithDifferentStatuses
		{
			get { return ResString.GetMultilingualString("7431E935-21D9-4EB0-94B9-564DBF4945AA", "There are multiple bills with different statuses"); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ConsigneeCodeTypes))]
		public override ZString BF_ConsigneeCodeType
		{
			get { return base.BF_ConsigneeCodeType; }
			set
			{
				ZString oldValue = BF_ConsigneeCodeType;
				base.BF_ConsigneeCodeType = value;
				if (!IsCopying && oldValue != BF_ConsigneeCodeType)
				{
					BF_ConsigneeCode = ZString.Empty;
					if ((oldValue == ConsigneeCodeTypeList.Codes.SocialSecurity || oldValue == ConsigneeCodeTypeList.Codes.Passport) &&
						BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.Passport && BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.SocialSecurity)
					{
						BF_ConsigneeFullName = ZString.Empty;
						BF_ConsigneeDateOfBirth = ZDateTime.Empty;
					}
					if (oldValue == ConsigneeCodeTypeList.Codes.Passport)
					{
						BF_ConsigneeCountryOfIssue = ZString.Empty;
					}
					UpdateConsigneePassportIssueCountryIfNeeded();
				}
			}
		}

		void UpdateConsigneePassportIssueCountryIfNeeded()
		{
			if (BF_ConsigneeCodeType == ImporterCodeTypeList.Codes.Passport && !BF_ConsigneeCode.IsEmpty)
			{
				var result = GetPassportCountry(BF_ConsigneeCode);
				if (!result.IsEmpty)
				{
					BF_ConsigneeCountryOfIssue = result;
				}
			}
		}

		public override ZString BF_ConsigneeCode
		{
			get { return base.BF_ConsigneeCode; }
			set
			{
				var oldValue = BF_ConsigneeCode;
				base.BF_ConsigneeCode = value;
				if (!IsCopying && oldValue != BF_ConsigneeCode)
				{
					UpdateConsigneePassportIssueCountryIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ConsigeeCodes))]
		[MaxLength(CusISFHeader.Schema.BF_ConsigneeCodeMaxLength)]
		[ReadOnlyMember(nameof(ConsigneeCodeForDisplay_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.ISF.Business.CusISFHeader|ConsigneeCodeForDisplay", Caption = "Consignee ID")]
		public ZString ConsigneeCodeForDisplay
		{
			get
			{
				var consigneeCode = BF_ConsigneeCode;
				if (BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.SocialSecurity && !OrgDetailsViewPersonalInformationIsAllow)
				{
					return SocialSecurityNumberValidator.SSNWithMask;
				}
				return consigneeCode;
			}
			set { BF_ConsigneeCode = value; }
		}

		bool ConsigneeCodeForDisplay_ReadOnly
		{
			get { return BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.SocialSecurity && !OrgDetailsViewPersonalInformationIsAllow; }
		}

		public ZWrappedPropertyInfo ConsigneeCodeForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ConsigneeCodeForDisplay), x => BF_ConsigneeCodeInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.BondNumberOrHolderCodes))]
		[MaxLength(CusISFHeader.Schema.BF_BondNumberOrHolderMaxLength)]
		[ReadOnlyMember(nameof(BondNumberOrHolderForDisplay_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.ISF.Business.CusISFHeader|BondNumberOrHolderForDisplay", Caption = "Bond Holder")]
		public ZString BondNumberOrHolderForDisplay
		{
			get
			{
				var bondNumberOrHolder = BF_BondNumberOrHolder;
				if (SocialSecurityNumberValidator.IsValidSSN(bondNumberOrHolder) && !OrgDetailsViewPersonalInformationIsAllow)
				{
					return SocialSecurityNumberValidator.SSNWithMask;
				}
				return bondNumberOrHolder;
			}
			set { BF_BondNumberOrHolder = value; }
		}

		bool BondNumberOrHolderForDisplay_ReadOnly
		{
			get { return SocialSecurityNumberValidator.IsValidSSN(BF_BondNumberOrHolder) && !OrgDetailsViewPersonalInformationIsAllow; }
		}

		public ZWrappedPropertyInfo BondNumberOrHolderForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BondNumberOrHolderForDisplay), x => BF_BondNumberOrHolderInfo); }
		}

		protected bool BF_ConsigneeDateOfBirth_ReadOnly
		{
			get { return BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.Passport && BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.SocialSecurity; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.Countries))]
		public override ZString BF_ConsigneeCountryOfIssue
		{
			get { return base.BF_ConsigneeCountryOfIssue; }
			set { base.BF_ConsigneeCountryOfIssue = value; }
		}

		protected bool BF_ConsigneeCountryOfIssue_ReadOnly
		{
			get { return BF_ConsigneeCodeType != ConsigneeCodeTypeList.Codes.Passport; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.USCarrierList))]
		public override ZString BF_SCAC
		{
			get { return base.BF_SCAC; }
			set { base.BF_SCAC = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.BondActivityCodeList))]
		public override ZString BF_BondActivityCode
		{
			get { return base.BF_BondActivityCode; }
			set { base.BF_BondActivityCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.BondTypeList))]
		public override ZString BF_BondType
		{
			get { return base.BF_BondType; }
			set { base.BF_BondType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.ActionReasonCodeList))]
		public override ZString BF_ActionReasonCode
		{
			get { return base.BF_ActionReasonCode; }
			set { base.BF_ActionReasonCode = value; }
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.MessageStatusList))]
		public override ZString BF_CustomsStatus
		{
			get { return base.BF_CustomsStatus; }
			set
			{
				ZString oldValue = BF_CustomsStatus;
				base.BF_CustomsStatus = value;
				if (!IsCopying && oldValue != BF_CustomsStatus)
				{
					LogManager.AddALogIfNecessary(oldValue, BF_CustomsStatus);
					if (BF_CustomsStatus.Equals(MessageStatusList.Codes.ClearISFDelete))
					{
						MarkMatchedOceanAndHouseBillsAsRemoved();
					}
				}
			}
		}

		void MarkMatchedOceanAndHouseBillsAsRemoved()
		{
			foreach (CusISFBill bill in ReferenceDatas)
			{
				if ((bill.IsOceanBillOfLading || bill.IsHouseBillOfLading) && !bill.BB_CustomsStatus.IsEmpty)
				{
					bill.BB_CustomsStatus = DispositionCodeList.Codes.XX;
				}
			}
		}

		public ZString BF_CustomsStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(BF_CustomsStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo BF_CustomsStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BF_CustomsStatusDescription); }
		}

		#region Entry Number
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_EntryNumber
		{
			get
			{
				CusISFBill bill = EntryNumber;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.USCBPEntryNumber, BF_EntryNumberInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_EntryNumber();
				}
			}
		}

		public CusISFBill EntryNumber
		{
			get
			{
				if (entryNumber == null || entryNumber.IsDeleted || entryNumber.BB_BillType != BillTypeList.Codes.USCBPEntryNumber)
				{
					entryNumber = GetFirstElementFromReferences(BillTypeList.Codes.USCBPEntryNumber);
				}
				return entryNumber;
			}
		}
		CusISFBill entryNumber;

		public ZString DeclarationJobNumber
		{
			get
			{
				return string.Join(",", ReferenceDatas.SelectMany(bill => bill.JobDeclarationNumbers).Distinct());
			}
		}

		public ZPropertyInfo BF_EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BF_EntryNumber); }
		}
		#endregion

		#region Ocean Bill
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_OceanBill
		{
			get
			{
				CusISFBill bill = OceanBill;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.OceanBillOfLading, BF_OceanBillInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_OceanBill();
				}
			}
		}

		public CusISFBill OceanBill
		{
			get
			{
				if (oceanBill == null || oceanBill.IsDeleted || oceanBill.BB_BillType != BillTypeList.Codes.OceanBillOfLading)
				{
					oceanBill = GetFirstElementFromReferences(BillTypeList.Codes.OceanBillOfLading);
				}
				return oceanBill;
			}
		}
		CusISFBill oceanBill;

		public ZPropertyInfo BF_OceanBillInfo
		{
			get { return GetZPropertyInfo(Schema.BF_OceanBill); }
		}
		#endregion

		#region House Bill
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_HouseBill
		{
			get
			{
				CusISFBill bill = HouseBill;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.HouseBillOfLading, BF_HouseBillInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_HouseBill();
				}
			}
		}

		public CusISFBill HouseBill
		{
			get
			{
				if (houseBill == null || houseBill.IsDeleted || houseBill.BB_BillType != BillTypeList.Codes.HouseBillOfLading)
				{
					houseBill = GetFirstElementFromReferences(BillTypeList.Codes.HouseBillOfLading);
				}
				return houseBill;
			}
		}
		CusISFBill houseBill;

		public ZPropertyInfo BF_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.BF_HouseBill); }
		}
		#endregion

		#region Surety Code
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_SuretyCode
		{
			get
			{
				CusISFBill bill = SuretyCode;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.SuretyCode, BF_SuretyCodeInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_SuretyCode();
				}
			}
		}

		public CusISFBill SuretyCode
		{
			get
			{
				if (suretyCode == null || suretyCode.IsDeleted || suretyCode.BB_BillType != BillTypeList.Codes.SuretyCode)
				{
					suretyCode = GetFirstElementFromReferences(BillTypeList.Codes.SuretyCode);
				}
				return suretyCode;
			}
		}
		CusISFBill suretyCode;

		public ZPropertyInfo BF_SuretyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BF_SuretyCode); }
		}

		#endregion

		#region Bond Reference Number
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_BondReferenceNumber
		{
			get
			{
				CusISFBill bill = BondReferenceNumber;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.BondReferenceNumber, BF_BondReferenceNumberInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_BondReferenceNumber();
				}
			}
		}

		public CusISFBill BondReferenceNumber
		{
			get
			{
				if (bondReferenceNumber == null || bondReferenceNumber.IsDeleted || bondReferenceNumber.BB_BillType != BillTypeList.Codes.BondReferenceNumber)
				{
					bondReferenceNumber = GetFirstElementFromReferences(BillTypeList.Codes.BondReferenceNumber);
				}
				return bondReferenceNumber;
			}
		}
		CusISFBill bondReferenceNumber;

		public ZPropertyInfo BF_BondReferenceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BF_BondReferenceNumber); }
		}
		#endregion

		#region Master Bill
		[MaxLength(CusISFBill.Schema.BB_BillNumMaxLength)]
		public ZString BF_MasterBill
		{
			get
			{
				CusISFBill bill = MasterBill;
				return bill == null ? ZString.Empty : bill.BB_BillNum;
			}
			set
			{
				LocateAndChangeElement(BillTypeList.Codes.MasterBillOfLading, BF_MasterBillInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBF_MasterBill();
				}
			}
		}

		public CusISFBill MasterBill
		{
			get
			{
				if (masterBill == null || masterBill.IsDeleted || masterBill.BB_BillType != BillTypeList.Codes.MasterBillOfLading)
				{
					masterBill = GetFirstElementFromReferences(BillTypeList.Codes.MasterBillOfLading);
				}
				return masterBill;
			}
		}
		CusISFBill masterBill;

		public ZPropertyInfo BF_MasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.BF_MasterBill); }
		}
		#endregion

		[List(nameof(Lookups) + "." + nameof(CusISFHeaderLookups.NumberOfHarmonizedDigitsToReportList))]
		public override ZString BF_NumOfHarmChars
		{
			get { return base.BF_NumOfHarmChars; }
			set { base.BF_NumOfHarmChars = value; }
		}

		public ZString MainShipToPartyOrgName
		{
			get
			{
				if (MainShipToParty.Organisation != null)
				{
					return MainShipToParty.Organisation.OH_FullNameTruncated;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString HouseMasterOceanBillReferences
		{
			get
			{
				ZString result = ZString.Empty;
				ZString bMresult = ZString.Empty;
				ZString mBresult = ZString.Empty;
				ZString oBresult = ZString.Empty;
				foreach (CusISFBill reference in ReferenceDatas)
				{
					switch (reference.BB_BillType)
					{
						case BillTypeList.Codes.HouseBillOfLading:
							bMresult += reference.BB_BillNum + ',';
							break;
						case BillTypeList.Codes.MasterBillOfLading:
							mBresult += reference.BB_BillNum + ',';
							break;
						case BillTypeList.Codes.OceanBillOfLading:
							oBresult += reference.BB_BillNum + ',';
							break;
					}
				}

				if (!bMresult.IsEmpty)
				{
					bMresult = bMresult.TrimEnd(',');
					result = "House: " + bMresult + " ";
				}
				if (!mBresult.IsEmpty)
				{
					mBresult = mBresult.TrimEnd(',');
					result += "Master: " + mBresult + " ";
				}
				if (!oBresult.IsEmpty)
				{
					oBresult = oBresult.TrimEnd(',');
					result += "Ocean Bill: " + oBresult;
				}
				return result;
			}
		}

		#endregion

		public ZBool SuspendDeleteProductLines { get; set; }

		public bool IsBond16SingleTransaction
		{
			get { return BF_BondActivityCode == ISFBondActivityCodeList.Codes.ISFBond16 && BF_BondType == ImporterBondTypeList.Codes.SingleTransactionBond; }
		}

		public bool IsWaitingForResponse
		{
			get { return MessageStatusList.IsWaitingForResponse(BF_CustomsStatus); }
		}

		public bool ShouldSendAdd
		{
			get { return BF_CustomsReference.IsEmpty; }
		}

		public bool CanSendDelete
		{
			get { return !BF_CustomsReference.IsEmpty; }
		}

		public void ResetToOriginal()
		{
			BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
			BF_CustomsReference = ZString.Empty;
			foreach (EDIMessage message in Messages.ToArray())
			{
				message.EM_Status = EDIMessage.Status.Discarded;
			}
			LogManager.CancelAll();

			foreach (CusISFBill reference in ReferenceDatas)
			{
				reference.LogManager.CancelAll();
			}
		}

		public ZString GetHarmonisedNumAsRequired(ZString tariffNumber)
		{
			return tariffNumber.Left(NumberOfHarmonisedDigitsRequired);
		}

		public int NumberOfHarmonisedDigitsRequired
		{
			get
			{
				if (numberOfHarmonisedDigitsRequiredCached == null)
				{
					numberOfHarmonisedDigitsRequiredCached = new CachedProperty<int>(Factory, delegate
					{
						int numberOfDigits = 10;
						switch (BF_NumOfHarmChars)
						{
							case NumberOfHarmonizedDigitsList.Codes.Six:
								numberOfDigits = 6;
								break;
							case NumberOfHarmonizedDigitsList.Codes.Eight:
								numberOfDigits = 8;
								break;
						}
						return numberOfDigits;
					});
				}
				return numberOfHarmonisedDigitsRequiredCached.Value;
			}
		}
		CachedProperty<int> numberOfHarmonisedDigitsRequiredCached;

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString CustomAttribute1
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.CustomAttribute1); }
			set
			{
				this.SetUserDefinedValue(Schema.CustomAttribute1, value);
				CustomAttribute1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomAttribute1Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttribute1); }
		}

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString CustomAttribute2
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.CustomAttribute2); }
			set
			{
				this.SetUserDefinedValue(Schema.CustomAttribute2, value);
				CustomAttribute2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomAttribute2Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttribute2); }
		}

		#region First US Transport

		public Transport FirstUSTransport
		{
			get
			{
				Transport result = null;

				if (Transports.Count > 0)
				{
					result = Transports.Cast<Transport>().Where(x => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(x.JW_RL_NKDiscPort.Substring(0, 2)) == Core.Constants.CountryCodes.UnitedStates).OrderBy(x => x.JW_ETA).FirstOrDefault();

					if (result == null)
					{
						result = Transports.Cast<Transport>().Where(x => x.JW_RL_NKDiscPort == ZString.Empty).OrderBy(x => x.JW_ETA).FirstOrDefault();
					}
				}

				return result;
			}
		}

		#endregion

		#region Related Objects

		#region Lines

		[ChildEditable]
		public CusISFLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new CusISFLineCollection(this);
					RegisterEditableChildObject(fLines);
				}
				return fLines;
			}
		}
		CusISFLineCollection fLines;

		#endregion

		#region Equipments

		[ChildEditable]
		public CusISFEquipCollection Equipments
		{
			get
			{
				if (fEquipments == null)
				{
					fEquipments = new CusISFEquipCollection(this);
					RegisterEditableChildObject(fEquipments);
				}
				return fEquipments;
			}
		}
		CusISFEquipCollection fEquipments;

		#endregion

		#region ReferenceData

		[ChildEditable]
		public CusISFBillCollection ReferenceDatas
		{
			get
			{
				if (fReferenceDatas == null)
				{
					fReferenceDatas = new CusISFBillCollection(this);
					RegisterEditableChildObject(fReferenceDatas);
				}
				return fReferenceDatas;
			}
		}
		CusISFBillCollection fReferenceDatas;

		#endregion

		#region Messages

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		#endregion

		#region DocAddresses

		[ChildEditable]
		[ChildEditableTestExclude]
		public ISFDocAddress MainShipToParty
		{
			get
			{
				if (shipToParty == null || shipToParty.IsDeleted)
				{
					shipToParty = DocAddresses.FindOrCreateWithRequirement(ISFDocAddressRequirementProvider.ShipToPartyDocAddressRequirement);
				}

				return shipToParty;
			}
		}
		ISFDocAddress shipToParty;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ISFDocAddress SellingParty
		{
			get
			{
				if (sellingParty == null || sellingParty.IsDeleted)
				{
					sellingParty = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.SellingParty));
				}

				return sellingParty;
			}
		}
		ISFDocAddress sellingParty;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ISFDocAddress BuyingParty
		{
			get
			{
				if (buyingParty == null || buyingParty.IsDeleted)
				{
					if (buyingParty != null)
					{
						buyingParty.OrgHeaderAfterChange -= new EventHandler(BuyingParty_OrgHeaderAfterChange);
					}
					buyingParty = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.BuyingParty));
					buyingParty.OrgHeaderAfterChange += new EventHandler(BuyingParty_OrgHeaderAfterChange);
				}

				return buyingParty;
			}
		}
		ISFDocAddress buyingParty;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ISFDocAddress StuffingLocation
		{
			get
			{
				if (stuffingLocation == null || stuffingLocation.IsDeleted)
				{
					stuffingLocation = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ScheduledContainerStuffingLocation));
				}

				return stuffingLocation;
			}
		}
		ISFDocAddress stuffingLocation;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ISFDocAddress Consolidator
		{
			get
			{
				if (consolidator == null || consolidator.IsDeleted)
				{
					consolidator = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.Consolidator));
				}

				return consolidator;
			}
		}
		ISFDocAddress consolidator;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ISFDocAddress BookingParty
		{
			get
			{
				if (bookingParty == null || bookingParty.IsDeleted)
				{
					bookingParty = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress));
				}

				return bookingParty;
			}
		}
		ISFDocAddress bookingParty;

		[ChildEditable]
		public CusISFHeaderExtraShipToPartyAddresses ExtraShipToPartyAddresses
		{
			get
			{
				if (extraShipToPartyAddresses == null)
				{
					extraShipToPartyAddresses = new CusISFHeaderExtraShipToPartyAddresses(this);
					RegisterEditableChildObject(extraShipToPartyAddresses);
				}
				return extraShipToPartyAddresses;
			}
		}
		CusISFHeaderExtraShipToPartyAddresses extraShipToPartyAddresses;

		[ChildEditable]
		public CusISFHeaderManufacturerAddresses ManufacturerAddresses
		{
			get
			{
				if (manufacturerAddresses == null)
				{
					manufacturerAddresses = new CusISFHeaderManufacturerAddresses(this);

					RegisterEditableChildObject(manufacturerAddresses);
				}
				return manufacturerAddresses;
			}
		}
		CusISFHeaderManufacturerAddresses manufacturerAddresses;

		public OrgAddress ImporterCustomsAddressFallingBackToMainAddress
		{
			get
			{
				var importer = Importer;
				var importerAddressDetails = importer.GetCustomsAddressDetailsFallingBackToMainAddress();
				return importerAddressDetails;
			}
		}

		#endregion

		#region Transports

		[ChildEditable]
		public TransportCollection Transports
		{
			get
			{
				if (fTransports == null)
				{
					fTransports = new TransportCollection(this);
					fTransports.Load();
					RegisterEditableChildObject(fTransports);
				}

				return fTransports;
			}
		}
		protected TransportCollection fTransports;

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public override void Delete()
		{
			Lines.DeleteAll();
			Equipments.DeleteAll();
			ReferenceDatas.DeleteAll();
			Messages.RemoveAndDeleteAll();
			DocAddresses.RemoveAndDeleteAll();
			RequiredDocuments.RemoveAndDeleteAll();
			Transports.RemoveAndDeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				if (IsDeleted)
				{
					return base.BusinessObjectsWithRelatedEventsCore;
				}
				List<BusinessObject> objects = new List<BusinessObject>();
				objects.AddRange(ReferenceDatas);

				var jobHeader = new JobHeader.Loader(this).Load();
				if (jobHeader != null)
				{
					objects.Add(jobHeader);
				}

				return objects.ToArray();
			}
		}

		public Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						var result = Guid.Empty;
						var branch = Branch;
						if (branch == null)
						{
							result = GlbCompany.CurrentCompany.PK.ToGuid();
						}
						else if (!branch.GB_GC.IsEmpty)
						{
							result = branch.GB_GC.ToGuid();
						}
						return result;
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		public Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						var branch = Branch;
						return branch == null ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		#region Type/Code for Document

		public ZString ImporterCodeForDocument => BF_ImporterCodeType == ImporterCodeTypeList.Codes.SocialSecurity ? ZString.Empty : BF_ImporterCode;

		public ZString ConsigneeTypeAndCodeForDocument => BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.SocialSecurity ? ZString.Empty : ZString.Format("{0}: {1}", BF_ConsigneeCodeType, BF_ConsigneeCode);

		public ZString BondHolderNumberForDocument => SocialSecurityNumberValidator.IsValidSSN(BF_BondNumberOrHolder) ? ZString.Empty : BF_BondNumberOrHolder;

		#endregion

		#region Implementation

		void BuyingParty_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			if (BF_OH_Importer.IsEmpty && !BuyingParty.OrganisationPK.IsEmpty)
			{
				BF_OH_Importer = BuyingParty.OrganisationPK;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		internal class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusISFHeader header)
				: base(header)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusISFLineSchema.BL_BF, BusinessObject.PK);
				Factory.AddFetchHint(CusISFEquipSchema.BE_BF, BusinessObject.PK);
				Factory.AddFetchHint(CusISFBillSchema.BB_BF, BusinessObject.PK);
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var column in columns)
				{
					switch (column.ColumnName)
					{
						case CusISFHeader.Schema.BF_ImporterFullName:
							Factory.AddFetchHint(CusISFBillSchema.BB_BF, BusinessObject.PK);
							break;
						case CusISFHeader.Schema.CustomAttribute1:
						case CusISFHeader.Schema.CustomAttribute2:
							Factory.AddFetchHint(GenCustomAddOnValueSchema.XV_ParentID, BusinessObject.PK);
							break;
					}
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (humanReadableNameCoreCached == null)
				{
					humanReadableNameCoreCached = new CachedProperty<ZString>(Factory, delegate
					{
						ZString result = BF_JobReference;
						OrgHeader importer = Importer;
						if (importer != null || !BF_OceanBill.IsEmpty || !BF_HouseBill.IsEmpty)
						{
							ZStringBuilder builder = new ZStringBuilder();
							if (importer != null)
							{
								builder.Append("Importer='" + importer.OH_Code + "'");
							}
							if (!BF_OceanBill.IsEmpty)
							{
								builder.Append("OceanBill='" + BF_OceanBill + "'");
							}
							else if (!BF_HouseBill.IsEmpty)
							{
								builder.Append("HouseBill='" + BF_HouseBill + "'");
							}
							result += " (" + builder.ToStringWithDelimiterBetweenAppends(", ") + ")";
						}
						return result;
					});
				}
				return humanReadableNameCoreCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableNameCoreCached;

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				if (humanReadableShortcutNameCached == null)
				{
					humanReadableShortcutNameCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = Res.GetString("7AE53CB5-1D44-404E-9E1F-3BDB79885F79", "ISF - {0}", BF_JobReference);
						var importer = Importer;
						if (importer != null)
						{
							result += Res.GetString("562137B0-12F9-43B2-9399-1AAACE16A509", " - {0}", importer.OH_FullName);
						}
						return result;
					});
				}
				return humanReadableShortcutNameCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableShortcutNameCached;

		CusISFBill GetFirstElementFromReferences(ZString type)
		{
			return ReferenceDatas.GetFirstMatchingType(type);
		}

		void LocateAndChangeElement(ZString type, ZPropertyInfo info, ZString newValue)
		{
			ZString oldValue = (ZString)info.Value;
			ZQuery query = new ZQuery(CusISFBillSchema.BB_BillType, type);
			query.AddToFilter(CusISFBillSchema.BB_BillNum, oldValue);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			List<CusISFBill> references = new List<CusISFBill>(ReferenceDatas.Find(query));

			bool shouldDelete = newValue.IsEmpty;
			//add one if none exist
			if (references.Count == 0)
			{
				if (!shouldDelete)
				{
					CusISFBill reference = ReferenceDatas.AddNew();
					reference.BB_BillType = type;
					reference.BB_BillNum = newValue;
				}
			}
			else
			{
				//update all matching
				foreach (CusISFBill reference in references)
				{
					if (shouldDelete)
					{
						reference.Delete();
					}
					else
					{
						reference.BB_BillNum = newValue;
					}
				}
			}
			info.RefreshBinding(oldValue);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BF_GB = GlbBranch.CurrentBranch.PK;
			BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
			BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			BF_EntryType = SubmissionTypeList.Codes.ISF10;
			BF_SendEquipment = YesNoDefaultList.Codes.Default;
			BF_LineMergeStyle = MergeStyleList.Codes.Default;
			BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
		}

		void UpdateConsingeeDetailsFromImporter()
		{
			if (Lookups.ConsigneeCodeTypes.ContainsCode(BF_ImporterCodeType))
			{
				BF_ConsigneeCodeType = BF_ImporterCodeType;
				BF_ConsigneeCode = BF_ImporterCode;
				UpdateConsigneePassportDetailsFromImporterIfNeeded();
			}
		}

		void UpdateConsigneePassportDetailsFromImporterIfNeeded()
		{
			if (BF_ConsigneeCodeType == BF_ImporterCodeType && BF_ConsigneeCode == BF_ImporterCode && BF_ConsigneeCodeType == ConsigneeCodeTypeList.Codes.Passport)
			{
				BF_ConsigneeCountryOfIssue = BF_CountryOfIssue;
				BF_ConsigneeDateOfBirth = BF_DateOfBirth;
			}
		}

		void UpdateBondHolderIfNeeded()
		{
			if (IsBondDataRequired && (BF_ImporterCodeType == ImporterCodeTypeList.Codes.IRS || BF_ImporterCodeType == ImporterCodeTypeList.Codes.CBPAssignedNumber || BF_ImporterCodeType == ImporterCodeTypeList.Codes.SocialSecurity))
			{
				BF_BondNumberOrHolder = BF_ImporterCode;
			}
		}

		void UpdateBF_OH_ImporterIfNeeded()
		{
			if (!updatingImporterCodeDetailsInProgress && !BF_ImporterCode.IsEmpty && !BF_ImporterCodeType.IsEmpty)
			{
				try
				{
					updatingImporterCodeDetailsInProgress = true;

					ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, BF_ImporterCodeType);
					query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, BF_ImporterCode);
					var countryCode = BF_ImporterCodeType == ImporterCodeTypeList.Codes.Passport ? BF_CountryOfIssue : (ZString)Core.Constants.CountryCodes.UnitedStates;
					if (!countryCode.IsEmpty)
					{
						query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
					}
					OrgCusCode cusCode = Factory.LoadTop1<OrgCusCode>(query);
					if (cusCode != null)
					{
						BF_OH_Importer = cusCode.OK_OH;
					}
				}
				finally
				{
					updatingImporterCodeDetailsInProgress = false;
				}
			}
		}

		void UpdateBondDetailsFromImporterIfNeeded()
		{
			if (IsBondDataRequired)
			{
				OrgHeaderWrapper importer = OrgHeaderWrapper.New(Importer);
				if (importer != null && importer.BondDetails.Count > 0)
				{
					ZDateTime today = ZDateTime.Today;
					US.Business.CusBondDetail bondDetail = importer.BondDetails.GetActiveBondDetailDataFor(new List<ZString>(new ZString[] { ActivityCodeList.Codes._16 }), ZString.Empty, today) ?? importer.BondDetails.GetActiveBondDetailDataFor(ISFBondActivityCodeList.GetEquivalentDeclarationActiveCodeList(), ZString.Empty, today);
					if (bondDetail != null)
					{
						BF_BondActivityCode = ISFBondActivityCodeList.GetCode(bondDetail.PW_ActivityCode);
						BF_BondType = bondDetail.PW_BondType;
						if (IsBond16SingleTransaction)
						{
							BF_SuretyCode = bondDetail.PW_SuretyCode;
						}
					}
				}
				else
				{
					BF_BondActivityCode = ZString.Empty;
					BF_BondType = ZString.Empty;
					BF_SuretyCode = ZString.Empty;
					BF_BondReferenceNumber = ZString.Empty;
				}
			}
		}

		bool updatingImporterCodeDetailsInProgress;
		void UpdateImporterCodeDetailsIfNeeded()
		{
			if (!updatingImporterCodeDetailsInProgress)
			{
				try
				{
					updatingImporterCodeDetailsInProgress = true;

					OrgHeader importer = Importer;
					if (importer != null)
					{
						OrgCusCode cusCode = importer.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.CodeTypes.PassportID);
						if (cusCode == null)
						{
							var cusCodes = importer.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.PassportID);
							if (cusCodes.Length == 1)
							{
								cusCode = cusCodes[0];
							}
						}
						if (cusCode != null)
						{
							BF_ImporterCodeType = cusCode.OK_CodeType;
							BF_ImporterCode = (cusCode.OK_CustomsRegNo.Length <= CusISFHeader.Schema.BF_ImporterCodeMaxLength) ?
								cusCode.OK_CustomsRegNo : new ZString("???");
							if (BF_ImporterCodeType == OrgCusCode.CodeTypes.PassportID)
							{
								BF_CountryOfIssue = cusCode.OK_RN_NKCodeCountry;
							}
						}
					}
				}
				finally
				{
					updatingImporterCodeDetailsInProgress = false;
				}
			}
		}

		internal StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs)); }
		}
		StatusLogManager fLogManager;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable]
		public ISFDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new ISFDocAddressDependentCollection(this);
					fDocAddresses.Load();
					fDocAddresses.Sort(ISFDocAddress.Schema.E2_AddressSequence);
					RegisterEditableChildObject(fDocAddresses);
					HookCountryCodeChangedEventHandler();
				}
				return fDocAddresses;
			}
		}
		internal ISFDocAddressDependentCollection fDocAddresses;

#if DEBUG
		public void ResetDocAddresses()
		{
			fDocAddresses = null;
		}
#endif
		void HookCountryCodeChangedEventHandler()
		{
			foreach (ISFDocAddress address in fDocAddresses)
			{
				if (address.DocAddressType == DocAddressType.Manufacturer)
				{
					address.CountryCodeChanged += Manufacturer_CountryCodeChanged;
				}
			}
		}

		public void Manufacturer_CountryCodeChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			var manufAddress = (ISFDocAddress)sender;
			foreach (var line in Lines)
			{
				if (line.BL_ManufacturerDocAddressPK == manufAddress.PK)
				{
					line.SetCountryOfOriginFromManufacturer();
				}
			}
		}

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		ISFDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Manufacturer:
					return ISFDocAddressRequirementProvider.ManufacturerDocAddressRequirement;
				case DocAddressType.ConsigneeAddress:
					return ISFDocAddressRequirementProvider.ConsigneeDocAddressRequirement;
				case DocAddressType.ShipToParty:
					return ISFDocAddressRequirementProvider.ShipToPartyDocAddressRequirement;
				case DocAddressType.SellingParty:
				case DocAddressType.BuyingParty:
					return ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(addressType, SubmissionTypeList.Codes.ISF10, true);
				case DocAddressType.ScheduledContainerStuffingLocation:
				case DocAddressType.Consolidator:
					return ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(addressType, SubmissionTypeList.Codes.ISF10, false);
				case DocAddressType.BookingPartyDocumentaryAddress:
					return ISFDocAddressRequirementProvider.GetOtherPartyISFDocAddressRequirement(addressType, SubmissionTypeList.Codes.ISF5, false);
			}

			return null;
		}

		public ISFDocAddressRequirementProvider ISFDocAddressRequirementProvider
		{
			get
			{
				return Factory.GetCachedValue("ISFDocAddressRequirementProvider", delegate
				{
					return new ISFDocAddressRequirementProvider(Factory);
				});
			}
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ConsigneeAddress,
					DocAddressType.Manufacturer,
					DocAddressType.SellingParty,
					DocAddressType.BuyingParty,
					DocAddressType.ShipToParty,
					DocAddressType.ScheduledContainerStuffingLocation,
					DocAddressType.Consolidator,
					DocAddressType.BookingPartyDocumentaryAddress
				};
			}
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return !(docAddress == shipToParty ||
				docAddress == sellingParty ||
				docAddress == buyingParty ||
				docAddress == consolidator ||
				docAddress == stuffingLocation ||
				docAddress == bookingParty);
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			PopulateJobReferenceIfNeeded();
			base.OnSaving();
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				RecoverFromUnsuccessfulSave();
			}
			base.OnSaved(saveSucceeded);
		}

		void RecoverFromUnsuccessfulSave()
		{
			if (IsInDatabase)
			{
				BF_CustomsStatus = (ZString)BF_CustomsStatusInfo.OriginalValue;
			}
			else
			{
				BF_JobReference = ZString.Empty;
				BF_CustomsStatus = ZString.Empty;
			}

			List<EDIMessage> messages = new List<EDIMessage>();
			Messages.CopyToList(messages);

			foreach (var message in messages)
			{
				if (!message.IsInDatabase && !message.IsDeleted)
				{
					message.Delete();
				}
			}
		}

		public void PopulateJobReferenceIfNeeded()
		{
			PopulateNumberPropertyIfRequired(BF_JobReferenceInfo, x => GetNewJobReference(x));
		}

		ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			ZGuid departmentPK = GlbDepartment.CurrentDepartment.PK;
			NumberGeneratorTarget isfTarget = new ImporterSecurityFilingNumberGeneratorTarget();

			NumberGenerator generator = new NumberGenerator();
			generator.Factory = factory;
			generator.Context = new NumberGeneratorContext(RegistryCompanyPK, RegistryBranchPK, departmentPK);
			generator.BaseFountain = Env.NumberFountains.ImporterSecurityFilingReference;
			generator.FountainGetter = Env.NumberFountains.GetImporterSecurityFilingReferenceGeneratorFountain;
			generator.PrimaryTarget = isfTarget;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(this));

			generator.Generate();
			generator.EnforceMaxLengths();
			return isfTarget.Value.ToUpper();
		}

		void AddRequiredDocuments()
		{
			RequiredDocuments.AddCountryRequiredDocuments(RefCountryRequiredDocumentSchema.RD_OnBrokerage, Core.Constants.TransportModes.Sea, "", JobRequiredDocumentDependentCollection.DirectionFilterType.Import, "", BF_RL_NKPlaceOfDelivery);
		}

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			var result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}
			return base.CanCancel();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (BF_IsCancelled)
			{
				UpdateReadOnlyForWhenCancelled();
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				this.SetCountedReadOnlyIncludingChildren(BF_IsCancelled);
			}
		}

		#endregion

		#region IImporterSecurityFiling Members

		TariffDataCollection.MergeStyle GetMergetStyle()
		{
			TariffDataCollection.MergeStyle result = TariffDataCollection.MergeStyle.NotMerge;
			switch (BF_LineMergeStyle)
			{
				case MergeStyleList.Codes.Merge:
					result = TariffDataCollection.MergeStyle.Merge;
					break;
				case MergeStyleList.Codes.Default:
					if (ISFRegistry.Instance.ImporterSecurityFilingShouldMergeLine.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
					{
						result = TariffDataCollection.MergeStyle.Merge;
					}
					break;
			}
			return result;
		}

		#region ISF 10
		IEnumerable<IManufacturerData> IImporterSecurityFiling.ManufacturerData
		{
			get { return ManufacturerDataGenerator.Generate(Lines, GetMergetStyle()); }
		}
		#endregion

		void IImporterSecurityFiling.UpdateBill(ZString billNumber, ZString status, ZDateTime statusDate)
		{
			ReferenceDatas.UpdateBill(billNumber, status, statusDate);
		}

		void IImporterSecurityFiling.UpdateAcceptedDate(ZDateTime acceptedDate)
		{
			if (BF_FirstAcceptedDate.IsEmpty)
			{
				BF_FirstAcceptedDate = acceptedDate;
			}
			BF_LastAcceptedDate = acceptedDate;
		}

		ZGuid IImporterSecurityFiling.PK
		{
			get { return PK; }
		}

		ZString IImporterSecurityFiling.JobReference
		{
			get { return BF_JobReference; }
		}

		ZString IImporterSecurityFiling.ConsigneeFullName
		{
			get { return ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters(BF_ConsigneeFullName); }
		}

		ZString IImporterSecurityFiling.ConsigneeNumber
		{
			get { return BF_ConsigneeCode; }
		}

		ZString IImporterSecurityFiling.ConsigneeNumberQualifier
		{
			get { return EntityIdentifierQualifierList.GetCodeFromCusCodeType(BF_ConsigneeCodeType); }
		}

		ZString IImporterSecurityFiling.ConsigneePassportCountryOfIssue
		{
			get { return BF_ConsigneeCountryOfIssue; }
		}

		ZDate IImporterSecurityFiling.ConsigneePassportDateOfBirth
		{
			get { return BF_ConsigneeDateOfBirth.Date; }
		}

		IEnumerable<IContainerData> IImporterSecurityFiling.ContainerData
		{
			get
			{
				bool sendEquipment = true;
				switch (BF_SendEquipment)
				{
					case YesNoDefaultList.Codes.No:
						sendEquipment = false;
						break;
					case YesNoDefaultList.Codes.Default:
						sendEquipment = ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
						break;
				}
				if (sendEquipment)
				{
					foreach (CusISFEquip container in Equipments)
					{
						yield return container;
					}
				}
			}
		}

		ZString IImporterSecurityFiling.ImporterFullName
		{
			get { return ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters(BF_ImporterFullName); }
		}

		ZString IImporterSecurityFiling.CountryOfIssuance
		{
			get { return BF_CountryOfIssue; }
		}

		ZDate IImporterSecurityFiling.DateOfBirth
		{
			get { return BF_DateOfBirth.Date; }
		}

		ZString IImporterSecurityFiling.IORNumber
		{
			get { return BF_ImporterCode; }
		}

		ZString IImporterSecurityFiling.IORNumberQualifier
		{
			get { return EntityIdentifierQualifierList.GetCodeFromCusCodeType(BF_ImporterCodeType); }
		}

		ZBool IImporterSecurityFiling.ISFBondIndicator
		{
			get { return new ZBool(BF_BondActivityCode == ISFBondActivityCodeList.Codes.ImporterOrBroker && BF_BondType == ImporterBondTypeList.Codes.SingleTransactionBond); }
		}

		ZString IImporterSecurityFiling.ISFImporterBondHolder
		{
			get { return BF_BondNumberOrHolder; }
		}

		ZString IImporterSecurityFiling.ISFBondActivityCode
		{
			get { return BF_BondActivityCode; }
		}

		ZString IImporterSecurityFiling.ISFBondType
		{
			get { return BF_BondType; }
		}

		ZString IImporterSecurityFiling.ModeOfTransportation
		{
			get { return BF_TransportMode; }
		}

		IEnumerable<IReferenceData> IImporterSecurityFiling.ReferenceData
		{
			get
			{
				foreach (CusISFBill referenceData in ReferenceDatas)
				{
					IReferenceData iReferenceData = referenceData;
					if (!iReferenceData.CodeQualifier.IsEmpty)
					{
						yield return iReferenceData;
					}
				}
			}
		}

		IEnumerable<IISFDocAddress> IImporterSecurityFiling.RelatedOrganizationData
		{
			get
			{
				foreach (ISFDocAddress docAddress in DocAddresses)
				{
					if (docAddress.DocAddressType != DocAddressType.Manufacturer && !docAddress.IsEmpty)
					{
						yield return new SanitizedISFDocAddressWrapper(docAddress);
					}
				}
			}
		}

		ZString IImporterSecurityFiling.SCAC
		{
			get { return BF_SCAC; }
		}

		ZString IImporterSecurityFiling.SFSubmissionType
		{
			get { return BF_EntryType; }
		}

		ZString IImporterSecurityFiling.SFTransactionNumber
		{
			get { return BF_CustomsReference; }
			set { BF_CustomsReference = value; }
		}

		IEnumerable<IShipmentReferenceID> IImporterSecurityFiling.ShipmentIDs
		{
			get
			{
				foreach (CusISFBill referenceData in ReferenceDatas)
				{
					IShipmentReferenceID shipmentReferenceID = referenceData;
					if (!shipmentReferenceID.CodeQualifier.IsEmpty)
					{
						yield return shipmentReferenceID;
					}
				}
			}
		}

		ZString IImporterSecurityFiling.ShipmentTypeCode
		{
			get { return BF_ShipmentType; }
		}

		ZString IImporterSecurityFiling.ActionReasonCode
		{
			get { return BF_ActionReasonCode; }
		}

		//SF13
		ZString IImporterSecurityFiling.ShipmentSubType
		{
			get { return BF_ShipmentSubType; }
		}

		ZDecimal IImporterSecurityFiling.EstimatedValue
		{
			get { return BF_EstimatedValue; }
		}

		ZDecimal IImporterSecurityFiling.EstimatedQuantity
		{
			get { return new ZDecimal(BF_EstimatedQuantity); }
		}

		ZString IImporterSecurityFiling.UnitOfMeasure
		{
			get { return BF_EstimatedQuantityUQ; }
		}

		ZDecimal IImporterSecurityFiling.EstimatedWeight
		{
			get { return WeightCalculator.Calculate(EstimatedWeight); }
		}

		ZString IImporterSecurityFiling.WeightQualifier
		{
			get { return WeightCalculator.CalculateUQ(BF_EstimatedWeightUQ); }
		}

		#region ISF 5

		ZString IImporterSecurityFiling.CodeQualifier1
		{
			get { return PortLocationTypeList.Codes.UNLOCO; }
		}

		ZString IImporterSecurityFiling.CodeQualifier2
		{
			get { return PortLocationTypeList.Codes.UNLOCO; }
		}

		ZString IImporterSecurityFiling.ForeignPortOfUnlading
		{
			get { return BF_RL_NKPortOfUnload; }
		}

		ZString IImporterSecurityFiling.PlaceOfDelivery
		{
			get { return BF_RL_NKPlaceOfDelivery; }
		}

		IEnumerable<ITariffData> IImporterSecurityFiling.Tariffs
		{
			get
			{
				if (iImporterSecurityFilingTariffsCached == null)
				{
					iImporterSecurityFilingTariffsCached = new CachedProperty<IEnumerable<ITariffData>>(Factory, delegate
					{
						TariffDataCollection result = new TariffDataCollection(GetMergetStyle());
						foreach (CusISFLine line in Lines)
						{
							result.Add(line.BL_RN_NKGoodsOrigin, line.HarmonisedNumToReportToCustoms);
						}
						return new TypedEnumerable<ITariffData>(result);
					});
				}
				return iImporterSecurityFilingTariffsCached.Value;
			}
		}
		CachedProperty<IEnumerable<ITariffData>> iImporterSecurityFilingTariffsCached;

		#endregion

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.ImporterSecurityFiling; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get { return Branch; }
		}

		GlbBranch IBranchProvider.Branch
		{
			get { return Branch; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return BF_CustomsStatus; }
			set { BF_CustomsStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return this; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return BF_JobReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Logs; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		CusISFHeaderInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CusISFHeaderInvoicingSupporter(this)); }
		}

		#endregion

		#region IJobHeaderParent Members

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Factory; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return PK; }
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		string IJobHeaderParentCore.TableName
		{
			get { return Schema.TableName; }
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return BF_JobReference; }
		}

		#endregion

		#region ITemplateCopyable Members

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return new CusISFHeaderDeepCloneStrategy(this).Clone();
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new CusISFHeaderDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new CusISFHeaderDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IHaveRequiredDocuments Members

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return null; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return BF_HouseBill; }
		}

		Logs IHaveRequiredDocuments.Logs
		{
			get { return Logs; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return BF_MasterBill; }
		}

		[ChildEditable]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}
				return requiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection requiredDocuments;

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return TablePrefix; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return BF_JobReference; }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
			if (!IsInDatabase && !requiredDocumentsAdded)
			{
				AddRequiredDocuments();
				requiredDocumentsAdded = true;
			}
		}

		bool requiredDocumentsAdded;

		#endregion

		#region IRoutingSupport Members

		RoutingCollection IRoutingSupport.TransportsIncludingRelated
		{
			get { return transportsIncludingRelated ?? (transportsIncludingRelated = new RoutingCollection(this)); }
		}
		RoutingCollection transportsIncludingRelated;

		TransportCollection IRoutingSupport.Transports
		{
			get { return Transports; }
		}

		ZString IRoutingSupport.TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		string IRoutingSupport.AdditionalETAUpdateMsg
		{
			get { return null; }
		}

		string IRoutingSupport.AdditionalETDUpdateMsg
		{
			get { return null; }
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new CusISFHeaderTransportSupporter(this); }
		}

		TransportCollection ITransportParent.Transports
		{
			get { return Transports; }
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Core.Constants.TransportParentTypes.ImporterSecurityFiling; }
		}

		#endregion

		#region ITransportChangeNotifier Members

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
		}

		#endregion

		#region IWorkflowProviderCore Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.ImporterSecurityFiling.Code; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CusISFHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		CusISFHeaderProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowInformationProvider;
		}

		WorkflowInformationProvider WorkflowInformationProvider
		{
			get
			{
				if (workflowInformationProvider == null)
				{
					workflowInformationProvider = new WorkflowInformationProvider(new ZGuid[] { Branch.Company.PK });
					workflowInformationProvider.Destination = ZString.Empty;
					workflowInformationProvider.Origin = ZString.Empty;
					workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.ISF;
				}
				return workflowInformationProvider;
			}
		}

		WorkflowInformationProvider workflowInformationProvider;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			JobHeader job = new JobHeader.Loader(this).Load();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_GB, job != null ? job.JH_GB : BF_GB, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, BF_RL_NKPlaceOfDelivery, BF_RL_NKPlaceOfDelivery.Substring(0, 2), ZString.Empty);

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			List<IZType> result = new List<IZType>();
			if (BF_OH_Importer.IsValid)
			{
				result.Add(BF_OH_Importer);
			}

			JobHeader job = new JobHeader.Loader(this).Load();
			if (job != null)
			{
				result.Add(job.LocalChargesPK);
			}
			result.Add(ZGuid.Empty);
			return result.ToArray();
		}

		#endregion

		#region IBillGenerationSupport Members

		OrgHeader IBillGenerationSupport.CarrierPrincipal
		{
			get { return null; }
		}

		ZString IBillGenerationSupport.TranshipmentIndicator
		{
			get { return ""; }
		}

		RefUNLOCO IBillGenerationSupport.Destination
		{
			get { return null; }
		}

		BusinessObjectFactory IBillGenerationSupport.Factory
		{
			get { return Factory; }
		}

		RefUNLOCO IBillGenerationSupport.Origin
		{
			get { return null; }
		}

		ZString IBillGenerationSupport.TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		ZString IBillGenerationSupport.ServiceLevel
		{
			get { return ""; }
		}

		RefUNLOCO IBillGenerationSupport.Load
		{
			get { return null; }
		}

		RefUNLOCO IBillGenerationSupport.Discharge
		{
			get { return null; }
		}

		#endregion

		#region IMessageFailStatusManager Members

		bool IMessageFailStatusManager.IsMessageTypeSupported(ZString messageType)
		{
			bool result = false;
			switch (messageType)
			{
				case ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling:
					result = true;
					break;
			}
			return result;
		}

		void IMessageFailStatusManager.SetFailStatus(MQEDIMessage message)
		{
			if (message.EM_MessageType == ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling)
			{
				new ImporterSecurityFilingMessageStatusCalculator(this).CalculateStatus(message, ABIResponseStatus.Rejected);
			}
		}

		#endregion

		#region IImportExport members

		public Directions JobDirection
		{
			get
			{
				if (InvoicingSupporter.IsImport)
				{
					return Directions.Import;
				}

				if (InvoicingSupporter.IsExport)
				{
					return Directions.Export;
				}

				if (InvoicingSupporter.IsDomestic)
				{
					return Directions.Domestic;
				}

				return Directions.Unknown;
			}
		}

		#endregion

		#region IJobDocAddressOverrideSupporter Members

		Type IJobDocAddressOverrideSupporter.ZDocAddressControlType
		{
			get { return ObjectFactory.GetType<IISFDocAddressControl>(); }
		}

		JobDocAddressCollectionForPlugin IJobDocAddressOverrideSupporter.GetJobDocAddressCollectionForPlugin(BusinessObjectFactory factory)
		{
			return new ISFDocAddressCollectionForPlugin(new ISFDocAddressCollection(factory));
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region IUSDISHost Members
		IUSDISDefaultValues IUSDISHost.ValueProvider
		{
			get { return new DIS.CusISFHeaderWrapper(this); }
		}

		ZString IUSDISHost.MessageSendingWarning
		{
			get { return ZString.Empty; }
		}

		ZString IUSDISHost.MessageSendingError
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				if (BF_CustomsReference.IsEmpty)
				{
					builder.Append(NoCustomsRefAvailable);
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		IEnumerable<ZString> IUSDISHost.FormGroups
		{
			get { return Array.Empty<ZString>(); }
		}

		IEnumerable<ZString> IDISHost.ErrorMessages
		{
			get { return new List<ZString>(); }
		}

		internal const string NoCustomsRefAvailable = "There is no Customs Reference Number assigned for this ISF job. Please send messages to Customs and get it accepted first.";

		#endregion

		#region IDISHost Members
		bool IDISHost.NeedToDoPreFormAction()
		{
			return false;
		}

		bool IDISHost.DoPreFormAction()
		{
			return false;
		}

		ZString IDISHost.ImporterName
		{
			get { return BF_ImporterFullName; }
		}

		IEnumerable<string> IDISHost.ApplicationCodes
		{
			get { yield return Core.Constants.Customs.DocumentImageSystemIDs.US_DIS; }
		}

		ZBool IDISHost.ShowDISFeatures
		{
			get { return true; }
		}

		ZGuid IDISHost.BranchPK
		{
			get { return Branch != null ? Branch.PK : ZGuid.Empty; }
		}

		ZGuid IDISHost.CompanyPK
		{
			get { return Branch != null ? Branch.GB_GC : ZGuid.Empty; }
		}

		IDISHost IDISHostProvider.DISHost
		{
			get { return this; }
		}

		ZString IDISHost.JobNumber
		{
			get { return BF_JobReference; }
		}

		ZString IDISHost.HumanReadable => "DIS";

		ZBool IDISHost.DISEditable => Environment.Env.Security.CustomsDISEdit.IsAllowed;

		IControllerIDProvider IDISHost.ControllerIDProvider
		{
			get { return this; }
		}

		IHaveRequiredDocuments IDISHost.RequiredDocumentsProvider
		{
			get { return this; }
		}

		event EventHandler IDISHost.DISFeatureVisibilityChanged
		{
			add { }
			remove { }
		}

		IEnumerable<IeDoc> IDISHost.EDocs
		{
			get
			{
				var docManager = ((IDocManagerSupport)this).DocManagerInfo;
				return docManager.GetRelatedEDocsView();
			}
		}

		Shared.IDISReferenceNumberFountainStrategy IDISHost.DISReferenceNumberFountainStrategy => null;

		#endregion

		#region IValidateForCustomsMessagingSupporter Members

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging => true;

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction) => this;

		#endregion

		#region IRelatedJob

		ZString IRelatedJob.JobNumber => BF_JobReference.IsEmpty ? new ZString("New ISF Header") : BF_JobReference;
		ZString IRelatedJob.JobDescription => HumanReadableName;
		ZString IRelatedJob.JobStatus => BF_CustomsStatus;

		#endregion

		#region IBaseAutoSendingMessageSupporter

		CargoWise.EntityFramework.IProcessor IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
		{
			return new Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor(this, Enterprise.Customs.Business.CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);
		}

		#endregion

		#region ICusISFHeader

		CargoWise.EntityFramework.IProcessor ICusISFAutoSendingMessageSupporter.CreateISFMessageWorkflowTriggerProcessor()
		{
			return new ISFMessageWorkflowTriggerProcessor(this);
		}

		IBusinessObjectCollection<IUSISFDocAddress> ICusISFHeader.DocAddresses
		{
			get { return DocAddresses; }
		}

		ICusISFBillCollection<ICusISFBill> ICusISFHeader.ReferenceDatas
		{
			get { return ReferenceDatas; }
		}

		ICusISFLineCollection<ICusISFLine> ICusISFHeader.Lines
		{
			get { return Lines; }
		}

		ICusISFEquipCollection<ICusISFEquip> ICusISFHeader.Equipments
		{
			get { return Equipments; }
		}

		IUSISFDocAddress ICusISFHeader.BookingParty
		{
			get { return BookingParty; }
		}

		IUSISFDocAddress ICusISFHeader.BuyingParty
		{
			get { return BuyingParty; }
		}

		IUSISFDocAddress ICusISFHeader.Consolidator
		{
			get { return Consolidator; }
		}

		IUSISFDocAddress ICusISFHeader.MainShipToParty
		{
			get { return MainShipToParty; }
		}

		IUSISFDocAddress ICusISFHeader.SellingParty
		{
			get { return SellingParty; }
		}

		IUSISFDocAddress ICusISFHeader.StuffingLocation
		{
			get { return StuffingLocation; }
		}

		#endregion
	}

	public class CusISFHeaderInvoicingSupporter : JobInvoicingSupporter
	{
		public CusISFHeaderInvoicingSupporter(CusISFHeader parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly CusISFHeader Parent;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.ImporterSecurityFilingAuditBilling;
		}

		public override OrgHeader Consignee
		{
			get { return Parent.Importer; }
		}

		public override ZString ConsolType
		{
			get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override RefCurrency ConsolRateCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Parent.Factory, Core.Constants.CountryCodes.UnitedStates); }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.ImporterSecurityFiling; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return Parent.Importer;
		}

		public override bool EditSecurityLock
		{
			get { return false; }
		}

		public override ZString EditSecurityMessage
		{
			get { return ""; }
		}

		public override ZString HouseBillNumber
		{
			get { return Parent.BF_HouseBill; }
		}

		public override bool IsImport
		{
			get { return true; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.ImporterSecurityFilingJobInvoicing;
		}

		public override ZString MasterBillNumber
		{
			get { return Parent.BF_MasterBill; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return Parent.Branch; }
		}

		public override ZString TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}
	}
}
