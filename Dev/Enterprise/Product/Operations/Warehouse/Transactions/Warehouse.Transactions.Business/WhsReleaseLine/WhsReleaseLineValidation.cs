using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReleaseLineValidation : ZValidation
	{
		public WhsReleaseLineValidation(WhsReleaseLine releaseLine)
			: base(releaseLine)
		{
		}

		WhsReleaseLine Parent => (WhsReleaseLine)ParentFilter;

		public override Type AutoValidationType => typeof(WhsReleaseLineValidation);

		#region Part Attributes

		#region ValidatePartAttribute1

		public void ValidatePartAttribute1()
		{
			ValidateCalculatedProperty(Parent.PartAttribute1Info);
		}

		protected void CheckPartAttribute1()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.PartAttribute1Info);
			CheckPartAttribute(1, Parent.PartAttribute1Info);
		}

		#endregion

		#region ValidatePartAttribute2

		public void ValidatePartAttribute2()
		{
			ValidateCalculatedProperty(Parent.PartAttribute2Info);
		}

		protected void CheckPartAttribute2()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.PartAttribute2Info);
			CheckPartAttribute(2, Parent.PartAttribute2Info);
		}

		#endregion

		#region ValidatePartAttribute3

		public void ValidatePartAttribute3()
		{
			ValidateCalculatedProperty(Parent.PartAttribute3Info);
		}

		protected void CheckPartAttribute3()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.PartAttribute3Info);
			CheckPartAttribute(3, Parent.PartAttribute3Info);
		}

		#endregion

		void CheckPartAttribute(int attributeNumber, ZPropertyInfo partAttributeInfo)
		{
			if (!partAttributeInfo.HasErrors() && !Parent.IsDeleted && Parent.ParentCollection.IsDuplicateReleaseLine(Parent))
			{
				partAttributeInfo.AddError(Res.GetString("3df79894-5e7f-4dcd-9eda-2acd919071d7", "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line."));
			}

			if (Parent.Quantity != 0m && !partAttributeInfo.HasErrors())
			{
				CheckAllocationsAndPartAttributeTypeRules(Parent.PickableDocket,
					partAttributeInfo,
					(product, client) => product.IsPartAttribReleaseCaptured(client, attributeNumber),
					(client, part, propertyInfo) => PartAttributeValidation.CheckAttribute(client, part, propertyInfo, attributeNumber));
			}
		}

		void CheckAllocationsAndPartAttributeTypeRules(WhsPickableDocket docket,
			ZPropertyInfo partAttributeInfo,
			Func<WhsProduct, OrgHeader, bool> isPartAttributeReleaseCapturedFunc,
			Action<OrgHeader, OrgSupplierPart, ZPropertyInfo> attributeValidationAction)
		{
			var product = docket != null ? Parent.Product : null;
			if (product != null)
			{
				var client = docket.Client;
				var isEmpty = Lazy.Create(() => partAttributeInfo.Value.IsEmpty);
				var isReleaseCaptured = Lazy.Create(() => isPartAttributeReleaseCapturedFunc(product, client));

				if (!partAttributeInfo.HasErrors()
					&& !isEmpty.Value
					&& isReleaseCaptured.Value
					&& Parent.ParentCollection.IsReleaseCapturedAttributeUnallocated(Parent))
				{
					partAttributeInfo.AddError(Res.GetString("fdc8c116-8602-4fc5-a203-7a8c7e19df42", "This Attribute cannot be Release Captured for the Product because there is not enough Stock Allocated to this Order Line."));
				}

				if (!partAttributeInfo.HasErrors()
					// don't validate empty release captured attributes till finalisation
					&& (docket.IsFinalising
					|| docket.IsFinalised
					|| !isEmpty.Value
					|| !isReleaseCaptured.Value)
					// don't validate empty attributes for component line Release Lines
					&& IsPartAttributeValidationRequired())
				{
					attributeValidationAction(client, product.Parent, partAttributeInfo);
				}
			}
		}

		bool IsPartAttributeValidationRequired()
		{
			// we should not check attributes for release lines created for BOM picked products, as those release lines will have no attributes by design
			/*
			 For example: 2 release lines are there
			 * Quantity		Part Attribute 1
			 * 2					PA1
			 * 3
			 * 
			 * That means 3 was created for child lines or any other line with empty attribute.
			 * We will not check this release line if attribute is empty && TotalPickLineQuantityFromComponents for that line is less then or equal to this Released Quantity
			 * See the test bellow: TestBOMReleaseLinePartAttributeValidation()
			 */

			return (!Parent.IsBOMProductPickedOnSalesOrder || (Parent.AllAttributesEmpty && Parent.TotalPickLineQuantityFromComponents > Parent.Quantity));
		}

		PartAttributeValidation PartAttributeValidation => new WhsReleaseLinePartAttributeValidation(Parent);

		#endregion

		#region ValidateSerialNumber

		public void ValidateSerialNumber()
		{
			ValidateCalculatedProperty(Parent.SerialNumberInfo);
		}

		protected void CheckSerialNumber()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.SerialNumberInfo);
			CheckSerialNumberCore(Parent.SerialNumberInfo);
		}

		void CheckSerialNumberCore(ZPropertyInfo partAttributeInfo)
		{
			if (!partAttributeInfo.HasErrors() && !Parent.IsDeleted && Parent.ParentCollection.IsDuplicateReleaseLine(Parent))
			{
				partAttributeInfo.AddError(Res.GetString("3df79894-5e7f-4dcd-9eda-2acd919071d7", "This Release Line's Part Attribute Combination is duplicated, either change the Part Attribute Combination or delete this line."));
			}

			if (Parent.Quantity != 0m && !partAttributeInfo.HasErrors())
			{
				CheckAllocationsAndPartAttributeTypeRules(
					Parent.PickableDocket,
					partAttributeInfo,
					(product, client) => product.IsSerialNumberReleaseCaptured(client),
					PartAttributeValidation.CheckSerialNumber);
			}
		}

		#endregion

		#region ValidateQuantity

		public void ValidateQuantity()
		{
			ValidateCalculatedProperty(Parent.QuantityInfo);
		}

		protected void CheckQuantity()
		{
			TypeValidation.CheckValidDecimal(Parent.QuantityInfo, WhsPickLineSchema.WZ_Units.Precision, WhsPickLineSchema.WZ_Units.Scale);
			MandatoryValidation.CheckNotNegative(Parent.QuantityInfo);

			AddErrorForZeroQuantityReleaseLines();
			CheckNotOverReleasingOrderedQty();
			CheckNotOverReleasingPickLines();
			CheckNotUnderReleasingPickLines();
			CheckQuantityWhenSerialProduct();
			CheckQuantityDoesNotReduceBelowPackedQty();
		}

		void CheckNotOverReleasingOrderedQty()
		{
			if (!Parent.QuantityInfo.HasErrors() && Parent.ParentCollection.IsOrderLineOverReleased)
			{
				Parent.QuantityInfo.AddError(Res.GetString("c10298db-3bfe-43c6-8aa5-59484def5136", "Quantity Met cannot be greater than Quantity Ordered"));
			}
		}

		void CheckNotOverReleasingPickLines()
		{
			if (!Parent.QuantityInfo.HasErrors() && Parent.ParentCollection.IsReleaseLineUnderCaptured(Parent))
			{
				Parent.QuantityInfo.AddError(Res.GetString("77d38862-70fd-4518-b0a7-83dfce7493d1", "The Release Captured Quantity for this Product cannot be greater than the Quantity Allocated for this Order Line."));
			}
		}

		void CheckNotUnderReleasingPickLines()
		{
			if (!Parent.QuantityInfo.HasErrors() && Parent.ParentCollection.IsAnyReleaseCapturedQuantityBelowPickedQuantity(Parent))
			{
				Parent.QuantityInfo.AddError(Res.GetString("2df25d34-6d45-4db0-bec8-f6fd499026da", "You must Release Capture the same Quantity that has been Picked for this Order Line."));
			}
		}

		void AddErrorForZeroQuantityReleaseLines()
		{
			if (Parent.Quantity == 0m)
			{
				Parent.QuantityInfo.AddError(Res.GetString("d724b557-eb50-45f4-8ee3-30453804510a", "You cannot release 0 units."));
			}
		}

		void CheckQuantityWhenSerialProduct()
		{
			if (!Parent.QuantityInfo.HasErrors())
			{
				// we don't do Release Captured Validation for Work Orders
				if (Order != null)
				{
					PartAttributeValidation.CheckQtyForSerialNumber(Parent.Product, Parent.Client, Parent.QuantityInfo, Parent.SerialNumberInfo, checkForReleaseSerial: true);
				}
			}
		}

		void CheckQuantityDoesNotReduceBelowPackedQty()
		{
			if (!Parent.QuantityInfo.HasErrors() && Parent.IsPacked)
			{
				var qtyPacked = Parent.GetPackedQty();

				// this sucks but until a better way is thought up, must be done this way.
				var qtyToCheck = Parent.InvalidQuantityThatWasReversed ?? Parent.Quantity;
				if (qtyToCheck < qtyPacked)
				{
					Parent.QuantityInfo.AddError(Res.GetString("81adf6df-44b5-48a0-b5b5-89acd12fe3ec",
						"This item is packed.\r\n\r\nQuantity released ({0}) cannot be less than quantity packed ({1}). Reduce the quantity packed first.", qtyToCheck, qtyPacked));
				}
			}
		}

		WhsOrder Order => Parent.PickableDocket as WhsOrder;

		#endregion

		#region ValidateUnreleasedQty

		public void ValidateUnreleasedQty()
		{
			ValidateCalculatedProperty(Parent.UnreleasedQtyInfo);
		}

		protected void CheckUnreleasedQty()
		{
			if (((IBusinessObjectInternals)Parent).IsInPreSaveValidation && Parent.UnreleasedQty != 0m)
			{
				Parent.UnreleasedQtyInfo.AddError(Res.GetString("7770e1ef-ec16-4a47-b05a-53ebf3ff5c3f", "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated."));
			}
		}

		#endregion

		//

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidatePartAttribute1();
			ValidatePartAttribute2();
			ValidatePartAttribute3();
			ValidateSerialNumber();
			ValidateQuantity();
			ValidateUnreleasedQty();
		}

		#endregion
	}
}
