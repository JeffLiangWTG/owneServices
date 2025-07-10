using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(SecurityJobConsolAWBSpecialHandling))]
	sealed class SecurityJobConsolAWBSpecialHandlingTest : JobConsolAWBSpecialHandlingTest
	{
		public void TestSecurityModifiedEventLogged_SPXChosen()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				Factory.Save();

				var secEvent = consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC");

				AssertNotNull("SEC event has been created on consolidation", secEvent);
				AssertEquals(string.Empty, secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Old]);
				AssertEquals("SPX", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.New]);
				AssertEquals("AWB Special Handling - SPX verified", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
			}
		}

		public void TestSecurityModifiedEventLogged_SPXChosenFromAnotherCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SCO";

				Factory.Save();

				AssertNull(consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC"));

				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				Factory.Save();

				var secEvent = consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC");

				AssertNotNull("SEC event has been created on consolidation", secEvent);
				AssertEquals("SCO", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Old]);
				AssertEquals("SPX", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.New]);
				AssertEquals("AWB Special Handling - SPX verified", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
			}
		}

		public void TestSecurityModifiedEvent_NotLoggedWhenNotVerified()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightNotVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("When not verified, SPX will not be set, and the consol Special Handling object will be deleted", string.Empty, consol.SecurityStatusCode);

				securityStatusCodeSpecialHandling.Delete();
				Factory.Save();

				AssertNull(consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC"));
			}
		}

		public void TestSecurityModifiedEvent_NotRequiredWhenNotSCSSupportedCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Constants.CountryCodes.India;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				Factory.Save();

				AssertEquals("When not Supply Chain Security supported Company, user is not asked to verify", false, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
				AssertNull(consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC"));
			}
		}

		public void TestSecurityModifiedEvent_RequiredWhenSCSSupportedCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Constants.CountryCodes.Canada;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("When Supply Chain Security supported Company, user is asked to verify", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);

				Factory.Save();

				var secEvent = consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC");

				AssertNotNull("SEC event has been created on consolidation", secEvent);
				AssertEquals(string.Empty, secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Old]);
				AssertEquals("SPX", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.New]);
				AssertEquals("AWB Special Handling - SPX verified", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
			}
		}

		public void TestSecurityModifiedEvent_RequiredWhenUKCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("When Supply Chain Security supported Company, user is asked to verify", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
			}
		}

		public void TestSecurityModifiedEvent_NotRequiredWhenNotUKConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.Australia;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("Not UK Consol, not required to verify", false, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
			}
		}

		public void TestSecurityModifiedEvent_RequiredWhenUKConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("UK Consol, required to verify", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
			}
		}

		public void TestSecurityModifiedEvent_WithNewMessage_RequiredWhenMAWBOverriden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("MAWB is overridden, still required to verify", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
				AssertEquals("MAWB is overridden, new message displayed", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).MAWBMessageDisplayed);
			}
		}

		public void TestSecurityModifiedEventLogged_WithNewMessageAndLogged_WhenMAWBOverridenWithNonSPXValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "NSR";

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				Factory.Save();

				var secEvent = consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC");

				AssertNotNull("SEC event has been created on consolidation", secEvent);
				AssertEquals(ZString.Empty, secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Old]);
				AssertEquals("SPX", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.New]);
				AssertEquals("AWB Special Handling - SPX verified", secEvent.Parameters[EventConstants.EventReferenceParameters.Codes.Reason]);
			}
		}

		public void TestSecurityModifiedEventLogged_OnlyOnce_WhenBothMAWBAndConsolHaveSPXValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				Factory.Save();

				var secEvents = consol.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == "SEC");

				AssertNotNull("SEC event has been created on consolidation", secEvents);
				AssertEquals(1, secEvents.Count());
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			var result = factory.New<SecurityJobConsolAWBSpecialHandling>();
			result.JKH_Code = "SCH";
			result.JKH_JK_Consol = consol.PK;

			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			var result = Factory.New<SecurityJobConsolAWBSpecialHandling>();
			result.JKH_Code = "SCH";
			result.JKH_JK_Consol = consol.PK;

			return result;
		}

		#endregion
	}
}
