using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportVisitedPort))]
	public class USExportVisitedPortTest : Customs.Business.Testing.CusCodeDataTest<USExportVisitedPort>
	{
		public void TestDefaultSCKFromUNLO()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "52345", "52345 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var catttPort = Factory.New<RefUNLOCO>();
			catttPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			catttPort.RL_Code = "CATTT";
			var map = catttPort.RefLocoMaps.AddNew();
			map.RY_LocalPortCode = "52345";
			map.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			map.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			Factory.Save();

			var port = CreateBusinessObjectForTesting();
			port.CY_Data = "CATTT";

			AssertEquals("Flash Point Temperature C", "52345", port.CY_Code);
		}

		public void TestLookups()
		{
			var port = Factory.New<USExportVisitedPort>();
			port.CY_Code = "CAHAL";
			AssertEquals("Lookups", typeof(USExportVisitedPortLookups), port.Lookups.GetType());
		}

		public void TestSetDefaultValuesOnCodeType()
		{
			var port = Factory.New<USExportVisitedPort>();
			AssertEquals(USExportCusCodeType.Codes.UVP, port.CY_Type);
		}

		public void TestParent()
		{
			var newPort = Factory.New<USExportVisitedPort>();
			var bill = Factory.New<USExportAsycudaBill>();
			newPort.CY_ParentID = bill.PK;
			newPort.CY_ParentTableCode = bill.TablePrefix;
			AssertEquals(bill, newPort.Parent);
		}

		public void TestSequenceCY_Order()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			var expBill = header.Bills.AddNew();
			var port1 = expBill.VisitedPorts.AddNew();
			AssertEquals((short)1, port1.CY_Order);
			var port2 = expBill.VisitedPorts.AddNew();
			AssertEquals((short)2, port2.CY_Order);
			var port3 = expBill.VisitedPorts.AddNew();
			AssertEquals((short)3, port3.CY_Order);
			var port4 = expBill.VisitedPorts.AddNew();
			AssertEquals((short)4, port4.CY_Order);

			port3.Delete();
			AssertEquals((short)1, port1.CY_Order);
			AssertEquals((short)2, port2.CY_Order);
			AssertEquals((short)3, port4.CY_Order);

			var port5 = Factory.New<USExportVisitedPort>();
			port5.CY_ParentID = expBill.PK;
			port5.CY_ParentTableCode = expBill.TablePrefix;
			AssertEquals((short)4, port5.CY_Order);
		}

		USExportVisitedPort CreateBusinessObjectForTesting()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			var bill = header.Bills.AddNew();
			return bill.VisitedPorts.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBusinessObjectForTesting();
		}

		protected override IEnumerable<USExportVisitedPort> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			var bill = header.Bills.AddNew();
			yield return bill.VisitedPorts.AddNew();
		}

		public void TestValidationType()
		{
			var port = CreateBusinessObjectForTesting();
			port.CY_Code = "CAHAL";
			AssertEquals("USExportVisistedPort Validation", port.Validation.GetType(), typeof(USExportVisitedPortValidation));
		}

		public void TestPortDefaulterRefLocoMappings()
		{
			RefUNLOCOTestDataHelper.CreateScheduleKPort(Factory, true);

			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			var expBill = header.Bills.AddNew();
			var port = expBill.VisitedPorts.AddNew();

			port.CY_Data = "TEST1";
			AssertEquals(nameof(FieldType.TextDropEdit), port.PortCodeFieldType);
			AssertEquals(2, port.PortDefaulterRefLocoMappings.Count);
			Assert(port.PortDefaulterRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60001"));
			Assert(port.PortDefaulterRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60002"));

			port.CY_Data = "TEST2";
			AssertEquals(nameof(FieldType.Text), port.PortCodeFieldType);
			AssertEquals(1, port.PortDefaulterRefLocoMappings.Count);
			Assert(port.PortDefaulterRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60004"));
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			var bill = header.Bills.AddNew();
			return bill.VisitedPorts.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, USExportVisitedPort bizObj)
		{
			factory.Load<USExportAsycudaBill>(bizObj.CY_ParentID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}

