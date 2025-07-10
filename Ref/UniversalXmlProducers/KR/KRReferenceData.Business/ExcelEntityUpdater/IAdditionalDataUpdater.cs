using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public interface IAdditionalDataUpdater<T> where T : RefDataRepoModelEntityType
	{
		void UpdateAdditionally(T dataEntity, IRow row, EntityConfiguration configuration);
		void UpdateRule(T dataEntity, Rule rule);
		bool IsDataRowValid(IRow row, EntityConfiguration configuration);
		void RegisterUpdated(IRow row, EntityConfiguration configuration);
	}

	enum Relationship { OR, AND };
}
