using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class OldCFSShipmentStatusProviderTest : TestCaseWithFactory
	{
		public void TestDetailsFromMessages()
		{
			AssertEquals(ZString.Empty, provider.DetailsFromMessages);
		}

		public void TestCanSaveAndPrint()
		{
			const string notClearMessage = "This shipment has not been cleared by customs.  Continue with Contingency Release?";
			const string acsSeizedMessage = "This shipment has been seized by customs and may not be gate passed.";
			const string aqisSeizedMessage = "This shipment has been seized by Quarantine and may not be gate passed";
			const string hrmWarningMessage = "This shipment is marked as high risk";
			const string condClearMessage = "Confirm conditional clearance actions have been completed.\r\nHave the conditional clearance requirements been met?";
			const string submovMessage = "This shipment is clear to be moved underbond but may not be delivered for home consumption";
			const string errorsMessage = "Please clear all errors before saving.";

			OldCFSShipmentStatusProviderTestHelper provider = new OldCFSShipmentStatusProviderTestHelper(shipment);

			AssertEquals("precondition", false, shipment.HasErrors);

			var mock = new Mock<ISaveAndPrintUI>();

			provider.StatusExposed = "foo";

			provider.ShortStatusExposed = "";
			mock.Setup(m => m.Ask(notClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = "";
			mock.Setup(m => m.Ask(notClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			mock.Setup(m => m.ShowError(acsSeizedMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			mock.Setup(m => m.ShowError(aqisSeizedMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			mock.Setup(m => m.ShowWarning(hrmWarningMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			mock.Setup(m => m.Ask(condClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			mock.Setup(m => m.Ask(condClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
			mock.Setup(m => m.ShowWarning(submovMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			mock.Setup(m => m.ShowWarning(hrmWarningMessage));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction;
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;
			mock.Setup(m => m.Ask(notClearMessage)).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.StatusExposed = ZString.Empty;
			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			mock.Setup(m => m.ShowError(aqisSeizedMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ShortStatusExposed = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			shipment.RunPreSaveValidation();
			AssertEquals("precondition: shipment has errors", true, shipment.HasErrors);
			mock.Setup(m => m.ShowError(errorsMessage));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();
		}

		public void TestStatus()
		{
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.AcsSeized);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.AqisSeized);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.Clear);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.ClearHrm);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.ConditionalClear);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.Detained);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.Held);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.SubUBMov);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.Tranship);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.TranshipHrm);
			AssertStatus(OldCFSShipmentStatusProvider.CMRGatePassStatuses.Transit);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "woo";
			}
			AssertEquals(ZString.Empty, provider.Status);
		}

		public void TestShortStatus()
		{
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, OldCFSShipmentStatusProvider.CMRGatePassStatuses.AcsSeized);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine, OldCFSShipmentStatusProvider.CMRGatePassStatuses.AqisSeized);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Clear);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, OldCFSShipmentStatusProvider.CMRGatePassStatuses.ClearHrm);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation, OldCFSShipmentStatusProvider.CMRGatePassStatuses.ConditionalClear);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Held);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, OldCFSShipmentStatusProvider.CMRGatePassStatuses.SubUBMov);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Tranship);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement, OldCFSShipmentStatusProvider.CMRGatePassStatuses.TranshipHrm);
			AssertShortStatus(CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction, OldCFSShipmentStatusProvider.CMRGatePassStatuses.Transit);
		}

		#region Implementation

		class OldCFSShipmentStatusProviderTestHelper : OldCFSShipmentStatusProvider
		{
			public OldCFSShipmentStatusProviderTestHelper(GatePassShipment shipment)
				: base(shipment)
			{
			}

			protected override ZString StatusCore()
			{
				return StatusExposed;
			}
			public ZString StatusExposed;

			protected override ZString ShortStatusCore()
			{
				return ShortStatusExposed;
			}
			public ZString ShortStatusExposed;
		}

		void AssertShortStatus(string expected, string source)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = source + "foo";
			}
			AssertEquals(expected, provider.ShortStatus);
		}

		void AssertStatus(string expected)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = expected + "foo";
			}
			AssertEquals(expected + "foo", provider.Status);
		}

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.New<GatePassShipment>();
			provider = new OldCFSShipmentStatusProvider(shipment);
			log = shipment.Logs.AddNew(Events.SeaCargoDepotEvent);
		}

		GatePassShipment shipment;
		OldCFSShipmentStatusProvider provider;
		StmALog log;

		#endregion
	}
}
