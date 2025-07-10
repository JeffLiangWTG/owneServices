//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesLookups
//
//    This class should be used for overriding collections in AutoOrgSalesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesLookups : AutoOrgSalesLookups
	{
		public OrgSalesLookups(AutoOrgSales parent)
			: base(parent)
		{
		}

		public new OrgSales Parent
		{
			get { return (OrgSales)base.Parent; }
		}

		#region Products

		public IOrgSalesProductCollection SalesProducts
		{
			get { return ObjectFactory.New<IOrgSalesProductCollection>(Factory); }
		}

		#endregion

		#region Suppliers

		public override OrgHeaderCollection Suppliers
		{
			get
			{
				if (fSuppliers == null)
				{
					fSuppliers = new ConsignorCollection(Factory);
				}

				return fSuppliers;
			}
		}

		OrgHeaderCollection fSuppliers;

		#endregion

		#region Buyers

		public override OrgHeaderCollection Buyers
		{
			get
			{
				if (fBuyers == null)
				{
					fBuyers = new ConsigneeCollection(Factory);
				}

				return fBuyers;
			}
		}

		OrgHeaderCollection fBuyers;

		#endregion

		#region Locations

		public ViewLocationCollection Locations
		{
			get
			{
				if (locations == null)
				{
					var parent = Parent;
					if (parent.Product != null)
					{
						locations = new ViewLocationCollection(Factory, parent.Product.AllowedLocationTypes);
					}
					else
					{
						locations = new ViewLocationCollection(Factory);
					}
				}
				return locations;
			}
		}

		ViewLocationCollection locations;

		#endregion

		#region Service Types

		public ICodeDescriptionPairList ServiceTypeList
		{
			get
			{
				var productCode = Parent.ProductCode;
				return GetServiceTypes(productCode);
			}
		}

		public CodeDescriptionPairList ServiceTypeInverseList
		{
			get
			{
				return Factory.GetCachedValue("OrgSalesLookups.ServiceTypeInverseList." + Parent.ProductCode, () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (ICodeDescription item in ServiceTypeList)
					{
						result.AddPair(item.Description, item.Code);
					}

					return result;
				});
			}
		}

		public static ICodeDescriptionPairList GetServiceTypes(ZString productCode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.Warehouse:
					return GetServiceTypes_Warehouse();

				default:
					return new CodeDescriptionPairList();
			}
		}

		static CodeDescriptionPairList GetServiceTypes_Warehouse()
		{
			return new OrgSalesWarehouseServiceTypesList();
		}

		#endregion

		#region Warehouses

		public BusinessObjectCollection Warehouses
		{
			get
			{
				var whsWarehouseCollectionType = ObjectFactory.GetType<IWhsWarehouseCollection>();
				var result = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, Factory);
				return result;
			}
		}

		#endregion

		#region ImpExpModeList

		public CodeDescriptionPairList ImpExpModeList
		{
			get
			{
				if (fImpExpModeList == null)
				{
					fImpExpModeList = new CodeDescriptionPairList(OLookUpEditType.SalesMode);
				}

				return fImpExpModeList;
			}
		}

		CodeDescriptionPairList fImpExpModeList;

		#endregion

		#region Unit Of Weight / Volume List

		public CodeDescriptionPairList UnitOfWeightList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		#endregion
	}
}
