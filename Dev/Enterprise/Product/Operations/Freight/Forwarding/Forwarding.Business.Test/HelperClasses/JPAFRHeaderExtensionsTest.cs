using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class JPAFRHeaderExtensionsTest : TestCaseWithFactory
	{
		public void TestGetAFRHeader()
		{
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "S#@";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "G#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var consol = Factory.New<ForwardingConsol>();
			AssertNull("No AFR", consol.GetAFRHeader());

			var afr1 = Factory.New<Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader>();
			afr1.JPH_IsShippingLineEntry = ZBool.False;
			afr1.JPH_ParentId = consol.PK;
			afr1.JPH_ParentTableCode = consol.TablePrefix;
			afr1.JPH_SystemCreateTimeUtc = new ZDateTime(2012, 3, 1);
			afr1.JPH_JobReference = "A001";
			var afr2 = Factory.New<Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader>();
			afr2.JPH_IsShippingLineEntry = ZBool.True;
			afr2.JPH_ParentId = consol.PK;
			afr2.JPH_ParentTableCode = consol.TablePrefix;
			afr2.JPH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			afr2.JPH_JobReference = "A002";
			var afr3 = Factory.New<Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader>();
			afr3.JPH_IsShippingLineEntry = ZBool.False;
			afr3.JPH_ParentId = consol.PK;
			afr3.JPH_ParentTableCode = consol.TablePrefix;
			afr3.JPH_SystemCreateTimeUtc = new ZDateTime(2012, 2, 1);
			afr3.JPH_JobReference = "A003";
			var afr4 = Factory.New<Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader>();
			afr4.JPH_IsShippingLineEntry = ZBool.False;
			afr4.JPH_ParentId = consol.PK;
			afr4.JPH_ParentTableCode = "S!";
			afr4.JPH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			afr4.JPH_JobReference = "A004";
			var afr5 = Factory.New<Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader>();
			afr5.JPH_IsShippingLineEntry = ZBool.False;
			afr5.JPH_ParentId = ZGuid.NewZGuid();
			afr5.JPH_ParentTableCode = consol.TablePrefix;
			afr5.JPH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			afr5.JPH_JobReference = "A005";

			AssertEquals("Should Match afr3", afr3, consol.GetAFRHeader());
		}

		public void TestIsEligableForJPAFR()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "JPTKI";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKDischargePort = "";
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKFirstForeignPort = "JPTKI";
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKFirstForeignPort = "";
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKLastForeignPort = "JPTKI";
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKLastForeignPort = "";
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKPortOfFirstArrival = "JPTKI";
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.JK_RL_NKPortOfFirstArrival = "";
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "JPTKI";
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "";
			AssertEquals("is disabled", false, consol.IsEligibleForJPAFR());
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "JPTZU";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
			AssertEquals("is enabled", false, consol.IsEligibleForJPAFR());
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			var testLeg = consol.Transports.AddNew();
			testLeg.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testLeg.JW_RL_NKLoadPort = "JPTKI";
			testLeg.JW_RL_NKDiscPort = "JPTKY";
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("is enabled", false, consol.IsEligibleForJPAFR());
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("is enabled", true, consol.IsEligibleForJPAFR());
			consol.Transports.MostInterestingTransport.Delete();
			AssertEquals("is enabled", false, consol.IsEligibleForJPAFR());
		}

		public void TestFactoryCaching()
		{
			var consol = Factory.New<ForwardingConsol>();
			var originalDBHitCount = Factory.DatabaseLoadCount;

			consol.GetAFRHeader(false);
			AssertEquals("First time must hit DB", 1, Factory.DatabaseLoadCount - originalDBHitCount);

			consol.GetAFRHeader(false);
			AssertEquals("Should not hit DB", 1, Factory.DatabaseLoadCount - originalDBHitCount);

			consol.GetAFRHeader();
			AssertEquals("Should reload from DB", 2, Factory.DatabaseLoadCount - originalDBHitCount);
		}
	}
}
