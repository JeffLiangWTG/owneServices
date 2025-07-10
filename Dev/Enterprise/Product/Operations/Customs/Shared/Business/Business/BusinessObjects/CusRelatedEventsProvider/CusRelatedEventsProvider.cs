using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusRelatedEventsProvider : Integration.Customs.Shared.ICusRelatedEventsProvider, Integration.Customs.Shared.ICusRelatedParentEventsProvider
	{
		public BusinessObject[] CusRelatedBusinessObjects(Integration.Forwarding.IForwardingShipment shipment, ZString referenceNumber)
		{
			cusHAWB = null;
			cusSCAHouse = null;
			cusSCAPivot = null;

			var result = new List<BusinessObject>();

			this.shipment = shipment as CommonShipment;
			if (this.shipment != null)
			{
				factory = this.shipment?.Factory;
				this.referenceNumber = referenceNumber;
				if (CusHAWB != null)
				{
					result.Add(CusHAWB);
				}
				if (CusSCAHouse != null)
				{
					result.Add(CusSCAHouse);
				}
				if (CusSCAPivot != null)
				{
					result.Add(CusSCAPivot);
				}
			}

			return result.Any() ? result.ToArray() : null;
		}

		public BusinessObject[] CusRelatedParentBusinessObjects(Integration.Forwarding.IForwardingShipment shipment)
		{
			cusMAWB = null;
			cusSCAOceanBill = null;

			var result = new List<BusinessObject>();

			this.shipment = shipment as CommonShipment;
			if (this.shipment != null)
			{
				factory = this.shipment?.Factory;
				if (CusMAWB != null)
				{
					result.Add(CusMAWB);
				}
				if (CusSCAOceanBill != null)
				{
					result.Add(CusSCAOceanBill);
				}
			}

			return result.Any() ? result.ToArray() : null;
		}

		protected CommonShipment shipment;
		protected BusinessObjectFactory factory;
		protected ZString referenceNumber;

		CusMAWB CusMAWB
		{
			get
			{
				if (cusMAWB == null && shipment?.ArrivalConsol != null)
				{
					if (shipment.ArrivalConsol != null)
					{
						var mawbQuery = new ZDBOnlyQuery(typeof(CusMAWB));
						mawbQuery.AddToFilter(CusMAWBSchema.CM_MAWB, shipment.ArrivalConsol.JK_MasterBillNum);
						mawbQuery.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, shipment.JS_HouseBill);
						mawbQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, AvailableApplicationCodes);

						var cusMAWBs = factory.Load<CusMAWB>(mawbQuery);
						if (cusMAWBs != null)
						{
							cusMAWB = cusMAWBs.OrderByDescending(mawb => mawb.CM_SystemCreateTimeUtc).FirstOrDefault();
						}
					}
				}

				return cusMAWB;
			}
		}
		CusMAWB cusMAWB;

		CusHAWB CusHAWB
		{
			get
			{
				if (cusHAWB == null && shipment?.ArrivalConsol != null)
				{
					var arrivalConsol = shipment.ArrivalConsol;
					if (arrivalConsol != null)
					{
						var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWB));
						hawbQuery.AddToFilter(CusHAWBSchema.CS_HAWB, referenceNumber);

						var mawbSubQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
						mawbSubQuery.AddToFilter(CusMAWBSchema.CM_MAWB, arrivalConsol.JK_MasterBillNum);
						mawbSubQuery.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, shipment.JS_HouseBill);
						mawbSubQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, AvailableApplicationCodes);

						var mawbDateQuery = GetDateOfFirstArrivalQuery(arrivalConsol);
						if (mawbDateQuery != null)
						{
							mawbSubQuery.AddToFilter(mawbDateQuery, JoinCondition.And);
						}
						hawbQuery.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.And);

						var cusHAWBs = factory.Load<CusHAWB>(hawbQuery);
						if (cusHAWBs != null)
						{
							cusHAWB = cusHAWBs.OrderByDescending(y => y.MAWB.CM_SystemCreateTimeUtc).FirstOrDefault();
						}
					}
				}

				return cusHAWB;
			}
		}
		CusHAWB cusHAWB;

		BaseCusSCAHouse CusSCAHouse
		{
			get
			{
				if (cusSCAHouse == null && shipment?.ArrivalConsol != null)
				{
					var arrivalConsol = shipment.ArrivalConsol;
					if (arrivalConsol != null)
					{
						var oceanSubQuery = new ZDBOnlySubQuery(typeof(BaseCusSCAOceanBill), CusSCAOceanBillSchema.PK);
						oceanSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, arrivalConsol.JK_MasterBillNum);
						oceanSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_MasterHouseBill, shipment.JS_HouseBill);
						oceanSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, AvailableApplicationCodes);

						var houseQuery = new ZDBOnlyQuery(typeof(BaseCusSCAHouse));
						houseQuery.AddToFilter(CusSCAHouseSchema.CA_HouseBill, referenceNumber);
						houseQuery.AddSubQuery(CusSCAHouseSchema.CA_CB, oceanSubQuery, JoinCondition.And);

						var cusSCAHouses = factory.Load<BaseCusSCAHouse>(houseQuery);
						cusSCAHouse = cusSCAHouses.FirstOrDefault(house => house.CusSCAPivotCollection.Any(pivot => shipment.Containers.OfType<AutoJobContainer>().Any(container => container.JC_ContainerNum == (pivot.Container?.CN_ContainerNumber ?? ZString.Empty))));
						if (cusSCAHouse == null)
						{
							cusSCAHouse = cusSCAHouses.FirstOrDefault();
						}
					}
				}

				return cusSCAHouse;
			}
		}
		BaseCusSCAHouse cusSCAHouse;

		BaseCusSCAOceanBill CusSCAOceanBill
		{
			get
			{
				if (cusSCAOceanBill == null && shipment?.ArrivalConsol != null)
				{
					if (shipment.ArrivalConsol != null)
					{
						var oceanBillQuery = new ZDBOnlyQuery(typeof(BaseCusSCAOceanBill));
						oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, shipment.ArrivalConsol.JK_MasterBillNum);
						oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_MasterHouseBill, shipment.JS_HouseBill);
						oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, AvailableApplicationCodes);

						var cusSCAOceanBills = factory.Load<BaseCusSCAOceanBill>(oceanBillQuery);
						if (cusSCAOceanBills != null)
						{
							cusSCAOceanBill = cusSCAOceanBills.OrderByDescending(sca => sca.CB_SystemCreateTimeUtc).FirstOrDefault();
						}
					}
				}

				return cusSCAOceanBill;
			}
		}
		BaseCusSCAOceanBill cusSCAOceanBill;

		BaseCusSCAPivot CusSCAPivot
		{
			get
			{
				if (cusSCAPivot == null && CusSCAHouse != null)
				{
					cusSCAPivot = CusSCAHouse.CusSCAPivotCollection.FirstOrDefault(pivot => shipment.Containers.OfType<AutoJobContainer>()
									.Any(container => container.JC_ContainerNum == (pivot.Container?.CN_ContainerNumber ?? ZString.Empty)));
				}
				return cusSCAPivot;
			}
		}
		BaseCusSCAPivot cusSCAPivot;

		protected ZQuery GetDateOfFirstArrivalQuery(CommonConsol consol)
		{
			ZQuery result = null;
			var dateOfFirstArrival = consol.IsAir && consol.JK_RL_NKDischargePort.StartsWith(shipment.CurrentCountryCode, StringComparison.OrdinalIgnoreCase) ? consol.JK_DatePortOfFirstArrival : ZDateTime.Empty;
			if (!dateOfFirstArrival.IsValid)
			{
				foreach (var transport in consol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder))
				{
					if (transport.IsAir && !transport.JW_RL_NKLoadPort.StartsWith(shipment.CurrentCountryCode, StringComparison.OrdinalIgnoreCase) && transport.JW_RL_NKDiscPort.StartsWith(shipment.CurrentCountryCode, StringComparison.OrdinalIgnoreCase))
					{
						dateOfFirstArrival = !transport.JW_ATA.IsEmpty ? transport.JW_ATA : transport.JW_ETA;
						break;
					}
				}
			}
			if (dateOfFirstArrival.IsValid)
			{
				result = new ZQuery(CusMAWBSchema.CM_ArrivalDate, ZDateTime.Empty);
				var recycledMAWBtime = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
				var mawbDateSubQuery = new ZQuery(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, dateOfFirstArrival.AddMonths(-recycledMAWBtime));
				mawbDateSubQuery.AddToFilter(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, dateOfFirstArrival.AddMonths(recycledMAWBtime));
				result.AddToFilter(mawbDateSubQuery, JoinCondition.Or);
			}
			return result;
		}

		public IEnumerable<ZString> AvailableApplicationCodes => GetAvailableApplicationCodes();

		protected virtual IEnumerable<ZString> GetAvailableApplicationCodes() => Enumerable.Empty<ZString>();
	}
}
