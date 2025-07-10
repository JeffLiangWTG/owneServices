using System;
using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public class DatabaseService : IDatabaseService
	{
		readonly BusinessObjectFactory _factory;
		public DatabaseService(BusinessObjectFactory factory)
		{
			this._factory = factory;
		}

		public (UNDGClassificationData item, UNDGSubstance substance, string errorMessage) Find(Guid id, string standard)
		{
			var item = _factory.Load<UNDGDataItem>(id);
			if (item == null)
			{
				var errorMessage = Res.GetString("29f1893e-faee-4f4f-b5cf-4967a789e84b", "Cannot find UNDG data item with PK = {0}", id);
				return (null, null, errorMessage);
			}

			var substance = FindSubstanceForItem(_factory, item, standard);
			if (substance == null)
			{
				var errorMessage = Res.GetString("80389e0c-1d01-4d5f-b761-5621789844d0", "Cannot find UNDG substance for standard {0} and UNDG data item with PK = {1}", standard, item.PK);
				return (null, null, errorMessage);
			}

			return (new UNDGClassificationData(item.DI_QuantityClassification), substance, string.Empty);
		}

		public (UNDGClassificationData classificationData, UNDGSubstance substance, string errorMessage) Find(UNDGDataItemDTO undgDataItemDto, string standard)
		{
			var substanceDto = undgDataItemDto.UNDGSubstanceDTOs.FirstOrDefault(x => x.Standard == standard);
			if (substanceDto == null)
			{
				var errorMessage = Res.GetString("649dfc87-d208-4388-ab74-ae33284e34f3", "Cannot find UNDG substance for standard {0}", standard);
				return (null, null, errorMessage);
			}

			var zQuery = new ZQuery(UNDGSubstanceSchema.DG_UNNO, substanceDto.Unno);
			zQuery.AddToFilter(UNDGSubstanceSchema.DG_Variant, substanceDto.Variant);
			zQuery.AddToFilter(UNDGSubstanceSchema.DG_Standard, substanceDto.Standard);
			var substance = _factory.LoadTop1<UNDGSubstance>(zQuery);
			if (substance == null)
			{
				var dataMissingErrorMessage = Res.GetString("17EB8D16-E36F-4DAD-89F8-7669CD6327E2", "Cannot find UNDG substance with UNNO = {0}, Variant = {1}, Standard = {2}", substanceDto.Unno, substanceDto.Variant, substanceDto.Standard);
				return (null, null, dataMissingErrorMessage);
			}

			return (undgDataItemDto.UNDGClassificationData, substance, string.Empty);
		}

		static UNDGSubstance FindSubstanceForItem(BusinessObjectFactory factory, UNDGDataItem item, string standard)
		{
			var substancePivot = item.UNDGSubstancePivotCollection.FirstOrDefault(x => x.DP_IsDefault && x.DP_Standard == standard);
			if (substancePivot == null)
			{
				return null;
			}

			return UNDGSubstanceLoader.LoadSubstance(factory, substancePivot);
		}

		public void PreloadData(UNDGDataItemDTO[] undgDataItemDtos)
		{
			foreach (var undgSubstanceDto in undgDataItemDtos.SelectMany(cd => cd.UNDGSubstanceDTOs).Distinct())
			{
				var zQuery = new ZQuery(UNDGSubstanceSchema.DG_UNNO, undgSubstanceDto.Unno);
				zQuery.AddToFilter(UNDGSubstanceSchema.DG_Variant, undgSubstanceDto.Variant);
				zQuery.AddToFilter(UNDGSubstanceSchema.DG_Standard, undgSubstanceDto.Standard);

				_factory.AddFetchHint(UNDGSubstanceSchema.Instance, zQuery);
			}
		}

		public void PreloadData(Guid[] ids, string[] standards)
		{
			_factory.AddFetchHint(UNDGDataItemSchema.Instance, new ZQuery(UNDGDataItemSchema.PK, ids));
			_factory.AddFetchHint(UNDGSubstancePivotSchema.Instance, new ZQuery(UNDGSubstancePivotSchema.DP_ParentId, ids));

			foreach (var id in ids)
			{
				var query = new ZQuery(UNDGSubstancePivotSchema.DP_ParentId, id);
				query.AddToFilter(new ZQuery(UNDGSubstancePivotSchema.DP_ParentTableCode, UNDGDataItemSchema.Constants.Prefix));
				var pivots = _factory.Load<UNDGSubstancePivot>(query);
				
				foreach (var standard in standards)
				{
					var pivot = pivots.FirstOrDefault(x => x.DP_IsDefault && x.DP_Standard == standard);
					if (pivot != null)
					{
						var zQuery = UNDGSubstanceLoader.BuildSubstanceQuery(pivot.DP_UNNO, pivot.DP_Variant, pivot.DP_Standard);
						_factory.AddFetchHint(UNDGSubstanceSchema.Instance, zQuery);
					}
				}
			}
		}
	}
}
