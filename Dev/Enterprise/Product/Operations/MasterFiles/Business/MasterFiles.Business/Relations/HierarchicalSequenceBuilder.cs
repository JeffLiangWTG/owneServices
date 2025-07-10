using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class HierarchicalSequenceBuilder
	{
		#region GetRelationshipSequence

		public static IEnumerable<T> GetRelationshipSequence<T>(Func<T, IEnumerable<T>> getRelatedEntitiesDelegate, T fromEntity, T targetAncestor, params Tuple<T, T>[] fromToRelationshipsToIgnore)
			where T : IIdentified
		{
			var relationshipSequence = new Stack<T>();
			BuildRelationshipSequence(getRelatedEntitiesDelegate, relationshipSequence, fromEntity, targetAncestor, new HashSet<ZGuid>(), fromToRelationshipsToIgnore);
			return new Stack<T>(relationshipSequence);
		}

		static bool BuildRelationshipSequence<T>(Func<T, IEnumerable<T>> getRelatedEntitiesDelegate, Stack<T> sequenceStack, T currentEntity, T targetRelatedEntity, HashSet<ZGuid> previouslyTraversedEntityPks, Tuple<T, T>[] relationshipsToIgnore)
			where T : IIdentified
		{
			sequenceStack.Push(currentEntity);
			if (currentEntity.Identifier == targetRelatedEntity.Identifier)
			{
				return true;
			}
			previouslyTraversedEntityPks.Add(currentEntity.Identifier);

			var relatedEntities =
				from
					relatedEntity in getRelatedEntitiesDelegate(currentEntity)
				where
					!previouslyTraversedEntityPks.Contains(relatedEntity.Identifier) &&
					!relationshipsToIgnore.Any(link => (link.Item1.Identifier == currentEntity.Identifier && link.Item2.Identifier == relatedEntity.Identifier))
				select
					relatedEntity;

			foreach (var relatedEntity in relatedEntities)
			{
				if (BuildRelationshipSequence(getRelatedEntitiesDelegate, sequenceStack, relatedEntity, targetRelatedEntity, previouslyTraversedEntityPks, relationshipsToIgnore))
				{
					return true;
				}
			}

			sequenceStack.Pop();
			return false;
		}

		#endregion
	}
}
