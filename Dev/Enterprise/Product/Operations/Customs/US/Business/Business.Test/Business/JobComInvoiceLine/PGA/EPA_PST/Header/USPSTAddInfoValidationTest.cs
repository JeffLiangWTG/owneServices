using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USPSTAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPSTValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			Pesticide.US_OA_ExaminationLocation = orgAddress1.PK;
			AssertHasWarning(Pesticide.US_OA_ExaminationLocationInfo, addressDescriptionWarning);
			AssertHasWarning(Pesticide.US_OA_ExaminationLocationInfo, addressCodeWarning);
			Pesticide.US_OA_ExaminationLocation = orgAddress2.PK;
			AssertNoWarning(Pesticide.US_OA_ExaminationLocationInfo, addressDescriptionWarning);
			AssertNoWarning(Pesticide.US_OA_ExaminationLocationInfo, addressCodeWarning);

			Pesticide.US_OA_ShipperAddress = orgAddress1.PK;
			AssertHasWarning(Pesticide.US_OA_ShipperAddressInfo, addressDescriptionWarning);
			AssertHasWarning(Pesticide.US_OA_ShipperAddressInfo, addressCodeWarning);
			Pesticide.US_OA_ShipperAddress = orgAddress2.PK;
			AssertNoWarning(Pesticide.US_OA_ShipperAddressInfo, addressDescriptionWarning);
			AssertNoWarning(Pesticide.US_OA_ShipperAddressInfo, addressCodeWarning);
		}

		public void TestCheckUS_PSTLabelsSent()
		{
			Pesticide.AddInfoValidation.ValidateUS_PSTLabelsSent();
			AssertHasMessageErrorContaining(Pesticide.US_PSTLabelsSentInfo, USPSTAddInfoValidation.LabelRequired);

			Pesticide.US_PSTLabelsSent = true;
			AssertNoMessageErrorContaining(Pesticide.US_PSTLabelsSentInfo, USPSTAddInfoValidation.LabelRequired);
		}

		public void TestCheckUS_IntendedUseCode()
		{
			Pesticide.US_IntendedUseCode = "ABC";
			AssertHasMessageErrorContaining(Pesticide.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			Pesticide.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._130026;
			AssertNoMessageErrors(Pesticide.US_IntendedUseCodeInfo);
		}

		public void TestCheckUS_ProductType()
		{
			Pesticide.US_ProductType = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_ProductTypeInfo, MandatoryValidation.YouHaveNotEntered);

			Pesticide.US_ProductType = "ABC";
			AssertNoMessageErrorContaining(Pesticide.US_ProductTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Pesticide.US_ProductTypeInfo, ListValidation.InvalidCodeMessageError);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			AssertHasMessageError(Pesticide.US_ProductTypeInfo, USPSTAddInfoValidation.LinesMissing);

			var detail0 = Pesticide.PesticideLines.AddNew();
			var detail1 = Pesticide.PesticideLines.AddNew();

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS2;
			AssertHasMessageError(Pesticide.US_ProductTypeInfo, USPSTAddInfoValidation.NoLinesAllowedForPS2);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			AssertNoMessageError(Pesticide.US_ProductTypeInfo, USPSTAddInfoValidation.LinesMissing);

			Pesticide.PesticideLines.RemoveAndDeleteAll();
			Pesticide.AddInfoValidation.ValidateUS_ProductType();
			AssertHasMessageError(Pesticide.US_ProductTypeInfo, USPSTAddInfoValidation.LinesMissing);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			Pesticide.AddInfoValidation.ValidateUS_ProductType();
			AssertHasMessageError(Pesticide.US_ProductTypeInfo, USPSTAddInfoValidation.LinesMissing);
		}

		public void TestCheckUS_UnregReasonCode()
		{
			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS2;

			Pesticide.US_UnregReasonCode = ZString.Empty;
			AssertNoMessageErrors(Pesticide.US_UnregReasonCodeInfo);
			Pesticide.US_UnregReasonCode = "ABC";
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.EEX;
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, MandatoryValidation.DoNotEntered);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;

			Pesticide.US_UnregReasonCode = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_UnregReasonCode = "ABC";
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			AssertNoMessageErrors(Pesticide.US_UnregReasonCodeInfo);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			Pesticide.US_UnregReasonCode = "EP5";
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, MandatoryValidation.DoNotEntered);

			Pesticide.US_UnregReasonCode = ZString.Empty;
			AssertNoMessageError(Pesticide.US_UnregReasonCodeInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckUS_UnregReasonCodeReasonRemarks()
		{
			var messageRessonCode = "You have not entered an Unregistered Reason Code.";
			var messageRessonRemarks = "You have not entered a General Remarks.";

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			Pesticide.US_UnregReasonRemarks = "Remarks";
			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonCode();
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonRemarks();

			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonRemarksInfo, USPSTAddInfoValidation.NowAllowBothUnregReasonCodeAndUnregReasonRemarksEntered);
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, USPSTAddInfoValidation.NowAllowBothUnregReasonCodeAndUnregReasonRemarksEntered);

			Pesticide.US_UnregReasonCode = ZString.Empty;
			Pesticide.US_UnregReasonRemarks = ZString.Empty;
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonCode();
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonRemarks();
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, messageRessonCode);
			AssertHasMessageErrorContaining(Pesticide.US_UnregReasonRemarksInfo, messageRessonRemarks);

			Pesticide.US_UnregReasonRemarks = ZString.Empty;
			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.EUP;
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonCode();
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonRemarks();
			AssertNoMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, messageRessonCode);
			AssertNoMessageErrorContaining(Pesticide.US_UnregReasonRemarksInfo, messageRessonRemarks);

			Pesticide.US_UnregReasonRemarks = "Remarks";
			Pesticide.US_UnregReasonCode = ZString.Empty;
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonCode();
			Pesticide.AddInfoValidation.ValidateUS_UnregReasonRemarks();
			AssertNoMessageErrorContaining(Pesticide.US_UnregReasonCodeInfo, messageRessonCode);
			AssertNoMessageErrorContaining(Pesticide.US_UnregReasonRemarksInfo, messageRessonRemarks);
		}

		public void TestCheckUS_BrandName()
		{
			Pesticide.US_BrandName = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_BrandName = "ABC";
			AssertNoMessageErrors(Pesticide.US_BrandNameInfo);
		}

		public void TestCheckUS_LPCONumber()
		{
			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			Pesticide.US_LPCONumber = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_LPCONumberInfo, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_LPCONumber = "ABC";
			AssertNoMessageErrors(Pesticide.US_LPCONumberInfo);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS2;
			Pesticide.US_LPCONumber = ZString.Empty;
			AssertNoMessageErrors(Pesticide.US_LPCONumberInfo);
			Pesticide.US_LPCONumber = "ABC";
			AssertNoMessageErrors(Pesticide.US_LPCONumberInfo);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			Pesticide.US_LPCONumber = ZString.Empty;
			AssertNoMessageErrors(Pesticide.US_LPCONumberInfo);
			Pesticide.US_LPCONumber = "ABC";
			AssertNoMessageErrors(Pesticide.US_LPCONumberInfo);
		}

		public void TestCheckUS_ProducerEstNo()
		{
			Pesticide.AddInfoValidation.ValidateUS_ProducerEstNo();
			AssertNoMessageErrorContaining(Pesticide.US_ProducerEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.TR1;
			Pesticide.AddInfoValidation.ValidateUS_ProducerEstNo();
			AssertNoMessageErrorContaining(Pesticide.US_ProducerEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			Pesticide.AddInfoValidation.ValidateUS_ProducerEstNo();
			AssertHasMessageErrorContaining(Pesticide.US_ProducerEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.TR2;
			Pesticide.AddInfoValidation.ValidateUS_ProducerEstNo();
			AssertHasMessageErrorContaining(Pesticide.US_ProducerEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			Pesticide.US_ProducerEstNo = "ABC";
			AssertNoMessageErrorContaining(Pesticide.US_ProducerEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("US_ProducerEstNo Format", Pesticide.US_ProducerEstNoInfo, USPSTAddInfoValidation.ProducerEstNoFormat);

			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			Pesticide.US_ProducerEstNo = ZString.Empty;
			AssertNoMessageErrorContaining(Pesticide.US_ProducerEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			Pesticide.US_ProducerEstNoForeign = "123456ABC123";
			AssertNoMessageErrorContaining("US_ProducerEstNo Format", Pesticide.US_ProducerEstNoInfo, USPSTAddInfoValidation.ProducerEstNoFormat);
		}

		public void TestCheckUS_ProducerEstNoForeign()
		{
			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			Pesticide.AddInfoValidation.ValidateUS_ProducerEstNoForeign();
			AssertHasMessageErrorContaining(Pesticide.US_ProducerEstNoForeignInfo, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_ProducerEstNoForeign = "ABC";
			AssertNoMessageErrorContaining(Pesticide.US_ProducerEstNoForeignInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Pesticide.US_ProducerEstNoForeignInfo, USPSTAddInfoValidation.ProducerEstNoForeignFormat);

			Pesticide.US_ProducerEstNoForeign = "123456ABC123";
			AssertNoMessageErrorContaining(Pesticide.US_ProducerEstNoForeignInfo, USPSTAddInfoValidation.ProducerEstNoForeignFormat);
		}

		public void TestCheckUS_OA_ExaminationLocation()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			Pesticide.US_OA_ExaminationLocation = ZGuid.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = "KR";

			Pesticide.US_OA_ExaminationLocation = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact");
			AssertHasMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, USPSTAddInfoValidation.ExaminationLocationShouldBeUSAddress);

			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			Pesticide.AddInfoValidation.ValidateUS_OA_ExaminationLocation();
			AssertNoMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, USPSTAddInfoValidation.ExaminationLocationShouldBeUSAddress);

			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "CASYD";
			Pesticide.US_OA_ExaminationLocation = address.PK;
			Pesticide.AddInfoValidation.ValidateUS_OA_ExaminationLocation();
			AssertHasMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Pesticide.US_OA_ExaminationLocation = address.PK;
			Pesticide.AddInfoValidation.ValidateUS_OA_ExaminationLocation();
			AssertNoMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Pesticide.US_OA_ExaminationLocation = address.PK;
			Pesticide.AddInfoValidation.ValidateUS_OA_ExaminationLocation();
			AssertNoMessageErrorContaining(Pesticide.US_OA_ExaminationLocationInfo, "The state is not a valid");
		}

		public void TestCheckUS_UQ1()
		{
			Pesticide.US_NoOfUnit1 = 15m;
			Pesticide.US_UQ1 = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_UQ1Info, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_UQ1 = "ABC";
			AssertHasMessageErrorContaining(Pesticide.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			Pesticide.US_UQ1 = "BT";
			AssertNoMessageErrors(Pesticide.US_UQ1Info);

			Pesticide.US_UQ1 = FDAUQList.Codes.CAG;
			AssertNoMessageErrors(Pesticide.US_UQ1Info);
		}

		public void TestCheckUS_NetWeight()
		{
			Pesticide.US_NetWeight = -1;
			AssertHasMessageError(Pesticide.US_NetWeightInfo, USPSTAddInfoValidation.EnterNumberGreaterThanZero);

			Pesticide.US_NetWeight = 0;
			AssertHasMessageError(Pesticide.US_NetWeightInfo, USPSTAddInfoValidation.NetWeightCannotBeBlank);

			Pesticide.US_NetWeight = 1;
			AssertNoMessageError(Pesticide.US_NetWeightInfo, USPSTAddInfoValidation.EnterNumberGreaterThanZero);
			AssertNoMessageError(Pesticide.US_NetWeightInfo, USPSTAddInfoValidation.NetWeightCannotBeBlank);
		}

		public void TestCheckUS_WeightUQ()
		{
			var expectedToHave = new List<string> {
				Core.Constants.Weight.Grams,
				Core.Constants.Weight.Kilograms,
				Core.Constants.Weight.Milligrams,
				Core.Constants.Weight.Ounces,
				"OTL",	//Quarts
				"ML",	//Milliliter
				"GAL"	//Gallons
			};

			var shouldNotHave = new List<string>
			{
				Core.Constants.Weight.Decitons,
				Core.Constants.Weight.Hectograms,
				Core.Constants.Weight.Kilotonnes,
				Core.Constants.Weight.Pounds,
				Core.Constants.Weight.PoundsTroy,
				Core.Constants.Weight.MetricCarat,
				Core.Constants.Weight.OuncesTroy,
				Core.Constants.Weight.Tonnes,
				Core.Constants.Weight.LongTons,
				Core.Constants.Weight.ShortTons,
			};

			var messageError = "The code you have selected is not in the list.";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1000m;

			var epaLine = invoiceLine.PSTLines.AddNew();

			foreach (var weight in expectedToHave)
			{
				epaLine.US_WeightUQ = weight;
				AssertNoMessageError(epaLine.US_WeightUQInfo, messageError);
			}

			foreach (var weight in shouldNotHave)
			{
				epaLine.US_WeightUQ = weight;
				AssertHasMessageError(epaLine.US_WeightUQInfo, messageError);
			}
		}

		public void TestCheckUS_CertifyingIndividual()
		{
			Pesticide.US_CertifyingIndividual = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_CertifyingIndividualInfo, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_CertifyingIndividual = "~";
			AssertHasMessageErrorContaining(Pesticide.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);
			Pesticide.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			AssertNoMessageErrors(Pesticide.US_CertifyingIndividualInfo);
		}

		public void TestCheckUS_NotifyParty()
		{
			Pesticide.US_NotifyParty = "~";
			AssertHasMessageErrorContaining(Pesticide.US_NotifyPartyInfo, ListValidation.InvalidCodeMessageError);
			Pesticide.US_NotifyParty = PartyTypeList.Codes.CustomsBroker;
			AssertNoMessageErrors(Pesticide.US_NotifyPartyInfo);

			Pesticide.US_NotifyParty = ZString.Empty;
			AssertHasMessageErrorContaining(Pesticide.US_NotifyPartyInfo, MandatoryValidation.YouHaveNotEntered);
			Pesticide.US_NotifyParty = PartyTypeList.Codes.CustomsBroker;
			AssertNoMessageErrorContaining(Pesticide.US_NotifyPartyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_OA_ShipperAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			Pesticide.US_OA_ShipperAddress = ZGuid.Empty;
			AssertHasMessageError(Pesticide.US_OA_ShipperAddressInfo, USPSTAddInfoValidation.ShipperRequired);

			var shipper = Factory.New<OrgHeader>();
			var shipperAddress = shipper.Addresses.AddNew();
			Pesticide.US_OA_ShipperAddress = shipperAddress.PK;
			AssertNoMessageError(Pesticide.US_OA_ShipperAddressInfo, USPSTAddInfoValidation.ShipperRequired);
			AssertHasMessageErrorContaining(Pesticide.US_OA_ShipperAddressInfo, "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.");

			var address = shipper.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "CASYD";
			Pesticide.US_OA_ShipperAddress = address.PK;
			Pesticide.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertHasMessageErrorContaining(Pesticide.US_OA_ShipperAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Pesticide.US_OA_ShipperAddress = address.PK;
			Pesticide.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageErrorContaining(Pesticide.US_OA_ShipperAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			Pesticide.US_OA_ShipperAddress = address.PK;
			Pesticide.AddInfoValidation.ValidateUS_OA_ShipperAddress();
			AssertNoMessageErrorContaining(Pesticide.US_OA_ShipperAddressInfo, "The state is not a valid");
		}

		public void TestPSTPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_CertifyCargoRelease = true;
			var invoiceLineCollection = declaration.Invoices.AddNew();
			var invoiceLine = invoiceLineCollection.InvoiceLines.AddNew();
			invoiceLine.US_PSTIndicator = "D";
			var pesticide = invoiceLine.PSTLines.AddNew();

			pesticide.US_NoOfUnit1 = ZDecimal.Zero;
			pesticide.US_NoOfUnit2 = 10;
			AssertHasMessageErrorContaining(pesticide.US_NoOfUnit1Info, "The largest package quantity is required.");
			AssertNoMessageErrorContaining(pesticide.US_NoOfUnit2Info, "The second largest package quantity is required.");

			pesticide.US_NoOfUnit1 = 10;
			pesticide.US_NoOfUnit2 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(pesticide.US_NoOfUnit1Info, "The largest package quantity is required.");
			AssertHasMessageErrorContaining(pesticide.US_NoOfUnit2Info, "The second largest package quantity is required.");

			pesticide.US_NoOfUnit1 = 10;
			pesticide.US_UQ1 = ZString.Empty;
			AssertHasMessageErrorContaining(pesticide.US_UQ1Info, "You have not entered a value.");

			pesticide.US_UQ1 = "AE";
			AssertNoMessageErrorContaining(pesticide.US_UQ1Info, "You have not entered a value.");
			AssertEquals("Total 10.00 AE", pesticide.PSTQtyRunningTotal);

			pesticide.US_NoOfUnit2 = 2;
			pesticide.US_UQ2 = "AM";
			AssertEquals("Total 20.00 AM", pesticide.PSTQtyRunningTotal);

			pesticide.US_NoOfUnit1 = ZDecimal.Zero;
			pesticide.US_UQ1 = ZString.Empty;

			pesticide.US_NoOfUnit2 = ZDecimal.Zero;
			pesticide.US_UQ2 = ZString.Empty;
			AssertEquals(ZString.Empty, pesticide.PSTQtyRunningTotal);
		}

		#region Implementation

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					fInvoiceLine = invoiceHeader.InvoiceLines.AddNew();
				}

				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		Pesticide Pesticide
		{
			get
			{
				if (fPesticide == null)
				{
					fPesticide = InvoiceLine.PSTLines.AddNew();
				}

				return fPesticide;
			}
		}
		Pesticide fPesticide;

		#endregion
	}
}
