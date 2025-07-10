using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseCusLinkPackageCollection : NonPersistentBusinessObjectCollection<BaseCusLinkPackage>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public BaseCusLinkPackageCollection(ICusLinkPackageSupporter supporter)
			: base(supporter.Factory)
		{
			Supporter = supporter;
			RebuildElements();
		}

		public ICusLinkPackageSupporter Supporter { get; }

		#region Implement

		public void RebuildElements()
		{
			RemoveAll();

			var declaration = Supporter.Declaration;
			var packages = declaration?.Packages;

			if (packages != null && Supporter.IsSupportPivot)
			{
				packages.CountChanged -= SyncLinkPackages;

				if (declaration.IsImportingData)
				{
					declaration.Packages.Load(); // needed during UXML import.
				}

				foreach (var package in packages.Cast<BasePackage>().ToArray())
				{
					package.CW_CW_ParentInfo.ValueChanged -= SyncLinkPackages;
					package.CW_CW_ParentInfo.ValueChanged += SyncLinkPackages;

					if (package.IsLowestPackage && ShouldAddItem(package))
					{
						AddLinkPackage(package);
					}
				}

				packages.CountChanged += SyncLinkPackages;
			}
			ActionAfterRebuilt();
		}

		protected virtual void ActionAfterRebuilt()
		{
		}

		protected virtual ZBool ShouldAddItem(BasePackage package) => ZBool.True;

		void SyncLinkPackages(object sender, EventArgs e)
		{
			RebuildElements();
			RefreshBinding();
		}

		void AddLinkPackage(BasePackage package)
		{
			var linkPackage = (BaseCusLinkPackage)CreateNonPersistentBusinessObject();
			linkPackage.Package = package;

			Add(linkPackage);
		}

		#endregion

		#region override

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BaseCusLinkPackage(Supporter);
		}

		#endregion
	}
}
