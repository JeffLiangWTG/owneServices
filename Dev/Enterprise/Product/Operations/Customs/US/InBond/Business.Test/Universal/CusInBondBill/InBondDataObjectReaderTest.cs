using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest
	{
		public void TestImportingInBondBillData()
		{
			var billDataObject = SetupInBondBill("OB2343", WayBillTypeList.Codes.Master, ZString.Empty, 1);
			billDataObject.OrganizationAddressCollection = new List<OrganizationAddress>(new[] { SetupOrganizationAddress(nameof(DocAddressType.ForeignShipperDocumentaryAddress)), SetupOrganizationAddress2(nameof(DocAddressType.ConsigneeAddress)), GetNewAddressData_WUFSHIJNB(DocAddressType.NotifyParty) });
			billDataObject.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[] { SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.IN, "IN324"), SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.CG, "CG986"), SetupCustomsReferenceForAdditionalReferenceType(ReferenceQualifierList.Codes.BL, "BL362") });
			var header = Factory.New<CusInBondHeader>();
			Factory.SaveForTesting();
			var reader = new CusInBondBillDataObjectReader(billDataObject, logger, Factory, Helper, header);
			var billBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);
			AssertNotNull(billBO);
			CombineAssertions(delegate
			{
				AssertCusInBondBillContents(billBO, "OB2343", ZString.Empty);
				AssertEquals("billBO.B0_BH", header.PK, billBO.B0_BH);
				AssertContents(billBO.ForeignShipper, addresOverride: ZBool.True, orgAddressPK: OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);
				AssertContents2(billBO.Consignee, addresOverride: ZBool.True, orgAddressPK: OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);
				AssertJobDocAddressContentMatches_WUFSHIJNB(billBO.NotifyParty);
				AssertEquals(3, billBO.AdditionalReferences.Count);
				AssertContains(ReferenceQualifierList.Codes.IN, "IN324", billBO.AdditionalReferences);
				AssertContains(ReferenceQualifierList.Codes.CG, "CG986", billBO.AdditionalReferences);
				AssertContains(ReferenceQualifierList.Codes.BL, "BL362", billBO.AdditionalReferences);
				AssertMultilineASCIIEquals("logger.Logs", @" 
Information - No matching CusInBondBill found, creating new CusInBondBill.
Information - Populating CusInBondBill...
Warning - Matching 'ForeignShipperDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'NotifyParty':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Information - Successfully saved Bill Of Lading XKD3 OB2343.".Trim(), logger.Logs);
			});
		}

		AdditionalBill SetupInBondBill(ZString billNumber, ZString billType, ZString parentBillNumber, ZDecimal manifestQty, ZString manifestUQ, ZDecimal weight, ZString weightUQ, ZDecimal volume,
			ZString volumeUQ, ZString portOfLadingKCode, ZString placeOfReceipt, ZString issuerCode, ZInt link)
		{
			var result = SetupInBondBill(billNumber, billType, parentBillNumber, manifestQty, link);
			result.AddInfoCollection = new List<AddInfo>()
			{ new AddInfo()
			{ Key = Constants.Bill.AddInfo.ManifestUnit, Value = manifestUQ }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.Weight, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(weight) }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.WeightUnit, Value = weightUQ }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.Volume, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(volume) }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.VolumeUnit, Value = volumeUQ }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.PortOfLadingScheduleK, Value = portOfLadingKCode }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.PlaceOfReceiptScheduleD, Value = placeOfReceipt }, new AddInfo()
			{ Key = Constants.Bill.AddInfo.IssuerCode, Value = issuerCode } };
			return result;
		}

		void AssertCusInBondBillContents(CusInBondBill billBO, ZString masterBillNumber, ZString houseBillNumber)
		{
			AssertCusInBondBillContents(billBO, masterBillNumber, 110, InBondManifestUQList.Codes.BAG, 1500m, Core.Constants.Weight.Kilograms, 1.5m, Core.Constants.Volume.CubicMetres, SeaForeignPort2ScheduleK.ZZD_Code, SeaLocalPort2ScheduleD.ZZD_Code, houseBillNumber, "XKD3");
		}

		void AssertCusInBondBillContents(CusInBondBill billBO, ZString masterBillNumber, ZInt manifestQty, ZString manifestUQ, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ, ZString portOfLadingKCode, ZString placeOfReceipt, ZString houseBillNumber, ZString issuerCode)
		{
			AssertEquals("billBO.B0_MasterBillNumber", masterBillNumber, billBO.B0_MasterBillNumber);
			AssertEquals("billBO.B0_ManifestQty", manifestQty, billBO.B0_ManifestQty);
			AssertEquals("billBO.B0_ManifestUQ", manifestUQ, billBO.B0_ManifestUQ);
			AssertEquals("billBO.B0_Weight", weight, billBO.B0_Weight);
			AssertEquals("billBO.B0_WeightUQ", weightUQ, billBO.B0_WeightUQ);
			AssertEquals("billBO.B0_Volume", volume, billBO.B0_Volume);
			AssertEquals("billBO.B0_VolumeUQ", volumeUQ, billBO.B0_VolumeUQ);
			AssertEquals("billBO.B0_PortOfLadingKCode", portOfLadingKCode, billBO.B0_PortOfLadingKCode);
			AssertEquals("billBO.B0_PlaceOfReceipt", placeOfReceipt, billBO.B0_PlaceOfReceipt);
			AssertEquals("billBO.B0_HouseBillNumber", houseBillNumber, billBO.B0_HouseBillNumber);
			AssertEquals("billBO.B0_IssuerCode", issuerCode, billBO.B0_IssuerCode);
		}

		UniversalCustoms.CustomsReference SetupCustomsReferenceForAdditionalReferenceType(ZString? code, ZString? reference)
		{
			return new UniversalCustoms.CustomsReference()
			{
				Type = new CodeDescriptionPair()
				{ Code = Constants.AdditionalReference.Type, Description = Constants.AdditionalReference.TypeDescription },
				SubType = new CodeDescriptionPair35Char()
				{ Code = code },
				Reference = reference,
			};
		}

		void AssertContains(ZString qualifier, ZString reference, CusInbondBillAddRefCollection collection)
		{
			AssertNotNull(string.Format("Additional Reference (Q:{0}, R:{1})", qualifier, reference), collection.FirstOrDefault(x => x.BR_Qualifier == qualifier && x.BR_ReferenceNum == reference));
		}

		AdditionalBill SetupInBondBill(ZString billNumber, ZString billType, ZString parentBillNumber, ZInt link)
		{
			return SetupInBondBill(billNumber, billType, parentBillNumber, 110m, InBondManifestUQList.Codes.BAG, 1500m, Core.Constants.Weight.Kilograms, 1.5m, Core.Constants.Volume.CubicMetres, SeaForeignPort2ScheduleK.ZZD_Code, SeaLocalPort2ScheduleD.ZZD_Code, "XKD3", link);
		}
	}
}
