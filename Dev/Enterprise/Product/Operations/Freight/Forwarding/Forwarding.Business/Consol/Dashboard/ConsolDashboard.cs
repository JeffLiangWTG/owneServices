using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDashboard : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ConsolDashboard(BusinessObjectFactory factory)
			: base(factory)
		{
			EnsureShipmentsAndConsolsWouldBecomeReadonlyWhenAdded();
		}

		#region Add/Remove

		public void AddConsols(IEnumerable<ForwardingConsol> consolsToAdd)
		{
			if (consolsToAdd == null)
			{
				return;
			}

			var consolPKQuery = new ZQuery(JobConsolSchema.PK, consolsToAdd.Where(consol => consol != null).Select(consol => consol.PK))
			{
				ReLoadExistingRows = true
			};
			var consolsReloadedInLocalFactory = Factory.Load<ForwardingConsol>(consolPKQuery);

			Consols.AddRange(consolsReloadedInLocalFactory);
		}

		public void RemoveConsols(IEnumerable<ForwardingConsol> consolsToRemove)
		{
			if (consolsToRemove == null)
			{
				return;
			}

			Consols.RemoveRange(consolsToRemove.Where(consol => consol != null));
		}

		#endregion

		#region State / Consol Snapshots

		public bool HasUnsavedChanges => capturedConsolSnapshots.Any();

		readonly List<DashboardConsolSnapshot> capturedConsolSnapshots = new List<DashboardConsolSnapshot>();

		void AddConsolSnapshot(DashboardConsolSnapshot snapshot)
		{
			if (snapshot == null)
			{
				return;
			}

			if (!capturedConsolSnapshots.Any(existingSnapshot => existingSnapshot.ConsolPK == snapshot.ConsolPK))
			{
				capturedConsolSnapshots.Add(snapshot);
			}
		}

		public void RemoveCapturedSnapshot(ForwardingConsol sourceConsol)
		{
			if (sourceConsol == null)
			{
				return;
			}

			var matchedSnapshot = capturedConsolSnapshots.FirstOrDefault(snapshot => snapshot.ConsolPK == sourceConsol.PK);
			if (matchedSnapshot != null)
			{
				capturedConsolSnapshots.Remove(matchedSnapshot);
			}
		}

		DashboardConsolSnapshot CreateConsolSnapshot(ForwardingConsol sourceConsol)
		{
			return new DashboardConsolSnapshot(
				sourceConsol.PK,
				sourceConsol.HumanReadableName,
				sourceConsol.Shipments.Select(shipment => shipment.PK),
				containers: sourceConsol.Containers);
		}

		public void ReloadDashboardConsol(ForwardingConsol standaloneConsol)
		{
			var dashboardConsol = Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.PK == standaloneConsol.PK);
			if (dashboardConsol != null)
			{
				using (dashboardConsol.CalculationWrapper.SuspendRefreshAllValues())
				using (dashboardConsol.Density.SuspendRefreshAllValues())
				using (dashboardConsol.Shipments.SuspendCountChanged(null))
				{
					dashboardConsol.Shipments.RemoveAll();
					dashboardConsol.Shipments.AddRange(standaloneConsol.Shipments);
				}
				dashboardConsol.ReloadSafe();
				dashboardConsol.RefreshBindingIncludingChildren();
			}
		}

		#endregion

		#region Create Consol

		public ForwardingConsol CreateConsol(BusinessObjectFactory factoryToCreateConsol, IEnumerable<ZGuid> sourceShipmentPKs)
		{
			var consol = factoryToCreateConsol.New<ForwardingConsol>();

			if (sourceShipmentPKs != null && sourceShipmentPKs.Any())
			{
				var shipmentPKQuery = new ZQuery(JobShipmentSchema.PK, sourceShipmentPKs);
				var shipmentsReloadedInConsolFactory = factoryToCreateConsol.Load<ForwardingShipment>(shipmentPKQuery);

				SetConsolDefaultsFromShipments(consol, shipmentsReloadedInConsolFactory);

				foreach (var shipmentReloadedInConsolFactory in shipmentsReloadedInConsolFactory)
				{
					consol.Shipments.Add(shipmentReloadedInConsolFactory);
				}
			}

			return consol;
		}

		void SetConsolDefaultsFromShipments(ForwardingConsol consol, IEnumerable<ForwardingShipment> sourceShipments)
		{
			var shipment = sourceShipments.FirstOrDefault();
			if (shipment != null)
			{
				consol.JK_TransportMode = shipment.JS_TransportMode;
				consol.JK_ConsolMode = shipment.JS_PackingMode;

				consol.JK_RL_NKLoadPort = shipment.JS_RL_NKOrigin;
				consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDestination;
			}
		}

		#endregion

		#region Attach

		public DashboardAttachDetachResult TryAttach(ForwardingConsol consol, IEnumerable<ForwardingShipment> shipmentsToAttach)
		{
			var errorMessage = CheckConsolAndShipmentsAreOnDashboard(consol, this.Shipments.Cast<ForwardingShipment>(), shipmentsToAttach);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				return new DashboardAttachDetachResult(
					shipmentsToAttach.ToArray(),
					Array.Empty<ForwardingShipment>(),
					new[] { new Notification(CargoWise.ComponentModel.NotificationType.Error, errorMessage) });
			}

			DashboardAttachDetachResult result = null;
			using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
			{
				var consolSnapshot = CreateConsolSnapshot(consol);

				result = CheckAndAttachIfAllowed(consol, shipmentsToAttach, isSavingCheck: false);

				OnShipmentsAttached(consol, result.AcceptedShipments.ToArray());

				if (result.AcceptedShipments.Any())
				{
					consolSnapshot.SetContainers(result.Containers);
					AddConsolSnapshot(consolSnapshot);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Event must return dialog result back to business level")]
		public event DatesOutsideRangeEventHandler DatesOutsideRange;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Event must return dialog result back to business level")]
		public delegate ZDialogResult DatesOutsideRangeEventHandler(IEnumerable<string> messages);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Event must return dialog result back to business level")]
		public event HasComplianceRiskEventHandler HasComplianceRisk;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Event must return dialog result back to business level")]
		public delegate ZDialogResult HasComplianceRiskEventHandler(string message);

		DashboardAttachDetachResult CheckAndAttachIfAllowed(ForwardingConsol consol, IEnumerable<ForwardingShipment> shipmentsToAttach, bool isSavingCheck)
		{
			var notifications = new List<INotification>();
			var attachedShipments = new List<ForwardingShipment>();

			var messages = DashboardShipmentVsConsolMessageHelper.Instance.CheckDatesWithinRange(shipmentsToAttach, new[] { consol });
			if (messages.Any())
			{
				if (DatesOutsideRange != null)
				{
					var dialogResult = DatesOutsideRange(messages);
					if (dialogResult == ZDialogResult.Cancel)
					{
						return new DashboardAttachDetachResult(
							shipmentsToAttach,
							Enumerable.Empty<ForwardingShipment>(),
							Enumerable.Empty<Notification>());
					}
					if (dialogResult == ZDialogResult.No)
					{
						shipmentsToAttach.ForEach(shipment => shipment.IsSuppressedETAETDOnAttachToConsol = true);
					}
				}
			}

			var message = DashboardShipmentVsConsolMessageHelper.Instance.CheckShipmentAndConsolComplianceRisk(shipmentsToAttach, new[] { consol });
			if (!message.IsNullOrEmpty() && (HasComplianceRisk != null) && !isSavingCheck)
			{
				var dialogResult = HasComplianceRisk(message);
				if (dialogResult == ZDialogResult.No)
				{
					return new DashboardAttachDetachResult(
						shipmentsToAttach,
						Enumerable.Empty<ForwardingShipment>(),
						Enumerable.Empty<Notification>());
				}
			}

			if (shipmentsToAttach.Any())
			{
				using (consol.CalculationWrapper.SuspendRefreshAllValues())
				using (consol.Density.SuspendRefreshAllValues())
				using (consol.Shipments.SuspendCountChanged(null))
				{
					foreach (var shipmentToAttach in shipmentsToAttach)
					{
						if (shipmentsToAttach.Any(s => s.PK == shipmentToAttach.JS_JS_ColoadMasterShipment && s.IsLeadOrMaster))
						{
							continue;
						}
						var attachRequest = DashboardShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(consol, shipmentToAttach);
						if (!attachRequest.Errors.IsEmpty)
						{
							notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, attachRequest.Errors));
						}
						else
						{
							if (isSavingCheck)
							{
								consol.IsSavingFromPlanningBoard = true;
							}

							AttachShipment(consol, shipmentToAttach);
							attachedShipments.Add(shipmentToAttach);

							if (!attachRequest.Warnings.IsEmpty)
							{
								notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, attachRequest.Warnings));
							}
						}
					}
				}

				consol.TriggerShipmentsCountChanged(new CollectionCountChangedEventArgs(false, null));
			}

			var result = new DashboardAttachDetachResult(
				shipmentsToAttach.ToArray(),
				attachedShipments.ToArray(),
				notifications,
				consol.Containers);

			return result;
		}

		void AttachShipment(ForwardingConsol consol, ForwardingShipment shipment)
		{
			consol.Shipments.Add(shipment);
		}

		void OnShipmentsAttached(ForwardingConsol consol, ForwardingShipment[] attachedShipments)
		{
			if (attachedShipments.Any())
			{
				foreach (var attachedShipment in attachedShipments)
				{
					Shipments.Remove(attachedShipment);
					attachedShipment.ReloadSafe();
				}

				consol.RefreshBinding();
			}
		}

		#endregion

		#region Detach

		public DashboardAttachDetachResult TryDetach(ForwardingConsol consol, IEnumerable<ForwardingShipment> shipmentsToDetach, IEnumerable<ForwardingShipment> excludeShipmentsFromBeingAddedToShipmentsGrid = null)
		{
			var errorMessage = CheckConsolAndShipmentsAreOnDashboard(consol, consol.Shipments.Cast<ForwardingShipment>(), shipmentsToDetach);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				return new DashboardAttachDetachResult(
					shipmentsToDetach,
					Array.Empty<ForwardingShipment>(),
					new[] { new Notification(CargoWise.ComponentModel.NotificationType.Error, errorMessage) });
			}

			DashboardAttachDetachResult result = null;
			using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
			{
				var consolSnapshot = CreateConsolSnapshot(consol);

				result = CheckAndDetachIfAllowed(consol, shipmentsToDetach);

				OnShipmentsDetached(consol, result.AcceptedShipments.ToArray(), excludeShipmentsFromBeingAddedToShipmentsGrid?.ToArray() ?? Array.Empty<ForwardingShipment>());

				if (result.AcceptedShipments.Any())
				{
					AddConsolSnapshot(consolSnapshot);
				}
			}

			return result;
		}

		DashboardAttachDetachResult CheckAndDetachIfAllowed(ForwardingConsol consol, IEnumerable<ForwardingShipment> shipmentsToDetach)
		{
			var notifications = new List<INotification>();
			var detachedShipments = new List<ForwardingShipment>();

			if (shipmentsToDetach.Any())
			{
				using (consol.CalculationWrapper.SuspendRefreshAllValues())
				using (consol.Density.SuspendRefreshAllValues())
				using (consol.Shipments.SuspendCountChanged(null))
				{
					var shipmentsToCheckDetach = new List<ForwardingShipment>();
					foreach (var shipmentToDetach in shipmentsToDetach)
					{
						if (consol.ShipmentsAttachedThisSession.ContainsKey(shipmentToDetach.PK))
						{
							DetachShipment(consol, shipmentToDetach);
							detachedShipments.Add(shipmentToDetach);
						}
						else
						{
							shipmentsToCheckDetach.Add(shipmentToDetach);
						}
					}

					var detachRequest = DashboardShipmentVsConsolMessageHelper.Instance.IsAllowedToDetachShipments(consol, shipmentsToCheckDetach);
					if (!detachRequest.RestrictedMessage.IsEmpty)
					{
						notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, detachRequest.RestrictedMessage));
					}
					else
					{
						shipmentsToCheckDetach.ForEach(s =>
						{
							DetachShipment(consol, s);
						});
						detachedShipments.AddRange(shipmentsToCheckDetach);
					}
				}

				consol.TriggerShipmentsCountChanged(new CollectionCountChangedEventArgs(false, null));
			}

			var result = new DashboardAttachDetachResult(
				shipmentsToDetach,
				detachedShipments.ToArray(),
				notifications);

			return result;
		}

		void DetachShipment(ForwardingConsol consol, ForwardingShipment shipment)
		{
			consol.Shipments.Remove(shipment);
		}

		void OnShipmentsDetached(ForwardingConsol consol, ForwardingShipment[] detachedShipments, ForwardingShipment[] excludeShipmentsFromBeingAddedToShipmentsGrid)
		{
			if (detachedShipments.Any())
			{
				foreach (var detachedShipment in detachedShipments)
				{
					if (!excludeShipmentsFromBeingAddedToShipmentsGrid.Any(s => s.PK == detachedShipment.PK))
					{
						Shipments.Add(detachedShipment);
						detachedShipment.ReloadSafe();
					}
				}

				consol.RefreshBinding();
			}
		}

		#endregion

		#region Save

		public DashboardSaveResult Save()
		{
			DashboardSaveResult result = null;

			bool isWorkflowAndTemplateApplicationDeferred = WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave.Value;

			using (Logs.DeferFiringWorkflow(isWorkflowAndTemplateApplicationDeferred))
			using (ProcessTask.Loader.SuppressTemplateApplication(isWorkflowAndTemplateApplicationDeferred))
			using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
			{
				result = ReloadValidateAndTryToSaveConsolsWithChangesOneByOneInTheStandaloneFactories();
			}

			return result;
		}

		DashboardSaveResult ReloadValidateAndTryToSaveConsolsWithChangesOneByOneInTheStandaloneFactories()
		{
			var currentConsolSnapshots = CreateCurrentSnapshotsForAllConsolsWithChanges();

			foreach (var consolSnapshot in currentConsolSnapshots)
			{
				var consol = ReloadConsolInStandaloneFactory(consolSnapshot.ConsolPK);
				if (consol != null)
				{
					if (CheckReloadedConsolHasConcurrencyConflicts(consolSnapshot, consol))
					{
						consolSnapshot.SetState(DashboardConsolSnapshot.SnapshotState.ModifiedByAnotherUser);
						continue;
					}

					UpdateConsolContainers(consolSnapshot, consol);
					UpdateConsolShipments(consolSnapshot, consol);

					var errorNotifications = RecreateShipmentConfiguration(consol, consolSnapshot.ShipmentPKs);
					errorNotifications = errorNotifications.Union(ValidateConsolForSavePreventingErrors(consol));

					if (errorNotifications.Any())
					{
						consolSnapshot.SetState(DashboardConsolSnapshot.SnapshotState.ValidationErrors);
						consolSnapshot.SetNotifications(errorNotifications);
					}
					else
					{
						bool consolSavedSuccessfully = TrySaveConsol(consol);
						if (consolSavedSuccessfully)
						{
							RemoveCapturedSnapshot(consol);
						}

						var snapshotState = consolSavedSuccessfully
							? DashboardConsolSnapshot.SnapshotState.Saved
							: DashboardConsolSnapshot.SnapshotState.SaveExceptions;

						consolSnapshot.SetState(snapshotState);
					}

					consol.Factory.RefreshEnabled = false;
				}
			}

			return new DashboardSaveResult(currentConsolSnapshots);
		}

		void UpdateConsolContainers(DashboardConsolSnapshot consolSnapshot, ForwardingConsol consol)
		{
			consol.Containers?.OfType<ForwardingContainer>().ForEach(consolContainer =>
			{
				var container = consolSnapshot.Containers?.Where(consolSnapshotContainer => consolSnapshotContainer.PK == consolContainer.PK).FirstOrDefault();

				if (container != null)
				{
					consolContainer.JC_IsGrossWeightOverridden = container.JC_IsGrossWeightOverridden;
				}
			});
		}

		DashboardConsolSnapshot[] CreateCurrentSnapshotsForAllConsolsWithChanges()
		{
			var result = new List<DashboardConsolSnapshot>();

			foreach (var capturedSnapshot in capturedConsolSnapshots)
			{
				var consol = Consols.FindByPK(capturedSnapshot.ConsolPK) as ForwardingConsol;
				if (consol != null)
				{
					result.Add(CreateConsolSnapshot(consol));
				}
			}

			return result.ToArray();
		}

		ForwardingConsol ReloadConsolInStandaloneFactory(ZGuid consolPK)
		{
			var standaloneFactory = new BusinessObjectFactory()
			{
				NameForDebugging = "ConsolDashboard's Factory for Validation and Saving"
			};

			var consol = standaloneFactory.Load<ForwardingConsol>(consolPK);

			return consol;
		}

		#region Delivery Due Date

		void UpdateConsolShipments(DashboardConsolSnapshot consolSnapshot, ForwardingConsol consol)
		{
			var currentConsolShipmentPKs = consol.Shipments.Select(s => s.PK);

			var (attachShipmentPks, detachShipmentPks) = GetAttachDetachList(currentConsolShipmentPKs, consolSnapshot.ShipmentPKs);
			var shipmentPks = new List<ZGuid>();
			shipmentPks.AddRange(attachShipmentPks);
			shipmentPks.AddRange(detachShipmentPks);

			foreach (var shipmentPk in shipmentPks)
			{
				var shipment = consol.Factory.Load<ForwardingShipment>(shipmentPk);
				_ = shipment.DocsAndCartage; // ensure DocsAndCartage is loaded otherwise shipment.IsDocsAndCartageSet will be false and we never calculate DDD
				if (!shipment.IsDeleted && !shipment.IsDeleting)
				{
					shipment.DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.ConsolAttached);
					shipment.CalculateDeliveryDueDateIfNecessary();
				}
			}
		}

		#endregion

		#region Concurrency Conflicts

		bool CheckReloadedConsolHasConcurrencyConflicts(DashboardConsolSnapshot currentSnapshot, ForwardingConsol consol)
		{
			var existingSnapshot = capturedConsolSnapshots.FirstOrDefault(snapshot => snapshot.ConsolPK == consol.PK);
			if (existingSnapshot != null)
			{
				var currentConsolShipmentPKs = consol.Shipments.Select(s => s.PK);
				var (lastEditedAttach, lastEditedDetach) = GetAttachDetachList(existingSnapshot.ShipmentPKs, currentConsolShipmentPKs);

				if (!lastEditedAttach.Any() && !lastEditedDetach.Any())
				{
					return false;
				}

				var (currentSnapshotAttach, currentSnapshotDetach) = GetAttachDetachList(existingSnapshot.ShipmentPKs, currentSnapshot.ShipmentPKs);
				DetectConcurrencyConflictsAndUpdateSnapshot(currentSnapshot, lastEditedAttach, lastEditedDetach, currentSnapshotAttach, currentSnapshotDetach);

				return currentSnapshot.Notifications.Any();
			}

			return false;
		}

		void DetectConcurrencyConflictsAndUpdateSnapshot(DashboardConsolSnapshot currentSnapshot, IEnumerable<ZGuid> lastEditedAttach, IEnumerable<ZGuid> lastEditedDetach, IEnumerable<ZGuid> currentSnapshotAttach, IEnumerable<ZGuid> currentSnapshotDetach)
		{
			var conflictErrors = new List<INotification>();
			var shipmentsWithConflict = new List<ZGuid>();

			var attachedByCurrentUserButNotAnotherUser = currentSnapshotAttach.Where(s => !lastEditedAttach.Contains(s)).ToArray();
			conflictErrors.AddRange(attachedByCurrentUserButNotAnotherUser.Select(GetAttachedByCurrentUserConflictError));
			shipmentsWithConflict.AddRange(attachedByCurrentUserButNotAnotherUser);

			var detachedByCurrentUserButNotAnotherUser = currentSnapshotDetach.Where(s => !lastEditedDetach.Contains(s)).ToArray();
			conflictErrors.AddRange(detachedByCurrentUserButNotAnotherUser.Select(GetDetachedByCurrentUserConflictError));
			shipmentsWithConflict.AddRange(detachedByCurrentUserButNotAnotherUser);

			var attachedByAnotherUserButNotCurrentUser = lastEditedAttach.Where(s => !currentSnapshotAttach.Contains(s)).ToArray();
			conflictErrors.AddRange(attachedByAnotherUserButNotCurrentUser.Select(GetAttachedByAnotherUserConflictError));
			shipmentsWithConflict.AddRange(attachedByAnotherUserButNotCurrentUser);

			var detachedByAnotherUserButNotCurrentUser = lastEditedDetach.Where(s => !currentSnapshotDetach.Contains(s)).ToArray();
			conflictErrors.AddRange(detachedByAnotherUserButNotCurrentUser.Select(GetDetachedByAnotherUserConflictError));
			shipmentsWithConflict.AddRange(detachedByAnotherUserButNotCurrentUser);

			currentSnapshot.SetNotifications(conflictErrors);

			var newShipmentPKs = new HashSet<ZGuid>(currentSnapshot.ShipmentPKs);
			newShipmentPKs.UnionWith(shipmentsWithConflict);
			currentSnapshot.SetShipmentPKs(newShipmentPKs);
		}

		INotification GetAttachedByCurrentUserConflictError(ZGuid shipmentPk)
		{
			return new Notification(
				CargoWise.ComponentModel.NotificationType.Error,
				ResString.GetMultilingualString(
					"9196b05c-b08a-4b6a-b19c-5117d5d841cc",
					"{0} - Attached by current user",
					GetShipmentHumanReadableName(shipmentPk)));
		}

		INotification GetDetachedByCurrentUserConflictError(ZGuid shipmentPk)
		{
			return new Notification(
				CargoWise.ComponentModel.NotificationType.Error,
				ResString.GetMultilingualString(
					"1452ada6-c5e7-49fd-9426-d8867a4c67af",
					"{0} - Detached by current user",
					GetShipmentHumanReadableName(shipmentPk)));
		}

		INotification GetAttachedByAnotherUserConflictError(ZGuid shipmentPk)
		{
			return new Notification(
				CargoWise.ComponentModel.NotificationType.Error,
				ResString.GetMultilingualString(
					"ff82f55c-8ed5-4e61-9c9b-541a7ae66400",
					"{0} - Attached by another user",
					GetShipmentHumanReadableName(shipmentPk)));
		}

		INotification GetDetachedByAnotherUserConflictError(ZGuid shipmentPk)
		{
			return new Notification(
				CargoWise.ComponentModel.NotificationType.Error,
				ResString.GetMultilingualString(
					"b0cbd3c2-cb89-478a-849a-94dcdd6f24b7",
					"{0} - Detached by another user",
					GetShipmentHumanReadableName(shipmentPk)));
		}

		ZString GetShipmentHumanReadableName(ZGuid shipmentPk)
		{
			return Factory.Load<ForwardingShipment>(shipmentPk)?.HumanReadableName ?? "";
		}

		(IEnumerable<ZGuid>, IEnumerable<ZGuid>) GetAttachDetachList(IEnumerable<ZGuid> oldShipmentPKs, IEnumerable<ZGuid> newShipmentPKs)
		{
			var attachShipmentPKs = newShipmentPKs.Where(pk => !oldShipmentPKs.Contains(pk)).ToArray();
			var detachShipmentPKs = oldShipmentPKs.Where(pk => !newShipmentPKs.Contains(pk)).ToArray();
			return (attachShipmentPKs, detachShipmentPKs);
		}

		#endregion

		public IEnumerable<INotification> RecreateShipmentConfiguration(ForwardingConsol consol, IEnumerable<ZGuid> selectedShipmentPKs)
		{
			if (consol == null || selectedShipmentPKs == null)
			{
				return Enumerable.Empty<INotification>();
			}

			var originalShipmentPKs = consol.Shipments
				.Select(shipment => shipment.PK)
				.ToArray();

			var (shipmentPKsToAttach, originalShipmentPKsToDetach) = GetAttachDetachList(originalShipmentPKs, selectedShipmentPKs);

			var shipmentsToDetachQuery = new ZQuery(JobShipmentSchema.PK, originalShipmentPKsToDetach);
			var shipmentsToDetach = consol.Factory.Load<ForwardingShipment>(shipmentsToDetachQuery);

			var errors = CheckAndDetachIfAllowed(consol, shipmentsToDetach)
				.Notifications
				.Where(notification => notification.Type == CargoWise.ComponentModel.NotificationType.Error);

			var shipmentsToAttachQuery = new ZQuery(JobShipmentSchema.PK, shipmentPKsToAttach);
			var shipmentsToAttach = consol.Factory.Load<ForwardingShipment>(shipmentsToAttachQuery);

			return errors.Union((CheckAndAttachIfAllowed(consol, shipmentsToAttach, isSavingCheck: true)
				.Notifications
				.Where(notification => notification.Type == CargoWise.ComponentModel.NotificationType.Error)));
		}

		IEnumerable<INotification> ValidateConsolForSavePreventingErrors(ForwardingConsol consol)
		{
			var validationErrors = new List<INotification>();

			consol.RunPreSaveValidationWithFetchHints();

			if (consol.HasErrors)
			{
				validationErrors.AddRange(consol.GetErrors());
			}

			if (consol.IsPreAllocationExceededAndRestricted)
			{
				validationErrors.Add(new Notification(
					CargoWise.ComponentModel.NotificationType.Error,
					ResString.GetMultilingualString("716812A4-1570-4FA8-9E4B-F1B53B20C6E6", "Pre-allocated values exceed the registry specified percentage.")));
			}

			return validationErrors;
		}

		bool TrySaveConsol(ForwardingConsol consol)
		{
			bool consolSavedSuccessfully = true;

			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(consol.Factory.Save, null, true);
			}
			catch (ZSaveException)
			{
				// Nom-nom-nom!
				// Consol would be marked as 'save exception happened'
				// User would be able to process failed consols one-by-one later

				consolSavedSuccessfully = false;
			}

			return consolSavedSuccessfully;
		}

		#endregion

		#region Properties

		public ForwardingShipmentCollection Shipments
		{
			get { return shipments ?? (shipments = new ForwardingShipmentCollection(Factory)); }
		}
		ForwardingShipmentCollection shipments;

		public ForwardingModuleShipmentCollection Shipments_List
		{
			get { return shipments_List ?? (shipments_List = new ForwardingModuleShipmentCollection(Factory)); }
		}
		ForwardingModuleShipmentCollection shipments_List;

		public ForwardingConsolCollection Consols
		{
			get { return consols ?? (consols = new ForwardingConsolCollection(Factory)); }
		}
		ForwardingConsolCollection consols;

		public ForwardingModuleConsolCollection Consols_List
		{
			get { return consols_List ?? (consols_List = new ForwardingModuleConsolCollection(Factory)); }
		}
		ForwardingModuleConsolCollection consols_List;

		#endregion

		#region Implementation

		void EnsureShipmentsAndConsolsWouldBecomeReadonlyWhenAdded()
		{
			Shipments.CountChanged += CollectionCountChanged_MakeNewItemReadonly;
			Consols.CountChanged += CollectionCountChanged_MakeNewItemReadonly;
		}

		void CollectionCountChanged_MakeNewItemReadonly(object sender, CollectionCountChangedEventArgs e)
		{
			if (e != null && e.ItemAdded && e.BizObject != null)
			{
				e.BizObject.ReloadSafe();
				e.BizObject.ReadOnly = true;
			}
		}

		string CheckConsolAndShipmentsAreOnDashboard(ForwardingConsol consolToCheck, IEnumerable<ForwardingShipment> allEligibleShipments, IEnumerable<ForwardingShipment> shipmentsToCheck)
		{
			var messageBuilder = new ZStringBuilder();
			if (!Consols.Contains(consolToCheck.PK))
			{
				messageBuilder.Append(consolToCheck.HumanReadableName);
			}

			foreach (var shipmentToCheck in shipmentsToCheck)
			{
				if (!allEligibleShipments.Any(shipment => shipment.PK == shipmentToCheck.PK))
				{
					messageBuilder.Append(shipmentToCheck.HumanReadableName);
				}
			}

			if (!messageBuilder.IsEmpty)
			{
				var notFoundMessage = ResString.GetMultilingualString("ae5af684-b122-405a-83df-01fe2dd47dc5", "Not found on dashboard:");
				messageBuilder.Prepend(notFoundMessage);
			}

			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
