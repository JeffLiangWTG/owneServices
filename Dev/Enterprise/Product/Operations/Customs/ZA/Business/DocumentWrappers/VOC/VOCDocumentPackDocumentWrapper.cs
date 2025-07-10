using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class VOCDocumentPackDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper
	{
		public VOCDocumentPackDocumentWrapper(CusEntryHeader cusEntryHeader) : base(cusEntryHeader?.Factory ?? new BusinessObjectFactory())
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			this.EntryHeader = cusEntryHeader;
			this.Declaration = cusEntryHeader.Declaration;
			var messageSendingObject = new MessageSendingObject(EntryHeader);
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			CUSDECSource = messageSendingObject;
			BeforeValueProvider = messageSendingObject;
			AfterValueProvider = messageSendingObject;
			((MessageSendingObject)CUSDECSource).MessageKeyFactor.DeclarationType = EntryDocType;
		}

		VOCDocumentPackDocumentWrapper(CUSDECEDIMessage message)
			: base(message?.Factory ?? new BusinessObjectFactory())
		{
			var linkedEntryHeader = message?.EM_LinkedObject as CusEntryHeader;
			this.EntryHeader = linkedEntryHeader;
			this.Declaration = linkedEntryHeader?.Declaration;
			SourceMessage = message;
			CUSDECSource = CUSDECMessageHelper.New(message);
			BeforeValueProvider = message;
			AfterValueProvider = message;
		}

		internal static VOCDocumentPackDocumentWrapper NewForMessage(CUSDECEDIMessage message)
		{
			VOCDocumentPackDocumentWrapper result = null;
			if ((message?.EM_LinkedObject as CusEntryHeader) != null)
			{
				var resultwrapper = new VOCDocumentPackDocumentWrapper(message);
				if (resultwrapper.CUSDECSource != null)
				{
					result = resultwrapper;
				}
			}
			return result;
		}

		#region Related Objects

		public CusEntryHeader EntryHeader { get; private set; }
		public JobDeclaration Declaration { get; private set; }

		internal ZAMessage SourceMessage;
		internal ICUSDECMessageDataProvider CUSDECSource;
		internal IVOCBeforeValues BeforeValueProvider;
		internal IVOCAfterValues AfterValueProvider;

		#endregion

		#region Properties

		public ZString EntryDocType => CUSDECSource.GetDeclarationTypeForDocumentWrapper();

		public ZDateTime DateOfAssessment => CUSDECSource.DateOfAssessment;

		public ZString OriginalMRN => CUSDECSource.OriginalMRN;

		public ZString CustomsProcedureCategory => CUSDECSource.CustomsProcedureCategory;

		public ZString AgentCode => CUSDECSource.AgentCode;

		public ZInt TotalLineCount => CUSDECSource.TotalLineCount;

		public ZString CountryOfExport => CUSDECSource.CountryOfExport;

		public ZString CountryOfDestination => CUSDECSource.CountryOfDestination;

		public ZString HouseBill => CUSDECSource.HouseBill;

		public ZString ShipmentType => CUSDECSource.ShipmentType;

		public ZDateTime MovementReferenceNumberDate
		{
			get
			{
				var result = ZDateTime.Invalid;
				var valueString = CUSDECSource.OriginalMRN.SubstringSafe(3, 8);
				ZDateTime.TryParseExact(valueString, out result, MessageBuilders.Constants.DateFormatCCYYMMDD);
				return result;
			}
		}

		public ZString LocalReferenceNumber => CUSDECSource.LocalReferenceNumber;

		public ZString MessageNumber => SourceMessage?.EM_MessageNum ?? ZString.Empty;

		#region Organizations Related

		public AddressInformationDocWrapper Importer
		{
			get
			{
				if (importer == null)
				{
					var source = CUSDECSource.Importer;
					var entryInstruction = EntryHeader?.EntryInstruction;
					var declarationImporter = entryInstruction != null && entryInstruction.HasBothOutOfAndIntoRegimeProcedure ? entryInstruction.Owner : Declaration?.Importer;
					var fallbackSource = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, source, declarationImporter, OrgCusCode.CodeTypes.CustomsClientCode);
					importer = new AddressInformationDocWrapper(source, fallbackSource);
				}
				return importer;
			}
		}
		AddressInformationDocWrapper importer;

		public AddressInformationDocWrapper Supplier
		{
			get
			{
				var isImport = Declaration?.IsImport ?? ZBool.False;
				if (supplier == null && isImport)
				{
					var source = CUSDECSource.Supplier;
					var fallbackSource = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, source, Declaration?.Supplier, OrgCusCode.CodeTypes.SupplierCode);
					supplier = new AddressInformationDocWrapper(source, fallbackSource);
				}
				return supplier;
			}
		}

		AddressInformationDocWrapper supplier;

		public AddressInformationDocWrapper Exporter => exporter ?? (exporter = new AddressInformationDocWrapper(CUSDECSource.Exporter));
		AddressInformationDocWrapper exporter;

		public ZString ToWarehouse => CUSDECSource.ToWarehouse;

		public ZString ToWarehouseAddress
		{
			get
			{
				var result = ZString.Empty;
				var warehouse = EntryHeader?.EntryInstruction?.Warehouse2;
				if (SourceMessage == null || ShouldGetWarehouseAddressFromJob(warehouse, OrgCusCode.CodeTypes.WarehouseControlledPremisesID, ToWarehouse))
				{
					result = warehouse?.AddressAsASingleLine ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZString FromWarehouse => CUSDECSource.FromWarehouse;

		public ZString FromWarehouseAddress
		{
			get
			{
				var result = ZString.Empty;
				var warehouse = EntryHeader?.EntryInstruction?.Warehouse;
				if (SourceMessage == null || ShouldGetWarehouseAddressFromJob(warehouse, OrgCusCode.CodeTypes.WarehouseControlledPremisesID, FromWarehouse))
				{
					result = warehouse?.AddressAsASingleLine ?? ZString.Empty;
				}
				return result;
			}
		}

		bool ShouldGetWarehouseAddressFromJob(OrgAddress address, ZString codeType, ZString codeFromMessage)
		{
			var result = false;
			if (!codeFromMessage.IsEmpty && !codeType.IsEmpty)
			{
				var regNoOfOrgFromJob = address?.GetRegNoWithOrganisationAddress(codeType) ?? ZString.Empty;
				result = regNoOfOrgFromJob == codeFromMessage;
			}
			return result;
		}

		public ZString RemoverTransporterCode => CUSDECSource.RemoverTransporterCode;

		public ZString RemoverAddress
		{
			get
			{
				var result = ZString.Empty;
				var remover = EntryHeader?.EntryInstruction?.Remover;
				if (SourceMessage == null || !RemoverTransporterCode.IsEmpty
					&& (remover?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.SouthAfrica, OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode) ?? ZString.Empty) == RemoverTransporterCode)
				{
					result = remover?.MainAddress.AddressAsASingleLine ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZString OwnerCode => CUSDECSource.OwnerCode;

		#endregion

		public ZString LocationOfGoods => CUSDECSource.LocationOfGoods;

		public ZString LocationOfGoodsName => (CUSDECSource as CUSDECMessageHelper)?.LocationOfGoodsName ?? Declaration?.LocationOfGoods?.ZZD_Description ?? ZString.Empty;

		public ZString TransportMode => CUSDECSource.TransportMode;

		public ZString TransportDocumentNumberInBusinessLogic => CUSDECSource.GetTransportDocumentNumberInBusinessLogic();

		public ZString TransportDocumentIssuedAt => CUSDECSource.TransportDocumentIssuedAt;

		public ZDateTime TransportDocumentDate => CUSDECSource.TransportDocumentDate;

		public ZString VoyageFlightNo => CUSDECSource.VoyageFlightNo;

		public ZString TransportName => CUSDECSource.TransportName;

		public ZDateTime DateOfDepartureOrDateOfFlight => CUSDECSource.DateOfDepartureOrDateOfFlight;

		public ZDateTime DateOfArrival => CUSDECSource.DateOfArrival;

		public ZString CreditTerms => CUSDECSource.CreditTerms;

		public ZDecimal TotalTransactionValue => CUSDECSource.TotalTransactionValue;

		public ZString TotalTransactionValueCurrency => CUSDECSource.TotalTransactionValueCurrency;

		public ZString TransactionBankCode => CUSDECSource.TransactionBankCode;

		public ZString UniqueConsignmentReference => CUSDECSource.UniqueConsignmentReference;

		public ZString PortOfExit => CUSDECSource.PortOfExit;

		public ZString PortOfExitName => ZARefCusCodeListTypes.GetCustomsOfficeList(Factory).GetDescriptionFromCode(PortOfExit);

		public ZInt CurrentPartIndex
		{
			get
			{
				return EntryHeader.CH_EntryNumber;
			}
		}

		public ZInt PartClearanceQuantity => CUSDECSource.PartClearanceQuantity;

		public ZString EndorsementsAppend => CUSDECSource.GetPartofPackagesValue(Declaration, CurrentPartIndex);

		public ZString VATIndicator => CUSDECSource.VATIndicator;

		#region Entry Lines

		public BusinessObjectCollectionWrapper<VOCLineDetailWrapper> EntryLines
		{
			get
			{
				if (entryLines == null)
				{
					entryLines = new BusinessObjectCollectionWrapper<VOCLineDetailWrapper>(CUSDECSource.LineLevelDetails?.Select(line => new VOCLineDetailWrapper(line, EntryHeader, Factory)));
				}
				return entryLines;
			}
		}

		BusinessObjectCollectionWrapper<VOCLineDetailWrapper> entryLines;

		#endregion

		#region Difference Segment

		public ZDecimal CIFValueBefore => BeforeValueProvider.CIFValue;
		public ZDecimal CustomsValueBefore => BeforeValueProvider.CustomsValue;
		public ZDecimal CustomsDutyNoS1P2BBefore => BeforeValueProvider.CustomsDutyNoS1P2B;
		public ZDecimal S1P2BDutyBefore => BeforeValueProvider.S1P2BDuty;
		public ZDecimal ValueAddedTaxBefore => BeforeValueProvider.ValueAddedTax;
		public ZDecimal ProvisionalPaymentsAndPenaltiesBefore => BeforeValueProvider.ProvisionalPaymentAmount + BeforeValueProvider.PenaltyAmount;

		public ZDecimal CIFValueAfter => AfterValueProvider.CIFValue;
		public ZDecimal CustomsValueAfter => AfterValueProvider.CustomsValue;
		public ZDecimal CustomsDutyNoS1P2BAfter => AfterValueProvider.CustomsDutyNoS1P2B;
		public ZDecimal S1P2BDutyAfter => AfterValueProvider.S1P2BDuty;
		public ZDecimal ValueAddedTaxAfter => AfterValueProvider.ValueAddedTax;
		public ZDecimal ProvisionalPaymentsAndPenaltiesAfter => AfterValueProvider.ProvisionalPaymentAmount + AfterValueProvider.PenaltyAmount;

		public ZDecimal CIFValueDifference => CIFValueAfter - CIFValueBefore;
		public ZDecimal CustomsValueDifference => CustomsValueAfter - CustomsValueBefore;
		public ZDecimal CustomsDutyNoS1P2BDifference => CustomsDutyNoS1P2BAfter - CustomsDutyNoS1P2BBefore;
		public ZDecimal S1P2BDutyDifference => S1P2BDutyAfter - S1P2BDutyBefore;
		public ZDecimal ValueAddedTaxDifference => ValueAddedTaxAfter - ValueAddedTaxBefore;
		public ZDecimal ProvisionalPaymentsAndPenaltiesDifference => ProvisionalPaymentsAndPenaltiesAfter - ProvisionalPaymentsAndPenaltiesBefore;

		public ZDecimal AmountDueBefore => CustomsDutyNoS1P2BBefore + S1P2BDutyBefore + ValueAddedTaxBefore + ProvisionalPaymentsAndPenaltiesBefore;
		public ZDecimal AmountDueAfter => CustomsDutyNoS1P2BAfter + S1P2BDutyAfter + ValueAddedTaxAfter + ProvisionalPaymentsAndPenaltiesAfter;
		public ZDecimal AmountDueDifference => AmountDueAfter - AmountDueBefore;

		#endregion

		#region Declaration Section

		public ZString MarksAndNumbers => ZString.Join(System.Environment.NewLine, CUSDECSource.MarksAndNumbers.Where(x => !x.IsEmpty).ToArray());

		ZInt NoOfPacksInNumber
		{
			get
			{
				if (!cachedNoOfPacksInNumber.HasValue)
				{
					ZInt result;
					var checkResult = ZInt.TryParse(CUSDECSource.TotalNoOfPacks, out result);
					if (!checkResult)
					{
						result = ZInt.Zero;
					}
					cachedNoOfPacksInNumber = result;
				}
				return cachedNoOfPacksInNumber.Value;
			}
		}
		ZInt? cachedNoOfPacksInNumber;

		public ZString TotalNoOfPacks => CUSDECSource.TotalNoOfPacks;

		public ZInt NoOfPacksUnit => NoOfPacksInNumber % 10;

		public ZInt NoOfPacksTens => NoOfPacksInNumber % 100 / 10;

		public ZInt NoOfPacksHundred => NoOfPacksInNumber % 1000 / 100;

		public ZInt NoOfPacksThousand => NoOfPacksInNumber / 1000;

		public ZDecimal GrossWeightInKG => CUSDECSource.GrossWeightInKG;

		public ZString PaymentMethod => CUSDECSource.PaymentMethod;

		public ZString CustomsOfficeCode => CUSDECSource.CustomsOfficeCode;

		public GlbStaff DeclarantUser => Declaration?.CusAgent ?? SourceMessage?.UserWhoQueuedThisRecord ?? GlbStaff.CurrentUser;

		public ZString VOCReason => SourceMessage?.VOCReason ?? ZString.Empty;

		public BusinessObjectCollectionWrapper<CusContainerDocWrapper> ContainersCollection
		{
			get
			{
				if (containersCollection == null)
				{
					containersCollection = new BusinessObjectCollectionWrapper<CusContainerDocWrapper>(CUSDECSource.Containers.Select(cont => new CusContainerDocWrapper(cont, Factory)));
				}
				return containersCollection;
			}
		}
		BusinessObjectCollectionWrapper<CusContainerDocWrapper> containersCollection;

		public ZBool ContainersOverFlow => ContainersCollection.Count > 6;

		#endregion

		#endregion

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => EntryHeader;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => this.EntryHeader.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
