using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing;

sealed class ForwardingShipmentProcessTaskConditionCheckerTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Factory is null", () =>
		{
			_ = new ForwardingShipmentProcessTaskConditionChecker(null, Shipment);
		});
		AssertExceptionThrown<ArgumentNullException>("Shipment is null", () =>
		{
			_ = new ForwardingShipmentProcessTaskConditionChecker(Factory, null);
		});
	});

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

		Shipment.JS_RL_NKOrigin = "AUSYD";
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));

		Shipment.JS_RL_NKOrigin = "AUMEL";
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
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

		Shipment.JS_RL_NKDestination = "SGSIN";
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));

		Shipment.JS_RL_NKDestination = "USLAX";
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
	}

	public void TestIsCondition1Met_ConsolDischargeLeg()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg));
		Consol1.Shipments.Add(Shipment);
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg));
	}

	public void TestIsCondition1Met_ConsolPreCarriageLeg()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolPreCarriageLeg));
		Consol1.Transports[0].JW_TransportType = Constants.TransportPlanningType.PreCarriage;
		Consol1.Shipments.Add(Shipment);
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolPreCarriageLeg));
	}

	public void TestIsCondition1Met_ForHasBrokerageAttached()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached));
		BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
		declaration[JobDeclarationSchema.JE_JS] = Shipment.PK;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached));
	}

	public void TestIsCondition1Met_ForHasAirCargoAttached()
	{
		Assert(!new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
		var hAWB = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
		hAWB.CS_JS = Shipment.PK;
		Assert(new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
	}

	public void TestIsCondition1Met_ForHasAirCargoAttachedNZ()
	{
		Assert(!new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
		var mAWB = Factory.New<Enterprise.Integration.Customs.NZ.ICusMAWB>();
		var hAWB = Factory.New<Enterprise.Integration.Customs.NZ.ICusHAWB>();
		hAWB.CS_CM = mAWB.PK;
		hAWB.CS_JS = Shipment.PK;
		Assert(!new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
	}

	public void TestIsCondition1Met_ForCurrentCompanyHandlesPickupCartage()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesPickupCartage));
		Shipment.DocsAndCartage.PickupCartageCoPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesPickupCartage));
	}

	public void TestIsCondition1Met_ForCurrentCompanyHandlesDeliveryCartage()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesDeliveryCartage));
		Shipment.DocsAndCartage.DeliveryCartageCoPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesDeliveryCartage));
	}

	#endregion

	#region IsCondition2Met

	public void TestIsCondition1or2Met_ForImport()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Import));
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Import, ""));
		Shipment.JS_RL_NKOrigin = "MYPKG";
		Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		AssertEquals(true, Shipment.IsImport());
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Import));
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Import, ""));
	}

	public void TestIsCondition1or2Met_ForExport()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Export));
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Export, ""));
		Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		Shipment.JS_RL_NKDestination = "MYPKG";
		AssertEquals(true, Shipment.IsExport());
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Export));
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Export, ""));
	}

	public void TestIsCondition1or2Met_ForDomestic()
	{
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Domestic));
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Domestic, ""));
		Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort.Substring(0, 2) + "XXX";
		AssertEquals(true, Shipment.IsDomestic());
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Domestic));
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Domestic, ""));
	}

	public void TestIsCondition2Met_ForLCL()
	{
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.Groupage;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
	}

	public void TestIsCondition2Met_ForFCL()
	{
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.FCL, ""));
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.FCL, ""));
	}

	public void TestIsCondition2Met_ForBuyersConsolMaster()
	{
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster, ""));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster, ""));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster, ""));
	}

	public void TestIsCondition2Met_ForBuyersConsolSub()
	{
		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub, ""));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub, ""));

		Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub, ""));
	}

	public void TestIsCondition2Met_ForColoadMaster()
	{
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster, ""));

		Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster, ""));

		Shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster, ""));
	}

	public void TestIsCondition2Met_ForColoadSub()
	{
		Shipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
		Shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub, ""));

		Shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub, ""));
	}

	public void TestIsCondition2Met_ForAssemblyMaster()
	{
		Shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster, ""));

		Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster, ""));

		Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster, ""));
	}

	public void TestIsCondition2Met_ForAssemblySub()
	{
		Shipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
		Shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));

		Shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));

		Shipment.CoLoadMasterShipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
		Shipment.CoLoadMasterShipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));

		Shipment.CoLoadShipments.AddNew();
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));
	}

	public void TestIsCondition2Met_ForReleaseType()
	{
		Shipment.JS_ReleaseType = "AAA";
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "AAA"));
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "BBB"));

		Shipment.JS_ReleaseType = "BBB";
		AssertEquals(false, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "AAA"));
		AssertEquals(true, new ForwardingShipmentProcessTaskConditionChecker(Factory, Shipment).IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "BBB"));
	}

	#endregion

	ForwardingConsol Consol1 => consol1 ??= Factory.NewWithValidTestData<ForwardingConsol>();
	ForwardingConsol consol1;

	ForwardingConsol Consol2 => consol2 ??= Factory.NewWithValidTestData<ForwardingConsol>();
	ForwardingConsol consol2;

	ForwardingShipment Shipment => shipment ??= Factory.NewWithValidTestData<ForwardingShipment>();
	ForwardingShipment shipment;
}
