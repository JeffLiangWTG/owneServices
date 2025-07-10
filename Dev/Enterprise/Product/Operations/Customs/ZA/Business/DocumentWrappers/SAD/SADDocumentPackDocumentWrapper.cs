using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.ZA.Business.MessageBuilders.Constants;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class SADDocumentPackDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper, ISourceIdentifierProvider
	{
		#region ctor

		public SADDocumentPackDocumentWrapper(CusEntryHeader cusEntryHeader)
			: base(cusEntryHeader?.Factory ?? new BusinessObjectFactory())
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			this.EntryHeader = cusEntryHeader;
			this.Declaration = cusEntryHeader.Declaration;
			CUSDECSource = new MessageSendingObject(EntryHeader);
			((MessageSendingObject)CUSDECSource).MessageKeyFactor.DeclarationType = EntryDocType;
		}

		SADDocumentPackDocumentWrapper(CUSDECEDIMessage message)
			: base(message?.Factory ?? new BusinessObjectFactory())
		{
			var linkedEntryHeader = message?.EM_LinkedObject as CusEntryHeader;
			this.EntryHeader = linkedEntryHeader;
			this.Declaration = linkedEntryHeader?.Declaration;
			CUSDECSource = CUSDECMessageHelper.New(message);
			sourceEDIMessage = message;
		}

		internal static SADDocumentPackDocumentWrapper NewForMessage(CUSDECEDIMessage message)
		{
			SADDocumentPackDocumentWrapper result = null;
			if ((message?.EM_LinkedObject as CusEntryHeader) != null)
			{
				var resultwrapper = new SADDocumentPackDocumentWrapper(message);
				if (resultwrapper.CUSDECSource != null)
				{
					result = resultwrapper;
				}
			}
			return result;
		}

		#endregion

		#region Related Objects

		readonly CUSDECEDIMessage sourceEDIMessage;
		public CusEntryHeader EntryHeader { get; private set; }
		public JobDeclaration Declaration { get; private set; }
		public ICUSDECMessageDataProvider CUSDECSource { get; private set; }

		#endregion

		#region SAD 500

		public ZString EntryDocType => CUSDECSource.GetDeclarationTypeForDocumentWrapper();

		public ZString CustomsProcedureCategory => CUSDECSource.CustomsProcedureCategory;

		public ZString CustomsOfficeCode => CUSDECSource.CustomsOfficeCode;

		public ZString TransportMode => CUSDECSource.TransportMode;

		public ZString TransportDocumentNumberInBusinessLogic => CUSDECSource.GetTransportDocumentNumberInBusinessLogic();

		public ZString TransportDocumentIssuedAtLocationName => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CUSDECSource.TransportDocumentIssuedAt)?.RL_PortName ?? ZString.Empty;

		public ZDateTime TransportDocumentDate => CUSDECSource.TransportDocumentDate;

		public ZString HouseBill => CUSDECSource.HouseBill;

		public ZDateTime HouseBillIssuedDate => CUSDECSource.HouseBillIssuedDate;

		public ZString UnregisteredTraderNumber => DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(CUSDECSource.UnregisteredTrader);

		public ZString ShipmentType => CUSDECSource.ShipmentType;

		public AddressInformationDocWrapper Exporter
		{
			get
			{
				if (exporter != null)
				{
					return exporter;
				}

				var source = CUSDECSource.Exporter;

				exporter = source as AddressInformationDocWrapper ?? new AddressInformationDocWrapper(source);

				return exporter;
			}
		}
		AddressInformationDocWrapper exporter;

		public AddressInformationDocWrapper Supplier
		{
			get
			{
				if (supplier == null)
				{
					var source = CUSDECSource.Supplier;
					OrgHeader currentOrgHeader = null;
					var existEntryLineWithVDNCode = EntryHeader.MergedLines.Cast<CusEntryLine>().Any
						(e => e.AdditionalInformationCodes.Cast<AdditionalInformation>().Any(a => a.CY_Code == UniversalReferenceConstants.AdditionalInformation.ValueDeterminationNumber));
					if (existEntryLineWithVDNCode)
					{
						currentOrgHeader = EntryHeader.InvoiceHeaders.FirstOrDefault()?.Supplier;
					}
					else
					{
						currentOrgHeader = Declaration?.Supplier;
					}
					var fallbackSource = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, source, currentOrgHeader, OrgCusCode.CodeTypes.SupplierCode);
					supplier = new AddressInformationDocWrapper(source, fallbackSource);
				}
				return supplier;
			}
		}

		AddressInformationDocWrapper supplier;

		public ZString MessageType => CUSDECSource.MessageType;

		public ZString LocalReferenceNumber => CUSDECSource.LocalReferenceNumber;

		public ZInt TotalLineCount => CUSDECSource.TotalLineCount;

		public ZString TotalNoOfPacks => CUSDECSource.TotalNoOfPacks;

		public ZInt TotalPartClearancePacks => Declaration?.ClearanceParts?.Sum(entryHeader => entryHeader.PackagesCount) ?? ZInt.Zero;

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

		public ZDecimal TotalCIFCAmount => CUSDECSource.TotalCIFCAmount;

		public InvoiceInformationDocWrapper FirstInvoice
		{
			get
			{
				if (firstInvoice == null)
				{
					var firstInvoiceRead = CUSDECSource.InvoiceInformations.FirstOrDefault();
					firstInvoice = (firstInvoiceRead as InvoiceInformationDocWrapper) ?? new InvoiceInformationDocWrapper(firstInvoiceRead);
				}
				return firstInvoice;
			}
		}
		InvoiceInformationDocWrapper firstInvoice;

		public ZString AgentCode => CUSDECSource.AgentCode;

		public OrgHeader EffectiveAgent => OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.AgentCode, AgentCode, Core.Constants.CountryCodes.SouthAfrica);

		public ZString CountryOfExport => CUSDECSource.CountryOfExport;

		public ZString CountryOfDestination => CUSDECSource.CountryOfDestination;

		public RefCountry DestinationCountry => RefCountry.LoadFromCountryCode(Factory, CountryOfDestination);

		public ZString OwnerCode => CUSDECSource.OwnerCode;

		public ZString VoyageFlightNo => CUSDECSource.VoyageFlightNo;

		public ZString TransportName => CUSDECSource.TransportName;

		public ZDateTime DateOfDepartureOrDateOfFlight => CUSDECSource.DateOfDepartureOrDateOfFlight;

		public ZDateTime DateOfArrival => CUSDECSource.DateOfArrival;

		public ZDecimal TotalCustomsValue => CUSDECSource.TotalCustomsValue;

		public ZString RemovalTransportMode => CUSDECSource.RemovalTransportMode;

		public ZString CreditTerms => CUSDECSource.CreditTerms;

		public ZDecimal TotalTransactionValue => CUSDECSource.TotalTransactionValue;

		public ZString TotalTransactionValueCurrency => CUSDECSource.TotalTransactionValueCurrency;

		public ZString TransactionBankCode => CUSDECSource.TransactionBankCode;

		public ZString UniqueConsignmentReference => CUSDECSource.UniqueConsignmentReference;

		public ZString VATIndicator => CUSDECSource.VATIndicator;

		public ZString PortOfExit => CUSDECSource.PortOfExit;

		public ZString LocationOfGoods => CUSDECSource.LocationOfGoods;

		public ZString MarksAndNumbers
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var markandnumber in (CUSDECSource.MarksAndNumbers ?? System.Array.Empty<ZString>()))
				{
					result.Append(markandnumber);
				}
				return result.ToString();
			}
		}

		public ZString MarksAndNumbersAppend => CUSDECSource.GetPartofPackagesValue(Declaration, CurrentPartIndex);

		public ZString TotalNoOfPacksInLongHand
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var charDigit in TotalNoOfPacks.ToString().ToCharArray())
				{
					result.Append(DocumentWrapperHelper.TranslateDigitToLongHand(charDigit));
				}
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZInt PartClearanceQuantity => CUSDECSource.PartClearanceQuantity;

		public ZInt CurrentPartIndex
		{
			get
			{
				return EntryHeader.CH_EntryNumber;
			}
		}

		public SADLineDetailWrapper FirstLineDetail
		{
			get
			{
				if (firstLineDetail == null)
				{
					if ((LineDetails?.Count ?? 0) > 0)
					{
						firstLineDetail = LineDetails[0];
					}
					else
					{
						firstLineDetail = new SADLineDetailWrapper(null, EntryHeader, sourceEDIMessage);
					}
				}
				return firstLineDetail;
			}
		}
		SADLineDetailWrapper firstLineDetail;

		BusinessObjectCollectionWrapper<SADLineDetailWrapper> LineDetails
		{
			get
			{
				if (lineDetails == null)
				{
					lineDetails = new BusinessObjectCollectionWrapper<SADLineDetailWrapper>(CUSDECSource.LineLevelDetails?.Select(x => new SADLineDetailWrapper(x, EntryHeader, sourceEDIMessage)));
				}
				return lineDetails;
			}
		}
		BusinessObjectCollectionWrapper<SADLineDetailWrapper> lineDetails;

		public BusinessObjectCollectionWrapper<CusContainerDocWrapper> ContainersCollection
		{
			get
			{
				if (containersCollection == null)
				{
					containersCollection = new BusinessObjectCollectionWrapper<CusContainerDocWrapper>(CUSDECSource.Containers.Select(cont => (cont as CusContainerDocWrapper) ?? new CusContainerDocWrapper(cont, Factory)));
				}
				return containersCollection;
			}
		}
		BusinessObjectCollectionWrapper<CusContainerDocWrapper> containersCollection;

		public ZDecimal GrossWeightInKG => CUSDECSource.GrossWeightInKG;

		public ZString RelatedPartyIndicator => MustSuppressBox43ValuationMethod ? ZString.Empty : CUSDECSource.RelatedPartyIndicatorForSADDocumentPackPurposes;

		public ZString ValuationCode => MustSuppressBox43ValuationMethod ? ZString.Empty : CUSDECSource.ValuationCodeForSADDocumentPackPurposes;

		bool MustSuppressBox43ValuationMethod
		{
			get
			{
				var mustSuppress = false;
				if (Declaration.JE_MessageType != ZAJobMessageTypeList.Codes.Import)
				{
					mustSuppress = true;
				}
				if (!mustSuppress)
				{
					var procedureCodeList = new List<string>() { "12", "20", "21", "22", "37", "78" };
					if (sourceEDIMessage != null)
					{
						if (CUSDECSource.LineLevelDetails.Cast<CUSDECMessageSG30Helper>().Any(e => !e.ValueDeterminationNumber.IsEmpty))
						{
							mustSuppress = true;
						}
						if (CUSDECSource.LineLevelDetails.Any(e => procedureCodeList.Contains(e.CustomsProcedureCode)))
						{
							mustSuppress = true;
						}
					}
					else
					{
						var invoiceHeaderHasVDN = EntryHeader.InvoiceHeaders.Cast<JobComInvoiceHeader>().Any(e => !e.JZ_VDN.IsEmpty);
						if (invoiceHeaderHasVDN && (Declaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import))
						{
							mustSuppress = true;
						}
						var procedureCode = EntryHeader.EntryInstruction?.CEI_Style ?? ZString.Empty;
						if (procedureCodeList.Contains(procedureCode))
						{
							mustSuppress = true;
						}
					}
				}
				return mustSuppress;
			}
		}

		public ZString FromWarehouse => CUSDECSource.FromWarehouse;

		public ZString PaymentMethod => CUSDECSource.PaymentMethod;

		public ZString ToWarehouse => CUSDECSource.ToWarehouse;

		public ZDecimal OverpaidExcise => CUSDECSource.OverpaidExcise;

		public ZDecimal UnderpaidExcise => CUSDECSource.UnderpaidExcise;

		public ZDecimal TotalDutiesAndTaxes
		{
			get
			{
				var result = ZDecimal.Zero;
				if (CUSDECSource is CUSDECMessageHelper messageHelper)
				{
					result = messageHelper.TotalDutiesAndTaxes;
				}
				else if (EntryHeader != null)
				{
					result = EntryHeader.TotalDutiesAndTaxes;
				}
				return result;
			}
		}

		public ZDecimal TotalPayable
		{
			get
			{
				var result = ZDecimal.Zero;
				if (CUSDECSource != null)
				{
					result = TotalDutiesAndTaxes + CUSDECSource.UnderpaidExcise - CUSDECSource.OverpaidExcise;
				}
				return result;
			}
		}

		public GlbStaff DeclarantUser => Declaration?.CusAgent ?? sourceEDIMessage?.UserWhoQueuedThisRecord ?? GlbStaff.CurrentUser;

		public ZDateTime LocalReferenceNumberDate
		{
			get
			{
				var result = ZDateTime.Invalid;
				var valueString = EntryHeader.CH_BGMReference.SubstringSafe(11, 8);
				ZDateTime.TryParseExact(valueString, out result, Constants.DateFormatCCYYMMDD);
				return result;
			}
		}

		public ZString MRNToBeReplaced => CUSDECSource.MRNToBeReplaced;

		#endregion

		#region SAD 501

		public BusinessObjectCollectionWrapper<SAD501DocumentPageWrapper> SAD501DocumentPages
		{
			get
			{
				if (sad501DocumentPages == null)
				{
					sad501DocumentPages = new BusinessObjectCollectionWrapper<SAD501DocumentPageWrapper>();
					var source = this.LineDetails;
					var sourceCount = source?.Count ?? 0;
					if ((sourceCount) > 1)
					{
						var runningTotalDutiesAndFeesStarting = this.FirstLineDetail.CalcDutiesAndFees;
						for (int i = 1; i < sourceCount; i += 3)
						{
							var first = source.ElementAtOrDefault(i) as SADLineDetailWrapper;
							var second = source.ElementAtOrDefault(i + 1) as SADLineDetailWrapper;
							var third = source.ElementAtOrDefault(i + 2) as SADLineDetailWrapper;
							var new501Page = new SAD501DocumentPageWrapper(first, second, third, runningTotalDutiesAndFeesStarting);
							sad501DocumentPages.Add(new501Page);
							runningTotalDutiesAndFeesStarting = new501Page.RunningTotalDutiesAndFeesOfThisPage;
						}
					}
				}
				return sad501DocumentPages;
			}
		}
		BusinessObjectCollectionWrapper<SAD501DocumentPageWrapper> sad501DocumentPages;

		#endregion

		#region SAD 502

		public ZString RemoverTransporterCode => EntryHeader?.RemoverLocalCustomsCarrierCode ?? ZString.Empty;

		public OrgHeader Remover
		{
			get
			{
				if (remover == null)
				{
					remover = OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, RemoverTransporterCode, Core.Constants.CountryCodes.SouthAfrica);
				}
				return remover;
			}
		}
		OrgHeader remover;

		public ZString SubContractorTransporterCode => EntryHeader?.SubContractorRemoverCarrierCode ?? ZString.Empty;

		public OrgHeader SubContractor => EntryHeader?.EntryInstruction?.SubContractor;

		#endregion

		#region SAD 507

		public BusinessObjectCollectionWrapper<SAD507DocumentPageWrapper> SAD507DocumentPages
		{
			get
			{
				if (sad507DocumentPages == null)
				{
					sad507DocumentPages = new BusinessObjectCollectionWrapper<SAD507DocumentPageWrapper>();
					var overflowContainers = new List<CusContainerDocWrapper>();
					var overflowAdditionalInfomation = new List<AdditionalInformationDocWrapperWithLineNumber>();
					for (int i = 8; i < ContainersCollection.Count; i++)
					{
						overflowContainers.Add(ContainersCollection[i]);
					}

					foreach (var line in LineDetails.Cast<SADLineDetailWrapper>())
					{
						var addinfos = line.GeneralAddInfos;
						for (int i = 3; i < addinfos.Count; i++)
						{
							overflowAdditionalInfomation.Add(new AdditionalInformationDocWrapperWithLineNumber(addinfos[i], line.LineNumberFormatted, CUSDECSource.DateOfAssessment, line.Factory));
						}
					}

					int containerLimit = 78;
					int addinfoLimit = 50;
					int containerCount = overflowContainers.Count;
					int addinfoCount = overflowAdditionalInfomation.Count;
					if (containerCount > 0 || addinfoCount > 0)
					{
						for (int i = 0; (i * containerLimit) < containerCount || (i * addinfoLimit) < addinfoCount; i++)
						{
							sad507DocumentPages.Add(new SAD507DocumentPageWrapper(overflowContainers.Skip(i * containerLimit).Take(containerLimit), overflowAdditionalInfomation.Skip(i * addinfoLimit).Take(addinfoLimit)));
						}
					}

					var haveHouseBillDetails = !EntryHeader.HAWBOverride.IsEmpty || !EntryHeader.CargoCarrierOverride.IsEmpty;

					if (sad507DocumentPages.Count == 0 && (!EntryHeader.Endorsements.IsEmpty || haveHouseBillDetails))
					{
						sad507DocumentPages.Add(new SAD507DocumentPageWrapper(null, null));
					}
				}
				return sad507DocumentPages;
			}
		}
		BusinessObjectCollectionWrapper<SAD507DocumentPageWrapper> sad507DocumentPages;

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

		#region ISourceIdentifierProvider

		public ZGuid SourceIdentifier => EntryHeader.PK;

		#endregion
	}
}
