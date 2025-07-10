using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class BasePackagePivotCollection<TChild, TMaster> : DependentBusinessObjectCollection<TChild, TMaster>, IBasePackagePivotCollection
		where TChild : BusinessObject, ICusPackagePivot
		where TMaster : BusinessObject, ICusLinkPackageSupporter
	{
		protected BasePackagePivotCollection(ICusLinkPackageSupporter supporter)
			: base((TMaster)supporter)
		{
			Supporter = supporter;
		}

		public ICusLinkPackageSupporter Supporter { get; }

		public abstract override Type GetTypeOfElementsFromPK(ZGuid pk);

		protected override bool AllowNewCore => false;

		protected abstract IDependentBusinessObjectCollection GetPivotCollection(BasePackage package);

		#region Get Pivot

		public ICusPackagePivot GetRelatedPivot(BasePackage package)
		{
			return package != null ? this.Cast<ICusPackagePivot>().FirstOrDefault(c => c.PackagePK == package.PK && !c.IsDeleted) : null;
		}

		#endregion

		#region Add Pivot

		public ICusPackagePivot AddPivotFor(BasePackage package)
		{
			var pivot = GetRelatedPivot(package);

			if (pivot == null)
			{
				pivot = AddNew();
				pivot.PackagePK = package.PK;
				if (package.IsLowestPackage)
				{
					pivot.NumberOfPacks = CalculateDefaultNumberOfPacks(package);
				}
				else
				{
					pivot.NumberOfPacks = Supporter.GetChildPackageUsage(package);
				}
				var quantityPivot = pivot as ICusQuantityPivot;
				if (quantityPivot != null)
				{
					quantityPivot.Quantity = CalculateDefaultQuantity(package);
				}
				var collection = GetPivotCollection(package);
				collection?.Add(pivot);
			}

			return pivot;
		}

		protected virtual ZDecimal CalculateDefaultQuantity(BasePackage package)
		{
			return ZDecimal.Zero;
		}

		ZInt CalculateDefaultNumberOfPacks(BasePackage package)
		{
			var result = package.CW_PackQty;
			var declaration = Supporter.Declaration;

			if (declaration != null)
			{
				var residualvalue = package.CW_PackQty - package.TotalUsageCount;
				result = (residualvalue > 0) ? residualvalue : 0;
			}

			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			if (!(Master is BaseJobDeclaration) && Master.Declaration is BaseJobDeclaration declaration)
			{
				((TChild)dependent).DeclarationPK = declaration.PK;
			}
		}

		#endregion

		#region Delete Pivot

		public void DeletePivotFor(BasePackage package)
		{
			var pivot = GetRelatedPivot(package) as BusinessObject;

			if (pivot != null)
			{
				RemoveAndDelete(pivot);

				var collection = (BusinessObjectCollection)GetPivotCollection(package);
				collection?.Remove(pivot);

				Supporter.SyncParentPivotPackNum(package?.ParentPackage);
			}

			package?.Validation.ValidateCW_PackQty();
		}

		#endregion
	}
}
