using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSHeaderProvider : INCTSHeader, Integration.Customs.TR.INctsMessageProvider
	{
		public NCTSHeaderProvider(Integration.Customs.TR.ICusInBondHeader header)
		{
			Header = (NctsHeader)Argument.NotNull(header, "NctsHeader cannot be null");
		}

		protected readonly NctsHeader Header;
		NctsDepartureMovementHeader MovementHeader => Header.MovementHeader;
		NctsGuarantee NctsGuarantee => Header.Guarantees?.FirstOrDefault();
		GlbCompany GlbCompany => Header.Branch?.Company;

		public string SyntaxIdentifier => NCTSMessageProviderConstants.NCTSHeader.SyntaxIdentifier;
		public int SyntaxVersionNumber => NCTSMessageProviderConstants.NCTSHeader.SyntaxVersionNumber;
		public string MessageSender => NCTSMessageProviderConstants.Messages.MessageSender;
		public string MessageRecipient => NCTSMessageProviderConstants.Messages.MessageRecipient;
		public string DateOfPreparation => ZDateTime.Now.ToString("yyMMdd");
		public int TimeOfPreparation => ZInt.TryParse(ZDateTime.Now.ToString("HHmm"), out var timeOfPreparation) ? timeOfPreparation : ZInt.Zero;
		public string InterchangeControlReference => Header.BH_JobReference;
		public int AcknowledgementRequest => ZInt.Zero;
		public int TestIndicator => ZInt.Zero;
		public string MessageIdentification => Header.BH_JobReference + GlbStaff.CurrentUser.GS_Code;
		public string MessageType => NCTSMessageProviderConstants.Messages.MessageType;
		public string CommunicationsAgreementId => NCTSMessageProviderConstants.NCTSHeader.ZeroValue;

		#region HEAHEA Tags
		public int ReferenceNumber => Header.Messages.Cast<EDIMessage>().Count(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && x.EM_MessageType == TRMessageTypes.Codes.TRN) + 1;
		public string TypeOfDeclaration => MovementHeader.BM_InBondEntryType;
		public string DestinationCountryCode => MovementHeader.BM_RL_NKDestinationPort;
		public string AgreedLocationOfGoodsCode => MovementHeader.BM_LocationOfGoodsCode;
		public string AgreedLocationOfGoods => MovementHeader.BM_LocationOfGoods;
		public string AgreedLocationOfGoodsLNG => !MovementHeader.BM_LocationOfGoodsCode.IsEmpty ? TRMessageConstants.LanguageCode : string.Empty;
		public string AuthorisedLocationOfGoodsCode => MovementHeader.BM_ExportTransportMode;
		public string PlaceOfLoadingCode => MovementHeader.BM_PlaceOfLoading;
		public string CountryOfDispatchExportCode => Header.BH_RL_NKImportLoadPort.SubstringSafe(0, 2);
		public string CustomsSubPlace => !MovementHeader.BM_CustomsSubPlace.IsEmpty ? MovementHeader.BM_CustomsSubPlace : string.Empty;
		public string InlandTransportMode => !MovementHeader.BM_InlandTransportMode.IsEmpty ? MovementHeader.BM_InlandTransportMode + "0" : string.Empty;
		public string TransportModeAtBorder => !MovementHeader.BM_ExportTransportMode.IsEmpty ? MovementHeader.BM_ExportTransportMode + "0" : string.Empty;
		public string IdentityOfMeansOfTransportAtDeparture => MovementHeader.BM_TransportAtDeparture;
		public string IdentityOfMeansOfTransportAtDepartureLNG => !MovementHeader.BM_TransportAtDeparture.IsEmpty ? TRMessageConstants.LanguageCode : string.Empty;
		public string NationalityOfMeansOfTransportAtDeparture => MovementHeader.BM_RN_NKTransportAtDepartureCountry;
		public string IdentityOfMeansOfTransportCrossingBorder => MovementHeader.BM_TOLCarrierID;
		public string IdentityOfMeansOfTransportCrossingBorderLNG => !MovementHeader.BM_TOLCarrierID.IsEmpty ? TRMessageConstants.LanguageCode : string.Empty;
		public string NationalityofMeansOfTransportCrossingBorder => MovementHeader.BM_TOLCarrierCode;
		public string TypeOfMeansOfTransportCrossingBorder => !MovementHeader.BM_ExportTransportMode.IsEmpty ? MovementHeader.BM_ExportTransportMode + "0" : string.Empty;
		public string ContainerisedIndicator => Header.DepartureHeaderContainers.Count > 0 ? "1" : "0";
		public string DialogLanguageIndicatorAtDeparture => Core.Constants.CountryCodes.Turkey;
		public string DialogLanguageIndicatorAtDepartureLNG => TRMessageConstants.LanguageCode;
		public int TotalNumberofCusInBondCargoDescItems => Header.MovementHeader.TotalNumberOfItems;
		public int TotalNumberofPackages => ZInt.ParseSafe(Header.MovementHeader.TotalNumberOfPackages.ToString(), 0);
		public decimal TotalGrossMass => Header.MovementHeader.TotalGrossMassInKilograms;
		public string DeclarationDate => ZDateTime.Now.ToString("yyyyMMdd");
		public string DeclarationPlace => MovementHeader.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, MovementHeader.BM_RL_NKForeignDestPort)?.RL_NameWithDiacriticals ?? ZString.Empty;
		public string DeclarationPlaceLNG => !MovementHeader.BM_RL_NKForeignDestPort.IsEmpty ? TRMessageConstants.LanguageCode : string.Empty;
		public string TransportChargesOfMethodOfPayment => MovementHeader.BM_MethodOfPayment;
		public string CommercialReferenceNumber => MovementHeader.BM_AdditionalText;
		public string SecurityEnable => MovementHeader.BM_BTAIndicator;
		public string ConveyanceReferenceNumber => MovementHeader.BM_ConveyanceNumber;
		public string PlaceOfUnloadingCode => MovementHeader.BM_PlaceOfUnloading;
		public string PlaceOfUnloadingCodeLNG => !MovementHeader.BM_PlaceOfUnloading.IsEmpty ? TRMessageConstants.LanguageCode : string.Empty;
		public string TruckId2 => Header.Trailer1;
		public string TruckId3 => Header.Trailer2;
		public decimal StampTax => Header.StampDuty.RoundAmount();
		public string ReferenceNumberCustomsData => GetCustomsOffices(Header, OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
		public string Tanker => NCTSMessageProviderConstants.NCTSHeader.ZeroValue;
		public string StampDutyStatus => Header.StampDutyStatus;
		public string StampDutyLedgerDate => Header.RegistrationDate.ToString();
		public string CHKGIK117 => MovementHeader.MoveToFTZ ? CHKGIKCodeList.Codes.Yes : CHKGIKCodeList.Codes.No;
		public string LOCGIK117 => MovementHeader.MoveToFTZ ? MovementHeader.BM_CustomsOfficeAtBorder : ZString.Empty;
		#endregion

		#region Companies

		public INCTSOrganization Principal => Header.Principal.Address != null ? new NCTSOrganizationProvider(Header.Principal) : null;
		public INCTSOrganization Consignor => Header.Consignor.Address != null ? new NCTSOrganizationProvider(Header.Consignor) : null;
		public INCTSOrganization Consignee => Header.Consignee.Address != null ? new NCTSOrganizationProvider(Header.Consignee) : null;
		public INCTSOrganization Carrier => Header.MovementHeader.Carrier.Address != null ? new NCTSOrganizationProvider(Header.MovementHeader.Carrier) : null;

		#endregion

		public string CustomsOfficeDepartureCode => GetCustomsOffices(Header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		public string CustomsOfficeDestinationCode => GetCustomsOffices(Header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		public string SignatureStaffName => GlbStaff.CurrentUser.GS_FullName;
		public string BondCodeType => NctsGuarantee?.PW_BondType ?? string.Empty;
		public string BondDescription => NctsGuarantee?.PW_BondNumber ?? string.Empty;
		public string BondPinCode => NctsGuarantee?.PW_Password ?? string.Empty;
		public string BondCurrencyCode => Core.Constants.CurrencyCodes.Turkey;
		public decimal BondAmount => NctsGuarantee?.PW_BondAmount.RoundAmount() ?? decimal.Zero;

		public IReadOnlyCollection<INCTSGoodsItems> GoodsItems => fGoodsItems = fGoodsItems ?? MovementHeader.GoodsItems.Select(goodsItems => new NCTSGoodsItemsProvider(goodsItems)).ToArray();
		IReadOnlyCollection<INCTSGoodsItems> fGoodsItems;

		public IReadOnlyCollection<INCTSManifestToOpen> ManifestToOpen => fManifestToOpen = fManifestToOpen ?? Header.ManifestsToOpenList.Select(manifestToOpen => new NCTSManifestsToOpenProvider(manifestToOpen)).ToArray();
		IReadOnlyCollection<INCTSManifestToOpen> fManifestToOpen;

		public IReadOnlyCollection<INCTSWarehouseToOpen> WarehouseToOpen
		{
			get
			{
				if (fWarehouseToOpen == null)
				{
					fWarehouseToOpen = new List<INCTSWarehouseToOpen>();

					foreach (NctsDepartureCargoDesc item in MovementHeader.GoodsItems.OrderBy(x => x.BY_LineNo))
					{
						fWarehouseToOpen.AddRange(item?.PreviousDocuments.Where(x => x.CSI_Code == NCTSMessageProviderConstants.PreviousDocuments.WarehouseCode).Select(warehouseToOpen => new NCTSWarehouseToOpenProvider(warehouseToOpen)));
					}
				}

				return fWarehouseToOpen;
			}
		}
		List<INCTSWarehouseToOpen> fWarehouseToOpen;

		#region BS

		public string DeclarantName => GlbCompany?.GC_Name.SubstringSafe(0, 35) ?? ZString.Empty;

		public string DeclarantAddress
		{
			get
			{
				ZString addresses = string.Join(" ", GlbCompany?.Address1.TrimEnd(), GlbCompany?.Address2.TrimEnd());
				return addresses.SubstringSafe(0, 35);
			}
		}

		public string DeclarantCity => GlbCompany?.GC_City ?? ZString.Empty;
		public string DeclarantVATID => GlbCompany?.GC_BusinessRegNo ?? ZString.Empty;
		public string ConsultantTaxNo => GlbCompany?.GC_BusinessRegNo ?? ZString.Empty;
		public string ConsultantName => GlbCompany?.GC_Name.SubstringSafe(0, 35) ?? ZString.Empty;

		#endregion

		#region Local Method

		ZString GetCustomsOffices(NctsHeader header, ZString euOfficeCodesTypes)
		{
			var customsOffices = header.IsPhase5 ? header.CommonMovementHeader.CustomsOffices : header.CustomsOffices;
			var officeCode = customsOffices?.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == euOfficeCodesTypes)?.CY_Data.ToString() ?? string.Empty;
			return officeCode;
		}

		#endregion
	}

	public class TRNctsMessageProviderHelper : Integration.Customs.TR.INctsHeaderProvider
	{
		public Integration.Customs.TR.INctsMessageProvider GetHeaderProvider(Integration.Customs.TR.ICusInBondHeader header)
		{
			return new NCTSHeaderProvider(header);
		}
	}
}
