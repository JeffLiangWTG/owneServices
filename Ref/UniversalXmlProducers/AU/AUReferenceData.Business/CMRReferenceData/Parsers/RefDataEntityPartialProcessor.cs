using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public abstract class RefDataEntityPartialProcessor<T> : IPartialProcessor where T : RefDataRepoModelEntityType
	{
		public void Parse(IXmlWriter xmlWriter, string content, ICMRReferenceDataPartialParser parser)
		{
			var changeReports = new List<ChangeReport>();

			PropertyMapping<ChangeReport>[] changeReportMappings =
			[
				new(entity => entity.ActionIndicator, parser.ActionIndicatorPosition, 1)
			];

			var changeConverter = new LineToEntityConverter<ChangeReport>(changeReportMappings, 1);

			foreach (var line in content.NonEmptyLines())
			{
				var code = parser.ProcessLine(line);
				if (code != null)
				{
					var change = changeConverter.Convert(line);
					change.Entity = code;
					changeReports.Add(change);
				}
			}

			ApplyUpdatingChanges(changeReports, parser);

			foreach (var code in changeReports)
			{
				xmlWriter.PopulateData(code.Entity);
			}
		}

		void ApplyUpdatingChanges(List<ChangeReport> changeReports, ICMRReferenceDataPartialParser parser)
		{
			var publishedDate = parser.PublishedDate;
			var updatingChanges = changeReports.Where(x => x.ActionIndicator is Constants.ChangeReportActions.Modify or Constants.ChangeReportActions.Delete).ToArray();
			var keySelector = GetEntityKeySelector();
			var updatingChangeEntityKeys = updatingChanges.Select(c => keySelector((T)c.Entity)).ToArray();

			if (updatingChanges.Any())
			{
				var existingEntities = LoadExistingEntities(updatingChangeEntityKeys);

				foreach (var change in updatingChanges)
				{
					var entity = (T)change.Entity;
					existingEntities.TryGetValue(keySelector(entity), out var existing);

					switch (change.ActionIndicator)
					{
						case Constants.ChangeReportActions.Delete:
							if (existing == null || ShouldSkipDeleteEntity(entity, existing, publishedDate))
							{
								// change has already been applied. Move on to next change.
								changeReports.Remove(change);
							}
							else
							{
								DeleteEntity(entity, publishedDate);
							}
							break;
						case Constants.ChangeReportActions.Modify when existing != null:
						{
							if (ShouldSkipUpdateEntity(entity, existing, publishedDate))
							{
								// change has already been applied. Move on to next change.
								changeReports.Remove(change);
							}
							else
							{
								UpdateEntity(entity, existing, publishedDate);
							}
							break;
						}
					}
				}
			}
		}

		public abstract Func<T, string> GetEntityKeySelector();
		public abstract Dictionary<string, T> LoadExistingEntities(string[] keys);
		public abstract bool ShouldSkipDeleteEntity(T entity, T existing, DateTime publishedDate);
		public abstract void DeleteEntity(T entity, DateTime publishedDate);
		public abstract bool ShouldSkipUpdateEntity(T entity, T existing, DateTime publishedDate);
		public abstract void UpdateEntity(T entity, T existing, DateTime publishedDate);
	}
}
