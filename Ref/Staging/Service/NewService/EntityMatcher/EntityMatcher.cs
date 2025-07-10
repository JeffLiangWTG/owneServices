using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Fastenshtein;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public class EntityMatcher : IEntityMatcher
	{
		public EntityMatcher(IStagingRepository repo, IEntityRecognition entityRecognition)
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(entityRecognition, nameof(entityRecognition));

			this.repo = repo;
			this.entityRecognition = entityRecognition;
		}
		readonly IStagingRepository repo;
		readonly IEntityRecognition entityRecognition;

		public IEnumerable<MatchingCode> GetBestMatchingCodes(EntityClass entityClass, string language, bool useRecognition, bool ignoreCase, params string[] entityNames)
		{
			if (language != "EN")
			{
				throw new System.NotSupportedException("Non-English is not supported yet.");
			}
			var allEntities = repo.Get<NamedEntityClassification>().Where(x => x.NEC_Class == entityClass.ToString()
				&& x.NEC_Language == language).ToArray();

			foreach (var entity in entityNames)
			{
				var score = int.MaxValue;
				NamedEntityClassification match = null;

				score = GetBestMatchingScore(ignoreCase, allEntities, entity, score, ref match);
				if (score == 0)
				{
					yield return new MatchingCode { Score = score, Result = match.NEC_Code };
					continue;
				}
				if (useRecognition)
				{
					var names = entityRecognition.GetNamedEntities(entity);
					foreach (var name in names)
					{
						score = GetBestMatchingScore(ignoreCase, allEntities, name, score, ref match);
						if (score == 0)
						{
							break;
						}
					}
				}
				if (match != null)
				{
					if ((entityClass == EntityClass.CURRENCY || entityClass == EntityClass.CACUSTOMUOM) && score > 1)
					{
						continue;
					}

					yield return new MatchingCode { Score = score, Result = match.NEC_Code };
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Keep original logic")]
		static int GetBestMatchingScore(bool ignoreCase, NamedEntityClassification[] allEntities, string levWord, int score, ref NamedEntityClassification match)
		{
			Argument.NotNull(allEntities, nameof(allEntities));
			var lev = new Levenshtein(ignoreCase ? levWord.ToLowerInvariant() : levWord);
			foreach (var entity in allEntities)
			{
				var newScore = lev.DistanceFrom(ignoreCase ? entity.NEC_Name.ToLowerInvariant() : entity.NEC_Name);
				if (score > newScore)
				{
					match = entity;
					score = newScore;
				}
				if (score == 0)
				{
					return score;
				}
			}
			return score;
		}
	}
}
