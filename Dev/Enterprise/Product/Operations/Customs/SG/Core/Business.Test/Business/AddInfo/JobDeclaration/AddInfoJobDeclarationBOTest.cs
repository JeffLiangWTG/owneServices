using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AddInfoJobDeclaration))]
	public class AddInfoJobDeclarationBOTest : AddInfoBOTest
	{
		public void TestCertificateCurrencyDefaults()
		{
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = "";
			AddInfoJobDeclaration.SG_ApplicationProductType = "";
			AssertEquals(true, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency.IsEmpty);
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency);
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = Core.Constants.CurrencyCodes.HongKong;
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			AssertEquals(Core.Constants.CurrencyCodes.HongKong, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency);
		}

		public void TestCertificateCurrencyRedefaultsIfRequired()
		{
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = "";
			AddInfoJobDeclaration.SG_ApplicationProductType = "";
			AssertEquals(true, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency.IsEmpty);
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AssertEquals("Currency will default to 'SGD' if COO details are entered.", Core.Constants.CurrencyCodes.Singapore, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency);
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = "";
			AssertEquals("Currency will re-default to 'SGD' when blanked out if COO details are entered.", Core.Constants.CurrencyCodes.Singapore, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency);
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = Core.Constants.CurrencyCodes.HongKong;
			AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency = "";
			AssertEquals("Currency will re-default to 'SGD' when blanked out if COO details are entered.", Core.Constants.CurrencyCodes.Singapore, AddInfoJobDeclaration.SG_RX_NKCertReferenceCurrency);
		}

		public void TestSG_DutyExempt()
		{
			AssertEquals("Duty Exempt", false, AddInfoJobDeclaration.SG_DutyExempt);
			AddInfoJobDeclaration.SG_DutyExempt = true;
			AssertEquals("Duty Exempt", true, AddInfoJobDeclaration.SG_DutyExempt);
		}

		#region Test SG_OutwardVessel

		void SetupTestVessels()
		{
			testVessel1 = Factory.NewWithValidTestData<RefVessel>();
			testVessel1.RV_Name = "HYOGO MARU";
			testVessel1.RV_LloydsNumber = "4567123";

			testVessel2 = Factory.NewWithValidTestData<RefVessel>();
			testVessel2.RV_Name = "HYOGO KENU";
			testVessel2.RV_LloydsNumber = "6712345";

			testVessel3 = Factory.NewWithValidTestData<RefVessel>();
			testVessel3.RV_Name = "VESSEL NO LLOYDS";
			testVessel3.RV_LloydsNumber = "";

			testVessel4 = Factory.NewWithValidTestData<RefVessel>();
			testVessel4.RV_Name = "DUP_VESS";
			testVessel4.RV_LloydsNumber = "4984731";

			// TODO: implement when unique key constraint is removed:

			//TestVessel5 = Factory.NewWithValidTestData<RefVessel>();
			//TestVessel5.RV_Name = "DUP_VESS";
			//TestVessel5.RV_LloydsNumber = "";

			//TestVessel6 = Factory.NewWithValidTestData<RefVessel>();
			//TestVessel6.RV_Name = "DUP_VESS";
			//TestVessel6.RV_LloydsNumber = "5738219";

			Factory.Save();
		}

		RefVessel testVessel1;
		RefVessel testVessel2;
		RefVessel testVessel3;
		RefVessel testVessel4;
		//RefVessel TestVessel5;
		//RefVessel TestVessel6;

		public void TestOutwardVesselLoadsByVesselNameIfNoLloydIMO()
		{
			SetupTestVessels();
			var testDec = Factory.NewWithValidTestData<JobDeclaration>();
			testDec.SG_OutwardVesselName = "VESSEL NO LLOYDS";
			AssertNotNull("RefVessel sans Lloyds number should still have been loaded", testDec.OutwardVessel);
		}

		public void TestRefVessel_GetsInactiveVessels()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "INACTIVE";
			vessel.RV_IsActive = false;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.SG_OutwardVesselName = vessel.RV_Name;
			AssertEquals("Vessel is inactive", false, declaration.OutwardVessel.RV_IsActive);
		}

		public void TestCheckSG_OutwardVesselName_WarningOnDuplicates()
		{
			/*
			 * TODO: This test can only be implemented once the change to the RefVessel table removes the vessel name natural key constraint
			 *	set up multiple test vessels, some with the same name
			 *	test when entering a RefVessel with a vessel name that is duplicated, the SG_OutwardVessel field contains a message error advising of duplicate vessels with this name.
			 *	
			 *	Test (when implemented) will need to validate this warning message is thrown on duplicate vessels:
			 *	Outward Vessel entered has duplicate entries in the Vessel Reference file.\r\nPlease use the <F4> module search functionality to select the appropriate vessel.
			 */
			if (RefVesselSchema.CsvColumnList(RefVesselSchema.Instance).Contains(RefVesselSchema.Constants.RV_Code))
			{
				Assert(true);
			}
			else
			{
				Assert("When changing RV_Code to RV_Name and removing the unique constraint, this test case needs to be implemented", false);
			}
		}

		#endregion

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion

		#region AddInfoJobDeclaration
		AddInfoJobDeclaration AddInfoJobDeclaration => Declaration.AddInfo;

		#endregion

		#region Overrides
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoJobDeclarationLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoJobDeclarationValidation_OUT);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.AddInfo;
		}
		#endregion
	}
}
