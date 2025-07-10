using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderConsigneeCollection))]
	sealed class CusSeaManOBLHeaderConsigneeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilterBusinessObjectDefaults()
		{
			AssertEquals("AUSYD", Header.Lookups.Consignees.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals(true, Header.Lookups.Consignees.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"].Value);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Header.Lookups.Consignees;
		}

		#region Implementation

		CusSeaManOBLHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.New<CusSeaManOBLHeader>();
					fHeader.BO_RL_NKDischargePort = "AUSYD";
					fHeader.BO_FreightForwarderIndicator = true;
				}

				return fHeader;
			}
		}
		CusSeaManOBLHeader fHeader;

		#endregion
	}
}
