using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(Package))]
	sealed class PackageTest : Customs.Business.Testing.BasePackageTest
	{
		public void TestLookups()
		{
			var package = Factory.New<Package>();
			AssertEquals(typeof(PackageLookups), package.Lookups.GetType());
		}

		public void TestValidation()
		{
			var package = Factory.New<Package>();
			AssertEquals(typeof(PackageValidation), package.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var pack = declaration.Packages.AddNew();
			return pack;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		public void TestSetCW_HouseBillDefaultValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "UPDATEDHB Test";
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "UPDATEDHB Test2";
			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = bill2.CU_BillUniqueCode;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 1;
			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 2;
			AssertEquals(bill2.CU_BillUniqueCode, package1.CW_HouseBill);
			AssertEquals(bill2.CU_BillUniqueCode, package2.CW_HouseBill);
			AssertEquals(bill2.CU_BillUniqueCode, package3.CW_HouseBill);
		}

		public void TestReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.Shipments.Add(shipment);
			consol.JK_MasterBillNum = "M1";
			var forwardingContainer1 = consol.Containers.AddNew();
			forwardingContainer1.JC_ContainerNum = "C1";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = forwardingContainer1.PK;
			packLine1.JL_ActualWeight = 10M;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_ActualVolume = 2M;
			packLine1.JL_ActualVolumeUQ = "M";
			packLine1.JL_Length = 1.1M;
			packLine1.JL_Width = 1.2M;
			packLine1.JL_Height = 1.3M;
			packLine1.JL_UnitOfDimension = "M";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var package = declaration.Packages.AddNew();
			var provider = package as ISynchroniserReadOnlyMembersProvider;
			AssertNotNull(provider);
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_GrossWeightInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_GrossWeightUQInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_VolumeInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_VolumeUQInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_LengthInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_WidthInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_HeightInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_DimensionUQInfo.Name));
			Assert(provider.SynchroniserReadOnlyMembers.Contains(package.CW_HouseBillInfo.Name));
			Assert(!provider.SynchroniserReadOnlyMembers.Contains(package.CW_MarksAndNosInfo.Name));
			Assert(!provider.SynchroniserReadOnlyMembers.Contains(package.CW_NetWeightInfo.Name));
			Assert(!provider.SynchroniserReadOnlyMembers.Contains(package.CW_NetWeightUQInfo.Name));
			Assert(!package.CW_GrossWeightInfo.ReadOnly);
			Assert(!package.CW_GrossWeightUQInfo.ReadOnly);
			Assert(!package.CW_VolumeInfo.ReadOnly);
			Assert(!package.CW_VolumeUQInfo.ReadOnly);
			Assert(!package.CW_LengthInfo.ReadOnly);
			Assert(!package.CW_WidthInfo.ReadOnly);
			Assert(!package.CW_HeightInfo.ReadOnly);
			Assert(!package.CW_DimensionUQInfo.ReadOnly);
			Assert(!package.CW_HouseBillInfo.ReadOnly);
			Assert(!package.CW_MarksAndNosInfo.ReadOnly);
			Assert(!package.CW_NetWeightInfo.ReadOnly);
			Assert(!package.CW_NetWeightUQInfo.ReadOnly);
			declaration.JE_JS = shipment.PK;
			Assert(package.CW_GrossWeightInfo.ReadOnly);
			Assert(package.CW_GrossWeightUQInfo.ReadOnly);
			Assert(package.CW_VolumeInfo.ReadOnly);
			Assert(package.CW_VolumeUQInfo.ReadOnly);
			Assert(package.CW_LengthInfo.ReadOnly);
			Assert(package.CW_WidthInfo.ReadOnly);
			Assert(package.CW_HeightInfo.ReadOnly);
			Assert(package.CW_DimensionUQInfo.ReadOnly);
			Assert(package.CW_HouseBillInfo.ReadOnly);
			Assert(!package.CW_MarksAndNosInfo.ReadOnly);
			Assert(!package.CW_NetWeightInfo.ReadOnly);
			Assert(!package.CW_NetWeightUQInfo.ReadOnly);
		}

		public void TestNetWeightInInKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_NetWeight = 1000;
			package.CW_NetWeightUQ = "G";
			AssertEquals(1m, package.NetWeightInKilograms);
		}

		public void TestGrossWeightInInKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_GrossWeight = 1000;
			package.CW_GrossWeightUQ = "G";
			AssertEquals(1m, package.GrossWeightInKilograms);
		}
	}
}
