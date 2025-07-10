using System;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	public interface IDatabaseService
	{
		(UNDGClassificationData item, UNDGSubstance substance, string errorMessage) Find(Guid id, string standard);
		(UNDGClassificationData classificationData, UNDGSubstance substance, string errorMessage) Find(UNDGDataItemDTO classificationData, string standard);
		void PreloadData(Guid[] ids, string[] standards);
		void PreloadData(UNDGDataItemDTO[] uNDGClassificationDatas);
	}
}
