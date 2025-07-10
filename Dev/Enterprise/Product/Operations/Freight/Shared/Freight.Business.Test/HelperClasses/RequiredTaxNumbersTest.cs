using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Constants = Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RequiredTaxNumbersTest : TestCaseWithFactory
	{
		public void TestGetRequiredTaxNumberWithType_OrgHeader()
		{
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Uruguay, OrgCusCode.BangladeshCodeTypes.AIN, UruguayOrgCusCodeInfo.OrgCusCodes.RUT);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Argentina, OrgCusCode.BangladeshCodeTypes.AIN, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);

			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Argentina, "", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Uruguay, "", UruguayOrgCusCodeInfo.OrgCusCodes.RUT);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Paraguay, "", OrgCusCode.ParaguayCodeTypes.RUC);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Algeria, "", AlgeriaOrgCusCodeInfo.OrgCusCodes.NIF);

			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Bangladesh, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Peru, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode);
			AssertRequiredVATNumber_OrgHeader(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Turkey, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode);
		}

		void AssertRequiredVATNumber_OrgHeader(ZString expCountry, ZString impCountry, ZString expCodeType, ZString impCodeType)
		{
			var orgFactory = new BusinessObjectFactory();
			var impRegNo = Get4DigitsCodeForTest(impCodeType);
			var expRegNo = Get4DigitsCodeForTest(expCodeType);

			var consignee = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee.OH_Code = string.Concat("Consignee", OrgCodeIndex++);
			var taxCode1 = consignee.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = impCodeType;
			taxCode1.OK_RN_NKCodeCountry = impCountry;
			taxCode1.OK_CustomsRegNo = impRegNo;

			var shipper = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			shipper.OH_Code = string.Concat("Consignor", OrgCodeIndex++);
			var taxCode2 = shipper.CustomsCodes.AddNew();

			if (expCountry.Equals(Core.Constants.CountryCodes.Bangladesh))
			{
				taxCode2.OK_CodeType = "BRN";
			}
			else
			{
				taxCode2.OK_CodeType = expCodeType;
			}

			taxCode2.OK_RN_NKCodeCountry = expCountry;
			taxCode2.OK_CustomsRegNo = expRegNo;

			var notify = orgFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			notify.OH_Code = string.Concat("notify", OrgCodeIndex++);
			var taxCode3 = notify.CustomsCodes.AddNew();
			taxCode3.OK_CodeType = impCodeType;
			taxCode3.OK_RN_NKCodeCountry = impCountry;
			taxCode3.OK_CustomsRegNo = impRegNo;

			orgFactory.Save();
			var shortLabel = impRegNo;
			if (impCountry == Constants.CountryCodes.Bangladesh && impCodeType == OrgCusCode.CodeTypes.VATCode)
			{
				shortLabel = OrgCusCode.BangladeshCodeTypes.BIN;
			}
			var consigneeNumber = RequiredTaxNumbers.GetRequiredTaxNumberWithType(impCountry, expCountry, consignee, RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			var consigneeActual = shortLabel + ": " + impRegNo;
			AssertEquals("Return Consignee VAT Numbers With Type", consigneeActual, consigneeNumber);

			var shipperNumber = RequiredTaxNumbers.GetRequiredTaxNumberWithType(impCountry, expCountry, shipper, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			if (Core.Constants.CountryCodes.UnitedStates.Equals(expCountry) && (Core.Constants.CountryCodes.Uruguay.Equals(impCountry) || Core.Constants.CountryCodes.Paraguay.Equals(impCountry)))
			{
				var shipperActual = !string.IsNullOrWhiteSpace(shipperNumber) ? ("EIN: " + expRegNo) : string.Empty;
				AssertEquals("Return Shipper VAT Numbers With Type.", shipperActual, shipperNumber);
			}
			else
			{
				var shipperActual = !string.IsNullOrWhiteSpace(shipperNumber) ? (expRegNo + ": " + expRegNo) : string.Empty;
				AssertEquals("Return Shipper VAT Numbers With Type.", shipperActual, shipperNumber);
			}

			var notifyNumber = RequiredTaxNumbers.GetRequiredTaxNumberWithType(impCountry, expCountry, notify, RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			var notifyActual = shortLabel + ": " + impRegNo;
			AssertEquals("Return AlsoNotify VAT Numbers With Type", notifyActual, notifyNumber);
		}

		public void TestGetRequiredTaxNumber_OrganizationAddress()
		{
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Uruguay, Core.Constants.CountryCodes.Bangladesh, UruguayOrgCusCodeInfo.OrgCusCodes.RUT, OrgCusCode.CodeTypes.VATCode);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Uruguay, OrgCusCode.BangladeshCodeTypes.AIN, UruguayOrgCusCodeInfo.OrgCusCodes.RUT);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Bangladesh, Core.Constants.CountryCodes.Argentina, OrgCusCode.BangladeshCodeTypes.AIN, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);

			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Argentina, "", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Uruguay, "", UruguayOrgCusCodeInfo.OrgCusCodes.RUT);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Paraguay, "", OrgCusCode.ParaguayCodeTypes.RUC);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Algeria, "", AlgeriaOrgCusCodeInfo.OrgCusCodes.NIF);

			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Bangladesh, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Peru, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode);
			AssertRequiredVATNumber_OrganizationAddress(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Turkey, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, OrgCusCode.CodeTypes.VATCode);
		}

		void AssertRequiredVATNumber_OrganizationAddress(ZString expCountry, ZString impCountry, ZString expCodeType, ZString impCodeType)
		{
			var orgFactory = new BusinessObjectFactory();
			var impRegNo = Get4DigitsCodeForTest(impCodeType);
			var expRegNo = Get4DigitsCodeForTest(expCodeType);

			var consignee = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consignee.CompanyName = string.Concat("Consignee", OrgCodeIndex++);
			var taxCode1 = new RegistrationNumber()
			{
				Type = new RegistrationNumberType() { Code = impCodeType },
				CountryOfIssue = new Country() { Code = impCountry },
				Value = impRegNo
			};
			consignee.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			consignee.RegistrationNumberCollection.Add(taxCode1);

			var shipper = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			shipper.CompanyName = string.Concat("Consignor", OrgCodeIndex++);
			var taxCode2 = new RegistrationNumber();
			shipper.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			shipper.RegistrationNumberCollection.Add(taxCode2);

			if (expCountry.Equals(Core.Constants.CountryCodes.Bangladesh))
			{
				taxCode2.Type = new RegistrationNumberType() { Code = "BRN" };
			}
			else
			{
				taxCode2.Type = new RegistrationNumberType() { Code = expCodeType };
			}

			taxCode2.CountryOfIssue = new Country() { Code = expCountry };
			taxCode2.Value = expRegNo;

			var notify = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			notify.CompanyName = string.Concat("notify", OrgCodeIndex++);
			var taxCode3 = new RegistrationNumber();
			notify.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			notify.RegistrationNumberCollection.Add(taxCode3);
			taxCode3.Type = new RegistrationNumberType() { Code = impCodeType };
			taxCode3.CountryOfIssue = new Country() { Code = impCountry };
			taxCode3.Value = impRegNo;

			orgFactory.Save();

			var consigneeNumber = RequiredTaxNumbers.GetRequiredTaxNumber(impCountry, expCountry, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			var consigneeActual = impRegNo;
			AssertEquals("Return Consignee VAT Numbers", consigneeActual, consigneeNumber);

			var shipperNumber = RequiredTaxNumbers.GetRequiredTaxNumber(impCountry, expCountry, new OrganizationAddressRegistrationNumberProvider(shipper), RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			if (Core.Constants.CountryCodes.UnitedStates.Equals(expCountry) && (Core.Constants.CountryCodes.Uruguay.Equals(impCountry) || Core.Constants.CountryCodes.Paraguay.Equals(impCountry)))
			{
				var shipperActual = !shipperNumber.IsEmpty ? expRegNo : ZString.Empty;
				AssertEquals("Return Shipper VAT Numbers.", shipperActual, shipperNumber);
			}
			else
			{
				var shipperActual = !shipperNumber.IsEmpty ? expRegNo : ZString.Empty;
				AssertEquals("Return Shipper VAT Numbers.", shipperActual, shipperNumber);
			}

			var notifyNumber = RequiredTaxNumbers.GetRequiredTaxNumber(impCountry, expCountry, new OrganizationAddressRegistrationNumberProvider(notify), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			var notifyActual = impRegNo;
			AssertEquals("Return AlsoNotify VAT Numbers.", notifyActual, notifyNumber);
		}

		public void TestGetRequiredTaxNumber_China()
		{
			var chinaCode = Core.Constants.CountryCodes.China;

			var consignee = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consignee.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var chinaTaxCode = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = OrgCusCode.ChinaCodeTypes.USC },
				CountryOfIssue = new Country { Code = chinaCode },
				Value = "USCI123"
			};

			consignee.RegistrationNumberCollection.Add(chinaTaxCode);

			var usciNumber = RequiredTaxNumbers.GetRequiredTaxNumber(chinaCode, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
			AssertEquals("USCI number should be ignored by default.", string.Empty, usciNumber);

			usciNumber = RequiredTaxNumbers.GetRequiredTaxNumber(chinaCode, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			AssertEquals("USCI number should not be ignored.", "USCI123", usciNumber);
		}

		public void TestGetRequiredTaxNumber_India()
		{
			var org = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			org.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var indiaGSTTaxCode = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.GSTCode },
				CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.India },
				Value = "GST123"
			};

			var indiaIECTaxCode = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = IndiaOrgCusCodeInfo.OrgCusCodes.IEC },
				CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.India },
				Value = "IEC123"
			};

			org.RegistrationNumberCollection.Add(indiaGSTTaxCode);
			org.RegistrationNumberCollection.Add(indiaIECTaxCode);

			var gstNumber = RequiredTaxNumbers.GetRequiredTaxNumber(Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(org), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
			AssertEquals("GST number should be displayed by default.", "GST123", gstNumber);

			gstNumber = RequiredTaxNumbers.GetRequiredTaxNumber(Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(org), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			AssertEquals("GST number should be displayed on shipping instruction.", "GST123", gstNumber);

			gstNumber = RequiredTaxNumbers.GetRequiredTaxNumber(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.India, new OrganizationAddressRegistrationNumberProvider(org), RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			AssertEquals("GST number should NOT be displayed for shipper.", "", gstNumber);

			var gstNumberList = RequiredTaxNumbers.GetRequiredTaxNumbers(Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(org), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General).ToArray();
			AssertArrayEqualsByElements("GST number should be displayed by default.", new ZString[] { "GST123", "IEC123" }, gstNumberList);

			gstNumberList = RequiredTaxNumbers.GetRequiredTaxNumbers(Core.Constants.CountryCodes.India, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(org), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction).ToArray();
			AssertArrayEqualsByElements("GST number should be displayed on shipping instruction.", new ZString[] { "GST123" }, gstNumberList);

			gstNumberList = RequiredTaxNumbers.GetRequiredTaxNumbers(Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.India, new OrganizationAddressRegistrationNumberProvider(org), RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.ShippingInstruction).ToArray();
			AssertArrayEqualsByElements("GST number should NOT be displayed for shipper.", System.Array.Empty<ZString>(), gstNumberList);
		}

		public void TestGetRequiredTaxNumber_IgnoreLocalTaxID_Vietnam()
		{
			var vietnamCode = Core.Constants.CountryCodes.VietNam;

			var consignee = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consignee.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var vietnamTaxCode = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.VATCode },
				CountryOfIssue = new Country { Code = vietnamCode },
				Value = "1234"
			};

			consignee.RegistrationNumberCollection.Add(vietnamTaxCode);

			var vatNumber = RequiredTaxNumbers.GetRequiredTaxNumber(vietnamCode, Core.Constants.CountryCodes.UnitedStates, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
			AssertEquals("VAT number should be ignored by default.", string.Empty, vatNumber);

			vatNumber = RequiredTaxNumbers.GetRequiredTaxNumber(vietnamCode, Core.Constants.CountryCodes.UnitedStates, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			AssertEquals("VAT number should not be ignored", "1234", vatNumber);
		}

		public void TestGetRequiredTaxNumber_Kenya()
		{
			var consignee = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consignee.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var kenyaTaxCode = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = OrgCusCode.KenyaCodeTypes.PIN },
				CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Kenya },
				Value = "PIN123"
			};

			consignee.RegistrationNumberCollection.Add(kenyaTaxCode);

			var taxNumberNonAWBDoc = RequiredTaxNumbers.GetRequiredTaxNumber(Core.Constants.CountryCodes.Kenya, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
			var taxNumberAWBDoc = RequiredTaxNumbers.GetRequiredTaxNumber(Core.Constants.CountryCodes.Kenya, Core.Constants.CountryCodes.Brazil, new OrganizationAddressRegistrationNumberProvider(consignee), RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.AirwayBill);

			CombineAssertions(() =>
			{
				AssertEquals("PIN should be ignored by default.", string.Empty, taxNumberNonAWBDoc);
				AssertEquals("PIN should not be ignored.", "PIN123", taxNumberAWBDoc);
			});
		}

		public void TestGetRequiredTaxNumber_Indonesia()
		{
			TestGetRequiredTaxNumber_IndonesiaCore(true);
			TestGetRequiredTaxNumber_IndonesiaCore(false);
		}

		public void TestGetRequiredTaxNumber_IndonesiaCore(bool isImportToIndonesia)
		{
			var importCountryCode = Core.Constants.CountryCodes.Indonesia;
			var exportCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var orgType = RequiredTaxNumbers.TaxOrgType.Consignee;

			if (!isImportToIndonesia)
			{
				importCountryCode = Core.Constants.CountryCodes.UnitedStates;
				exportCountryCode = Core.Constants.CountryCodes.Indonesia;
				orgType = RequiredTaxNumbers.TaxOrgType.Shipper;
			}

			var orgAddressWithPPNPAS = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressWithPPNPAS.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.PassportID },
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Indonesia },
						Value = "34243"
					},
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType { Code = OrgCusCode.IndonesiaCodeTypes.PPN },
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Indonesia },
						Value = "33333"
					}
				});

			var orgAddressWithPAS = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressWithPAS.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.PassportID },
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Indonesia },
						Value = "34243"
					}
				});

			var taxnumber = RequiredTaxNumbers.GetRequiredTaxNumber(importCountryCode, exportCountryCode, new OrganizationAddressRegistrationNumberProvider(orgAddressWithPPNPAS), orgType, RequiredTaxNumbers.DocumentType.General);
			AssertEquals("Should not default any number when document is BOL.", string.Empty, taxnumber);

			taxnumber = RequiredTaxNumbers.GetRequiredTaxNumber(importCountryCode, exportCountryCode, new OrganizationAddressRegistrationNumberProvider(orgAddressWithPPNPAS), orgType, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			AssertEquals("33333", taxnumber);

			taxnumber = RequiredTaxNumbers.GetRequiredTaxNumber(importCountryCode, exportCountryCode, new OrganizationAddressRegistrationNumberProvider(orgAddressWithPAS), orgType, RequiredTaxNumbers.DocumentType.General);
			AssertEquals("Should not default any number when document is BOL.", string.Empty, taxnumber);

			taxnumber = RequiredTaxNumbers.GetRequiredTaxNumber(importCountryCode, exportCountryCode, new OrganizationAddressRegistrationNumberProvider(orgAddressWithPAS), orgType, RequiredTaxNumbers.DocumentType.ShippingInstruction);
			AssertEquals("34243", taxnumber);
		}

		ZString Get4DigitsCodeForTest(ZString defaultNo)
		{
			var typeCode = defaultNo;
			switch (defaultNo)
			{
				case ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT:
					typeCode = "CUIT";
					break;
				case BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ:
					typeCode = "CNPJ";
					break;
				case OrgCusCode.ChinaCodeTypes.USC:
					typeCode = "USCI";
					break;
			}

			return typeCode;
		}

		int OrgCodeIndex { get; set; }

		public void TestGetTaxInfoFromRefTable()
		{
			var refDocOrgCusCodeBO = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			refDocOrgCusCodeBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.UnitedKingdom;
			refDocOrgCusCodeBO.DOC_Notes = "Company Number TST";
			refDocOrgCusCodeBO.DOC_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
			refDocOrgCusCodeBO.DOC_Priority = 1;

			var refDocOrgCusCodeBO2 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBO2.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO2.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			refDocOrgCusCodeBO2.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			refDocOrgCusCodeBO2.DOC_Notes = "Trade Register Number TST";
			refDocOrgCusCodeBO2.DOC_CodeType = OrgCusCode.CodeTypes.VATCode;
			refDocOrgCusCodeBO2.DOC_Priority = 1;

			var refDocOrgCusCodeBO3 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBO3.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO3.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			refDocOrgCusCodeBO3.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			refDocOrgCusCodeBO3.DOC_Notes = "Trade Register Number 2 TST";
			refDocOrgCusCodeBO3.DOC_CodeType = OrgCusCode.CodeTypes.AgentCode;
			refDocOrgCusCodeBO3.DOC_Priority = 2;

			var refDocOrgCusCodeChina = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeChina.DOC_DocumentType = "ESI";
			refDocOrgCusCodeChina.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			refDocOrgCusCodeChina.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			refDocOrgCusCodeChina.DOC_Notes = "Trade Register Number TST";
			refDocOrgCusCodeChina.DOC_CodeType = OrgCusCode.CodeTypes.VATCode;
			refDocOrgCusCodeChina.DOC_Priority = 2;

			var refDocOrgCusCodeBrazil = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBrazil.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBrazil.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			refDocOrgCusCodeBrazil.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Brazil;
			refDocOrgCusCodeBrazil.DOC_Notes = "Trade Register Number TST";
			refDocOrgCusCodeBrazil.DOC_CodeType = OrgCusCode.CodeTypes.VATCode;
			refDocOrgCusCodeBrazil.DOC_Priority = 2;

			Factory.Save();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetupOrgHeaderCustomsCodes(consignee);

			var orgRegistrationNumberProvider = new OrgHeaderRegistrationNumberProvider(consignee);

			var indiaValues = RequiredTaxNumbers.GetTaxInfoFromRefTable(Constants.CountryCodes.India, orgRegistrationNumberProvider);
			AssertEquals(2, indiaValues.Count);

			var brazilValues = RequiredTaxNumbers.GetTaxInfoFromRefTable(Constants.CountryCodes.Brazil, orgRegistrationNumberProvider);
			AssertEquals(1, brazilValues.Count);

			var indiaValuesWithRegulatingCountry = RequiredTaxNumbers.GetTaxInfoFromRefTable(Constants.CountryCodes.India, orgRegistrationNumberProvider, new BusinessObjectFactory(), Constants.CountryCodes.UnitedKingdom);
			AssertEquals(1, indiaValuesWithRegulatingCountry.Count);
		}

		void SetupOrgHeaderCustomsCodes(OrgHeader consignee)
		{
			void SetupVatAndCompanyNumberForCountry(ZString country)
			{
				var cnoCode = consignee.CustomsCodes.AddNew();
				cnoCode.OK_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
				cnoCode.OK_RN_NKCodeCountry = country;
				cnoCode.OK_CustomsRegNo = "123CNO";
				var vstCode = consignee.CustomsCodes.AddNew();
				vstCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vstCode.OK_RN_NKCodeCountry = country;
				vstCode.OK_CustomsRegNo = "123VAT";
			}
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.India);
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.Australia);
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.China);
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.Brazil);
		}
	}
}
