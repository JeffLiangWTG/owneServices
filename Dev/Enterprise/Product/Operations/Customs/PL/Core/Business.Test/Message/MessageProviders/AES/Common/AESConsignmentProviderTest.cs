using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

class AESConsignmentProviderTest : DataProviderTestCase<AESConsignmentProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null EntryHeader", "Value cannot be null.\r\nParameter name: entryHeader",
			() => new AESConsignmentProvider(null));
		AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: entryHeader.Declaration",
			() => new AESConsignmentProvider(Factory.New<CusEntryHeader>()));
		AssertExceptionThrown<ArgumentNullException>("Null EntryInstruction", "Value cannot be null.\r\nParameter name: entryHeader.EntryInstruction",
			() => new AESConsignmentProvider(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
	});

	public void TestContainerIndicator() => CombineAssertions(() =>
	{
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
		AssertEquals("Declaration ContainerMode is LCL", 1, GetProvider().ContainerIndicator);
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
		AssertEquals("Declaration ContainerMode is not LCL, FCL, ULD, CNT", 0, GetProvider().ContainerIndicator);
		EntryInstruction.CEI_SubStyle = SubStyleCodes.B;
		AssertNull("ContainerIndicator null, CEI_SubStyle is B", GetProvider().ContainerIndicator);
	});

	public void TestModeOfTransportAtTheBorder() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("Declaration.JE_TransportMode is not set", GetProvider().ModeOfTransportAtTheBorder);

		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals("Declaration.JE_TransportModeInland is Sea", "1", GetProvider().ModeOfTransportAtTheBorder);

		EntryInstruction.CEI_SubStyle = SubStyleCodes.B;
		AssertNull("ModeOfTransportAtTheBorder null, CEI_SubStyle is B", GetProvider().ModeOfTransportAtTheBorder);
	});

	public void TestGrossMass() => CombineAssertions(() =>
	{
		invoiceLine.JI_Weight = 2.1267m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		AssertEquals("Gross Mass", 2.127m, GetProvider().GrossMass);

		invoiceLine.JI_Weight = 123.0m;
		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
		AssertEquals("Gross Mass Grams to Kilograms", 0.123m, GetProvider().GrossMass);

		invoiceLine.JI_Weight = 123.6m;
		AssertEquals("Gross Mass rounded to 3 decimal places", 0.124m, GetProvider().GrossMass);
	});

	public void TestReferenceNumberUCR()
	{
		declaration.JE_UCR = "ABC";
		AssertEquals("ReferenceNumberUCR", "ABC", GetProvider().ReferenceNumberUCR);
	}

	public virtual void TestCarrierIdentificationNumber()
	{
		var shippingLine = Factory.New<OrgHeader>();
		var carrierCusCode = shippingLine.CustomsCodes.AddNew();
		carrierCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo;
		carrierCusCode.OK_CustomsRegNo = "123";

		var org = Factory.New<OrgHeader>();
		var declarantAddress = org.Addresses.AddNew();
		var declarantCusCode = declarantAddress.CustomsCodes.AddNew();
		declarantCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		declarantCusCode.OK_CustomsRegNo = "789";

		declaration.JE_OH_ShippingLine = shippingLine.PK;
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Declaration Shipping Line without OrgCusCode of type EOR", GetProvider().CarrierIdentificationNumber);

			carrierCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			declarantCusCode.OK_CustomsRegNo = "123";
			AssertNull("CarrierIdentificationNumber equals DeclarantIdentificationNumber", GetProvider().CarrierIdentificationNumber);

			declarantCusCode.OK_CustomsRegNo = "789";
			AssertEquals("CarrierIdentificationNumber is not equal to DeclarantIdentificationNumber", "PL123", GetProvider().CarrierIdentificationNumber);
		});
	}

	public virtual void TestConsignor() => CombineAssertions(() =>
	{
		AssertNull("No Consignor data exists", GetProvider().Consignor);

		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();

		declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;
		invoiceLine.JI_OA_ExporterAddress = orgAddress.PK;
		AssertNull("JI_OA_ExporterAddress not null", GetProvider().Consignor);

		invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
		AssertNotNull("Consignor not null and no JI_OA_ExporterAddress", GetProvider().Consignor);
	});

	public virtual void TestConsignorType()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress.PK;

		AssertType<AESConsignmentConsigneeConsignorProvider>(GetProvider().Consignor);
	}

	public void TestConsignee()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();

		CombineAssertions(() =>
		{
			using (declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				AssertNull("No Consignee data exists", GetProvider().Consignee);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;
				AssertNotNull("Consignee Not Null, AES Not UCC6", GetProvider().Consignee);
			}

			declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			using (declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				AssertNull("No Consignee data exists", GetProvider().Consignee);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;
				AssertNotNull("AES UCC6, No EntryLine with Additional Info Code 30600", GetProvider().Consignee);
				var addInfo = invoice.AdditionalInfos.AddNew();
				addInfo.CSI_Code = AdditionalInfoCodes._30600;
				addInfo.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
				addInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
				AssertNull("AES UCC6, EntryLine with Additional Info Code 30600", GetProvider().Consignee);
			}
		});
	}

	public virtual void TestConsigneeType()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;

		AssertType<AESConsignmentConsigneeConsignorProvider>(GetProvider().Consignee);
	}

	public virtual void TestCountryOfRoutingOfConsignments() => CombineAssertions(() =>
	{
		AssertEquals("No Routings", 0, GetProvider().CountryOfRoutingOfConsignments.Count);

		var transport1 = declaration.ItineraryCountries.AddNew();
		transport1.CY_Code = CountryCodes.Poland;
		var transport2 = declaration.ItineraryCountries.AddNew();
		transport2.CY_Code = CountryCodes.Germany;
		declaration.JE_GoodsOrigin = ZString.Empty;
		declaration.JE_GoodsDestination = ZString.Empty;

		AssertEquals("JE_GoodsOrigin and JE_GoodsDestination are empty", 2, GetProvider().CountryOfRoutingOfConsignments.Count);
	
		declaration.JE_GoodsOrigin = CountryCodes.Poland;
		declaration.JE_GoodsDestination = CountryCodes.Germany;
		AssertEquals("JE_GoodsDestination and transport2 have same country code", 2, GetProvider().CountryOfRoutingOfConsignments.Count);

		declaration.JE_GoodsDestination = ZString.Empty;
		AssertEquals("JE_GoodsOrigin and transport1 have same country code", 2, GetProvider().CountryOfRoutingOfConsignments.Count);
	});

	public void TestActiveBorderTransportMeans_Allowed_EX()
	{
		var modesAll = new[] { TransportModes.Air, TransportModes.FixedTransportInstallations, TransportModes.InlandWaterwayTransport,
			TransportModes.Mail, TransportModes.OwnPropulsion, TransportModes.Rail, TransportModes.Road, TransportModes.Sea };

		var codesAllowedForEX = new[] { ProcedureCodes._10, ProcedureCodes._11, ProcedureCodes._23, ProcedureCodes._31 };

		var style = EntryStyleListExport.Codes.ExportNormal;
		declaration.JE_EntryStyle = style;

		CombineAssertions("Allowed for all modes when style is EX and procedure code in [10, 11, 23, 31]", () =>
		{
			foreach (var mode in modesAll)
			{
				declaration.JE_TransportMode = mode;
				foreach (var code in codesAllowedForEX)
				{
					EntryInstruction.CEI_Procedure = code;
					SetTransportId("12345");
					AssertNotNull($"[mode:{mode}; style:{style}; procedureCode:{code}]: Active Border Transport Means is not null for not empty Transport ID", GetProvider().ActiveBorderTransportMeans);

					SetTransportId(string.Empty);
					AssertNull($"[mode:{mode}; style:{style}; procedureCode:{code}]: Active Border Transport Means is null for empty Transport ID", GetProvider().ActiveBorderTransportMeans);
				}
			}
		});
	}

	public void TestActiveBorderTransportMeans_Allowed_CO()
	{
		var modesAllowedForCO = new[] { TransportModes.Air, TransportModes.InlandWaterwayTransport,
			TransportModes.OwnPropulsion, TransportModes.Rail, TransportModes.Road, TransportModes.Sea };

		var codesAllowedForCO = new[] { ProcedureCodes._76, ProcedureCodes._77 };

		var style = EntryStyleListExport.Codes.ExportToSpecialTerritory;
		declaration.JE_EntryStyle = style;

		CombineAssertions("Allowed for modes not in [MAI, FIX] when style is CO and procedure code in [76, 77]", () =>
		{
			foreach (var mode in modesAllowedForCO)
			{
				declaration.JE_TransportMode = mode;
				foreach (var code in codesAllowedForCO)
				{
					EntryInstruction.CEI_Procedure = code;
					SetTransportId("12345");
					AssertNotNull($"[mode:{mode}; style:{style}; procedureCode:{code}]: Active Border Transport Means is not null for not empty Transport ID", GetProvider().ActiveBorderTransportMeans);

					SetTransportId(string.Empty);
					AssertNull($"[mode:{mode}; style:{style}; procedureCode:{code}]: Active Border Transport Means is null for empty Transport ID", GetProvider().ActiveBorderTransportMeans);
				}
			}
		});
	}

	public void TestActiveBorderTransportMeans_Forbidden() => CombineAssertions(() =>
	{
		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
		declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
		EntryInstruction.CEI_Procedure = ProcedureCodes._44;
		SetTransportId("123");
		AssertNull("Empty for any mode when style is EX but procedure code not in [10, 11, 23, 31]", GetProvider().ActiveBorderTransportMeans);

		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
		EntryInstruction.CEI_Procedure = ProcedureCodes._76;
		declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;
		SetTransportId("321");
		AssertNull("Empty for FIX when style is CO for any procedure code", GetProvider().ActiveBorderTransportMeans);

		declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Mail;
		SetTransportId("123");
		AssertNull("Empty for MAI when style is CO for any procedure code", GetProvider().ActiveBorderTransportMeans);

		declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
		EntryInstruction.CEI_Procedure = ProcedureCodes._44;
		SetTransportId("321");
		AssertNull("Empty for any mode when style is CO and procedure code not in [76, 77]", GetProvider().ActiveBorderTransportMeans);

		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
		declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
		EntryInstruction.CEI_Procedure = ProcedureCodes._11;
		SetTransportId("123");
		AssertNull("Empty for any mode when style is not in [EX, CO]", GetProvider().ActiveBorderTransportMeans);
	});

	void SetTransportId(string value)
	{
		if (declaration.JE_TransportMode == TransportModes.Air)
		{
			declaration.JE_VoyageFlightNo = value;
		}
		else if (declaration.JE_TransportMode == Core.Constants.TransportModes.Sea
				&& declaration.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._10)
		{
			declaration.JE_LloydsIMO = value;
		}
		else
		{
			declaration.JE_VesselName = value;
		}
	}

	public void TestLocationOfGoods() => AssertNotNull(GetProvider().LocationOfGoods);

	public virtual void TestTransportDocument() => CombineAssertions(() =>
	{
		using (declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("no documents present - UCC6", 0, GetProvider().TransportDocument.Count);

			var entryInstructionDocument = EntryInstruction.AdditionalInfos.AddNew();
			entryInstructionDocument.CSI_Description = "Entry Instruction document";
			entryInstructionDocument.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertEquals("1 document - UCC6", 1, GetProvider().TransportDocument.Count);

			var document = EntryInstruction.AdditionalInfos.AddNew();
			document.CSI_Description = "Entry Instruction document 2";
			document.CSI_ReferenceNumber = "123";
			document.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertEquals("2 documents - UCC6", 2, GetProvider().TransportDocument.Count);

			var documentInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
			documentInvoiceLine.CSI_Description = "Invoice Line document 3";
			documentInvoiceLine.CSI_ReferenceNumber = "234";
			documentInvoiceLine.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertEquals("3 documents - UCC6 - invoice line should be included", 3, GetProvider().TransportDocument.Count);

			var documentInvoiceHeader = invoice.AdditionalInfos.AddNew();
			documentInvoiceHeader.CSI_Description = "Invoice document 4";
			documentInvoiceHeader.CSI_ReferenceNumber = "345";
			documentInvoiceHeader.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertEquals("4 documents - UCC6 - invoice header should be included", 4, GetProvider().TransportDocument.Count);

			var documentDuplicate = invoice.AdditionalInfos.AddNew();
			documentDuplicate.CSI_Description = "Invoice document 4";
			documentDuplicate.CSI_ReferenceNumber = "345";
			documentDuplicate.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertEquals("4 documents - UCC6 - duplicates should be excluded", 4, GetProvider().TransportDocument.Count);

			var documentINF = invoice.AdditionalInfos.AddNew();
			documentDuplicate.CSI_Description = "INF document 4";
			documentDuplicate.CSI_ReferenceNumber = "999";
			documentDuplicate.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			var documentREF = invoice.AdditionalInfos.AddNew();
			documentDuplicate.CSI_Description = "REF document 4";
			documentDuplicate.CSI_ReferenceNumber = "888";
			documentDuplicate.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			AssertEquals("4 documents - UCC6 - INF & REF should be excluded", 4, GetProvider().TransportDocument.Count);
		}

		using (declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertEquals("0 documents - not UCC6", 0, GetProvider().TransportDocument.Count);
		}
	});

	public virtual void TestTransportChargesMethodOfPayment() => CombineAssertions(() =>
	{
		AssertEquals("Empty TransportChargesMethodOfPayment", string.Empty, GetProvider().TransportChargesMethodOfPayment);

		invoice.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.A;
		EntryInstruction.CEI_SubStyle = SubStyleCodes.B;
		AssertEquals("Empty TransportChargesMethodOfPayment due to CEI_Substyle", string.Empty, GetProvider().TransportChargesMethodOfPayment);

		EntryInstruction.CEI_SubStyle = ZString.Empty;
		AssertEquals("Not Empty TransportChargesMethodOfPayment", ExportTransportMethodOfPaymentList.Codes.A, GetProvider().TransportChargesMethodOfPayment);
	});

	protected override AESConsignmentProvider GetProvider() => new AESConsignmentProvider(EntryHeader);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		EntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = EntryInstruction.PK;
		invoiceLine.JI_Description = "invoice line 1";

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		EntryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
	}

	protected JobDeclaration declaration;
	protected CusEntryHeader EntryHeader { get; set; }
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	protected CusEntryInstruction EntryInstruction { get; set; }
}
