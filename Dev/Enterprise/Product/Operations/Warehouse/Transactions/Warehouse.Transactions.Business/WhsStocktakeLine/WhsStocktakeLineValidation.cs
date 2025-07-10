using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeLineValidation : AutoWhsStocktakeLineValidation
	{
		public WhsStocktakeLineValidation(AutoWhsStocktakeLine parent)
			: base(parent)
		{
		}

		#region CheckWU_GS_NKVerifiedBy

		protected override void CheckWU_GS_NKVerifiedBy()
		{
			ListValidation.ErrorIfInvalidCode(Parent.WU_GS_NKVerifiedByInfo);
		}

		#endregion

		#region CheckWU_Count2VerifiedBy

		protected override void CheckWU_Count2VerifiedBy()
		{
			ListValidation.ErrorIfInvalidCode(Parent.WU_Count2VerifiedByInfo);
			ListValidation.ErrorIfCancelledAndEditable(Parent.WU_Count2VerifiedByInfo);
		}

		#endregion

		#region CheckWU_Count3VerifiedBy

		protected override void CheckWU_Count3VerifiedBy()
		{
			ListValidation.ErrorIfInvalidCode(Parent.WU_Count3VerifiedByInfo);
			ListValidation.ErrorIfCancelledAndEditable(Parent.WU_Count3VerifiedByInfo);
		}

		#endregion

		#region CheckWU_DateVerified

		protected override void CheckWU_DateVerified()
		{
			var parent = Parent;
			if (parent.WU_Status == StocktakeLineStatus.Codes.Open
				&& parent.WU_TotalCounts == 1
				&& parent.WU_DateVerified > ZDateTime.Now)
			{
				parent.WU_DateVerifiedInfo.AddWarning(CannotSelectPastVerfiedDate);
			}
		}

		#endregion

		#region CheckWU_ExpiryDateIsValidZDateRange

		protected override void CheckWU_ExpiryDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.WU_ExpiryDateInfo);
		}

		protected override void CheckWU_ExpiryDateIsValidZDateRange()
		{
			var limits = new TypeValidationLimits { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears };
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WU_ExpiryDateInfo);
		}

		#endregion

		#region CheckWU_PackingDateIsValidZDateRange

		protected override void CheckWU_PackingDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.WU_PackingDateInfo);
		}

		protected override void CheckWU_PackingDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateRange(Parent.WU_PackingDateInfo);
		}

		#endregion

		#region CheckWU_Count2DateVerified

		protected override void CheckWU_Count2DateVerified()
		{
			var parent = Parent;
			if (parent.WU_Status == StocktakeLineStatus.Codes.Open
				&& parent.WU_TotalCounts == 2
				&& parent.WU_Count2DateVerified > ZDateTime.Now)
			{
				parent.WU_Count2DateVerifiedInfo.AddWarning(CannotSelectPastVerfiedDate);
			}
		}

		#endregion

		#region CheckWU_Count3DateVerified

		protected override void CheckWU_Count3DateVerified()
		{
			var parent = Parent;
			if (parent.WU_Status == StocktakeLineStatus.Codes.Open
				&& parent.WU_TotalCounts == 3
				&& parent.WU_Count3DateVerified > ZDateTime.Now)
			{
				parent.WU_Count3DateVerifiedInfo.AddWarning(CannotSelectPastVerfiedDate);
			}
		}

		#endregion

		#region Check last counts

		#region CheckWU_LastCount

		protected override void CheckWU_LastCount()
		{
			base.CheckWU_LastCount();

			CheckCount(Parent.WU_LastCountInfo, 1, Parent.WU_LastCount);
			CheckCountIsDivisibleByPerPackageQty(Parent.WU_LastCountInfo, 1);
			CheckCount_AllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(Parent.WU_LastCountInfo, 1);
			CheckCount_DoesNotOverflowLocation(Parent.WU_LastCountInfo, 1, Parent.WU_LastCount);
		}

		#endregion

		#region CheckWU_Count2

		protected override void CheckWU_Count2()
		{
			base.CheckWU_Count2();

			CheckCount(Parent.WU_Count2Info, 2, Parent.WU_Count2);
			CheckCountIsDivisibleByPerPackageQty(Parent.WU_Count2Info, 2);
			CheckCount_AllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(Parent.WU_Count2Info, 2);
			CheckCount_DoesNotOverflowLocation(Parent.WU_Count2Info, 2, Parent.WU_Count2);
		}

		#endregion

		#region CheckWU_Count3

		protected override void CheckWU_Count3()
		{
			base.CheckWU_Count3();

			CheckCount(Parent.WU_Count3Info, 3, Parent.WU_Count3);
			CheckCountIsDivisibleByPerPackageQty(Parent.WU_Count3Info, 3);
			CheckCount_AllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(Parent.WU_Count3Info, 3);
			CheckCount_DoesNotOverflowLocation(Parent.WU_Count3Info, 3, Parent.WU_Count3);
		}

		#endregion

		#region CheckCount

		void CheckCount(ZPropertyInfo info, ZByte countNumber, ZDecimal count)
		{
			var parent = Parent;
			if (parent.WU_TotalCounts == countNumber)
			{
				var stocktake = parent.Stocktake;
				if (count > 1 && stocktake != null && parent.Product != null
					&& parent.Product.IsSerialNumberUsedAndNotReleaseCaptured(stocktake.Client))
				{
					if (SkipCheckPickModeOnOrgPartRelation || IsAttributeSpecified(parent.Product.Parent, stocktake.Client))
					{
						info.AddError(PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
					}
				}
				else
				{
					CompareValidation.CheckNumberNotNegative(info);
				}
			}
		}

		bool IsAttributeSpecified(OrgSupplierPart part, OrgHeader client)
		{
			var relation = part.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			return relation.OU_PickMode == WhsPickMode.Codes.AttributeSpecified;
		}

		protected virtual bool SkipCheckPickModeOnOrgPartRelation
		{
			get { return false; }
		}

		#endregion

		#region CheckCountIsDivisibleByPerPackageQty

		void CheckCountIsDivisibleByPerPackageQty(ZPropertyInfo info, ZByte countNumber)
		{
			if (Parent.WU_TotalCounts == countNumber)
			{
				Helper.CheckUnitsIsDivisibleByPerPackageQty(info);
			}
		}

		#endregion

		#region CheckCount_AllSameProductsWithSamePackageGroupIDHaveSameTotalPacks

		void CheckCount_AllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(ZPropertyInfo info, ZByte countNumber)
		{
			if (Parent.WU_TotalCounts == countNumber)
			{
				var stocktake = Parent.Stocktake;
				if (stocktake != null && stocktake.CountryCode == Constants.CountryCodes.UnitedStates)
				{
					Helper.CheckAllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(info, Parent.WU_PerPackageQtyInfo);
				}
			}
		}

		#endregion

		#region CheckCount_DoesNotOverflowLocation

		void CheckCount_DoesNotOverflowLocation(ZPropertyInfo info, ZByte countNumber, ZDecimal count)
		{
			var line = Parent;
			var stocktake = line.Stocktake;
			if (!line.IsClosed && line.WU_TotalCounts == countNumber && count > 0 && stocktake != null && (line.Location?.HasCapacityLimitation ?? false))
			{
				var location = line.Location;
				var requiredCapacityInfo = stocktake.LocationCapacityValidationManager.GetLocationRequiredCapacity(location);
				var availableCapacityInfo = stocktake.LocationCapacityValidationManager.GetLocationAvailableCapacity(location);

				stocktake.LocationCapacityValidationManager.ValidateForLocationCapacity(info, location, availableCapacityInfo, requiredCapacityInfo);
			}
		}

		#endregion

		#endregion

		#region ValidateLocationString

		public virtual void ValidateLocationString()
		{
			//No location validation if the line is automatically added line.
		}

		#endregion

		#region CheckWU_OP

		protected override void CheckWU_OP()
		{
			CheckWU_OPIsNotEmpty();
			base.CheckWU_OP();
		}

		#endregion

		#region CheckWU_OPIsNotEmpty

		void CheckWU_OPIsNotEmpty()
		{
			MandatoryValidation.CheckEntered(Parent.WU_OPInfo);
		}

		#endregion

		#region Implementation

		protected new WhsStocktakeLine Parent
		{
			get { return (WhsStocktakeLine)base.Parent; }
		}

		public static string CannotSelectInvalidStaffCode
		{
			get { return Res.GetString("4b230e47-ecc3-498e-8c8a-98ac11a088cd", "Please enter a valid staff code."); }
		}

		public static string CannotSelectPastVerfiedDate
		{
			get { return Res.GetString("9f6dbf46-b2ea-4433-858d-2eb68de91df6", "Date is in future!"); }
		}

		public static string CountShouldBeZeroForEmptyLocation
		{
			get { return Res.GetString("ae12caba-9876-469b-9394-2fb1a25f3811", "Count should be zero for empty location"); }
		}

		public static string ValueHasToBeTrimmed
		{
			get { return Res.GetString("B979D4F9-EF72-4F56-924B-47E973B3A9ED", "This value cannot begin or end with white-spaces."); }
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsStocktakeLineSchema.Constants.WU_WL, WhsStocktakeLineSchema.Constants.WU_WS, WhsStocktakeLineSchema.Constants.WU_F3_NKPackType);

		internal StocktakeLineValidationHelper Helper
		{
			get { return helper ?? (helper = new StocktakeLineValidationHelper(Parent)); }
		}

		StocktakeLineValidationHelper helper;

		#endregion
	}

	public class EmptyWhsStocktakeLineValidation : WhsStocktakeLineValidation
	{
		public EmptyWhsStocktakeLineValidation(AutoWhsStocktakeLine parent)
			: base(parent)
		{
		}

		#region CheckWU_OP

		public static string ProductShouldBeEmpty
		{
			get { return Res.GetString("be608758-df05-4791-9f02-4f5e4702e794", "Product should be empty."); }
		}

		protected override void CheckWU_OP()
		{
			if (!Parent.WU_OP.IsEmpty)
			{
				Parent.WU_OPInfo.AddError(ProductShouldBeEmpty);
			}
		}

		#endregion

		#region CheckWU_OP

		public static string ClientShouldBeEmpty
		{
			get { return Res.GetString("470bb129-23ae-4c31-980c-382617dc0590", "Client should be empty."); }
		}

		protected override void CheckWU_OH_Client()
		{
			if (!Parent.WU_OH_Client.IsEmpty)
			{
				Parent.WU_OH_ClientInfo.AddError(ClientShouldBeEmpty);
			}
		}

		#endregion

		#region CheckWU_LastCount

		protected override void CheckWU_LastCount()
		{
			if (Parent.WU_LastCount != 0)
			{
				Parent.WU_LastCountInfo.AddError(CountShouldBeZeroForEmptyLocation);
			}
		}

		#endregion

		#region CheckWU_Count2

		protected override void CheckWU_Count2()
		{
			base.CheckWU_Count2();

			if (Parent.WU_Count2 != 0)
			{
				Parent.WU_Count2Info.AddError(CountShouldBeZeroForEmptyLocation);
			}
		}

		#endregion

		#region CheckWU_Count3

		protected override void CheckWU_Count3()
		{
			base.CheckWU_Count3();

			if (Parent.WU_Count3 != 0)
			{
				Parent.WU_Count3Info.AddError(CountShouldBeZeroForEmptyLocation);
			}
		}

		#endregion
	}
}
