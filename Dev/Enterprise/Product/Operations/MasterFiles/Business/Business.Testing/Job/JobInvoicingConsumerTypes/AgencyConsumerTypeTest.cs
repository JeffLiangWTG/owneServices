using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AgencyConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		#region WIP / Accrual Creation

		public void TestShouldCreateWIPsOrAccruals_Import()
		{
			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);

				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.Misc));

				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.Misc));
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(true);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(true);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.Misc));

				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.Misc));
			}

			registry.VerifyAll();
		}

		public void TestShouldCreateWIPsOrAccruals_Export()
		{
			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = HomePort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = OverseasPort;

				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.Misc));

				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(false, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.Misc));
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(true);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(true);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.Misc));

				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
				AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.Misc));
			}

			registry.VerifyAll();
		}

		public void TestShouldCreateWIPsOrAccruals_Domestic()
		{
			((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = HomePort;
			((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = AltHomePort;

			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateWIPs(Job, AgencyInvoiceTypesList.Codes.Misc));

			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.LocalCollect));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.ForeignCollect));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.DoNotPost));
			AssertEquals(true, GetJobInvoicingConsumerType().ShouldCreateAccruals(Job, AgencyInvoiceTypesList.Codes.Misc));
		}

		#endregion

		#region Charge Posting

		public void TestShouldPostCharges_Import()
		{
			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(true);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(true);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
			}

			registry.VerifyAll();
		}

		public void TestShouldPostCharges_Export()
		{
			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = HomePort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = OverseasPort;

				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
				AssertShouldPostCharges(false, "The Invoice Type is Collect however the destination on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type is Collect however the destination on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
				AssertShouldPostCharges(false, "The Invoice Type is Collect however the destination on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
				AssertShouldPostCharges(false, "The Invoice Type is Collect however the destination on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(true);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(true);
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = OverseasPort;
				((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = HomePort;

				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
				AssertShouldPostCharges(false, "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
				AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
				AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
			}

			registry.VerifyAll();
		}

		public void TestShouldPostCharges_Domestic()
		{
			((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Origin = HomePort;
			((AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter).Destination = AltHomePort;

			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, false));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, false));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, false));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, false));
			AssertShouldPostCharges(false, "The Invoice Type for charges is set to 'Do Not Post'.", GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, false));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, false));

			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalPrePaid, true));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignPrePaid, true));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.LocalCollect, true));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.ForeignCollect, true));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.DoNotPost, true));
			AssertShouldPostCharges(true, null, GetJobInvoicingConsumerType().ShouldPostCharges(Job, AgencyInvoiceTypesList.Codes.Misc, true));
		}

		void AssertShouldPostCharges(bool shouldPost, string expectedReason, PostChargesAllowedInformation info)
		{
			AssertEquals(shouldPost, info.PostAllowed);
			AssertEquals(expectedReason, info.ReasonForDisallowing);
		}

		#endregion

		#region Get Invoice Type

		/// <summary>
		/// Given a desired payment term to use e.g. "Prepaid", it maps this to many of the possible
		/// combinations of test inputs that can produce it, e.g.: using a freight charge code, an
		/// inco type of Prepaid and no invoice type is one way to achieve a "Prepaid" payment term.
		/// </summary>
		Dictionary<string, List<InputForPaymentTerm>> inputsToProducePaymentTermCollection;

		const string testGetOverriddenInvoiceType_LookupTable =
			//Job Direction: Payment Term: Currency:    Debtor Posting Style:   Expected Invoice Type Default:
			@"Import,        Prepaid,       Local,      Any,                    Prepaid Local
