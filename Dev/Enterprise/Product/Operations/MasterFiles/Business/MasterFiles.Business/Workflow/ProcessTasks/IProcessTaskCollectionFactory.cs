using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public delegate TProcessTaskCollection ProcessTaskCollectionCreator<TProcessTaskCollection>();
	public interface IProcessTaskCollectionFactory : IService
	{
		TProcessTaskCollection GetOrCreate<TProcessTaskCollection>(ZGuid parentPK, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc)
			where TProcessTaskCollection : ProcessTaskCollection;

		TProcessTaskCollection GetOrCreate<TProcessTaskCollection, TAdditionalUniquenessConstraint>(ZGuid parentPK, TAdditionalUniquenessConstraint key, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc)
			where TProcessTaskCollection : ProcessTaskCollection
			where TAdditionalUniquenessConstraint : IEquatable<TAdditionalUniquenessConstraint>;
	}
}
