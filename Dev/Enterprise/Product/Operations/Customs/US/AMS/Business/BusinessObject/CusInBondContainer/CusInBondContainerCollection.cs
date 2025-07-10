using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondContainerCollection : CusInBondContainerCollection<CusInBondContainer>, ISailingSynchronisationTargetCollection<BillOfLadingContainer, CusInBondContainer>
	{
		public CusInBondContainerCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}

		public ZDecimal TotalMonetaryValue
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var container in this)
				{
					if (!container.IsDeleted)
					{
						result += container.Commodities.TotalMonetaryValue;
					}
				}
				return result;
			}
		}

		public ZWeight TotalCargoWeight
		{
			get
			{
				var result = new ZWeight();
				var master = Master;
				if (master != null)
				{
					var bill = master.Bill;
					var convertedToBillWeightUQ = false;
					foreach (var container in this)
					{
						if (!container.IsDeleted)
						{
							var totalCargoWeight = container.Commodities.TotalCargoWeight;
							if (totalCargoWeight.IsValid)
							{
								if (result.IsEmpty)
								{
									result = totalCargoWeight;
								}
								else if (!totalCargoWeight.IsEmpty)
								{
									if (!convertedToBillWeightUQ && bill != null && bill.Weight.IsValid && result.Unit != totalCargoWeight.Unit)
									{
										result = new ZWeight(result.ConvertTo(bill.B0_WeightUQ), bill.B0_WeightUQ);
									}
									result += totalCargoWeight;
								}
							}
						}
					}
				}
				return result;
			}
		}

		public ZInt TotalManifestQty
		{
			get
			{
				var result = ZInt.Zero;
				foreach (var container in this)
				{
					if (!container.IsDeleted)
					{
						result += container.Commodities.TotalManifestQty;
					}
				}
				return result;
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var master = Master;
				return master != null && !master.IsDeleted && !master.ShouldSynchroniseWithConsol;
			}
		}

		CusInBondMoveDetail Master
		{
			get { return (CusInBondMoveDetail)Relationship.Master; }
		}
	}
}
