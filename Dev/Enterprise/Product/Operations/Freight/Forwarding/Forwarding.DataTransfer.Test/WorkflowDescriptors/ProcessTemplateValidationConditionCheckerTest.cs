using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing;

sealed class ProcessTemplateValidationConditionCheckerTest : TestCaseWithFactory
{
	#region IsCondition1Met  

	public void TestIsCondition1Met_ForOriginDifferentFromFirstLoad()
	{
		Consol1.Shipments.Add(Shipment);
		Consol2.Shipments.Add(Shipment);

		Consol1.JK_RL_NKLoadPort = "AUSYD";
		Consol1.JK_RL_NKDischargePort = "MYPKG";
		Consol1.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 1, 1);
		Consol1.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 2, 2);
		Consol2.JK_RL_NKLoadPort = "MYPKG";
		Consol2.JK_RL_NKDischargePort = "SGSIN";
		Consol2.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 3, 3);
		Consol2.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 4, 4);

		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));

		Shipment.JS_RL_NKOrigin = "AUSYD";
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_RL_NKOrigin = "AUMEL";
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ForDestinationDifferentFromFinalDischarge()
	{
		Consol1.Shipments.Add(Shipment);
		Consol2.Shipments.Add(Shipment);

		Consol1.JK_RL_NKLoadPort = "AUSYD";
		Consol1.JK_RL_NKDischargePort = "MYPKG";
		Consol1.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 1, 1);
		Consol1.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 2, 2);
		Consol2.JK_RL_NKLoadPort = "MYPKG";
		Consol2.JK_RL_NKDischargePort = "SGSIN";
		Consol2.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 3, 3);
		Consol2.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 4, 4);

		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));

		Shipment.JS_RL_NKDestination = "SGSIN";
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_RL_NKDestination = "USLAX";
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ConsolDischargeLeg()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg));

		Assert(!new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		Consol1.Shipments.Add(Shipment);
		Assert(new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ConsolPreCarriageLeg()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.ConsolPreCarriageLeg));

		Assert(!new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		Consol1.Transports[0].JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
		Consol1.Shipments.Add(Shipment);
		Assert(new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ForHasBrokerageAttached()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached));

		Assert(!new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
		declaration[JobDeclarationSchema.JE_JS] = Shipment.PK;

		var mock = Mock.Get(rule);
		mock.Setup(x => x.P0_GC).Returns(GlbCompany.CurrentCompany.PK);
		Assert(new ProcessTemplateValidationConditionChecker(mock.Object, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ForHasAirCargoAttached()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));

		Assert(!new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		var hAWB = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
		hAWB.CS_JS = Shipment.PK;
		Assert(new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ForHasAirCargoAttachedNZ()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));

		Assert(!new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		var mAWB = Factory.New<Enterprise.Integration.Customs.NZ.ICusMAWB>();
		var hAWB = Factory.New<Enterprise.Integration.Customs.NZ.ICusHAWB>();
		hAWB.CS_CM = mAWB.PK;
		hAWB.CS_JS = Shipment.PK;
		Assert(!new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ForCurrentCompanyHandlesPickupCartage()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesPickupCartage));

		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		Shipment.DocsAndCartage.PickupCartageCoPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1Met_ForCurrentCompanyHandlesDeliveryCartage()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesDeliveryCartage));

		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
		Shipment.DocsAndCartage.DeliveryCartageCoPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	#endregion

	#region IsCondition2Met

	public void TestIsCondition1or2Met_ForImport()
	{
		var ruleCondition1 = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.Import));
		var ruleCondition2 = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.Import));

		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleCondition1, Shipment).AreConditionsMet());
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleCondition2, Shipment).AreConditionsMet());

		Shipment.JS_RL_NKOrigin = "MYPKG";
		Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

		AssertEquals(true, Shipment.IsImport());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleCondition1, Shipment).AreConditionsMet());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleCondition2, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1or2Met_ForExport()
	{
		var ruleCondition1 = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.Export));
		var ruleCondition2 = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.Export));

		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleCondition1, Shipment).AreConditionsMet());
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleCondition2, Shipment).AreConditionsMet());

		Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		Shipment.JS_RL_NKDestination = "MYPKG";
		AssertEquals(true, Shipment.IsExport());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleCondition1, Shipment).AreConditionsMet());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleCondition2, Shipment).AreConditionsMet());
	}

	public void TestIsCondition1or2Met_ForDomestic()
	{
		var ruleCondition1 = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.Domestic));
		var ruleCondition2 = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.Domestic));

		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleCondition1, Shipment).AreConditionsMet());
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleCondition2, Shipment).AreConditionsMet());

		Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort.Substring(0, 2) + "XXX";
		AssertEquals(true, Shipment.IsDomestic());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleCondition1, Shipment).AreConditionsMet());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleCondition2, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForLCL()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.LCL));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.Groupage;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForFCL()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.FCL));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForBuyersConsolMaster()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForBuyersConsolSub()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForColoadMaster()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster));

		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BlindCoLoadMaster;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForColoadSub()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub));

		Shipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
		Shipment.CoLoadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForAssemblyMaster()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster));

		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForAssemblySub()
	{
		var rule = SetupRuleForTest(mock => mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub));

		Shipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
		Shipment.CoLoadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.CoLoadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.CoLoadMasterShipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
		Shipment.CoLoadMasterShipment.CoLoadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());

		Shipment.CoLoadShipments.AddNew();
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(rule, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_ForReleaseType()
	{
		IProcessTemplateValidation SetupRuleWithReleaseType(string releaseType) =>
			SetupRuleForTest(mock =>
				{
					mock.Setup(x => x.P0V_Condition2).Returns((ZString)JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType);
					mock.Setup(x => x.P0V_Condition2Value).Returns((ZString)releaseType);
				});

		var ruleAAA = SetupRuleWithReleaseType("AAA");
		var ruleBBB = SetupRuleWithReleaseType("BBB");

		Shipment.JS_ReleaseType = "AAA";
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleAAA, Shipment).AreConditionsMet());
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleBBB, Shipment).AreConditionsMet());

		Shipment.JS_ReleaseType = "BBB";
		AssertEquals(false, new ProcessTemplateValidationConditionChecker(ruleAAA, Shipment).AreConditionsMet());
		AssertEquals(true, new ProcessTemplateValidationConditionChecker(ruleBBB, Shipment).AreConditionsMet());
	}

	public void TestIsCondition2Met_BRK() => CombineAssertions(() =>
	{
		using var disposable = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
		declaration.JE_JS = shipment.PK;
		declaration.JE_IsCancelled = ZBool.True;

		IProcessTemplateValidation SetupRuleWithCompany(ZGuid companyPK) =>
			SetupRuleForTest(mock =>
			{
				mock.Setup(x => x.P0V_GC_Company).Returns(companyPK);
				mock.Setup(x => x.P0V_Condition1).Returns((ZString)JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached);
				mock.Setup(x => x.P0V_Condition2).Returns((ZString)ProcessTasksLookups.MacroCondition);
				mock.Setup(x => x.P0V_Condition2Value).Returns((ZString)"\"<JE_IsCancelled>\"==\"Y\"");
			});

		var checker1 = new ProcessTemplateValidationConditionChecker(SetupRuleWithCompany(Factory.New<GlbCompany>().PK), shipment);
		AssertEquals("Without Related JobDeclaration", false, checker1.AreConditionsMet());

		var checker2 = new ProcessTemplateValidationConditionChecker(SetupRuleWithCompany(declaration.JE_GC), shipment);
		AssertEquals("With Related JobDeclaration", true, checker2.AreConditionsMet());
	});

	#endregion

	public void TestIsCompanyMet_BRK()
	{
		using var disposable = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
		declaration.JE_JS = shipment.PK;
		var checker = new ProcessTemplateValidationConditionChecker(
			SetupRuleForTest(mock =>
			{
				mock.Setup(x => x.P0V_Condition1).Returns(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached);
				mock.Setup(x => x.P0V_GC_Company).Returns(GlbCompany.CurrentCompany.PK);
			}), shipment);
		AssertEquals(true, checker.AreConditionsMet());
	}

	public void TestIsCompanyMet_NotBRK()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var rule = SetupRuleForTest(mock =>
		{
			mock.Setup(x => x.P0V_GC_Company).Returns(GlbCompany.CurrentCompany.PK);
		});

		var checker = new ProcessTemplateValidationConditionChecker(rule, shipment);
		AssertEquals(true, checker.AreConditionsMet());
	}

	ForwardingConsol Consol1 => consol1 ??= Factory.NewWithValidTestData<ForwardingConsol>();
	ForwardingConsol consol1;

	ForwardingConsol Consol2 => consol2 ??= Factory.NewWithValidTestData<ForwardingConsol>();
	ForwardingConsol consol2;

	ForwardingShipment Shipment => shipment ??= Factory.NewWithValidTestData<ForwardingShipment>();
	ForwardingShipment shipment;

	public IProcessTemplateValidation SetupRuleForTest(Action<Mock<IProcessTemplateValidation>> setupAction)
	{
		var mock = new Mock<IProcessTemplateValidation>();
		mock.Setup(x => x.Factory).Returns(Factory);
		mock.Setup(x => x.P0V_ContextType).Returns(ProcessTemplateValidationContextType.Codes.CargoWise);
		setupAction?.Invoke(mock);
		return mock.Object;
	}
}
