using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageItemDivotCollection : ActiveBusinessObjectCollection<PkgPackageItemDivot>, IPackingHasChanges
	{
		public PkgPackageItemDivotCollection(PkgPackage master)
			: base(master.Factory, master, new ZQuery(), PkgPackageItemDivotSchema.KI_KP_Package)
		{
		}

		#region FindByPackableItemParent_ForTesting
#if DEBUG
		/// <summary>
		/// Only used for tests, does not make sense to look for a single divot for a Packable Item in One Package anymore.
		/// </summary>
		internal PkgPackageItemDivot FindByPackableItemParent_ForTesting(IPackableItemParent packableItemParent)
		{
			Argument.NotNull(packableItemParent, nameof(packableItemParent));
			return this.FirstOrDefault(item => packableItemParent.PackableItems.Any(i => i.PK == item.KI_ParentID));
		}

#endif
		#endregion

		#region IPackingHasChanges Members

		bool IPackingHasChanges.HasChangesThatAreInvalidIfFinalised
		{
			// tested in IBusinessExtensionsTest.TestHasChangesOnChildrenNotValidIfFinalised()
			get { return this.HasChangesOnChildrenNotValidIfFinalised(); }
		}

		#endregion

		#region GetDivotWrappers

		internal IReadOnlyCollection<PkgPackageItemDivotsWrapper> GetDivotWrappers()
		{
			var wrappers = new Dictionary<GroupingKey, PkgPackageItemDivotsWrapper>();
			var packageJob = ((PkgPackage)Relationship.Master)?.PackageJob;
			if (packageJob != null)
			{
				var parentJob = packageJob.ParentJob as IPackingParentWithPackableItems;
				parentJob?.LoadAllPackableItemParentsInOneHit(packageJob);

				foreach (var divot in this)
				{
					var packedItem = divot.PackedItem;
					if (packedItem != null)
					{
						PkgPackageItemDivotsWrapper existingWrapper;

						var key = packedItem.Key;
						if (!wrappers.TryGetValue(key, out existingWrapper))
						{
							var packableItemParent = GetPackableItemParentFromKey(packageJob, key);
							var wrapper = PkgPackageItemDivotsWrapper.New(divot, packableItemParent);
							wrappers.Add(key, wrapper);
						}
						else
						{
							existingWrapper.AddDivotToWrapper(divot);
						}
					}
				}
			}

			return wrappers.Values.ToArray();
		}

		static IPackableItemParent GetPackableItemParentFromKey(PkgPackageJob packageJob, GroupingKey key)
		{
			IPackableItemParent result;

			if (key != null)
			{
				// on the very slim chance the Packable item is 'valid' with no Parent in the Packable Item Parents Collection, we should show 'Item no longer exists'
				result = packageJob.PackableItemParents.GetPackableItemParentFromKey(key) ?? EmptyPackableItemParent.Instance;
			}
			else
			{
				result = EmptyPackableItemParent.Instance;
			}

			return result;
		}

		#endregion
	}
}
