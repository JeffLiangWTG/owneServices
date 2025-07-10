using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	interface IVoyagePortsCollection : IBusinessObjectCollection
	{
		IDisposable SuppressSailingGeneration();
		event EventHandler<CannotDeletePortEventArgs> CannotDeletePort;
	}

	public abstract class VoyagePortsCollection<TPort> : DependentBusinessObjectCollection<TPort, JobVoyage>, IVoyagePortsCollection
		where TPort : BusinessObject
	{
		public VoyagePortsCollection(JobVoyage parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			Voyage = parent;
		}

		protected readonly JobVoyage Voyage;

		public event EventHandler<CannotDeletePortEventArgs> CannotDeletePort;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var relatedSailings = new List<JobSailing>();
			if (Voyage != null)
			{
				relatedSailings.AddRange(Voyage.Sailings.Cast<JobSailing>().Where(sailing => (ZGuid)sailing[SailingFKColumn] == elementToDelete.PK));
			}

			var referencedRelatedSailing = relatedSailings.FirstOrDefault(sailing => sailing.IsReferenced());
			if (referencedRelatedSailing != null)
			{
				if (CannotDeletePort != null)
				{
					CannotDeletePort(this, new CannotDeletePortEventArgs(referencedRelatedSailing.GetReferencingJobNumbers()));
				}
			}
			else
			{
				foreach (JobSailing sailing in relatedSailings)
				{
					Voyage.Sailings.RemoveAndDelete(sailing);
				}

				base.RemoveAndDelete(elementToDelete);
			}
		}

		protected abstract SchemaGuidColumn SailingFKColumn { get; }

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			if (!IsLoading && !IsUpdatingByDataRefreshBus && !isSailingGenerationSuppressed)
			{
				Voyage.GenerateSailings(true);
			}

			base.OnAdded(bizOAdded);
		}

		public IDisposable SuppressSailingGeneration()
		{
			return new DisposableAction(() => isSailingGenerationSuppressed = true, () => isSailingGenerationSuppressed = false);
		}
		bool isSailingGenerationSuppressed;
	}
}
