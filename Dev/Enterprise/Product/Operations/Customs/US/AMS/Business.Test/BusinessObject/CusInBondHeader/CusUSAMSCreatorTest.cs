using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusUSAMSCreatorTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "08111111111";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolCopy = newFactory.Load<ForwardingConsol>(consol.PK);
			var firstCreator = new CusUSAMSCreator(consol);
			var secondCreator = new CusUSAMSCreator(consolCopy);
			var notifications1 = new NotificationBuffer();
			var notifications2 = new NotificationBuffer();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (firstCreator.TryCreateUSAMSData(notifications1))
				{
					AssertEquals(string.Empty, notifications1.AsString);
					var ams1 = (CusInBondHeader)consol.USAMS;
					AssertNotNull(ams1);
					AssertEquals(consol.PK, ams1.BH_ParentID);

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
					using (secondCreator.TryCreateUSAMSData(notifications2))
					{
						AssertContains(string.Format("Someone else is already in the process of creating an AMS for this Consol {0}.\r\nYou should be able to access the AMS when the person has saved the record. Please try later.", consol.HumanReadableName), notifications2.AsString);
					}
				}
			}
		}

		public void TestCheckSupported()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var creator = new CusUSAMSCreator(consol);
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("The automatic creation of US AMS is not available for this transport mode by AIR.", creator.CheckSupported);
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.JK_RL_NKDischargePort = "";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.JK_RL_NKFirstForeignPort = "USLAX";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.JK_RL_NKFirstForeignPort = "";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.JK_RL_NKLastForeignPort = "USLAX";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.JK_RL_NKLastForeignPort = "";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.JK_RL_NKPortOfFirstArrival = "USLAX";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.JK_RL_NKPortOfFirstArrival = "";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "USLAX";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "USCHI";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", creator.CheckSupported);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "PRADJ";
			AssertEquals(string.Empty, creator.CheckSupported);
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("The automatic creation of US AMS is not available for this transport mode by ROA.", creator.CheckSupported);
			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(string.Empty, creator.CheckSupported);
		}

		public void TestMutexLockIsEnforced()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "08111111111";
				consol.JK_RL_NKDischargePort = "USLAX";
				Factory.Save();

				var notifications = new NotificationBuffer();
				var creator = new CusUSAMSCreator(consol);

				using (var mutex1 = CusInBondHeader.CreateMutex(consol.PK))
				{
					mutex1.Lock();

					using (creator.TryCreateUSAMSData(notifications))
					{
						AssertContains(string.Format("Someone else is already in the process of creating an AMS for this Consol {0}.\r\nYou should be able to access the AMS when the person has saved the record. Please try later.", consol.HumanReadableName), notifications.AsString);
					}

					AssertNull("Cannot create an US AMS when the Mutex is already locked", consol.USAMS);
				}

				notifications.Clear();
				using (creator.TryCreateUSAMSData(notifications))
				{
					AssertNotNull("created an US AMS", consol.USAMS);
					AssertNullOrEmptyOrWhitespace("No Errors", notifications.AsString);

					using (var mutex2 = CusInBondHeader.CreateMutex(consol.PK))
					{
						Assert("Mutex is locked inside the using", !mutex2.Lock());
					}
				}

				using (var mutex3 = CusInBondHeader.CreateMutex(consol.PK))
				{
					Assert("Mutex is unlocked after dispose", mutex3.Lock());
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, consol.PK);
				var ams = newFactory.LoadTop1(typeof(CusInBondHeader), query);

				AssertEquals("Created US AMS can be retrieved from another factory", consol.USAMS.PK, ams?.PK);
			}
		}

		public void TestExistingUSAMS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "08111111111";
				consol.JK_RL_NKDischargePort = "USLAX";
				Factory.Save();

				var creator = new CusUSAMSCreator(consol);
				using (creator.TryCreateUSAMSData(null))
				{
					AssertNotNull("created an US AMS", consol.USAMS);
					AssertSame("US AMS is in Consol Factory", consol.Factory, consol.USAMS.Factory);
					Assert(consol.USAMS.IsInDatabase);
				}

				var newFactory = new BusinessObjectFactory();
				var consolIOF = newFactory.Load<ForwardingConsol>(consol.PK);
				var creator2 = new CusUSAMSCreator(consolIOF);
				using (creator2.TryCreateUSAMSData(null))
				{
					AssertEquals("Creator finds existing US AMS", consol.USAMS.PK, consolIOF.USAMS.PK);
				}

				var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, consol.PK);
				query.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, JobConsolSchema.Constants.Prefix);
				var ams = newFactory.LoadTop1(typeof(CusInBondHeader), query);
				AssertNotNull("Created US AMS can be retrieved from another factory", ams);
			}
		}

		public void TestConsolNotInDatabase()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_MasterBillNum = "08111111111";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.Shipments.AddNew().JS_HouseBill = "HB001";
				consol.Shipments.AddNew().JS_HouseBill = "HB002";

				var creator = new CusUSAMSCreator(consol);
				using (creator.TryCreateUSAMSData(null))
				{
					AssertNotNull("created an US AMS", consol.USAMS);

					AssertSame("US AMS is in Consol Factory", consol.Factory, consol.USAMS.Factory);
					Assert("US AMS is not saved", !consol.USAMS.IsInDatabase);
					Assert("Consol is not saved", !consol.IsInDatabase);
				}
			}
		}
	}
}
