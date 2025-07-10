using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DOTIDOTTest : TestCaseWithFactory
	{
		public void TestIDOTMembers()
		{
			DOT dot = Factory.New<DOT>();
			dot.US_DOTCommercialDesc = "HELLO WORLD";
			dot.US_DOTBoxNo = "A2";
			dot.US_DOTPassport = "PS323";
			dot.US_DOTCountryOfOrigin = "AU";
			dot.US_DOTBondSuretyCode = "795";
			dot.US_DOTPriorApproval = ZBool.True;
			dot.US_DOTImpSubstStatement = ZBool.False;
			dot.US_DOTClarCode = "V";
			dot.US_DOTTireID = "D2";
			dot.US_DOTTireBrandName = "B2";
			var vin1 = dot.DOTVINs.AddNew();
			vin1.US_DOTVIN = "VIN1";
			var vin2 = dot.DOTVINs.AddNew();
			vin2.US_DOTVIN = "VIN2";
			DOTIDOT iDOT = new DOTIDOT(dot, true);
			AssertEquals("HELLO WORLD", iDOT.CommercialDescription);
			AssertEquals("Y", iDOT.BoxCertification);
			AssertEquals("PS323", iDOT.PassportNumber);
			AssertEquals("A2", iDOT.BoxNumber);
			AssertEquals("AU", iDOT.CountryISO);
			AssertEquals("795", iDOT.DOTBondSuretyCode);
			AssertEquals(ZBool.True, iDOT.NHTSAPermissionLetterOfficialOrdersCertification);
			AssertEquals(ZBool.False, iDOT.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter);
			AssertEquals("V", iDOT.ClarificationCode);
			AssertEquals("D2", iDOT.TireManufacturerIDCode);
			AssertEquals("B2", iDOT.TireManufacturerBrandName);
			AssertEquals("HELLO WORLD", iDOT.CommercialDesc);
			List<IDOTVIN> vins = new List<IDOTVIN>(iDOT.VINs);
			AssertEquals(2, vins.Count);
			IDOTVIN iDotVin1 = vins[0];
			IDOTVIN iDotVin2 = vins[1];
			if (iDotVin1.VehicleIdentificationNumber == "VIN2")
			{
				iDotVin1 = vins[1];
				iDotVin2 = vins[0];
			}
			AssertEquals("VIN1", iDotVin1.VehicleIdentificationNumber);
			AssertEquals("VIN2", iDotVin2.VehicleIdentificationNumber);

			iDOT = new DOTIDOT(dot, false);
			vins = new List<IDOTVIN>(iDOT.VINs);
			AssertEquals(0, vins.Count);

			iDOT = new DOTIDOT(dot, true, 324);
			vins = new List<IDOTVIN>(iDOT.VINs);
			AssertEquals(1, vins.Count);
			var vin = vins[0];
			AssertEquals("324".PadLeft(17, '0'), vin.VehicleIdentificationNumber);
		}
	}
}
