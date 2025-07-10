using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class ProcessTaskCollectionFactoryFactory
	{
		public static TCollection GetOrCreateProcessTaskCollection<TBizo, TCollection>(this TBizo bizo, ProcessTaskCollectionCreator<TCollection> initCollection)
			where TBizo : BusinessObject
			where TCollection : ProcessTaskCollection
		{
			return GetService(bizo).GetOrCreate(bizo.PK, initCollection);
		}

		public static TCollection GetOrCreateProcessTaskCollectionWithKey<TBizo, TCollection, TAdditionalUniquenessConstraint>(this TBizo bizo, TAdditionalUniquenessConstraint key, ProcessTaskCollectionCreator<TCollection> initCollection)
			where TBizo : BusinessObject
			where TCollection : ProcessTaskCollection
			where TAdditionalUniquenessConstraint : IEquatable<TAdditionalUniquenessConstraint>
		{
			return GetService(bizo).GetOrCreate(bizo.PK, key, initCollection);
		}

		static IProcessTaskCollectionFactory GetService<TBizo>(TBizo bizo) where TBizo : BusinessObject
		{
			var factory = bizo.Factory;
			return factory.ServiceContainer.GetService<IProcessTaskCollectionFactory>() ?? factory.ServiceContainer.AddService(NewInstance);
		}

		public static IProcessTaskCollectionFactory NewInstance => new ProcessTaskCollectionFactory();
	}

	internal sealed class ProcessTaskCollectionFactory : IProcessTaskCollectionFactory
	{
		readonly Dictionary<ValueTuple<Type, ZGuid, Type>, object> processTaskCollections = new Dictionary<(Type, ZGuid, Type), object>();

		public TProcessTaskCollection GetOrCreate<TProcessTaskCollection, TAdditionalUniquenessConstraint>(ZGuid parentPK, TAdditionalUniquenessConstraint key, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc)
			where TProcessTaskCollection : ProcessTaskCollection
			where TAdditionalUniquenessConstraint : IEquatable<TAdditionalUniquenessConstraint>
		{
			TProcessTaskCollection processTaskCollection;

			if (!processTaskCollections.TryGetValue((typeof(TProcessTaskCollection), parentPK, typeof(TAdditionalUniquenessConstraint)), out var processTaskCollectionsByKey))
			{
				var processTaskCollectionsByKeyDict = new Dictionary<TAdditionalUniquenessConstraint, TProcessTaskCollection>();
				processTaskCollections.Add((typeof(TProcessTaskCollection), parentPK, typeof(TAdditionalUniquenessConstraint)), processTaskCollectionsByKeyDict);
				processTaskCollection = InitCollection(key, creatorFunc, processTaskCollectionsByKeyDict);
			}
			else
			{
				var processTaskCollectionsByKeyDict = (Dictionary<TAdditionalUniquenessConstraint, TProcessTaskCollection>)processTaskCollectionsByKey;

				if (!processTaskCollectionsByKeyDict.TryGetValue(key, out processTaskCollection))
				{
					processTaskCollection = InitCollection(key, creatorFunc, processTaskCollectionsByKeyDict);
				}
			}

			return processTaskCollection;
		}

		static TProcessTaskCollection InitCollection<TProcessTaskCollection, TAdditionalUniquenessConstraint>(TAdditionalUniquenessConstraint key, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc, Dictionary<TAdditionalUniquenessConstraint, TProcessTaskCollection> processTaskCollectionsByKeyDict)
			where TProcessTaskCollection : ProcessTaskCollection
			where TAdditionalUniquenessConstraint : IEquatable<TAdditionalUniquenessConstraint>
		{
			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var processTaskCollection = creatorFunc();
				processTaskCollectionsByKeyDict[key] = processTaskCollection;
				processTaskCollection.Load();
				return processTaskCollection;
			}
		}

		TProcessTaskCollection IProcessTaskCollectionFactory.GetOrCreate<TProcessTaskCollection>(ZGuid parentPK, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc)
		{
			return GetOrCreate(parentPK, new DefaultNoCompare(), creatorFunc);
		}

		struct DefaultNoCompare : IEquatable<DefaultNoCompare>
		{
			public bool Equals(DefaultNoCompare other)
			{
				return true;
			}

			public override int GetHashCode()
			{
				return 1;
			}

			public override bool Equals(object rhs)
			{
				return rhs is DefaultNoCompare;
			}
		}
	}
}
