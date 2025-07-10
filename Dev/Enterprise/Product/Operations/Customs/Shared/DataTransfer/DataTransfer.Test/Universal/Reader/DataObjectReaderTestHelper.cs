using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public class DataObjectReaderTestHelper : OrganizationAddressTestHelper
	{
		public DataObjectReaderTestHelper()
		{
		}

		public DataObjectReaderTestHelper(UniversalObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected TestErrorLogger logger;

		public GlbCompany CurrentCompany
		{
			get { return currentCompany ?? (currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK)); }
		}
		GlbCompany currentCompany;

		public GlbBranch CurrentCompanySecondBranch
		{
			get
			{
				if (currentCompanySecondBranch == null)
				{
					currentCompanySecondBranch = CurrentCompany.Branches.AddNew();
					currentCompanySecondBranch.GB_BranchName = "2ND BRANCH";
					currentCompanySecondBranch.GB_Code = "Z$2";
					currentCompanySecondBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}
				return currentCompanySecondBranch;
			}
		}
		GlbBranch currentCompanySecondBranch;

		public Currency LocalCurrency
		{
			get { return Currency.New(BaseJobComInvoiceHeader.GetLocalCurrencyFor(null)); }
		}

		public Currency ForeignCurrency
		{
			get { return Currency.New(ForeignCurrencyBO); }
		}

		public RefCurrency ForeignCurrencyBO
		{
			get
			{
				if (foreignCurrencyBO == null)
				{
					foreignCurrencyBO = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, CurrentCompany.Country.RN_RX_NKLocalCurrency));
				}
				return foreignCurrencyBO;
			}
		}
		RefCurrency foreignCurrencyBO;

		public ChargeLine GetChargeLine(ZString? branchCode, ZString? chargeCode, ZString? costAPInvoiceNumber, ZDateTime? costDueDate, ZString? costGSTVATID, ZDateTime? costInvoiceDate,
				ZDecimal? costLocalAmount, ZDecimal? costOSAmount, ZString? costOSCurrency, ZDecimal? costOSGSTVATAmount, ZString? creditor, ZString? debtor, ZString? departmentCode,
				ZString? description, ZShort? displaySequence, ZString? sellGSTVATID, ZString? sellInvoiceType, ZDecimal? sellLocalAmount, ZDecimal? sellOSAmount,
				ZString? sellOSCurrency, ZDecimal? sellOSGSTVATAmount)
		{
			ChargeLine chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);

			if (branchCode.HasValue)
			{
				chargeLine.Branch = new Branch();
				chargeLine.Branch.Code = branchCode;
				chargeLine.Branch.Name = "EDIHQ";
			}

			if (chargeCode.HasValue)
			{
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = chargeCode;
				chargeLine.ChargeCode.Description = "International Freight";
			}

			chargeLine.CostAPInvoiceNumber = costAPInvoiceNumber;
			chargeLine.CostDueDate = costDueDate;

			if (costGSTVATID.HasValue)
			{
				chargeLine.CostGSTVATID = new TaxID();
				chargeLine.CostGSTVATID.TaxCode = costGSTVATID;
				chargeLine.CostGSTVATID.Description = "Cost Tax Description";
			}

			chargeLine.CostInvoiceDate = costInvoiceDate;
			chargeLine.CostLocalAmount = costLocalAmount;
			chargeLine.CostOSAmount = costOSAmount;

			if (costOSCurrency.HasValue)
			{
				chargeLine.CostOSCurrency = new Currency();
				chargeLine.CostOSCurrency.Code = costOSCurrency;
				chargeLine.CostOSCurrency.Description = "Cost Currency Description";
			}

			chargeLine.CostOSGSTVATAmount = costOSGSTVATAmount;

			if (creditor.HasValue)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = creditor;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (debtor.HasValue)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = debtor;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
			}

			if (departmentCode.HasValue)
			{
				chargeLine.Department = new Department();
				chargeLine.Department.Code = departmentCode;
				chargeLine.Department.Name = "Test Department";
			}

			chargeLine.Description = description;
			chargeLine.DisplaySequence = displaySequence;

			if (sellGSTVATID.HasValue)
			{
				chargeLine.SellGSTVATID = new TaxID();
				chargeLine.SellGSTVATID.TaxCode = sellGSTVATID;
				chargeLine.SellGSTVATID.Description = "Sell Tax Description";
			}

			chargeLine.SellInvoiceType = sellInvoiceType;
			chargeLine.SellLocalAmount = sellLocalAmount;
			chargeLine.SellOSAmount = sellOSAmount;

			if (sellOSCurrency.HasValue)
			{
				chargeLine.SellOSCurrency = new Currency();
				chargeLine.SellOSCurrency.Code = sellOSCurrency;
				chargeLine.SellOSCurrency.Description = "Sell Currency Description";
			}

			chargeLine.SellOSGSTVATAmount = sellOSGSTVATAmount;

			return chargeLine;
		}

		public void AssertCharge(JobCharge charge, ChargeLine chargeLine)
		{
			if (chargeLine.Branch != null && chargeLine.Branch.Code.HasValue)
			{
				AssertNotNull("charge.Branch should not be null", charge.Branch);
				AssertEquals("Branch should be equal", chargeLine.Branch.Code, charge.Branch.GB_Code);
			}

			if (chargeLine.ChargeCode != null && chargeLine.ChargeCode.Code.HasValue)
			{
				AssertNotNull("charge.ChargeCode should not be null", charge.ChargeCode);
				AssertEquals("ChargeCode should be equal", chargeLine.ChargeCode.Code, charge.ChargeCode.AC_Code);
			}

			if (chargeLine.CostAPInvoiceNumber.HasValue)
			{
				AssertEquals("APInvoiceNum should be equal", chargeLine.CostAPInvoiceNumber, charge.JR_APInvoiceNum);
			}

			if (chargeLine.CostDueDate.HasValue)
			{
				AssertEquals("CostDueDate should be equal", chargeLine.CostDueDate, charge.JR_PaymentDate);
			}

			if (chargeLine.CostGSTVATID != null && chargeLine.CostGSTVATID.TaxCode.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", charge.CostGSTRate);
				AssertEquals("CostGSTRate should be equal", chargeLine.CostGSTVATID.TaxCode, charge.CostGSTRate.AT_Code);
			}

			if (chargeLine.CostInvoiceDate.HasValue)
			{
				AssertEquals("CostInvoiceDate should be equal", chargeLine.CostInvoiceDate, charge.JR_APInvoiceDate);
			}

			if (chargeLine.CostLocalAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostLocalAmount, charge.JR_LocalCostAmt);
			}

			if (chargeLine.CostOSAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostOSAmount, charge.JR_OSCostAmt);
			}

			if (chargeLine.CostOSCurrency != null && chargeLine.CostOSCurrency.Code.HasValue)
			{
				AssertEquals("Cost Currency should be equal", chargeLine.CostOSCurrency.Code, charge.JR_RX_NKCostCurrency);
			}

			if (chargeLine.CostOSGSTVATAmount.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertEquals("Cost OS GST VAT Amount should be equal", chargeLine.CostOSGSTVATAmount, charge.JR_OSCostGSTAmt_Calc);
			}

			if (chargeLine.Creditor != null && chargeLine.Creditor.Key.HasValue)
			{
				AssertNotNull("CostAccount should not be null", charge.CostAccount);
				AssertEquals("Creditor should be equal", chargeLine.Creditor.Key, charge.CostAccount.OH_Code);
			}

			if (chargeLine.Debtor != null && chargeLine.Debtor.Key.HasValue)
			{
				AssertNotNull("SellAccount should not be null", charge.SellAccount);
				AssertEquals("Debtor should be equal", chargeLine.Debtor.Key, charge.SellAccount.OH_Code);
			}

			if (chargeLine.Department != null && chargeLine.Department.Code.HasValue)
			{
				AssertNotNull("Department should not be null", charge.Department);
				AssertEquals("Department should be equal", chargeLine.Department.Code, charge.Department.GE_Code);
			}

			if (chargeLine.Description.HasValue)
			{
				AssertEquals("Description should be equal", chargeLine.Description, charge.JR_Desc);
			}

			if (chargeLine.DisplaySequence.HasValue)
			{
				AssertEquals("DisplaySequence should be equal", chargeLine.DisplaySequence, charge.JR_DisplaySequence);
			}

			if (chargeLine.SellGSTVATID != null && chargeLine.SellGSTVATID.TaxCode.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotNull("SellGSTRate should not be null", charge.SellGSTRate);
				AssertEquals("SellGSTRate should be equal", chargeLine.SellGSTVATID.TaxCode, charge.SellGSTRate.AT_Code);
			}

			if (chargeLine.SellInvoiceType.HasValue)
			{
				AssertEquals("InvoiceType should be equal", chargeLine.SellInvoiceType, charge.JR_InvoiceType);
			}

			if (chargeLine.SellLocalAmount.HasValue)
			{
				AssertEquals("Sell Local Amount should be equal", chargeLine.SellLocalAmount, charge.JR_LocalSellAmt);
			}

			if (chargeLine.SellOSAmount.HasValue)
			{
				AssertEquals("Sell OS Amount should be equal", chargeLine.SellOSAmount, charge.JR_OSSellAmt);
			}

			if (chargeLine.SellOSCurrency != null && chargeLine.SellOSCurrency.Code.HasValue)
			{
				AssertEquals("Sell OS Currency should be equal", chargeLine.SellOSCurrency.Code, charge.JR_RX_NKSellCurrency);
			}

			if (chargeLine.SellOSGSTVATAmount.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertEquals("SellOSGSTVAT Amount should be equal", chargeLine.SellOSGSTVATAmount, charge.JR_OSSellGSTAmt_Calc);
			}
		}

		public OrganizationAddress SetupOrganizationAddress(ZString? addressType)
		{
			return SetupOrganizationAddress(addressType, "THEMOMENT", "INTHEMSYD", "In The Moment", "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", new UNLOCO { Code = "AUMEL", Name = "Melbourne" }, new Country { Code = "AU", Name = "Australia" });
		}

		public OrganizationAddress SetupOrganizationAddress2(ZString? addressType)
		{
			return SetupOrganizationAddress(addressType, "TOOLATE", "TOAPOLOGISE", "Too Late To Apologise", "Unit 24, Level 10", "455 There St", "Big City", "Small State", "56845", new UNLOCO { Code = "USLAX", Name = "Los Angeles" }, new Country { Code = "US", Name = "United States" });
		}

		public void AssertContents(IAddressDetails address)
		{
			AssertContents(address, "In The Moment", "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU");
		}

		public void AssertContents2(IAddressDetails address)
		{
			AssertContents(address, "Too Late To Apologise", "Unit 24, Level 10", "455 There St", "Big City", "Small State", "56845", "US");
		}

		public void AssertContents(IAddressDetails address, ZString companyName, ZString addressLine1, ZString addressLine2, ZString city, ZString state, ZString postCode, ZString country, ZString? phone = null, ZString? fax = null, ZString? contactName = null, ZString? email = null)
		{
			AssertEquals("CompanyName", companyName, address.CompanyName);
			AssertEquals("AddressLine1", addressLine1, address.AddressLine1);
			AssertEquals("AddressLine2", addressLine2, address.AddressLine2);
			AssertEquals("City", city, address.City);
			AssertEquals("State", state, address.State);
			AssertEquals("PostCode", postCode, address.PostCode);
			AssertEquals("Country", country, address.Country);
			if (phone.HasValue)
			{
				AssertEquals("Phone", phone.Value, address.Phone);
			}
			if (fax.HasValue)
			{
				AssertEquals("Fax", fax.Value, address.Fax);
			}
			if (contactName.HasValue)
			{
				AssertEquals("ContactName", contactName.Value, address.ContactName);
			}
			if (email.HasValue)
			{
				AssertEquals("Email", email.Value, address.Email);
			}
		}

		public void AssertContents(IDocAddress address, ZString? phone = null, ZString? fax = null, ZBool? addresOverride = null, ZGuid? orgAddressPK = null, ZString? addressType = null)
		{
			AssertContents(address, "In The Moment", "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU", phone, fax, addresOverride, orgAddressPK, addressType);
		}

		public void AssertContents2(IDocAddress address, ZString? phone = null, ZString? fax = null, ZBool? addresOverride = null, ZGuid? orgAddressPK = null, ZString? addressType = null)
		{
			AssertContents(address, "Too Late To Apologise", "Unit 24, Level 10", "455 There St", "Big City", "Small State", "56845", "US", phone, fax, addresOverride, orgAddressPK, addressType);
		}

		public void AssertContents(IDocAddress address, ZString companyName, ZString addressLine1, ZString addressLine2, ZString city, ZString state, ZString postCode, ZString country, ZString? phone = null, ZString? fax = null, ZBool? addresOverride = null, ZGuid? orgAddressPK = null, ZString? addressType = null)
		{
			AssertEquals("E2_CompanyName", companyName, address.E2_CompanyName);
			AssertEquals("E2_Address1", addressLine1, address.E2_Address1);
			AssertEquals("E2_Address2", addressLine2, address.E2_Address2);
			AssertEquals("E2_City", city, address.E2_City);
			AssertEquals("E2_State", state, address.E2_State);
			AssertEquals("E2_Postcode", postCode, address.E2_Postcode);
			AssertEquals("CountryCode", country, address.CountryCode);
			if (phone.HasValue)
			{
				AssertEquals("E2_Phone", phone.Value, address.E2_Phone);
			}
			if (fax.HasValue)
			{
				AssertEquals("E2_Fax", fax.Value, address.E2_Fax);
			}
			if (addresOverride.HasValue)
			{
				AssertEquals("E2_AddressOverride", addresOverride.Value, address.E2_AddressOverride);
			}
			if (orgAddressPK.HasValue)
			{
				AssertEquals("E2_OA_Address", orgAddressPK.Value, address.E2_OA_Address);
			}
			if (addressType.HasValue)
			{
				AssertEquals("E2_AddressType", addressType.Value, address.E2_AddressType);
			}
		}

		public OrganizationAddress SetupOrganizationAddress(ZString? addressType, ZString? addressShortCode, ZString? organizationCode, ZString? companyName, ZString? address1, ZString? address2, ZString? city, ZString? state, ZString? postcode, UNLOCO port, Country country, ZString? contact = null, ZString? phone = null)
		{
			return new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressShortCode = addressShortCode,
				AddressOverride = false,
				OrganizationCode = organizationCode,
				CompanyName = companyName,
				Address1 = address1,
				Address2 = address2,
				City = city,
				State = state,
				Postcode = postcode,
				Port = port,
				Country = country,
				Contact = contact,
				Phone = phone
			};
		}

		public RefServiceLevel ServiceLevel1
		{
			get
			{
				if (serviceLevel1 == null)
				{
					serviceLevel1 = Factory.New<RefServiceLevel>();
					serviceLevel1.RS_Code = "Z!2";
					serviceLevel1.RS_Description = "DUMMY SERVICE";
				}
				return serviceLevel1;
			}
		}
		RefServiceLevel serviceLevel1;

		public AdditionalReference SetupAdditionalReference()
		{
			return SetupAdditionalReference("INFORMER", new ZDateTime(2011, 3, 3), "CE00001", new EntryType { Code = "AMS", Description = "AMS Number" });
		}

		public AdditionalReference SetupAdditionalReference2()
		{
			return SetupAdditionalReference("NOTICED", new ZDateTime(2011, 4, 3), "DG00005", new EntryType { Code = "CON", Description = "Carrier Contract Number" });
		}

		public AdditionalReference SetupAdditionalReference(ZString? contextInformation, ZDateTime? issueDate, ZString? referenceNumber, EntryType type)
		{
			return new AdditionalReference()
			{
				ContextInformation = contextInformation,
				IssueDate = issueDate,
				ReferenceNumber = referenceNumber,
				Type = type
			};
		}

		public TransportLeg SetupTransportLeg()
		{
			return SetupTransportLeg(
				1,
				TransportMode.Sea,
				new ZDateTime(2011, 3, 7),
				new ZDateTime(2011, 3, 5),
				"BOOKMEUP",
				new ZDateTime(2011, 3, 6),
				new ZDateTime(2011, 3, 4),
				Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.Main,
				new UNLOCO() { Code = "NZAKL", Name = "Auckland" },
				new UNLOCO() { Code = "AUMEL", Name = "Melbourne" },
				"BUNGA DELIMA",
				"343L",
				SetupAddressData_INTHEMSYD("Fooey"));
		}

		public TransportLeg SetupTransportLeg2()
		{
			return SetupTransportLeg(
				2,
				TransportMode.Road,
				new ZDateTime(2011, 4, 7),
				new ZDateTime(2011, 4, 5),
				"DRIVEME",
				new ZDateTime(2011, 4, 6),
				new ZDateTime(2011, 4, 4),
				Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.OnForwarding,
				new UNLOCO() { Code = "AUMEL", Name = "Melbourne" },
				new UNLOCO() { Code = "AUSYD", Name = "Sydney" },
				"",
				"DRK324",
				SetupAddressData_INTHEMSYD("LEG2SMITH"));
		}

		public TransportLeg SetupTransportLeg(
			ZByte? legOrder,
			TransportMode transportMode,
			ZDateTime? actualArrival,
			ZDateTime? actualDeparture,
			ZString? carrierBookingReference,
			ZDateTime? estimatedArrival,
			ZDateTime? estimatedDeparture,
			LegType legType,
			UNLOCO portOfLoading,
			UNLOCO portOfDischarge,
			ZString? vesselName,
			ZString? voyageFlightNo,
			OrganizationAddress carrier
		)
		{
			return new TransportLeg()
			{
				LegOrder = legOrder,
				TransportMode = transportMode,
				ActualArrival = actualArrival,
				ActualDeparture = actualDeparture,
				CarrierBookingReference = carrierBookingReference,
				EstimatedArrival = estimatedArrival,
				EstimatedDeparture = estimatedDeparture,
				LegType = legType,
				PortOfLoading = portOfLoading,
				PortOfDischarge = portOfDischarge,
				VesselName = vesselName,
				VoyageFlightNo = voyageFlightNo,
				Carrier = carrier
			};
		}

		public void AssertContents(Transport transportBO)
		{
			AssertContents(transportBO, "BUNGA DELIMA", "343L", "SEA", "NZAKL", new ZDateTime(2011, 3, 4), new ZDateTime(2011, 3, 5), "AUMEL", new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 7), "BOOKMEUP", ZGuid.Empty);
		}

		public void AssertContents2(Transport transportBO)
		{
			AssertContents(transportBO, "", "DRK324", "ROA", "AUMEL", new ZDateTime(2011, 4, 4), new ZDateTime(2011, 4, 5), "AUSYD", new ZDateTime(2011, 4, 6), new ZDateTime(2011, 4, 7), "DRIVEME", ZGuid.Empty);
		}

		public void AssertContents(Transport transportBO, ZString vessel, ZString voyageFlight, ZString transportMode, ZString loadPort, ZDateTime etd, ZDateTime atd, ZString discPort, ZDateTime eta, ZDateTime ata, ZString carrierBookingReference, ZGuid carrierPK)
		{
			AssertEquals("transportBO.JW_Vessel", vessel, transportBO.JW_Vessel);
			AssertEquals("transportBO.JW_VoyageFlight", voyageFlight, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_TransportMode", transportMode, transportBO.JW_TransportMode);

			AssertEquals("transportBO.JW_RL_NKLoadPort", loadPort, transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", etd, transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", atd, transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", discPort, transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", eta, transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", ata, transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", carrierBookingReference, transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.CarrierPK", carrierPK, transportBO.CarrierPK);
		}

		public OrganizationAddress SetupAddressData_INTHEMSYD(ZString addressType)
		{
			return SetupAddressData(
				addressType,
				true,
				"INTHEMSYD",
				"In The Moment",
				"Unit 12, Level 3",
				"233 Here St",
				"ThereVille",
				"OfBliss",
				"1233",
				new Country() { Code = "AU", Name = "Australia" },
				"Starshine Moonbeam",
				"s.m@moment.com.au",
				"234098234",
				"234098293",
				"1239813209",
				new CodeDescriptionPair() { Code = "UNK", Description = "Unknown" },
				"55555",
				new RegistrationNumberType() { Code = "GST", Description = "GST Code" }
			);
		}

		public OrganizationAddress SetupAddressData(
			ZString? addressType,
			ZBool? addressOverride,
			ZString? organizationCode,
			ZString? companyName,
			ZString? address1,
			ZString address2,
			ZString? city,
			ZString? state,
			ZString? postcode,
			Country country,
			ZString? contact,
			ZString? email,
			ZString? fax,
			ZString? mobile,
			ZString? phone,
			CodeDescriptionPair screeningStatus,
			ZString? govRegNum,
			RegistrationNumberType govRegNumType
		)
		{
			return new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressOverride = addressOverride,

				OrganizationCode = organizationCode,
				CompanyName = companyName,
				Address1 = address1,
				Address2 = address2,
				City = city,
				State = state,
				Postcode = postcode,
				Country = country,

				Contact = contact,
				Email = email,
				Fax = fax,
				Mobile = mobile,
				Phone = phone,

				ScreeningStatus = screeningStatus,

				GovRegNum = govRegNum,
				GovRegNumType = govRegNumType
			};
		}

		public void AssertContentsMatches_INTHEMSYD(JobDocAddress jobDocAddressBO, ZString addressType)
		{
			AssertContents(jobDocAddressBO,
				addressType,
				ZBool.True,
				MiscOrgAddressPK,
				"In The Moment",
				"Unit 12, Level 3",
				"233 Here St",
				"ThereVille",
				"OfBliss",
				"1233",
				"AU",
				"Starshine Moonbeam",
				"s.m@moment.com.au",
				"234098234",
				"234098293",
				"1239813209",
				"NOT",
				"55555",
				"GST");
		}

		public void AssertContents(JobDocAddress jobDocAddressBO,
			ZString addressType,
			ZBool addressOverride,
			ZGuid addressPK,
			ZString companyName,
			ZString address1,
			ZString address2,
			ZString city,
			ZString state,
			ZString postcode,
			ZString country,
			ZString contact,
			ZString email,
			ZString fax,
			ZString mobile,
			ZString phone,
			ZString screeningStatus,
			ZString govRegNum,
			ZString govRegNumType
		)
		{
			AssertEquals("jobDocAddressBO.E2_AddressType", addressType, jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", addressOverride, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressPK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_CompanyName", companyName, jobDocAddressBO.E2_CompanyName);
			AssertEquals("jobDocAddressBO.E2_Address1", address1, jobDocAddressBO.E2_Address1);
			AssertEquals("jobDocAddressBO.E2_Address2", address2, jobDocAddressBO.E2_Address2);
			AssertEquals("jobDocAddressBO.E2_City", city, jobDocAddressBO.E2_City);
			AssertEquals("jobDocAddressBO.E2_State", state, jobDocAddressBO.E2_State);
			AssertEquals("jobDocAddressBO.E2_Postcode", postcode, jobDocAddressBO.E2_Postcode);
			AssertEquals("jobDocAddressBO.E2_RN_NKCountryCode", country, jobDocAddressBO.E2_RN_NKCountryCode);
			AssertEquals("jobDocAddressBO.E2_Contact", contact, jobDocAddressBO.E2_Contact);
			AssertEquals("jobDocAddressBO.E2_Email", email, jobDocAddressBO.E2_Email);
			AssertEquals("jobDocAddressBO.E2_Fax", fax, jobDocAddressBO.E2_Fax);
			AssertEquals("jobDocAddressBO.E2_Mobile", mobile, jobDocAddressBO.E2_Mobile);
			AssertEquals("jobDocAddressBO.E2_Phone", phone, jobDocAddressBO.E2_Phone);
			AssertEquals("jobDocAddressBO.E2_ScreeningStatus", screeningStatus, jobDocAddressBO.E2_ScreeningStatus);
			AssertEquals("jobDocAddressBO.E2_GovRegNum", govRegNum, jobDocAddressBO.E2_GovRegNum);
			AssertEquals("jobDocAddressBO.E2_GovRegNumType", govRegNumType, jobDocAddressBO.E2_GovRegNumType);
		}

		public Note SetupNote()
		{
			return SetupNote("DOG FLOGGER!!", new CodeDescriptionPair() { Code = nameof(CargoWise.Definitions.StmNoteVisibility.PUB), Description = "Public" }, new NoteContext() { Code = "AAA", Description = "Baby Eats Banana" }, true, "GOODBYE WORLD");
		}

		public Note SetupNote2()
		{
			return SetupNote("WORM EATER!!", new CodeDescriptionPair() { Code = nameof(CargoWise.Definitions.StmNoteVisibility.PRV), Description = "Private" }, new NoteContext() { Code = "AAA", Description = "Eat Worms" }, false, "HELLO WORLD");
		}

		public Note SetupNote(ZString? description, CodeDescriptionPair visibility, NoteContext noteContext, ZBool? isCustomDescription, ZString? noteText)
		{
			return new Note()
			{
				Description = description,
				Visibility = visibility,
				NoteContext = noteContext,
				IsCustomDescription = isCustomDescription,
				NoteText = noteText
			};
		}

		public void AssertContents(StmNote noteBO)
		{
			AssertContents(noteBO, "DOG FLOGGER!!", "AAA", "PUB", ZBool.True, "GOODBYE WORLD");
		}

		public void AssertContents2(StmNote noteBO)
		{
			AssertContents(noteBO, "WORM EATER!!", "AAA", "PRV", ZBool.True, "HELLO WORLD");
		}

		public void AssertContents(StmNote noteBO, ZString description, ZString noteContext, ZString noteType, ZBool isCustomDescription, ZString noteText)
		{
			AssertEquals("noteBO.ST_Description", description, noteBO.ST_Description);
			AssertEquals("noteBO.ST_NoteContext", noteContext, noteBO.ST_NoteContext);
			AssertEquals("noteBO.ST_NoteType", noteType, noteBO.ST_NoteType);
			AssertEquals("noteBO.ST_IsCustomDescription", isCustomDescription, noteBO.ST_IsCustomDescription);
			AssertEquals("noteBO.ST_NoteDataAsText", noteText, noteBO.ST_NoteDataAsText);
		}

		public OrgHeader CreateOrganisation(ZString name, ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			org.OH_Code = code;
			org.MainAddress.OA_Address1 = name + " ADDRESS 1";
			return org;
		}

		public AccChargeCode AccChargeCode1
		{
			get
			{
				if (accChargeCode1 == null)
				{
					accChargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, CurrentCompany.PK));
				}
				return accChargeCode1;
			}
		}
		AccChargeCode accChargeCode1;

		public AccChargeCode AccChargeCode2
		{
			get
			{
				if (accChargeCode2 == null)
				{
					var query = new ZQuery(AccChargeCodeSchema.AC_GC, CurrentCompany.PK);
					query.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, AccChargeCode1.PK);
					accChargeCode2 = Factory.LoadTop1<AccChargeCode>(query);
				}
				return accChargeCode2;
			}
		}
		AccChargeCode accChargeCode2;

		public TransportLogisticsCost SetupTransportLogisticsCost()
		{
			return SetupTransportLogisticsCost(new ChargeCode() { Code = AccChargeCode1.AC_Code }, "BOB THE BUILDER", 1000m, new Currency() { Code = Core.Constants.CurrencyCodes.Australia }, new CodeDescriptionPair() { Code = CostDistributionMechanismList.Codes.Item }, new CodeDescriptionPair() { Code = "1" }, 0.9834m);
		}

		public TransportLogisticsCost SetupTransportLogisticsCost2()
		{
			return SetupTransportLogisticsCost(new ChargeCode() { Code = AccChargeCode2.AC_Code }, "WENDY THE DESTROYER", 2000m, new Currency() { Code = Core.Constants.CurrencyCodes.Singapore }, new CodeDescriptionPair() { Code = CostDistributionMechanismList.Codes.LineValue }, new CodeDescriptionPair() { Code = "2" }, 1.1245m);
		}

		public TransportLogisticsCost SetupTransportLogisticsCost(ChargeCode chargeCode, ZString? chargeDescription, ZDecimal? costAmount, Currency costCurrency, CodeDescriptionPair distributeCostBy, CodeDescriptionPair landedCostGroup, ZDecimal? serviceExRate)
		{
			return new TransportLogisticsCost()
			{
				ChargeCode = chargeCode,
				ChargeDescription = chargeDescription,
				CostAmount = costAmount,
				CostCurrency = costCurrency,
				DistributeCostBy = distributeCostBy,
				LandedCostGroup = landedCostGroup,
				ServiceExRate = serviceExRate
			};
		}

		public void AssertLandCostInputContents(Integration.LandedCosting.ILandCostInput costInputBO, BusinessObject parent)
		{
			AssertLandCostInputContents(costInputBO, parent, AccChargeCode1.PK, "BOB THE BUILDER", 1000m, Core.Constants.CurrencyCodes.Australia, CostDistributionMechanismList.Codes.Item, 1, 0.9834m);
		}

		public void AssertLandCostInputContents2(Integration.LandedCosting.ILandCostInput costInputBO, BusinessObject parent)
		{
			AssertLandCostInputContents(costInputBO, parent, AccChargeCode2.PK, "WENDY THE DESTROYER", 2000m, Core.Constants.CurrencyCodes.Singapore, CostDistributionMechanismList.Codes.LineValue, 2, 1.1245m);
		}

		public void AssertLandCostInputContents(Integration.LandedCosting.ILandCostInput costInputBO, BusinessObject parent, ZGuid chargeCodePK, ZString chargeDescription, ZDecimal costAmount, ZString costCurrency, ZString distributeCostBy, ZByte landedCostGroup, ZDecimal serviceExRate)
		{
			AssertEquals("costInputBO.LI_ParentID", parent.PK, costInputBO.LI_ParentID);
			AssertEquals("costInputBO.LI_ParentTableCode", parent.TablePrefix, costInputBO.LI_ParentTableCode);
			AssertEquals("costInputBO.LI_AC_ChargeCode", chargeCodePK, costInputBO.LI_AC_ChargeCode);
			AssertEquals("costInputBO.LI_ChargeDescription", chargeDescription, costInputBO.LI_ChargeDescription);
			AssertEquals("costInputBO.LI_CostAmount", costAmount, costInputBO.LI_CostAmount);
			AssertEquals("costInputBO.LI_RX_NKCostCurrency", costCurrency, costInputBO.LI_RX_NKCostCurrency);
			AssertEquals("costInputBO.LI_DistributeCostBy", distributeCostBy, costInputBO.LI_DistributeCostBy);
			AssertEquals("costInputBO.LI_LandedCostGroup", landedCostGroup, costInputBO.LI_LandedCostGroup);
			AssertEquals("costInputBO.LI_ServiceExRate", serviceExRate, costInputBO.LI_ServiceExRate);
		}
	}
}
