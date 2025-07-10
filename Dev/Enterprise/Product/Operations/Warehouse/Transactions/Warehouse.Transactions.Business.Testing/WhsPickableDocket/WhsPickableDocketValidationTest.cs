using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketValidationTest<TPickableDocket> : WhsDocketValidationTestCase<TPickableDocket>
		where TPickableDocket : WhsPickableDocket
	{
		#region TestCheckWD_WP

		public void TestCheckWD_WP()
		{
			var errorMsg1 = "Un-finalized Docket without status of ATTACHED TO PICK or PICKING must not be attached to any pick.";
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", "NEW", docket.WD_DocketStatus);
			docket.WD_WP = ZGuid.Empty;
			AssertNoErrors("Docket should be able to have no pick when status is not ATTACHED TO PICK, PICKING or FINALISED", docket.WD_WPInfo);

			docket.WD_WP = ZGuid.NewZGuid();
			AssertHasError(docket.WD_WPInfo, errorMsg1);

			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertNoErrors(docket.WD_WPInfo);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertHasError(docket.WD_WPInfo, errorMsg1);

			docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertNoErrors(docket.WD_WPInfo);

			var errorMsg2 = "Finalized Docket or Docket with status set to ATTACHED TO PICK or PICKING must be attached to any pick.";
			docket.WD_WP = ZGuid.Empty;
			AssertHasError("Docket is in picking status, therefore detaching from pick must have errors.", docket.WD_WPInfo, errorMsg2);

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket.WD_WP = ZGuid.NewZGuid();
			AssertNoErrors("Docket is finalized and there is a pick. Therefore must not have any errors.", docket.WD_WPInfo);
			docket.WD_WP = ZGuid.Empty;
			AssertHasError("Docket is finalized, therefore when pick is removed there must be errors.", docket.WD_WPInfo, errorMsg2);
		}

		#endregion

		#region TestCheckWD_DocketStatus

		public virtual void TestCheckWD_DocketStatus()
		{
			var docket = GetNewBusinessObject();
			docket.WD_WP = ZGuid.Empty;

			var errorMsg1 = "Un-finalized Docket not attached to any pick so the docket status cannot be set to ATTACHED TO PICK or PICKING.";
			var errorMsg2 = "Un-finalized Docket has a pick so docket status must set to either ATTACHED TO PICK or PICKING.";

			var isWorkOrder = docket.WD_DocketType == DocketType.Codes.WorkOrder;

			docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertHasError(docket.WD_DocketStatusInfo, errorMsg1);

			if (isWorkOrder)
			{
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertHasError(docket.WD_DocketStatusInfo, errorMsg1);
			}
			else
			{
				docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
				AssertHasError(docket.WD_DocketStatusInfo, errorMsg1);
			}

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertNoErrors(docket.WD_DocketStatusInfo);

			var codeNotPickFinPutaway = docket.Statuses.GetAllCodes().Where(
				code =>
				code != DocketStatus.Codes.AttachedToPick &&
				code != DocketStatus.Codes.Picking &&
				code != DocketStatus.Codes.Finalised &&
				code != DocketStatus.Codes.Putaway);
			foreach (var code in codeNotPickFinPutaway)
			{
				docket.WD_DocketStatus = code;
				AssertNoErrors($"Docket status:{code} should have no error.", docket.WD_DocketStatusInfo);
			}

			docket.WD_WP = ZGuid.NewZGuid();
			AssertHasError(docket.WD_DocketStatusInfo, errorMsg2);

			docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertNoErrors(docket.WD_DocketStatusInfo);

			if (isWorkOrder)
			{
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertNoErrors(docket.WD_DocketStatusInfo);
			}
			else
			{
				docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
				AssertNoErrors(docket.WD_DocketStatusInfo);
			}
		}

		#endregion

		#region TestConsigneeNameOrPKValidation

		public virtual void TestConsigneeNameOrPKValidation()
		{
			TestConsigneeNameOrPKValidationCore();
		}

		protected virtual void TestConsigneeNameOrPKValidationCore()
		{
			var docket = GetNewBusinessObject();
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;

			docket.ConsigneeDocAddress.E2_AddressOverride = false;
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_CompanyName = "Blah";
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.E2_CompanyName = "";
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertHasErrors(docket.ConsigneeNameOrPKInfo);
		}

		#endregion

		#region TestTransportCoNameOrPKValidation

		protected override void ValidateTransportCoNameOrPK(WhsDocket docket)
		{
			((TPickableDocket)docket).Validation.ValidateTransportCoNameOrPK();
		}

		#endregion

		#region TestValidateWD_PickOption

		public void TestValidateWD_PickOption()
		{
			var docket = GetNewBusinessObject();
			docket.WD_PickOption = "";
			docket.Validation.ValidateWD_PickOption();
			AssertHasErrors(docket.WD_PickOptionInfo);

			docket.WD_PickOption = WhsPickOption.Codes.Auto;
			AssertNoErrors(docket.WD_PickOptionInfo);

			docket.WD_PickOption = "ABC";
			AssertHasErrors(docket.WD_PickOptionInfo);
		}

		#endregion

		#region TestCheckWD_IsInwardsProcessingJob

		public void TestCheckWD_IsInwardsProcessingJob()
		{
			TestCheckWD_IsInwardsProcessingJobCore();
		}

		protected virtual void TestCheckWD_IsInwardsProcessingJobCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_IsVirtualWarehouse = false;
			Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);

			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = warehouse.PK;

			docket.WD_IsInwardsProcessingJob = true;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Inward Processing Jobs can only be created in Virtual Warehouses.");

			docket.WD_IsInwardsProcessingJob = false;
			AssertNoErrors(docket.WD_IsInwardsProcessingJobInfo);

			warehouse.WW_IsVirtualWarehouse = true;
			docket.WD_IsInwardsProcessingJob = true;
			AssertNoErrors(docket.WD_IsInwardsProcessingJobInfo);

			docket.WD_WW_Whs = ZGuid.Empty;
			docket.WD_IsInwardsProcessingJob = true;
			AssertHasError(docket.WD_IsInwardsProcessingJobInfo, "Inward Processing Jobs can only be created in Virtual Warehouses.");
		}

		#endregion
	}
}
