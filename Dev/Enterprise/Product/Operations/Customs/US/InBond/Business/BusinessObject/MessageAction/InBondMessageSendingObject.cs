using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMessageSendingObject : AutoInBondMessageSendingObject, IInbondMessageSendingData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public InBondMessageSendingObject(IInBondMessagingHeader moveHeader, InBondMessageType messageType)
			: base(((WarehouseExtensions.IInBondWarehouseIntegrationSupporter)moveHeader).Factory)
		{
			this.moveHeader = moveHeader;
			this.messageType = messageType;
			isAMSHBREffective = ZZCustomsFunctionality.IsAMSHBREffective;
			SetDefaultValuesIfRequired();
		}
		readonly bool isAMSHBREffective;

		public InBondMessageSendingObject(IInBondMessagingHeader moveHeader, CusInBondBill bill, InBondMessageType messageType)
			: this(moveHeader, messageType)
		{
			this.Bill = bill;
		}

		public InBondMessageSendingObject(IInBondMessagingHeader moveHeader, CusInBondBill bill, CusInBondContainer container, InBondMessageType messageType)
			: this(moveHeader, bill, messageType)
		{
			this.Container = container;
		}

		public new class Schema : AutoInBondMessageSendingObject.Schema
		{
			public const string US_ArrivalDate = "US_ArrivalDate";
			public const string US_DestinationPortDCode = "US_DestinationPortDCode";
			public const string US_ForeignDestPortKCode = "US_ForeignDestPortKCode";
			public const string US_MonetaryValue = "US_MonetaryValue";
			public const string US_InBondCarrierID = "US_InBondCarrierID";
			public const string US_InBondEntryType = "US_InBondEntryType";
			public const string US_InBondNumber = "US_InBondNumber";
			public const string US_InBondCarrierSCAC = "US_InBondCarrierSCAC";
			public const string US_ArrivalPort = "US_ArrivalPort";
			public const string US_ArrivalFirmsCode = "US_ArrivalFirmsCode";
			public const string US_BTAIndicator = "US_BTAIndicator";
			public const string US_MessageContents = "US_MessageContents";
			public const string US_ExportDate = "US_ExportDate";
			public const string US_ExportPort = "US_ExportPort";
			public const string US_ExportConveyance = "US_ExportConveyance";
			public const string US_ExportTransportMode = "US_ExportTransportMode";
			public const string US_TOLDate = "US_TOLDate";
			public const string US_TOLCarrierCode = "US_TOLCarrierCode";
			public const string US_TOLCarrierID = "US_TOLCarrierID";
			public const string US_TOLCityName = "US_TOLCityName";
			public const string US_TOLStateCode = "US_TOLStateCode";
			public const string US_WarehouseAddressDetail = "US_WarehouseAddressDetail";

			public const string US_DiversionPortCode = "US_DiversionPortCode";
			public const string US_DiversionDate = "US_DiversionDate";
			public const string US_OA_DiversionInBondCarrier = "US_OA_DiversionInBondCarrier";
			public const string DiversionInBondCarrierOrgPK = "DiversionInBondCarrierOrgPK";
			public const string US_DiversionInBondCarrierID = "US_DiversionInBondCarrierID";
			public const string US_DiversionCarrierSCAC = "US_DiversionCarrierSCAC";
			public const string US_MasterBillNumber = "US_MasterBillNumber";
			public const string US_ContainerNumber = "US_ContainerNumber";
			public const string US_ActionCode = "US_ActionCode";
			public const string US_ActionDescription = "US_ActionDescription";
			public const string US_MasterBillIssuer = "US_MasterBillIssuer";
			public const string US_HouseBillNumber = "US_HouseBillNumber";
			public const string US_HouseBillIssuer = "US_HouseBillIssuer";
		}

		#region New Properties
		internal InBondMessageSendingHeaderObject Master { get; set; }

		void SetDefaultValuesIfRequired()
		{
			US_ShouldSend = ShouldSendByDefaultForDeparture;
			if (messageType == InBondMessageType.DiversionRequest)
			{
				US_DiversionDate = ZDateTime.Now;
				US_OA_DiversionInBondCarrier = moveHeader.InBondCarrierPK;
				US_DiversionCarrierSCAC = moveHeader.InbondCarrierSCAC;
				US_DiversionInBondCarrierID = ((IInBondQPHeader)moveHeader).InBondCarrierID;
			}
		}

		public ZString US_MessageContents
		{
			get
			{
				if (!uS_MessageContentsCached.HasValue)
				{
					var generator = new ACEInputBlockControlGenerator(moveHeader);
					generator.AddMessageBlocks(GetMessageBlocks());
					uS_MessageContentsCached = generator.Serialise(true);
				}
				return uS_MessageContentsCached.Value;
			}
		}
		ZString? uS_MessageContentsCached;

		public ZPropertyInfo US_MessageContentsInfo
		{
			get { return GetZPropertyInfo(Schema.US_MessageContents); }
		}

		void ReGenerateMessageContents()
		{
			var oldMessageContents = ZString.Empty;
			if (uS_MessageContentsCached.HasValue)
			{
				oldMessageContents = uS_MessageContentsCached.Value;
				uS_MessageContentsCached = null;
			}
			US_MessageContentsInfo.RefreshBinding(oldMessageContents);
		}

		public ZString US_InBondEntryType
		{
			get { return ((IInBondQPHeader)moveHeader).EntryType; }
		}

		public ZPropertyInfo US_InBondEntryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.US_InBondEntryType); }
		}

		public ZString US_InBondNumber
		{
			get
			{
				if (us_InBondNumber.IsEmpty)
				{
					us_InBondNumber = ((IInBondQPHeader)moveHeader).InBondNumber;
					if (!us_InBondNumber.IsEmpty)
					{
						US_InBondNumberInfo.RefreshBinding(ZString.Empty);
					}
				}
				return us_InBondNumber;
			}
		}
		ZString us_InBondNumber;

		public ZPropertyInfo US_InBondNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_InBondNumber); }
		}

		public ZString US_InBondCarrierSCAC
		{
			get { return moveHeader.ImportingCarrierSCAC; }
		}

		public ZPropertyInfo US_InBondCarrierSCACInfo
		{
			get { return GetZPropertyInfo(Schema.US_InBondCarrierSCAC); }
		}

		public ZString US_DestinationPortDCode
		{
			get { return moveHeader.USDestination; }
		}

		public ZPropertyInfo US_DestinationPortDCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_DestinationPortDCode); }
		}

		public ZString US_ForeignDestPortKCode
		{
			get { return ((IInBondQPHeader)moveHeader).ForeignDestination; }
		}

		public ZPropertyInfo US_ForeignDestPortKCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_ForeignDestPortKCode); }
		}

		public ZDecimal US_MonetaryValue
		{
			get { return ((IInBondQPHeader)moveHeader).Value; }
		}

		public ZPropertyInfo US_MonetaryValueInfo
		{
			get { return GetZPropertyInfo(Schema.US_MonetaryValue); }
		}

		public ZString US_InBondCarrierID
		{
			get { return ((IInBondQPHeader)moveHeader).InBondCarrierID; }
		}

		public ZPropertyInfo US_InBondCarrierIDInfo
		{
			get { return GetZPropertyInfo(Schema.US_InBondCarrierID); }
		}

		public ZString US_BTAIndicator
		{
			get { return moveHeader.BTAIndicator ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No; }
		}

		public ZPropertyInfo US_BTAIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.US_BTAIndicator); }
		}

		[ReadOnlyMember(nameof(BillOrContainerLevelReadOnly))]
		public ZDateTime US_ArrivalDate
		{
			get { return moveHeader.ArrivalDate; }
			set
			{
				moveHeader.ArrivalDate = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_ArrivalDate();
				}
				ReGenerateMessageContents();
				US_ArrivalDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.US_ArrivalDate); }
		}

		public void ValidateUS_ArrivalDate()
		{
			US_ArrivalDateInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelArrivalMessageType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(US_ArrivalDateInfo, US_ArrivalDateInfo.HumanReadableName);
				CusInBondMoveHeaderValidation.ValidateArrivalDate(US_ArrivalDateInfo);
			}
		}

		[List(nameof(ArrivalPortList))]
		public ZString US_ArrivalPort
		{
			get { return moveHeader.ArrivalPort; }
		}

		public ZPropertyInfo US_ArrivalPortInfo
		{
			get { return GetZPropertyInfo(Schema.US_ArrivalPort); }
		}

		public ZZRefCusCodeListCombinedCollection ArrivalPortList
		{
			get { return moveHeader.ArrivalPortList; }
		}

		[ReadOnlyMember(nameof(BillOrContainerLevelReadOnly))]
		public ZDateTime US_ExportDate
		{
			get { return moveHeader.ExportDate; }
			set
			{
				moveHeader.ExportDate = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_ExportDate();
				}
				US_ExportDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ExportDateInfo
		{
			get { return GetZPropertyInfo(Schema.US_ExportDate); }
		}

		public void ValidateUS_ExportDate()
		{
			US_ExportDateInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelExportMessageType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(US_ExportDateInfo, US_ExportDateInfo.HumanReadableName);
			}
		}

		[List(nameof(ExportPortList))]
		public ZString US_ExportPort
		{
			get { return moveHeader.ExportPort; }
		}

		public ZPropertyInfo US_ExportPortInfo
		{
			get { return GetZPropertyInfo(Schema.US_ExportPort); }
		}

		public ZZRefCusCodeListCombinedCollection ExportPortList
		{
			get { return moveHeader.ExportPortList; }
		}

		[List(nameof(ExportConveyanceList))]
		[MaxLength(35)]
		[ReadOnlyMember(nameof(BillOrContainerLevelReadOnly))]
		public ZString US_ExportConveyance
		{
			get { return moveHeader.ExportLadenOn; }
			set
			{
				moveHeader.ExportLadenOn = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_ExportConveyance();
				}
				US_ExportConveyanceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ExportConveyanceInfo
		{
			get { return GetZPropertyInfo(Schema.US_ExportConveyance); }
		}

		public void ValidateUS_ExportConveyance()
		{
			US_ExportConveyanceInfo.ClearAllNotifications();

			if (IsInBondLevelExportMessageType && US_ShouldSend)
			{
				if (!US_ExportTransportMode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(US_ExportConveyanceInfo, US_ExportConveyanceInfo.HumanReadableName);
				}
				CusInBondMoveHeaderValidation.ValidateExportLadenOn(US_ExportConveyanceInfo, US_ExportConveyance);
			}
		}

		public RefVesselCollection ExportConveyanceList
		{
			get { return moveHeader.ExportLadenOnList; }
		}

		[List(nameof(ExportTransportModeList))]
		[MaxLength(2)]
		[ReadOnlyMember(nameof(BillOrContainerLevelReadOnly))]
		public ZString US_ExportTransportMode
		{
			get { return moveHeader.ExportTransportMode; }
			set
			{
				moveHeader.ExportTransportMode = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_ExportTransportMode();
				}
				US_ExportTransportModeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ExportTransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.US_ExportTransportMode); }
		}

		public void ValidateUS_ExportTransportMode()
		{
			US_ExportTransportModeInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelExportMessageType)
			{
				if (!US_ExportConveyance.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(US_ExportTransportModeInfo, US_ExportTransportModeInfo.HumanReadableName);
				}
				CusInBondMoveHeaderValidation.ValidateExportTransportMode(US_ExportTransportModeInfo);
			}
		}

		public CodeDescriptionPairList ExportTransportModeList
		{
			get { return moveHeader.ExportTransportModeList; }
		}

		public ZDateTime US_TOLDate
		{
			get { return moveHeader.TOLDate; }
			set
			{
				moveHeader.TOLDate = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_TOLDate();
				}
				US_TOLDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_TOLDateInfo
		{
			get { return GetZPropertyInfo(Schema.US_TOLDate); }
		}

		public void ValidateUS_TOLDate()
		{
			US_TOLDateInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelTransferOfLiabilityMessageType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(US_TOLDateInfo, US_TOLDateInfo.HumanReadableName);
			}
		}

		[List(nameof(TOLCarrierCodeList))]
		[MaxLength(4)]
		public ZString US_TOLCarrierCode
		{
			get { return moveHeader.TOLCarrierCode; }
			set
			{
				moveHeader.TOLCarrierCode = value;

				if (!IsValidationSuspended)
				{
					ValidateUS_TOLCarrierCode();
				}
				US_TOLCarrierCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_TOLCarrierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_TOLCarrierCode); }
		}

		public void ValidateUS_TOLCarrierCode()
		{
			US_TOLCarrierCodeInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelTransferOfLiabilityMessageType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(US_TOLCarrierCodeInfo, US_TOLCarrierCodeInfo.HumanReadableName);
			}
		}

		public USCarrierCombinedCollection TOLCarrierCodeList
		{
			get { return moveHeader.TOLCarrierCodeList; }
		}

		[MaxLength(27)]
		public ZString US_TOLCarrierID
		{
			get { return moveHeader.TOLCarrierID; }
			set
			{
				CheckMaximumLength(US_TOLCarrierIDInfo, value);
				moveHeader.TOLCarrierID = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_TOLCarrierID();
				}
				US_TOLCarrierIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_TOLCarrierIDInfo
		{
			get { return GetZPropertyInfo(Schema.US_TOLCarrierID); }
		}

		public void ValidateUS_TOLCarrierID()
		{
			US_TOLCarrierIDInfo.ClearAllNotifications();
			if (US_ShouldSend && IsInBondLevelTransferOfLiabilityMessageType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(US_TOLCarrierIDInfo, US_TOLCarrierIDInfo.HumanReadableName);
				CusInBondMoveHeaderValidation.ValidateCarrierID(US_TOLCarrierIDInfo, US_TOLCarrierID);
			}
		}

		[MaxLength(19)]
		public ZString US_TOLCityName
		{
			get { return moveHeader.TOLCityName; }
			set
			{
				moveHeader.TOLCityName = value;

				if (!IsValidationSuspended)
				{
					ValidateUS_TOLCityName();
				}
				US_TOLCityNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_TOLCityNameInfo
		{
			get { return GetZPropertyInfo(Schema.US_TOLCityName); }
		}

		public void ValidateUS_TOLCityName()
		{
			US_TOLCityNameInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelTransferOfLiabilityMessageType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(US_TOLCityNameInfo, US_TOLCityNameInfo.HumanReadableName);
			}
		}

		[List(nameof(TOLStateCodeList))]
		[MaxLength(2)]
		public ZString US_TOLStateCode
		{
			get { return moveHeader.TOLStateCode; }
			set
			{
				moveHeader.TOLStateCode = value;

				if (!IsValidationSuspended)
				{
					ValidateUS_TOLStateCode();
				}
				CusInBondMoveHeaderValidation.ValidateTOLStateInfo(US_TOLStateCodeInfo, US_TOLCityName);
				US_TOLStateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_TOLStateCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_TOLStateCode); }
		}

		public void ValidateUS_TOLStateCode()
		{
			US_TOLStateCodeInfo.ClearAllNotifications();
			CusInBondMoveHeaderValidation.ValidateTOLStateInfo(US_TOLStateCodeInfo, US_TOLCityName);
		}

		public CodeDescriptionPairList TOLStateCodeList
		{
			get { return moveHeader.TOLStateCodeList; }
		}

		public ZString US_WarehouseAddressDetail
		{
			get { return moveHeader.WarehouseAddressDetail; }
		}

		public ZPropertyInfo US_WarehouseAddressDetailInfo
		{
			get { return GetZPropertyInfo(Schema.US_WarehouseAddressDetail); }
		}

		[List(nameof(FirmsCodeList))]
		[MaxLength(4)]
		[ReadOnlyMember(nameof(BillOrContainerLevelReadOnly))]
		public ZString US_ArrivalFirmsCode
		{
			get { return moveHeader.FIRMSCode; }
			set
			{
				moveHeader.FIRMSCode = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_ArrivalFirmsCode();
				}
				ReGenerateMessageContents();
				US_ArrivalFirmsCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ArrivalFirmsCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_ArrivalFirmsCode); }
		}

		public void ValidateUS_ArrivalFirmsCode()
		{
			US_ArrivalFirmsCodeInfo.ClearAllNotifications();

			if (US_ShouldSend && IsInBondLevelArrivalMessageType && !moveHeader.IsAir)
			{
				CusInBondMoveHeaderValidation.ValidateFIRMSCode(US_ArrivalFirmsCodeInfo, FirmsCodeList);
			}
		}

		bool IsInBondLevelArrivalMessageType => messageType == InBondMessageType.AirEntireInBondArrival || messageType == InBondMessageType.InBondLevelArrival;

		bool IsInBondLevelExportMessageType => messageType == InBondMessageType.AirEntireInBondExportation || messageType == InBondMessageType.InBondLevelExportation;

		bool IsInBondLevelTransferOfLiabilityMessageType => messageType == InBondMessageType.InBondLevelTransferOfLiability;

		bool IsBillofLadingLevelMessageType => messageType == InBondMessageType.BillOfLadingLevelArrival || messageType == InBondMessageType.BillOfLadingLevelExportation
			|| messageType == InBondMessageType.AirBillDelete || messageType == InBondMessageType.DepartureBillDelete;

		bool IsContainerLevelMessageType => messageType == InBondMessageType.ContainerLevelArrival || messageType == InBondMessageType.ContainerLevelExportation;

		public bool BillOrContainerLevelReadOnly => IsBillofLadingLevelMessageType || IsContainerLevelMessageType;

		public ZZRefCusCodeListCombinedCollection FirmsCodeList
		{
			get
			{
				return UniversalReferenceDataHelper.GetCachedRefCusCodeListCombinedCollection(Factory,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
					true,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode,
					US_ArrivalPort);
			}
		}

		#region Diversion Request

		#region DiversionPortCode

		[List(nameof(RegionDistrictPorts))]
		[MaxLength(4)]
		public ZString US_DiversionPortCode
		{
			get { return diversionPortCode; }
			set
			{
				if (diversionPortCode != value)
				{
					CheckMaximumLength(US_DiversionPortCodeInfo, value);
					diversionPortCode = value;
					if (!IsValidationSuspended)
					{
						ValidateUS_DiversionPortCode();
					}
					US_DiversionPortCodeInfo.RefreshBinding();
					ReGenerateMessageContents();
				}
			}
		}
		ZString diversionPortCode;

		public ZPropertyInfo US_DiversionPortCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_DiversionPortCode, "Prot Code"); }
		}

		public void ValidateUS_DiversionPortCode()
		{
			US_DiversionPortCodeInfo.ClearAllNotifications();

			if (US_ShouldSend && messageType == InBondMessageType.DiversionRequest)
			{
				if (US_DiversionPortCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(US_DiversionPortCodeInfo, US_DiversionPortCodeInfo.HumanReadableName);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(US_DiversionPortCodeInfo);

					if (US_DiversionPortCode == USPortOfDestFromOriginalMessage)
					{
						US_DiversionPortCodeInfo.AddMessageError(ZString.Format("Port Code must be different than original US Port of Destination ({0})", USPortOfDestFromOriginalMessage));
					}
				}
			}
		}

		ZString USPortOfDestFromOriginalMessage
		{
			get
			{
				var result = ZString.Empty;
				var lastOutgoingMessage = (MQEDIMessage)moveHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.InbondTransaction, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, EM_MessageSubTypeList.Codes.InBondDepartureOriginal);
				if (lastOutgoingMessage != null)
				{
					var inbqp10 = lastOutgoingMessage.MessageBlock.MessageBlocks.OfType<IINBQP10>().FirstOrDefault();
					if (inbqp10 != null)
					{
						result = inbqp10.USPortOfDestination;
					}
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return moveHeader.ArrivalPortList; }
		}

		#endregion

		#region DiversionDate

		public ZDateTime US_DiversionDate
		{
			get { return diversionDate; }
			set
			{
				diversionDate = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_DiversionDate();
				}
				US_DiversionDateInfo.RefreshBinding();
				ReGenerateMessageContents();
			}
		}
		ZDateTime diversionDate;

		public ZPropertyInfo US_DiversionDateInfo
		{
			get { return GetZPropertyInfo(Schema.US_DiversionDate, "Date"); }
		}

		public void ValidateUS_DiversionDate()
		{
			US_DiversionDateInfo.ClearAllNotifications();

			if (US_ShouldSend && messageType == InBondMessageType.DiversionRequest)
			{
				if (US_DiversionDate.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(US_DiversionDateInfo, US_DiversionDateInfo.HumanReadableName);
				}
			}
		}

		#endregion

		#region DiversionInBondCarrier

		[RelatedBusinessObject("InBondCarrier")]
		[List(nameof(InBondCarriers))]
		public ZGuid US_OA_DiversionInBondCarrier
		{
			get { return diversionInBondCarrierPK; }
			set
			{
				diversionInBondCarrierPK = value;
				if (!IsValidationSuspended)
				{
					ValidateUS_OA_DiversionInBondCarrier();
				}
				US_OA_DiversionInBondCarrierInfo.RefreshBinding();
				ReGenerateMessageContents();
			}
		}
		ZGuid diversionInBondCarrierPK;

		void ValidateUS_OA_DiversionInBondCarrier()
		{
			US_OA_DiversionInBondCarrierInfo.ClearAllNotifications();

			if (US_ShouldSend && messageType == InBondMessageType.DiversionRequest)
			{
				TypeValidation.CheckValidGuid(US_OA_DiversionInBondCarrierInfo);

				if (!US_OA_DiversionInBondCarrier.IsEmpty)
				{
					var cusInbondHeader = moveHeader.TopLevelBusinessObject as CusInBondHeader;
					if (cusInbondHeader != null)
					{
						new PowerOfAttorneyValidator().Validate(cusInbondHeader, DiversionInBondCarrierOrg, US_OA_DiversionInBondCarrierInfo);
					}
				}

				ValidationUS_DiversionInBondCarrierID();
			}
		}

		public OrgAddress InBondCarrier
		{
			get { return Factory.Load<OrgAddress>(US_OA_DiversionInBondCarrier); }
		}

		public ZPropertyInfo US_OA_DiversionInBondCarrierInfo
		{
			get { return GetZPropertyInfo(Schema.US_OA_DiversionInBondCarrier); }
		}

		public ZAddress US_OA_DiversionInBondCarrier_ZAddress
		{
			get
			{
				if (fUS_OA_DiversionInBondCarrier_ZAddress == null)
				{
					fUS_OA_DiversionInBondCarrier_ZAddress = GetNewUS_OA_DiversionInBondCarrier_ZAddress();
				}
				return fUS_OA_DiversionInBondCarrier_ZAddress;
			}
		}
		ZAddress fUS_OA_DiversionInBondCarrier_ZAddress;

		protected ZAddress GetNewUS_OA_DiversionInBondCarrier_ZAddress()
		{
			var result = new ZAddress(US_OA_DiversionInBondCarrierInfo);
			result.OnOrgChanged += new EventHandler(US_OA_DiversionInBondCarrier_OnOrgChanged);
			result.DefaultAddressType = AddressType.OFC;
			result.OrgPKValidation = (ZPropertyInfo info) => TypeValidation.CheckValidGuid(info);
			return result;
		}

		void US_OA_DiversionInBondCarrier_OnOrgChanged(object sender, EventArgs e)
		{
			CusInBondMoveHeader.DefaultInBondCarrierDetails(DiversionInBondCarrierOrg, US_DiversionCarrierSCACInfo, US_DiversionInBondCarrierIDInfo, IsCopying);
		}

		public OrgAddressCollection InBondCarriers => moveHeader.InBondCarriers;

		[List(nameof(ShippingProviders))]
		public ZGuid DiversionInBondCarrierOrgPK
		{
			get { return US_OA_DiversionInBondCarrier_ZAddress.OrgPK; }
			set
			{
				US_OA_DiversionInBondCarrier_ZAddress.OrgPK = value;
				ListValidation.ErrorIfInvalidPK(DiversionInBondCarrierOrgPKInfo);
			}
		}

		public ZPropertyInfo DiversionInBondCarrierOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DiversionInBondCarrierOrgPK, x => US_OA_DiversionInBondCarrier_ZAddress.OrgPKInfo); }
		}

		public OrgHeader DiversionInBondCarrierOrg
		{
			get { return (OrgHeader)US_OA_DiversionInBondCarrier_ZAddress.OrgHeader; }
		}

		public ShippingProviderCollection ShippingProviders => moveHeader.ShippingProviders;

		#endregion

		#region DiversionInBondCarrierID

		[MaxLength(12)]
		public ZString US_DiversionInBondCarrierID
		{
			get { return diversionInBondCarrierID; }
			set
			{
				if (diversionInBondCarrierID != value)
				{
					CheckMaximumLength(US_DiversionInBondCarrierIDInfo, value);

					diversionInBondCarrierID = value;
					if (!IsValidationSuspended)
					{
						ValidationUS_DiversionInBondCarrierID();
					}

					US_DiversionInBondCarrierIDInfo.RefreshBinding();
					ReGenerateMessageContents();
				}
			}
		}
		ZString diversionInBondCarrierID;

		public ZPropertyInfo US_DiversionInBondCarrierIDInfo
		{
			get { return GetZPropertyInfo(Schema.US_DiversionInBondCarrierID); }
		}

		void ValidationUS_DiversionInBondCarrierID()
		{
			US_DiversionInBondCarrierIDInfo.ClearAllNotifications();
			if (US_ShouldSend && messageType == InBondMessageType.DiversionRequest)
			{
				var cusInbondHeader = moveHeader.TopLevelBusinessObject as CusInBondHeader;
				ValidationHelper.ValidationInBondCarrierID(US_DiversionInBondCarrierIDInfo, US_DiversionInBondCarrierID, US_OA_DiversionInBondCarrier, cusInbondHeader);
			}
		}

		#endregion

		#region DiversionCarrierSCAC

		[List(nameof(TOLCarrierCodeList))]
		[MaxLength(4)]
		public ZString US_DiversionCarrierSCAC
		{
			get { return diversionCarrierSCAC; }
			set
			{
				if (diversionCarrierSCAC != value)
				{
					CheckMaximumLength(US_DiversionCarrierSCACInfo, value);
					diversionCarrierSCAC = value;

					if (!IsValidationSuspended)
					{
						ValidationUS_DiversionCarrierSCAC();
					}
					US_DiversionCarrierSCACInfo.RefreshBinding();
					ReGenerateMessageContents();
				}
			}
		}
		ZString diversionCarrierSCAC;

		public ZPropertyInfo US_DiversionCarrierSCACInfo
		{
			get { return GetZPropertyInfo(Schema.US_DiversionCarrierSCAC); }
		}

		void ValidationUS_DiversionCarrierSCAC()
		{
			US_DiversionCarrierSCACInfo.ClearAllNotifications();
			if (US_ShouldSend && messageType == InBondMessageType.DiversionRequest)
			{
				ListValidation.MessageErrorIfInvalidCode(US_DiversionCarrierSCACInfo, TOLCarrierCodeList);
			}
		}

		#endregion

		#region IInbondDiversionRequest Members

		ZString IInbondMessageSendingData.PortCode => US_DiversionPortCode;
		ZDateTime IInbondMessageSendingData.DiversionDateTime => US_DiversionDate;
		ZString IInbondMessageSendingData.InBondCarrierCode => US_DiversionCarrierSCAC;
		ZString IInbondMessageSendingData.BondedCarrierID => US_DiversionInBondCarrierID;

		#endregion

		#endregion

		public ZString US_MasterBillNumber
		{
			get
			{
				if (Bill != null)
				{
					if (messageType == InBondMessageType.AirBillDelete)
					{
						return Bill.B0_HouseBillNumber.IsEmpty ? Bill.B0_MasterBillNumber : ZString.Format("{0}/{1}", Bill.B0_MasterBillNumber, Bill.B0_HouseBillNumber);
					}
					return Bill.B0_MasterBillNumber;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo US_MasterBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_MasterBillNumber); }
		}

		public ZString US_MasterBillIssuer => Bill?.B0_IssuerCode ?? ZString.Empty;

		public ZPropertyInfo US_MasterBillIssuerInfo
		{
			get { return GetZPropertyInfo(Schema.US_MasterBillIssuer); }
		}

		public ZString US_HouseBillNumber => isAMSHBREffective && Bill != null && Bill.IsSea ? Bill.B0_HouseBillNumber : ZString.Empty;

		public ZPropertyInfo US_HouseBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_HouseBillNumber); }
		}

		public ZString US_HouseBillIssuer => isAMSHBREffective && Bill != null && Bill.IsSea ? Bill.B0_HouseBillIssuerCode : ZString.Empty;

		public ZPropertyInfo US_HouseBillIssuerInfo
		{
			get { return GetZPropertyInfo(Schema.US_HouseBillIssuer); }
		}

		public ZString US_ContainerNumber => Container?.BC_ContainerNum ?? ZString.Empty;

		public ZPropertyInfo US_ContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_ContainerNumber); }
		}

		public ZString US_ActionCode => GetActionCode();

		public ZPropertyInfo US_ActionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_ActionCode); }
		}

		public ZString US_ActionDescription => ActionCodeList.GetDescriptionFromCode(US_ActionCode);

		public ZPropertyInfo US_ActionDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.US_ActionDescription); }
		}
		#endregion

		[ReadOnlyMember(nameof(US_ShouldSendReadOnly))]
		public override ZBool US_ShouldSend
		{
			get { return base.US_ShouldSend; }
			set
			{
				ZBool oldValue = US_ShouldSend;
				using (GetValidationSuspender())
				{
					base.US_ShouldSend = value;
					if (!IsCopying && oldValue != US_ShouldSend)
					{
						if (messageType == InBondMessageType.DepartureAdd || messageType == InBondMessageType.DepartureAmend ||
							messageType == InBondMessageType.AirInBondAdd || messageType == InBondMessageType.AirInBondAmend || messageType == InBondMessageType.BondedWarehouseUpdate)
						{
							moveHeader.ReloadInBondNumber();
							if (US_InBondNumber.IsEmpty)
							{
								if (US_ShouldSend)
								{
									moveHeader.LockInBondNumberAllocationMutex();
								}
								else
								{
									moveHeader.UnLockInBondNumberAllocationMutex();
								}
							}
						}

						if (this.Container != null)
						{
							Container.ShouldSend = value;
							Container.Validation.ValidateAll();
						}
						if (this.Bill != null)
						{
							Bill.ShouldSend = value;
							Bill.Validation.ValidateAll();
						}
						moveHeader.ShouldSend = value;
						moveHeader.ValidateAll();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateUS_ShouldSend();
				}
			}
		}

		public override ZPropertyInfo US_ShouldSendInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.US_ShouldSend, "In-Bond Movement");
			}
		}

		public bool US_ShouldSendReadOnly
		{
			get
			{
				var result = false;
				if ((messageType == InBondMessageType.BillOfLadingLevelArrival || messageType == InBondMessageType.BillOfLadingLevelExportation) && Master != null)
				{
					result = Master.SendingObjects.Cast<InBondMessageSendingObject>().Any(x => x.PK != this.PK && x.Bill.PK == this.Bill.PK && x.US_ShouldSend);
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override void ValidateUS_ShouldSend()
		{
			base.ValidateUS_ShouldSend();

			if (US_ShouldSend)
			{
				if (messageType == InBondMessageType.DepartureAdd || messageType == InBondMessageType.DepartureAmend || messageType == InBondMessageType.BondedWarehouseUpdate)
				{
					var message = GetMessageForRequiredFieldsForBondedWarehousing();
					if (!message.IsEmpty)
					{
						US_ShouldSendInfo.AddError(message);
					}
				}

				if (!US_ShouldSendInfo.HasErrors())
				{
					if (US_InBondNumber.IsEmpty && (messageType == InBondMessageType.DepartureAdd || messageType == InBondMessageType.DepartureAmend ||
						messageType == InBondMessageType.AirInBondAdd || messageType == InBondMessageType.AirInBondAmend || messageType == InBondMessageType.BondedWarehouseUpdate)
						&& !moveHeader.InBondNumberAllocationMutexHasLock())
					{
						US_ShouldSendInfo.AddError(ValidationConstants.MoveHeader.InBondNumberAllocationIsInProgress(moveHeader.GetInBondNumberAllocationMutexLockInfo()));
					}
				}

				if (!US_ShouldSendInfo.HasErrors() && messageType != InBondMessageType.BondedWarehouseUpdate &&
					messageType != InBondMessageType.BondedWarehouseCancel && messageType != InBondMessageType.DepartureAdd &&
					messageType != InBondMessageType.AirInBondAdd && messageType != InBondMessageType.DiversionRequest && !moveHeader.IsPostDepartureMessageOnly && !moveHeader.HasClearDepartureAdd)
				{
					US_ShouldSendInfo.AddWarning(ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance(US_InBondNumber));
				}

				if (!US_ShouldSendInfo.HasErrors() && messageType == InBondMessageType.DiversionRequest && !moveHeader.IsPostDepartureMessageOnly && !moveHeader.HasClearDepartureAdd)
				{
					US_ShouldSendInfo.AddMessageError(ValidationConstants.MoveHeader.MoveHeaderMustHaveCustomsClearance(US_InBondNumber));
				}

				if (!US_ShouldSendInfo.HasErrors() && moveHeader.IsPostDepartureMessageOnly && IsPostDepartureRelatedMessageType && US_InBondNumber.IsEmpty)
				{
					US_ShouldSendInfo.AddMessageError(ValidationConstants.MoveHeader.InBondNumberIsRequired);
				}

				if (!US_ShouldSendInfo.HasErrors()
					&& !moveHeader.IsPostDepartureMessageOnly
					&& messageType != InBondMessageType.AirEntireInBondArrival
					&& messageType != InBondMessageType.AirEntireInBondExportation
					&& messageType != InBondMessageType.InBondLevelArrival
					&& messageType != InBondMessageType.InBondLevelExportation
					&& messageType != InBondMessageType.InBondLevelTransferOfLiability
					&& !moveHeader.HasAtLeastOneDetail)
				{
					US_ShouldSendInfo.AddMessageError(ValidationConstants.MoveHeader.AtLeastOneMovementDetailsIsRequired);
				}
			}
		}

		bool IsPostDepartureRelatedMessageType
		{
			get
			{
				return messageType == InBondMessageType.InBondLevelArrival || messageType == InBondMessageType.InBondLevelExportation || messageType == InBondMessageType.InBondLevelTransferOfLiability;
			}
		}

		bool IsWarehouseRelatedMessageType
		{
			get
			{
				return messageType == InBondMessageType.DepartureAdd || messageType == InBondMessageType.DepartureAmend ||
				messageType == InBondMessageType.DepartureDelete || messageType == InBondMessageType.BondedWarehouseUpdate ||
				messageType == InBondMessageType.BondedWarehouseCancel;
			}
		}

		ZString GetMessageForRequiredFieldsForBondedWarehousing()
		{
			var result = new ZStringBuilder();
			if (IsWarehouseRelatedMessageType && moveHeader.IsActive && !moveHeader.IsBondedWarehousingDisabled &&
				(moveHeader.HasWHSTransaction() || moveHeader.IsExBondAutomationEnabled))
			{
				if (moveHeader.IsWarehouseAddressOutsideOfHeaderCountry)
				{
					result.Append(moveHeader.GetWarehouseShouldBeInsideHeaderCountryMessage());
				}
				else if (ACommodityMustHaveAProduct && moveHeader.HasAtLeastOneCommodityWithoutProduct)
				{
					result.Append(ValidationConstants.BondedWarehouse.AProductIsRequiredForBondedWarehousingCommodity);
				}
				else if (!moveHeader.HasAtLeastOneCommodityWithProduct)
				{
					result.Append(ValidationConstants.BondedWarehouse.AtLeastOneCommodityWithProductIsRequired);
				}
				else if (moveHeader.IsMoveToFTZRequiredAndItIsEmpty)
				{
					result.Append(ValidationConstants.MoveHeader.MoveToFTZIndicatorIsRequired);
				}

				if (moveHeader.HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity)
				{
					result.Append(ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresAnInvoiceQuantity);
				}
				if (moveHeader.HasAtLeastOneCommodityWithProductWithoutProperEntryDetails)
				{
					result.Append(ValidationConstants.BondedWarehouse.ABondedWarehousingCommodityRequiresEntryDetails);
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		bool ACommodityMustHaveAProduct
		{
			get
			{
				return USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.Value;
			}
		}

		public void SendWhsTransaction()
		{
			switch (messageType)
			{
				case InBondMessageType.BondedWarehouseUpdate:
					SendBondedWarehouseUpdate();
					break;
				case InBondMessageType.BondedWarehouseCancel:
					SendBondedWarehouseCancel();
					break;
				default:
					throw new InvalidOperationException("SendWhsTransaction does not support messagetype: " + messageType.ToString());
			}
		}

		void SendBondedWarehouseCancel()
		{
			var result = moveHeader.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
			if (result != null)
			{
				if (result.ResultType == UniversalResult.HadErrors)
				{
					moveHeader.MessageInitiator.WarnUserAboutSomething(result.ErrorMessage, "Cannot Cancel Stock Release");
				}
				else
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					if (warehouseJob != null)
					{
						moveHeader.MessageInitiator.NotifyUserOfASuccessfulSend("Stock Release has been canceled. (WHS Order:" + warehouseJob.JobNumber + ")");
					}
				}
			}
		}

		void SendBondedWarehouseUpdate()
		{
			moveHeader.AllocateInBondNumberIfNeeded();
			var result = moveHeader.PublishShipmentForWHSOutward(true);
			if (result != null && moveHeader.MessageInitiator.IsPublishToUniversalTransactionOK(result))
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				result = moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				moveHeader.MessageInitiator.NotifyUserOfASuccessfulSend(string.Format("Stock Release has been updated.{0}", warehouseJob == null ? "" : string.Format(" (WHS Order:{0})", warehouseJob.JobNumber)));
			}
		}

		public MQEDIMessage Send()
		{
			MQEDIMessage result = GenerateMessage();
			if (result != null)
			{
				if (IsBillofLadingLevelMessageType)
				{
					Bill.Messages.Add(result);
				}
				else if (IsContainerLevelMessageType)
				{
					Container.Messages.Add(result);
				}
				else
				{
					moveHeader.Messages.Add(result);
				}
			}
			return result;
		}

		public readonly IInBondMessagingHeader moveHeader;
		public readonly InBondMessageType messageType;
		public readonly CusInBondBill Bill;
		public readonly CusInBondContainer Container;

		#region Implementation

		CodeDescriptionPairList ActionCodeList => Factory.GetCachedValue<InBondWPActionCodeList>();

		ZString GetActionCode()
		{
			switch (messageType)
			{
				case InBondMessageType.InBondLevelArrival:
					return InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination;
				case InBondMessageType.InBondLevelExportation:
					return InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort;
				case InBondMessageType.InBondLevelTransferOfLiability:
					return InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForEntireInBond;
				case InBondMessageType.AirEntireInBondArrival:
					return InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination;
				case InBondMessageType.AirEntireInBondExportation:
					return InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort;
				case InBondMessageType.DiversionRequest:
					return InBondWPActionCodeList.Codes.DiversionRequest;
				case InBondMessageType.BillOfLadingLevelArrival:
					return InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination;
				case InBondMessageType.ContainerLevelArrival:
					return InBondWPActionCodeList.Codes.ArriveContainerAtDestination;
				case InBondMessageType.BillOfLadingLevelExportation:
					return InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort;
				case InBondMessageType.ContainerLevelExportation:
					return InBondWPActionCodeList.Codes.ExportContainerFromDestinationPort;
				case InBondMessageType.AirBillDelete:
				case InBondMessageType.DepartureBillDelete:
					return InBondWPActionCodeList.Codes.DeleteBill;
				default:
					return ZString.Empty;
			}
		}

		MQEDIMessage GenerateMessage()
		{
			switch (messageType)
			{
				case InBondMessageType.DepartureAdd:
					return new InBondQPMessageBuilder(moveHeader, InBondQPMessageType.Original).PopulateMessage();
				case InBondMessageType.DepartureBillDelete:
				case InBondMessageType.AirBillDelete:
					return new InBondQPMessageBuilder(moveHeader, Bill, Bill.MovementDetail, InBondQPMessageType.BillLevelDelete).PopulateMessage();
				case InBondMessageType.DepartureDelete:
				case InBondMessageType.DepartureAmend:
					return new InBondQPMessageBuilder(new InBondDeleteMessageSendingObject(moveHeader), InBondQPMessageType.Delete).PopulateMessage();
				case InBondMessageType.InBondLevelArrival:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination);
				case InBondMessageType.BillOfLadingLevelArrival:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, Bill, Bill, InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination);
				case InBondMessageType.ContainerLevelArrival:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, Bill, Bill, Container, Container, InBondWPActionCodeList.Codes.ArriveContainerAtDestination);
				case InBondMessageType.InBondLevelExportation:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort);
				case InBondMessageType.BillOfLadingLevelExportation:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, Bill, Bill, InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort);
				case InBondMessageType.ContainerLevelExportation:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, Bill, Bill, Container, Container, InBondWPActionCodeList.Codes.ExportContainerFromDestinationPort);
				case InBondMessageType.InBondLevelTransferOfLiability:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, null, null, InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForEntireInBond);
				case InBondMessageType.AirInBondAdd:
					return new InBondQPMessageBuilder(moveHeader, InBondQPMessageType.Original).PopulateMessage();
				case InBondMessageType.AirInBondDelete:
				case InBondMessageType.AirInBondAmend:
					return new InBondQPMessageBuilder(moveHeader, InBondQPMessageType.Delete).PopulateMessage();
				case InBondMessageType.AirEntireInBondArrival:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, null, null, InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination);
				case InBondMessageType.AirEntireInBondExportation:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, null, null, InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort);
				case InBondMessageType.DiversionRequest:
					return new InBondWPMQEDIMessageBuilder().Generate(moveHeader, null, null, InBondWPActionCodeList.Codes.DiversionRequest, this);
				default:
					throw new NotSupportedException();
			}
		}

		bool ShouldSendByDefaultForDeparture
		{
			get { return (messageType == InBondMessageType.DepartureBillDelete || messageType == InBondMessageType.DepartureAdd) && !moveHeader.HasClearDepartureAdd; }
		}

		IEnumerable<MessageBlock> GetMessageBlocks()
		{
			switch (messageType)
			{
				case InBondMessageType.DepartureAdd:
					return new ACEInbondQPMessageBlockBuilder(moveHeader).Build(InBondQPMessageType.Original);
				case InBondMessageType.DepartureBillDelete:
				case InBondMessageType.AirBillDelete:
					return new ACEInbondQPMessageBlockBuilder(moveHeader).Build(InBondQPMessageType.BillLevelDelete, Bill.MovementDetail);
				case InBondMessageType.DepartureDelete:
				case InBondMessageType.DepartureAmend:
					return new ACEInbondQPMessageBlockBuilder(new InBondDeleteMessageSendingObject(moveHeader)).Build(InBondQPMessageType.Delete);
				case InBondMessageType.InBondLevelArrival:
					return new ACEInBondWPMessageBlockBuilder(moveHeader).Build(InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination);
				case InBondMessageType.BillOfLadingLevelArrival:
					return new ACEInBondWPMessageBlockBuilder(moveHeader, Bill).Build(InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination);
				case InBondMessageType.ContainerLevelArrival:
					return new ACEInBondWPMessageBlockBuilder(moveHeader, Bill, Container).Build(InBondWPActionCodeList.Codes.ArriveContainerAtDestination);
				case InBondMessageType.InBondLevelExportation:
					return new ACEInBondWPMessageBlockBuilder(moveHeader).Build(InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort);
				case InBondMessageType.BillOfLadingLevelExportation:
					return new ACEInBondWPMessageBlockBuilder(moveHeader, Bill).Build(InBondWPActionCodeList.Codes.ExportBillOfLadingFromDestinationPort);
				case InBondMessageType.ContainerLevelExportation:
					return new ACEInBondWPMessageBlockBuilder(moveHeader, Bill, Container).Build(InBondWPActionCodeList.Codes.ExportContainerFromDestinationPort);
				case InBondMessageType.InBondLevelTransferOfLiability:
					return new ACEInBondWPMessageBlockBuilder(moveHeader).Build(InBondWPActionCodeList.Codes.TransferOfInBondLiabilityForEntireInBond);
				case InBondMessageType.AirInBondAdd:
					return new ACEInbondQPMessageBlockBuilder(moveHeader).Build(InBondQPMessageType.Original);
				case InBondMessageType.AirInBondDelete:
				case InBondMessageType.AirInBondAmend:
					return new ACEInbondQPMessageBlockBuilder(moveHeader).Build(InBondQPMessageType.Delete);
				case InBondMessageType.AirEntireInBondArrival:
					return new ACEInBondWPMessageBlockBuilder(moveHeader).Build(InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination);
				case InBondMessageType.AirEntireInBondExportation:
					return new ACEInBondWPMessageBlockBuilder(moveHeader).Build(InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort);
				case InBondMessageType.DiversionRequest:
					return new ACEInBondWPMessageBlockBuilder(moveHeader, this).Build(InBondWPActionCodeList.Codes.DiversionRequest);
				default:
					throw new NotSupportedException();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_ShouldSend();

			ValidateUS_ArrivalFirmsCode();
			ValidateUS_ArrivalDate();

			ValidateUS_ExportDate();
			ValidateUS_ExportConveyance();
			ValidateUS_ExportTransportMode();
			ValidateUS_ExportConveyance();

			ValidateUS_TOLDate();
			ValidateUS_TOLCarrierCode();

			ValidateUS_DiversionPortCode();
			ValidateUS_OA_DiversionInBondCarrier();
			ValidationUS_DiversionInBondCarrierID();
			ValidationUS_DiversionCarrierSCAC();
		}
		#endregion

		public override void Delete()
		{
			base.Delete();
			if (this.Container != null)
			{
				Container.ShouldSend = false;
			}
			if (this.Bill != null)
			{
				Bill.ShouldSend = false;
			}
			moveHeader.ShouldSend = false;
		}
	}
}
