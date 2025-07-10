using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Workflow.Business
{
	public class WorkflowSetFieldProcessor : IWorkflowSetFieldProcessor
	{
		public WorkflowSetFieldProcessor(IWorkflowTriggerActionSource source)
		{
			this.source = source;
			validation = new WorkflowMacroSetFieldValidation(Action);

			var triggerAction = ((ITriggerAction)Action);
			areFailuresErrors = triggerAction.RunLocation == TriggerRunLocation.Client;
			verboseLogs = triggerAction.RunLocation != TriggerRunLocation.Client;

			actionNotifications = new NotificationCollection();
			IsSet = false;
		}

		readonly IWorkflowTriggerActionSource source;

		IProcessTaskNotification Action => source.Action;
		IBaseTrigger Trigger => source.Trigger;
		BusinessObject workflowProvider => source.Job;

		IReadOnlyCollection<IWorkflowSetFieldResult> EmptyResults => emptyResults ?? (emptyResults = Array.AsReadOnly(Array.Empty<WorkflowSetFieldResult>()));
		IReadOnlyCollection<IWorkflowSetFieldResult> emptyResults;

		readonly bool areFailuresErrors;
		readonly bool verboseLogs;

		readonly WorkflowMacroValidation validation;

		readonly NotificationCollection actionNotifications;
		bool IsSet;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			using (workflowProvider?.Factory.ServiceContainer.GetService<ITextMacroProcessingCachingService>()?.WithNoCaching())
			{
				var results = EmptyResults;
				try
				{
					if (!HasNullErrors())
					{
						var actionRoots = ObjectFactory.Get<ITriggerActionRootProvider>().GetRoots(Action, workflowProvider, source.Event);

						var trigger = actionRoots.FirstOrDefault(x => typeof(ProcessTask).IsAssignableFrom(x.GetType())) as IBaseTrigger;
						var actionRoot = actionRoots.FirstOrDefault(x => IsTriggeringEventPropertyProviderOfIStmALog(x));
						var triggeringEvent = GetTriggeringEventFromActionRoot(actionRoot);

						using ((workflowProvider as IRegisterStatusChangeContext)?.TemporarilySetStatusChangedByTriggerEvent(triggeringEvent))
						{
							var (value, errors) = actionRoots.Any() ? new WorkflowMacroEvaluator(actionNotifications, new WorkflowMacroValidation(), actionRoots).EvaluateMacros(actionRoots, Action.PQ_FieldValue) : (null, new List<IReportError>());
							if (value == null)
							{
								var msg = Res.GetString("fbd3b0fd-8f98-4db4-aaf4-efafdf0818f6", "Set field ({0}) macro evaluated to null. Macro: [{1}] Source: [{2}]",
									Action.PQ_TriggerType, Action.PQ_FieldValue, SourcePrettyPrint(actionRoots));
								NotifyWarning(actionNotifications, msg);
							}
							else if (errors.Any(error => error.Type.Severity < NotificationType.Information.Severity) && value.IsEmpty)
							{
								var msg = Res.GetString("e7386d48-36a7-40c7-9171-a0a302e6cb84", "Set field ({0}) macro evaluated with errors. Macro: [{1}] Source: [{2}]",
									Action.PQ_TriggerType, Action.PQ_FieldValue, SourcePrettyPrint(actionRoots));
								NotifyWarning(actionNotifications, msg);
							}
							else
							{
								try
								{
									var getFieldNameNotifications = new ProcessTaskNotificationValidation.ValidationNotifications();
									var (actionFieldPropertyInfo, actionFieldNameParentType, fieldName) = GetFinalPropertyInfoAndParentType(Action, getFieldNameNotifications, fieldValue: value);
									if (actionFieldPropertyInfo == null && actionFieldNameParentType == null)
									{
										actionNotifications.AddRange(getFieldNameNotifications);
									}
									else
									{
										if (actionFieldNameParentType != null && actionRoots.Length > 0)
										{
											var nullRootIsThere = actionRoots.Where(x => x == null || actionFieldNameParentType.IsAssignableFrom(x.GetType())).DefaultIfEmpty().Select(x => x == null).First();
											if (nullRootIsThere)
											{
												var warningMessageBuilder = new StringBuilder((NoResString)"<<<NullRoot Happened V2>>>\r\nAction Roots:\r\n");
												warningMessageBuilder.AppendFormat((NoResString)"Field Value: (Type:{0}, Value:{1})\r\n", value?.GetType()?.FullName, value);
												warningMessageBuilder.AppendFormat((NoResString)"Result of GetFinalPropertyInfoAndParentType: ({0}, {1}, {2})\r\n", actionFieldPropertyInfo?.ToString(), actionFieldNameParentType.FullName, fieldName);
												foreach (var item in actionRoots)
												{
													warningMessageBuilder.AppendFormat((NoResString)"\t- (Type:{0}, Value:{1})\r\n", item == null ? "null" : item.GetType().Name, item?.ToString());
												}

												var actionBO = Action as BusinessObject ?? throw new InvalidOperationException("Action which is an IProcessTaskNotification must be of type BusinessObject.");
												warningMessageBuilder.AppendFormat((NoResString)"Action:(ID:{0}, Type:{1}, FieldName:{2}, FieldValue: {3}, IsInDatabase:{4}, IsInDeletion: {5}, IsDeleted: {6}, IsDeleting: {7})\r\n", actionBO.PK, Action.GetType().FullName, Action.PQ_FieldName, Action.PQ_FieldValue, actionBO.IsInDatabase, actionBO.IsInDeletion(), actionBO.IsDeleted, actionBO.IsDeleting);
												warningMessageBuilder.AppendFormat((NoResString)"Workflow Provider:(ID:{0}, Type:{1}, IsInDatabase:{2}, IsInDeletion: {3}, IsDeleted: {4}, IsDeleting: {5})\r\n ", workflowProvider.PK, workflowProvider.GetType().FullName, workflowProvider.IsInDatabase, workflowProvider.IsInDeletion(), workflowProvider.IsDeleted, workflowProvider.IsDeleting);

												actionNotifications.AddWarning(warningMessageBuilder.ToString());
											}
										}

										var fieldBizo = actionFieldNameParentType != null
											? actionRoots.FirstOrDefault(x => x != null && actionFieldNameParentType.IsAssignableFrom(x.GetType())) ??
											  workflowProvider
											: workflowProvider;

										results = TrySetProperty(fieldBizo, actionRoots, fieldName, value, actionNotifications, Action.PQ_FieldName);
									}
									IsSet = true;
								}
								catch (TargetInvocationException ex)
								{
									var invalidOperationException = ex.Find<InvalidOperationException>();
									if (invalidOperationException != null && invalidOperationException.Message.StartsWith(StmNote.TextOnNonTextNoteExceptionMessage, StringComparison.Ordinal))
									{
										var msg = Res.GetString("09f6fab8-4774-449e-9eec-8c5d0e4298ba", "Parent: {0}\r\nProperty Path: {1}", workflowProvider.GetType().ToString(), Action.PQ_FieldNameTrimmed);
										NotifyWarning(actionNotifications, msg);
									}
									else
									{
										throw;
									}
								}
							}
						}
					}
				}
				catch (WorkflowMacroEvaluationException e)
				{
					var msg = Res.GetString("3bf84d93-5da6-492e-a6e6-3a1377983ef6", "IFC/FLD Action was forbidden by internal rules.\r\nParent: {0}\r\nProperty Path: {1}\r\nError: {2}", workflowProvider.GetType().ToString(), Action.PQ_FieldNameTrimmed, e.MultilingualMessage);
					actionNotifications.AddError(msg);
				}

				SetFieldTriggerResults.AddResult(Trigger, this, notifications, workflowProvider, results);
			}
		}

		static IStmALog GetTriggeringEventFromActionRoot(IBusiness obj)
		{
			if (obj == null)
			{
				return null;
			}

			var propertyName = nameof(TriggeringEventPropertyProvider<IStmALog>.TriggeringEvent);
			var property = obj.GetType().GetProperty(propertyName);
			return property?.GetValue(obj) as IStmALog;
		}

		static bool IsTriggeringEventPropertyProviderOfIStmALog(IBusiness obj)
		{
			if (obj == null)
			{
				return false;
			}

			var objType = obj.GetType();
			if (!objType.IsGenericType)
			{
				return false;
			}

			var genericType = objType.GetGenericTypeDefinition();
			if (genericType != typeof(TriggeringEventPropertyProvider<>))
			{
				return false;
			}

			var genericArgument = objType.GetGenericArguments()[0];
			if (typeof(IStmALog).IsAssignableFrom(genericArgument))
			{
				return true;
			}

			return false;
		}

		bool HasNullErrors()
		{
			if (Action == null)
			{
				NotifyWarning(actionNotifications, Res.GetString("e843a89a-1dac-4750-a125-4962b9066628", "FLD/IFC action not found"));
			}
			else if (Action.Parent == null)
			{
				NotifyWarning(actionNotifications, Res.GetString("ef84b6f3-0611-4f40-aa82-d7961d6b3ea4", "{0} action parent not found", Action.PQ_TriggerType));
			}
			else if (workflowProvider == null)
			{
				NotifyWarning(actionNotifications, Res.GetString("38430118-3679-41fd-99d8-0f98fd9bb44e", "No parent found for Trigger"));
			}
			else
			{
				return false;
			}
			return true;
		}

		public void LogNotifcations(INotifications notifications, BusinessObject parent, IReadOnlyCollection<IWorkflowSetFieldResult> results)
		{
			var isClient = Action.RunLocation == TriggerRunLocation.Client;
			if (!isClient)
			{
				WorkflowTriggerActionRunnerLogs.FiringAction(Action, notifications);
			}

			foreach (var log in actionNotifications)
			{
				notifications.Add(log);
			}

			if (IsSet)
			{
				if (results.Count == 0)
				{
					var msg = Res.GetString("69a16932-2f2d-4769-b0d7-9a380b4cd1f4", "Set field ({0}) target not found. Macro [{1}] Target [{2}] ", Action.PQ_TriggerType, Action.PQ_FieldValue, parent.ToString());
					if (areFailuresErrors)
					{
						notifications.AddError(msg);
					}
					else
					{
						NotifyWarning(notifications, msg);
					}
				}
				else
				{
					var successCount = 0;
					var unchangedCount = 0;
					var successes = new List<string>();
					var unchanged = new List<string>();
					var generalWarnings = new List<string>();
					var failureReasons = new List<string>();

					const int MaxSuccessMessages = 5; // Limit success messages because we don't want too much log spam.
					const int MaxUnchangedMessages = 1; // Limit unchanged messages because they will all be the same probably.

					foreach (var result in results)
					{
						switch (result.Status)
						{
							case WorkflowSetFieldStatus.Success:
								successCount++;
								if (successes.Count < MaxSuccessMessages)
								{
									successes.Add(Res.GetString("48f9fbac-7aa5-47d3-913d-c14f6570c463", "Target Object: '{0}', Source: {1}, Target: {2}", result.TargetObjectName, result.SourceValue, result.TargetValue));
								}
								else if (successes.Count == MaxSuccessMessages)
								{
									successes.Add(Res.GetString("86f108d1-c196-4a69-b6be-1cd0d71544db", "Success logs truncated"));
								}
								break;
							case WorkflowSetFieldStatus.Warning:
								successCount++;
								generalWarnings.Add(result.FailureReason);
								break;
							case WorkflowSetFieldStatus.NoChange:
								unchangedCount++;
								if (unchanged.Count < MaxUnchangedMessages)
								{
									unchanged.Add(Res.GetString("48f9fbac-7aa5-47d3-913d-c14f6570c463", "Target Object: '{0}', Source: {1}, Target: {2}", result.TargetObjectName, result.SourceValue, result.TargetValue));
								}
								break;
							case WorkflowSetFieldStatus.Failure:
								failureReasons.Add(Res.GetString("b6947e59-081d-4d11-8f15-bf0fa3f0bbba", "{0}. Source: {1}, Target: {2}", result.FailureReason, result.SourceValue, result.TargetValue));
								break;
						}
					}

					notifications?.Add(NotificationType.Information,
						Res.GetString("cf58b491-16cc-42bc-8a00-412a69a16c84", "{0} setting [{1}] with [{2}] succeeded on {3} properties. Unchanged on {4}. Failed on {5}",
										Action.PQ_TriggerType, Action.PQ_FieldNameTrimmed, Action.PQ_FieldValue, successCount, unchangedCount, failureReasons.Count));

					if (successes.Count > 0)
					{
						notifications?.Add(NotificationType.Information,
							Res.GetString("c0cecdb5-5e63-45da-acf0-417a0e80cd64", "Success logs:\r\n{0}", string.Join(System.Environment.NewLine, successes)));
					}

					if (unchanged.Count > 0)
					{
						notifications?.Add(NotificationType.Information,
							Res.GetString("833deaa5-e5ba-4c91-b700-3be3c54da580", "Unchanged logs:\r\n{0}", string.Join(System.Environment.NewLine, unchanged)));
					}

					if (generalWarnings.Count > 0)
					{
						NotifyWarning(notifications, Res.GetString("23383b7c-6087-4405-a53e-99f9a271f56c", "Success with {0} general warnings:\r\n{1}",
																	generalWarnings.Count, string.Join(System.Environment.NewLine, generalWarnings)));
					}

					if (failureReasons.Count > 0)
					{
						var msg = Res.GetString("5b5f67c0-ce5d-4a2e-86f7-713292b62ad2", "Failure with {0} reasons:\r\n{1}",
												failureReasons.Count, string.Join(System.Environment.NewLine, failureReasons));
						if (areFailuresErrors)
						{
							notifications.AddError(msg);
						}
						else
						{
							NotifyWarning(notifications, msg);
						}
					}
				}
			}

			if (!isClient)
			{
				WorkflowTriggerActionRunnerLogs.ActionComplete(Action, notifications);
			}
		}

		string SourcePrettyPrint(IBusiness[] actionRoots)
		{
			return actionRoots != null
					? string.Join(", ", actionRoots.WhereNotNull().Select(s => s.ToString()))
					: Res.GetString("9429f8d8-eb2e-4a99-b750-225b3727bdc5", "Unknown");
		}

		#region Implementation

		void NotifyWarning(INotifications notifications, ZString message)
		{
			MacroHelper.NotifyWarning(notifications, message, verboseLogs ? validation.DetailWarningMessage : ZString.Empty);
		}

		#region Set Property

		internal IReadOnlyCollection<IWorkflowSetFieldResult> TrySetProperty(IBusiness bizo, IBusiness[] actionRoots, string propertyPath, IZType value, INotifications notifications, string macroDefinition = "")
		{
			var collectionReader = MacroHelper.GetCollectionReader(bizo, macroDefinition);

			if (collectionReader != null)
			{
				return TrySetCollectionProperty(collectionReader, actionRoots, propertyPath, value, notifications);
			}
			else
			{
				return TrySetObjectProperty(bizo, actionRoots, propertyPath, value, notifications, macroDefinition);
			}
		}

		IReadOnlyCollection<IWorkflowSetFieldResult> TrySetCollectionProperty(BusinessObjectReader collectionReader, IBusiness[] actionRoots, string propertyPath, IZType value, INotifications notifications)
		{
			var macroClauseProcessor = ObjectFactory.New<IMacroClauseProcessor>(null, validation);
			macroClauseProcessor.ProcessPropertyAndValue(propertyPath, out string nextPath, out string expression);

			var results = new List<IReadOnlyCollection<IWorkflowSetFieldResult>>();
			foreach (var element in collectionReader)
			{
				var businessObjects = actionRoots != null
					? new[] { element }.Concat(actionRoots).ToArray()
					: new[] { element };

				if (MacroHelper.MatchesFilter(businessObjects, expression, notifications))
				{
					results.Add(TrySetProperty(element, actionRoots, nextPath, value, notifications));
				}
			}

			return new ConcatCollection<IWorkflowSetFieldResult>(results);
		}

		IReadOnlyCollection<IWorkflowSetFieldResult> TrySetObjectProperty(IBusiness bizo, IBusiness[] actionRoots, string propertyPath, IZType value, INotifications notifications, string macroDefinition = "")
		{
			if (bizo.GetType().GetCustomAttribute<DisableWorkflowSettingPropertiesAfterOnSavingAttribute>() != null && bizo.Factory.IsInSaveTransaction)
			{
				return Array.AsReadOnly(new[] {
					WorkflowSetFieldResult.Failure(value, Res.GetString("784826dd-9fa2-493c-b2dd-12b9219d7fd2",
					"Class {0} is protected from making changes when saving and its properties cannot be changed by the Immediate Field Change ({1}) trigger action. Consider using a Set Field trigger action instead. Target Object: '{2}'",
					GetTypeNameForLog(bizo),
					Action.PQ_TriggerType,
					bizo.HumanReadableName))
				});
			}

			// GetCustomField
			var (fieldName, fieldType) = MacroHelper.GetCustomFieldNameAndType(propertyPath);
			if (!fieldName.IsEmpty)
			{
				return Array.AsReadOnly(new[] { TrySetCustomField(bizo, fieldName, fieldType, value, notifications) });
			}
			else
			{
				var propertyInfo = new WorkflowMacroEvaluator(notifications, validation).GetNextPropertyInfo(
					bizo.GetType(),
					propertyPath,
					out string subPath);

				if (propertyInfo != null)
				{
					if (string.IsNullOrEmpty(subPath))
					{
						return Array.AsReadOnly(new[] { TrySetPhysicalProperty(bizo, propertyInfo, value, notifications) });
					}
					else
					{
						var nextBizo = (IBusiness)WorkflowMacroEvaluator.EvaluatePropertyInfo(propertyInfo, bizo, propertyPath);
						if (nextBizo != null)
						{
							return TrySetProperty(
								nextBizo,
								actionRoots,
								subPath,
								value,
								notifications,
								macroDefinition);
						}
						else
						{
							return Array.AsReadOnly(new[] { WorkflowSetFieldResult.Failure(value, Res.GetString("03daf9f7-95ff-4fe8-a26f-cd29b22bd38b", "Value of {0} was null", propertyInfo.Name)) });
						}
					}
				}

				return EmptyResults;
			}
		}

		#endregion

		#region Test stuff
#if DEBUG

		internal string PropertyToIgnoreReadonly { get; set; }

#endif
		#endregion

		IWorkflowSetFieldResult TrySetPhysicalProperty(IBusiness bizo, PropertyInfo propertyInfo, IZType value, INotifications notifications)
		{
			if (MacroHelper.IsSystemRecord(bizo))
			{
				var msg = Res.GetString("6d045e16-fdc7-4893-8343-ecdf2f5771cd", "Cannot make changes on system record {0} by Set Field ({1}) trigger action. Target Object: '{2}'", GetTypeNameForLog(bizo), Action.PQ_TriggerType, bizo.HumanReadableName);
				return WorkflowSetFieldResult.Failure(value, msg);
			}

			object convertedValue = MacroHelper.ConvertValue(bizo.GetType(), propertyInfo, value, notifications, validation.DetailWarningMessage);
			if (convertedValue == null)
			{
				return WorkflowSetFieldResult.Failure(value, Res.GetString("c44d6be8-db62-424e-920b-a649efa6067f", "Converting value [{0}] to type {1} returned null. Target Object: '{2}'", value, propertyInfo.PropertyType, bizo.HumanReadableName));
			}

			var businessObject = bizo as BusinessObject;
			var zPropertyInfo = businessObject?.FindPropertyInfo(propertyInfo.Name);

			IZType GetPropertyValue()
			{
				try
				{
					return zPropertyInfo.Value;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new WorkflowMacroEvaluationException(ResString.GetMultilingualString("bd489c12-2c86-4cac-9b66-0e7cf3604e21", "Error occurred retrieving property value"), ex);
				}
			}

			void SetPropertyValue(IZType val)
			{
				try
				{
					using ((bizo as IRegisterStatusChangeMode)?.TemporarilySetStatusChangeModeToChangedByTriggerOrMilestone())
					{
						zPropertyInfo.Value = val;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException()
					&& !(ex.InnerException is MaxLengthExceededException maxLengthExceededException)
					&& !(ex.InnerException is CannotSaveAfterCriticalErrorException))
				{
					throw new WorkflowMacroEvaluationException(ResString.GetMultilingualString("409c5d61-052b-4f68-9b2b-16be1042aa23", "Error occurred setting property value"), ex);
				}
			}

			object originalValue = null;
			try
			{
				using (BusinessObject.ThrowWhenMaxPropertyLengthExceeded())
				{
					if (zPropertyInfo != null && convertedValue is IZType zValue)
					{
						if (zPropertyInfo.PropertyDescriptor.Attributes[typeof(WorkflowSetFieldReadonly)] != null || (zPropertyInfo.ReadOnly && zPropertyInfo.PropertyDescriptor.Attributes[typeof(WorkflowSetFieldReadonlyCheckBypass)] == null
#if DEBUG
								&& zPropertyInfo.Name != PropertyToIgnoreReadonly
#endif
						))
						{
							return WorkflowSetFieldResult.Failure(value, Res.GetString("e98977ce-c540-406d-9a9e-013ab1616caf",
									"Property {0}.{1} of type {2} is read-only and cannot be changed by the Set Field ({3}) trigger action. Target Object: '{4}'",
									GetTypeNameForLog(bizo),
									propertyInfo.Name,
									propertyInfo.PropertyType.Name,
									Action.PQ_TriggerType,
									bizo.HumanReadableName));
						}

						originalValue = GetPropertyValue();

						var maxLength = zPropertyInfo.MaxLength;
						if (maxLength != -1 && value.ToString().Length > maxLength)
						{
							return GetResultForPropertyMaxLengthExceeded(bizo, propertyInfo, maxLength, value);
						}

						if (originalValue.Equals(zValue))
						{
							return WorkflowSetFieldResult.NoChange(originalValue, bizo.HumanReadableName);
						}

						using (businessObject.IgnoreValidationSuspended ? null : new DisposableAction(
								() => { businessObject.IgnoreValidationSuspended = true; },
								() => { businessObject.IgnoreValidationSuspended = false; }))
						{
							SetPropertyValue(zValue);

							var newValue = GetPropertyValue();
							if (newValue.Equals(originalValue))
							{
								return WorkflowSetFieldResult.NoChange(originalValue, bizo.HumanReadableName);
							}

							return WorkflowSetFieldResult.Success(originalValue, newValue, bizo.HumanReadableName, GetValidator(zPropertyInfo, propertyInfo, bizo, SetPropertyValue, newValue, originalValue as IZType, zValue),
								() => { SetPropertyValue(originalValue as IZType); }, zPropertyInfo);
						}
					}
					else
					{
						originalValue = WorkflowMacroEvaluator.EvaluatePropertyInfo(propertyInfo, bizo, propertyInfo.Name);
						if (originalValue != convertedValue)
						{
							try
							{
								propertyInfo.SetValue(bizo, convertedValue, null);
							}
							catch (Exception ex) when (!ex.IsCriticalException() && !(ex.InnerException is MaxLengthExceededException))
							{
								throw new WorkflowMacroEvaluationException(ResString.GetMultilingualString("41c53dea-1b83-410e-bf6f-87b0c0360cc2", "Error occurred during set."), ex);
							}

							var newValue = propertyInfo.GetValue(bizo); // No WorkflowMacroEvaluator.EvaluatePropertyInfo because if we set something then it immediately crashes that is bad.
							if (!originalValue.Equals(newValue))
							{
								return WorkflowSetFieldResult.Success(originalValue, newValue, bizo.HumanReadableName);
							}
						}
						return WorkflowSetFieldResult.NoChange(originalValue, bizo.HumanReadableName);
					}
				}
			}
			catch (Exception ex) when (ex.InnerException is MaxLengthExceededException maxLengthExceededException)
			{
				propertyInfo.SetValue(bizo, originalValue, null);
				return GetResultForPropertyMaxLengthExceeded(bizo, propertyInfo, maxLengthExceededException.MaxLength, value);
			}
		}

		IWorkflowSetFieldResult TrySetCustomField(IBusiness bizo, string customFieldName, string customFieldType, IZType value, INotifications notifications)
		{
			var (customProperty, customBizo) = MacroHelper.GetCustomProperty(bizo, customFieldName, customFieldType, value);

			if (customBizo == null || customProperty == null)
			{
				return WorkflowSetFieldResult.Failure(value,
					Res.GetString("e8da9686-5778-47de-bb1b-a3a8641c1dab",
						"Cannot find custom field '{0}' on {1} to set value '{2}' by Set Field ({3}) trigger action. Target Object: '{4}'",
						customFieldName,
						GetTypeNameForLog(bizo),
						value,
						Action.PQ_TriggerType,
						bizo.HumanReadableName));
			}

			var convertedValue = MacroHelper.ConvertValue(bizo.GetType(), customProperty, customFieldName, value, notifications, validation.DetailWarningMessage);
			if (convertedValue == null)
			{
				return WorkflowSetFieldResult.Failure(convertedValue, Res.GetString("0247124d-ea4c-4502-b9e0-ed48670a2b54", "Could not convert macro value [{0}]", value));
			}

			var metaData = customProperty.Info.MetaData.SingleOrDefault(meta => meta.Id == MetaDataTypes.MaxLength);
			var maxLength = (int)(metaData?.Value ?? -1);

			if (maxLength != -1 && value.ToString().Length > maxLength)
			{
				return WorkflowSetFieldResult.Failure(convertedValue,
					Res.GetString("c39a3a6f-e23b-4837-b5cb-b18cc124cf30",
						"Custom field '{0}' cannot be set with value '{1}' by Set Field ({2}) trigger action because it exceeds maximum length of {3}. Target Object: '{4}'",
						customFieldName,
						value,
						Action.PQ_TriggerType,
						maxLength,
						bizo.HumanReadableName));
			}

			var initialValue = customProperty.GetValue(customBizo);
			if (initialValue.Equals(convertedValue))
			{
				return WorkflowSetFieldResult.NoChange(value, customBizo.HumanReadableName);
			}

			if (customProperty.TrySetValue(customBizo, convertedValue))
			{
				return WorkflowSetFieldResult.Success(initialValue, convertedValue, customBizo.HumanReadableName);
			}

			return WorkflowSetFieldResult.Failure(convertedValue,
				Res.GetString("2f8403b4-188e-482d-ad60-a2fb3d956369",
					"Could not set custom property for field '{0}' on {1} to value '{2}' by Set Field ({3}) trigger action. Target Object: '{4}'",
					customFieldName,
					GetTypeNameForLog(bizo),
					value,
					Action.PQ_TriggerType,
					bizo.HumanReadableName));
		}

		string GetTypeNameForLog(IBusiness bizo)
		{
			var t = bizo.GetType();
			return verboseLogs ? t.FullName : t.Name;
		}

		WorkflowSetFieldResult GetResultForPropertyMaxLengthExceeded(IBusiness bizo, PropertyInfo propertyInfo, int maxLength, IZType value)
		{
			var message = Res.GetString("d4055611-5fdf-4b6e-86ae-f68e208f8e3d",
								"Property {0}.{1} of type {2} cannot be set to value '{3}' by Set Field ({4}) trigger action because it exceeds maximum length of {5}. Target Object: '{6}'",
								GetTypeNameForLog(bizo),
								propertyInfo.Name,
								propertyInfo.PropertyType.Name,
								value,
								Action.PQ_TriggerType,
								maxLength,
								bizo.HumanReadableName);
			return WorkflowSetFieldResult.Failure(value, message);
		}

		WorkflowSetFieldResult.Validator GetValidator(ZPropertyInfo zPropertyInfo, PropertyInfo propertyInfo, IBusiness bizo, Action<IZType> propertySetter, IZType currentValue, IZType originalValue, IZType valueToSet)
		{
			bool DoesSettingPropertyCauseValidationErrors(out string errorMessage)
			{
				errorMessage = null;

				if (zPropertyInfo.HasErrors())
				{
					errorMessage = Res.GetString("c237c046-104a-424b-b7ea-7e28c427f72a",
						"Property {0}.{1} of type {2} has validation error if set with value '{3}' by Set Field ({4}) trigger action. Target Object: '{5}'. Validation Error: '{6}'",
						GetTypeNameForLog(bizo),
						propertyInfo.Name,
						propertyInfo.PropertyType.Name,
						valueToSet.ToString(),
						Action.PQ_TriggerType,
						bizo.HumanReadableName,
						string.Join(", ", zPropertyInfo.GetErrors().Select((notification) => notification.Message)));
				}
				else
				{
					var businessObject = bizo as BusinessObject;
					var propertiesWithErrors = businessObject.PropertiesWithNotifications.Where(p => p.HasErrors()).ToArray();
					if (propertiesWithErrors.Length > 0)
					{
						IDisposable TemporarilyRevertPropertyValueChange()
						{
							propertySetter(originalValue);
							return new DisposableAction(() => propertySetter(currentValue));
						}

						using (TemporarilyRevertPropertyValueChange())
						{
							if (propertiesWithErrors.Length > businessObject.PropertiesWithNotifications.Count(p => p.HasErrors()))
							{
								errorMessage = Res.GetString("BA9BE511-0E17-4E04-83F2-1D9653154858",
									"Setting property {0}.{1} of type {2} to value to '{3}' by Set Field ({4}) trigger action causes validation errors on other dependent properties. Target Object: '{5}'. Properties with errors: [{6}]",
									GetTypeNameForLog(bizo),
									propertyInfo.Name,
									propertyInfo.PropertyType.Name,
									valueToSet.ToString(),
									Action.PQ_TriggerType,
									bizo.HumanReadableName,
									string.Join(",", propertiesWithErrors.Select(p => p.Name)));
							}
						}
					}
				}

				return errorMessage == null;
			}
			return DoesSettingPropertyCauseValidationErrors;
		}

		#endregion

		#region Service

		internal static (PropertyInfo FinalPropertyInfo, Type ParentType, ZString fieldName) GetFinalPropertyInfoAndParentType(IProcessTaskNotification action, INotifications notifications, IZType fieldValue = null)
		{
			return WorkflowProcessorHelper.GetFinalPropertyInfoAndParentType(action, notifications, new WorkflowMacroSetFieldValidation(action), fieldValue);
		}

		#endregion
	}
}
