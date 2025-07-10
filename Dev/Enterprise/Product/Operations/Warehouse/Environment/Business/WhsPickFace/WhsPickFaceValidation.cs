using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceValidation : AutoWhsPickFaceValidation
	{
		public WhsPickFaceValidation(AutoWhsPickFace parent)
			: base(parent)
		{
		}

		protected new WhsPickFace Parent => (WhsPickFace)base.Parent;

		#region CheckWF_ReplenishMinimum

		protected override void CheckWF_ReplenishMinimum()
		{
			if (Parent.WF_ReplenishMinimum < 0)
			{
				Parent.WF_ReplenishMinimumInfo.AddError(
					Res.GetString("31142759-4b1d-4ad7-a322-f9b13e2efa07", "Please enter an amount greater than or equal to zero."));
			}
		}

		#endregion

		#region CheckWF_ReplenishMaximum

		protected override void CheckWF_ReplenishMaximum()
		{
			if (Parent.WF_ReplenishMaximum <= 0)
			{
				Parent.WF_ReplenishMaximumInfo.AddError(
					Res.GetString("c2ca522a-a9e1-4ab7-8cbf-da196ad91ffb", "Please enter an amount greater than zero."));
			}
			else if (Parent.WF_ReplenishMaximum <= Parent.WF_ReplenishMinimum)
			{
				Parent.WF_ReplenishMaximumInfo.AddError(
					Res.GetString("08b2991e-b39a-40e7-86ef-bf4f5b0ecbbd", "Please enter an amount greater than minimum."));
			}
		}

		#endregion

		#region CheckWF_ReplenishMultiple

		protected override void CheckWF_ReplenishmentMultiple()
		{
			var pickFace = Parent;
			var part = pickFace.SupplierPart;
			var smallestStockKeepingUnitSize = (part != null ? part.SmallestStockKeepingUnitSize : 1m);

			if (pickFace.WF_ReplenishMaximum > pickFace.WF_ReplenishMinimum)
			{
				if (pickFace.WF_ReplenishmentMultiple < smallestStockKeepingUnitSize)
				{
					pickFace.WF_ReplenishmentMultipleInfo.AddError(Res.GetString("ce41da2f-87a4-40d7-9210-f93c925f76a7",
						"Please enter a Replenishment Multiple greater than or equal to {0}.", smallestStockKeepingUnitSize));
				}
				else if (pickFace.WF_ReplenishmentMultiple > pickFace.WF_ReplenishMaximum - pickFace.WF_ReplenishMinimum)
				{
					pickFace.WF_ReplenishmentMultipleInfo.AddError(Res.GetString("35d153b6-64f9-4c25-bde2-888688df3638",
						"Please enter a Replenishment Multiple less than or equal to {0}.", pickFace.WF_ReplenishMaximum - pickFace.WF_ReplenishMinimum));
				}
			}

			base.CheckWF_ReplenishmentMultiple();
		}

		#endregion

		#region Client

		bool AssignedToDynamicPickFaceArea()
		{
			var pickFace = Parent;
			var part = pickFace.SupplierPart;
			var warehouse = pickFace.Warehouse;
			var client = pickFace.Client;

			var result = false;
			if (part != null && warehouse != null && client != null)
			{
				var query = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, pickFace.WF_OP);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_WW, warehouse.PK);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OH, pickFace.WF_OH_Client);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea, SQLComparisonOperator.NotEqual, null);

				var productParams = pickFace.Factory.LoadTop1<IWhsProductParamsByWhsAndClient>(query);
				result = productParams != null;
			}

			return result;
		}

		protected override void CheckWF_OH_Client()
		{
			MandatoryValidation.CheckEntered(Parent.WF_OH_ClientInfo);

			if (!Parent.WF_OH_ClientInfo.HasErrors() && AssignedToDynamicPickFaceArea())
			{
				Parent.WF_OH_ClientInfo.AddError(Res.GetString("5ec7f1aa-a0e0-4466-8a4c-f7fafda84688",
						"Product is also assigned to a dynamic pick face area."));
			}
		}

		#endregion

		#region Warehouse

		public void ValidateWarehouse()
		{
			ValidateCalculatedProperty(Parent.LocationWhsGuidInfo);
		}

		protected virtual void CheckLocationWhsGuid()
		{
			if (AssignedToDynamicPickFaceArea())
			{
				Parent.LocationWhsGuidInfo.AddError(Res.GetString("5ec7f1aa-a0e0-4466-8a4c-f7fafda84688",
						"Product is also assigned to a dynamic pick face area."));
			}
		}

		#endregion

		#region LocationString

		public void ValidateLocationString()
		{
			ValidateCalculatedProperty(Parent.LocationStringInfo);
		}

		protected virtual void CheckLocationString()
		{
			WhsLocation.ValidateLocation(Parent, Parent.LocationStringInfo);

			if (!Parent.LocationStringInfo.HasErrors())
			{
				var location = Parent.Location;
				if (!location.IsFixedLocation)
				{
					Parent.LocationStringInfo.AddError(Res.GetString("8083E05E-6524-4c58-A6DC-579732AC03EC", "A Fixed pick face location must be selected in a Pick Face."));
				}
			}

			if (!Parent.LocationStringInfo.HasErrors())
			{
				var filter = new ZQuery(WhsPickFaceSchema.WF_OH_Client, Parent.WF_OH_Client);
				filter.AddToFilter(WhsPickFaceSchema.WF_OP, Parent.WF_OP);
				filter.AddToFilter(WhsPickFaceSchema.WF_WL, Parent.WF_WL);
				filter.AddToFilter(WhsPickFaceSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsPickFace>(filter) != null)
				{
					Parent.LocationStringInfo.AddError(Res.GetString("6972c593-9740-4a01-a413-630fbb6988ec", "There is already a Pick Face with the same Client, Warehouse and Location."));
				}

				CheckLocationForMaxDifferentProductType();
			}
		}

		#region CheckLocationForMaxDifferentProductType

		void CheckLocationForMaxDifferentProductType()
		{
			var locationTypeValue = Parent.Location.LocationType;
			if (locationTypeValue != null && locationTypeValue.WLT_LocationClass == LocationClasses.Codes.FIX)
			{
				var maximumNumberOfProducts = locationTypeValue.WLT_MaximumNumberOfProducts;

				var filter = new ZQuery(WhsPickFaceSchema.WF_WL, Parent.WF_WL);
				var currentNumberOfProduct = Parent.Factory.Load<WhsPickFace>(filter).Length;
				if (currentNumberOfProduct > maximumNumberOfProducts)
				{
					Parent.LocationStringInfo.AddError(
						Res.GetString(
							"ceb2591f-7363-45e0-b1b3-55339f39ea98",
							"The number of products assigned to this fixed pick face location ({0}) exceeds the maximum allowable ({1})",
							currentNumberOfProduct,
							maximumNumberOfProducts));
				}
			}
		}

		#endregion

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocationString();
			ValidateWarehouse();
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> info.Name != WhsPickFaceSchema.Constants.WF_WL && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
