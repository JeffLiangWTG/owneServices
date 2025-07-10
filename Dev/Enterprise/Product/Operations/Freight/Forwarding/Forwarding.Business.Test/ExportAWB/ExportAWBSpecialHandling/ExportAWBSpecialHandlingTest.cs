using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBSpecialHandling))]
	sealed class ExportAWBSpecialHandlingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSpecialHandlingDescription()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.EP_SpecialHandling = Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CoolGoods;
			AssertEquals("SpecialHandlingDescription should show description for code", Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CoolGoods, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = ZString.Empty;
			AssertEquals("SpecialHandlingDescription should be empty when code is empty", ZString.Empty, specialHandlingItem.SpecialHandlingDescription);

			specialHandlingItem.EP_SpecialHandling = "IVD";
			AssertEquals("SpecialHandlingDescription should be empty when code is invalid", ZString.Empty, specialHandlingItem.SpecialHandlingDescription);
		}

		public void TestIsSavedByFactory()
		{
			var specialHandlingItem = Factory.New<ExportAWBSpecialHandling>();
			AssertEquals("Object w/o header behaves normally", true, specialHandlingItem.IsSavedByFactory);
			specialHandlingItem.Delete();

			var aWBHeader = Factory.New<ConsolExportAWBHeader>();
			specialHandlingItem = aWBHeader.AWBSpecialHandlingItems.AddNew();
			aWBHeader.EH_ParentID = Factory.New<ForwardingConsol>().PK;
			aWBHeader.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Not overridden new AWB is never saved", false, specialHandlingItem.IsSavedByFactory);
			Factory.Save();
			AssertEquals(false, specialHandlingItem.IsInDatabase);

			aWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			aWBHeader.Consol.JK_OverrideWaybillDefaults = false;
			aWBHeader.EH_AreRateLinesOverridden = false;
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed and repopulated", false, specialHandlingItem.IsSavedByFactory);

			specialHandlingItem = aWBHeader.AWBSpecialHandlingItems.AddNew();
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, specialHandlingItem.IsSavedByFactory);

			aWBHeader.ForceSavingByFactory = true;
			AssertEquals("Forced AWB is always saved", true, specialHandlingItem.IsSavedByFactory);
			Factory.Save();
			AssertEquals(true, specialHandlingItem.IsInDatabase);

			aWBHeader.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, specialHandlingItem.IsSavedByFactory);

			aWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AssertEquals("Saved when 'Override' is ticked", true, specialHandlingItem.IsSavedByFactory);
			Factory.Save();

			AssertEquals("Overridden is not saved if doesn't have changes", false, specialHandlingItem.IsSavedByFactory);

			specialHandlingItem.EP_SpecialHandling = "AAA";
			AssertEquals("Overridden is saved when has changes", true, specialHandlingItem.IsSavedByFactory);
			Factory.Save();

			aWBHeader.Consol.JK_OverrideWaybillDefaults = false;
			AssertEquals("Overridden is saved when 'Override' has changes and already in the database", true, specialHandlingItem.IsSavedByFactory);
			var specialHandlingItem2 = aWBHeader.AWBSpecialHandlingItems.AddNew();
			Factory.Save();
			AssertEquals(true, specialHandlingItem.IsDeleted);
			AssertEquals(true, specialHandlingItem2.IsInDatabase);

			specialHandlingItem2.EP_SpecialHandling = "BBB";
			AssertEquals("Not overridden and saved is not saved when has changes", false, specialHandlingItem2.IsSavedByFactory);
		}

		public void TestValidation()
		{
			var specialHandlingItem = Factory.New<ExportAWBSpecialHandling>();
			AssertEquals(typeof(ExportAWBSpecialHandlingValidation), specialHandlingItem.Validation.GetType());
		}

		public void TestSecurityModifiedEventLogged_SPXChosen()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

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
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SCO";

				Factory.Save();

				AssertNull(consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC"));

				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				specialHandlingItem.EP_SpecialHandling = "SPX";

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
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

				AssertEquals("When not verified, SPX will not be set", string.Empty, specialHandlingItem.EP_SpecialHandling);

				Factory.Save();

				AssertNull(consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC"));
			}
		}

		public void TestSecurityModifiedEvent_NotRequiredWhenNotSCSCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

				AssertEquals("When not Supply Chain Security supported Company, user is not asked to verify", false, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);

				Factory.Save();

				AssertNull(consol.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_SE_NKEvent == "SEC"));
			}
		}

		public void TestSecurityModifiedEvent_RequiredWhenSCSSupportedCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.Canada;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

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
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

				AssertEquals("When UK Company, user is asked to verify", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
			}
		}

		public void TestSecurityModifiedEvent_NotRequiredWhenNotUKConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.Australia;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

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
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

				var header = consol.AWBHeader;

				var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();
				specialHandlingItem.EP_SpecialHandling = "SPX";

				AssertEquals("UK Consol, required to verify", true, ((FreightVerifiedStub)Factory.GetValue<ISecuredFreightVerificationChecker>()).StubCalled);
			}
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();

			return specialHandlingItem;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var header = consol.AWBHeader;
			header.ForceSavingByFactory = true;

			var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();

			AssertEquals("Precondition: awbHeader.IsSavedByFactory", true, header.IsSavedByFactory);
			AssertEquals("Precondition: specialHandlingItem.IsSavedByFactory", true, specialHandlingItem.IsSavedByFactory);

			return specialHandlingItem;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			var specialHandlingItem = header.AWBSpecialHandlingItems.AddNew();

			return specialHandlingItem;
		}

		#endregion
	}
}
