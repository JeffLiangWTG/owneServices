using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderCollectionView : BusinessObjectCollectionView<CusSeaManOBLHeader>
	{
		public CusSeaManOBLHeaderCollectionView(CusSeaManOBLHeaderCollection collection) : base(collection)
		{
		}

		ZString fDischargePort;
		public ZString DischargePort
		{
			get
			{
				return fDischargePort;
			}
			set
			{
				if (DischargePort != value)
				{
					fDischargePort = value;
					Rebuild();
				}
			}
		}

		#region Implementation

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			CusSeaManOBLHeader header = child as CusSeaManOBLHeader;
			if (!DischargePort.IsEmpty)
			{
				header.BO_RL_NKDischargePort = DischargePort;
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			CusSeaManOBLHeader header = element as CusSeaManOBLHeader;
			return DischargePort.IsEmpty || header.BO_RL_NKDischargePort == DischargePort;
		}

		#endregion
	}
}
