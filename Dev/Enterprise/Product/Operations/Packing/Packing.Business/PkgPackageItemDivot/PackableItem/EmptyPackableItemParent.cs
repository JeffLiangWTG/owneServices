using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
	[Immutable]
	internal class EmptyPackableItemParent : IPackableItemParent
	{
		EmptyPackableItemParent()
		{
		}

		ICustomPropertyContainer IPackableItemParent.AdditionalProperties => null;
		bool IPackableItemParent.IsDeleted => true;

		ZString IPackableItemParent.AutoPackPackageType
		{
			get { throw new NotSupportedException(); }
		}

		ZDecimal IPackableItemParent.AutoPackQtyPerPackage
		{
			get { throw new NotSupportedException(); }
		}

		ZString IPackableItemParent.Code
		{
			get { throw new NotSupportedException(); }
		}

		ZString IPackableItemParent.Description
		{
			get { throw new NotSupportedException(); }
		}

		ZString IPackableItemParent.DescriptionSupplement
		{
			get { throw new NotSupportedException(); }
		}

		ZString IPackableItemParent.DescriptionSupplementSeparator
		{
			get { throw new NotSupportedException(); }
		}

		IEnumerable<IPackableItem> IPackableItemParent.PackableItems
		{
			get { throw new NotImplementedException(); }
		}

		ZDecimal IPackableItemParent.TotalQty
		{
			get { throw new NotSupportedException(); }
		}

		ZString IPackableItemParent.TotalQtyUQ
		{
			get { throw new NotSupportedException(); }
		}

		Money IPackableItemParent.UnitPrice
		{
			get { throw new NotSupportedException(); }
		}

		ZDecimal IPackableItemParent.WeightPerUnit
		{
			get { throw new NotSupportedException(); }
		}

		ZString IPackableItemParent.WeightUQ
		{
			get { throw new NotSupportedException(); }
		}

		BarcodeMatch IPackableItemParent.IsMatch(string barcode)
		{
			throw new NotSupportedException();
		}

		void IPackableItemParent.RefreshPackableItems()
		{
			throw new NotSupportedException();
		}

		public static readonly EmptyPackableItemParent Instance = new EmptyPackableItemParent();
	}
}
