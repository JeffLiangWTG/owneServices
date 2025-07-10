using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(VisitedPort))]
	public class VisitedPortTest : Customs.Business.Testing.CusCodeDataTest<VisitedPort>
	{
		public void TestValidation()
		{
			AssertEquals("Validation", typeof(VisitedPortValidation), Port.Validation.GetType());
		}

		public void TestLookups()
		{
			AssertEquals("Lookups", typeof(VisitedPortLookups), Port.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.TRVisitedPort, Port.CY_Type);
		}

		protected override IEnumerable<VisitedPort> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			yield return bill.VisitedPorts.AddNew();
			yield return header.VisitedPorts.AddNew();
		}

		public void TestParents()
		{
			var newPort = Factory.New<VisitedPort>();
			newPort.CY_ParentID = Bill.PK;
			newPort.CY_ParentTableCode = Bill.TablePrefix;
			AssertEquals(Bill, newPort.Parent);
		}

		public void TestDescriptionOfSelectedCY_Code()
		{
			Port.CY_Code = "TRIST-001";
			Bill.VisitedPorts.Add(Port);
			AssertEquals(Bill.VisitedPorts[0].Description, Port.Description);
		}

		public void TestLegNumberIsSet()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var port = header.VisitedPorts.AddNew();
			port.CY_Code = "TRIST-001";
			var port2 = header.VisitedPorts.AddNew();
			port2.CY_Data = "AUSYD";
			AssertEquals((ZShort)1, port.CY_Order);
			AssertEquals((ZShort)2, port2.CY_Order);
			var port3 = header.VisitedPorts.AddNew();
			port3.CY_Code = "TRIST-003";
			header.VisitedPorts.Remove(port2.PK);
			AssertEquals((ZShort)2, port3.CY_Order);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<VisitedPort>();
		}

		AsycudaBill Bill
		{
			get
			{
				return bill ?? (bill = Factory.New<AsycudaBill>());
			}
		}

		AsycudaBill bill;
		VisitedPort Port
		{
			get
			{
				return port ?? (port = Factory.New<VisitedPort>());
			}
		}

		VisitedPort port;
		public void TestValidationType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var port = header.VisitedPorts.AddNew();
			port.CY_Code = "TRIST-001";
			AssertEquals(port.Validation.GetType(), typeof(VisitedPortValidationForHeader));
		}
	}
}
