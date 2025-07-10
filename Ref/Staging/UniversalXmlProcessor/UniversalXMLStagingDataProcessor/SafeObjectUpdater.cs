using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Validation;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class SafeObjectUpdater : ISafeObjectUpdater
	{
		public SafeObjectUpdater(ISafeDataProvider safeDataProvider, IMetadataProvider metadataProvider, IOverlappingCalculator overlappingCalculator, bool isDeletionType)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));

			this.safeDataProvider = safeDataProvider;
			this.metadataProvider = metadataProvider;
			this.overlappingCalculator = overlappingCalculator;
			this.isDeletionType = isDeletionType;
			noEffectiveDateChecker = new NoEffectiveDateRangeChecker();
			enableExpirableChecker = new EnableExpirableChecker(metadataProvider);
			utcNow = DateTime.UtcNow.ToUTCDateTimeOffset();
		}

		readonly ISafeDataProvider safeDataProvider;
		readonly NoEffectiveDateRangeChecker noEffectiveDateChecker;
		readonly EnableExpirableChecker enableExpirableChecker;
		readonly IMetadataProvider metadataProvider;
		readonly IOverlappingCalculator overlappingCalculator;
		readonly bool isDeletionType;
		readonly DateTimeOffset utcNow;

		public SafeObjectUpdaterResult[] Update(IStagingDataWrapper[] wrappers)
		{
			Argument.NotNull(wrappers, nameof(wrappers));
			var results = new SafeObjectUpdaterResult[0];
			if (wrappers.Length > 0)
			{
				var stagingTypeName = wrappers[0].GetStagingTypeName();
				var safeObjType = typeof(RefCusTariff).GetTypeFromBaseType(stagingTypeName);
				var safeObjs = safeDataProvider.GetData(safeObjType, wrappers, metadataProvider)?.ToArray();
				if (isDeletionType)
				{
					Delete(wrappers, safeObjType, safeObjs);
				}
				else
				{
					results = UpdateOrInsert(wrappers, safeObjType, safeObjs, null, null, null).Values.ToArray();
				}
			}
			return results;
		}

		[SuppressMessage("Microsoft.Contracts", "Enum-50-0")]
		[SuppressMessage("Microsoft.Contracts", "Enum-367-0")]
		[SuppressMessage("Microsoft.Contracts", "Enum-388-0")]
#if DEBUG
		public
#endif
		int GetIdenticalLevel(object safeObj, Type safeObjType, IStagingDataWrapper wrapper)
		{
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrapper, nameof(wrapper));

			var result = (IdenticalLevel)safeDataProvider.GetIdenticalLevel(safeObj, safeObjType, wrapper, metadataProvider);
			if (result != IdenticalLevel.HasChange)
			{
				foreach (var relatedEntityType in wrapper.GetRelatedEntityTypesAndFKs())
				{
					var relatedSafeEntityType = typeof(RefCusTariff).GetTypeFromBaseType(relatedEntityType.Item1);
					if (!DataProviderHelper.IsExpirableType(relatedSafeEntityType))
					{
						var relatedEntities = wrapper.GetRelatedEntities(relatedEntityType.Item1).ToArray();
						if (relatedEntities.Length > 0)
						{
							var relatedSafeObjs = safeDataProvider.GetRelatedData(safeObj, safeObjType, relatedSafeEntityType.Name).ToArray();
							if (relatedEntities.Length != relatedSafeObjs.Length)
							{
								return (int)IdenticalLevel.HasChange;
							}
							foreach (var relatedEntity in relatedEntities)
							{
								var safeRelatedObjs = safeDataProvider.GetNewestObjectFromList(relatedSafeObjs, relatedSafeEntityType, relatedEntity, metadataProvider);
								if (safeRelatedObjs == null || safeRelatedObjs.Keys.Count == 0)
								{
									return (int)IdenticalLevel.HasChange;
								}
								foreach (var safeRelatedObj in safeRelatedObjs.OrderBy(x => x.Key).First().Value)
								{
									var identicalLevel = (IdenticalLevel)GetIdenticalLevel(safeRelatedObj, relatedSafeEntityType, relatedEntity);
									if (identicalLevel == IdenticalLevel.HasChange)
									{
										return (int)IdenticalLevel.HasChange;
									}
									else
									{
										result = (int)result > (int)identicalLevel ? identicalLevel : result;
									}
								}
							}
						}
					}
				}
			}
			return (int)result;
		}