Import,          Prepaid,       Foreign,    Local Currency,         Prepaid Local
Import,          Prepaid,       Foreign,    Foreign Currency,       Prepaid Foreign
Import,          Prepaid,       Foreign,    Not Specified,          Prepaid Foreign
Import,          Collect,       Local,      Any,                    Collect Local
Import,          Collect,       Foreign,    Local Currency,         Collect Local
Import,          Collect,       Foreign,    Foreign Currency,       Collect Foreign
Import,          Collect,       Foreign,    Not Specified,          Collect Foreign
Export,          Collect,       Local,      Any,                    Collect Local
Export,          Collect,       Foreign,    Local Currency,         Collect Local
Export,          Collect,       Foreign,    Foreign Currency,       Collect Foreign
Export,          Collect,       Foreign,    Not Specified,          Collect Foreign
Export,          Prepaid,       Local,      Any,                    Prepaid Local
Export,          Prepaid,       Foreign,    Local Currency,         Prepaid Local
Export,          Prepaid,       Foreign,    Foreign Currency,       Prepaid Foreign
Export,          Prepaid,       Foreign,    Not Specified,          Prepaid Foreign
Any,             Not Specified, Any,        Any,                    None";

		public void TestGetOverriddenInvoiceType()
		{
			var invoicingSupporter = (AgencyShipmentMockJobInvoicingSupporter)Job.InvoicingSupporter;

			GetOverriddenInvoiceTypeTest_InitLookups();
			GetOverriddenInvoiceTypeTest_InputsToProducePaymentTermCollection();

			var lines = testGetOverriddenInvoiceType_LookupTable.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
			foreach (string line in lines)
			{
				var items = line.Split(',').Select(s => s.Trim()).ToArray();

				// Each line in the lookup table can be tested using many different inputs, so iterate through a bunch of these combinations and test them
				var testCombinations =
					from origin in AnyLookup(items[0], jobDirectionOriginLookup)
					from inputForPaymentTerm in AnyLookup(items[1], incotermsLookup).SelectMany(incoterm => inputsToProducePaymentTermCollection[incoterm])
					from currency in AnyLookup(items[2], currencyLookup)
					from debtorPostingStyle in AnyLookup(items[3], debtorPostingStylesLookup)
					from expectedInvoiceTypeDefault in AnyLookup(items[4], expectedInvoiceTypeDefaultLookup)
					select new OverriddenInvoiceTypeTestCombination
					{
						TestRuleInfoString = string.Join(",", items),
						Origin = origin,
						Destination = origin == HomePort ? OverseasPort : HomePort,
						InputForPaymentTerm = inputForPaymentTerm,
						Currency = currency,
						DebtorPostingStyle = debtorPostingStyle,
						ExpectedInvoiceTypeDefault = expectedInvoiceTypeDefault
					};

				foreach (var testCombination in testCombinations)
				{
					testCombination.PerformAssert(Job, invoicingSupporter, GetJobInvoicingConsumerType());
				}
			}
		}

		static T[] AnyLookup<T>(string key, Dictionary<string, T> dictionary)
		{
			return key == "Any" ? dictionary.Values.ToArray() : new[] { dictionary[key] };
		}

		void GetOverriddenInvoiceTypeTest_InputsToProducePaymentTermCollection()
		{
			const string prepaid = Constants.IncoTerms.CostInsuranceAndFreight;
			const string collect = Constants.IncoTerms.FreeOnBoard;

			var allIncoTermsToTest = new[] { collect, prepaid };
			inputsToProducePaymentTermCollection = new Dictionary<string, List<InputForPaymentTerm>>();

			var inputsForPrepaid = new List<InputForPaymentTerm>();
			inputsToProducePaymentTermCollection.Add(prepaid, inputsForPrepaid);
			foreach (var incoTerm in allIncoTermsToTest)
			{
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid, ChargeCode = FreightChargeCode, IncoTerm = incoTerm });
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid, ChargeCode = DestinationChargeCode, IncoTerm = incoTerm });
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid, ChargeCode = OriginChargeCode, IncoTerm = incoTerm });
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid, ChargeCode = FreightChargeCode, IncoTerm = incoTerm });
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid, ChargeCode = DestinationChargeCode, IncoTerm = incoTerm });
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid, ChargeCode = OriginChargeCode, IncoTerm = incoTerm });
				inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = "", ChargeCode = OriginChargeCode, IncoTerm = incoTerm });
			}
			inputsForPrepaid.Add(new InputForPaymentTerm { CurrentInvoiceType = "", ChargeCode = FreightChargeCode, IncoTerm = prepaid });

			var inputsForCollect = new List<InputForPaymentTerm>();
			inputsToProducePaymentTermCollection.Add(collect, inputsForCollect);
			foreach (var incoTerm in allIncoTermsToTest)
			{
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect, ChargeCode = FreightChargeCode, IncoTerm = incoTerm });
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect, ChargeCode = DestinationChargeCode, IncoTerm = incoTerm });
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect, ChargeCode = OriginChargeCode, IncoTerm = incoTerm });
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect, ChargeCode = FreightChargeCode, IncoTerm = incoTerm });
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect, ChargeCode = DestinationChargeCode, IncoTerm = incoTerm });
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect, ChargeCode = OriginChargeCode, IncoTerm = incoTerm });
				inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = "", ChargeCode = DestinationChargeCode, IncoTerm = incoTerm });
			}
			inputsForCollect.Add(new InputForPaymentTerm { CurrentInvoiceType = "", ChargeCode = FreightChargeCode, IncoTerm = collect });

			var inputsForNone = new List<InputForPaymentTerm>();
			inputsForNone.Add(new InputForPaymentTerm { CurrentInvoiceType = "", ChargeCode = FreightChargeCode, IncoTerm = "" });
			inputsToProducePaymentTermCollection.Add("", inputsForNone);
		}

		void GetOverriddenInvoiceTypeTest_InitLookups()
		{
			jobDirectionOriginLookup = new Dictionary<string, RefUNLOCO>
			{
				{ "Import", OverseasPort },
				{ "Export", HomePort }
			};
			incotermsLookup = new Dictionary<string, string>
			{
				{ "Prepaid", "CIF" },
				{ "Collect", "FOB" },
				{ "Not Specified", "" }
			};
			currencyLookup = new Dictionary<string, RefCurrency>
			{
				{ "Local", LocalCurrency },
				{ "Foreign", ForeignCurrency }
			};
			debtorPostingStylesLookup = new Dictionary<string, string>
			{
				{ "Local Currency", InvoicePostingOptionsList.Codes.FinalInvoiceOnly },
				{ "Foreign Currency", InvoicePostingOptionsList.Codes.DisbursementForeignOnly },
				{ "Not Specified", null }
			};
			expectedInvoiceTypeDefaultLookup = new Dictionary<string, string>
			{
				{ "Prepaid Local", AgencyInvoiceTypesList.Codes.LocalPrePaid },
				{ "Prepaid Foreign", AgencyInvoiceTypesList.Codes.ForeignPrePaid },
				{ "Collect Local", AgencyInvoiceTypesList.Codes.LocalCollect },
				{ "Collect Foreign", AgencyInvoiceTypesList.Codes.ForeignCollect },
				{ "None", "" }
			};
		}

		public void TestGetInvoiceTypeWithNoDebtor()
		{
			ZString currentInvoiceType = ZString.Empty;
			AssertEquals(ZString.Empty, GetJobInvoicingConsumerType().GetInvoiceTypeWithNoDebtor(currentInvoiceType));

			currentInvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect;
			AssertEquals(AgencyInvoiceTypesList.Codes.LocalCollect, GetJobInvoicingConsumerType().GetInvoiceTypeWithNoDebtor(currentInvoiceType));

			currentInvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			AssertEquals(AgencyInvoiceTypesList.Codes.ForeignPrePaid, GetJobInvoicingConsumerType().GetInvoiceTypeWithNoDebtor(currentInvoiceType));
		}

		#endregion

		#region Charge Collect

		public void TestIsChargeCollectReturnsValueForAllChargeGroups()
		{
			//Collecting all Charge Group Name
			Type type = typeof(ChargeCodeGroupList.Codes);
			var allCodeFieldInfo = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToArray();
			if (allCodeFieldInfo.Any())
			{
				var chargeCode = Factory.New<AccChargeCode>();
				var host = new JobInvoicingPlugInImplementorForTest(Factory);

				var expected = new List<Tuple<ZString, bool?>>
				{
					{ ChargeCodeGroupList.Codes.Brokerage, true },
					{ ChargeCodeGroupList.Codes.BrokerageOnly, true },
					{ ChargeCodeGroupList.Codes.CFSLoadList, true },
					{ ChargeCodeGroupList.Codes.CFSShipment, true },
					{ ChargeCodeGroupList.Codes.ContainerStorage, null },
					{ ChargeCodeGroupList.Codes.CustomsDuty, true },
					{ ChargeCodeGroupList.Codes.Destination, true },
					{ ChargeCodeGroupList.Codes.Freight, true },
					{ ChargeCodeGroupList.Codes.Insurance, true },
					{ ChargeCodeGroupList.Codes.Loading, false },
					{ ChargeCodeGroupList.Codes.LabourHourRate, true },
					{ ChargeCodeGroupList.Codes.NonJobRelated, true },
					{ ChargeCodeGroupList.Codes.NotGrouped, true },
					{ ChargeCodeGroupList.Codes.Origin, false },
					{ ChargeCodeGroupList.Codes.OriginBrokerage, false },
					{ ChargeCodeGroupList.Codes.OriginBrokerageOnly, false },
					{ ChargeCodeGroupList.Codes.ShippingDisbursements, null },
					{ ChargeCodeGroupList.Codes.Transport, true },
					{ ChargeCodeGroupList.Codes.TransportBooking, true },
					{ ChargeCodeGroupList.Codes.Unloading, true },
					{ ChargeCodeGroupList.Codes.WHSAdHocServiceJob, true },
					{ ChargeCodeGroupList.Codes.WHSInwards, true },
					{ ChargeCodeGroupList.Codes.WHSOutwards, true },
					{ ChargeCodeGroupList.Codes.WHSStorage, true },
					{ ChargeCodeGroupList.Codes.YardGateIn, true },
					{ ChargeCodeGroupList.Codes.YardGateOut, true },
					{ ChargeCodeGroupList.Codes.YardStorage, true },
					{ ChargeCodeGroupList.Codes.TRWReceive, true },
					{ ChargeCodeGroupList.Codes.TRWDispatch, true },
					{ ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, true },
					{ ChargeCodeGroupList.Codes.TRWDispatchLoadList, true },
					{ ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, true },
					{ ChargeCodeGroupList.Codes.CYDReceiveAdvice, true },
					{ ChargeCodeGroupList.Codes.CYDReleaseAdvice, true },
					{ ChargeCodeGroupList.Codes.CYDTransportationUnit, true },
					{ ChargeCodeGroupList.Codes.MNRWorkOrderHeader, true },
					{ ChargeCodeGroupList.Codes.YardTransportationUnitGateIn, true },
					{ ChargeCodeGroupList.Codes.YardTransportationUnitGateOut, true },
					// Not sure if a new groups is needed, or if we'll join the existing brokergae group
				};

				Func<FieldInfo, Tuple<ZString, bool?>> selector = fi =>
				{
					chargeCode.AC_ChargeGroup = Convert.ToString(fi.GetRawConstantValue());
					return Tuple.Create(chargeCode.AC_ChargeGroup, AgencyConsumerType.IsChargeCollect(host, chargeCode));
				};

				AssertContainsExactElementsInAnyOrder("IsChargeCollect should return correct data", expected, allCodeFieldInfo.Select(selector));
			}
		}

		#endregion

		#region Implementation

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}

		protected override void SetUp()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Constants.CountryCodes.Australia)
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			}
		}

		AccChargeCode OriginChargeCode
		{
			get { return originChargeCode ?? (originChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Origin)); }
		}

		AccChargeCode originChargeCode;

		AccChargeCode DestinationChargeCode
		{
			get { return destinationChargeCode ?? (destinationChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Destination)); }
		}

		AccChargeCode destinationChargeCode;

		AccChargeCode FreightChargeCode
		{
			get { return freightChargeCode ?? (freightChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Freight)); }
		}

		AccChargeCode freightChargeCode;

		AccChargeCode GetChargeCode(string group)
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = group;

			return chargeCode;
		}

		AgencyShipmentMockJob Job
		{
			get { return fJob ?? (fJob = new AgencyShipmentMockJob(Factory, GetJobInvoicingConsumerType())); }
		}

		AgencyShipmentMockJob fJob;

		RefUNLOCO HomePort
		{
			get { return homePort ?? (homePort = GetUnloco("AUSYD")); }
		}

		RefUNLOCO homePort;

		RefUNLOCO AltHomePort
		{
			get { return altHomePort ?? (altHomePort = GetUnloco("AUFRE")); }
		}

		RefUNLOCO altHomePort;

		RefUNLOCO OverseasPort
		{
			get { return overseasPort ?? (overseasPort = GetUnloco("GBSUN")); }
		}

		RefUNLOCO overseasPort;

		RefUNLOCO GetUnloco(string code)
		{
			return Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, code));
		}

		RefCurrency LocalCurrency
		{
			get { return HomePort.Country.LocalCurrency; }
		}

		RefCurrency ForeignCurrency
		{
			get { return OverseasPort.Country.LocalCurrency; }
		}

		#region Mapping Dictionaries

		// These dictionaries map the strings in testGetOverriddenInvoiceType_LookupTable to objects we can test with:
		Dictionary<string, RefUNLOCO> jobDirectionOriginLookup;
		Dictionary<string, string> incotermsLookup;
		Dictionary<string, RefCurrency> currencyLookup;
		Dictionary<string, string> debtorPostingStylesLookup;
		Dictionary<string, string> expectedInvoiceTypeDefaultLookup;

		#endregion

		#endregion
	}
}
