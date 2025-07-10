using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSCAOceanBillSafeCreatorTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "08111111111";
			consol.Shipments.AddNew().JS_HouseBill = "HB001";
			consol.Shipments.AddNew().JS_HouseBill = "HB002";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolCopy = newFactory.Load<ForwardingConsol>(consol.PK);
			var firstCreator = new CusSCAOceanBillSafeCreator(consol);
			var secondCreator = new CusSCAOceanBillSafeCreator(consolCopy);
			var notifications1 = new NotificationBuffer();
			var notifications2 = new NotificationBuffer();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (firstCreator.TryCreateOrUpdateCargoAndChildBills(notifications1, out var outOC1))
				{
					AssertEquals(string.Empty, notifications1.AsString);
					var oceanBill1 = (BaseCusSCAOceanBill)outOC1;
					AssertNotNull(oceanBill1);
					AssertEquals(consol, oceanBill1.Consol);
					AssertEquals("08111111111", oceanBill1.CB_OceanBill);
					var childBills = Factory.Load<BaseCusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBill1.PK));
					AssertEquals(2, childBills.Length);
					AssertEquals("HB001", childBills[0].CA_HouseBill);
					AssertEquals(consol.Shipments[0], childBills[0].Shipment);
					AssertEquals("HB002", childBills[1].CA_HouseBill);
					AssertEquals(consol.Shipments[1], childBills[1].Shipment);

					using (secondCreator.TryCreateOrUpdateCargoAndChildBills(notifications2, out var outOC2))
					{
						AssertContains(string.Format("A Customs master bill record cannot be created or updated as someone else is trying to create or update a master bill for this consol {0}.", consol.HumanReadableName), notifications2.AsString);
					}
				}
			}
		}

		public void TestMutexLockIsEnforced()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "08111111111";
				consol.Shipments.AddNew().JS_HouseBill = "HB001";
				consol.Shipments.AddNew().JS_HouseBill = "HB002";
				Factory.Save();

				BusinessObject outOB = null;
				var notifications = new NotificationBuffer();
				var creator = new CusSCAOceanBillSafeCreator(consol);

				using (var mutex1 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					mutex1.Lock();

					using (creator.TryCreateOrUpdateCargoAndChildBills(notifications, out outOB))
					{
						AssertContains(string.Format("A Customs master bill record cannot be created or updated as someone else is trying to create or update a master bill for this consol {0}.", consol.HumanReadableName), notifications.AsString);
					}

					AssertNull("Cannot create an Ocean Bill when the Mutex is already locked", outOB);
				}

				notifications.Clear();
				using (creator.TryCreateOrUpdateCargoAndChildBills(notifications, out outOB))
				{
					AssertNotNull("created an Ocean Bill", outOB);
					AssertNullOrEmptyOrWhitespace("No Errors", notifications.AsString);

					using (var mutex2 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
					{
						Assert("Mutex is locked inside the using", !mutex2.Lock());
					}
				}

				using (var mutex3 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					Assert("Mutex is unlocked after dispose", mutex3.Lock());
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var oceanBill = new BaseCusSCAOceanBill.Loader(newFactory).LoadFromConsolAndApplicationCode(consolIOF, new ZString[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages });

				AssertEquals("Created ocean bill can be retrieved from another factory", outOB.PK, oceanBill?.PK);
			}
		}

		public void TestExistingOceanBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "08111111111";
				consol.Shipments.AddNew().JS_HouseBill = "HB001";
				consol.Shipments.AddNew().JS_HouseBill = "HB002";
				Factory.Save();

				BusinessObject outOB = null;
				var creator = new CusSCAOceanBillSafeCreator(consol);
				using (creator.TryCreateOrUpdateCargoAndChildBills(null, out outOB))
				{
					AssertNotNull("created an Ocean Bill", outOB);
					AssertSame("Ocean Bill is in Consol Factory", consol.Factory, outOB.Factory);
					Assert(outOB.IsInDatabase);
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var creator2 = new CusSCAOceanBillSafeCreator(consolIOF);
				using (creator2.TryCreateOrUpdateCargoAndChildBills(null, out var outOB2))
				{
					AssertEquals("Creator finds existing ocean bill", outOB.PK, outOB2.PK);
				}

				var query = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
				var consolOceanBills = newFactory.Load<BaseCusSCAOceanBill>(query);
				AssertEquals("There is just one ocean bill", 1, consolOceanBills.Length);
			}
		}

		public void TestConsolNotInDatabase()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "08111111111";
				consol.Shipments.AddNew().JS_HouseBill = "HB001";
				consol.Shipments.AddNew().JS_HouseBill = "HB002";

				var creator = new CusSCAOceanBillSafeCreator(consol);
				using (creator.TryCreateOrUpdateCargoAndChildBills(null, out var outOB))
				{
					AssertNotNull("created an Ocean Bill", outOB);

					AssertSame("Ocean Bill is in Consol Factory", consol.Factory, outOB.Factory);
					Assert("Ocean Bill is not saved", !outOB.IsInDatabase);
					Assert("Consol is not saved", !consol.IsInDatabase);
				}
			}
		}
	}
}
