using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IStagingDataWrapperFactory
	{
		IEnumerable<IStagingDataWrapper> CreateWrapper(object stagingObject);
		IEnumerable<Type> GetExpriableKeyRelatedStagingEntityTypes(Type stagingType);
	}
}
