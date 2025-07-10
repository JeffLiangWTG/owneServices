using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineDefaultCommodityCodeLookups : AutoRefAirlineDefaultCommodityCodeLookups
	{
		public RefAirlineDefaultCommodityCodeLookups(AutoRefAirlineDefaultCommodityCode parent) : base(parent)
		{
		}
		public ZString AirlineID
		{
			get
			{
				if (((AutoRefAirlineDefaultCommodityCode)Parent).Airline != null)
				{
					return ((AutoRefAirlineDefaultCommodityCode)Parent).Airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#region RDC_RAR_NKProductCode_List

		public CodeDescriptionPairList RDC_RAR_NKProductCode_List
		{
			get
			{
				return Factory.GetCachedValue($"RDC_RAR_NKProductCode_List{AirlineID}", () =>
				{
					var query = new ZQuery(RefAirlineProductCodeSchema.RAR_AirlineID, AirlineID);
					var productCodes = Factory.Load<RefAirlineProductCode>(query);
					var productList = new CodeDescriptionPairList();
					foreach (var code in productCodes)
					{
						productList.AddPair(code.RAR_Code, code.RAR_Description);
					}
					return productList;
				});
			}
		}

		#endregion

		#region RDC_RAC_NKCommodityCode_List

		public RefAirlineCommodityCodeCollection RDC_RAC_NKCommodityCode_List
		{
			get
			{
				var productPK = FindProductCodePK();
				return Factory.GetCachedValue($"RDC_RAC_NKCommodityCode_List{AirlineID}_{productPK}", () =>
				{
					var query = new ZDBOnlyQuery(typeof(RefAirlineCommodityCode));
					var subQuery = new ZDBOnlySubQuery(typeof(RefAirlineProductCodeCommodityCodePivot), RefAirlineCommodityCodeSchema.PK, RefAirlineProductCodeCommodityCodePivotSchema.RPC_RAC);
					subQuery.AddToFilter(RefAirlineProductCodeCommodityCodePivotSchema.RPC_RAR, productPK);
					query.AddSubQuery(subQuery, JoinCondition.And);

					return new RefAirlineCommodityCodeCollection(Factory, query);
				});
			}
		}

		ZGuid FindProductCodePK()
		{
			var query = new ZQuery();
			query.AddToFilter(RefAirlineProductCodeSchema.RAR_Code, ((AutoRefAirlineDefaultCommodityCode)Parent).RDC_RAR_NKProductCode);
			query.AddToFilter(RefAirlineProductCodeSchema.RAR_AirlineID, AirlineID);
			return Parent.Factory.LoadTop1<RefAirlineProductCode>(query)?.PK ?? ZGuid.Empty;
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion
	}
}