#pragma warning disable CA1502 // Avoid excessive complexity
		Dictionary<Guid, SafeObjectUpdaterResult> UpdateOrInsert(IStagingDataWrapper[] originalWrappers, Type safeObjType, object[] safeObjs, object parentObj, Guid? expirableAncestorPK, Guid? datasetPK = null)
#pragma warning restore CA1502 // Avoid excessive complexity
		{
			Argument.NotNull(safeObjs, nameof(safeObjs));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(originalWrappers, nameof(originalWrappers));

			var safeObjsListToSearch = new List<object>(safeObjs);
			var result = new Dictionary<Guid, SafeObjectUpdaterResult>();
			var safeObjTablePrefix = safeObjType.GetTablePrefix();
			var wrappers = originalWrappers.AsEnumerable();
			var deletedPKList = new List<Guid>();

			if (!wrappers.First().IsData)  // IsData = False
			{
				foreach (var wrapper in wrappers)
				{
					var safeObjsListToProcessAfterUpdate = new List<(ResultAction, object)>();
					var safeObjsListToExpire = new List<(object expiredObj, Guid newObjectPk)>();
					var keyOrderAndSafeNewestsObjects = safeDataProvider.GetNewestObjectFromList(safeObjsListToSearch.ToArray(), safeObjType, wrapper, metadataProvider);
					var safeNewestsObjectsWithKeyorder = GetSafeNewestsObjectsFromKeyOrderAndSafeNewestsObjects(keyOrderAndSafeNewestsObjects, deletedPKList);
					var safeNewestsObjects = safeNewestsObjectsWithKeyorder.Item1;
					var keyOrder = safeNewestsObjectsWithKeyorder.Item2;
					if (safeNewestsObjects.Any())
					{
						foreach (var newestObject in safeNewestsObjects)
						{
							if (!safeDataProvider.IsUserOverride(safeObjType, newestObject))
							{
								safeDataProvider.Update(newestObject, safeObjType, wrapper, metadataProvider, IdenticalLevel.Identical, keyOrder);
								safeObjsListToProcessAfterUpdate.Add((ResultAction.Update, newestObject));
							}
							else
							{
								DataProviderHelper.AddUpdaterResult(result, newestObject, safeObjTablePrefix, ResultAction.Update, expirableAncestorPK, null, datasetPK ?? newestObject.GetPKValue());
							}
						}
					}
					if (safeObjsListToProcessAfterUpdate.Count > 0)
					{
						ProcessSafeObjectsAfterUpdate(wrapper, safeObjType, safeObjsListToProcessAfterUpdate, result, expirableAncestorPK, datasetPK);
					}
				}
			}
			else if (DataProviderHelper.IsExpirableType(safeObjType)) // Process expirabletype
			{
				if (DataSetStructureProvider.StructuredDataSets.Select(x => x.First()).Contains(safeObjType.Name))
				{
					wrappers = wrappers.OrderBy(x => x.GetDateTimeRange().StartDate);
					foreach (var wrapper in wrappers)
					{
						enableExpirableChecker.ValidateWrapper(wrapper);
						result = UpdateOrInsertCore(wrapper, safeObjType, safeObjsListToSearch, deletedPKList, result, parentObj, expirableAncestorPK, datasetPK);
					}
				}
				else
				{
					var expirableSafeObjs = EnableExpirableChecker.ShouldValidateEnableExpirable(safeObjType) ? safeObjs.Where(x => x.GetValue($"{safeObjTablePrefix}_{Constants.StartDatePropertySuffix}") != null) : safeObjs;
					var existingFutureSafeObjToSearch = expirableSafeObjs.Where(x => safeDataProvider.GetDateTimeRange(safeObjType, x).StartDate > utcNow).ToList();
					var existingNonFutureSafeObjToSearch = expirableSafeObjs.Where(x => safeDataProvider.GetDateTimeRange(safeObjType, x).StartDate <= utcNow).ToList();
					var newFutureSafeObjListToSearch = new List<object>();

					foreach (var wrapper in wrappers.OrderBy(x => x.GetWrapperStartDateTime(safeObjTablePrefix)))
					{
						var safeObjsListToProcessAfterUpdate = new List<(ResultAction, object)>();
						var wrapperDateRange = HandleNewFutureOverlapping(wrapper, safeObjType, newFutureSafeObjListToSearch, deletedPKList);
						var isIdentical = HandleExistFutureOverlapping(wrapper, safeObjType, existingFutureSafeObjToSearch, deletedPKList, safeObjsListToProcessAfterUpdate);
						if (isIdentical)
						{
							ProcessSafeObjectsAfterUpdate(wrapper, safeObjType, safeObjsListToProcessAfterUpdate, result, expirableAncestorPK, datasetPK);
						}
						else
						{
							result = UpdateOrInsertCore(wrapper, safeObjType, existingNonFutureSafeObjToSearch, deletedPKList, result, parentObj, expirableAncestorPK, datasetPK, newFutureSafeObjListToSearch);
						}
					}
				}
			}
			else // Process non expirabletype
			{
				foreach (var wrapper in wrappers)
				{
					var safeObjsListToProcessAfterUpdate = new List<(ResultAction, object)>();
					var keyOrderAndSafeNewestsObjects = safeDataProvider.GetNewestObjectFromList(safeObjsListToSearch.ToArray(), safeObjType, wrapper, metadataProvider);
					var safeNewestsObjectsWithKeyorder = GetSafeNewestsObjectsFromKeyOrderAndSafeNewestsObjects(keyOrderAndSafeNewestsObjects, deletedPKList);
					var safeNewestsObjects = safeNewestsObjectsWithKeyorder.Item1;
					var keyOrder = safeNewestsObjectsWithKeyorder.Item2;
					if (safeNewestsObjects.Any())
					{
						var deletedPKs = safeDataProvider.DeleteConflictedRecordsByOrder(safeObjType, keyOrder, keyOrderAndSafeNewestsObjects); // TODO: WI00905386 - Not delete immediately during merging
						if (deletedPKs.Any())
						{
							deletedPKList.AddRange(deletedPKs);
						}

						foreach (var newestObject in safeNewestsObjects)
						{
							enableExpirableChecker.ValidateNewestObjectFromSafeDb(newestObject);
							if (!safeDataProvider.IsUserOverride(safeObjType, newestObject))
							{
								var identicalLevel = GetIdenticalLevel(newestObject, safeObjType, wrapper);
								if ((IdenticalLevel)identicalLevel == IdenticalLevel.Identical)
								{
									safeObjsListToProcessAfterUpdate.Add((ResultAction.Update, newestObject));
								}
								else
								{
									Update(wrapper, newestObject, safeObjType, identicalLevel, safeObjsListToProcessAfterUpdate, keyOrder);
								}
							}
							else
							{
								DataProviderHelper.AddUpdaterResult(result, newestObject, safeObjTablePrefix, ResultAction.Update, expirableAncestorPK, null, datasetPK ?? newestObject.GetPKValue());
							}
						}
					}
					else
					{
						Create(wrapper, safeObjType, safeObjsListToSearch, parentObj, safeObjsListToProcessAfterUpdate);
					}

					if (safeObjsListToProcessAfterUpdate.Count > 0)
					{
						ProcessSafeObjectsAfterUpdate(wrapper, safeObjType, safeObjsListToProcessAfterUpdate, result, expirableAncestorPK, datasetPK);
					}
				}
			}

			return result;
		}

