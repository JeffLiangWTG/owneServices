using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class NewShipmentExportAWBHeaderTest : NewExportAWBHeaderTest<ShipmentExportAWBHeader>
	{
		public void TestShippersSignature_AU()
		{
			var oldName = GlbStaff.CurrentUser.GS_FullName;

			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
				{
					var certificate = Factory.New<GenRegCertAccredMaintList>();
					certificate.XZ_Type = "DGN";
					certificate.XZ_Comment = "Test DGN Number";
					certificate.XZ_ParentID = GlbStaff.CurrentUser.PK;
					certificate.XZ_ParentTableCode = "GS";
					certificate.XZ_RefNumber = "AU123456789012345678";

					Factory.Save();

					AssertShippersSignature("If there are more than 2 words in the Full Name field try to fit all words in to the max length char space.", "John Smith", "John Smith", "John Smith AU123456789012345678");
					AssertShippersSignature("If all words in the full Name field do not fit, omit middle names.", "Sally Gomez Louise Jones", "Sally Jones", "Sally Gomez Louise Jones AU12345678");
					AssertShippersSignature("If first name and last name still do not fit in full, initial the first name.", "Sam Jucy Simson-Brown", "S Simson-Brown", "Sam Jucy Simson-Brown AU12345678901");
					AssertShippersSignature("If initial of the first name and last name still do not fit in full, use the first name.", "Sandra Daisy Simpson-Brown-Ahsa", "Sandra", "Sandra Daisy Simpson-Brown-Ahsa AU1");
					AssertShippersSignature("If the first name still do not fit in full, use the initial of the first name.", "RedWackyLeagueAntlezBroketheStereoNeonTideBring", "R", "RedWackyLeagueAntlezBroketheStereoN");
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_FullName = oldName;
			}
		}

		void AssertShippersSignature(string message, string userName, string expectedUserNameForAU, string expectedUserNameWithDGForOtherCountries)
		{
			GlbStaff.CurrentUser.GS_FullName = userName;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				AWBHeader.Populate();
				AssertEquals("SG: " + message, expectedUserNameWithDGForOtherCountries, AWBHeader.EH_ShippersSignature.ToString());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AWBHeader.Populate();
				AssertEquals("AU: " + message, GlbStaff.CurrentUser.DangerousGoodsCertificateNumber + " " + expectedUserNameForAU, AWBHeader.EH_ShippersSignature.ToString());
			}
		}

		public void TestIssuedByMatchesByRelatedPort()
		{
			Func<string, string, OrgHeader> newOrgHeaderWithAddress = (name, address) =>
			{
				OrgHeader result = Factory.New<OrgHeader>();
				result.OH_Code = name;
				result.OH_FullName = name;
				result.MainAddress.OA_Address1 = address;
				return result;
			};

			OrgHeader companyOrg = newOrgHeaderWithAddress("COMPANY ORG", "123 COMPANY ST, COMPANYLAND");
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrg.PK;
			OrgHeader branchOrg = newOrgHeaderWithAddress("BRANCH ORG", "123 BRANCH ST, BRANCHVILLE");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrg.PK;
			Factory.Save();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USCHI";
			var port = GlbBranch.CurrentBranch.ExtraPorts.AddNew();
			port.GY_RL_NKAdditionalBranchRelatedPort = "USLAX";

			AWBHeader.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Action<string, string, OrgHeader> assertChangingPortsReturnsOrg = (message, newPort, expectedOrg) =>
			{
				AWBHeader.Consol.JK_RL_NKLoadPort = newPort;
				AWBHeader.Shipment.JS_RL_NKOrigin = newPort;
				AWBHeader.Populate();
				AssertEquals(message, expectedOrg.OH_FullName, AWBHeader.EH_IssuingAgentName);
				AssertEquals(message, expectedOrg.MainAddress.OA_Address1, AWBHeader.EH_IssuingAgentAddress1);
			};

			assertChangingPortsReturnsOrg("Branch's home port provided as load/origin, Branch's OrgProxy should populate bill", "USCHI", branchOrg);
			assertChangingPortsReturnsOrg("Branch's related port provided as load/origin, Branch's OrgProxy should populate bill", "USLAX", branchOrg);
			assertChangingPortsReturnsOrg("Other port provided as load/origin, Company's OrgProxy should populate bill", "AUBNE", companyOrg);
		}

		public void TestItalyCodes()
		{
			var accountingInformations = AWBHeader.AWBAccountingInformations.Cast<ExportAWBAccountingInformation>();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";
			OrgHeader branchProxy = GlbCompany.CurrentCompany.FirstBranchForUnLoco(AWBHeader.Shipment.Origin).OrgProxy;

			AWBHeader.Populate();
			Assert(AWBHeader.AWBAccountingInformations.Count == 0);

			var ivaCC = branchProxy.CustomsCodes.AddNew();
			ivaCC.OK_RN_NKCodeCountry = "IT";
			ivaCC.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			ivaCC.OK_CustomsRegNo = "10987654321";

			AWBHeader.Populate();
			Assert(AWBHeader.AWBAccountingInformations.Count == 1);

			AssertNotNull(accountingInformations.FirstOrDefault(info =>
																info.IsItalianRegistrationCode &&
																info.EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA &&
																info.EA_Information == "10987654321"));

			OrgHeader consignor = Factory.New<OrgHeader>();
			AWBHeader.Shipment.ConsignorPK = consignor.PK;

			var consignorSIV = consignor.CustomsCodes.AddNew();
			consignorSIV.OK_RN_NKCodeCountry = "IT";
			consignorSIV.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			consignorSIV.OK_CustomsRegNo = "11223344551";

			var consignorIVA = consignor.CustomsCodes.AddNew();
			consignorIVA.OK_RN_NKCodeCountry = "IT";
			consignorIVA.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			consignorIVA.OK_CustomsRegNo = "55667788990";

			AWBHeader.Populate();
			Assert(AWBHeader.AWBAccountingInformations.Count == 2);

			AssertNotNull(accountingInformations.FirstOrDefault(info =>
																info.IsItalianRegistrationCode &&
																info.EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA &&
																info.EA_Information == "10987654321"));

			AssertNotNull(accountingInformations.FirstOrDefault(info =>
																info.IsItalianRegistrationCode &&
																info.EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA &&
																info.EA_Information == "11223344551"));

			consignor.CustomsCodes.RemoveAndDeleteAll();
			consignorIVA = consignor.CustomsCodes.AddNew();
			consignorIVA.OK_RN_NKCodeCountry = "IT";
			consignorIVA.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			consignorIVA.OK_CustomsRegNo = "55667788990";

			AWBHeader.Populate();
			Assert(AWBHeader.AWBAccountingInformations.Count == 2);

			AssertNotNull(accountingInformations.FirstOrDefault(info =>
																info.IsItalianRegistrationCode &&
																info.EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA &&
																info.EA_Information == "10987654321"));

			AssertNotNull(accountingInformations.FirstOrDefault(info =>
																info.IsItalianRegistrationCode &&
																info.EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA &&
																info.EA_Information == "55667788990"));
		}

		public void TestThrowReplaceMacrosException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"{First}\" == \"1\").First()>"))
			{
				AssertExceptionThrown<ExportAWBHeaderReplaceMacrosException>("Exception throw", $@"HAWB cannot be generated.
Please correct macro format in Registry -> {FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.HumanReadableRegistryPath()}.", () => shipment.AWBHeader.Populate());
			}
		}

		#region OtherCharges

		protected override BooleanRegistryItem GroupOtherChargesByIataCodeRegistry
		{
			get { return ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode; }
		}

		public override void TestIncludeCharges()
		{
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { AWBHeader.Shipment.Origin.Country.PK.ToGuid() });

			CreateShipmentHeader(AWBHeader.Shipment);

			JobCharge charge1 = CreateShipmentCharge(AWBHeader.Shipment, null, null, 200, 445, AWBHeader.Shipment.Job.LocalCharges);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { charge1 }, GetShipmentCharges(AWBHeader.Shipment));

			AWBHeader.Populate();
			AssertEquals("ignores charge with no AccChargeCode", 0, AWBHeader.AWBOtherCharges.Count);

			JobCharge charge2 = CreateShipmentCharge(AWBHeader.Shipment, null, Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode), 500, 450, AWBHeader.Shipment.Job.LocalCharges);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { charge1, charge2 }, GetShipmentCharges(AWBHeader.Shipment));

			AWBHeader.Populate();
			AssertEquals("ignores charge that is FreightChargeCode", 0, AWBHeader.AWBOtherCharges.Count);

			JobCharge charge3 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 1000, 988, AWBHeader.Shipment.Job.LocalCharges);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { charge1, charge2, charge3 }, GetShipmentCharges(AWBHeader.Shipment));

			AWBHeader.Populate();
			AssertEquals("processes correct charge", 1, AWBHeader.AWBOtherCharges.Count);
			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals("ignores charge set to hide", 0, AWBHeader.AWBOtherCharges.Count);

			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("processes correct charge", 1, AWBHeader.AWBOtherCharges.Count);
			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();

			charge3.JR_LocalSellAmt = 0;

			AWBHeader.Populate();
			AssertEquals("ignores charge with 0 amount", 0, AWBHeader.AWBOtherCharges.Count);
		}

		public override void TestSplitCharges()
		{
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { AWBHeader.Shipment.Origin.Country.PK.ToGuid() });

			var header = CreateShipmentHeader(AWBHeader.Shipment);
			header.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			header.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			JobCharge charge1 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 1000, 920, header.LocalCharges);
			JobCharge charge2 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 250, 200, header.LocalCharges);

			AssertContainsExactElementsInAnyOrder(new[] { charge1, charge2 }, GetShipmentCharges(AWBHeader.Shipment));

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			collection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = Core.Constants.AWB.EntitlementCode.Agent;
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("AS and AC show total charged", 2, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AC|A|PPD|250"
			},
			GetChargesDescriptions());

			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Split;
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("AS splits cost and profit and AC shows total charged", 3, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|920", "AS|A|PPD|80", "AC|A|PPD|250"
			},
			GetChargesDescriptions());

			charge1.JR_LocalCostAmt = 1000;
			AssertEquals("profit on AS charge", 0m, charge1.JR_LocalSellAmt - charge1.JR_LocalCostAmt);

			AWBHeader.Populate();
			AssertEquals("AS ignores profit because there's non and AC shows total charged", 2, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AC|A|PPD|250"
			},
			GetChargesDescriptions());

			charge1.JR_LocalCostAmt = 1400;
			AssertEquals("loss on AS charge", -400m, charge1.JR_LocalSellAmt - charge1.JR_LocalCostAmt);

			AWBHeader.Populate();
			AssertEquals("AS shows loss and cost and AC shows total charged", 3, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1400", "AS|A|PPD|-400", "AC|A|PPD|250"
			},
			GetChargesDescriptions());
		}

		public override void TestGroupExcessiveCharges()
		{
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { AWBHeader.Shipment.Origin.Country.PK.ToGuid() });

			GroupOtherChargesByIataCodeRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals("prerequisite", 15, AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB);

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);

			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Agent;
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Show);

			collection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			collection[Core.Constants.AWB.ChargeCodes.AC].Visibility = nameof(AWBDisplayOptionVisibility.Show);

			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			CreateShipmentHeader(AWBHeader.Shipment);
			AssertGroupExcessiveChargesNotEnoughToGroup();

			ResetHeaderAndParent();

			CreateShipmentHeader(AWBHeader.Shipment);
			AssertGroupExcessiveChargesExactlyMaxAllowed();

			ResetHeaderAndParent();

			CreateShipmentHeader(AWBHeader.Shipment);
			AssertGroupExcessiveChargesTwoGroups();

			ResetHeaderAndParent();

			CreateShipmentHeader(AWBHeader.Shipment);
			AssertGroupExcessiveChargesFourGroups();
		}

		public override void TestMaxOtherChargesOnGroupExcessiveCharges()
		{
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { AWBHeader.Shipment.Origin.Country.PK.ToGuid() });

			CreateShipmentHeader(AWBHeader.Shipment);
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.AC), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.AS), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.AT), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.AW), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.BF), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.BI), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.BM), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.BR), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CA), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CB), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CC), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CD), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CF), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CG), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CH), 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(Core.Constants.AWB.ChargeCodes.CI), 10, 0, AWBHeader.Shipment.Job.LocalCharges);

			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;
			AWBHeader.Populate();

			AssertEquals(15, AWBHeader.AWBOtherCharges.Count);

			CreateShipmentCharge(AWBHeader.Shipment, null, CreateChargeCode(string.Empty), 10, 0);
			AWBHeader.Populate();

			AssertEquals(10, AWBHeader.AWBOtherCharges.Count);

			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = false;
			AWBHeader.Populate();

			AssertEquals(10, AWBHeader.AWBOtherCharges.Count);
		}

		void AssertGroupExcessiveChargesNotEnoughToGroup()
		{
			AWBHeader.Shipment.Job.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AWBHeader.Shipment.Job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			for (int i = 0; i < 6; i++)
			{
				CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			}

			AWBHeader.Populate();

			AssertEquals(6, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10"
			},
			GetChargesDescriptions());
		}

		void AssertGroupExcessiveChargesExactlyMaxAllowed()
		{
			AWBHeader.Shipment.Job.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AWBHeader.Shipment.Job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			for (int i = 0; i < 10; i++)
			{
				CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			}

			AWBHeader.Populate();

			AssertEquals(10, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10"
			},
			GetChargesDescriptions());
		}

		void AssertGroupExcessiveChargesTwoGroups()
		{
			AWBHeader.Shipment.Job.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AWBHeader.Shipment.Job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			for (int i = 0; i < 6; i++)
			{
				CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 10, 0, AWBHeader.Shipment.Job.LocalCharges);
				CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 10, 0, AWBHeader.Shipment.Job.LocalCharges);
			}

			AWBHeader.Populate();

			AssertEquals(10, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "MB|A|PPD|30", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10"
			},
			GetChargesDescriptions());
		}

		public void TestOtherChargesPrepaidCollect_WithoutIncoterms()
		{
			AssertOtherChargesPrepaidCollect(ZString.Empty, new[] { "COL", "PPD" });
		}

		public void TestOtherChargesPrepaidCollect_WithIncoterms_PPD()
		{
			AssertOtherChargesPrepaidCollect(IncoTerms.DeliveredDutyPaid, new[] { "COL", "PPD" });
		}

		void AssertOtherChargesPrepaidCollect(string incoTerm, string[] expectedPPDCLT)
		{
			AssertEquals("Precondition", "PPD", IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, IncoTerms.DeliveredDutyPaid));
			AssertEquals("Precondition", "CCX", IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, IncoTerms.ExWorks));

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = CreateShipmentHeader(AWBHeader.Shipment);
			jobHeader.LocalChargesPK = localClient.PK;
			jobHeader.AgentCollectPK = overseasAgent.PK;
			AWBHeader.Shipment.JS_INCO = incoTerm;

			var charge1 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 1000, 920, overseasAgent);
			var charge2 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 250, 200, localClient);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { charge1, charge2 }, GetShipmentCharges(AWBHeader.Shipment));

			var collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			collection[Constants.AWB.ChargeCodes.AS].Entitlement = Constants.AWB.EntitlementCode.Agent;
			collection[Constants.AWB.ChargeCodes.AC].Entitlement = Constants.AWB.EntitlementCode.Agent;
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();

			AssertEquals("Expected two other charges to be present", 2, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"AS|A|{expectedPPDCLT[0]}|1000",
				$"AC|A|{expectedPPDCLT[1]}|250"
			}, GetChargesDescriptions());
		}

		public void TestOtherChargesPrepaidCollect_WithIncoterms_DDP_ThirdPartyDebtor()
		{
			AssertOtherChargesPrepaidCollectBasedOnIncoTerms(IncoTerms.DeliveredDutyPaid, "PPD");
		}

		public void TestOtherChargesPrepaidCollect_WithoutIncoterms_DDP_ThirdPartyDebtor()
		{
			AssertOtherChargesPrepaidCollectBasedOnIncoTerms(String.Empty, "COL");
		}

		public void TestOtherChargesPrepaidCollect_NoRegistryItem_DDP_ThirdPartyDebtor()
		{
			SetUpShipmentWithCharges(IncoTerms.DeliveredDutyPaid);
			RegistryFactory.Instance.ClearCachedValue<IncoTermChargeCodesCollection>("IncoTermList");

			var incoTermDefinitions = new IncoTermChargeCodesCollection();
			var def = incoTermDefinitions.AddNew();
			def.Origin = PaymentParty.Consignee;
			def.Loading = PaymentParty.Consignee;
			def.Freight = PaymentParty.Consignee;
			def.Insurance = PaymentParty.Consignee;
			def.Unloading = PaymentParty.Consignee;
			def.Destination = PaymentParty.Consignee;
			def.Brokerage = PaymentParty.Consignee;
			def.CustomsDuty = PaymentParty.Consignee;
			def.OriginBrokerage = PaymentParty.Consignee;
			def.IncoTerm = IncoTerms.ExWorks;

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			{
				AssertOtherCharges("COL");
			}	
		}

		public void TestOtherChargesPrepaidCollect_WithIncoterms_EXW_ThirdPartyDebtor()
		{
			AssertOtherChargesPrepaidCollectBasedOnIncoTerms(IncoTerms.ExWorks, "COL");
		}

		void SetUpShipmentWithCharges(string incoTerm)
		{
			AssertEquals("Precondition", "PPD", IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, IncoTerms.DeliveredDutyPaid));
			AssertEquals("Precondition", "CCX", IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, IncoTerms.ExWorks));

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			var thirdPartyDebtor = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = CreateShipmentHeader(AWBHeader.Shipment);
			jobHeader.LocalChargesPK = localClient.PK;
			jobHeader.AgentCollectPK = overseasAgent.PK;
			AWBHeader.Shipment.JS_INCO = incoTerm;

			var charge1 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 1000, 920, thirdPartyDebtor);
			var charge2 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 250, 200, thirdPartyDebtor);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition", new[] { charge1, charge2 }, GetShipmentCharges(AWBHeader.Shipment));
		}

		void AssertOtherCharges(string expectedPPDCLT)
		{
			AWBHeader.Populate();

			AssertEquals("Expected two other charges to be present", 2, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"AS|A|{expectedPPDCLT}|1000",
				$"AC|A|{expectedPPDCLT}|250"
			}, GetChargesDescriptions());
		}	
		void AssertOtherChargesPrepaidCollectBasedOnIncoTerms(string incoTerm, string expectedPPDCLT)
		{
			SetUpShipmentWithCharges(incoTerm);
			AssertOtherCharges(expectedPPDCLT);
		}

		void AssertGroupExcessiveChargesFourGroups()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();

			AWBHeader.Shipment.Job.AgentCollectPK = client.PK;
			AWBHeader.Shipment.Job.LocalChargesPK = debtor.PK;

			for (int i = 0; i < 3; i++)
			{
				JobCharge charge = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 10, 0);
				charge.JR_OH_SellAccount = client.PK;

				charge = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 10, 0);
				charge.JR_OH_SellAccount = client.PK;
			}

			for (int i = 0; i < 3; i++)
			{
				JobCharge charge = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 10, 0);
				charge.JR_OH_SellAccount = debtor.PK;

				charge = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 10, 0);
				charge.JR_OH_SellAccount = debtor.PK;
			}

			AWBHeader.Populate();

			AssertEquals(10, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"MB|A|PPD|30", "AS|A|COL|10", "AS|A|COL|10", "AS|A|COL|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|COL|10", "AC|C|COL|10", "AC|C|COL|10"
			},
			GetChargesDescriptions());
		}

		public override void TestMergeWithExistingOtherCharges()
		{
			using (FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { AWBHeader.Shipment.Origin.Country.PK.ToGuid() }))
			{
				var header = CreateShipmentHeader(AWBHeader.Shipment);
				header.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				header.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				JobCharge charge1 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 1000, 920, header.LocalCharges);
				JobCharge charge2 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAC, 250, 200, header.LocalCharges);
				AssertContainsExactElementsInAnyOrder("prerequisite", new[] { charge1, charge2 }, GetShipmentCharges(AWBHeader.Shipment));

				AWBHeader.Populate();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"AS|A|PPD|1000", "AC|A|PPD|250"
				},
				GetChargesDescriptions());

				Factory.Save();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);

				AWBHeader.Populate();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"AS|A|PPD|1000", "AC|A|PPD|250"
				},
				GetChargesDescriptions());

				JobCharge charge3 = CreateShipmentCharge(AWBHeader.Shipment, null, ChargeCodeAS, 999, 850, header.LocalCharges);
				AssertContainsExactElementsInAnyOrder("prerequisite", new[] { charge1, charge2, charge3 }, GetShipmentCharges(AWBHeader.Shipment));

				AWBHeader.Populate();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"AS|A|PPD|1000", "AC|A|PPD|250", "AS|A|PPD|999"
				},
				GetChargesDescriptions());

				AssertEquals(false, AWBHeader.AWBOtherCharges
					.Cast<ExportAWBOtherCharges>()
					.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "A" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 1000).HasChanges);

				AssertEquals(false, AWBHeader.AWBOtherCharges
					.Cast<ExportAWBOtherCharges>()
					.First(charge => charge.EO_ChargeCode == "AC" && charge.EO_EntitlementCode == "A" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 250).HasChanges);

				AssertEquals(false, AWBHeader.AWBOtherCharges
					.Cast<ExportAWBOtherCharges>()
					.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "A" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 999).HasChanges);

				Factory.Save();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);

				AWBHeader.Populate();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"AS|A|PPD|1000", "AC|A|PPD|250", "AS|A|PPD|999"
				},
				GetChargesDescriptions());

				charge2.Delete();
				AssertContainsExactElementsInAnyOrder("prerequisite", new[] { charge1, charge3 }, GetShipmentCharges(AWBHeader.Shipment));

				AWBHeader.Populate();
				AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"AS|A|PPD|1000", "AS|A|PPD|999"
				},
				GetChargesDescriptions());

				AssertEquals(false, AWBHeader.AWBOtherCharges
					.Cast<ExportAWBOtherCharges>()
					.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "A" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 1000).HasChanges);

				AssertEquals(false, AWBHeader.AWBOtherCharges
					.Cast<ExportAWBOtherCharges>()
					.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "A" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 999).HasChanges);
			}
		}

		#endregion

		protected override BusinessObject GetNewParent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";

			return shipment;
		}

		public void TestAutoCalculationOfTaxOnFreightCharge_IndonesiaOnly()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Indonesia);
			AWBHeader.Shipment.JS_RL_NKOrigin = "IDDPS";
			AWBHeader.Shipment.JS_RL_NKDestination = "USCHI";

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var invoiceJobHeader = CreateShipmentHeader(AWBHeader.Shipment);
			invoiceJobHeader.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoiceJobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";

			Env.Registry.FreightChargeCode = chargeCode.PK.ToGuid();

			var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.SetRateNumerator_ForTestOnly(10);
			rate.AT_Type = "RAT";

			var prepaidFreightCharge1 = CreateShipmentCharge(AWBHeader.Shipment, null, chargeCode, 11m, 0m, invoiceJobHeader.LocalCharges, rate);
			var prepaidFreightCharge2 = CreateShipmentCharge(AWBHeader.Shipment, null, chargeCode, 13m, 0m, invoiceJobHeader.LocalCharges, rate);
			var collectFreightCharge1 = CreateShipmentCharge(AWBHeader.Shipment, null, chargeCode, 33m, 0m, invoiceJobHeader.AgentCollect, rate);
			var collectFreightCharge2 = CreateShipmentCharge(AWBHeader.Shipment, null, chargeCode, 30m, 0m, invoiceJobHeader.AgentCollect, rate);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			prepaidFreightCharge2.JR_JH = jobHeader.PK;
			collectFreightCharge2.JR_JH = jobHeader.PK;
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals(1m, AWBHeader.EH_TaxesPPD);
			AssertEquals(3m, AWBHeader.EH_TaxesCOL);

			prepaidFreightCharge2.JR_JH = invoiceJobHeader.PK;
			collectFreightCharge2.JR_JH = invoiceJobHeader.PK;

			AWBHeader.Populate();

			AssertEquals(2m, AWBHeader.EH_TaxesPPD);
			AssertEquals(6m, AWBHeader.EH_TaxesCOL);
		}
	}
}
