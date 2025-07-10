using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageCollection : PkgPackageCollectionCommon, IPackingHasChanges
	{
		#region Construction

		public PkgPackageCollection(PkgPackageJob master)
			: base(master)
		{
		}

		public PkgPackageCollection(PkgPackageJob master, ZQuery filter)
			: base(master, filter)
		{
		}

		public PkgPackageCollection(PkgPackage master)
			: base(master.Factory, master, new ZQuery(), PkgPackageSchema.KP_KP_ParentPackage)
		{
			if (!master.IsDeletingPackage && !master.KP_KJ_ParentPackageJob.IsValid) // don't check this when deleting (binding can create a dodgy uncommitted package and then delete it)
			{
				throw new ArgumentException("The Master Package must have a link to the PackageJob, otherwise the link cannot be propogated to child packages.");
			}
		}

		/// <summary>
		/// WARNING -- THIS CONSTRUCTOR SHOULD NOT BE USED DIRECTLY (it exists for the module grid).
		/// </summary>
		public PkgPackageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Add / AddNew

		public PkgPackage AddNew(ZString packageType, int qty = 1)
		{
			return AddNew(packageType, "", qty);
		}

		public PkgPackage AddNew(ZString packageType, ZString packageID)
		{
			return AddNew(packageType, packageID, 1);
		}

		PkgPackage AddNew(ZString packageType, ZString packageID, int qty)
		{
			PkgPackage result = null;

			using (SuspendDefaultingPackTypeSemaphore())
			using (SuspendNotificationsChanged())
			{
				result = AddNew();
			}

			result.KP_F3_NKPackType = packageType;

			if (!packageID.IsEmpty)
			{
				result.KP_PackageID = packageID;
			}

			result.KP_PackageQty = qty;

			return result;
		}

		protected override ZString GetPackTypeForNewElement(PkgPackage newElement)
		{
			return newElement.IsOuter ? PackageJob.LastUsedOuterPackType : PackageJob.LastUsedInnerPackType;
		}

		protected override void SetRelationshipDefaultsForElementCore(PkgPackage newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);

			if (Relationship.Master is PkgPackageJob)
			{
				newElement.KP_KP_ParentPackage = ZGuid.Empty;
			}
		}

		protected override PkgPackageJob PackageJob
		{
			get
			{
				var package = Relationship.Master as PkgPackage;
				return (package != null) ? package.PackageJob : (PkgPackageJob)Relationship.Master;
			}
		}

		#endregion

		#region ToStringSummary

		public ZString ToStringSummary(int maxPackageTypeToDisplay = 3)
		{
			ZString result = "";

			if (Count > 0)
			{
				var dict = new SortedDictionary<ZString, int>();

				// build dictionary of package types
				foreach (var package in this)
				{
					var key = package.KP_F3_NKPackType.IsEmpty ? package.EmptyPackageCode : package.KP_F3_NKPackType;
					if (dict.ContainsKey(key))
					{
						dict[key] += package.KP_PackageQty;
					}
					else
					{
						dict.Add(key, package.KP_PackageQty);
					}
				}

				// build a string description of the first n package types
				foreach (var keyValuePair in dict.Take(maxPackageTypeToDisplay))
				{
					result += Res.GetString("5a5f399e-670a-4671-82af-67fad8f29036", "{0}x {1},", keyValuePair.Value, keyValuePair.Key) + " ";
				}
				result = result.TrimEndIncludingWhiteSpace(',');

				if (dict.Count > maxPackageTypeToDisplay)
				{
					result += "...";
				}
			}

			return result;
		}

		#endregion

		#region IPackingHasChanges Members

		bool IPackingHasChanges.HasChangesThatAreInvalidIfFinalised
		{
			// tested in IBusinessExtensionsTest.TestHasChangesOnChildrenNotValidIfFinalised()
			get { return this.HasChangesOnChildrenNotValidIfFinalised(); }
		}

		#endregion

		#region Override

		protected override bool RunPreSaveValidationCore()
		{
			AddFetchHintsForDefaultPrinter(); // tested in WhsOrderTest.TestFinaliseDocket_DBHits_WithPackageAudit
			return base.RunPreSaveValidationCore();
		}

		void AddFetchHintsForDefaultPrinter()
		{
			foreach (var package in this)
			{
				Factory.AddFetchHint(StmDefaultPrinterSchema.SDP_SubjectID, package.PK);
			}
		}

		#endregion
	}
}

