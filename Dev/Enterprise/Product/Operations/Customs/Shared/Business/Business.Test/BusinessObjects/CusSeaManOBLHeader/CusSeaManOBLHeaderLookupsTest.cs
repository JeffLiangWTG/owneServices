using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public class CusSeaManOBLHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsignees()
		{
			AssertEquals(typeof(CusSeaManOBLHeaderConsigneeCollection), lookups.Consignees.GetType());

			lookups.Parent.BO_RL_NKDischargePort = "AUSYD";
			AssertEquals("AUSYD", lookups.Consignees.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			lookups.Parent.BO_RL_NKDischargePort = "AUMEL";
			AssertEquals("AUMEL", lookups.Consignees.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestConsignors()
		{
			AssertEquals(typeof(CusSeaManOBLHeaderConsignorCollection), lookups.Consignors.GetType());

			lookups.Parent.BO_RL_NKLoadPort = "NZAKL";
			AssertEquals("NZAKL", lookups.Consignors.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			lookups.Parent.BO_RL_NKLoadPort = "AQMCM";
			AssertEquals("AQMCM", lookups.Consignors.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public virtual void TestMethodsOfPayment()
		{
			AssertEquals(typeof(CodeDescriptionPairList), lookups.MethodsOfPayment.GetType());
		}

		public virtual void TestCargoCodes()
		{
			AssertEquals(typeof(CodeDescriptionPairList), lookups.CargoCodes.GetType());
		}

		#region Implementation

		protected virtual CusSeaManOBLHeaderLookups GetNewLookups()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			return header.Lookups;
		}

		protected override void SetUp()
		{
			base.SetUp();

			lookups = GetNewLookups();
		}

		CusSeaManOBLHeaderLookups lookups;

		#endregion
	}
}
