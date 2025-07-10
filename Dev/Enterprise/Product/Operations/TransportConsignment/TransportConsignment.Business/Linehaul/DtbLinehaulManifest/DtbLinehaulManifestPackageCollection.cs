using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestPackageCollection : ActiveBusinessObjectCollection<PkgPackage>
	{
		public DtbLinehaulManifestPackageCollection(DtbLinehaulManifest manifest)
			: base(manifest.Factory, new ManifestPackageRelationship(manifest, typeof(PkgPackage)))
		{
			this.manifest = manifest;

			manifest.Factory.Saving += (factory) => Update();
			AddRange(GetPackages(manifest.LHM_PackageString, Factory));
		}

		readonly DtbLinehaulManifest manifest;

		#region Implementation

		static IEnumerable<PkgPackage> GetPackages(string packageString, BusinessObjectFactory factory)
		{
			var result = new List<PkgPackage>();
			if (!string.IsNullOrEmpty(packageString.Trim()))
			{
				var packageIds = packageString.Split(',');
				foreach (var id in packageIds.Where(x => !string.IsNullOrEmpty(x.Trim())))
				{
					var package = DtbLinehaulManifest.LoadPackage(id.Trim(), factory);
					if (package != null && !result.Contains(package))
					{
						result.Add(package);
					}
				}
			}

			return result;
		}

		void Update()
		{
			if (!manifest.IsInDatabase)
			{
				manifest.LHM_PackageString = SerializePackagesToString();
			}
			else
			{
				var newPackageString = SerializePackagesToString();
				if (newPackageString != (ZString)manifest.LHM_PackageStringInfo.OriginalValue)
				{
					var newFactory = new BusinessObjectFactory { NameForDebugging = "DtbLinehaulManifestPackageCollection.Update" };
					LockManifestUntilTransactionCompletes();

					var reloadedManifest = newFactory.Load<DtbLinehaulManifest>(manifest.PK);

					var originalPackages = DtbLinehaulManifestPackageCollection.GetPackages((ZString)manifest.LHM_PackageStringInfo.OriginalValue, Factory).ToArray();

					var equalityComparer = new PackageEqualityComparer();
					var packagesOtherPersonRemoved = originalPackages.Except(reloadedManifest.Packages, equalityComparer).ToArray();
					var packagesIRemoved = originalPackages.Except(this, equalityComparer).ToArray();
					var packagesOtherPersonAdded = reloadedManifest.Packages.Except(originalPackages, equalityComparer).ToArray();
					var packagesIAdded = this.Except(originalPackages, equalityComparer).ToArray();

					foreach (var package in this.ToArray())
					{
						RemoveFromRelationship(package);
					}

					var packagesToAdd = originalPackages.Union(packagesIAdded).Union(packagesOtherPersonAdded).Except(packagesIRemoved).Except(packagesOtherPersonRemoved);
					AddRange(packagesToAdd);

					manifest.LHM_PackageString = SerializePackagesToString();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LockManifestUntilTransactionCompletes()
		{
			var sqlText = Invariant($@"
SELECT {DtbLinehaulManifestSchema.Constants.PK}
FROM {DtbLinehaulManifestSchema.Constants.SqlSchemaName}.{DtbLinehaulManifestSchema.Constants.TableName} WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
WHERE {DtbLinehaulManifestSchema.Constants.PK} = '{manifest.PK.ToString()}'
"); // SQL query to run in DB

			using (var command = ((IDbConnected)Factory).Connection.Command(sqlText))
			using (var reader = command.ExecuteReader())    // should block if anyone else is also reading this manifest using the same query
			{
#if DEBUG
				if (OnAfterManifestLockGrantedForTesting != null)
				{
					OnAfterManifestLockGrantedForTesting(this, EventArgs.Empty);
				}
#endif

				return;
			}

			// should only happen if the locks can't be granted within the SQL timeout
			throw new ZCannotSaveException("Only 1 user can modify the packages on a Linehaul manifest at any time.", "Cannot save");
		}

#if DEBUG
		internal event EventHandler OnAfterManifestLockGrantedForTesting;
#endif

		string SerializePackagesToString()
		{
			var ids = this.Where(x => !x.KP_PackageID.IsEmpty).OrderBy(x => x.KP_PackageID).Select(x => x.KP_PackageID).Distinct();
			return string.Join(",", ids);
		}

		class PackageEqualityComparer : IEqualityComparer<PkgPackage>
		{
			bool IEqualityComparer<PkgPackage>.Equals(PkgPackage x, PkgPackage y)
			{
				if (x == null && y == null)
				{
					return true;
				}
				else if ((x == null && y != null) || (y == null && x != null))
				{
					return false;
				}
				else
				{
					return x.PK == y.PK;
				}
			}

			int IEqualityComparer<PkgPackage>.GetHashCode(PkgPackage obj)
			{
				return obj != null ? obj.PK.GetHashCode() : 0;
			}
		}

		#endregion

		#region Relationship

		class ManifestPackageRelationship : AdhocCollectionRelationship
		{
			public ManifestPackageRelationship(DtbLinehaulManifest manifest, Type elementType)
				: base(elementType)
			{
				this.manifest = manifest;
			}

			readonly DtbLinehaulManifest manifest;

			protected override void RemoveFromRelationship(BusinessObject businessObject)
			{
				base.RemoveFromRelationship(businessObject);
				manifest.HasChanges = true;
			}
		}

		#endregion
	}
}
