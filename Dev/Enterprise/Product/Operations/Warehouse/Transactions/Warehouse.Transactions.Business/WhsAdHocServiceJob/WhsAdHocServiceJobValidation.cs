using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobValidation : AutoWhsAdHocServiceJobValidation
	{
		public WhsAdHocServiceJobValidation(AutoWhsAdHocServiceJob parent)
			: base(parent)
		{
		}

		new WhsAdHocServiceJob Parent
		{
			get { return (WhsAdHocServiceJob)base.Parent; }
		}

		#region ValidateBillingDate

		public void ValidateBillingDate()
		{
			ValidateCalculatedProperty(Parent.BillingDateInfo);
		}

		#region CheckBillingDate

		protected void CheckBillingDate()
		{
			MandatoryValidation.CheckEntered(Parent.BillingDateInfo, (IMultilingualString)ResString.GetMultilingualString("d9e87013-d390-408e-9106-3afe86feb0d8", "Billing Date"));
			TypeValidation.CheckValidSmallDateTime(Parent.BillingDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.BillingDateInfo);
		}

		#endregion

		#endregion

		#region CheckWSJ_CustomerReference

		protected override void CheckWSJ_CustomerReference()
		{
			if (Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.WSJ_CustomerReferenceInfo, (IMultilingualString)ResString.GetMultilingualString("5b99eeae-2a84-4eea-8f46-066b47a9116d", "Customer Reference Number"));
			}

			CheckWSJ_CustomerReferenceIsUniquePerClient();
		}

		void CheckWSJ_CustomerReferenceIsUniquePerClient()
		{
			if (!Parent.WSJ_CustomerReference.IsEmpty
				&& Parent.WSJ_OH_Client.IsValid
				&& !Parent.WSJ_OH_ClientInfo.HasErrors()
				&& !Parent.WSJ_CustomerReferenceInfo.HasErrors()
				&& (!Parent.IsInDatabase || Parent.WSJ_CustomerReferenceInfo.HasChanges || Parent.WSJ_OH_ClientInfo.HasChanges))
			{
				var query = new ZQuery(WhsAdHocServiceJobSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(WhsAdHocServiceJobSchema.WSJ_OH_Client, SQLComparisonOperator.Equal, Parent.WSJ_OH_Client);
				query.AddToFilter(WhsAdHocServiceJobSchema.WSJ_CustomerReference, SQLComparisonOperator.Equal, Parent.WSJ_CustomerReference);

				if (Parent.Factory.LoadTop1<WhsAdHocServiceJob>(query) != null)
				{
					Parent.WSJ_CustomerReferenceInfo.AddError(Res.GetString("e3e9ad5f-efaa-4c6b-b3c8-5888f8555d38", "Customer Reference must be unique per Client."));
				}
			}
		}

		#endregion

		#region CheckWSJ_OH_Client

		protected override void CheckWSJ_OH_Client()
		{
			MandatoryValidation.CheckEntered(Parent.WSJ_OH_ClientInfo);
			ListValidation.ErrorIfInvalidPK(Parent.WSJ_OH_ClientInfo);
		}

		#endregion

		#region CheckWSJ_WW_Whs

		protected override void CheckWSJ_WW_Whs()
		{
			MandatoryValidation.CheckEntered(Parent.WSJ_WW_WhsInfo);
			ListValidation.ErrorIfInvalidPK(Parent.WSJ_WW_WhsInfo);

			CheckWSJ_WW_Whs_IsActive();
		}

		#endregion

		#region CheckWarehousePKIsActive

		protected void CheckWSJ_WW_Whs_IsActive()
		{
			if (!Parent.HasErrors() && ((!Parent.Warehouse?.WW_IsActive) ?? false))
			{
				Parent.WSJ_WW_WhsInfo.AddError(Res.GetString("ae452517-f3df-4a49-a1aa-ac25dd303b2e", "Warehouse must be active."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateWSJ_OH_Client();
			ValidateWSJ_WW_Whs();
			ValidateBillingDate();
			ValidateWSJ_CustomerReference();
		}

		#endregion

		public override Type AutoValidationType
		{
			get { return typeof(WhsAdHocServiceJobValidation); }
		}
	}
}
