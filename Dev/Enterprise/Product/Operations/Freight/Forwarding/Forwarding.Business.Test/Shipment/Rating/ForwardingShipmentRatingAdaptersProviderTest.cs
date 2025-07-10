using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using FluentAssertions;

namespace Enterprise.Freight.Forwarding.Business
{
	sealed class ForwardingShipmentRatingAdaptersProviderTest : BaseFreightTest
	{
		#region BCN

		#region Lead

		#region Master

		public void TestGetAdapters_BCN_LeadShipment_MasterInvoicingStyle()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Master);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_LeadShipment_MasterInvoicingStyle_OriginIsAmongApportionCharges()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.BuyersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(lead).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If ORG charge is among buyers consol apportioned codes, it will be calculated based on total amount same as freight and destination charges and we don't need to calculate it separately for each shipment");

				Assert(true);
			}
		}

		#endregion

		#region Apportions

		public void TestGetAdapters_BCN_LeadShipment_ApportionInvoicingStyle()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_CrossTrade_LeadShipment_ApportionInvoicingStyle()
		{
			var (lead, sub) = GetBcnCrossTradeShipments(Constants.ConsolInvoicingStyles.Apportion);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_LeadShipment_ApportionInvoicingStyle_OriginIsAmongApportionCharges()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.BuyersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(lead).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(
				[
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				], "If ORG charge is among buyers consol apportioned codes, it will be apportioned as freight and destination charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		#region ApportionInvoiceMaster

		public void TestGetAdapters_BCN_LeadShipment_ApportionInvoiceMasterInvoicingStyle()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_CrossTrade_LeadShipment_ApportionInvoiceMasterInvoicingStyle()
		{
			var (lead, sub) = GetBcnCrossTradeShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_LeadShipment_ApportionInvoiceMasterInvoicingStyle_OriginIsAmongApportionCharges()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			using (RawDataRegistry.Instance.BuyersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(lead).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If ORG charge is among buyers consol apportioned codes, it will be apportioned as freight and destination charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		#endregion

		#region Sub

		public void TestGetAdapters_BCN_SubShipment_MasterInvoicingStyle()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Master);

			var actualAdapters = GetAdapters(sub);
			actualAdapters.Should().BeEmpty();

			Logger.errors.Should().Contain(FormattableString.Invariant($"This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment {lead.JS_UniqueConsignRef}"));
			Assert(true);
		}

		public void TestGetAdapters_BCN_SubShipment_ApportionInvoicingStyle_NotAttachedToLeadShipment()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);
			sub.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			var actualAdapters = GetAdapters(sub);
			actualAdapters.Should().BeEmpty();

			Logger.errors.Should().Contain("This Shipment is marked as a Buyer's Consol but there is no Lead specified. Please specify a Lead Shipment if you wish to apportion Buyer's Consol charges when Autorating.");
			Assert(true);
		}

		public void TestGetAdapters_BCN_SubShipment_ApportionInvoiceMasterInvoicingStyle_NotAttachedToLeadShipment()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);
			sub.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			var actualAdapters = GetAdapters(sub);
			actualAdapters.Should().BeEmpty();

			Logger.errors.Should().Contain("This Shipment is marked as a Buyer's Consol but there is no Lead specified. Please specify a Lead Shipment if you wish to apportion Buyer's Consol charges when Autorating.");
			Assert(true);
		}

		public void TestGetAdapters_BCN_SubShipment_ApportionInvoicingStyle()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);

			var actualAdapters = GetAdapters(sub).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_CrossTrade_SubShipment_ApportionInvoicingStyle()
		{
			var (lead, sub) = GetBcnCrossTradeShipments(Constants.ConsolInvoicingStyles.Apportion);

			var actualAdapters = GetAdapters(sub).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_SubShipment_ApportionInvoicingStyle_OriginIsAmongApportionCharges()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.BuyersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(sub).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If ORG charge is among buyers consol apportioned codes, it will be apportioned as freight and destination charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		public void TestGetAdapters_BCN_SubShipment_ApportionInvoiceMasterInvoicingStyle()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			var actualAdapters = GetAdapters(sub).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_CrossTrade_SubShipment_ApportionInvoiceMasterInvoicingStyle()
		{
			var (lead, sub) = GetBcnCrossTradeShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			var actualAdapters = GetAdapters(sub).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST,FRT,INS,LOD,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "ORG"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_BCN_SubShipment_ApportionInvoiceMasterInvoicingStyle_OriginIsAmongApportionCharges()
		{
			var (lead, sub) = GetBcnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			using (RawDataRegistry.Instance.BuyersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(sub).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If ORG charge is among buyers consol apportioned codes, it will be apportioned as freight and destination charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		(ForwardingShipment lead, ForwardingShipment bcn) GetBcnShipments(string invoicingStyle)
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingStyle;

			var overseasAgent = Factory.New<OrgHeader>();
			overseasAgent.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Master;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_RL_NKOrigin = "AUSYD";
			leadShipment.JS_RL_NKDestination = "USLAX";
			leadShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_UniqueConsignRef = "Lead";
			new JobHeader.Loader(leadShipment).TryCreate();
			leadShipment.Job.LocalChargesPK = localClient.PK;
			leadShipment.Job.AgentCollectPK = overseasAgent.PK;

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_RL_NKOrigin = "AUSYD";
			subShipment.JS_RL_NKDestination = "USLAX";
			subShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_UniqueConsignRef = "Sub";
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			new JobHeader.Loader(subShipment).TryCreate();
			subShipment.Job.LocalChargesPK = localClient.PK;
			subShipment.Job.AgentCollectPK = overseasAgent.PK;

			return (leadShipment, subShipment);
		}

		(ForwardingShipment lead, ForwardingShipment bcn) GetBcnCrossTradeShipments(string invoicingStyle)
		{
			var collectBillToParty = Factory.New<OrgHeader>();
			collectBillToParty.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingStyle;

			var prepaidBillToParty = Factory.New<OrgHeader>();
			prepaidBillToParty.CompanyData.OB_ARBuyersConsolInvoicingStyle = Constants.ConsolInvoicingStyles.Master;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "USLAX";

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_RL_NKOrigin = "SGSIN";
			leadShipment.JS_RL_NKDestination = "USLAX";
			leadShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_UniqueConsignRef = "Lead";
			new JobHeader.Loader(leadShipment).TryCreate();
			leadShipment.Job.LocalChargesPK = prepaidBillToParty.PK;
			leadShipment.Job.AgentCollectPK = collectBillToParty.PK;

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_RL_NKOrigin = "SGSIN";
			subShipment.JS_RL_NKDestination = "USLAX";
			subShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_UniqueConsignRef = "Sub";
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			new JobHeader.Loader(subShipment).TryCreate();
			subShipment.Job.LocalChargesPK = prepaidBillToParty.PK;
			subShipment.Job.AgentCollectPK = collectBillToParty.PK;

			return (leadShipment, subShipment);
		}

		IEnumerable<IAutoRating> GetAdapters(ForwardingShipment shipment)
		{
			var adaptersProvider = new ForwardingShipmentRatingAdaptersProvider(shipment);
			var adapters = adaptersProvider.GetAdapters(Logger, new AutoRateOptions(autoRateRevenue: true, billingType: BillingType.Invoicing));

			return adapters;
		}

		RatingAdaptersProviderTest.TestUIInteractor Logger { get; } = new RatingAdaptersProviderTest.TestUIInteractor();

		#endregion

		#region SCN

		#region Lead

		#region Master

		public void TestGetAdapters_SCN_LeadShipment_MasterInvoicingStyle()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Master);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "FRT,INS,LOD,ORG,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_SCN_LeadShipment_MasterInvoicingStyle_DestinationIsAmongApportionCharges()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.ShippersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(lead).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If DST charge is among shipper consol apportioned codes, it will be calculated based on total amount same as freight and origin charges and we don't need to calculate it separately for each shipment");

				Assert(true);
			}
		}

		#endregion

		#region Apportion

		public void TestGetAdapters_SCN_LeadShipment_ApportionInvoicingStyle()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "FRT,INS,LOD,ORG,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_SCN_LeadShipment_ApportionInvoicingStyle_DestinationIsAmongApportionCharges()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.ShippersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(lead).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If DST charge is among buyers consol apportioned codes, it will be apportioned as freight and origin charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		#region ApportionInvoiceMaster

		public void TestGetAdapters_SCN_LeadShipment_ApportionInvoiceMasterInvoicingStyle()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			var actualAdapters = GetAdapters(lead).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "FRT,INS,LOD,ORG,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_SCN_LeadShipment_ApportionInvoiceMasterInvoicingStyle_DestinationIsAmongApportionCharges()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.ShippersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(lead).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If DST charge is among buyers consol apportioned codes, it will be apportioned as freight and origin charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		#endregion

		#region Sub

		#region Master

		public void TestGetAdapters_SCN_SubShipment_MasterInvoicingStyle()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Master);

			var actualAdapters = GetAdapters(sub);
			actualAdapters.Should().BeEmpty();

			Logger.errors.Should().Contain(FormattableString.Invariant($"This Shipment is part of a Shipper's Consol. Generally, you should Autorate and invoice from the Shipper's Consol Master - Shipment {lead.JS_UniqueConsignRef}"));
			Assert(true);
		}

		#endregion

		public void TestGetAdapters_SCN_SubShipment_ApportionInvoicingStyle_NotAttachedToLeadShipment()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);
			sub.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			var actualAdapters = GetAdapters(sub);
			actualAdapters.Should().BeEmpty();

			Logger.errors.Should().Contain("This Shipment is marked as a Shipper's Consol but there is no Lead specified. Please specify a Lead Shipment if you wish to apportion Shipper's Consol charges when Autorating.");
			Assert(true);
		}

		public void TestGetAdapters_SCN_SubShipment_ApportionInvoiceMasterInvoicingStyle_NotAttachedToLeadShipment()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);
			sub.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			var actualAdapters = GetAdapters(sub);
			actualAdapters.Should().BeEmpty();

			Logger.errors.Should().Contain("This Shipment is marked as a Shipper's Consol but there is no Lead specified. Please specify a Lead Shipment if you wish to apportion Shipper's Consol charges when Autorating.");
			Assert(true);
		}

		#region Apportion

		public void TestGetAdapters_SCN_SubShipment_ApportionInvoicingStyle()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);

			var actualAdapters = GetAdapters(sub).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "FRT,INS,LOD,ORG,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_SCN_SubShipment_ApportionInvoicingStyle_DestinationIsAmongApportionCharges()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.Apportion);

			using (RawDataRegistry.Instance.ShippersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(sub).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If DST charge is among buyers consol apportioned codes, it will be apportioned as freight and origin charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		#region ApportionInvoiceMaster

		public void TestGetAdapters_SCN_SubShipment_ApportionInvoiceMasterInvoicingStyle()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			var actualAdapters = GetAdapters(sub).Select(a => new
			{
				AdapterType = a.AdapterType.ToString(),
				JobID = a.JobID.ToString(),
				ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
			});

			actualAdapters.Should().BeEquivalentTo(new[]
			{
				new
				{
					AdapterType = "Consolidation",
					JobID = lead.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "FRT,INS,LOD,ORG,UNL"
				},
				new
				{
					AdapterType = "Shipment",
					JobID = sub.JS_UniqueConsignRef.ToString(),
					ChargeCodeGroups = "DST"
				}
			});

			Assert(true);
		}

		public void TestGetAdapters_SCN_SubShipment_ApportionInvoiceMasterInvoicingStyle_DestinationIsAmongApportionCharges()
		{
			var (lead, sub) = GetScnShipments(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);

			using (RawDataRegistry.Instance.ShippersConsolApportionedCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DST,FRT,INS,LOD,ORG,UNL"))
			{
				var actualAdapters = GetAdapters(sub).Select(a => new
				{
					AdapterType = a.AdapterType.ToString(),
					JobID = a.JobID.ToString(),
					ChargeCodeGroups = string.Join(",", a.ChargeCodeGroups.Cast<string>().OrderBy(c => c))
				});

				actualAdapters.Should().BeEquivalentTo(new[]
				{
					new
					{
						AdapterType = "Consolidation",
						JobID = lead.JS_UniqueConsignRef.ToString(),
						ChargeCodeGroups = "DST,FRT,INS,LOD,ORG,UNL"
					}
				}, "If DST charge is among buyers consol apportioned codes, it will be apportioned as freight and origin charges and we don't need to calculate it separately");

				Assert(true);
			}
		}

		#endregion

		#endregion

		(ForwardingShipment lead, ForwardingShipment scn) GetScnShipments(string invoicingStyle)
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.CompanyData.OB_ARShippersConsolInvoicingStyle = invoicingStyle;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			leadShipment.JS_UniqueConsignRef = "Lead";
			new JobHeader.Loader(leadShipment).TryCreate();
			leadShipment.Job.LocalChargesPK = localClient.PK;

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_UniqueConsignRef = "Sub";
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			new JobHeader.Loader(subShipment).TryCreate();
			subShipment.Job.LocalChargesPK = localClient.PK;

			return (leadShipment, subShipment);
		}
		#endregion
	}
}