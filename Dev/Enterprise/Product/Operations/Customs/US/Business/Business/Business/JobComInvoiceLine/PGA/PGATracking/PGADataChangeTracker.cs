using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class PGADataChangeTracker
	{
		public PGADataChangeTracker(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		public static MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("ACCD563F-CCE0-46E7-A296-4D8545F5CEA8", "PGA line has been accepted, please use menu 'Delete PGA Line' to delete a PGA line."); }
		}

		BusinessObjectFactory Factory
		{
			get { return declaration.Factory; }
		}

		public ZString LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(ZString oldValue, ZString newValue,
			IEnumerable<IPGADataCorrection> pgas)
		{
			var result = newValue;
			LoadAllPGARelatedDataIfNeeded();
			if (OGAIndicatorList.IsToBeDeclared(oldValue) && !OGAIndicatorList.IsToBeDeclared(newValue) && pgas.Any(x => !x.TrackingStatusInfo.Value.IsEmpty && x.IncludedInMessage()))
			{
				if (declaration.MarkPGAStatusToBeDeletedWarningChecker == null || declaration.MarkPGAStatusToBeDeletedWarningChecker(LodgedWillBeDeletedWarning))
				{
					foreach (var pga in pgas)
					{
						if (!pga.TrackingStatusInfo.Value.IsEmpty && pga.IncludedInMessage())
						{
							pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.ToBeDeleted;
						}
					}
				}
				else
				{
					result = oldValue;
				}
			}
			return result;
		}

		public static string LodgedWillBeDeletedWarning
		{
			get { return Res.GetString("8859F59E-CE55-4A34-93F0-D216E4E895C2", "This line has accepted PGA data against it. The changes that you have made will cause the PGA Lines to be marked with a message status of “To Be Deleted” if you were to send a PGA data correction message. The overall PGA status of the line will remain. Are you sure that you wish to continue?"); }
		}

		public void LoadAllPGARelatedDataIfNeeded()
		{
			if (!hasLoadedAllPGARelatedData)
			{
				hasLoadedAllPGARelatedData = true;
				if (declaration.IsPGATrackingEnabled)
				{
					declaration.LoadChildEditableObjectsForChild(new IBusiness[] { declaration.Invoices, declaration.InvoiceLines }, declaration.HasPGARelatedDataLoadProgress ? declaration.PGARelatedDataLoadProgress : null);
				}
			}
		}
		bool hasLoadedAllPGARelatedData;

		public IDisposable SuspendTracking()
		{
			return new TrackingSuspender(this);
		}

		bool IsTrackingSuspended
		{
			get { return trackingSuspenderIndex > 0; }
		}
		byte trackingSuspenderIndex;

		class TrackingSuspender : IDisposable
		{
			public TrackingSuspender(PGADataChangeTracker tracker)
			{
				this.tracker = tracker;
				tracker.trackingSuspenderIndex++;
			}

			readonly PGADataChangeTracker tracker;

			void IDisposable.Dispose()
			{
				tracker.trackingSuspenderIndex--;
			}
		}

		public void RegisterTracker(IPGADataCorrection tracker, JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null && !IsTrackingSuspended)
			{
				declaration.IsPGATrackingEnabled = true;
				tracker.HasChangesChanged -= Tracker_HasChangesChanged;
				tracker.HasChangesChanged += Tracker_HasChangesChanged;
				AddInvoiceLineFieldTracker(invoiceLine, tracker);
				AddInvoiceFieldTracker(invoiceLine, tracker);
				AddContainerFieldTracker(invoiceLine, tracker);
				AddDeclarationFieldTracker(tracker);
			}
		}

		void Tracker_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			var tracker = sender as IPGADataCorrection;
			if (tracker != null && e.ObjectJustWasChanged)
			{
				tracker.RelatedDataChanged();
			}
		}

		public void UnRegisterTracker(IPGADataCorrection tracker, JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null && !IsTrackingSuspended)
			{
				tracker.HasChangesChanged -= Tracker_HasChangesChanged;
				RemoveInvoiceLineFieldTracker(invoiceLine, tracker);
				RemoveInvoiceFieldTracker(invoiceLine, tracker);
				RemoveContainerFieldTracker(invoiceLine, tracker);
				RemoveDeclarationFieldTracker(tracker);
			}
		}

		#region Declaration Field Trackers

		string[] GetRelatedDeclarationFields(IPGADataCorrection tracker)
		{
			return tracker.GetRelatedDeclarationFields().Where(x => !string.IsNullOrEmpty(x)).ToArray();
		}

		void RemoveDeclarationFieldTracker(IPGADataCorrection tracker)
		{
			var fieldNames = GetRelatedDeclarationFields(tracker);
			if (fieldNames.Length > 0)
			{
				foreach (var fieldName in fieldNames)
				{
					List<IPGADataCorrection> list;
					if (DeclarationFieldTrackers.TryGetValue(fieldName, out list) && list.Contains(tracker))
					{
						list.Remove(tracker);
						if (list.Count == 0)
						{
							UnHookEvents(declaration, fieldName, DeclarationDataChanged);
							DeclarationFieldTrackers.Remove(fieldName);
						}
					}
				}
			}
		}

		void AddDeclarationFieldTracker(IPGADataCorrection tracker)
		{
			var fieldNames = GetRelatedDeclarationFields(tracker);
			if (fieldNames.Length > 0)
			{
				foreach (var fieldName in fieldNames)
				{
					List<IPGADataCorrection> list;
					if (!DeclarationFieldTrackers.TryGetValue(fieldName, out list))
					{
						list = new List<IPGADataCorrection>();
						DeclarationFieldTrackers.Add(fieldName, list);
					}
					if (!list.Contains(tracker))
					{
						list.Add(tracker);
						HookEvents(declaration, fieldName, DeclarationDataChanged);
					}
				}
			}
		}

		void DeclarationDataChanged(object sender, EventArgs e)
		{
			var valueE = e as ValueChangedEventArgs;
			if (valueE != null)
			{
				var name = valueE.Info.Name;
				List<IPGADataCorrection> list;
				if (DeclarationFieldTrackers.TryGetValue(name, out list))
				{
					foreach (var tracker in list.ToArray())
					{
						tracker.RelatedDataChanged();
					}
				}
			}
		}

		Dictionary<string, List<IPGADataCorrection>> DeclarationFieldTrackers
		{
			get { return declarationFieldTrackers ?? (declarationFieldTrackers = new Dictionary<string, List<IPGADataCorrection>>()); }
		}
		Dictionary<string, List<IPGADataCorrection>> declarationFieldTrackers;

		#endregion

		#region Invoice Field Trackers

		string[] GetRelatedInvoiceFields(IPGADataCorrection tracker)
		{
			return tracker.GetRelatedInvoiceFields().Where(x => !string.IsNullOrEmpty(x) && x != JobComInvoiceLine.Schema.JI_JZ).ToArray();
		}

		void RemoveInvoiceFieldTracker(JobComInvoiceLine invoiceLine, IPGADataCorrection tracker)
		{
			var fieldNames = GetRelatedInvoiceFields(tracker);
			if (fieldNames.Length > 0)
			{
				var invoice = invoiceLine.InvoiceHeader;
				foreach (var fieldName in fieldNames)
				{
					List<IPGADataCorrection> list;
					if (InvoiceFieldTrackers.TryGetValue(fieldName, out list) && list.Contains(tracker))
					{
						list.Remove(tracker);
						if (list.Count == 0)
						{
							UnHookEvents(invoice, fieldName, InvoiceDataChanged);
							InvoiceFieldTrackers.Remove(fieldName);
							if (InvoiceFieldTrackers.Count == 0)
							{
								UnHookInvoiceHeaderChangedEvent(invoiceLine.JI_JZInfo);
							}
						}
					}
				}
			}
		}

		void AddInvoiceFieldTracker(JobComInvoiceLine invoiceLine, IPGADataCorrection tracker)
		{
			var fieldNames = GetRelatedInvoiceFields(tracker);
			if (fieldNames.Length > 0)
			{
				var invoice = invoiceLine.InvoiceHeader;
				HookInvoiceHeaderChangedEvent(invoiceLine.JI_JZInfo);
				foreach (var fieldName in fieldNames)
				{
					List<IPGADataCorrection> list;
					if (!InvoiceFieldTrackers.TryGetValue(fieldName, out list))
					{
						list = new List<IPGADataCorrection>();
						InvoiceFieldTrackers.Add(fieldName, list);
					}
					if (!list.Contains(tracker))
					{
						list.Add(tracker);
						HookEvents(invoice, fieldName, InvoiceDataChanged);
					}
				}
			}
		}

		void HookInvoiceHeaderChangedEvent(ZPropertyInfo info)
		{
			info.ValueChanged -= InvoiceHeaderChanged;
			info.ValueChanged += InvoiceHeaderChanged;
		}

		void UnHookInvoiceHeaderChangedEvent(ZPropertyInfo info)
		{
			info.ValueChanged -= InvoiceHeaderChanged;
		}

		void InvoiceHeaderChanged(object sender, EventArgs e)
		{
			var valueE = e as ValueChangedEventArgs;
			if (valueE != null)
			{
				var fieldNames = InvoiceFieldTrackers.Keys.ToArray();
				if (fieldNames.Length > 0)
				{
					var oldInvoice = Factory.Load<JobComInvoiceHeader>((ZGuid)valueE.OldValue);
					var newInvoice = Factory.Load<JobComInvoiceHeader>((ZGuid)valueE.NewValue);
					foreach (var fieldName in fieldNames)
					{
						UnHookEvents(oldInvoice, fieldName, InvoiceDataChanged);
						HookEvents(newInvoice, fieldName, InvoiceDataChanged);
					}
				}
			}
		}

		void InvoiceDataChanged(object sender, EventArgs e)
		{
			var valueE = e as ValueChangedEventArgs;
			if (valueE != null)
			{
				var name = valueE.Info.Name;
				List<IPGADataCorrection> list;
				if (InvoiceFieldTrackers.TryGetValue(name, out list))
				{
					foreach (var tracker in list.ToArray())
					{
						tracker.RelatedDataChanged();
					}
				}
			}
		}

		Dictionary<string, List<IPGADataCorrection>> InvoiceFieldTrackers
		{
			get { return invoiceFieldTrackers ?? (invoiceFieldTrackers = new Dictionary<string, List<IPGADataCorrection>>()); }
		}
		Dictionary<string, List<IPGADataCorrection>> invoiceFieldTrackers;

		#endregion

		#region Invoice Line Field Trackers

		string[] GetRelatedInvoiceLineFields(IPGADataCorrection tracker)
		{
			return tracker.GetRelatedInvoiceLineFields().Where(x => !string.IsNullOrEmpty(x)).Concat(DefaultInvoiceLineFields()).Distinct().ToArray();
		}

		string[] DefaultInvoiceLineFields()
		{
			return new[]
			{
				JobComInvoiceLine.Schema.JI_ParentID,
				JobComInvoiceLine.Schema.JI_Description
			};
		}

		void RemoveInvoiceLineFieldTracker(JobComInvoiceLine invoiceLine, IPGADataCorrection tracker)
		{
			Dictionary<string, List<IPGADataCorrection>> fieldTrackers;
			if (InvoiceLineFieldTrackers.TryGetValue(invoiceLine, out fieldTrackers))
			{
				var fieldNames = GetRelatedInvoiceLineFields(tracker);
				foreach (var fieldName in fieldNames)
				{
					List<IPGADataCorrection> list;
					if (fieldTrackers.TryGetValue(fieldName, out list) && list.Contains(tracker))
					{
						list.Remove(tracker);
						if (list.Count == 0)
						{
							UnHookEvents(invoiceLine, fieldName, InvoiceLineDataChanged);
							fieldTrackers.Remove(fieldName);
							if (fieldTrackers.Count == 0)
							{
								InvoiceLineFieldTrackers.Remove(invoiceLine);
							}
						}
					}
				}
			}
		}

		void AddInvoiceLineFieldTracker(JobComInvoiceLine invoiceLine, IPGADataCorrection tracker)
		{
			Dictionary<string, List<IPGADataCorrection>> fieldTrackers;
			if (!InvoiceLineFieldTrackers.TryGetValue(invoiceLine, out fieldTrackers))
			{
				fieldTrackers = new Dictionary<string, List<IPGADataCorrection>>();
				InvoiceLineFieldTrackers.Add(invoiceLine, fieldTrackers);
			}
			HookEvents(invoiceLine, tracker, fieldTrackers, GetRelatedInvoiceLineFields(tracker), InvoiceLineDataChanged);
		}

		void InvoiceLineDataChanged(object sender, EventArgs e)
		{
			var valueE = e as ValueChangedEventArgs;
			if (valueE != null)
			{
				var provider = valueE.Info.BizObj as IInvoiceLineProvider;
				var invoiceLine = provider?.InvoiceLine;
				Dictionary<string, List<IPGADataCorrection>> fieldTrackers;
				if (invoiceLine != null && InvoiceLineFieldTrackers.TryGetValue(invoiceLine, out fieldTrackers))
				{
					var name = valueE.Info.Name;
					List<IPGADataCorrection> list;
					if (fieldTrackers.TryGetValue(name, out list))
					{
						foreach (var tracker in list.ToArray())
						{
							tracker.RelatedDataChanged();
						}
					}
				}
			}
		}

		Dictionary<JobComInvoiceLine, Dictionary<string, List<IPGADataCorrection>>> InvoiceLineFieldTrackers
		{
			get { return invoiceLineFieldTrackers ?? (invoiceLineFieldTrackers = new Dictionary<JobComInvoiceLine, Dictionary<string, List<IPGADataCorrection>>>()); }
		}
		Dictionary<JobComInvoiceLine, Dictionary<string, List<IPGADataCorrection>>> invoiceLineFieldTrackers;

		#endregion

		#region Common

		void HookEvents(BusinessObject bizObj, IPGADataCorrection tracker, Dictionary<string, List<IPGADataCorrection>> fieldTrackers, IEnumerable<string> fieldNames, EventHandler dataChanged)
		{
			foreach (var fieldName in fieldNames)
			{
				var info = bizObj.ZPropertyInfoHash.GetPropertySafe(fieldName);
				if (info != null)
				{
					HookEvents(info, tracker, fieldTrackers, dataChanged);
				}
			}
		}

		void HookEvents(BusinessObject bizObj, string fieldName, EventHandler dataChanged)
		{
			var info = bizObj?.ZPropertyInfoHash.GetPropertySafe(fieldName);
			if (info != null)
			{
				info.ValueChanged -= dataChanged;
				info.ValueChanged += dataChanged;
			}
		}

		void UnHookEvents(BusinessObject bizObj, string fieldName, EventHandler dataChanged)
		{
			var info = bizObj?.ZPropertyInfoHash.GetPropertySafe(fieldName);
			if (info != null)
			{
				info.ValueChanged -= dataChanged;
			}
		}

		void HookEvents(ZPropertyInfo info, IPGADataCorrection tracker, Dictionary<string, List<IPGADataCorrection>> fieldTrackers, EventHandler dataChanged)
		{
			info.ValueChanged -= dataChanged;
			info.ValueChanged += dataChanged;
			var name = info.Name;
			List<IPGADataCorrection> list;
			if (!fieldTrackers.TryGetValue(name, out list))
			{
				list = new List<IPGADataCorrection>();
				fieldTrackers.Add(name, list);
			}
			if (!list.Contains(tracker))
			{
				list.Add(tracker);
			}
		}

		#endregion

		#region Container Field Trackers

		string[] GetRelatedContainerFields(IPGADataCorrection tracker)
		{
			return tracker.GetRelatedContainerFields().Where(x => !string.IsNullOrEmpty(x)).ToArray();
		}

		void RemoveContainerFieldTracker(JobComInvoiceLine invoiceLine, IPGADataCorrection tracker)
		{
			var fieldNames = GetRelatedContainerFields(tracker);
			if (fieldNames.Length > 0)
			{
				var containersPivot = invoiceLine.ContainersPivot;
				List<IPGADataCorrection> list;
				if (ContainerTrackers.TryGetValue(containersPivot, out list))
				{
					list.Remove(tracker);
					if (list.Count == 0)
					{
						ContainerTrackers.Remove(containersPivot);
						containersPivot.CountChanged -= ContainersPivot_CountChanged;
					}
				}
				foreach (var fieldName in fieldNames)
				{
					list = null;
					if (ContainerFieldTrackers.TryGetValue(fieldName, out list) && list.Contains(tracker))
					{
						list.Remove(tracker);
						if (list.Count == 0)
						{
							UnHookContainersEvents(containersPivot, fieldName);
							ContainerFieldTrackers.Remove(fieldName);
						}
					}
				}
			}
		}

		void AddContainerFieldTracker(JobComInvoiceLine invoiceLine, IPGADataCorrection tracker)
		{
			var fieldNames = GetRelatedContainerFields(tracker);
			if (fieldNames.Length > 0)
			{
				var containersPivot = invoiceLine.ContainersPivot;
				List<IPGADataCorrection> list;
				if (!ContainerTrackers.TryGetValue(containersPivot, out list))
				{
					list = new List<IPGADataCorrection>();
					ContainerTrackers.Add(containersPivot, list);
					containersPivot.CountChanged -= ContainersPivot_CountChanged;
					containersPivot.CountChanged += ContainersPivot_CountChanged;
				}
				list.Add(tracker);
				foreach (var fieldName in fieldNames)
				{
					list = null;
					if (!ContainerFieldTrackers.TryGetValue(fieldName, out list))
					{
						list = new List<IPGADataCorrection>();
						ContainerFieldTrackers.Add(fieldName, list);
					}
					if (!list.Contains(tracker))
					{
						list.Add(tracker);
						HookContainersEvents(containersPivot, fieldName);
					}
				}
			}
		}

		void HookContainersEvents(CusContainersInvoiceLinesCollection containersPivot, string fieldName)
		{
			foreach (CusContainerInvoiceLinePivot pivot in containersPivot)
			{
				var container = pivot?.Container;
				if (container != null)
				{
					HookEvents(container, fieldName, ContainerDataChanged);
				}
			}
		}

		void UnHookContainersEvents(CusContainersInvoiceLinesCollection containersPivot, string fieldName)
		{
			foreach (CusContainerInvoiceLinePivot pivot in containersPivot)
			{
				var container = pivot?.Container;
				if (container != null)
				{
					UnHookEvents(container, fieldName, ContainerDataChanged);
				}
			}
		}

		void ContainersPivot_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var pivot = e.BizObject as CusContainerInvoiceLinePivot;
			if (pivot != null)
			{
				if (e.ItemAdded)
				{
					HookContainerChangedEvent(pivot.C2_COInfo);
					var container = pivot.Container;
					if (container != null)
					{
						HookContainerEvents(container);
					}
				}
				else if (e.ItemRemoved)
				{
					UnHookContainerChangedEvent(pivot.C2_COInfo);
					var container = pivot.Container;
					if (container != null)
					{
						UnHookContainerEvents(container);
					}
				}
			}
		}

		void HookContainerChangedEvent(ZPropertyInfo info)
		{
			info.ValueChanged -= ContainerChanged;
			info.ValueChanged += ContainerChanged;
		}

		void UnHookContainerChangedEvent(ZPropertyInfo info)
		{
			info.ValueChanged -= ContainerChanged;
		}

		void HookContainerEvents(CusContainer container)
		{
			if (container != null)
			{
				var fieldNames = ContainerFieldTrackers.Keys.ToArray();
				foreach (var fieldName in fieldNames)
				{
					HookEvents(container, fieldName, ContainerDataChanged);
				}
			}
		}

		void UnHookContainerEvents(CusContainer container)
		{
			if (container != null)
			{
				var fieldNames = ContainerFieldTrackers.Keys.ToArray();
				foreach (var fieldName in fieldNames)
				{
					UnHookEvents(container, fieldName, ContainerDataChanged);
				}
			}
		}

		void ContainerChanged(object sender, EventArgs e)
		{
			var valueE = e as ValueChangedEventArgs;
			if (valueE != null)
			{
				var fieldNames = ContainerFieldTrackers.Keys.ToArray();
				if (fieldNames.Length > 0)
				{
					var oldContainer = Factory.Load<CusContainer>((ZGuid)valueE.OldValue);
					var newContainer = Factory.Load<CusContainer>((ZGuid)valueE.NewValue);
					foreach (var fieldName in fieldNames)
					{
						UnHookEvents(oldContainer, fieldName, ContainerDataChanged);
						HookEvents(newContainer, fieldName, ContainerDataChanged);
					}
				}
			}
		}

		void ContainerDataChanged(object sender, EventArgs e)
		{
			var valueE = e as ValueChangedEventArgs;
			if (valueE != null)
			{
				var name = valueE.Info.Name;
				List<IPGADataCorrection> list;
				if (ContainerFieldTrackers.TryGetValue(name, out list))
				{
					foreach (var tracker in list.ToArray())
					{
						tracker.RelatedDataChanged();
					}
				}
			}
		}

		Dictionary<CusContainersInvoiceLinesCollection, List<IPGADataCorrection>> ContainerTrackers
		{
			get { return containerTrackers ?? (containerTrackers = new Dictionary<CusContainersInvoiceLinesCollection, List<IPGADataCorrection>>()); }
		}
		Dictionary<CusContainersInvoiceLinesCollection, List<IPGADataCorrection>> containerTrackers;

		Dictionary<string, List<IPGADataCorrection>> ContainerFieldTrackers
		{
			get { return containerFieldTrackers ?? (containerFieldTrackers = new Dictionary<string, List<IPGADataCorrection>>()); }
		}
		Dictionary<string, List<IPGADataCorrection>> containerFieldTrackers;

		#endregion
	}
}

// Test are in PGADataCorrectionlTest
