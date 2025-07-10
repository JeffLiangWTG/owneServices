using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class BaseCusLinkPackage : NonPersistentBusinessObject<BaseCusLinkPackageValidation>, IObsoleteValidation
	{
		#region Schema
		public static class Schema
		{
			public const string PackQty = "PackQty";
		}
		#endregion

		public BaseCusLinkPackage(ICusLinkPackageSupporter supporter)
			: base(supporter.Factory)
		{
			Supporter = supporter;
		}

		ICusLinkPackageSupporter Supporter { get; }

		#region Properties

		#region IsLinked

		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(IsLinked_ReadOnly))]
		public virtual ZBool IsLinked
		{
			get => Pivot != null;
			set
			{
				Supporter.ToggleLinkageWithPackage(Package, value);

				if (!IsValidationSuspended)
				{
					ValidateAllIsLinked();
					ValidateAllPackageQty();
					ValidateAllQuantity();
				}

				IsLinkedInfo.RefreshBinding();
				PackQtyInfo.RefreshBinding();
				QuantityInfo.RefreshBinding();
			}
		}

		protected virtual bool IsLinked_ReadOnly => false;

		void ValidateAllIsLinked()
		{
			Validation.ValidateIsLinked();

			var parentCollection = ParentCollections.FirstOrDefault(c => c is BaseCusLinkPackageCollection);
			var otherLinkPackages = parentCollection?.Cast<BaseCusLinkPackage>().Where(c => c != this);

			if (otherLinkPackages != null)
			{
				foreach (var package in otherLinkPackages)
				{
					package.Validation.ValidateIsLinked();
				}
			}
		}

		void ValidateAllPackageQty()
		{
			Validation.ValidatePackQty();

			var parentCollection = ParentCollections.FirstOrDefault(c => c is BaseCusLinkPackageCollection);
			var otherLinkPackages = parentCollection?.Cast<BaseCusLinkPackage>().Where(c => c != this);

			if (otherLinkPackages != null)
			{
				foreach (var package in otherLinkPackages)
				{
					package.Validation.ValidatePackQty();
				}
			}
		}

		void ValidateAllQuantity()
		{
			Validation.ValidateQuantity();
			var parentCollection = ParentCollections.FirstOrDefault(c => c is BaseCusLinkPackageCollection);
			var otherLinkPackages = parentCollection?.Cast<BaseCusLinkPackage>().Where(c => c != this);

			if (otherLinkPackages != null)
			{
				foreach (var package in otherLinkPackages)
				{
					package.Validation.ValidateQuantity();
				}
			}
		}

		public ZPropertyInfo IsLinkedInfo => GetZPropertyInfo(nameof(IsLinked));

		#endregion

		#region PackQty

		[ResourceStringData("BaseInvoiceLineUserControl|11111111-1111-4e91-b699-705b07f9f4ee", Caption = "Pack Quantity")]
		[ReadOnlyMember(nameof(IsPackQty_ReadOnly))]
		public virtual ZInt PackQty
		{
			get => Pivot?.NumberOfPacks ?? ZInt.Zero;
			set
			{
				if (Pivot != null)
				{
					Pivot.NumberOfPacks = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidatePackQty();
					}
				}
				else
				{
					ReportErrorIfPivotIsEmpty();
				}

				PackQtyInfo.RefreshBinding();
			}
		}

		public bool IsPackQty_ReadOnly => GetIsPackQty_ReadOnlyCore();

		protected virtual bool GetIsPackQty_ReadOnlyCore() => !IsLinked;

		public ZPropertyInfo PackQtyInfo => GetZPropertyInfo(Schema.PackQty);

		[ReadOnlyMember(nameof(IsQuantity_ReadOnly))]
		public ZDecimal Quantity
		{
			get => QuantityPivot?.Quantity ?? ZDecimal.Zero;
			set
			{
				if (QuantityPivot != null)
				{
					QuantityPivot.Quantity = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuantity();
					}
				}
				else
				{
					ReportErrorIfPivotIsEmpty();
				}

				QuantityInfo.RefreshBinding();
			}
		}

		public bool IsQuantity_ReadOnly => GetIsQuantity_ReadOnlyCore();

		protected virtual bool GetIsQuantity_ReadOnlyCore() => !IsLinked;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		protected void ReportErrorIfPivotIsEmpty()
		{
			var message = Res.GetString("67a93465-da6a-45d3-b21c-bed64b80daf4", "There is no pivot at this point. Please tick {0} first.", IsLinkedInfo.HumanReadableName);
			ErrorReporter.ReportOnce(message);
		}

		#endregion

		#region PackageNumber

		[ResourceStringData("BaseInvoiceLineUserControl|f8d2fc87-1111-4373-ad00-6e53d355e36a", Caption = "Package Number")]
		public ZString PackageNumber => GetPackageNumberDesc();

		protected virtual ZString GetPackageNumberDesc()
		{
			var package = Package;
			var result = string.Empty;

			if (package != null)
			{
				var maybeContainerised = package.CW_ContainerNoOrEquipmentNo.IsEmpty ? string.Empty : Res.GetString("7331A12A-BAEE-409B-AB2D-0019A6300ACB", "in {0} ", package.CW_ContainerNoOrEquipmentNo);
				var maybeMarked = package.CW_MarksAndNos.IsEmpty ? string.Empty : Res.GetString("C06A07AD-639F-4CF6-B96E-04A116EDEC57", "marked '{0}' ", package.CW_MarksAndNos);
				var forBill = Res.GetString("FA848731-E521-42B7-A1D8-CA016AD87F3B", "for {0}", package.CW_HouseBill);
				result = Res.GetString("65C691E2-3983-447B-A4F9-9C224DDA26B3", "{0}{1} {2}{3}{4}", package.CW_PackQty, package.CW_PackType, maybeContainerised, maybeMarked, forBill);
			}

			return result;
		}

		public ZPropertyInfo PackageNumberInfo => GetZPropertyInfo(nameof(PackageNumber));

		#endregion

		#endregion

		#region override

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
		}

		protected sealed override void OnFactorySaving()
		{
		}

		public sealed override void OnSaved(bool saveSucceeded)
		{
		}

		public sealed override void OnSaving()
		{
		}

		#endregion

		#region Implement

		public ICusPackagePivot Pivot => Package != null ? Supporter?.CusPackPivots.GetRelatedPivot(Package) : null;
		public ICusQuantityPivot QuantityPivot => Pivot != null && Pivot is ICusQuantityPivot ? (ICusQuantityPivot)Pivot : null;

		public BasePackage Package
		{
			get
			{
				return fPackage != null && !fPackage.IsDeleted
					? fPackage
					: null;
			}
			set
			{
				fPackage = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsLinked();
					ValidateAllPackageQty();
				}

				PackQtyInfo.RefreshBinding();
			}
		}
		BasePackage fPackage;

		public ZGuid PackagePk
		{
			get => Package?.PK ?? ZGuid.Empty;
			set => fPackage = Factory.Load<BasePackage>(value);
		}

		#endregion

		#region Validation

		public override BaseCusLinkPackageValidation GetNewValidation()
		{
			return Supporter.GetNewLinkPackValidation(this);
		}

		public void ValidateQuantity()
		{
			Validation.ValidateQuantity();
		}

		#endregion
	}
}
