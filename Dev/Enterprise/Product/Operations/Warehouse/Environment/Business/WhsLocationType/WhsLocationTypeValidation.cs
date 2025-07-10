using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationTypeValidation : AutoWhsLocationTypeValidation
	{
		#region Constructor

		public WhsLocationTypeValidation(AutoWhsLocationType parent)
			: base(parent)
		{
		}

		#endregion

		#region CheckWLT_Code

		protected override void CheckWLT_Code()
		{
			base.CheckWLT_Code();

			MandatoryValidation.CheckEntered(Parent.WLT_CodeInfo);
			CheckCodeIsUnique();
		}

		void CheckCodeIsUnique()
		{
			if (!Parent.WLT_CodeInfo.HasErrors())
			{
				var query = new ZQuery(WhsLocationTypeSchema.WLT_Code, Parent.WLT_Code);
				query.AddToFilter(WhsLocationTypeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsLocationType>(query) != null)
				{
					Parent.WLT_CodeInfo.AddError(Res.GetString("AF94BC9E-D7B9-47E8-A1B4-A0FA65AD1638", "Code must be unique."));
				}
			}
		}

		#endregion

		#region CheckWLT_MaximumNumberOfProducts

		protected override void CheckWLT_MaximumNumberOfProducts()
		{
			base.CheckWLT_MaximumNumberOfProducts();

			if (Parent.WLT_LocationClass != LocationClasses.Codes.FIX)
			{
				if (Parent.WLT_MaximumNumberOfProducts != 0)
				{
					Parent.WLT_MaximumNumberOfProductsInfo.AddError(Res.GetString("803A29D8-0FF0-4400-A79A-9A1F982A5005", "Maximum Number Of Products can only be 0 for this location class."));
				}
			}
			else
			{
				CompareValidation.CheckGreaterThanOrEqualTo(Parent.WLT_MaximumNumberOfProductsInfo, 1);
			}
		}

		#endregion

		#region CheckWLT_MinimumTemperature

		protected override void CheckWLT_MinimumTemperature()
		{
			base.CheckWLT_MinimumTemperature();

			if (Parent.WLT_LocationClass != LocationClasses.Codes.TCL)
			{
				if (Parent.WLT_MinimumTemperature != 0)
				{
					Parent.WLT_MinimumTemperatureInfo.AddError(Res.GetString("17b735f7-7942-4e6e-8f14-6ff058ab8777", "Minimum Temperature can only be 0 for this location class."));
				}
			}
			else
			{
				CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.WLT_MinimumTemperatureInfo, Parent.WLT_MaximumTemperatureInfo);
			}
		}

		#endregion

		#region CheckWLT_MaximumTemperature

		protected override void CheckWLT_MaximumTemperature()
		{
			base.CheckWLT_MaximumTemperature();

			if (Parent.WLT_LocationClass != LocationClasses.Codes.TCL)
			{
				if (Parent.WLT_MaximumTemperature != 0)
				{
					Parent.WLT_MaximumTemperatureInfo.AddError(Res.GetString("158703ba-f014-4f53-84bf-f5283f6ec034", "Maximum Temperature can only be 0 for this location class."));
				}
			}
			else
			{
				CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.WLT_MaximumTemperatureInfo, Parent.WLT_MinimumTemperatureInfo);
			}
		}

		#endregion

		#region CheckWLT_TemperatureUnit

		protected override void CheckWLT_TemperatureUnit()
		{
			base.CheckWLT_TemperatureUnit();

			if (Parent.WLT_LocationClass == LocationClasses.Codes.TCL)
			{
				if (Parent.WLT_TemperatureUnit.IsEmpty)
				{
					Parent.WLT_TemperatureUnitInfo.AddError(Res.GetString("d7da6d11-a65b-4ad0-9f70-fd19370ecf62", "Temperature Unit should be set for this location class."));
				}

				ListValidation.ErrorIfInvalidCode(Parent.WLT_TemperatureUnitInfo);
			}
			else if (!Parent.WLT_TemperatureUnit.IsEmpty)
			{
				Parent.WLT_TemperatureUnitInfo.AddError(Res.GetString("638cd239-47e0-4071-9b9e-a9787d1c377c", "Temperature Unit should not be set for this location class."));
			}
		}

		#endregion

		#region CheckWLT_LocationClass

		protected override void CheckWLT_LocationClass()
		{
			base.CheckWLT_LocationClass();
			ListValidation.ErrorIfInvalidCode(Parent.WLT_LocationClassInfo);

			CheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsWarehouseDefaultDDL();
			CheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsPicksDDL();
			CheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsPicksPST();
			CheckWLT_LocationClass_CannotChangeFromFIXIfLocationIsUsedInPickFace();
			CheckWLT_LocationClass_MustNotBeUsedOnLocationWithPendingAvailableStock();
		}

		void CheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsWarehouseDefaultDDL()
		{
			if (!Parent.WLT_LocationClassInfo.HasErrors() && Parent.WLT_LocationClassInfo.HasChanges)
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsWarehouseSchema.WW_DefaultOutboundDockDoor);
				locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WLT_LocationType, Parent.PK);

				var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
				warehouseQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

				if (Parent.Factory.LoadTop1<WhsWarehouse>(warehouseQuery) != null)
				{
					Parent.WLT_LocationClassInfo.AddError(Res.GetString("03611D8A-3B9D-43DB-A908-97645BA2B558", "Location Class cannot be changed if it is used on warehouse default outbound dock door location."));
				}
			}
		}

		void CheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsPicksDDL()
		{
			CheckWLT_LocationClass_CannotChangeIfLocationIsUsedOnPickCore(
				LocationClasses.Codes.DDL,
				WhsPickSchema.WP_WL_DockDoor,
				() => Res.GetString("B61A3A1A-74D5-469E-B8D0-E76B970C7D3F", "Location Class cannot be changed if it is used on pick as dock door location."));
		}

		void CheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsPicksPST()
		{
			CheckWLT_LocationClass_CannotChangeIfLocationIsUsedOnPickCore(
				LocationClasses.Codes.PST,
				WhsPickSchema.WP_WL_PackingStation,
				() => Res.GetString("9def50d8-8c00-49c3-9ee0-8c73bc415f14", "Location Class cannot be changed if it is used on pick as packing station location."));
		}

		void CheckWLT_LocationClass_CannotChangeIfLocationIsUsedOnPickCore(string locationClass, SchemaGuidColumn column, Func<string> getErrorMessage)
		{
			if (!Parent.WLT_LocationClassInfo.HasErrors() && Parent.WLT_LocationClassInfo.HasChanges &&
				Parent.WLT_LocationClassInfo.OriginalValue.Equals(locationClass))
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), column);
				locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WLT_LocationType, Parent.PK);

				var pickQuery = new ZDBOnlyQuery(typeof(IWhsPick));
				pickQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

				if (Parent.Factory.ExistsInDatabase(WhsPickSchema.Constants.TableName, pickQuery))
				{
					Parent.WLT_LocationClassInfo.AddError(getErrorMessage());
				}
			}
		}

		void CheckWLT_LocationClass_CannotChangeFromFIXIfLocationIsUsedInPickFace()
		{
			if (!Parent.WLT_LocationClassInfo.HasErrors() && Parent.WLT_LocationClassInfo.HasChanges &&
				Parent.WLT_LocationClassInfo.OriginalValue.Equals(LocationClasses.Codes.FIX))
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsPickFaceSchema.WF_WL);
				locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WLT_LocationType, Parent.PK);

				var pickFaceQuery = new ZDBOnlyQuery(typeof(WhsPickFace));
				pickFaceQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

				if (Parent.Factory.ExistsInDatabase(WhsPickFaceSchema.Constants.TableName, pickFaceQuery))
				{
					Parent.WLT_LocationClassInfo.AddError(Res.GetString("42AE28D5-9792-4b72-8354-D3641C4AD549", "For Location used in a Pick face the location type class must be FIX"));
				}
			}
		}

		void CheckWLT_LocationClass_MustNotBeUsedOnLocationWithPendingAvailableStock()
		{
			if (!Parent.WLT_LocationClassInfo.HasErrors() && Parent.WLT_LocationClassInfo.HasChanges)
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsDocketLineSchema.WE_WL);
				locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WLT_LocationType, Parent.PK);

				var docketLineQuery = new ZDBOnlyQuery(typeof(IWhsDocketLine));
				docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
				docketLineQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

				if (Parent.Factory.LoadTop1<IWhsDocketLine>(docketLineQuery) != null)
				{
					Parent.WLT_LocationClassInfo.AddError(Res.GetString("F5969ED2-F00D-42D2-A99F-B0267F63E31A", "Location Class cannot be changed if it is used on location with pending/available stock."));
				}
			}
		}

		#endregion

		#region CheckWLT_IsPalletIDNeutral

		protected override void CheckWLT_IsPalletIDNeutral()
		{
			base.CheckWLT_IsPalletIDNeutral();

			if (!Parent.WLT_IsPalletIDNeutralInfo.HasErrors() && Parent.WLT_IsPalletIDNeutral)
			{
				if (WhsLocationType.OutboundLocationClasses.Contains(Parent.WLT_LocationClass))
				{
					Parent.WLT_IsPalletIDNeutralInfo.AddError(Res.GetString("B5C25281-9CFB-40BE-A7D9-857539E26752", "Pallet ID Neutral cannot be checked for Location Class: {0}.", Parent.WLT_LocationClass));
				}
				else if (Parent.WLT_LocationClass == LocationClasses.Codes.FIX && !Parent.WLT_RetainPalletIDsInFixedPickFaces)
				{
					Parent.WLT_IsPalletIDNeutralInfo.AddError(Res.GetString("2b5b41d5-3aa5-4775-b814-dc55b5d22815", "Pallet ID Neutral cannot be checked for Location Class FIX when Retain Pallet IDs In Fixed Pick Faces is not checked."));
				}
			}
		}

		#endregion

		#region CheckWLT_Description

		protected override void CheckWLT_Description()
		{
			base.CheckWLT_Description();

			MandatoryValidation.CheckEntered(Parent.WLT_DescriptionInfo);
		}

		#endregion

		#region CheckWLT_DefaultCycleCountGranularity

		protected override void CheckWLT_DefaultCycleCountGranularity()
		{
			base.CheckWLT_DefaultCycleCountGranularity();

			if (WhsLocationType.IsCycleCountingSupportedForLocationClass(Parent.WLT_LocationClass))
			{
				MandatoryValidation.CheckEntered(Parent.WLT_DefaultCycleCountGranularityInfo);
			}
			else if (Parent.WLT_DefaultCycleCountGranularity != "")
			{
				var locationType = Parent.Lookups.LocationClasses.GetDescriptionFromCode(Parent.WLT_LocationClass);
				var errorMessage = Res.GetString(
					"5f95368c-bd47-4157-b59a-fbf1cb089b79",
					"{0} types cannot have a Default Cycle Count Granularity specified.",
					locationType);

				Parent.WLT_DefaultCycleCountGranularityInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckWLT_RetainPalletIDsInFixedPickFaces

		protected override void CheckWLT_RetainPalletIDsInFixedPickFaces()
		{
			base.CheckWLT_RetainPalletIDsInFixedPickFaces();

			if (!Parent.WLT_RetainPalletIDsInFixedPickFacesInfo.HasErrors() && Parent.WLT_RetainPalletIDsInFixedPickFaces && Parent.WLT_LocationClass != LocationClasses.Codes.FIX)
			{
				Parent.WLT_RetainPalletIDsInFixedPickFacesInfo.AddError(Res.GetString("CADF71E7-0242-452B-8DD0-FA875840736F", "Retain Pallet IDs In Fixed Pick Faces cannot be checked for Location Class: {0}.", Parent.WLT_LocationClass));
			}
		}

		#endregion
	}
}
