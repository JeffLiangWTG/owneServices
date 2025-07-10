using System.Collections.Generic;
using CargoWise.Types;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IAdditionalAddInfoGroupCollectionDataObjectReader
	{
		void Process(IEnumerable<UniversalCustoms.AddInfoGroup> addInfoGroupCollection, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase);
	}
}
