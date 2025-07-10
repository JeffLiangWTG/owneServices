#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FactoryChangeMonitor : IDisposable
	{
		bool hasSavedChanges;
		readonly ICollection<BusinessObject> hookedObjects = new List<BusinessObject>();

		public BusinessObjectFactory Factory { get; }
		public bool WasModified => hasSavedChanges || HasPendingChanges();
		bool HasPendingChanges()
		{
			var allBizos = ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects;
			return allBizos.Count != hookedObjects.Count || allBizos.Any(bo => bo.HasChanges || !bo.IsInDatabase);
		}

		public FactoryChangeMonitor(BusinessObjectFactory factory)
		{
			Factory = factory;

			Factory.Loaded += HookLoadedBizos;
			Factory.Saved += NotifyFactoryModified;

			HookBizos(((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects);
		}

		void HookLoadedBizos(object sender, LoadedEventArgs e) => HookBizos(e.NewObjects);
		void NotifyFactoryModified(object sender, bool successful) => hasSavedChanges = true;
		void NotifyBizoModifiedByRefresh(object sender, EventArgs e) => hasSavedChanges = true;

		void HookBizos(IEnumerable<BusinessObject> bizos)
		{
			foreach (var bizo in bizos)
			{
				bizo.UpdatedByDataRefresh += NotifyBizoModifiedByRefresh;
				hookedObjects.Add(bizo);
			}
		}

		public void Dispose()
		{
			Factory.Loaded -= HookLoadedBizos;
			Factory.Saved -= NotifyFactoryModified;
			hookedObjects.ForEach(bo => bo.UpdatedByDataRefresh -= NotifyBizoModifiedByRefresh);
		}
	}
}

#endif
