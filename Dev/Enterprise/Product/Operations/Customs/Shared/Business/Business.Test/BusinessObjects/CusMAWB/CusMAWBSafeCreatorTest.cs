using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusMAWBSafeCreatorTest : TestCaseWithFactory
	{
		[TestDate(2000, 1, 1)]
		public void TestEndToEnd()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "08111111111";
				consol.Shipments.AddNew().JS_HouseBill = "HB001";
				consol.Shipments.AddNew().JS_HouseBill = "HB002";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consolCopy = newFactory.Load<ForwardingConsol>(consol.PK);

				var firstCusMAWBSafeCreator = new CusMAWBSafeCreator(consol);
				var secondCusMAWBSafeCreator = new CusMAWBSafeCreator(consolCopy);

				using (firstCusMAWBSafeCreator.TryCreateOrUpdateCargoAndChildBills(null, out var outMawb))
				{
					var mawb = consol.AUCusMAWB as CusMAWB;
					AssertNotNull(mawb);
					AssertEquals(consol, mawb.Consol);
					AssertEquals("08111111111", mawb.CM_MAWB);
					AssertEquals(2, mawb.ChildBills.Count);
					AssertEquals("HB001", mawb.ChildBills[0].CS_HAWB);
					AssertEquals(consol.Shipments[0], mawb.ChildBills[0].Shipment);
					AssertEquals("HB002", mawb.ChildBills[1].CS_HAWB);
					AssertEquals(consol.Shipments[1], mawb.ChildBills[1].Shipment);

					BusinessObject outMawb1 = null;
					using (secondCusMAWBSafeCreator.TryCreateOrUpdateCargoAndChildBills(null, out outMawb1))
					{
						AssertNull(outMawb1);
					}
				}
			}
		}

		public void TestNotToCreateTwoCusHAWBForDetachedShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_JS = shipment.PK;
			hawb.CS_ApplicationCode = "CMR";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08111111111";
			consol.Shipments.Add(shipment);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consolCopy = Factory.Load<ForwardingConsol>(consol.PK);
				var cusMAWBSafeCreator = new CusMAWBSafeCreator(consolCopy);
				BusinessObject outMawb = null;

				using (cusMAWBSafeCreator.TryCreateOrUpdateCargoAndChildBills(null, out outMawb))
				{
					var hawbCollection = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK));
					AssertEquals(1, hawbCollection.Length);
					AssertEquals(hawb, hawbCollection[0]);
				}
			}
		}

		public void TestShipmentWithAirCargoAttachToNewConsol()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.JK_MasterBillNum = "08111111111";
			var shipment = consol1.Shipments.AddNew();
			shipment.JS_HouseBill = "HB001";
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_MasterBillNum = "08111111112";
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol1.PK;
			mawb.CM_ApplicationCode = "CMR";

			var hawb = Factory.New<CusHAWB>();
			hawb.CS_CM = mawb.PK;
			hawb.CS_JS = shipment.PK;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				consol2.Shipments.Add(shipment);
				var secondCusMAWBSafeCreator = new CusMAWBSafeCreator(consol2);
				using (secondCusMAWBSafeCreator.TryCreateOrUpdateCargoAndChildBills(null, out var outMawb))
				{
					AssertNotNull(outMawb);
					var hawbCollection = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK));
					AssertEquals(1, hawbCollection.Length);
					AssertEquals(hawb, hawbCollection[0]);
				}
			}
		}

		public void TestMutexLockIsEnforced()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "08111111111";
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB001";
				Factory.Save();

				BusinessObject outMawb = null;
				var notifications = new NotificationBuffer();
				var creator = new CusMAWBSafeCreator(consol);

				using (var mutex1 = CusMAWB.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					mutex1.Lock();

					using (creator.TryCreateOrUpdateCargoAndChildBills(notifications, out outMawb))
					{
						AssertContains(string.Format("A Customs master bill record cannot be created as someone else is trying to create a master bill for this consol {0}.", consol.HumanReadableName), notifications.AsString);
					}

					AssertNull("Cannot create a MAWB when the Mutex is already locked", outMawb);
				}

				notifications.Clear();
				using (creator.TryCreateOrUpdateCargoAndChildBills(notifications, out outMawb))
				{
					AssertNotNull("created a MAWB", outMawb);
					AssertNullOrEmptyOrWhitespace("No Errors", notifications.AsString);

					using (var mutex2 = CusMAWB.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
					{
						Assert("Mutex is locked inside the using", !mutex2.Lock());
					}
				}

				using (var mutex3 = CusMAWB.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					Assert("Mutex is unlocked after dispose", mutex3.Lock());
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var mawb = (CusMAWB)ForwardingConsol.GetFirstMatchingMAWB<Integration.Customs.Shared.ICusMAWB>(newFactory, consolIOF.PK, false, false,
					CusMAWBTypeDecider.GetApplicationCodesForForwarding().Select(x => new ZString(x)).ToArray());

				AssertEquals("Created MAWB can be retrieved from another factory", outMawb.PK, mawb?.PK);
			}
		}

		public void TestMultipleShipmentsOnConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "08111111111";
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_HouseBill = "HB001";
				Factory.Save();

				BusinessObject outMawb = null;
				var notifications = new NotificationBuffer();
				var creator = new CusMAWBSafeCreator(consol);
				using (creator.TryCreateOrUpdateCargoAndChildBills(notifications, out outMawb))
				{
					AssertNotNull(notifications.AsString, outMawb);
					AssertSame("MAWB is in Consol Factory", consol.Factory, outMawb.Factory);
					Assert("MAWB is saved", outMawb.IsInDatabase);
					AssertSame("MAWB is on Consol", consol.AUCusMAWB, outMawb);

					var childBills = (outMawb as CusMAWB).ChildBills.Cast<CusHAWB>().ToArray();
					AssertEquals("shipment1 has a HAWB", shipment1.PK, childBills.FirstOrDefault(h => h.CS_HAWB == "HB001").CS_JS);
					AssertEquals(1, childBills.Length);
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var shipment2 = consolIOF.Shipments.AddNew();
				shipment2.JS_HouseBill = "HB002";
				newFactory.Save();

				var creator2 = new CusMAWBSafeCreator(consolIOF);
				using (creator2.TryCreateOrUpdateCargoAndChildBills(notifications, out var outMawb2))
				{
					AssertEquals("Creator finds existing MAWB", outMawb.PK, outMawb2.PK);

					var childBills = (outMawb2 as CusMAWB).ChildBills.Cast<CusHAWB>().ToArray();
					AssertEquals("shipment1 has a HAWB", shipment1.PK, childBills.FirstOrDefault(h => h.CS_HAWB == "HB001").CS_JS);
					AssertEquals("shipment2 has a HAWB", shipment2.PK, childBills.FirstOrDefault(h => h.CS_HAWB == "HB002").CS_JS);
					AssertEquals(2, childBills.Length);
				}

				var query = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
				var mawbs = newFactory.Load<CusMAWB>(query);
				AssertEquals("There is just one MAWB", 1, mawbs.Length);
			}
		}

		public void TestConsolNotInDatabase()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "08111111111";
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB001";

				BusinessObject outMawb = null;
				var creator = new CusMAWBSafeCreator(consol);
				using (creator.TryCreateOrUpdateCargoAndChildBills(null, out outMawb))
				{
					AssertNotNull("created a MAWB", outMawb);

					AssertSame("MAWB is in Consol Factory", consol.Factory, outMawb.Factory);
					Assert("MAWB not saved", !outMawb.IsInDatabase);
					Assert("Consol not saved", !consol.IsInDatabase);
					AssertEquals(1, (outMawb as CusMAWB).ChildBills.Count);
				}

				var creator2 = new CusMAWBSafeCreator(consol);
				using (creator2.TryCreateOrUpdateCargoAndChildBills(null, out var outMawb2))
				{
					AssertEquals("Creator finds existing MAWB", outMawb.PK, outMawb2.PK);
					AssertEquals("Does not create additional Bills", 1, (outMawb as CusMAWB).ChildBills.Count);
				}
			}
		}

		public void TestMAWBInSaveCollision()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_UniqueConsignRef = "CON4TST";
				consol.JK_MasterBillNum = "08111111111";
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB001";
				Factory.Save();

				var notifications = new NotificationBuffer();
				var creator = new CusMAWBSafeCreatorForTest(consol, (BusinessObjectFactory f) =>
				{
					var f2 = new BusinessObjectFactory();
					f2.RefreshEnabled = false;

					var mawb = (CusMAWB)f2.New(CusMAWBTypeDecider.GetTypeForForwarding());
					mawb.CM_MAWB = "08111111112";
					mawb.CM_JK = consol.PK;
					f2.Save();
				});

				using (creator.TryCreateOrUpdateCargoAndChildBills(notifications, out var outMawb))
				{
					AssertNotNull("created a MAWB", outMawb);
					AssertSame("MAWB is in Consol Factory", consol.Factory, outMawb.Factory);
					Assert("MAWB is saved", outMawb.IsInDatabase);
					AssertContains("An error occurred when creating a new Customs master bill for this consol Consol CON4TST (Master Bill='08111111111').\r\nA Master Bill for this Consol has already been created", notifications.AsString);
				}

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
				var mawbs = newFactory.Load<CusMAWB>(query);
				AssertEquals("There is just one MAWB", 1, mawbs.Length);
			}
		}

		class CusMAWBSafeCreatorForTest : CusMAWBSafeCreator
		{
			public CusMAWBSafeCreatorForTest(ForwardingConsol consol, Action<BusinessObjectFactory> onSavingFunc = null) : base(consol)
			{
				this.onSavingFunc = onSavingFunc;
			}

			Action<BusinessObjectFactory> onSavingFunc { get; set; }

			protected override void OnTemporaryFactorySaving(BusinessObjectFactory f)
			{
				onSavingFunc?.Invoke(f);
				base.OnTemporaryFactorySaving(f);
			}
		}
	}
}
