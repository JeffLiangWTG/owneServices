using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business.Testing
{
	public abstract class ItemsBusinessObjectTestCase<T> : PackingNonPersistentBusinessObjectTestCase<T>
		where T : ItemsBusinessObject
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewItemsBusinessObject();
		}

		protected abstract T GetNewItemsBusinessObject();
		protected abstract Type ValidationType { get; }
		protected abstract ZString ProposedColumnName { get; }
		protected abstract ZString ProposedDescription { get; }

		protected IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> WrapPackedItemsWithBarcodeYes(params PkgPackageItemDivotsWrapper[] packedItems)
		{
			return packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, BarcodeMatch.Yes));
		}

		#region Validation

		public void TestValidation()
		{
			var bizO = GetNewItemsBusinessObject();
			AssertEquals(ValidationType, bizO.Validation.GetType());
			AssertNotEquals("Validation is lightweight and should not be cached.", bizO.Validation, bizO.Validation);
		}

		#endregion

		#region Column/Desc

		public void TestProposedPackUnpackColumnName()
		{
			AssertEquals(ProposedColumnName, GetNewItemsBusinessObject().ProposedPackUnpackColumnName);
		}

		public void TestProposedPackUnpackDescription()
		{
			AssertEquals(ProposedDescription, GetNewItemsBusinessObject().ProposedPackUnpackDescription);
		}

		#endregion

		#region TestSetPackOrRemoveQuantity

		public abstract void TestSetPackOrRemoveQuantity();

		#endregion
	}
}
