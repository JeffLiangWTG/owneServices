using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing;

public class HouseBillChargesTest : TestCaseWithFactory
{
	#region GetChargesAtOrigin

	public void TestGetChargesAtOrigin_ShipmentWithoutOrigin_NoExceptionThrown()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = ZString.Empty;
		shipment.JS_RL_NKDestination = ZString.Empty;

		var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		var (nzCompany, nzBranch) = CreateGlobalCompanyAndBranch("NZ", "NZ Branch 1");

		Factory.Save();

		var au = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, nzBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		using (FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetTemporaryValue(nzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { au.PK.ToGuid() }))
		{
			var houseBillCharges = new HouseBillCharges(shipment);
			AssertNoExceptionThrown(() => houseBillCharges.GetChargesAtOrigin());
		}
	}

	public void TestGetChargesAtOrigin_NoCompanyWorkInOrigin_ReturnEmpty()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "UAIEV";
		shipment.JS_RL_NKDestination = "USLAX";

		Factory.Save();

		var houseBillCharges = new HouseBillCharges(shipment);
		var actualCharges = houseBillCharges.GetChargesAtOrigin();
		actualCharges.Should().BeEmpty("because no company in Ukraine");
		Assert(true);
	}

	public void TestGetChargesAtOrigin_CommunityRegionCompanyHasChargesAtOrigin_ReturnChargesFromCommunityRegionCompany()
	{
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "USLAX";

		var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		var (nzCompany, nzBranch) = CreateGlobalCompanyAndBranch("NZ", "NZ Branch 1");

		Factory.Save();

		var au = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));

		// Make sure that the NZ company handles job originating in Australia
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, nzBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		using (FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetTemporaryValue(nzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { au.PK.ToGuid() }))
		{
				var houseBillCharges = new HouseBillCharges(shipment);
				var actualCharges = houseBillCharges.GetChargesAtOrigin();
				actualCharges.Should().BeEmpty("because neither australian company nor nz one has job header");

				// NZ company has created job header with charges for the shipment
				var newFactory = new BusinessObjectFactory();
				var shipmentInANewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
				var header = (Job)new JobHeader.Loader(shipmentInANewFactory).TryLoadOrCreateWithMutex();
				header.JH_GE = Env.CurrentDepartmentPK;
				var localClient = newFactory.NewWithValidTestData<OrgHeader>();
				localClient.OH_Code = "ORG1";
				header.LocalChargesPK = localClient.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 10, sellAccount: header.LocalCharges);
				newFactory.Save();

				houseBillCharges = new HouseBillCharges(shipmentInANewFactory);
				actualCharges = houseBillCharges.GetChargesAtOrigin();
				actualCharges.Should().HaveCount(1, "because NZ company has charges for the shipment");
				actualCharges.Single().JR_OSSellAmt.Should().Be(10m, "because NZ company has charges for the shipment");
		}

		Assert(true);
	}

	public void TestGetChargesAtOrigin()
	{
		var (auCompany1, auBranch1) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		var (auCompany2, auBranch2) = CreateGlobalCompanyAndBranch("AU", "AU Branch 2");
		var (auCompany3, auBranch3) = CreateGlobalCompanyAndBranch("AU", "AU Branch 3");
		var (auCompany4, auBranch4) = CreateGlobalCompanyAndBranch("AU", "AU Branch 4");
		var (auCompany5, auBranch5) = CreateGlobalCompanyAndBranch("AU", "AU Branch 5");
		var (usCompany1, usBranch1) = CreateGlobalCompanyAndBranch("US", "US Branch 1");
		var (sgCompany1, sgBranch1) = CreateGlobalCompanyAndBranch("SG", "SG Branch 1");
		var (sgCompany2, sgBranch2) = CreateGlobalCompanyAndBranch("SG", "SG Branch 2");
		var (sgCompany3, sgBranch3) = CreateGlobalCompanyAndBranch("SG", "SG Branch 3");

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignor = true;

		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignee = true;

		var consignorIFTOrg = sgCompany1.OrgProxy;
		var consignorIFTrelatedParty = consignor.AllRelatedParties.AddNew();
		consignorIFTrelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
		consignorIFTrelatedParty.PR_OH_RelatedParty = consignorIFTOrg.PK;
		consignorIFTrelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
		consignorIFTrelatedParty.PR_FreightTransportMode = Core.Constants.TransportModes.All;

		var consignorIFTOrg2 = sgCompany2.OrgProxy;
		var consignorIFTrelatedParty2 = consignor.AllRelatedParties.AddNew();
		consignorIFTrelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
		consignorIFTrelatedParty2.PR_OH_RelatedParty = consignorIFTOrg2.PK;
		consignorIFTrelatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
		consignorIFTrelatedParty2.PR_FreightTransportMode = Core.Constants.TransportModes.Air;

		var consignorIFTOrg3 = sgCompany3.OrgProxy;
		var consignorIFTrelatedParty3 = consignor.AllRelatedParties.AddNew();
		consignorIFTrelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
		consignorIFTrelatedParty3.PR_OH_RelatedParty = consignorIFTOrg3.PK;
		consignorIFTrelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
		consignorIFTrelatedParty3.PR_FreightTransportMode = Core.Constants.TransportModes.All;

		var consignorTpOrg = Factory.NewWithValidTestData<OrgHeader>();
		consignorTpOrg.OH_IsConsignor = true;

		Factory.Save();

		ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.ControllingCustomerNameOrPK = auCompany1.OrgProxy.PK.ToString();

			Factory.Save();

			return shipment;
		}

		TestCase(() =>
		{
			// The shipment is AU based
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Factory.Save();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges but with zero sell amount
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				AddPerUnitCharge(header, "FRT", 0, "KG", sellRate: 5);
			});

			// 4th AU branch has job with charges
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 10);
			});

			// 5th AU branch also has job with charges
			SetupJobHeaderInBranch(auBranch5, shipment, header =>
			{
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 20);
			});

			// US branch also has job with charges for the requested consignor
			SetupJobHeaderInBranch(usBranch1, shipment, header =>
			{
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 30);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);
				actualCharges.Should().HaveCount(0, "because none of the companies meet the criteria of an Export Company");
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = shipment.Consignor.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 4th AU branch has job with charges
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = consignorIFTOrg.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 120, sellAccount: consignorIFTOrg);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: usCompany1.OrgProxy);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 4th branch has FRT Sell Amount > 0 and Debtor = Related IFT of Consignor", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 120).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 50).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = auCompany4.OrgProxy.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 4th AU branch has job with charges
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = auCompany1.OrgProxy.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 120, sellAccount: auCompany1.OrgProxy);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: usCompany1.OrgProxy);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 4th branch has FRT Sell Amount > 0 and Debtor = Controlling Customer even it's an Organization Proxy", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 120).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 50).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = auCompany4.OrgProxy.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 4th AU branch has job with charges
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = auCompany1.OrgProxy.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 120, sellAccount: auCompany1.OrgProxy);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: usCompany1.OrgProxy);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 4th branch has FRT Sell Amount > 0 and Debtor = Controlling Customer even it's an Organization Proxy", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 120).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 50).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();
			shipment.ConsignorPK = auCompany5.OrgProxy.PK;

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = shipment.Consignor.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
				AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 80, sellAccount: shipment.Consignor);
			});

			// 4th AU branch has job with charges
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = consignorIFTOrg.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 120, sellAccount: consignorIFTOrg);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: usCompany1.OrgProxy);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 4th branch has job that contain charge with Sell Amount > 0 and Debtor = IFT related to Consignor, which takes priority over Consignor", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 120).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 50).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = shipment.Consignor.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 4th AU branch has job with charges
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = shipment.Consignor.PK;
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: usCompany1.OrgProxy);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);
				actualCharges.Should().HaveCount(0, "because none of the companies meet the criteria of an Export Company");
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges to third party consignor
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = consignorTpOrg.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: consignorTpOrg);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: consignorTpOrg);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 3rd branch contain charge with Sell Amount > 0 and Debtor = Third Party Non-OrgProxy Consignor", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 100).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 50).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges billed to an org proxy
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = auCompany4.OrgProxy.PK;
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 4th AU branch has job with charges billed to overseas agent
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = consignorIFTOrg.PK;
				header.AgentCollectPK = usCompany1.GC_OH_OrgProxy;
				AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 310, sellAccount: header.AgentCollect);
				AddPerUnitCharge(header, "DADF", 1, "KG", sellRate: 330, sellAccount: header.AgentCollect);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 4th branch has Sell Amount > 0 and Debtor = Overseas Agent", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 310).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 330).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();
			var agentCollect1 = Factory.NewWithValidTestData<OrgHeader>();
			var agentCollect2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			// 2nd AU branch has job but without charges
			SetupJobHeaderInBranch(auBranch2, shipment, header => { });

			// 3rd AU branch has job with charges billed to overseas agent
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = auCompany4.OrgProxy.PK;
				header.AgentCollectPK = agentCollect1.PK;
				header.Branch.GB_RL_NKHomePort = "AUMEL";
				header.Branch.GB_RL_NKHomePort.Should().NotBe(shipment.JS_RL_NKOrigin, "precondition");
				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: header.AgentCollect);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 110, sellAccount: header.AgentCollect);
			});

			// 4th AU branch has job with charges billed to overseas agent
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = consignorIFTOrg.PK;
				header.AgentCollectPK = agentCollect2.PK;
				header.Branch.GB_RL_NKHomePort = shipment.JS_RL_NKOrigin;
				AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 310, sellAccount: header.AgentCollect);
				AddPerUnitCharge(header, "DADF", 1, "KG", sellRate: 330, sellAccount: header.AgentCollect);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);

				CombineAssertions("because 4th branch home port is equal to shipment's origin", () =>
				{
					actualCharges.FirstOrDefault(c => c.SellAmount == 310).Should().NotBeNull();
					actualCharges.FirstOrDefault(c => c.SellAmount == 330).Should().NotBeNull();
				});
			}
		});

		TestCase(() =>
		{
			var shipment = GetNewShipment();

			// AU branch has job with charges to the least specific IFT related party
			SetupJobHeaderInBranch(auBranch2, shipment, header =>
			{
				header.LocalChargesPK = consignor.PK;
				AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 300, sellAccount: consignorIFTOrg3);
			});

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var actualCharges = GetJobChargesFromOrigin(shipment.PK);
				actualCharges.FirstOrDefault(c => c.SellAmount == 300).Should().NotBeNull("Charge of 3rd IFT Debtor should be included.");
			}
		});

		Assert(true);
	}

	[ExpectNoExceptions]
	public void TestGetChargesAtOrigin_NullSellAccount()
	{
		// Arrange
		var (auCompany1, auBranch1) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		var (auCompany2, auBranch2) = CreateGlobalCompanyAndBranch("AU", "AU Branch 2");
		var (auCompany3, auBranch3) = CreateGlobalCompanyAndBranch("AU", "AU Branch 3");

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignor = true;
		Factory.Save();

		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_RL_NKOrigin = "AUSYD";
		shipment.JS_RL_NKDestination = "USLAX";
		shipment.ConsignorPK = consignor.PK;
		Factory.Save();

		// Branch 2: Job with a charge that has a null SellAccount
		SetupJobHeaderInBranch(auBranch2, shipment, header =>
		{
			AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 150, sellAccount: null);
			AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 15, sellAccount: consignor);
		});

		// Branch 3: Job with a charge that has a null SellAccount
		SetupJobHeaderInBranch(auBranch3, shipment, header =>
		{
			AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 250, sellAccount: null);
			AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 25, sellAccount: consignor);
		});

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			var charges = GetJobChargesFromOrigin(shipment.PK);
			charges.Should().HaveCountGreaterThan(1);
		}
	}

	public void TestGetCharges()
	{
		var (auCompany1, auBranch1) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		var (auCompany2, auBranch2) = CreateGlobalCompanyAndBranch("AU", "AU Branch 2");
		var (auCompany3, auBranch3) = CreateGlobalCompanyAndBranch("AU", "AU Branch 3");
		var (usCompany1, usBranch1) = CreateGlobalCompanyAndBranch("US", "US Branch 1");

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignor = true;

		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignee = true;

		Factory.Save();

		ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			Factory.Save();

			return shipment;
		}

		var shipment = GetNewShipment();

		// 2nd AU branch has job with charges but with zero sell amount
		SetupJobHeaderInBranch(auBranch2, shipment, header => {
			header.LocalChargesPK = consignor.PK;
			AddPerUnitCharge(header, "FRT", 0, "KG", sellRate: 100, sellAccount: consignor);
		});

		// 3rd AU branch has job with charges but debtor is an org proxy
		SetupJobHeaderInBranch(auBranch3, shipment, header => {
			header.LocalChargesPK = auCompany2.GC_OH_OrgProxy;
			AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 90, sellAccount: header.LocalCharges);
		});

		// US branch has job with charges
		SetupJobHeaderInBranch(usBranch1, shipment, header =>
		{
			header.LocalChargesPK = consignor.PK;
			AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: consignor);
			AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: consignor);
		});

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch1.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			var actualCharges = GetCharges(shipment.PK);

			CombineAssertions("because US (destination) branch contain charge with Sell Amount > 0 and Debtor = Local Client / Consignor", () =>
			{
				actualCharges.FirstOrDefault(c => c.SellAmount == 100).Should().NotBeNull();
				actualCharges.FirstOrDefault(c => c.SellAmount == 50).Should().NotBeNull();
			});
		}

		Assert(true);
	}

	#endregion

	public void TestGetChargesAtDestination_WhenAutoratingForDestination_ShowsMessage()
	{
		var shipment = Factory.New<ForwardingShipment>();

		var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		auCompany.GC_Code = "ABC";

		Factory.Save();

		var messageSeen = string.Empty;
		shipment.OnShowMessageOnGUI += (s, e) =>
		{
			messageSeen = e.Message;
		};

		var houseBillCharges = new HouseBillCharges(shipment);
		_ = houseBillCharges.AutoRateOverseasJob(auCompany);

		var expectedMessage = @"Charges intended to be billed by Destination Company ABC have not yet been created at Destination.
	Autorating Revenue is run for the Destination Company ABC with 0 Charges found and printed as Collect Charges. (0 is the number of Charges found.)

	This process will not create Job Header nor charges under the Destination Company.

	Shipment S00001000 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.";

		messageSeen.Should().StartWith(expectedMessage);

		Assert(true);
	}

	#region Performance Test

	[DeveloperOnlyTest]
	public void TestGetCharges_Performance()
	{
		var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch Default");

		var shipments = SetupPerformanceTestData(10);

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			var (chargeTimes, jobTimes) = RunJobCharges(shipments);

			CombineAssertions(() =>
			{
				AssertRunJobChargesTimes(jobTimes, 90d, $"Called HouseBillCharges.JobHeaderAtOrigin for {shipments.Count} shipments");
				AssertRunJobChargesTimes(chargeTimes, 60d, $"Called HouseBillCharges.GetCharges for {shipments.Count} shipments");
			});
		}
	}

	public void TestGetCharges_DBHits()
	{
		var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch Default");

		var shipments = SetupPerformanceTestData(1);

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			var newFactory = new BusinessObjectFactory();
			var shipmentInANewFactory = newFactory.Load<ForwardingShipment>(shipments[0].PK);
			var houseBillCharges = new HouseBillCharges(shipmentInANewFactory);
			newFactory.ResetDatabaseLoadCount();
			var job = houseBillCharges.JobHeaderAtOrigin;
			job.Should().NotBeNull("precondition");
			var charges = houseBillCharges.GetCharges();
			charges.Should().HaveCountGreaterThan(0, "precondition");

			AssertMaxDbHits(11, newFactory);
			// AccTransactionLines: 2
			// JobCharge: 1
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// OrgRelatedParty: 2
			// GlbBranch: 1
			// GlbCompany: 2
		}
	}

	[DeveloperOnlyTest]
	public void TestGetChargesAtPortShouldNotErrorReportOnContextSwitching()
	{
		var (auCompany, auBranch) = CreateGlobalCompanyAndBranch("AU", "AU Branch Default");

		var shipments = SetupPerformanceTestData(10);

		using (Env.SetTemporaryUserContext("CW1Service", auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			AssertNoExceptionThrown(() => RunJobCharges(shipments));
		}
	}

	List<ForwardingShipment> SetupPerformanceTestData(int numberOfShipments = 1)
	{
		var (auCompany1, auBranch1) = CreateGlobalCompanyAndBranch("AU", "AU Branch 1");
		var (auCompany2, auBranch2) = CreateGlobalCompanyAndBranch("AU", "AU Branch 2");
		var (auCompany3, auBranch3) = CreateGlobalCompanyAndBranch("AU", "AU Branch 3");
		var (auCompany4, auBranch4) = CreateGlobalCompanyAndBranch("AU", "AU Branch 4");
		var (auCompany5, auBranch5) = CreateGlobalCompanyAndBranch("AU", "AU Branch 5");
		var (usCompany1, usBranch1) = CreateGlobalCompanyAndBranch("US", "US Branch 1");

		var consignor = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignor = true;

		var consignee = Factory.NewWithValidTestData<OrgHeader>();
		consignor.OH_IsConsignee = true;

		var consignorIFTOrg = Factory.NewWithValidTestData<OrgHeader>();
		consignorIFTOrg.OH_IsConsignor = true;
		var consignorIFTrelatedParty = consignor.AllRelatedParties.AddNew();
		consignorIFTrelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
		consignorIFTrelatedParty.PR_OH_RelatedParty = consignorIFTOrg.PK;
		consignorIFTrelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

		auCompany5.GC_OH_OrgProxy = consignorIFTOrg.PK;

		var consignorTpOrg = Factory.NewWithValidTestData<OrgHeader>();
		consignorTpOrg.OH_IsConsignor = true;

		Factory.Save();

		ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.ControllingCustomerNameOrPK = auCompany1.OrgProxy.PK.ToString();

			Factory.Save();

			return shipment;
		}

		consignorIFTrelatedParty.PR_FreightDirection.Should().Match<ZString>(v => v == RelatedPartyDirectionList.Codes.Pickup, "Precondition");

		var shipments = Enumerable.Range(0, numberOfShipments)
								.Select(_ => GetNewShipment())
								.ToList();

		foreach (var shipment in shipments)
		{
			// 2nd AU branch has job with charges but none are valid
			SetupJobHeaderInBranch(auBranch2, shipment, header =>
			{
				header.LocalChargesPK = auCompany4.OrgProxy.PK;

				for (var i = 0; i < 20; i++)
				{
					AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 100 + i, sellAccount: auCompany4.OrgProxy);
				}

				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 3rd AU branch has job with charges but none are valid
			SetupJobHeaderInBranch(auBranch3, shipment, header =>
			{
				header.LocalChargesPK = auCompany4.OrgProxy.PK;

				for (var i = 0; i < 20; i++)
				{
					AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 100 + i, sellAccount: auCompany4.OrgProxy);
				}

				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 100, sellAccount: auCompany4.OrgProxy);
			});

			// 4th AU branch has job with charges with one of them valid
			SetupJobHeaderInBranch(auBranch4, shipment, header =>
			{
				header.LocalChargesPK = consignorIFTOrg.PK;

				for (var i = 0; i < 20; i++)
				{
					AddPerUnitCharge(header, "ODOC", 1, "KG", sellRate: 100 + i, sellAccount: auCompany3.OrgProxy);
				}

				AddPerUnitCharge(header, "FRT", 1, "KG", sellRate: 120, sellAccount: consignorIFTOrg);
				AddPerUnitCharge(header, "DDOC", 1, "KG", sellRate: 50, sellAccount: usCompany1.OrgProxy);
			});
		}

		return shipments;
	}

	void AssertRunJobChargesTimes(TimeSpan[] times, double baselineInMs, string message)
	{
		var elapsedTimesInMs = string.Join(", ", times.Select(t => t.TotalMilliseconds));
		var sanitisedTimes = RemoveOutliers(times);
		var elapsedSanitisedTimesInMs = string.Join(", ", sanitisedTimes.Select(t => t.TotalMilliseconds));
		var sanitisedAverageInMs = sanitisedTimes.Average(t => t.TotalMilliseconds);

		var averageInMs = times.Average(t => t.TotalMilliseconds);
		const double allowedVariation = 2d;

		var durationMessage = @$"{message}.
Elapsed (in ms) per shipment: {elapsedTimesInMs}
Sanitized times (in ms): {elapsedSanitisedTimesInMs}
Average: {sanitisedAverageInMs}ms

The average time exceeded significantly the baseline of {baselineInMs}ms created on {System.Environment.MachineName}.
Please make sure that your changes in code (if any) did not contribute to making this method slower.
If this is a false positive then please disregard and carry on :) 

