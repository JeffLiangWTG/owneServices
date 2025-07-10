using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	public class MockExportAWBHeader : ExportAWBHeader
	{
		public MockExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fAWBType = ExportAWBHeader.TypeOfAWB.AgentMaster;
		}

		public override IAWBParent Parent
		{
			get { return Consol; }
		}

		protected override AWBActions GetAWBActions() => awbActions;
		AWBActions awbActions;

		public void SetAWBActions(AWBActions awbActions)
		{
			this.awbActions = awbActions;
		}

		protected override ZString MessageForReplaceMacrosFailed => "MAWB cannot be generated.";

		public override bool ShouldPopulateSlacLine(CommonContainer uldContainer)
		{
			return base.ShouldPopulateSlacLine(uldContainer) || OverrideShouldPopulateScacLine;
		}

		public bool OverrideShouldPopulateScacLine { get; set; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EH_Table = DummyBizoSchema.Constants.TableName;
		}

		protected override bool IsImportToCountry(ZString countryCode) => false;

		protected override bool IsExportFromCountry(ZString countryCode) => false;

		protected override bool IsTransitingThrough(ZString countryCode) => false;

		protected override bool ShouldPopulate
		{
			get { return true; }
		}

		#region Address Overrides

		#region Shipper Address

		protected override JobDocAddress ShipperDocumentaryAddress
		{
			get { return GetDocumentaryAddress(); }
		}

		protected override List<OrgAddress> GetShipperAddresses()
		{
			return GetAddresses();
		}

		protected override OrgHeader Shipper
		{
			get { return Organisation; }
		}

		protected override OrgAddress ShipperOfficeAddress
		{
			get { return Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Office, false); }
		}

		protected override OrgAddress ShipperPickupAddress
		{
			get { return Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false); }
		}

		protected override ZString DefaultShipperCompanyName
		{
			get { return null; }
		}

		protected override bool ShowExportStatementSetting(ExportStatementSetting mandatorySetting)
		{
			return true;
		}

		#endregion

		#region Consignee Address

		protected override OrgHeader Consignee
		{
			get { return Organisation; }
		}

		protected override JobDocAddress ConsigneeDocumentaryAddress
		{
			get { return GetDocumentaryAddress(); }
		}

		protected override List<OrgAddress> GetConsigneeAddresses()
		{
			return GetAddresses();
		}

		protected override OrgAddress ConsigneeOfficeAddress
		{
			get { return Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Office, false); }
		}

		protected override OrgAddress ConsigneeDeliveryAddress
		{
			get { return Organisation.Addresses.DefaultAddressOfType(OrgAddressType.Delivery, false); }
		}

		protected override ZString DefaultConsigneeCompanyName
		{
			get { return null; }
		}

		protected override ZString ConsigneeAccount
		{
			get { return "ACCT"; }
		}

		#endregion

		#region Also Notify Address

		internal protected override JobDocAddress NotifyPartyDocumentaryAddress
		{
			get { return GetDocumentaryAddress(); }
		}

		protected override List<OrgAddress> GetAlsoNotifyAddresses()
		{
			return GetAddresses();
		}

		#endregion

		#region Address Common

		JobDocAddress GetDocumentaryAddress()
		{
			JobDocAddress result = Factory.New<JobDocAddress>();

			result.E2_OA_Address = Organisation.Addresses.DefaultAddressOfType(OrgAddressType.PickupAndDelivery, false).PK;
			result.Address.OA_OH = Organisation.PK;
			result.E2_Contact = "CONTACT NAME";
			return result;
		}

		List<OrgAddress> GetAddresses()
		{
			List<OrgAddress> result = new List<OrgAddress>();

			AddAddresses(result);

			OrgAddress orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
			result.Add(orgAddress);

			return result;
		}

		void AddAddresses(IList addressList)
		{
			OrgAddress orgAddress = addressList.Count == 1 ? (OrgAddress)addressList[0] : Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "COMPANYNAME";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.OA_Address1 = "ADDRESSOFC";
			orgAddress.OA_Address2 = "ADDRESSOFC2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2006";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96(3)766";
			if (orgAddress.AddressCapability.IsEmpty)
			{
				orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			}

			addressList.Add(orgAddress);

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "ADDRESSPIC";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Pickup);

			addressList.Add(orgAddress);

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "ADDRESSDLV";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);

			addressList.Add(orgAddress);

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "CONAMEDOC";
			orgAddress.OA_OH = Organisation.PK;
			orgAddress.OA_Address1 = "ADDRESSPAD";
			orgAddress.OA_Address2 = "ADDRESSPAD2";
			orgAddress.OA_City = "SYDNEY";
			orgAddress.OA_PostCode = "1005";
			orgAddress.OA_RL_NKRelatedPortCode = "SGSIN";
			orgAddress.OA_State = "VICTORIA";
			orgAddress.OA_Phone = "+96(1)723";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			orgAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);

			addressList.Add(orgAddress);
		}

		public OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.OH_Code = "ACCT";
					AddAddresses(fOrganisation.Addresses);
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;

		#endregion

		#endregion

		protected override ZString AsAgreed1st
		{
			get { return Core.Constants.AWB.AsAgreedTypes.Codes.None; }
		}

		protected override ZString AsAgreed2nd
		{
			get { return Core.Constants.AWB.AsAgreedTypes.Codes.None; }
		}

		protected override Forwarding.AWB.Business.ExportAWBOtherChargesCollection GetNewAWBOtherCharges()
		{
			return new MockCollection(this);
		}

		class MockCollection : ExportAWBOtherChargesCollection
		{
			public MockCollection(MockExportAWBHeader master) : base(master) { }

			public override int MaxOtherChargesThatCouldFitOnPrintedAWB
			{
				get { return ((MockExportAWBHeader)Master).MaxOtherCharges; }
			}
		}

		public int MaxOtherCharges
		{
			get;
			set;
		}

		public override ZString AWBRatelineOvertypedNotes
		{
			get { return ""; }
		}

		protected override string VolumeAndDimensionsPrintOption
		{
			get { return string.Empty; }
		}

		public override TypeOfAWB AWBType
		{
			get { return fAWBType; }
		}

		public void OverrideAWBType(TypeOfAWB newAWBType)
		{
			fAWBType = newAWBType;
		}
		TypeOfAWB fAWBType;

		protected override ZString BillNumber
		{
			get { return ""; }
		}

		protected override CodeDescriptionPairListRegistryItem ExtraAccountingInfo
		{
			get { return FreightDataRegistry.Instance.MAWBAccountingInfoExtraText; }
		}

		protected override StringRegistryItem ExtraNatureAndQtyOfGoods
		{
			get { return FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText; }
		}

		protected override ZString ChargesCode
		{
			get { return ""; }
		}

		public override ForwardingConsol Consol
		{
			get { return consol; }
		}

		ForwardingConsol consol;

		public void SetConsol(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public IEnumerable<CommonContainer> ULDContainers_Exposed { get; set; }

		public override IEnumerable<CommonContainer> ULDContainers
		{
			get { return ULDContainers_Exposed ?? base.ULDContainers; }
		}

		protected override ZDecimal CustomsValue
		{
			get { return 0M; }
		}

		protected override ZString CustomsValueCurrency
		{
			get { return ""; }
		}

		protected override ZDecimal InsuranceValue
		{
			get { return 0M; }
		}

		protected override ZString InsuranceValueCurrency
		{
			get { return ""; }
		}

		protected override RefUNLOCO DestinationLOCO
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Destination); }
		}

		protected override ZString CustomsEntryNumber
		{
			get { return ""; }
		}

		public ZString Destination { get; set; }

		public override ZDecimal EH_TotalWeightPPD
		{
			get { return ResultEH_TotalWeightPPD != 0m ? ResultEH_TotalWeightPPD : base.EH_TotalWeightPPD; }
		}
		public ZDecimal ResultEH_TotalWeightPPD;

		public override ZDecimal EH_TotalWeightCOL
		{
			get { return ResultEH_TotalWeightCOL != 0m ? ResultEH_TotalWeightCOL : base.EH_TotalWeightCOL; }
		}
		public ZDecimal ResultEH_TotalWeightCOL;

		public override ZDecimal EH_OtherChargesDueAgentCOL
		{
			get { return ResultEH_OtherChargesDueAgentCOL; }
		}
		public ZDecimal ResultEH_OtherChargesDueAgentCOL;

		public override ZDecimal EH_OtherChargesDueAgentPPD
		{
			get { return ResultEH_OtherChargesDueAgentPPD; }
		}
		public ZDecimal ResultEH_OtherChargesDueAgentPPD;

		public override ZDecimal EH_OtherChargesDueCarrierCOL
		{
			get { return ResultEH_OtherChargesDueCarrierCOL; }
		}
		public ZDecimal ResultEH_OtherChargesDueCarrierCOL;

		public override ZDecimal EH_OtherChargesDueCarrierPPD
		{
			get { return ResultEH_OtherChargesDueCarrierPPD; }
		}
		public ZDecimal ResultEH_OtherChargesDueCarrierPPD;

		protected override ZString HandlingInformation
		{
			get { return ""; }
		}

		public override ZString GoodsDescription
		{
			get { return ""; }
		}

		protected override ZString OptionalShippingInformation1
		{
			get { return ""; }
		}

		protected override ZString OptionalShippingInformation2
		{
			get { return ""; }
		}

		protected override ZString OriginCode
		{
			get { return ZString.Empty; }
		}

		protected override ZString AirportOfDeparture
		{
			get { return ZString.Empty; }
		}

		public override GlbBranch DeparturePortRelatedBranch
		{
			get { return null; }
		}

		protected override RefUNLOCO OriginLOCO
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Origin); }
		}

		protected override ZString OtherPPDCOL
		{
			get { return Constants.PrepaidCollect3CharCodes.Prepaid; }
		}

		public override ZString WeightVPPDCOL
		{
			get { return Constants.PrepaidCollect3CharCodes.Prepaid; }
		}

		protected override ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet => false;

		public ZString Origin { get; set; }
		public ZBool TestOverrideWayBillDefaults;
		protected override ZBool OverrideWaybillDefaults
		{
			get { return TestOverrideWayBillDefaults; }
		}

		protected override ZDecimal RateLineChargeableWeight
		{
			get { return 0M; }
		}

		protected override ZDecimal RateLineGrossWeight
		{
			get { return 0M; }
		}

		protected override ZString RateLineNoPieces
		{
			get { return "0"; }
		}

		protected override ZDecimal RateLineRateChargeOrDiscount
		{
			get { return 0M; }
		}

		protected override ZString RateLineWeightUnit
		{
			get { return "K"; }
		}

		protected override ZString RateLineGrossWeightUnit
		{
			get { return Core.Constants.Weight.Kilograms; }
		}

		protected override ZString ReferenceNumber
		{
			get { return ""; }
		}

		protected override ZInt ShippingLoadAndCount
		{
			get { return 0; }
		}

		protected override ZBool IsDomestic
		{
			get { return ZBool.True; }
		}

		Transport fDepartureFlight1;
		protected override Transport DepartureFlight1
		{
			get
			{
				if (fDepartureFlight1 == null)
				{
					fDepartureFlight1 = Factory.New<CommonConsol>().Transports[0];
					fDepartureFlight1.JW_VoyageFlight = "TST001";
					fDepartureFlight1.JW_ETD = new ZDateTime(2005, 1, 1);
				}
				return fDepartureFlight1;
			}
		}

		Transport fDepartureFlight2;
		protected override Transport DepartureFlight2
		{
			get
			{
				if (fDepartureFlight2 == null)
				{
					fDepartureFlight2 = Factory.New<CommonConsol>().Transports[0];
					fDepartureFlight2.JW_VoyageFlight = "TST002";
					fDepartureFlight2.JW_ETD = new ZDateTime(2005, 2, 2);
				}
				return fDepartureFlight2;
			}
		}

		protected override ZString ExtraCarrierInfoLine2
		{
			get { return ""; }
		}

		protected override ZString ExtraShipperInfoLine1
		{
			get { return ""; }
		}

		protected override ZString ExtraShipperInfoLine2
		{
			get { return ""; }
		}

		protected override void SetVOLNatureAndQtyOfGoods()
		{
		}

		protected override void SetPKSNatureAndQtyOfGoods()
		{
		}

		protected override void SetDEFNatureAndQtyOfGoods()
		{
		}

		protected override void SetALLNatureAndQtyOfGoods()
		{
		}

		protected override void SetNDANatureAndQtyOfGoods()
		{
		}

		public override ZString RegistrationNumber
		{
			get { return RegistrationNumberForTesting; }
		}

		public override ZString ExtraShipperData
		{
			get { return ExtraShipperDataForTesting; }
		}

		public override ZString SpecialHandlingCode
		{
			get { return ""; }
		}

		public new void SetIssuedByAddress(BillIssuedBy issuedBy)
		{
			base.SetIssuedByAddress(issuedBy);
		}

		public new bool WillFitInFreeSpace(int dimensionsLinesCount)
		{
			return base.WillFitInFreeSpace(dimensionsLinesCount);
		}

		public new string GetDimensionText(PackLine packLine)
		{
			return base.GetDimensionText(packLine);
		}

		protected override IEnumerable<OtherChargeTemplate> GetOtherChargeTemplates()
		{
			return templates;
		}

		protected override bool GroupChargesByIATACode
		{
			get
			{
				return AWBType == TypeOfAWB.House
								? ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.Value
								: ExportAWBRegistry.Instance.MAWBGroupOtherChargesByIATACode.Value;
			}
		}

		IEnumerable<OtherChargeTemplate> templates = new List<OtherChargeTemplate>();

		public ZString RegistrationNumberForTesting { get; set; }

		public ZString ExtraShipperDataForTesting { get; set; }

		public ZString ShipperTraderTypeWithNoForTesting
		{
			get { return ShipperTraderTypeWithNo; }
		}

		public ZString ConsigneeTraderTypeWithNoForTesting
		{
			get { return ConsigneeTraderTypeWithNo; }
		}

		public ZString AlsoNotifyTraderTypeWithNoForTesting
		{
			get { return AlsoNotifyTraderTypeWithNo; }
		}

		public void SetTemplates(List<OtherChargeTemplate> templates)
		{
			this.templates = templates;
		}

		public new void PopulateOtherCharges()
		{
			base.PopulateOtherCharges();
		}

		string _supportedTaxDocumentType = mawbDocumentType;

		protected override string SupportedTaxDocumentType => _supportedTaxDocumentType;

		public void SetSupportedTaxDocumentType(string documentType)
		{
			_supportedTaxDocumentType = documentType;
		}

		protected override void PopulateSpecialHandlingItems()
		{
		}

		protected override IEnumerable<ZString> GetDGCodesFromParentBO()
		{
			return Enumerable.Empty<ZString>();
		}

		protected override BillIssuedBy GetNewIssuedBy()
		{
			return new BillIssuedBy((OrgHeader)null);
		}

		public new void AddExtraText(string macro, ref ZString input, IRegistryItemInternals registryitem, string delimiter = " ")
		{
			base.AddExtraText(macro, ref input, registryitem);
		}

		public Func<IBODocDataProvider> GetNewExtraTextMacroDataProviderImplementation { get; set; }
		protected override IBODocDataProvider GetNewExtraTextMacroDataProvider()
		{
			return GetNewExtraTextMacroDataProviderImplementation != null ? GetNewExtraTextMacroDataProviderImplementation() : base.GetNewExtraTextMacroDataProvider();
		}

		public override SaveMode FactorySaveMode
		{
			get { return SaveMode.Normal; }
		}

		public override bool SecurityStatusAWBVisibility
		{
			get { return EnableSecurityStatusDisplay; }
		}

		public bool EnableSecurityStatusDisplay { get; set; }

		JobCharge[] prepaidFreightCharges;
		JobCharge[] collectFreightCharges;

		public void SetPrepaidFreightCharges(JobCharge[] charges)
		{
			prepaidFreightCharges = charges;
		}

		public void SetCollectFreightCharges(JobCharge[] charges)
		{
			collectFreightCharges = charges;
		}

		protected override FreightTaxes CalculateTotalsForTaxFromFreightCharges()
		{
			var prepaidTaxAmt = prepaidFreightCharges != null
				? prepaidFreightCharges.Sum(x => x.JR_OSSellGSTAmt_Calc)
				: 0;

			var collectTaxAmt = collectFreightCharges != null
				? collectFreightCharges.Sum(x => x.JR_OSSellGSTAmt_Calc)
				: 0;

			return new FreightTaxes(prepaidTaxAmt, collectTaxAmt);
		}

		public override StringCollectionX GetAvailableHarmonisedCodes()
		{
			return new StringCollectionX();
		}

		protected override TaxCodeInformation GetTaxCodeInformationForBangladesh(List<TaxCodeInformation> taxInfos)
		{
			return taxInfos.FirstOrDefault();
		}
	}
}
