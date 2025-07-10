using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public abstract class PkgPackageCollectionCommon : ActiveBusinessObjectCollection<PkgPackage>
	{
		#region Construction

		protected PkgPackageCollectionCommon(PkgPackageJob master)
			: this(master.Factory, master, AddAdditionalFilter(null), PkgPackageSchema.KP_KJ_ParentPackageJob)
		{
		}

		protected PkgPackageCollectionCommon(PkgPackageJob master, ZQuery filter)
			: this(master.Factory, master, AddAdditionalFilter(filter), PkgPackageSchema.KP_KJ_ParentPackageJob)
		{
		}

		protected PkgPackageCollectionCommon(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected PkgPackageCollectionCommon(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaColumn relationshipColumn)
			: base(factory, master, filter, relationshipColumn)
		{
		}

		static ZQuery AddAdditionalFilter(ZQuery filter)
		{
			var result = new ZQuery();
			result.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);

			if (filter != null)
			{
				result.AddToFilter(filter);
			}

			return result;
		}

		#endregion

		#region SetDefaultsForNewElementCore

		protected override void SetDefaultsForNewElementCore(PkgPackage newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (!DefaultingPackTypeSemaphore.IsSuspended)
			{
				newElement.KP_F3_NKPackType = GetPackTypeForNewElement(newElement);
			}
		}

		protected IDisposable SuspendDefaultingPackTypeSemaphore() => new SemaphoreManager(DefaultingPackTypeSemaphore);

		readonly Semaphore DefaultingPackTypeSemaphore = new Semaphore();

		protected abstract ZString GetPackTypeForNewElement(PkgPackage newElement);

		#endregion

		#region Add

		protected sealed override void OnAdded(PkgPackage package)
		{
			base.OnAdded(package);

			package.KP_KJ_ParentPackageJob = PackageJob.PK; // link to packageJob for faster db querying..
			OnAddedCore(package);
		}

		protected virtual void OnAddedCore(PkgPackage package)
		{
		}

		protected abstract PkgPackageJob PackageJob { get; }

		#endregion
	}
}

