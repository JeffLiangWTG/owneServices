using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestTypeDecider()
		{
			Assert(Factory.New<Customs.Business.BaseCusContainer>() is CusContainer);
		}

		public void TestDeclaration()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestCO_RC()
		{
			EntryContainer.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("20", EntryContainer.CO_ContainerSize);
		}

		public void TestLookupsCachesInstance()
		{
			CusContainer container = (CusContainer)GetNewBusinessObject();
			CusContainerLookups lookup1 = container.Lookups;
			CusContainerLookups lookup2 = container.Lookups;
			AssertEquals(lookup2, lookup1);
		}

		public void TestSetDefaultValues()
		{
			CusContainer container = Factory.New<CusContainer>();
			AssertEquals(Core.Constants.Weight.Tonnes, container.CO_WeightUQ);
		}

		public override void TestDefaultingCO_WeightUQ()
		{
			CusContainer container = Factory.New<CusContainer>();
			AssertEquals(Core.Constants.Weight.Tonnes, container.CO_WeightUQ);
		}

		public override void TestGoodsWeightUQIsKG()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(Core.Constants.Weight.Tonnes, container.GoodsWeightUQ);
		}

		public override void TestFindContainerOnShipmentByContainerNumber()
		{
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode }));
			var port3 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode, port2.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = port1.RL_Code;
			shipment.JS_RL_NKDestination = port3.RL_Code;
			var consol1 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = port2.RL_Code;
			consol1.JK_RL_NKDischargePort = localPort.RL_Code;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234567";
			var consol2 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = localPort.RL_Code;
			consol2.JK_RL_NKDischargePort = port3.RL_Code;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT8901234";
			var line1 = shipment.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";
			var line2 = shipment.OuterPackLines.AddNew();
			container2.PackLines.Add(line2);
			line2.JL_ActualWeight = 150m;
			line2.JL_ActualWeightUQ = "LB";
			Factory.Save(); // Stop JobContainer from being deleted
			var dec = GetJobDeclaration();
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_JS = shipment.PK;
			dec.ShipmentSynchroniser.Synchronise(true);
			var cusContainer = dec.CusContainers.Count > 0 ? dec.CusContainers[0] : dec.CusContainers.AddNew();
			AssertEquals(container2, cusContainer.JobContainer);
			cusContainer.CO_ContainerNumber = "CONT1234567";
			AssertNotEquals(container1, cusContainer.JobContainer);
			AssertEquals(true, cusContainer.JobContainer.IsInDatabase);
		}

		public override void TestUpdateETAAndDelivery()
		{
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = "FCL";
			defaultDelay.G1_RL_NKDischargePort = "USNYC";
			defaultDelay.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;
			var defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = "LCL";
			defaultDelay2.G1_RL_NKDischargePort = "USNYC";
			defaultDelay2.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay2.G1_DaysFromDestinationArrivalToClientDelivery = 2;
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised; //US does not have FCL or LCL in Container Mode
			declaration.JE_RL_NKPortOfArrival = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USCHI";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 1);
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("Delivery defaulted", new ZDateTime(2010, 1, 4), declaration.JE_EstimatedDeliveryOrPickup);
			declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			declaration.CusContainers.RemoveAndDeleteAll();
			AssertEquals("Delivery defaulted", ZDateTime.Empty, declaration.JE_EstimatedDeliveryOrPickup);
			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "LCL";
			AssertEquals("Delivery defaulted", new ZDateTime(2010, 1, 3), declaration.JE_EstimatedDeliveryOrPickup);
		}

		public override void TestUpdateETAAndDeliveryWhenContainerIsDeleted()
		{
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = "FCL";
			defaultDelay.G1_RL_NKDischargePort = "USNYC";
			defaultDelay.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;
			var defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = "LCL";
			defaultDelay2.G1_RL_NKDischargePort = "USNYC";
			defaultDelay2.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay2.G1_DaysFromDestinationArrivalToClientDelivery = 2;
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised; //US does not have FCL or LCL in Container Mode
			declaration.JE_RL_NKPortOfArrival = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USCHI";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 1);
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "LCL";
			declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			declaration.CusContainers.RemoveAndDelete(container);
			AssertEquals("Delivery defaulted", new ZDateTime(2010, 1, 3), declaration.JE_EstimatedDeliveryOrPickup);
		}

		#region Interface Tests
		public void TestContainerNumber()
		{
			AssertEquals("Blank Container", "", CusContainer.ContainerNumber);
			EntryContainer.CO_ContainerNumber = "FMNU03948577";
			AssertEquals("Container No.", "FMNU03948577", CusContainer.ContainerNumber);
		}

		public void TestContainerType()
		{
			AssertEquals("Blank Container", "", CusContainer.ContainerType);
			EntryContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("Container Type", "FCL", CusContainer.ContainerType);
		}

		public void TestContainerSize()
		{
			AssertEquals(0, CusContainer.ContainerSize);
			RefContainer twentyFooter = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			EntryContainer.CO_ContainerSize = "20";
			AssertEquals(20, CusContainer.ContainerSize);
			var companyContainerType = Factory.NewWithValidTestData<RefContainer>();
			companyContainerType.RC_Code = "500";
			companyContainerType.RC_ContainerType = ContainerTypeCodeList.Codes.FCL;
			companyContainerType.RC_Description = "Geodis Special";
			companyContainerType.RC_Length = 393;
			fEntryContainer = Factory.New<CusContainer>();
			EntryContainer.CO_RC = companyContainerType.PK;
			AssertEquals("Container length should not default if it is not a standard length allowed by SG Customs", 0, CusContainer.ContainerSize);
		}

		public void TestContainerWeight()
		{
			AssertEquals("Blank Container", 0m, CusContainer.ContainerWeight);
			EntryContainer.CO_Weight = 15.0m;
			AssertEquals("Container Weight", 15m, CusContainer.ContainerWeight);
		}

		public void TestContainerWeightUnit()
		{
			EntryContainer.CO_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Container Weight Unit", "T", CusContainer.ContainerWeightUnit);
		}

		public void TestSealNumber()
		{
			AssertEquals("Blank Container", "", CusContainer.SealNumber);
			EntryContainer.CO_Seal = "XX-93847Z";
			AssertEquals("Container Seal", "XX-93847Z", CusContainer.SealNumber);
		}

		#endregion
		#region Implementation

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo)
		{
			ICustomLabelsProvider result = new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
			return result;
		}

		#region EntryContainer
		ICusContainer CusContainer => EntryContainer;

		CusContainer EntryContainer
		{
			get
			{
				if (fEntryContainer == null)
				{
					fEntryContainer = Factory.New<CusContainer>();
				}
				return fEntryContainer;
			}
		}
		CusContainer fEntryContainer;

		#endregion
		#endregion
	}
}
