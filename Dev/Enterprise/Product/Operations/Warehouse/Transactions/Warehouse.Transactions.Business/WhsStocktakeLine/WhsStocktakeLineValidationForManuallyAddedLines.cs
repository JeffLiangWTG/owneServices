using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeLineValidationForManuallyAddedLines : WhsStocktakeLineValidation
	{
		public WhsStocktakeLineValidationForManuallyAddedLines(WhsStocktakeLine parent)
			: base(parent)
		{
		}

		#region CheckWU_BondedEntryKey

		protected override void CheckWU_BondedEntryKey()
		{
			base.CheckWU_BondedEntryKey();

			if (!Parent.WU_BondedEntryKeyInfo.HasErrors())
			{
				var stocktake = Parent.Stocktake;
				if (stocktake != null && stocktake.IsBondedWarehouse && Parent.WU_BondedEntryKey.IsEmpty)
				{
					var location = Parent.Location;
					if (location != null && location.WLV_PickingAreaType == AreaTypes.Codes.Bonded)
					{
						Parent.WU_BondedEntryKeyInfo.AddError(Res.GetString("79b87734-acf0-40e8-ab52-9aeb7bf90138", "Entry Number is mandatory for Locations in a Bonded Area."));
					}
				}
			}
		}

		#endregion

		#region CheckWU_OP

		protected override void CheckWU_OP()
		{
			var stocktake = Parent.Stocktake;
			if (stocktake != null)
			{
				WhsValidationHelper.CheckProductIsValid(Parent.SupplierPart, Parent.WU_OH_Client, Parent.WU_OPInfo);

				PartAttributeValidation.ValidateCanCalculateExpiryDateIfJulianBatchNumberIsUsed(Parent.Client, stocktake.Warehouse, Parent.Product, Parent.WU_OPInfo);
			}
			base.CheckWU_OP();
		}

		#endregion

		#region CheckWU_OH_Client

		protected override void CheckWU_OH_Client()
		{
			var product = Parent.Product;
			if (product != null)
			{
				if (!product.Parent.RelatedOrganisations.Cast<OrgPartRelation>().Any(org => org.OU_OH == Parent.WU_OH_Client && (org.IsBoth || org.IsOwner)))
				{
					Parent.WU_OH_ClientInfo.AddError(MustHaveProperClient);
				}
			}
		}

		#endregion

		#region CheckWU_InventoryStatus

		protected override void CheckWU_InventoryStatus()
		{
			var parent = Parent;
			if (parent.WU_IsManuallyAdded)
			{
				MandatoryValidation.CheckEntered(parent.WU_InventoryStatusInfo);
				ListValidation.ErrorIfInvalidCode(parent.WU_InventoryStatusInfo);
			}
		}

		#endregion

		#region Location

		public override void ValidateLocationString()
		{
			ValidateCalculatedProperty(Parent.LocationStringInfo);
		}

		protected void CheckLocationString()
		{
			var parent = Parent;
			WhsLocation.ValidateLocation(parent, parent.LocationStringInfo);
		}

		#endregion

		#region Part attributes

		#region CheckWU_PartAttrib1

		protected override void CheckWU_PartAttrib1()
		{
			base.CheckWU_PartAttrib1();

			CheckIfTrimIsNeeded(Parent.WU_PartAttrib1Info);
			CheckPartAttribute(Parent.WU_PartAttrib1Info, 1);
		}

		#endregion

		#region CheckWU_PartAttrib2

		protected override void CheckWU_PartAttrib2()
		{
			base.CheckWU_PartAttrib2();

			CheckIfTrimIsNeeded(Parent.WU_PartAttrib2Info);
			CheckPartAttribute(Parent.WU_PartAttrib2Info, 2);
		}

		#endregion

		#region CheckWU_PartAttrib3

		protected override void CheckWU_PartAttrib3()
		{
			base.CheckWU_PartAttrib3();

			CheckIfTrimIsNeeded(Parent.WU_PartAttrib3Info);
			CheckPartAttribute(Parent.WU_PartAttrib3Info, 3);
		}

		#endregion

		#region CheckWU_SerialNumber

		protected override void CheckWU_SerialNumber()
		{
			base.CheckWU_SerialNumber();

			CheckIfTrimIsNeeded(Parent.WU_SerialNumberInfo);
			CheckSerialNumber(Parent.WU_SerialNumberInfo);
		}

		void CheckSerialNumber(ZPropertyInfo attributeInfo)
		{
			var product = Parent.Product;
			if (product != null)
			{
				var client = Parent.Stocktake.Client;
				if (!product.IsSerialNumberReleaseCaptured(client))
				{
					PartAttributeValidation.CheckSerialNumber(client, product.Parent, attributeInfo);
				}

				PartAttributeValidation.ValidateSerialIsNotReleaseCapturedWithValue(product, client, attributeInfo);
			}
		}

		#endregion

		#region CheckWU_ExpiryDate

		protected override void CheckWU_ExpiryDateIsValidZDateRange()
		{
			base.CheckWU_ExpiryDateIsValidZDateRange();

			TypeValidation.CheckValidZDateTimeRange(Parent.WU_ExpiryDateInfo,
				new TypeValidationLimits() { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears });
		}

		protected override void CheckWU_ExpiryDate()
		{
			base.CheckWU_ExpiryDate();

			var parent = Parent;
			PartAttributeValidation.CheckExpiryDate(parent.Stocktake.Client, parent.SupplierPart, parent.WU_ExpiryDateInfo);
		}

		#endregion

		#region CheckWU_PackingDate

		protected override void CheckWU_PackingDate()
		{
			base.CheckWU_PackingDate();

			var parent = Parent;
			PartAttributeValidation.CheckPackingDate(parent.Stocktake.Client, parent.SupplierPart, parent.WU_PackingDateInfo);
		}

		#endregion

		void CheckIfTrimIsNeeded(ZPropertyInfo propertyInfo)
		{
			var propertyValue = (ZString)propertyInfo.Value;
			if (!propertyValue.Equals(propertyValue.Trim()))
			{
				propertyInfo.AddError(Res.GetString("F29B0FD5-676B-4762-90C9-032EE5ED5E30", "This value cannot begin or end with white-spaces."));
			}
		}

		void CheckPartAttribute(ZPropertyInfo attributeInfo, int attributeNumber)
		{
			var client = Parent.Stocktake.Client;
			var product = Parent.Product;
			if (product != null)
			{
				if (!product.IsPartAttribReleaseCaptured(client, attributeNumber))
				{
					PartAttributeValidation.CheckAttribute(client, product.Parent, attributeInfo, attributeNumber);
				}

				PartAttributeValidation.ValidatePartAttribIsNotReleaseCapturedWithValue(product, client, attributeInfo, attributeNumber);
				PartAttributeValidation.CheckJulianBatchNumberAttributeFormat(client, product, attributeInfo, attributeNumber);
			}
		}

		#endregion

		#region CheckWU_PalletID

		protected override void CheckWU_PalletID()
		{
			base.CheckWU_PalletID();

			var parent = Parent;

			if (parent.WU_WL.IsValid && !parent.WU_PalletID.IsEmpty)
			{
				var query = new ZQuery(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
				query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, parent.WU_PalletID);
				query.AddToFilter(WhsInventoryViewSchema.WI_WL, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

				var loadedInventoryCollection = parent.Factory.Load<WhsInventoryView>(query);
				var inventory = loadedInventoryCollection.FirstOrDefault(i => i.WI_WL != parent.WU_WL || (i.WI_WL == parent.WU_WL && i.IsInTransit));
				var isExistingPalletId = loadedInventoryCollection.Any(i => i.WI_WL == parent.WU_WL && !i.IsInTransit);
				var duplicatedLine = parent.Stocktake.Lines.FirstOrDefault(l => l.PK != parent.PK && l.WU_IsManuallyAdded && l.WU_WL != parent.WU_WL && l.WU_PalletID == parent.WU_PalletID);

				if (!isExistingPalletId && (inventory != null || duplicatedLine != null))
				{
					parent.WU_PalletIDInfo.AddError(WhsValidationHelper.GetDuplicatePalletIdMessage(inventory?.Location ?? duplicatedLine.Location));
				}
			}

			CheckIfTrimIsNeeded(Parent.WU_PalletIDInfo);
		}

		#endregion

		#region SkipCheckPickModeOnOrgPartRelation

		protected override bool SkipCheckPickModeOnOrgPartRelation
		{
			get { return true; }
		}

		#endregion

		#region ValidateDuplicateLine

		public void ValidateDuplicateLine()
		{
			var parent = Parent;
			var errorMessageSuffix = Res.GetString("397852b6-8f3f-4d9d-92e0-3fd3ff59af5a", "You must edit the existing stock take line. If you cannot see the line clear all filters.");
			parent.ClearRowNotificationsContaining(errorMessageSuffix);

			if (parent.WU_Status == StocktakeLineStatus.Codes.Open)
			{
				var line = parent.Stocktake.Lines.GetDuplicateStocktakeLine(Parent);
				if (line != null)
				{
					parent.AddRowError(Res.GetString("328f24a8-285e-4e9d-8769-aff810ab6146",
						"Product {0} in Location {1} with status {2} is already on this stocktake on Line {3}. {4}",
						parent.WU_OP_Desc, parent.LocationString, parent.WU_InventoryStatus, line.WU_LineNo, errorMessageSuffix));
				}
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocationString();
			ValidateDuplicateLine();
		}

		#endregion

		#region PartAttributeValidation

		PartAttributeValidation PartAttributeValidation
		{
			get { return partAttributeValidation ?? (partAttributeValidation = new PartAttributeValidation(Parent)); }
		}

		PartAttributeValidation partAttributeValidation;

		#endregion

		#region Error messages

		public static string MustHaveProperClient
		{
			get { return Res.GetString("5A30B93F-ED61-4960-8453-4F5D3EFA9147", "Selected client must be an Owner of the Product."); }
		}

		#endregion
	}
}
