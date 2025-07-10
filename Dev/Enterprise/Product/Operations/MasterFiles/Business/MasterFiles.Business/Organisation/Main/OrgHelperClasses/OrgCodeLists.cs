using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.MasterFiles.Business
{
	public enum OrgModuleType
	{
		Standard,
		ClientIntelligence,
		CompetitorIntelligence,
		CompanyCampaignContact
	}

	public class OrgCodeLists : IOrgCodeLists
	{
		#region AddressType_List

		public static CodeDescriptionPairList AddressType_List(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			return factory.GetCachedValue("AddressType_List",
					delegate
					{
						CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

						list.AddPair(OrgConstants.AddressType.Office, Res.GetString("MasterFiles|AddressType|Office", "Office Address"));
						list.AddPair(OrgConstants.AddressType.Postal, Res.GetString("MasterFiles|AddressType|Postal", "Postal Address"));
						list.AddPair(OrgConstants.AddressType.Receivables, Res.GetString("MasterFiles|AddressType|Receivables", "Accounts Receivable Mailing Address"));
						list.AddPair(OrgConstants.AddressType.Payables, Res.GetString("MasterFiles|AddressType|Payables", "Accounts Payable Mailing Address"));
						list.AddPair(OrgConstants.AddressType.Sales, Res.GetString("MasterFiles|AddressType|Sales", "Mailing address for Sales updates, quotes and special offers"));
						list.AddPair(OrgConstants.AddressType.Pickup, Res.GetString("MasterFiles|AddressType|Pickup", "Consignment Pickup Address"));
						list.AddPair(OrgConstants.AddressType.Delivery, Res.GetString("MasterFiles|AddressType|Delivery", "Consignment Delivery Address"));
						list.AddPair(OrgConstants.AddressType.PickupAndDelivery, Res.GetString("MasterFiles|AddressType|PickupAndDelivery", "Consignment Pickup and Delivery Address"));
						list.AddPair(OrgConstants.AddressType.Miscellaneous, Res.GetString("MasterFiles|AddressType|Miscellaneous", "Miscellaneous Address (no Automation)"));
						list.AddPair(OrgConstants.AddressType.Residential, Res.GetString("MasterFiles|AddressType|Residential", "Residential Address"));
						list.AddPair(OrgConstants.AddressType.CustomsAddressOfRecord, Res.GetString("MasterFiles|AddressType|CustomsAddressOfRecord", "Customs Address of Record"));
						list.AddPair(OrgConstants.AddressType.AWB, Res.GetString("MasterFiles|AddressType|AWB", "AWB Address"));
						list.AddPair(OrgConstants.AddressType.EUCustomsAddress, Res.GetString("MasterFiles|AddressType|EUCustomsAddress", "EU Customs Address"));

						return list;
					});
		}

		CodeDescriptionPairList IOrgCodeLists.GetAddressTypesList(BusinessObjectFactory factory)
		{
			return AddressType_List(factory);
		}

		#endregion

		#region AttachmentType_List

		public static CodeDescriptionPairList AttachmentType_List
		{
			get
			{
				return ContactAttachmentTypeList.AttachmentType_List;
			}
		}

		#endregion

		#region ContactType_List

		public static CodeDescriptionPairList ContactType_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

				list.AddPair(ContactType.All, Res.GetString("MasterFiles|ContactType|All", "All Documents"));
				list.AddPair(ContactType.Receivables, Res.GetString("MasterFiles|ContactType|Receivables", "Accounts Receivable"));
				list.AddPair(ContactType.Payables, Res.GetString("MasterFiles|ContactType|Payables", "Accounts Payable"));
				list.AddPair(ContactType.Consignor, Res.GetString("MasterFiles|ContactType|Consignor", "Consignor, Supplier or Exporter"));
				list.AddPair(ContactType.Consignee, Res.GetString("MasterFiles|ContactType|Consignee", "Consignee, Buyer or Importer"));
				list.AddPair(ContactType.TransportServices, Res.GetString("MasterFiles|ContactType|TransportServices", "Transport Services Client"));
				list.AddPair(ContactType.Warehouse, Res.GetString("MasterFiles|ContactType|Warehouse", "Warehouse, Distribution Client"));
				list.AddPair(ContactType.Sales, Res.GetString("MasterFiles|ContactType|Sales", "Sales updates, quotes, special offers"));
				list.AddPair(ContactType.Marketing, Res.GetString("MasterFiles|ContactType|Marketing", "Marketing contact for all sales information"));
				list.AddPair(ContactType.ExportFreightAgent, Res.GetString("MasterFiles|ContactType|ExportFreightAgent", "Forwarding Agent Export Freight Contact"));
				list.AddPair(ContactType.ImportFreightAgent, Res.GetString("MasterFiles|ContactType|ImportFreightAgent", "Forwarding Agent Import Freight Contact"));
				list.AddPair(ContactType.ImportSeaFreightAgent, Res.GetString("MasterFiles|ContactType|ImportSeaFreightAgent", "Forwarding Agent Import Sea Freight Contact"));
				list.AddPair(ContactType.ImportAirFreightAgent, Res.GetString("MasterFiles|ContactType|ImportAirFreightAgent", "Forwarding Agent Import Air Freight Contact"));
				list.AddPair(ContactType.ExportSeaFreightAgent, Res.GetString("MasterFiles|ContactType|ExportSeaFreightAgent", "Forwarding Agent Export Sea Freight Contact"));
				list.AddPair(ContactType.ExportAirFreightAgent, Res.GetString("MasterFiles|ContactType|ExportAirFreightAgent", "Forwarding Agent Export Air Freight Contact"));
				list.AddPair(ContactType.CTO, Res.GetString("MasterFiles|ContactType|CTO", "Container Terminal Operator"));
				list.AddPair(ContactType.Depot, Res.GetString("MasterFiles|ContactType|Depot", "Depot / Freight Station"));
				list.AddPair(ContactType.ExportDepot, Res.GetString("MasterFiles|ContactType|ExportDepot", "Export Depot / CFS"));
				list.AddPair(ContactType.ImportDepot, Res.GetString("MasterFiles|ContactType|ImportDepot", "Import Depot / CFS"));
				list.AddPair(ContactType.ExportSeaDepot, Res.GetString("MasterFiles|ContactType|ExportSeaDepot", "Export Sea Freight Depot / CFS"));
				list.AddPair(ContactType.ExportAirDepot, Res.GetString("MasterFiles|ContactType|ExportAirDepot", "Export Air Freight Depot / CFS"));
				list.AddPair(ContactType.ImportSeaDepot, Res.GetString("MasterFiles|ContactType|ImportSeaDepot", "Import Sea Freight Depot / CFS"));
				list.AddPair(ContactType.ImportAirDepot, Res.GetString("MasterFiles|ContactType|ImportAirDepot", "Import Air Freight Depot / CFS"));
				list.AddPair(ContactType.ShippingLine, Res.GetString("MasterFiles|ContactType|ShippingLine", "Carrier, Co-Load or NVOCC"));
				list.AddPair(ContactType.AirWholesaler, Res.GetString("MasterFiles|ContactType|AirWholesaler", "Airline or Airfreight Wholesaler"));
				list.AddPair(ContactType.LocalTransport, Res.GetString("MasterFiles|ContactType|LocalTransport", "Port Transport Contractor"));
				list.AddPair(ContactType.Warehouse3PL, Res.GetString("MasterFiles|ContactType|Warehouse3PL", "Warehouse Client"));
				list.AddPair(ContactType.CustomerService, Res.GetString("MasterFiles|ContactType|CustomerService", "Customer Service"));
				list.AddPair(ContactType.Administration, Res.GetString("MasterFiles|ContactType|Administration", "Administration"));
				list.AddPair(ContactType.NotifyParty, Res.GetString("MasterFiles|ContactType|NotifyParty", "Notify Party"));
				list.AddPair(ContactType.Miscellaneous, Res.GetString("MasterFiles|ContactType|Miscellaneous", "Miscellaneous Contact (no Automation)"));
				list.AddPair(ContactType.LocalClient, Res.GetString("MasterFiles|ContactType|LocalClient", "Local Client"));
				list.AddPair(ContactType.ExportBroker, Res.GetString("MasterFiles|ContactType|ExportCustomsBroker", "Export Customs Broker"));
				list.AddPair(ContactType.ImportBroker, Res.GetString("MasterFiles|ContactType|ImportCustomsBroker", "Import Customs Broker"));
				list.AddPair(ContactType.CommissionAgreementRecipient, Res.GetString("MasterFiles|ContactType|CommissionAgreementRecipient", "Commission Agreement Recipient"));
				list.AddPair(ContactType.TransitWarehouse, Res.GetString("MasterFiles|ContactType|TransitWarehouse", "Transit Warehouse Service Provider"));
				list.AddPair(ContactType.VerifiedGrossWeightContact, Res.GetString("MasterFiles|ContactType|VerifiedGrossWeightContact", "Verified Gross Weight Contact"));
				list.AddPair(ContactType.NettingParticipantStatement, Res.GetString("MasterFiles|ContactType|NettingParticipantStatement", "Netting Participant Statement"));
				list.AddPair(ContactType.NettingClearingJournal, Res.GetString("MasterFiles|ContactType|NettingClearingJournal", "Netting Clearing Journal"));
				list.AddPair(ContactType.ControllingCustomer, Res.GetString("MasterFiles|ContactType|ControllingCutomer", "Controlling Customer"));
				list.AddPair(ContactType.ControllingAgent, Res.GetString("MasterFiles|ContactType|ControllingAgent", "Controlling Agent"));
				list.AddPair(ContactType.Declarant, Res.GetString("MasterFiles|ContactType|Declarant", "Declarant"));
				list.AddPair(ContactType.Principal, Res.GetString("MasterFiles|ContactType|Principal", "Principal"));

				return OverridableContactTypeListDelegate.Value?.Invoke(list) ?? list;
			}
		}

		protected delegate CodeDescriptionPairList ContactTypeListDelegate(CodeDescriptionPairList list);
		protected static readonly Overridable<ContactTypeListDelegate> OverridableContactTypeListDelegate = new Overridable<ContactTypeListDelegate>();

		#endregion

		#region Customs-Codes List

		public CodeDescriptionPairList CustomsCodes_List(ZString countryCode)
		{
			return CustomsCodes_List(countryCode, new BusinessObjectFactory());
		}

		public CodeDescriptionPairList CustomsCodes_List(ZString countryCode, BusinessObjectFactory factory)
		{
			return CustomsCodes_List(new RefCountry.Loader(factory).LoadForCountry(countryCode));
		}

		public CodeDescriptionPairList CustomsCodes_List(RefCountry country)
		{
			var list = CustomsCodes_StaticList(country);

			if (country != null)
			{
				if (country.IsEuOrFriend)
				{
					var addEori = country.IsPartOfEuropeanUnion
						|| country.Code.In(new ZString[] { CountryCodes.UnitedKingdom, CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, CountryCodes.Vatican,
							CountryCodes.SanMarino, CountryCodes.Liechtenstein, CountryCodes.Andorra, CountryCodes.Norway, CountryCodes.Switzerland });

					var addTcuin = country.IsRecognisedByEuropeanUnionAsIssuerOfThirdCountryUniqueIdentificationNumbersOrAEO
						|| country.Code == CountryCodes.Poland;
					var addAeo = addEori || addTcuin;
					if (addEori)
					{
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Res.GetString("83768ec6-ee43-4759-a44b-856157e35715", "EORI code (to be sent verbatim)"));
						var turnTex = country.Code == CountryCodes.UnitedKingdom ? Res.GetString("CF8E067D-42C6-4EA5-9DBE-AF32DFE10D4E", "(Obsolete) EORI code (to be substituted for head office's EORI code)") : Res.GetString("929c48eb-2edc-4b5b-a274-480c2670efae", "EORI code (to be substituted for head office's EORI code)");
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, turnTex);
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, Res.GetString("1648A166-A7BD-453C-8655-18E31BA8F3D2|DefermentApprovalNumber", "Deferment Approval Number"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Res.GetString("b5056af6-744d-428e-83f5-a96dd9c3d6e3", "Trader Excise Number"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Res.GetString("05e1e7f2-a53d-4fbf-b323-8dad596eac49", "Trader ID"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo, Res.GetString("1c8dd23e-33b1-4b92-8743-168fa0df06f4", "Consignee Exempt Number"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber, Res.GetString("OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber", "Inward Processing Relief Authorization Number"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.OutwardProcessingReliefNumber, Res.GetString("OrgCusCode.EuropeanUnionSharedCodeTypes.OutwardProcessingReliefNumber", "Outward Processing Relief Authorization Number"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration, Res.GetString("OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration", "Import One Stop Shop VAT Registration"));
						list.AddPairIfNotExist(OrgCusCode.CodeTypes.CustomsOfficeForTransit, Res.GetString("OrgCusCode.CodeTypes.CustomsOfficeForTransitEU", "Customs Office For Transit"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit, Res.GetString("2597C5C0-4920-43F7-83A0-BAF75522ECFB", "Customs Office For Exit"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.TrustedTrader, Res.GetString("1C91F8E7-007B-4E3A-86E2-89D184284E80", "Trusted Trader (EU/UK) as per Articles 9-11 of Decision No 1/2023"));
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionSharedCodeTypes.UKInternalMarketSchemeCode, Res.GetString("OrgCusCode.CodeTypes.UKInternalMarketSchemeCode", "United Kingdom Internal Market Scheme Code"));
					}
					if (addAeo)
					{
						list.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, Res.GetString("72c61125-84a4-4242-a618-2aac2debbe9f", "Authorized Economic Operator"));
					}
					if (addTcuin)
					{
						list.AddPairIfNotExist(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, Res.GetString("50f54c2a-2da3-45ee-a220-e1f0bb9c8b9f", "Third Country/Region Unique Identification Number"));
					}
				}

				if (country.IsPartOfEuOrGspPlusOCT || country.RN_Code == Core.Constants.CountryCodes.UnitedKingdom)
				{
					list.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber, Res.GetString("OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber", "Registered Exporter Number"));
				}
			}

			list.SortByDescription();

			return list;
		}

		/// <summary>
		/// The list of static OrgCusCodes for the country, that don't require RefDB for conditional codes. This doesn't return the complete list, the complete list gets created in CustomsCodes_List.
		/// </summary>
		internal CodeDescriptionPairList CustomsCodes_StaticList(RefCountry country)
		{
			var list = OrgCusCodeCountryFactory.GetIOrgCusCodeProvider(country?.RN_Code).AddOrgCusCodes(GetDefaultList());

			if (country != null)
			{
				// This region is for codes that aren't for a single country, and instead are for a shared group of countries that are
				// to tedious to define in *OrgCusCodeInfo files.
				#region Country-independent common codes

				if (country.IsFranceOrTerritory)
				{
					list.AddPair(OrgCusCode.FranceCodeTypes.CI5, Res.GetString("bddff635-1d3a-4985-9adf-ca9948dfa5a8", "Ci5 Port Community System Code"));
					list.AddPair(OrgCusCode.FranceCodeTypes.SON, Res.GetString("11e516de-08c1-4da3-90bc-7c1e30bcdab9", "S)One Port Community System Code"));
					list.AddPair(OrgCusCode.FranceCodeTypes.SOA, Res.GetString("bd3be915-4eae-47ad-9c89-e251ea9522b4", "S)One Port Community System Agent Code"));
					list.AddPair(OrgCusCode.FranceCodeTypes.SOW, Res.GetString("b82bab9b-3ce5-47f9-83e6-fc71c4ed9a68", "S)One Port Community System Warehouse Code"));
				}

				#endregion
			}

			list = CAGCodeIssuer.UpdateListWithCAGCodeIfNeeded(list, country);

			return list;
		}

		CodeDescriptionPairList GetDefaultList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			OrgCusCodeListHelpers.InsertDefaultOrgCusCodes(list);
			return list;
		}

		#endregion

		#region CustomForms_List

		public static CodeDescriptionPairList CustomForms_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.CustomLabels); }
		}

		#endregion

		#region CustomDocuments_List

		public static CodeDescriptionPairList CustomDocuments_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.CustomDocuments); }
		}

		#endregion

		#region RelatedPartyFinancialDescriptions_List

		public CodeDescriptionPairList RelatedPartyFinancialDescriptions_List()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(RelatedPartyTypeList.Descriptions.APNettingGroup);
			list.AddPair(RelatedPartyTypeList.Descriptions.APSettlementGroup);
			list.AddPair(RelatedPartyTypeList.Descriptions.ARNettingGroup);
			list.AddPair(RelatedPartyTypeList.Descriptions.ARSettlementGroup);
			list.AddPair(RelatedPartyTypeList.Descriptions.LocalTransportBillTo);
			list.AddPair(RelatedPartyTypeList.Descriptions.InvoiceCustomsJobsTo);
			list.AddPair(RelatedPartyTypeList.Descriptions.InvoiceFreightJobsTo);
			list.AddPair(RelatedPartyTypeList.Descriptions.ReportRevenueTo);
			list.AddPair(RelatedPartyTypeList.Descriptions.ForwarderGroup);
			list.AddPair(RelatedPartyTypeList.Descriptions.AccountingVATGSTGroup);
			list.AddPair(RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo);

			return list;
		}

		#endregion

		#region InvoiceLineGroupings_List

		public static CodeDescriptionPairList InvoiceLineGroupings_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.None, OrgDescriptions.InvoiceLineGroupings.None);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.All, OrgDescriptions.InvoiceLineGroupings.All);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.AEC, OrgDescriptions.InvoiceLineGroupings.AEC);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.OandF, OrgDescriptions.InvoiceLineGroupings.OandF);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.ORF, OrgDescriptions.InvoiceLineGroupings.ORF);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.FRT, OrgDescriptions.InvoiceLineGroupings.FRT);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.FandD, OrgDescriptions.InvoiceLineGroupings.FandD);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.OFD, OrgDescriptions.InvoiceLineGroupings.OFD);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.OFO, OrgDescriptions.InvoiceLineGroupings.OFO);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.OFF, OrgDescriptions.InvoiceLineGroupings.OFF);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.OFI, OrgDescriptions.InvoiceLineGroupings.OFI);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CCD, OrgDescriptions.InvoiceLineGroupings.CCD);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CLC, OrgDescriptions.InvoiceLineGroupings.CLC);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CCG, OrgDescriptions.InvoiceLineGroupings.CCG);

				return list;
			}
		}

		#endregion

		#region MergeInvoiceLinesBy_List

		[ThreadStatic]
		static CodeDescriptionPairList fMergeInvoiceLinesBy_List;
		public static CodeDescriptionPairList MergeInvoiceLinesBy_List
		{
			get
			{
				if (fMergeInvoiceLinesBy_List == null || CountryHasChanged)
				{
					fMergeInvoiceLinesBy_List = GetMergeInvoiceLinesByListByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return fMergeInvoiceLinesBy_List;
			}
		}

		public static CodeDescriptionPairList GetMergeInvoiceLinesByListByCountryCode(ZString countryCode)
		{
			var mergeInvoiceLinesBy_List = GetMergeByListByCountryCode(countryCode);
			mergeInvoiceLinesBy_List.AddPair(OrgConstants.MergeInvoiceLines.Default, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|Default", "Functions based on the system registry set-up"));
			if (countryCode == Core.Constants.CountryCodes.Canada)
			{
				if (!mergeInvoiceLinesBy_List.ContainsCode(OrgConstants.MergeInvoiceLines.TariffAndMultiInvoices))
				{
					mergeInvoiceLinesBy_List.AddPair(OrgConstants.MergeInvoiceLines.TariffAndMultiInvoices, ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|TariffAndMultiInvoices", "(CA ONLY) Classification/Tariff over multiple invoices"));
				}
			}
			return mergeInvoiceLinesBy_List;
		}

		public static CodeDescriptionPairList GetMergeByListByCountryCode(ZString countryCode)
		{
			var retriever = (IMergeByListReflectionRetriever)ObjectFactory.Get("IMergeByListReflectionRetriever");
			var list = (CodeDescriptionPairList)retriever.GetMergeByListByCountryCode(countryCode);
			if (list == null || list.Count <= 0)
			{
				list = new CodeDescriptionPairList(OLookUpEditType.CommercialInvoiceMergeMethod);
			}
			return list;
		}

		#endregion

		[ThreadStatic]
		static ZString previousCountryCode;
		public static ZString PreviousCountryCode
		{
			get
			{
				if (previousCountryCode.IsEmpty)
				{
					previousCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				return previousCountryCode;
			}
		}

		static bool CountryHasChanged
		{
			get
			{
				if (PreviousCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					previousCountryCode = ZString.Empty;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		#region PaymentMethod_List
		public static CodeDescriptionPairList PaymentMethod_List
		{
			get
			{
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.SouthAfrica:
						return new Customs.PaidByCodeList();
					case Core.Constants.CountryCodes.Singapore:
						return new Customs.Asycuda.SGPayeeIndicatorList();
					default:
						return new PaymentMethodList();
				}
			}
		}
		#endregion

		#region SendImportDocsTo_List

		public static CodeDescriptionPairList SendImportDocsTo_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);

				list.AddPair(OrgConstants.SendDocsTo.Importer, Res.GetString("MasterFiles|SendImportDocsToList|Importer", "Send Import Documents to the Importer."));
				list.AddPair(OrgConstants.SendDocsTo.Broker, Res.GetString("MasterFiles|SendImportDocsToList|Broker", "Send Import Documents to the Broker."));
				list.AddPair(OrgConstants.SendDocsTo.Both, Res.GetString("MasterFiles|SendImportDocsToList|Both", "Send to both the Importer and the Broker."));

				return list;
			}
		}

		#endregion

		#region State_List

		public CodeDescriptionPairList State_List(RefUNLOCO uNLOCO)
		{
			if (uNLOCO != null && uNLOCO.Country != null)
			{
				State_List(uNLOCO.Factory, uNLOCO.Country.RN_Code);
			}
			return InternalState_List;
		}

		public CodeDescriptionPairList State_List(RefCountry country)
		{
			if (country != null)
			{
				State_List(country.Factory, country.RN_Code);
			}
			return InternalState_List;
		}

		public CodeDescriptionPairList State_List(BusinessObjectFactory factory, ZString countryCode)
		{
			if (!string.IsNullOrEmpty(countryCode))
			{
				if (fPreviousCountryCode != countryCode)
				{
					InternalState_List.Clear();
					var filter = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
					filter.AddToFilter(JoinCondition.And, RefCountryStatesSchema.RW_IsActive, true);
					filter.OrderBy = RefCountryStates.Schema.RW_Code;
					BusinessObject[] states = factory.Load(typeof(RefCountryStates), filter);
					InternalState_List.AddRange(states);
					fPreviousCountryCode = countryCode;
				}
			}
			return InternalState_List;
		}
		ZString fPreviousCountryCode = "";

		protected CodeDescriptionPairList InternalState_List
		{
			get
			{
				if (fInternalState_List == null)
				{
					fInternalState_List = new CodeDescriptionPairList(OLookUpEditType.CustomType);
				}
				return fInternalState_List;
			}
		}
		protected CodeDescriptionPairList fInternalState_List;

		#endregion

		#region Company Tariff Lists

		public CodeDescriptionPairList CompanyTariffTypes_List(BusinessObjectFactory factory, ZGuid companyPK)
		{
			if (fCompanyTariffTypes == null)
			{
				BusinessObject[] ratingHeaders = GetCompanyRatingHeaders(factory, companyPK);

				fCompanyTariffTypes = new CodeDescriptionPairList();
				fCompanyTariffTypes.AddPair(OrgRateTariffLevel.DefaultTariffType, Res.GetString("Organisation.TariffLevel.DefaultTariffType", "Default for All Job Types"));
				if (ratingHeaders.Length > 0)
				{
					fCompanyTariffTypes.AddRange(((ICompanyTariff)ratingHeaders[0]).CompanyTariffTypes);
				}

				fCompanyTariffLevels = new CodeDescriptionPairList();

				if (ratingHeaders.Cast<ICompanyTariff>().Any(x => x.TH_GC == ZGuid.Empty))
				{
					fCompanyTariffLevels.AddPair("0", Res.GetString("dbd3c9a6-205c-4204-b2a2-2897eccead8c", "Do not use Global or Company Tariff"));
				}
				else
				{
					fCompanyTariffLevels.AddPair("0", Res.GetString("Organisation.TariffLevel.DoNotUse", "Do not use Company Tariff"));
				}

				foreach (ICompanyTariff tariff in ratingHeaders)
				{
					if (!fCompanyTariffLevels.ContainsCode(tariff.TH_GlobalRateLevel))
					{
						var description = ratingHeaders.Cast<ICompanyTariff>().Any(x => x.TH_GlobalRateLevel == tariff.TH_GlobalRateLevel && x.TH_GC != tariff.TH_GC) ? Res.GetString("db413d64-44c4-4e13-83df-39f6aa55e75a", "Global or Company Tariff Level {0}", tariff.TH_GlobalRateLevel) : tariff.TH_GlobalRateDescriptionMultilingual;
						fCompanyTariffLevels.AddPair(tariff.TH_GlobalRateLevel.ToString(), description);
					}
				}
			}

			return fCompanyTariffTypes;
		}

		public CodeDescriptionPairList GetCompanyTransportModes(BusinessObjectFactory factory, ZGuid companyPK, ZString tariffType)
		{
			ICompanyTariff tariff = GetCompanyRatingHeaders(factory, companyPK).FirstOrDefault() as ICompanyTariff;

			return tariff != null
					? tariff.GetCompanyTransportModes(tariffType)
					: new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList GetApplicableDirections(BusinessObjectFactory factory, ZGuid companyPK, ZString tariffType)
		{
			ICompanyTariff tariff = GetCompanyRatingHeaders(factory, companyPK).FirstOrDefault() as ICompanyTariff;

			return tariff != null
					? tariff.GetApplicableDirections(tariffType)
					: new CodeDescriptionPairList();
		}

		public BusinessObject[] GetCompanyRatingHeaders(BusinessObjectFactory factory, ZGuid companyPK)
		{
			Type ratingHeaderType = ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RatingHeaderType;
			ZQuery filter = new ZQuery(RatingHeaderSchema.TH_GC, companyPK);
			filter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, null);
			filter.AddToFilter(RatingHeaderSchema.TH_RateType, "GLB");

			BusinessObject[] ratingHeaders = factory.Load(ratingHeaderType, filter);
			Array.Sort(ratingHeaders, new CompanyTariffComparer());

			return ratingHeaders;
		}

		public CodeDescriptionPairList CompanyTariffLevels_List(BusinessObjectFactory factory, ZGuid companyPK)
		{
			if (CompanyTariffTypes_List(factory, companyPK) != null)
			{
				return fCompanyTariffLevels;
			}
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList DefaultCompanyTariffLevels_List(BusinessObjectFactory factory)
		{
			return CompanyTariffLevels_List(factory, GlbCompany.CurrentCompany.PK);
		}

		class CompanyTariffComparer : IComparer<ICompanyTariff>, IComparer
		{
			public int Compare(ICompanyTariff x, ICompanyTariff y)
			{
				return x.TH_GlobalRateLevel.CompareTo(y.TH_GlobalRateLevel);
			}

			public int Compare(object x, object y)
			{
				return Compare((ICompanyTariff)x, (ICompanyTariff)y);
			}
		}

		CodeDescriptionPairList fCompanyTariffTypes;
		CodeDescriptionPairList fCompanyTariffLevels;

		#endregion

		#region ZeroToTen_List

		public static CodeDescriptionPairList ZeroToTen_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
				list.AddPair("0");
				list.AddPair("1");
				list.AddPair("2");
				list.AddPair("3");
				list.AddPair("4");
				list.AddPair("5");
				list.AddPair("6");
				list.AddPair("7");
				list.AddPair("8");
				list.AddPair("9");
				list.AddPair("10");
				return list;
			}
		}

		#endregion
	}
}
