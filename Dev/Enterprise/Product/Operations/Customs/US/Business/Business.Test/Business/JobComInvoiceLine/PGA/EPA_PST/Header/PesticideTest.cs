using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Pesticide))]
	public class PesticideTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<Pesticide>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<Pesticide>();
			originalBO.PesticideLines.AddNew();

			var newBO = (Pesticide)originalBO.Clone();

			AssertEquals(1, newBO.PesticideLines.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (Pesticide)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(Pesticide), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.PesticideLines[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.PesticideLines[0].Factory.GetHashCode());
		}

		public void TestPGALineReadOnly()
		{
			Pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			Pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.DSP;
			Pesticide.US_UnregReasonRemarks = "NONE";
			Pesticide.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			Factory.Save();
			Pesticide.OnLoaded();
			Assert(!Pesticide.ReadOnly);

			Pesticide.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			Pesticide.OnLoaded();
			Assert(Pesticide.ReadOnly);

			Pesticide.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			Pesticide.OnLoaded();
			Assert(!Pesticide.ReadOnly);

			Pesticide.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			Pesticide.OnLoaded();
			Assert(Pesticide.ReadOnly);

			Pesticide.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			Pesticide.OnLoaded();
			Assert(Pesticide.ReadOnly);
		}

		public void TestCertifySignatureDate()
		{
			Pesticide.InvoiceHeader.US_PSTSignDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, ((IPSTData)Pesticide).CertifySignatureDate);

			var expectedDate = new ZDate(2017, 01, 01);
			Pesticide.InvoiceHeader.US_PSTSignDate = expectedDate;
			AssertEquals(expectedDate, ((IPSTData)Pesticide).CertifySignatureDate);

			Pesticide.InvoiceHeader.US_PSTSignDate = ZDateTime.Empty;
			((IPSTData)Pesticide).CertifySignatureDate = expectedDate;
			AssertEquals(expectedDate, ((IPSTData)Pesticide).CertifySignatureDate);

			((IPSTData)Pesticide).CertifySignatureDate = new ZDate(2017, 05, 05);
			AssertEquals(new ZDate(2017, 05, 05), Pesticide.InvoiceHeader.US_PSTSignDate.Date);
		}

		public void TestDeclarationCertificate()
		{
			Pesticide.InvoiceHeader.US_PSTSignDate = ZDateTime.Empty;
			AssertEquals("", ((IPSTData)Pesticide).DeclarationCertificate);

			Pesticide.InvoiceHeader.US_PSTSignDate = ZDateTime.Today;
			AssertEquals("Y", ((IPSTData)Pesticide).DeclarationCertificate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Pesticide;
		}

		protected override IEnumerable<Pesticide> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (Pesticide)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.PSTLines.AddNew();
		}

		[TestDate(2016, 12, 1)]
		public void TestCarrierAddressSelection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var carrier = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = carrier.PK;

			carrier.OH_FullName = "Carrier";
			var address = carrier.MainAddress;
			address.OA_Address1 = "ADDRESS 1";
			address.OA_City = "SYDNEY";
			address.OA_State = "NSW";
			address.OA_PostCode = "2017";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_RN_NKCountryCode = "AU";

			var address2 = carrier.AddressesActive.AddNew();
			address2.OA_Address1 = "ADDRESS 1";
			address2.OA_City = "CHICAGO";
			address2.OA_State = "IL";
			address2.OA_PostCode = "60010";
			address2.OA_RL_NKRelatedPortCode = "USCHI";
			address2.OA_RN_NKCountryCode = "US";

			DeclarationTestHelper.AddPGAContact(address2, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_PSTIndicator = "D";
			invoiceLine.JI_Description = "ABCDEFG DESC";
			var pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._130026;
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			pesticide.US_UnregReasonRemarks = "TEST REMARKS TEXT";
			pesticide.US_BrandName = "UC-HDO";
			pesticide.US_ProducerEstNo = "0123456";
			pesticide.US_ProducerEstNoForeign = "1234567";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = (EntryHeaderMessageSendingAction)actions[0];
			var builder = new MessageBuilders.ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			AssertContains(
@"PG19CAR                  CARRIER                         ADDRESS 1              
PG20                                     CHICAGO              IL US60010        
PG21CARIOR ALEXANDER THE GREAT04123456       IOR EMAIL                          ", message.EM_FormattedMessageText);
		}

		public void TestShipperAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var invoice = declaration.Invoices.AddNew();

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Seller";
			seller.MainAddress.OA_Address1 = "ADDRESS 1";
			seller.MainAddress.OA_City = "SYDNEY";
			seller.MainAddress.OA_State = "NSW";
			seller.MainAddress.OA_PostCode = "2017";
			seller.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			seller.MainAddress.OA_RN_NKCountryCode = "AU";

			var address2 = seller.Addresses.AddNew();
			address2.OA_Address1 = "ADDRESS 2";
			address2.OA_City = "CHICAGO";
			address2.OA_State = "IL";
			address2.OA_PostCode = "60010";
			address2.OA_RL_NKRelatedPortCode = "USCHI";
			address2.OA_RN_NKCountryCode = "US";

			invoice.JZ_OA_SellerAddress = seller.MainAddress.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_PSTIndicator = "D";
			invoiceLine.JI_Description = "DESCRIPTION";

			var pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._130026;
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			AssertEquals("Fallback to invoice seller main address", seller.MainAddress, pesticide.US_OA_ShipperAddress_ZAddress.OrgAddress);
			AssertEquals("Fallback to invoice seller main address", seller.MainAddress.PK, pesticide.US_OA_ShipperAddress);

			var seller2 = Factory.New<OrgHeader>();
			seller2.OH_FullName = "Seller 2";
			seller2.MainAddress.OA_Address1 = "ADDRESS for seller 2";
			seller2.MainAddress.OA_City = "SYDNEY";
			seller2.MainAddress.OA_State = "NSW";

			var seller2Address2 = seller2.Addresses.AddNew();
			pesticide.US_OA_ShipperAddress = seller2Address2.PK;
			AssertEquals(seller2Address2, pesticide.US_OA_ShipperAddress_ZAddress.OrgAddress);
			AssertEquals(seller2Address2.PK, pesticide.US_OA_ShipperAddress);
		}

		public void TestClone()
		{
			var invoiceLine = Pesticide.InvoiceLine;

			var pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.DSP;
			pesticide.US_UnregReasonRemarks = "NONE";
			pesticide.US_LPCONumber = "TTA4";
			pesticide.US_ProducerEstNoForeign = "JI5";
			pesticide.US_ProducerEstNo = "RE3";
			pesticide.US_OA_ExaminationLocation = ZGuid.Empty;
			pesticide.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._150000;
			pesticide.ShipperOrgPK = ZGuid.Empty;
			pesticide.US_NoOfUnit1 = 5m;
			pesticide.US_UQ1 = "AE";
			pesticide.US_OA_ShipperAddress = ZGuid.Empty;
			pesticide.US_NoOfUnit2 = 4m;
			pesticide.US_UQ2 = "AP";
			pesticide.US_NetWeight = 5m;
			pesticide.US_WeightUQ = "KG";
			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			pesticide.US_PSTLabelsSent = ZBool.True;

			var pesticideLine = pesticide.PesticideLines.AddNew();
			pesticideLine.US_NameOfActiveIngredient = "TOLUENE";
			pesticideLine.US_ActiveIngredientPercentage = 12m;
			pesticideLine.US_LPCOType = ProductCodeQualifiersList.Codes.PCCode;
			pesticideLine.US_LPCONumber = "A54";

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var pesticideNew = invoiceLine.PSTLines.AddNew();

			AssertEquals("ProductType must be the same", pesticide.US_ProductType, pesticideNew.US_ProductType);
			AssertEquals("UnregReasonCode must be the same", pesticide.US_UnregReasonCode, pesticideNew.US_UnregReasonCode);
			AssertEquals("UnregReasonRemarks must be the same", pesticide.US_UnregReasonRemarks, pesticideNew.US_UnregReasonRemarks);
			AssertEquals("LPCONumber must be the same", pesticide.US_LPCONumber, pesticideNew.US_LPCONumber);
			AssertEquals("ProducerEstNoForeign must be the same", pesticide.US_ProducerEstNoForeign, pesticideNew.US_ProducerEstNoForeign);
			AssertEquals("ProducerEstNo must be the same", pesticide.US_ProducerEstNo, pesticideNew.US_ProducerEstNo);
			AssertEquals("OH_ExaminationLocation must be the same", pesticide.US_OA_ExaminationLocation, pesticideNew.US_OA_ExaminationLocation);
			AssertEquals("IntendedUseCode must be the same", pesticide.US_IntendedUseCode, pesticideNew.US_IntendedUseCode);
			AssertEquals("ShipperOrgPK must be the same", pesticide.ShipperOrgPK, pesticideNew.ShipperOrgPK);
			AssertEquals("NoOfUnit1 must be the same", pesticide.US_NoOfUnit1, pesticideNew.US_NoOfUnit1);
			AssertEquals("UQ1 must be the same", pesticide.US_UQ1, pesticideNew.US_UQ1);
			AssertEquals("OA_ShipperAddress must be the same", pesticide.US_OA_ShipperAddress, pesticideNew.US_OA_ShipperAddress);
			AssertEquals("NoOfUnit2 must be the same", pesticide.US_NoOfUnit2, pesticideNew.US_NoOfUnit2);
			AssertEquals("UQ2 must be the same", pesticide.US_UQ2, pesticideNew.US_UQ2);
			AssertEquals("NetWeight must be the same", pesticide.US_NetWeight, pesticideNew.US_NetWeight);
			AssertEquals("WeightUQ must be the same", pesticide.US_WeightUQ, pesticideNew.US_WeightUQ);
			AssertEquals("CertifyingIndividual must be Importer", PartyTypeList.Codes.Importer, pesticideNew.US_CertifyingIndividual);
			AssertEquals("NotifyParty must be Customs Broker", PartyTypeList.Codes.CustomsBroker, pesticideNew.US_NotifyParty);
			AssertEquals("PSTLabelsSent must false", ZBool.False, pesticideNew.US_PSTLabelsSent);

			AssertEquals("PesticideLine: NameOfActiveIngredient must be the same", pesticide.PesticideLines[0].US_NameOfActiveIngredient, pesticideNew.PesticideLines[0].US_NameOfActiveIngredient);
			AssertEquals("PesticideLine: ActiveIngredientPercentage must be the same", pesticide.PesticideLines[0].US_ActiveIngredientPercentage, pesticideNew.PesticideLines[0].US_ActiveIngredientPercentage);
			AssertEquals("PesticideLine: LPCOType must be the same", pesticide.PesticideLines[0].US_LPCOType, pesticideNew.PesticideLines[0].US_LPCOType);
			AssertEquals("PesticideLine: LPCONumber must be the same", pesticide.PesticideLines[0].US_LPCONumber, pesticideNew.PesticideLines[0].US_LPCONumber);
		}

		Pesticide Pesticide
		{
			get
			{
				if (pesticide == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pesticide = invoiceLine.PSTLines.AddNew();
				}
				return pesticide;
			}
		}
		Pesticide pesticide;
	}
}
