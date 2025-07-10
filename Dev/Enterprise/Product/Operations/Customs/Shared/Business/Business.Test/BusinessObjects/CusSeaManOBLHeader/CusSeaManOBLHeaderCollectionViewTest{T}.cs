using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusSeaManOBLHeaderCollectionViewTest<T> : BusinessObjectCollectionViewTestCase<T> where T : CusSeaManOBLHeaderCollectionView
	{
		public void TestAddNewReallyAddsToCollection()
		{
			View.AddNew();
			AssertEquals("View.Count", 1, View.Count);
		}

		public void TestSettingDischargePortsAddsToCollection()
		{
			AssertEquals("View.Count", 0, View.Count);
			CusSeaManOBLHeader oB = TranHeader.OceanBills.AddNew();
			oB.BO_RL_NKDischargePort = "AUSYD";
			AssertEquals("View.Count", 1, View.Count);
		}

		public void TestSettingDischargePortsRemovesFromCollection()
		{
			CusSeaManOBLHeader oB = TranHeader.OceanBills.AddNew();
			oB.BO_RL_NKDischargePort = "AUSYD";
			AssertEquals("View.Count", 1, View.Count);
			oB.BO_RL_NKDischargePort = "AUMEL";
			AssertEquals("View.Count", 0, View.Count);
		}

		public void TestChangingTheDischargePortRebuilds()
		{
			CusSeaManOBLHeader oB = TranHeader.OceanBills.AddNew();
			oB.BO_RL_NKDischargePort = "AUMEL";
			AssertEquals("View.Count", 0, View.Count);
			View.DischargePort = "AUMEL";
			AssertEquals("View.Count", 1, View.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusSeaManOBLHeader result = Factory.New<CusSeaManOBLHeader>();
			result.BO_RL_NKDischargePort = "AUSYD";
			return result;
		}

		CusSeaManOBLHeaderCollectionView fView;
		protected CusSeaManOBLHeaderCollectionView View
		{
			get
			{
				if (fView == null)
				{
					fView = new CusSeaManOBLHeaderCollectionView(TranHeader.OceanBills);
					fView.DischargePort = "AUSYD";
				}
				return fView;
			}
		}

		CusSeaManTranHead fTranHeader;
		CusSeaManTranHead TranHeader
		{
			get
			{
				if (fTranHeader == null)
				{
					fTranHeader = Factory.New<CusSeaManTranHead>();
				}
				return fTranHeader;
			}
		}

		#endregion
	}
}
