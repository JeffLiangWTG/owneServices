using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public static class CusSCADataObjectHelper
	{
		public static IEnumerable<TCusSCAHouse> LoadHouseBills<TCusSCAHouse>(IColumnIndexer oceanBill, BusinessObjectFactory factory)
			where TCusSCAHouse : BaseCusSCAHouse
		{
			var query = new ZQuery(CusSCAHouseSchema.CA_CB, oceanBill.GetValue(CusSCAOceanBillSchema.PK));
			query.OrderBy = CusSCAHouseSchema.CA_HouseBill.Name;
			var houses = factory.Load<BaseCusSCAHouse>(query);
			return houses != null ? houses.OfType<TCusSCAHouse>() : Enumerable.Empty<TCusSCAHouse>();
		}

		public static TCusSCAHouse LoadHouseBill<TCusSCAHouse>(IColumnIndexer oceanBill, ZString houseBill, BusinessObjectFactory factory)
			where TCusSCAHouse : BaseCusSCAHouse
		{
			var oceanBillPk = oceanBill.GetValue(CusSCAOceanBillSchema.PK);
			var query = new ZQuery(CusSCAHouseSchema.CA_CB, oceanBillPk);
			query.AddToFilter(CusSCAHouseSchema.CA_HouseBill, houseBill);
			return factory.LoadTop1<TCusSCAHouse>(query);
		}

		public static IEnumerable<TCusSCAContainer> LoadContainers<TCusSCAContainer>(IColumnIndexer oceanBill, BusinessObjectFactory factory)
			where TCusSCAContainer : BaseCusSCAContainer
		{
			var query = new ZQuery(CusSCAContainerSchema.CN_CB, oceanBill.GetValue(CusSCAOceanBillSchema.PK));
			query.OrderBy = CusSCAContainerSchema.CN_ContainerNumber.Name;
			var containers = factory.Load<BaseCusSCAContainer>(query);
			return containers != null ? containers.OfType<TCusSCAContainer>() : Enumerable.Empty<TCusSCAContainer>();
		}

		public static IEnumerable<TCusSCAPivot> LoadPackages<TCusSCAPivot>(IColumnIndexer houseBill, BusinessObjectFactory factory)
			where TCusSCAPivot : BaseCusSCAPivot
		{
			var query = new ZQuery(CusSCAPivotSchema.CV_CA, houseBill.GetValue(CusSCAHouseSchema.PK));
			query.OrderBy = CusSCAPivotSchema.CV_LineNo.Name;
			var packages = factory.Load<BaseCusSCAPivot>(query);
			return packages != null ? packages.OfType<TCusSCAPivot>() : Enumerable.Empty<TCusSCAPivot>();
		}

		public static IEnumerable<TUNDG> LoadUNDGs<TUNDG>(IColumnIndexer parent, SchemaGuidColumn parentPKColumn, BusinessObjectFactory factory)
			where TUNDG : UNDGDataItem
		{
			var query = new ZQuery(UNDGDataItemSchema.DI_ParentID, parent.GetValue(parentPKColumn));
			var undgs = factory.Load<UNDGDataItem>(query);
			return undgs != null ? undgs.OfType<TUNDG>() : Enumerable.Empty<TUNDG>();
		}
	}
}
