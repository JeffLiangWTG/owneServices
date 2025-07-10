//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsVASOrderValidation
//
//    This class should be used for overriding validation in AutoWhsVASOrderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderValidation : AutoWhsVASOrderValidation
	{
		public WhsVASOrderValidation(AutoWhsVASOrder parent)
			: base(parent)
		{
		}

		new WhsVASOrder Parent
		{
			get { return (WhsVASOrder)base.Parent; }
		}

		// persistent

		#region CheckWVO_CustomerReferenceNo

		protected override void CheckWVO_CustomerReferenceNo()
		{
			base.CheckWVO_CustomerReferenceNo();

			if (Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.WVO_CustomerReferenceNoInfo);
			}

			CheckWVO_CustomerReferenceNoIsUniquePerClient();
		}

		void CheckWVO_CustomerReferenceNoIsUniquePerClient()
		{
			if (!Parent.WVO_CustomerReferenceNo.IsEmpty
				&& Parent.WVO_OH_Client.IsValid
				&& !Parent.WVO_OH_ClientInfo.HasErrors()
				&& !Parent.WVO_CustomerReferenceNoInfo.HasErrors()
				&& (!Parent.IsInDatabase || Parent.WVO_CustomerReferenceNoInfo.HasChanges || Parent.WVO_OH_ClientInfo.HasChanges))
			{
				var vasOrderQuery = new ZQuery(WhsVASOrderSchema.WVO_OH_Client, Parent.WVO_OH_Client);
				vasOrderQuery.AddToFilter(WhsVASOrderSchema.WVO_CustomerReferenceNo, Parent.WVO_CustomerReferenceNo);
				vasOrderQuery.AddToFilter(WhsVASOrderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<WhsVASOrder>(vasOrderQuery) != null)
				{
					Parent.WVO_CustomerReferenceNoInfo.AddError(Res.GetString("620d6ac6-820a-4b41-9a99-79e4a119a7df", "Customer Reference Number must be unique per Client."));
				}
			}
		}

		#endregion

		#region CheckWVO_WA_ServiceArea

		protected override void CheckWVO_WA_ServiceArea()
		{
			base.CheckWVO_WA_ServiceArea();

			CheckAreaIsFreeStore();
			CheckAreaHasAtLeastOneLocation();
		}

		void CheckAreaIsFreeStore()
		{
			if (!Parent.WVO_WA_ServiceAreaInfo.HasErrors() && Parent.WVO_WA_ServiceArea.IsValid)
			{
				var area = Parent.ServiceArea;
				if (area != null && !area.WA_AreaType.EqualsIgnoringCase(AreaTypes.Codes.FreeStore))
				{
					Parent.WVO_WA_ServiceAreaInfo.AddError(Res.GetString("0d206b61-ce88-4d64-bab5-c141fd7c19ca", "Service Area should be a {0} Area.", AreaTypes.Descriptions.FreeStore));
				}
			}
		}

		void CheckAreaHasAtLeastOneLocation()
		{
			if (!Parent.WVO_WA_ServiceAreaInfo.HasErrors() && Parent.WVO_WA_ServiceArea.IsValid)
			{
				var area = Parent.ServiceArea;
				if (area != null && area.PutawayLocations.Count < 1)
				{
					Parent.WVO_WA_ServiceAreaInfo.AddError(Res.GetString("e741ff25-bdd0-41d4-ae93-f86dc74cd0a5", "Service Area should have at least one Location."));
				}
			}
		}

		#endregion

		// calculated

		#region ValidateWarehousePK

		public void ValidateWarehousePK()
		{
			ValidateCalculatedProperty(Parent.WarehousePKInfo);
		}

		protected void CheckWarehousePK()
		{
			MandatoryValidation.CheckEntered(Parent.WarehousePKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.WarehousePKInfo);
		}

		#endregion

		//

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWarehousePK();
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> WhsVASOrderSchema.Constants.WVO_WA_ServiceArea != info.Name && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
