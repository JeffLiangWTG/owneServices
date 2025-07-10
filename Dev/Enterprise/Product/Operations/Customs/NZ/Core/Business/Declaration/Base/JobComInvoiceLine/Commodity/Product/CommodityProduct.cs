using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityProduct : AutoCommodityProduct, ICusCodeDataTypeSupporter
	{
		public CommodityProduct(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public CommodityProductDataCollection CommodityProducts
		{
			get
			{
				if (commodityProducts == null)
				{
					commodityProducts = new CommodityProductDataCollection(this);
					commodityProducts.Load();
					RegisterEditableChildObject(commodityProducts);
				}

				return commodityProducts;
			}
		}
		CommodityProductDataCollection commodityProducts;

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.CommodityProduct|NZ_ProductID", Caption = "ID")]
		public override ZString NZ_ProductID { get => base.NZ_ProductID; set => base.NZ_ProductID = value; }

		[List(nameof(AddInfoLookups) + "." + nameof(NZCommodityProductAddInfoLookups.IDTypeList))]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.CommodityProduct|NZ_ProductIDType", Caption = "ID Type")]
		public override ZString NZ_ProductIDType
		{
			get { return base.NZ_ProductIDType; }
			set { base.NZ_ProductIDType = value; }
		}

		#region ICusCodeDataTypeSupporter Members
		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData, typeof(CommodityProductData));
			return result;
		}

		#endregion
	}
}