#pragma warning disable CA1502 // Avoid excessive complexity
		Dictionary<Guid, SafeObjectUpdaterResult> UpdateOrInsertCore(IStagingDataWrapper wrapper, Type safeObjType, List<object> safeObjListToSearch, List<Guid> deletedPKList, Dictionary<Guid, SafeObjectUpdaterResult> result, object parentObj, Guid? expirableAncestorPK, Guid? datasetPK = null, List<object> newFutureSafeObjListToSearch = null)
#pragma warning restore CA1502 // Avoid excessive complexity
		{
			var safeObjTablePrefix = safeObjType.GetTablePrefix();
			var safeObjsListToProcessAfterUpdate = new List<(ResultAction, object)>();
			var safeObjsListToExpire = new List<(object expiredObj, Guid newObjectPk)>();

			var keyOrderAndSafeNewestsObjects = safeDataProvider.GetNewestObjectFromList(safeObjListToSearch.ToArray(), safeObjType, wrapper, metadataProvider);
			var safeNewestsObjectsWithKeyorder = GetSafeNewestsObjectsFromKeyOrderAndSafeNewestsObjects(keyOrderAndSafeNewestsObjects, deletedPKList);
			var safeNewestsObjects = safeNewestsObjectsWithKeyorder.Item1;
			var keyOrder = safeNewestsObjectsWithKeyorder.Item2;
			var wrapperDateRange = wrapper.GetDateTimeRange();

			if (safeNewestsObjects.Any())
			{
				var deletedPKs = safeDataProvider.DeleteConflictedRecordsByOrder(safeObjType, keyOrder, keyOrderAndSafeNewestsObjects);
				if (deletedPKs.Any())
				{
					deletedPKList.AddRange(deletedPKs);
				}

				foreach (var newestObject in safeNewestsObjects)
				{
					enableExpirableChecker.ValidateNewestObjectFromSafeDb(newestObject);
					noEffectiveDateChecker.IsValid(newestObject);

					if (!safeDataProvider.IsUserOverride(safeObjType, newestObject))
					{
						var identicalLevel = GetIdenticalLevel(newestObject, safeObjType, wrapper);
						if ((IdenticalLevel)identicalLevel == IdenticalLevel.Identical)
						{
							safeObjsListToProcessAfterUpdate.Add((ResultAction.Update, newestObject));
						}
						else if (!DataProviderHelper.IsExpirableType(safeObjType))
						{
							Update(wrapper, newestObject, safeObjType, identicalLevel, safeObjsListToProcessAfterUpdate, keyOrder);
						}
						else
						{
							var safeDateRange = safeDataProvider.GetDateTimeRange(safeObjType, newestObject);
							if (safeDataProvider.IsExpirable(safeObjType, safeDateRange, wrapperDateRange) && ((IdenticalLevel)identicalLevel == IdenticalLevel.HasChange))
							{
								var newObjPK = newFutureSafeObjListToSearch != null && wrapperDateRange.StartDate > utcNow ? Create(wrapper, safeObjType, newFutureSafeObjListToSearch, parentObj, safeObjsListToProcessAfterUpdate)
									: Create(wrapper, safeObjType, safeObjListToSearch, parentObj, safeObjsListToProcessAfterUpdate);
								if (safeDateRange.EndDate > wrapperDateRange.StartDate)
								{
									safeObjsListToExpire.Add((newestObject, newObjPK));
								}
							}
							else
							{
								if (metadataProvider.GetKeys(safeObjType.Name).All(o => o.Operation == Operations.Equals) && (DataProviderHelper.IsExpirableType(safeObjType) && !safeDataProvider.ShouldOverWriteAndNotExpire(safeObjType)))
								{
									if (overlappingCalculator.IsOverlapped(new[] { safeDateRange, wrapperDateRange }))
									{
										Update(wrapper, newestObject, safeObjType, identicalLevel, safeObjsListToProcessAfterUpdate, keyOrder);
									}
									else
									{
										var objectListForOverlappingCheckDictionary = safeDataProvider.GetNewestObjectFromList(safeObjListToSearch.ToArray(), safeObjType, wrapper, metadataProvider, true);
										var objectListForOverlappingCheck = Enumerable.Empty<object>();
										var allDateRanges = new List<Tuple<DateTimeRange, Guid>>();
										if (objectListForOverlappingCheckDictionary != null && objectListForOverlappingCheckDictionary.Keys.Count > 0)
										{
											objectListForOverlappingCheck = objectListForOverlappingCheckDictionary.OrderBy(x => x.Key).First().Value;
											foreach (var safeObj in objectListForOverlappingCheck)
											{
												noEffectiveDateChecker.IsValid(safeObj);
												var tempDateRange = safeDataProvider.GetDateTimeRange(safeObjType, safeObj);
												if (tempDateRange != null)
												{
													allDateRanges.Add(Tuple.Create(tempDateRange, safeObj.GetPKValue()));
												}
											}
										}

										if (wrapperDateRange != null)
										{
											if (allDateRanges.Select(x => x.Item1).Contains(wrapperDateRange))
											{
												var specifiedObject = safeDataProvider.GetSpecifiedDateTimeRangeObjectFromList(objectListForOverlappingCheck.ToArray(), safeObjType, wrapperDateRange, wrapper, metadataProvider);

												if (specifiedObject != null)
												{
													var iLevel = GetIdenticalLevel(specifiedObject, safeObjType, wrapper);
													if ((IdenticalLevel)iLevel == IdenticalLevel.Identical)
													{
														safeObjsListToProcessAfterUpdate.Add((ResultAction.Update, specifiedObject));
													}
													else
													{
														Update(wrapper, specifiedObject, safeObjType, iLevel, safeObjsListToProcessAfterUpdate, keyOrder);
													}
												}
											}
											else
											{
												var overlappingChecker = new OverlappingChecker(wrapper, overlappingCalculator, objectListForOverlappingCheck, wrapperDateRange, allDateRanges);
												if (overlappingChecker.IsValid())
												{
													Create(wrapper, safeObjType, safeObjListToSearch, parentObj, safeObjsListToProcessAfterUpdate);
												}
											}
										}
									}
								}
								else
								{
									Update(wrapper, newestObject, safeObjType, identicalLevel, safeObjsListToProcessAfterUpdate, keyOrder);
								}
							}
						}
					}
					else
					{
						DataProviderHelper.AddUpdaterResult(result, newestObject, safeObjTablePrefix, ResultAction.Update, expirableAncestorPK, null, datasetPK ?? newestObject.GetPKValue());
					}
				}
			}
			else
			{
				Create(wrapper, safeObjType, safeObjListToSearch, parentObj, safeObjsListToProcessAfterUpdate);
			}

			if (safeObjsListToExpire.Count > 0)
			{
				safeObjsListToExpire.ForEach((item) =>
				{
					safeDataProvider.Expire(item.expiredObj, safeObjType, wrapper, metadataProvider);
					DataProviderHelper.AddUpdaterResult(result, item.expiredObj, safeObjTablePrefix, ResultAction.Expire, expirableAncestorPK, item.newObjectPk, datasetPK ?? item.newObjectPk);
				});
			}

			if (safeObjsListToProcessAfterUpdate.Count > 0)
			{
				ProcessSafeObjectsAfterUpdate(wrapper, safeObjType, safeObjsListToProcessAfterUpdate, result, expirableAncestorPK, datasetPK);
			}

			return result;
		}

		void Update(IStagingDataWrapper wrapper, object safeObj, Type safeObjType, int identicalLevel, List<(ResultAction, object)> safeObjsListToProcessAfterUpdate, int keyOrder = 0)
		{
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObj, nameof(safeObj));
			safeDataProvider.Update(safeObj, safeObjType, wrapper, metadataProvider, (IdenticalLevel)identicalLevel, keyOrder);
			safeObjsListToProcessAfterUpdate.Add((ResultAction.Update, safeObj));
		}

		Guid Create(IStagingDataWrapper wrapper, Type safeObjType, List<object> safeObjsListToSearch, object parentObj, List<(ResultAction, object)> safeObjsListToProcessAfterUpdate)
		{
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObjsListToSearch, nameof(safeObjsListToSearch));
			var objResult = safeDataProvider.Create(safeObjType, wrapper, parentObj, safeObjsListToSearch, metadataProvider);
			safeObjsListToProcessAfterUpdate.Add((ResultAction.Insert, objResult));
			safeObjsListToSearch.Add(objResult);
			return objResult.GetPKValue();
		}

		DateTimeRange HandleNewFutureOverlapping(IStagingDataWrapper wrapper, Type safeObjType, IEnumerable<object> newFutureSafeObjListToSearch, List<Guid> deletedPKList)
		{
			enableExpirableChecker.ValidateWrapper(wrapper);
			var wrapperDateRange = wrapper.GetDateTimeRange();
			var keyOrderedRelatedNewFutureRecords = safeDataProvider.GetNewestObjectFromList(newFutureSafeObjListToSearch.ToArray(), safeObjType, wrapper, metadataProvider, true);
			var relatedNewFutureRecordsWithKeyorder = GetSafeNewestsObjectsFromKeyOrderAndSafeNewestsObjects(keyOrderedRelatedNewFutureRecords, deletedPKList);
			var relatedNewFutureRecords = relatedNewFutureRecordsWithKeyorder.Item1;
			var allNewFutureDateRanges = relatedNewFutureRecords.Select(x => new Tuple<DateTimeRange, Guid>(safeDataProvider.GetDateTimeRange(safeObjType, x), x.GetPKValue()));
			var overlappingCheckerNewFuture = new OverlappingChecker(wrapper, overlappingCalculator, relatedNewFutureRecords, wrapperDateRange, allNewFutureDateRanges);
			overlappingCheckerNewFuture.IsValid();
			return wrapperDateRange;
		}

		bool HandleExistFutureOverlapping(IStagingDataWrapper wrapper, Type safeObjType, List<object> existingFutureSafeObjToSearch, List<Guid> deletedPKList, List<(ResultAction, object)> safeObjsListToProcessAfterUpdate)
		{
			var wrapperDateRange = wrapper.GetDateTimeRange();
			var keyOrderedRelatedExistFutureRecords = safeDataProvider.GetNewestObjectFromList(existingFutureSafeObjToSearch.ToArray(), safeObjType, wrapper, metadataProvider, true);
			var relatedExistFutureRecordsWithKeyorder = GetSafeNewestsObjectsFromKeyOrderAndSafeNewestsObjects(keyOrderedRelatedExistFutureRecords, deletedPKList);
			var relatedExistFutureRecords = relatedExistFutureRecordsWithKeyorder.Item1;
			var allExistFutureDateRanges = relatedExistFutureRecords.Select(x => new Tuple<DateTimeRange, Guid>(safeDataProvider.GetDateTimeRange(safeObjType, x), x.GetPKValue()));
			if (overlappingCalculator.IsOverlapped(allExistFutureDateRanges.Select(x => x.Item1), wrapperDateRange))
			{
				foreach (var safeObj in relatedExistFutureRecords)
				{
					var safeObjDateRange = safeDataProvider.GetDateTimeRange(safeObjType, safeObj);
					if (safeObjDateRange.Equals(wrapperDateRange))
					{
						var iLevel = GetIdenticalLevel(safeObj, safeObjType, wrapper);
						if ((IdenticalLevel)iLevel == IdenticalLevel.Identical)
						{
							safeObjsListToProcessAfterUpdate.Add((ResultAction.Update, safeObj));
							return true;
						}
					}
					if (overlappingCalculator.IsOverlapped([safeObjDateRange], wrapperDateRange))
					{
						existingFutureSafeObjToSearch.Remove(safeObj);
						CascadeDelete(safeObjType, safeObj);
					}
				}
			}
			return false;
		}

		void ProcessSafeObjectsAfterUpdate(IStagingDataWrapper wrapper, Type safeObjType, IEnumerable<(ResultAction, object)> safeObjs, Dictionary<Guid, SafeObjectUpdaterResult> results, Guid? expirableAncestorPK, Guid? datasetPK)
		{
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(safeObjs, nameof(safeObjs));
			Argument.NotNull(results, nameof(results));

			var isExpirableType = DataProviderHelper.IsExpirableType(safeObjType);
			var safeObjTblPrefix = safeObjType.GetTablePrefix();
			foreach (var safeObj in safeObjs)
			{
				var safeObjPK = safeObj.Item2.GetPKValue();
				DataProviderHelper.AddUpdaterResult(results, safeObj.Item2, safeObjTblPrefix, safeObj.Item1, expirableAncestorPK, null, datasetPK ?? safeObjPK);
				var newExpirableAncestorPK = isExpirableType ? safeObj.Item2.GetPKValue() : expirableAncestorPK;

				foreach (var relatedEntityType in wrapper.GetRelatedEntityTypesAndFKs(true))
				{
					var relatedEntities = wrapper.GetRelatedEntities(relatedEntityType.Item1).ToArray();
					if (relatedEntities.Length > 0)
					{
						Array.ForEach(relatedEntities, x => x.SetWrapperValue(relatedEntityType.Item2, safeObjPK));
						var relatedSafeEntityType = typeof(RefCusTariff).GetTypeFromBaseType(relatedEntityType.Item1);
						var relatedSafeObjs = safeDataProvider.GetRelatedData(safeObj.Item2, safeObjType, relatedEntityType.Item1).ToArray();
						results.MergeIfNotExists(UpdateOrInsert(relatedEntities, relatedSafeEntityType, relatedSafeObjs, safeObj.Item2, newExpirableAncestorPK, datasetPK ?? safeObjPK));
					}
				}
			}
		}

		void Delete(IStagingDataWrapper[] wrappers, Type safeObjType, object[] safeObjs)
		{
			Argument.NotNull(safeObjs, nameof(safeObjs));
			Argument.NotNull(safeObjType, nameof(safeObjType));
			Argument.NotNull(wrappers, nameof(wrappers));

			var safeObjsListToSearch = new List<object>(safeObjs);
			foreach (var wrapper in wrappers)
			{
				var safeNewestsObjectsDictionary = safeDataProvider.GetNewestObjectFromList(safeObjsListToSearch.ToArray(), safeObjType, wrapper, metadataProvider);
				if (safeNewestsObjectsDictionary != null && safeNewestsObjectsDictionary.Keys.Count > 0)
				{
					foreach (var order in safeNewestsObjectsDictionary.Keys)
					{
						var safeNewestsObjects = safeNewestsObjectsDictionary[order];
						foreach (var newestObject in safeNewestsObjects)
						{
							DeleteRelatedEntities(wrapper, safeObjType, newestObject);

							if (wrapper.IsData)
							{
								Console.WriteLine($"Deleting {safeObjType.Name} {newestObject.GetPKValue()}.");
								safeDataProvider.Delete(newestObject, safeObjType);
							}
						}
					}
				}
			}
		}

		void DeleteRelatedEntities(IStagingDataWrapper wrapper, Type parentObjType, object parentObj)
		{
			Argument.NotNull(wrapper, nameof(wrapper));
			Argument.NotNull(parentObjType, nameof(parentObjType));
			Argument.NotNull(parentObj, nameof(parentObj));

			foreach (var relatedEntityType in wrapper.GetRelatedEntityTypesAndFKs(true))
			{
				var relatedEntities = wrapper.GetRelatedEntities(relatedEntityType.Item1).ToArray();
				var relatedSafeEntityType = typeof(RefCusTariff).GetTypeFromBaseType(relatedEntityType.Item1);
				var relatedSafeObjs = safeDataProvider.GetRelatedData(parentObj, parentObjType, relatedEntityType.Item1).ToArray();
				Delete(relatedEntities, relatedSafeEntityType, relatedSafeObjs);
			}
		}

		static public Dictionary<int, IEnumerable<object>> GetUndeletedNewestObjects(Dictionary<int, IEnumerable<object>> keyOrderAndSafeObjectsDictioanry, List<Guid> deletedPKList)
		{
			Argument.NotNull(keyOrderAndSafeObjectsDictioanry, nameof(keyOrderAndSafeObjectsDictioanry));
			Argument.NotNull(deletedPKList, nameof(deletedPKList));

			var filteredMatches = keyOrderAndSafeObjectsDictioanry.ToDictionary(kv => kv.Key, kv => kv.Value.Where(v => !deletedPKList.Contains(v.GetPKValue())))
				.Where(kv => kv.Value.Any()).ToDictionary(kv => kv.Key, kv => kv.Value);
			return filteredMatches;
		}

		static (IEnumerable<object>, int) GetSafeNewestsObjectsFromKeyOrderAndSafeNewestsObjects(Dictionary<int, IEnumerable<object>> keyOrderAndSafeNewestsObjects, List<Guid> deletedPKList)
		{
			keyOrderAndSafeNewestsObjects = GetUndeletedNewestObjects(keyOrderAndSafeNewestsObjects, deletedPKList);
			var keyOrder = 0;
			var safeNewestsObjects = Enumerable.Empty<object>();
			if (keyOrderAndSafeNewestsObjects.Keys.Count > 0)
			{
				keyOrder = keyOrderAndSafeNewestsObjects.Keys.OrderBy(x => x).First();
				safeNewestsObjects = keyOrderAndSafeNewestsObjects[keyOrder];
			}
			return (safeNewestsObjects, keyOrder);
		}

		void CascadeDelete(Type safeObjType, object safeObj)
		{
			Argument.NotNull(safeObj, nameof(safeObj));
			Argument.NotNull(safeObjType, nameof(safeObjType));

			foreach (var relatedEntityTypeName in metadataProvider.GetProperties(safeObjType.Name).Where(x => x.IndexOf("_", StringComparison.Ordinal) < 0))
			{
				var relatedSafeEntityType = typeof(RefCusTariff).GetTypeFromBaseType(relatedEntityTypeName);
				var relatedSafeObjs = safeDataProvider.GetRelatedData(safeObj, relatedEntityTypeName).ToArray();
				foreach (var relatedSafeObj in relatedSafeObjs)
				{
					CascadeDelete(relatedSafeEntityType, relatedSafeObj);
				}
			}
			Console.WriteLine($"Deleting {safeObjType.Name} {safeObj.GetPKValue()}.");
			safeDataProvider.Delete(safeObj, safeObjType);
		}
	}
}
