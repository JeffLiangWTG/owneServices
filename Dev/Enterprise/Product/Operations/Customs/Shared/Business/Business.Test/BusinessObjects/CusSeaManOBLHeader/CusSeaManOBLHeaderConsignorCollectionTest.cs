using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderConsignorCollection))]
	sealed class CusSeaManOBLHeaderConsignorCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilterBusinessObjectDefaults()
		{
			AssertEquals("NZAKL", Header.Lookups.Consignors.FilterBusinessObjectDefaults[OrgConstants.FilterControl.UNLOCOType.OrgPort + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Header.Lookups.Consignors;
		}

		#region Implementation

		CusSeaManOBLHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.New<CusSeaManOBLHeader>();
					fHeader.BO_RL_NKLoadPort = "NZAKL";
				}

				return fHeader;
			}
		}
		CusSeaManOBLHeader fHeader;

		#endregion
	}
}
