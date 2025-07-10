using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CrossTradeDebtorDefaultingTest : TestCaseWithFactory
	{
		public void TestGetDefaultPartyToChargeForCrossTrade_JobType()
		{
			var configs = new List<ICrossTradeDebtorDefaultingConfigurationItem>()
			{
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, true, false, ChargedPartyForCrossTradeJob.LocalClient),
				GetConfigurationItem("ALL", Constants.TransportModes.All, true, false, ChargedPartyForCrossTradeJob.Agent)
			};

			var configProvider = new Mock<ICrossTradeDebtorDefaultingConfigurationProvider>(MockBehavior.Strict);
			configProvider.Setup(p => p.GetConfiguration()).Returns(configs);

			using (ObjectFactory.Substitute(configProvider.Object))
			{
				var chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, true);
				AssertEquals("SHP-Air-Collect", ChargedPartyForCrossTradeJob.LocalClient, chargeParty);

				chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Air, true);
				AssertEquals("QSH-Air-Collect", ChargedPartyForCrossTradeJob.Agent, chargeParty);

				foreach (var jobTypeCode in JobInvoicingConsumerTypes
									.NewOnlyJobInvoicingTypes()
									.GetAllCodes()
									.Except(new string[] { JobInvoicingConsumerTypes.ShipmentCode, JobInvoicingConsumerTypes.QuotedBookingCode, JobInvoicingConsumerTypes.BrokerageCode }))
				{
					chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(jobTypeCode, Constants.TransportModes.Air, true);
					AssertEquals(FormattableString.Invariant($"{jobTypeCode}-Air-Collect"), ChargedPartyForCrossTradeJob.Unknown, chargeParty);
				}
			}
		}

		public void TestGetDefaultPartyToChargeForCrossTrade_TransportMode()
		{
			var configs = new List<ICrossTradeDebtorDefaultingConfigurationItem>()
			{
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, true, false, ChargedPartyForCrossTradeJob.LocalClient),
				GetConfigurationItem("ALL", Constants.TransportModes.Sea, true, false, ChargedPartyForCrossTradeJob.Agent)
			};

			var configProvider = new Mock<ICrossTradeDebtorDefaultingConfigurationProvider>(MockBehavior.Strict);
			configProvider.Setup(p => p.GetConfiguration()).Returns(configs);

			using (ObjectFactory.Substitute(configProvider.Object))
			{
				var chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, true);
				AssertEquals("SHP-Sea-Collect", ChargedPartyForCrossTradeJob.Unknown, chargeParty);

				chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Sea, true);
				AssertEquals("QSH-Sea-Collect", ChargedPartyForCrossTradeJob.Agent, chargeParty);
			}
		}

		public void TestGetDefaultPartyToChargeForCrossTrade_ChargePayCollectType()
		{
			var configs = new List<ICrossTradeDebtorDefaultingConfigurationItem>()
			{
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Air, true, false, ChargedPartyForCrossTradeJob.LocalClient),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, true, false, ChargedPartyForCrossTradeJob.Agent),
				GetConfigurationItem(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, false, true, ChargedPartyForCrossTradeJob.LocalClient),
				GetConfigurationItem(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Air, true, true, ChargedPartyForCrossTradeJob.Agent),
				GetConfigurationItem("ALL", "ALL", true, false, ChargedPartyForCrossTradeJob.Agent)
			};

			var configProvider = new Mock<ICrossTradeDebtorDefaultingConfigurationProvider>(MockBehavior.Strict);
			configProvider.Setup(p => p.GetConfiguration()).Returns(configs);

			using (ObjectFactory.Substitute(configProvider.Object))
			{
				var chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, true);
				AssertEquals("SHP-Sea-Collect", ChargedPartyForCrossTradeJob.Agent, chargeParty);

				chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.ShipmentCode, Constants.TransportModes.Sea, false);
				AssertEquals("SHP-Sea-Prepaid", ChargedPartyForCrossTradeJob.LocalClient, chargeParty);

				chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.QuotedBookingCode, Constants.TransportModes.Air, true);
				AssertEquals("QSH-Air-Collect", ChargedPartyForCrossTradeJob.Agent, chargeParty);

				chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.BrokerageCode, Constants.TransportModes.Sea, true);
				AssertEquals("BRK-Sea-Collect", ChargedPartyForCrossTradeJob.Unknown, chargeParty);

				chargeParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(JobInvoicingConsumerTypes.BrokerageCode, Constants.TransportModes.Sea, false);
				AssertEquals("BRK-Sea-Prepaid", ChargedPartyForCrossTradeJob.Unknown, chargeParty);
			}
		}

		ICrossTradeDebtorDefaultingConfigurationItem GetConfigurationItem(ZString jobType, ZString transportMode, bool isCollect, bool isPrepaid, ChargedPartyForCrossTradeJob chargedParty)
		{
			var mockConfigItem = new Mock<ICrossTradeDebtorDefaultingConfigurationItem>(MockBehavior.Strict);
			mockConfigItem.Setup(c => c.JobTypeCode).Returns(jobType);
			mockConfigItem.Setup(c => c.TransportModeCode).Returns(transportMode);
			mockConfigItem.Setup(c => c.IsCollect).Returns(isCollect);
			mockConfigItem.Setup(c => c.IsPrepaid).Returns(isPrepaid);
			mockConfigItem.Setup(c => c.BillToParty).Returns(chargedParty);
			return mockConfigItem.Object;
		}
	}
}
