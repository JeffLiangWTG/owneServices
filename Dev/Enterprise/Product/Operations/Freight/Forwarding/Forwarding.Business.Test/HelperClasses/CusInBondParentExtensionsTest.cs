using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CusInBondParentExtensionsTest : TestCaseWithFactory
	{
		public void TestGetInBondHeader()
		{
			string usApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "S#@";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "G#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var shipment = Factory.New<ForwardingShipment>();
			AssertNull("No InBond", shipment.GetInBondHeader(usApplicationCode));

			var inBond1 = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBond1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inBond1.BH_ParentID = shipment.PK;
			inBond1.BH_ParentTableCode = shipment.TablePrefix;
			inBond1.BH_SystemCreateTimeUtc = new ZDateTime(2012, 3, 1);
			var inBond2 = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBond2.BH_ApplicationCode = "Z!D";
			inBond2.BH_ParentID = shipment.PK;
			inBond2.BH_ParentTableCode = shipment.TablePrefix;
			inBond2.BH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			var inBond3 = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBond3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inBond3.BH_ParentID = shipment.PK;
			inBond3.BH_ParentTableCode = shipment.TablePrefix;
			inBond3.BH_SystemCreateTimeUtc = new ZDateTime(2012, 2, 1);
			var inBond4 = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBond4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inBond4.BH_ParentID = shipment.PK;
			inBond4.BH_ParentTableCode = "S!";
			inBond4.BH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			var inBond5 = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBond5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inBond5.BH_ParentID = ZGuid.NewZGuid();
			inBond5.BH_ParentTableCode = shipment.TablePrefix;
			inBond5.BH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			var inBondInDiffComp = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inBondInDiffComp.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inBondInDiffComp.BH_ParentID = shipment.PK;
			inBondInDiffComp.BH_ParentTableCode = shipment.TablePrefix;
			inBondInDiffComp.BH_GB = otherBranch.PK;
			inBondInDiffComp.BH_SystemCreateTimeUtc = new ZDateTime(2012, 1, 1);
			var inbond6DifferentApplicationCode = Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			inbond6DifferentApplicationCode.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			inbond6DifferentApplicationCode.BH_ParentID = shipment.PK;
			inbond6DifferentApplicationCode.BH_ParentTableCode = shipment.TablePrefix;
			inbond6DifferentApplicationCode.BH_SystemCreateTimeUtc = new ZDateTime(2011, 3, 1);
			var inbond7EUNCTS5ApplicationCode = Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			inbond7EUNCTS5ApplicationCode.BH_ParentID = shipment.PK;
			inbond7EUNCTS5ApplicationCode.BH_ParentTableCode = shipment.TablePrefix;
			inbond7EUNCTS5ApplicationCode.BH_SystemCreateTimeUtc = new ZDateTime(2023, 3, 1);
			inbond7EUNCTS5ApplicationCode.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Should Match InBond3", inBond3, shipment.GetInBondHeader(usApplicationCode));
			AssertEquals("Should Match InBond6", inbond6DifferentApplicationCode, shipment.GetInBondHeader(CusInBondApplicationCodeList.Codes.NCTS4));
			AssertEquals("Should Match InBond6", inbond7EUNCTS5ApplicationCode, shipment.GetInBondHeader(CusInBondApplicationCodeList.Codes.NCTS5));
		}

		public void TestFactoryCaching()
		{
			string usApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			var shipment = Factory.New<ForwardingShipment>();
			var originalDBHitCount = Factory.DatabaseLoadCount;

			shipment.GetInBondHeader(usApplicationCode, false);
			AssertEquals("First time must hit DB", 1, Factory.DatabaseLoadCount - originalDBHitCount);

			shipment.GetInBondHeader(usApplicationCode, false);
			AssertEquals("Should not hit DB", 1, Factory.DatabaseLoadCount - originalDBHitCount);

			shipment.GetInBondHeader(usApplicationCode);
			AssertEquals("Should reload from DB", 2, Factory.DatabaseLoadCount - originalDBHitCount);
		}
	}
}
