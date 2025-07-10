using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityConstituent : AutoCommodityConstituent, ICusCodeDataTypeSupporter
	{
		public CommodityConstituent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public CommodityCodeCollection CommodityCodes
		{
			get
			{
				if (commodityCodes == null)
				{
					commodityCodes = new CommodityCodeCollection(this);
					commodityCodes.Load();
					RegisterEditableChildObject(commodityCodes);
				}

				return commodityCodes;
			}
		}
		CommodityCodeCollection commodityCodes;

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.CommodityConstituent|NZ_ConstituentName", Caption = "Name")]
		public override ZString NZ_ConstituentName { get => base.NZ_ConstituentName; set => base.NZ_ConstituentName = value; }

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.CommodityConstituent|NZ_ConstituentQty", Caption = "Quantity")]
		public override ZDecimal NZ_ConstituentQty { get => base.NZ_ConstituentQty; set => base.NZ_ConstituentQty = value; }

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityData, typeof(CommodityCode));
			return result;
		}

		#endregion
	}
}
