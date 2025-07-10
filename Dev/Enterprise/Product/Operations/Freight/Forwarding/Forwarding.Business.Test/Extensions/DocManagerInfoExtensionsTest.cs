using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DocManagerInfoExtensionsTest : TestCaseWithFactory
	{
		public void TestAllRelatedObjectsRetrievedForAUOceanBill()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var oceanBizObjType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusSCAOceanBill>();
			var oceanBill = Factory.New(oceanBizObjType);
			oceanBill[CusSCAOceanBillSchema.CB_ParentId] = consol.PK;
			oceanBill[CusSCAOceanBillSchema.CB_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			var relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			AssertContainsExactElementsInAnyOrder("Consol exptected related objects", new BusinessObject[] { shipment, oceanBill }, relatedObjects);
			relatedObjects = new ForwardingShipmentDocManagerInfo(shipment).RelatedObjects;
			AssertContainsExactElementsInAnyOrder("Shipment exptected related objects", new BusinessObject[] { consol, oceanBill }, relatedObjects);
		}

		public void TestGetCurrentCountryRelatedObjectsChainForCusMAWB()
		{
			var commonCusMAWBType = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusMAWB>();
			var consol = Factory.New<ForwardingConsol>();
			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();

			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();

			var gbCompany = Factory.New<GlbCompany>();
			gbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var gbBranch = gbCompany.Branches.AddNew();

			var cusMAWBAU = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusMAWB>());
			cusMAWBAU[CusMAWBSchema.CM_JK] = consol.PK;
			cusMAWBAU[CusMAWBSchema.CM_ApplicationCode] = "CMR";
			cusMAWBAU[CusMAWBSchema.CM_GB] = auBranch.PK;

			var cusMAWBNZ = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.ICusMAWB>());
			cusMAWBNZ[CusMAWBSchema.CM_JK] = consol.PK;
			cusMAWBNZ[CusMAWBSchema.CM_ApplicationCode] = "TSW";
			cusMAWBNZ[CusMAWBSchema.CM_GB] = nzBranch.PK;

			var cusMAWBGB = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.GB.CCSUK.ICusMAWB>());
			cusMAWBGB[CusMAWBSchema.CM_JK] = consol.PK;
			cusMAWBGB[CusMAWBSchema.CM_ApplicationCode] = "CUK";
			cusMAWBGB[CusMAWBSchema.CM_GB] = gbBranch.PK;

			var docManagerInfo = new ForwardingConsolDocManagerInfo(consol);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var relatedBOs = docManagerInfo.RelatedObjects;
				AssertEquals(1, relatedBOs.Length);
				AssertEquals(cusMAWBAU.PK, relatedBOs[0].PK);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var relatedBOs = docManagerInfo.RelatedObjects;
				AssertEquals(1, relatedBOs.Length);
				AssertEquals(cusMAWBNZ.PK, relatedBOs[0].PK);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var relatedBOs = docManagerInfo.RelatedObjects;
				AssertEquals(1, relatedBOs.Length);
				AssertEquals(cusMAWBGB.PK, relatedBOs[0].PK);
			}
		}

		public void TestGetCurrentCountryRelatedObjectsChainForCusHAWB()
		{
			var commonCusMAWBType = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.ICusHAWB>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();

			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();

			var gbCompany = Factory.New<GlbCompany>();
			gbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var gbBranch = gbCompany.Branches.AddNew();

			var cusMAWBAU = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusMAWB>());
			cusMAWBAU[CusMAWBSchema.CM_ApplicationCode] = "CMR";
			var cusHAWBAU = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>());
			cusHAWBAU[CusHAWBSchema.CS_CM] = cusMAWBAU.PK;
			cusHAWBAU[CusHAWBSchema.CS_JS] = shipment.PK;
			cusMAWBAU[CusMAWBSchema.CM_JK] = consol.PK;
			cusMAWBAU[CusMAWBSchema.CM_GB] = auBranch.PK;

			var cusMAWBNZ = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.ICusMAWB>());
			cusMAWBNZ[CusMAWBSchema.CM_ApplicationCode] = "TSW";
			var cusHAWBNZ = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.ICusHAWB>());
			cusHAWBNZ[CusHAWBSchema.CS_CM] = cusMAWBNZ.PK;
			cusHAWBNZ[CusHAWBSchema.CS_JS] = shipment.PK;
			cusMAWBNZ[CusMAWBSchema.CM_JK] = consol.PK;
			cusMAWBNZ[CusMAWBSchema.CM_GB] = nzBranch.PK;

			var cusMAWBGB = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.GB.CCSUK.ICusMAWB>());
			cusMAWBGB[CusMAWBSchema.CM_ApplicationCode] = "CUK";
			var cusHAWBGB = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>());
			cusHAWBGB[CusHAWBSchema.CS_CM] = cusMAWBGB.PK;
			cusHAWBGB[CusHAWBSchema.CS_JS] = shipment.PK;
			cusMAWBGB[CusMAWBSchema.CM_JK] = consol.PK;
			cusMAWBGB[CusMAWBSchema.CM_GB] = gbBranch.PK;

			var docManagerInfo = new ForwardingConsolDocManagerInfo(consol);
			var docManagerInfoShipment = new ForwardingShipmentDocManagerInfo(shipment);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var relatedBOs = docManagerInfo.RelatedObjects;
				AssertEquals(3, relatedBOs.Length);  // One of them is shipment.
				Assert(relatedBOs.Any(x => x.PK == cusMAWBAU.PK));
				Assert(relatedBOs.Any(x => x.PK == cusHAWBAU.PK));

				var relatedBOsShipment = docManagerInfoShipment.RelatedObjects;
				AssertEquals(2, relatedBOsShipment.Length);  // One of them is consol.
				Assert(relatedBOsShipment.Any(x => x.PK == cusHAWBAU.PK));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var relatedBOs = docManagerInfo.RelatedObjects;
				AssertEquals(2, relatedBOs.Length);  // NZ CusHAWB is not IDocManagerSupport
				Assert(relatedBOs.Any(x => x.PK == cusMAWBNZ.PK));
				var relatedBOsShipment = docManagerInfoShipment.RelatedObjects;
				AssertEquals(1, relatedBOsShipment.Length);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var relatedBOs = docManagerInfo.RelatedObjects;
				AssertEquals(3, relatedBOs.Length);
				Assert(relatedBOs.Any(x => x.PK == cusMAWBGB.PK));
				Assert(relatedBOs.Any(x => x.PK == cusHAWBGB.PK));
				var relatedBOsShipment = docManagerInfoShipment.RelatedObjects;
				AssertEquals(3, relatedBOsShipment.Length);   // GB will include CusMAWB.
				Assert(relatedBOsShipment.Any(x => x.PK == cusHAWBGB.PK));
			}
		}
	}
}
