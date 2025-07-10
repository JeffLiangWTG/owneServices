using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	sealed class USAMSImportAirManifestCreatorTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08111111111";
			consol.JK_RL_NKLoadPort = "USLAX";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolCopy = newFactory.Load<ForwardingConsol>(consol.PK);
			var firstCreator = new USAMSImportAirManifestCreator(consol);
			var secondCreator = new USAMSImportAirManifestCreator(consolCopy);
			var notifications1 = new NotificationBuffer();
			var notifications2 = new NotificationBuffer();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (firstCreator.TryCreateUSAMSData(notifications1))
				{
					AssertEquals(string.Empty, notifications1.AsString);
					var ams1 = firstCreator.GetExistingAMS(newFactory);
					AssertNotNull(ams1);
					AssertEquals(consol.PK, ams1.AMA_ParentId);

					using (secondCreator.TryCreateUSAMSData(notifications2))
					{
						AssertContains(string.Format("Someone else is already in the process of creating a Customs Manifest for this Consol {0}.\r\nYou should be able to access the Customs Manifest when the person has saved the record. Please try later.", consol.HumanReadableName), notifications2.AsString);
					}
				}
			}
		}

		public void TestCheckSupported()
		{
			var consol = Factory.New<ForwardingConsol>();
			var creator = new USAMSImportAirManifestCreator(consol);
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "VUVLI";
			consol.JK_TransportMode = "AIR";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.JK_TransportMode = "SEA";
			AssertEquals("The automatic creation of US AMS is not available for this transport mode by SEA.", creator.CheckSupported);
			consol.JK_TransportMode = "ROA";
			AssertEquals("The automatic creation of US AMS is not available for this transport mode by ROA.", creator.CheckSupported);
			consol.JK_RL_NKLoadPort = "";
			consol.JK_TransportMode = "AIR";
			AssertEquals("This Consol does not require a manifest.", creator.CheckSupported);
		}

		public void TestMutexLockIsEnforced()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "08111111111";
				consol.JK_RL_NKLoadPort = "USLAX";
				Factory.Save();

				BusinessObject outOB = null;
				var notifications = new NotificationBuffer();
				var creator = new USAMSImportAirManifestCreator(consol);

				using (var mutex1 = AsycudaManifestHeader.CreateMutex(consol.PK))
				{
					mutex1.Lock();

					using (creator.TryCreateUSAMSData(notifications))
					{
						AssertContains(string.Format("Someone else is already in the process of creating a Customs Manifest for this Consol {0}.\r\nYou should be able to access the Customs Manifest when the person has saved the record. Please try later.", consol.HumanReadableName), notifications.AsString);
					}
					outOB = creator.GetExistingAMS(Factory);

					AssertNull("Cannot create an US AMS Import Air Manifest when the Mutex is already locked", outOB);
				}

				notifications.Clear();
				using (creator.TryCreateUSAMSData(notifications))
				{
					outOB = creator.GetExistingAMS(Factory);
					AssertNotNull("created an US AMS Import Air Manifest", outOB);
					AssertNullOrEmptyOrWhitespace("No Errors", notifications.AsString);

					using (var mutex2 = AsycudaManifestHeader.CreateMutex(consol.PK))
					{
						Assert("Mutex is locked inside the using", !mutex2.Lock());
					}
				}

				using (var mutex3 = AsycudaManifestHeader.CreateMutex(consol.PK))
				{
					Assert("Mutex is unlocked after dispose", mutex3.Lock());
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
				var ams = newFactory.LoadTop1(typeof(AsycudaManifestHeader), query);

				AssertEquals("Created US AMS Import Air Manifest can be retrieved from another factory", outOB.PK, ams?.PK);
			}
		}

		public void TestExistingUSAMS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "08111111111";
				consol.JK_RL_NKLoadPort = "USLAX";
				Factory.Save();

				BusinessObject outOB = null;
				var creator = new USAMSImportAirManifestCreator(consol);
				using (creator.TryCreateUSAMSData(null))
				{
					outOB = creator.GetExistingAMS(Factory);
					AssertNotNull("created an US AMS Import Air Manifest", outOB);
					AssertSame("US AMS Import Air Manifest is in Consol Factory", consol.Factory, outOB.Factory);
					Assert(outOB.IsInDatabase);
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var creator2 = new USAMSImportAirManifestCreator(consolIOF);
				using (creator2.TryCreateUSAMSData(null))
				{
					var outOB2 = creator2.GetExistingAMS(Factory);
					AssertEquals("Creator finds existing US AMS Import Air Manifest", outOB.PK, outOB2.PK);
				}

				var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, JobConsolSchema.Constants.Prefix);
				var ams = newFactory.LoadTop1(typeof(AsycudaManifestHeader), query);
				AssertNotNull("Created US AMS Import Air Manifest can be retrieved from another factory", ams);
			}
		}

		public void TestConsolNotInDatabase()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "08111111111";
				consol.JK_RL_NKLoadPort = "USLAX";
				consol.Shipments.AddNew().JS_HouseBill = "HB001";
				consol.Shipments.AddNew().JS_HouseBill = "HB002";

				var creator = new USAMSImportAirManifestCreator(consol);
				using (creator.TryCreateUSAMSData(null))
				{
					var manifests = consol.GetGlobalManifestHeaders();
					AssertEquals(manifests.Length, 1);
					var outOB = (AsycudaManifestHeader)manifests.FirstOrDefault();
					AssertNotNull("created an US AMS Import Air Manifest", outOB);

					AssertSame("US AMS Import Air Manifest is in Consol Factory", consol.Factory, outOB.Factory);
					Assert("US AMS Import Air Manifest is not saved", !outOB.IsInDatabase);
					Assert("Consol is not saved", !consol.IsInDatabase);
				}
			}
		}
	}
}