Note: This is DEVELOPER ONLY test and won't fail on DAT.";
		Assert(durationMessage, sanitisedAverageInMs < baselineInMs * allowedVariation);
	}

	(TimeSpan[] JobTimes, TimeSpan[] ChargeTimes) RunJobCharges(List<ForwardingShipment> shipments)
	{
		var chargeTimes = new List<TimeSpan>(shipments.Count);
		var jobTimes = new List<TimeSpan>(shipments.Count);

		foreach (var shipment in shipments)
		{
			var newFactory = new BusinessObjectFactory();
			var shipmentInANewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var houseBillCharges = new HouseBillCharges(shipmentInANewFactory);

			var stopwatch = Stopwatch.StartNew();
			var job = houseBillCharges.JobHeaderAtOrigin;
			job.Should().NotBeNull("precondition");
			stopwatch.Stop();
			jobTimes.Add(stopwatch.Elapsed);

			stopwatch = Stopwatch.StartNew();
			var actualCharges = GetCharges(shipment.PK);
			actualCharges.Should().HaveCountGreaterThan(0, "precondition");
			stopwatch.Stop();
			chargeTimes.Add(stopwatch.Elapsed);
		}

		return (jobTimes.ToArray(), chargeTimes.ToArray());
	}

	TimeSpan[] RemoveOutliers(TimeSpan[] times)
	{
		if (times.Length == 0)
		{
			return Array.Empty<TimeSpan>();
		}

		var meanValue = times.Average(t => t.TotalMilliseconds);
		var orderedTimes = times.OrderByDescending(t => Math.Abs(t.TotalMilliseconds - meanValue)).ToArray();
		var objectFarthestFromMean = orderedTimes.First();

		// remove time if is 1.2 times further from the mean
		var maxRange = 1.2d * meanValue;

		if (Math.Abs(objectFarthestFromMean.TotalMilliseconds - meanValue) > maxRange)
		{
			var timesWithoutOutlier = times.Skip(1).ToArray();

			return RemoveOutliers(timesWithoutOutlier);
		}

		return times;
	}

	#endregion

	#region Helpers

	void SetupJobHeaderInBranch(GlbBranch branch, ForwardingShipment shipment, Action<Job> configureJobHeader)
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			var newFactory = new BusinessObjectFactory();
			var shipmentInANewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var header = (Job)new JobHeader.Loader(shipmentInANewFactory).TryLoadOrCreateWithMutex();
			header.JH_GE = Env.CurrentDepartmentPK;
			configureJobHeader(header);
			newFactory.Save();
		}
	}

	(GlbCompany, GlbBranch) CreateGlobalCompanyAndBranch(string countryCode, string branchName)
	{
		var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
		orgProxy.OH_IsConsignor = true;

		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = countryCode;
		company.GC_OH_OrgProxy = orgProxy.PK;

		var branch = Factory.NewWithValidTestData<GlbBranch>();
		branch.GB_RN_NKCountryCode = countryCode;
		branch.GB_GC = company.PK;
		branch.GB_BranchName = branchName;

		return (company, branch);
	}

	Charge AddPerUnitCharge(Job job, string chargeCode, decimal chargeable, string unit = "KG", decimal costRate = 0, decimal sellRate = 0, string containerType = null, OrgHeader sellAccount = null)
	{
		var chargeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
		chargeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

		var charge = job.Charges.AddNew();
		charge.JR_AC = Factory.LoadTop1<AccChargeCode>(chargeQuery).PK;

		if (!string.IsNullOrEmpty(containerType))
		{
			var attr = charge.JobChargeAttributes.AddNew();
			attr.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
			attr.EC_Value = containerType;
		}

		if (costRate > 0)
		{
			charge.JR_OSCostAmt = costRate * chargeable;

			var basis = charge.PaymentBases.AddNew();
			basis.PBS_ChargeableAmount = chargeable;
			basis.PBS_ChargeableUnit = unit;
			basis.PBS_PerUnitRate = costRate;
			basis.PBS_RateUnit = unit;
			basis.PBS_RX_NKRateCurrency = "AUD";
			basis.PBS_IsCost = true;
		}

		if (sellRate > 0)
		{
			charge.JR_OSSellAmt = sellRate * chargeable;
			charge.JR_OH_SellAccount = sellAccount?.PK ?? ZGuid.Empty;

			var basis = charge.PaymentBases.AddNew();
			basis.PBS_ChargeableAmount = chargeable;
			basis.PBS_ChargeableUnit = unit;
			basis.PBS_PerUnitRate = sellRate;
			basis.PBS_RateUnit = unit;
			basis.PBS_RX_NKRateCurrency = "AUD";
		}

		return charge;
	}

	JobCharge[] GetCharges(ZGuid shipmentPK)
	{
		var newFactory = new BusinessObjectFactory();
		var shipmentInANewFactory = newFactory.Load<ForwardingShipment>(shipmentPK);
		var houseBillCharges = new HouseBillCharges(shipmentInANewFactory);
		return houseBillCharges.GetCharges();
	}

	JobCharge[] GetJobChargesFromOrigin(ZGuid shipmentPK)
	{
		var newFactory = new BusinessObjectFactory();
		var shipmentInANewFactory = newFactory.Load<ForwardingShipment>(shipmentPK);
		var houseBillCharges = new HouseBillCharges(shipmentInANewFactory);
		return houseBillCharges.GetChargesAtOrigin();
	}

	void TestCase(Action testLogic)
	{
		testLogic();
	}

	#endregion
}
