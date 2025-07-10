using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class SupplyTypeConfigurationValidationTest : JobConfigurationSelectorValidationTest
	{
		public override void TestValidateJobType()
		{
			base.TestValidateJobType();

			BizObj.JobType = "";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "ABC";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "CLL";
			AssertNoErrors(BizObj.JobTypeInfo);

			var newBizObjCollection = GetNewBizObjCollection;

			var setting1 = (ISupplyTypeSelector)newBizObjCollection.AddNew();
			var setting2 = (ISupplyTypeSelector)newBizObjCollection.AddNew();

			AssertNoErrors("Precondition: setting1.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.JobTypeInfo);

			var supplyTypeConfigurationLookups = new SupplyTypeConfigurationLookups(BizObj);
			var incoterm1 = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			setting1.JobType = SupplyTypeConfigurationLookups.JobTypeAdditionalCodes.All;
			setting1.Incoterm = incoterm1;
			setting2.JobType = SupplyTypeConfigurationLookups.JobTypeAdditionalCodes.All;
			setting2.Incoterm = incoterm1;

			newBizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);

			setting1.Incoterm = INCOTermCodes.All;
			newBizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);

			setting1.Incoterm = incoterm1;
			setting2.Incoterm = INCOTermCodes.All;
			newBizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);

			setting1.Incoterm = incoterm1;
			setting2.Incoterm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;

			newBizObjCollection.RunPreSaveValidation();
			AssertNoErrors("Precondition: setting1.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.JobTypeInfo);

			setting1.Incoterm = incoterm1;
			setting2.Incoterm = incoterm1;
			setting1.LineDepartmentPK = ZGuid.Empty;
			setting2.LineDepartmentPK = ZGuid.Empty;
			newBizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);

			var nonCurrentDepartmentPK = Factory.NewWithValidTestData<GlbDepartment>().PK;
			setting1.LineDepartmentPK = nonCurrentDepartmentPK;

			newBizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);

			setting1.LineDepartmentPK = nonCurrentDepartmentPK;
			setting2.LineDepartmentPK = nonCurrentDepartmentPK;

			newBizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);

			setting1.LineDepartmentPK = nonCurrentDepartmentPK;
			setting2.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK;

			newBizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
		}

		public void TestValidateIncoterm()
		{
			BizObj.Incoterm = ZString.Empty;
			BizObj.ValidateIncoterm();
			AssertHasErrors("Will have error when Incoterm is empty", BizObj.IncotermInfo);

			BizObj.Incoterm = "!@#";
			BizObj.ValidateIncoterm();
			AssertHasErrors("Will have error when Incoterm an invalid code", BizObj.IncotermInfo);

			BizObj.Incoterm = "ALL";
			BizObj.ValidateIncoterm();
			AssertNoErrors(BizObj.IncotermInfo);
		}

		public void TestValidateLineDepartmentPK()
		{
			var nonCurrentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			BizObj.LineDepartmentPK = Guid.NewGuid();
			BizObj.ValidateLineDepartmentPK();
			AssertHasErrors("Will have error when LineDepartmentPK is not a PK of Department.", BizObj.LineDepartmentPKInfo);

			BizObj.LineDepartmentPK = nonCurrentDepartment.PK;
			BizObj.ValidateLineDepartmentPK();
			AssertNoErrors(BizObj.LineDepartmentPKInfo);

			BizObj.LineDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			nonCurrentDepartment.GE_IsActive = false;
			Factory.Save();

			BizObj.LineDepartmentPK = nonCurrentDepartment.PK;

			BizObj.ValidateLineDepartmentPK();
			AssertHasErrors("Will have error when LineDepartmentPK is a PK of inactive Department.", BizObj.LineDepartmentPKInfo);

			nonCurrentDepartment.GE_IsActive = true;
			Factory.Save();

			BizObj.ValidateLineDepartmentPK();
			AssertNoErrors(BizObj.LineDepartmentPKInfo);
		}

		public void TestValidateSupplyType()
		{
			BizObj.SupplyType = ZString.Empty;
			BizObj.ValidateSupplyType();
			AssertHasErrors("Will have error when SupplyType is empty", BizObj.SupplyTypeInfo);

			BizObj.SupplyType = "!@#";
			BizObj.ValidateSupplyType();
			AssertHasErrors("Will have error when SupplyType an invalid code", BizObj.SupplyTypeInfo);

			BizObj.SupplyType = "LOC";
			BizObj.ValidateSupplyType();
			AssertNoErrors(BizObj.SupplyTypeInfo);
		}

		#region Implementation

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.Incoterm = "!@#";
			BizObj.LineDepartmentPK = Guid.NewGuid();
			BizObj.SupplyType = "!@#";

			((BusinessObject)BizObj).RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.IncotermInfo);
			AssertHasErrors(BizObj.LineDepartmentPKInfo);
			AssertHasErrors(BizObj.SupplyTypeInfo);
		}

		protected override string ExpectedDuplicateJobParametersError
		{
			get { return "At least one more record already sets supply type behavior for the same Job parameters"; }
		}

		new ISupplyTypeSelector BizObj
		{
			get { return (ISupplyTypeSelector)base.BizObj; }
			set { base.BizObj = value; }
		}

		protected override IJobConfigurationSelector GetNewBizObj => new SupplyTypeConfiguration();

		protected override IRegistrySettingCollection GetNewBizObjCollection => new SupplyTypeConfigurationCollection();

		#endregion

	}
}
