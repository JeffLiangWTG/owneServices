using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USFSISLotAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLists()
		{
			AssertType<FSISProductSpeciesNameList>(lookups.ProductSpeciesNames);
			Assert(lookups.ProductSpeciesNames.ContainsCode(FSISProductSpeciesNameList.Codes.SiluriformesCatfishMeat));
			Assert(lookups.ProductSpeciesNames.ContainsCode(FSISProductSpeciesNameList.Codes.SiluriformesOtherMeat));
			AssertType<FSISProductQualifierCodeList>(lookups.ProductQualifierCodes);

			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			AssertType<EEPCharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.FCNS;
			AssertType<FCNSCharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.HTSS;
			AssertType<HTSSCharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.NFC;
			AssertType<NFCCharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.NHTS;
			AssertType<NHTSCharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.PWSI;
			AssertType<PWSICharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.RPI;
			AssertType<RPICharacteristicList>(lookups.ProductCharacteristics);
			AssertEquals(35, lookups.ProductCharacteristics.Count);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.RPNI;
			AssertType<RPNICharacteristicList>(lookups.ProductCharacteristics);
			AssertEquals(39, lookups.ProductCharacteristics.Count);
			fsisAddInfo.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.TPCS;
			AssertType<TPCSCharacteristicList>(lookups.ProductCharacteristics);
			fsisAddInfo.US_ProductQualifierCode = ZString.Empty;
			AssertType<CodeDescriptionPairList>(lookups.ProductCharacteristics);
		}

		protected override void SetUp()
		{
			base.SetUp();
			fsisAddInfo = (USFSISLotAddInfo)((Customs.Business.IAddInfoManager)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew().Lots.AddNew()).AddInfo;
			lookups = fsisAddInfo.Lookups;
		}
		USFSISLotAddInfo fsisAddInfo;
		USFSISLotAddInfoLookups lookups;
	}
}
