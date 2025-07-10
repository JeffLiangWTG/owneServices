using System.Collections.Generic;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		IEnumerable<UniversalCustoms.AddInfoGroup> CreateCollection();
	}
}
