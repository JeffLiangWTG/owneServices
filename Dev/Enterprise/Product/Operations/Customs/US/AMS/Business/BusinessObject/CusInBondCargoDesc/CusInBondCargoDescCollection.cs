using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondCargoDescCollection : CusInBondCargoDescCollection<CusInBondCargoDesc>,
		ISailingSynchronisationTargetCollection<BillOfLadingPackLine, CusInBondCargoDesc>,
		ISailingSynchronisationTargetCollection<AgencyShipmentContainer, CusInBondCargoDesc>,
		IOverrideDefaultValuesCollection
	{
		public CusInBondCargoDescCollection(CusInBondContainer master)
			: base(master)
		{
			header = master.Header;
			tariffFormatter = new TariffFormatter();
		}

		readonly CusInBondHeader header;

		public CusInBondCargoDesc this[ZString tariff]
		{
			get
			{
				tariff = tariffFormatter.Format(tariff);
				CusInBondCargoDesc result = null;
				foreach (var commodity in this)
				{
					if (!commodity.IsDeleted && commodity.BY_HarmonisedTariff == tariff)
					{
						result = commodity;
						break;
					}
				}
				return result;
			}
		}

		public ZInt TotalPieceCount
		{
			get
			{
				var result = ZInt.Zero;
				foreach (var commodity in this)
				{
					if (!commodity.IsDeleted)
					{
						result += commodity.BY_PieceCount;
					}
				}
				return result;
			}
		}

		public ZWeight TotalCargoWeight
		{
			get
			{
				var billWeight = Master.Bill?.Weight ?? ZWeight.Invalid;
				var result = new ZWeight();
				var convertedToBillWeightUQ = false;
				foreach (var commodity in this)
				{
					if (!commodity.IsDeleted)
					{
						var weight = commodity.Weight;
						if (result.IsEmpty || !result.IsValid)
						{
							result = weight;
						}
						else if (!weight.IsEmpty)
						{
							if (!convertedToBillWeightUQ && billWeight.IsValid && result.Unit != weight.Unit)
							{
								result = new ZWeight(result.ConvertTo(billWeight.Unit), billWeight.Unit);
								convertedToBillWeightUQ = true;
							}
							result += weight;
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
				foreach (var commodity in this)
				{
					if (!commodity.IsDeleted)
					{
						result += commodity.BY_PieceCount;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalMonetaryValue
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var commodity in this)
				{
					if (!commodity.IsDeleted)
					{
						result += commodity.BY_MonetaryValue;
					}
				}
				return result;
			}
		}

		bool IOverrideDefaultValuesCollection.IsOverrideDefaultValuesEnabled => header.Consol != null;
		ZPropertyInfoBool IOverrideDefaultValuesCollection.OverrideDefaultValuesInfo => (ZPropertyInfoBool)header.BH_OverrideFreightDefaultsInfo;

		#region Implementation

		protected override bool AllowNew
		{
			get
			{
				var master = Master;
				return master != null && !master.IsDeleted && !master.ShouldSynchronise;
			}
		}

		protected override void SetDefaultsForNewElementCore(CusInBondCargoDesc newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Count == 0)
			{
				DefaultFromBill(newElement);
			}
			else if (Count > 0)
			{
				DefaultFromPreviousCommodity(newElement, GetPreviousCommodity());
			}
		}

		CusInBondCargoDesc GetPreviousCommodity()
		{
			CusInBondCargoDesc result = null;
			for (var i = Count - 1; i >= 0; i--)
			{
				var commodity = this[i];
				if (!commodity.IsDeleted)
				{
					result = commodity;
					break;
				}
			}
			return result;
		}

		void DefaultFromPreviousCommodity(CusInBondCargoDesc newCommodity, CusInBondCargoDesc previousCommodity)
		{
			if (previousCommodity == null)
			{
				DefaultFromBill(newCommodity);
			}
			else
			{
				newCommodity.BY_GrossWeightUnit = previousCommodity.BY_GrossWeightUnit;
				newCommodity.BY_ManifestUnitCode = previousCommodity.BY_ManifestUnitCode;
				newCommodity.BY_RN_NKCountryOfOrigin = previousCommodity.BY_RN_NKCountryOfOrigin;
				DefaultWeightAndManifestValue(newCommodity);
			}
		}

		void DefaultWeightAndManifestValue(CusInBondCargoDesc newCommodity)
		{
			var bill = newCommodity.Bill;
			if (bill != null)
			{
				var moveDetail = bill.MovementDetail;
				if (moveDetail != null)
				{
					var containers = moveDetail.Containers;
					if (bill.B0_Weight > ZDecimal.Zero)
					{
						var billWeight = bill.Weight;
						if (billWeight.IsValid)
						{
							var commodityWeight = newCommodity.Weight;
							var totalCargoWeight = containers.TotalCargoWeight;
							var totalCargoWeightValue = commodityWeight.IsValid && totalCargoWeight.IsValid ? totalCargoWeight.ConvertTo(newCommodity.BY_GrossWeightUnit) : ZDecimal.Zero;
							newCommodity.BY_GrossWeight = Math.Max(0, (newCommodity.BY_GrossWeightUnit == billWeight.Unit ? billWeight.Amount : (commodityWeight.IsValid ? billWeight.ConvertTo(commodityWeight.Unit) : ZDecimal.Zero)) - totalCargoWeightValue);
						}
					}
					if (bill.B0_ManifestQty > ZInt.Zero)
					{
						var totalManifestQty = containers.TotalManifestQty;
						newCommodity.BY_PieceCount = Math.Max(0, bill.B0_ManifestQty - totalManifestQty);
					}
				}
			}
		}

		void DefaultFromBill(CusInBondCargoDesc newCommodity)
		{
			var bill = newCommodity.Bill;
			if (bill != null)
			{
				newCommodity.BY_GrossWeightUnit = bill.B0_WeightUQ;
				newCommodity.BY_ManifestUnitCode = bill.B0_ManifestUQ.Left(3);
				var portOfLading = bill.PortOfLading;
				if (portOfLading != null)
				{
					newCommodity.BY_RN_NKCountryOfOrigin = portOfLading.RL_RN_NKCountryCode;
				}
				DefaultWeightAndManifestValue(newCommodity);
			}
		}

		CusInBondContainer Master
		{
			get { return (CusInBondContainer)Relationship.Master; }
		}

		readonly TariffFormatter tariffFormatter;

		#endregion
	}
}
