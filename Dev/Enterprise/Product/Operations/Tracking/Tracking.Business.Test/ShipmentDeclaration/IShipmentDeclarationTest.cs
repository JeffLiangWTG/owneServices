using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Testing
{
	public abstract class IShipmentDeclarationTest : TestCaseWithFactory
	{
		#region TestSuppressedFields

		void AssertDateAndVoyageSuppression(ZDateTime testDate, ZString voyage, IShipmentDeclaration bizO)
		{
			AssertEquals("ETD", testDate.ToLongTimeString(), bizO.ETDWithSuppression.ToLongTimeString());
			AssertEquals("ETA", testDate.ToLongTimeString(), bizO.ETAWithSuppression.ToLongTimeString());
			AssertEquals("Voyage", voyage, bizO.CurrentVoyageWithSuppression);
			if (bizO is TrackingShipment)
			{
				AssertEquals("Main Voyage", voyage, bizO.MainVoyageWithSuppression);
			}
		}

		public void TestSuppressedFields()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				ZDateTime testDate = ZDateTime.Now.AddDays(1);
				RefVessel testVessel = Factory.New<RefVessel>();
				testVessel.RV_Code = "123";
				ZString testVoyage = "AU123";

				IShipmentDeclaration bizO1 = GetNewBizOForSuppressedFieldsTest(testDate, testVessel.RV_Code, testVoyage, "SEA");
				AssertDateAndVoyageSuppression(testDate, testVoyage, bizO1);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, false);
				IShipmentDeclaration bizO2 = GetNewBizOForSuppressedFieldsTest(testDate, testVessel.RV_Code, testVoyage, "SEA");
				AssertDateAndVoyageSuppression(testDate, testVoyage, bizO2);

				IShipmentDeclaration bizO3 = GetNewBizOForSuppressedFieldsTest(testDate, ZString.Empty, testVoyage, "AIR");
				AssertDateAndVoyageSuppression(testDate, testVoyage, bizO3);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(RegistryItem, true);
				IShipmentDeclaration bizO4 = GetNewBizOForSuppressedFieldsTest(testDate, testVessel.RV_Code, testVoyage, "SEA");
				AssertDateAndVoyageSuppression(testDate, testVoyage, bizO4);

				IShipmentDeclaration bizO5 = GetNewBizOForSuppressedFieldsTest(testDate, ZString.Empty, testVoyage, "AIR");
				AssertDateAndVoyageSuppression(Suppression.SuppressedDate, Suppression.SuppressedString, bizO5);

				ZDateTime testDate2 = ZDateTime.Now.AddDays(-1);
				IShipmentDeclaration bizO6 = GetNewBizOForSuppressedFieldsTest(testDate2, testVessel.RV_Code, testVoyage, "SEA");
				AssertDateAndVoyageSuppression(testDate2, testVoyage, bizO6);

				IShipmentDeclaration bizO7 = GetNewBizOForSuppressedFieldsTest(testDate2, ZString.Empty, testVoyage, "AIR");
				if (bizO7 is TrackingShipment)
				{
					AssertDateAndVoyageSuppression(Suppression.SuppressedDate, Suppression.SuppressedString, bizO7);
				}
				else if (bizO7 is TrackingDeclaration)
				{
					AssertDateAndVoyageSuppression(testDate2, testVoyage, bizO7);
				}
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		#endregion

		#region TestCharges

		public void TestCharges()
		{
			AssertNotNull("TestContact", TestContact);
			Factory.Save(); // Need to save OrgContact before logging in
			AssertNotNull("TestSiteUser", TestSiteUser);
			Assert("WebUser should be logged in", TestSiteUser.IsLoggedIn);

			IShipmentDeclaration shipDec = GetNewBizOForChargesTest();

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipDec.PersistentBizOPK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = TestSiteUser.LoggedInOrganisation.PK;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_JH = job.PK;
			invoice.AH_TransactionType = "INV";
			invoice.AH_OSTotal = 1000;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_ConsolidatedInvoiceRef = shipDec.Number;

			AccTransactionLines chargeLine = Factory.New<AccTransactionLines>();
			chargeLine.AL_AH = invoice.PK;
			chargeLine.AL_JH = job.PK;

			ITransactionSupport transactionSupport = shipDec as ITransactionSupport;
			AssertNotNull(transactionSupport.InvoiceLoader.Transactions);
			AssertEquals("Should be one transactions", 1, transactionSupport.InvoiceLoader.Transactions.Count);
			AssertEquals("Totals as string should be AUD 1000", "AUD 1,000.00", transactionSupport.InvoiceLoader.ChargesTotalsAsString);

			AssertEquals("ChargesAsString should be AUD 1000", "AUD 1,000.00", shipDec.Charges);
		}
		#endregion

		#region Public Abstract Methods

		public abstract void TestPersistentBizOPK();

		public abstract void TestNumber();
		public abstract void TestHouseBill();
		public abstract void TestMasterBill();

		public abstract void TestConsignorProperties();
		public abstract void TestConsigneeProperties();

		public abstract void TestOriginPortCode();
		public abstract void TestDestinationPortCode();
		public abstract void TestCurrentLoadPort();
		public abstract void TestCurrentDischargePort();

		public abstract void TestETA();

		public abstract void TestBookingReference();
		public abstract void TestOwnerReference();

		public abstract void TestPacksWithUnits();
		public abstract void TestVolumeWithUnits();
		public abstract void TestWeightWithUnits();

		public abstract void TestGoodsProperties();

		public abstract void TestDocsAndCartageProperties();

		public abstract void TestServiceLevelCode();

		public abstract void TestDeliveredLegProperties();

		public abstract void TestBookedOnline();

		public abstract void TestTop3Containers();

		public abstract void TestOrderReference();

		public abstract void TestForwarders();

		public abstract void TestLoadingMeters();

		#endregion

		#region Protected Abstract Methods

		protected abstract IShipmentDeclaration GetNewBizOForChargesTest();
		protected abstract IShipmentDeclaration GetNewBizOForSuppressedFieldsTest(ZDateTime date, ZString vessel, ZString voyageFlight, ZString transportMode);
		protected abstract CodeDescriptionBoolRegistryItem RegistryItem { get; }

		#endregion

		#region Implementation

		protected TrackingSiteUser TestSiteUser
		{
			get
			{
				if (fTestSiteUser == null)
				{
					fTestSiteUser = new TrackingSiteUser();
					fTestSiteUser.Login(TestOrg.OH_Code, TestContact.OC_Email, TestContact.PasswordForTesting);
				}
				return fTestSiteUser;
			}
		}
		TrackingSiteUser fTestSiteUser;

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.NoData);
					fTestOrg.OH_FullName = "FullName";
					fTestOrg.OH_Code = "FULLNASYD";
					fTestOrg.OH_RL_NKClosestPort = "AUSYD";
					fTestOrg.Addresses[0].OA_Address1 = "100 Main Street";
					fTestOrg.Addresses[0].OA_City = "City Town";
					fTestOrg.Addresses[0].OA_State = "State";
					fTestOrg.Addresses[0].OA_PostCode = "0000";
					OrgContact contact = fTestOrg.Contacts.AddNew();
					contact.OC_ContactName = "ContactName";
					//fTestOrg.OH_Web = "Web";
					fTestOrg.PrimaryRegistrationNumber.Number = "LocalBusinessNumber";
					if (fTestOrg.CompanyDataCollection.Count == 0)
					{
						fTestOrg.CompanyDataCollection.AddNew();
					}
					fTestOrg.CompanyDataCollection[0].OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		protected OrgContact TestContact
		{
			get
			{
				if (fTestContact == null)
				{
					fTestContact = Factory.NewWithValidTestData<OrgContact>(TestBusinessObjectKind.NoData);
					fTestContact.OC_OH = TestOrg.PK;
					fTestContact.OC_ContactName = "TestContact";
					fTestContact.OC_WebAccessEnabled = true;
					fTestContact.OC_Email = "test@cargowise.com";
					fTestContact.SetHashedPassword("test");
				}
				return fTestContact;
			}
		}
		OrgContact fTestContact;

		#endregion
	}
}
