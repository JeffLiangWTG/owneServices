using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	[SystemDefinedValues]
	public class PkgPackageItemDivot : AutoPkgPackageItemDivot, IPackingHasChanges, IPackageParent
	{
		public PkgPackageItemDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoPkgPackageItemDivot.Schema
		{
			public const string PkgNWUQ = "PkgNWUQ";
			public const string PkgNW = "PkgNW";
		}

		#region Related Entities

		#region PackedItem

		public IPackableItem PackedItem
		{
			get
			{
				var bizO = Factory.Load(KI_ParentTableCode, KI_ParentID);
				var result = bizO as IPackableItem;

				if (bizO != null && result == null)
				{
					throw new NotSupportedException("PkgPackageItemDivot.PackedItem must implement IPackableItem.");
				}

				return result;
			}
		}

		public ZDecimal PkgNetWeight
		{
			get => this.GetSystemDefinedValue<ZDecimal>(Schema.PkgNW);
			set
			{
				this.SetSystemDefinedValue(Schema.PkgNW, value);
				Validation.ValidataNetWeight();
				PkgNetWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PkgNetWeightInfo => GetZPropertyInfo(nameof(PkgNetWeight));

		[MaxLength(3)]
		[List("Lookups.WeightUQs")]
		public ZString PkgNetWeightUQ
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.PkgNWUQ);
			set
			{
				CheckMaximumLength(PkgNetWeightUQInfo, value);
				this.SetSystemDefinedValue(Schema.PkgNWUQ, value);
				Validation.ValidataNetWeightUQ();
				PkgNetWeightUQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PkgNetWeightUQInfo => GetZPropertyInfo(nameof(PkgNetWeightUQ));

		#endregion

		#region ParentPackage

		public PkgPackage ParentPackage
		{
			get { return Factory.Load<PkgPackage>(KI_KP_Package); }
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KI_KP_Package

		[RelatedBusinessObject("ParentPackage")]
		public override ZGuid KI_KP_Package
		{
			get { return base.KI_KP_Package; }
			set { base.KI_KP_Package = value; }
		}

		#endregion

		#region KI_ParentID

		[BusinessObjectTestExclude] // the setter throws an exception if you try to set a guid after a valid one is set
		public override ZGuid KI_ParentID
		{
			get { return base.KI_ParentID; }
			set
			{
				if (KI_ParentID.IsValid && value != KI_ParentID)
				{
					throw new InvalidOperationException("Should not change the PackableItem the Divot is pointing to.");
				}

				base.KI_ParentID = value;
			}
		}

		#endregion

		#endregion

		#region Business Object Overrides

		#region Delete

		public override void Delete()
		{
			BeforeDelete?.Invoke(this, EventArgs.Empty); // Before Delete must happen before Re-merging the Packed Item.
			PackedItem?.ReMerge();

			base.Delete();
		}

		internal event EventHandler BeforeDelete;

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = Factory.GetCachedValue("PkgPackageItemDivotUniqueIndexFailureHandler", () => new PackingUniqueIndexFailureHandler(new[]
				{
					PkgPackageItemDivotSchema.Constants.Indexes.FK_UX__KI_KP_Package_KI_ParentID,
					PkgPackageItemDivotSchema.Constants.Indexes.NR_UX__KI_ParentID
				})));
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#endregion

		internal void UpdateIPackableItem(IPackableItem packableItem)
		{
			base.KI_ParentID = packableItem.PK.ToGuid();
		}

		#region IPackingHasChanges Members

		bool IPackingHasChanges.HasChangesThatAreInvalidIfFinalised
		{
			// tested in IBusinessExtensionsTest.TestHasChangesOnChildrenNotValidIfFinalised()
			get { return ((IBusiness)this).HasChangesNotIncludingChildren; }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (KI_ParentID.IsEmpty)
			{
				KI_ParentID = ZGuid.NewZGuid();
				KI_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			}

			if (KI_PackedQty < 1)
			{
				KI_PackedQty = 1;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}
}
