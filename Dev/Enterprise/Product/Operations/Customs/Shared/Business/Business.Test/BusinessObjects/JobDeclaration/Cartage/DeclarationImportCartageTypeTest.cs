using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationImportCartageTypeTest : TestCaseWithFactory
	{
		public void TestDeliveryCompletedContainerised()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			BaseCusContainer container1 = dec.CusContainers.AddNew();
			CommonContainer jobCOntainer1 = Factory.New<CommonContainer>();
			container1.CO_JC = jobCOntainer1.PK;

			BaseCusContainer container2 = dec.CusContainers.AddNew();
			CommonContainer jobCOntainer2 = Factory.New<CommonContainer>();
			container2.CO_JC = jobCOntainer2.PK;

			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			DeclarationImportCartageType decCT = new DeclarationImportCartageType(dec);

			ZDateTime time = ZDateTime.Now.AddDays(-1);

			decCT.DeliveryCompleted(jobCOntainer1, DocAddressType.LocalCartageImporter, time);
			AssertEquals(time, jobCOntainer1.JC_ArrivalCartageComplete);
			AssertEquals(ZDateTime.Empty, dec.DocsAndCartage.JP_DeliveryCartageCompleted);

			time = time.AddMinutes(3);
			decCT.DeliveryCompleted(jobCOntainer2, DocAddressType.LocalCartageImporter, time);
			AssertEquals(time, jobCOntainer2.JC_ArrivalCartageComplete);
			AssertEquals(time, dec.DocsAndCartage.JP_DeliveryCartageCompleted);

			var time2 = time.AddHours(-11);
			decCT.DeliveryCompleted(jobCOntainer2, DocAddressType.ArrivalCFSAddress, time2);
			AssertEquals(time, jobCOntainer2.JC_ArrivalCartageComplete);
			AssertEquals(time, dec.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		public void TestDeliveryCompletedLoose()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			BaseCusContainer container1 = dec.CusContainers.AddNew();
			BaseCusContainer container2 = dec.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			DeclarationImportCartageType decCT = new DeclarationImportCartageType(dec);

			ZDateTime time = ZDateTime.Now;

			decCT.DeliveryCompleted(DocAddressType.LocalCartageImporter, time);
			AssertEquals(time, dec.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		public void TestCartageJobType()
		{
			//Container LCL
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			BaseCusContainer container1 = dec.CusContainers.AddNew();
			BaseCusContainer container2 = dec.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			DeclarationImportCartageType decCT = new DeclarationImportCartageType(dec);
			AssertEquals(Constants.CartageJobType.NEW_LCLImport, decCT.CartageJobType);

			//Container FCL Mixed
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportToCNE, decCT.CartageJobType);

			//Container FCL
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportToCNE, decCT.CartageJobType);

			//Container FCL with Depot
			dec.DepotDocAddress.MakePersistentEvenIfEmpty();
			dec.DepotDocAddress.E2_Address1 = "Add1";
			dec.DepotDocAddress.E2_Contact = "Cont1";
			dec.DepotDocAddress.E2_OA_Address = ZGuid.NewZGuid();
			AssertEquals(Constants.CartageJobType.NEW_FCLImportUnpack, decCT.CartageJobType);

			//Road
			dec.JE_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.CartageJobType.NEW_LCLImport, decCT.CartageJobType);
		}

		public void TestEstimatedCartagePickup()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			ZDateTime now = ZDateTime.Now;
			DeclarationImportCartageType ct = new DeclarationImportCartageType(dec);
			dec.DocsAndCartage.JP_EstimatedDelivery = now;
			AssertEquals(now, ct.EstimatedCartagePickup);
		}

		public void TestEstimatedCartageDelivery()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			ZDateTime now = ZDateTime.Now;
			DeclarationImportCartageType ct = new DeclarationImportCartageType(dec);
			dec.DocsAndCartage.JP_DeliveryRequiredBy = now;
			AssertEquals(now, ct.EstimatedCartageDelivery);
		}

		[TestDate(2011, 10, 4)]
		public void TestSetTotalDemurrage()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var now = ZDateTime.Now;
			var ct = new DeclarationImportCartageType(dec);
			AssertEquals(true, dec.DocsAndCartage.JP_DeliveryTruckWaitTime.IsEmpty);

			ct.SetTotalDemurrage(new TimeSpan(2, 3, 4));
			AssertEquals(new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 1, 1, 2, 3, 4), dec.DocsAndCartage.JP_DeliveryTruckWaitTime);
		}

		public void TestDropMode()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			DeclarationImportCartageType ct = new DeclarationImportCartageType(dec);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, ct.DropMode);
		}

		public void TestCartageLooseCargo()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			DeclarationExportCartageType ct = new DeclarationExportCartageType(dec);
			AssertEquals(1, ct.CartageLooseCargo.Count);
			AssertEquals(dec, ct.CartageLooseCargo.First());
		}

		public void TestGetMatchingDirectionCodes()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cartageType = new DeclarationImportCartageType(declaration);
			AssertArrayEqualsByElements(new ZString[] { "IMP", "DST" }, cartageType.GetMatchingDirectionCodes().ToArray());
		}
	}
}
