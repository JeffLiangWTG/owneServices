using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitHeaderValidationTest<TSharedCusPermitHeader> : CusPermitHeaderValidationTest
			where TSharedCusPermitHeader : SharedCusPermitHeader
	{
		public void TestCheckCPH_OA_AppliesTo()
		{
			var permit = GetNewPermitHeader(Factory);
			permit.CPH_Type = "MND";
			permit.Validation.ValidateCPH_OA_AppliesTo();
			AssertHasError(permit.CPH_OA_AppliesToInfo, SharedCusPermitHeaderValidation.AppliesToRequired(permit.CPH_Type));
			permit.CPH_OA_AppliesTo = ZGuid.BrettsGuid;
			AssertNoError(permit.CPH_OA_AppliesToInfo, SharedCusPermitHeaderValidation.AppliesToRequired(permit.CPH_Type));

			permit.CPH_Type = "EPT";
			permit.Validation.ValidateCPH_OA_AppliesTo();
			AssertHasWarning(permit.CPH_OA_AppliesToInfo, SharedCusPermitHeaderValidation.AppliesToNotApplicable(permit.CPH_Type));
			permit.CPH_OA_AppliesTo = ZGuid.Empty;
			AssertNoWarning(permit.CPH_OA_AppliesToInfo, SharedCusPermitHeaderValidation.AppliesToNotApplicable(permit.CPH_Type));
		}

		public void TestCheckCPH_UnitOfMeasure()
		{
			CombineAssertions("Check MustBeEntered", () =>
			{
				PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
				PermitHeader.Validation.ValidateCPH_UnitOfMeasure();
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
				PermitHeader.Validation.ValidateCPH_UnitOfMeasure();
				AssertHasErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_UnitOfMeasure = "KG";
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
				PermitHeader.CPH_UnitOfMeasure = ZString.Empty;
				AssertHasErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_UnitOfMeasure = "KG";
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
			});

			CombineAssertions("Check MinLength", () =>
			{
				PermitHeader.CPH_UnitOfMeasure = ZString.Empty;
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, SharedCusPermitHeaderValidation.UnitOfMeasureMinimumLength);
				PermitHeader.CPH_UnitOfMeasure = "K";
				AssertHasErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, SharedCusPermitHeaderValidation.UnitOfMeasureMinimumLength);
				PermitHeader.CPH_UnitOfMeasure = "KG";
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, SharedCusPermitHeaderValidation.UnitOfMeasureMinimumLength);
			});
		}

		public void TestCheckCPH_Number()
		{
			PermitHeader.CPH_Number = ZString.Empty;
			AssertHasErrorContaining(PermitHeader.CPH_NumberInfo, MandatoryValidation.MustBeEntered);
			PermitHeader.CPH_Number = "ZZZ";
			AssertNoErrorContaining(PermitHeader.CPH_NumberInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCPH_QtyValIndicator()
		{
			PermitHeader.CPH_QtyValIndicator = ZString.Empty;
			AssertHasErrorContaining(PermitHeader.CPH_QtyValIndicatorInfo, MandatoryValidation.MustBeEntered);
			PermitHeader.CPH_QtyValIndicator = "ZZZ";
			AssertHasErrorContaining(PermitHeader.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);
			PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			AssertNoErrorContaining(PermitHeader.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);
		}

		public void TestCPH_EndDate()
		{
			PermitHeader.CPH_StartDate = ZDate.Empty;
			PermitHeader.CPH_EndDate = ZDate.Today;
			AssertNoErrors(PermitHeader.CPH_EndDateInfo);
			PermitHeader.CPH_StartDate = ZDate.Today;
			PermitHeader.CPH_EndDate = PermitHeader.CPH_StartDate.AddDays(-1);
			AssertHasError(PermitHeader.CPH_EndDateInfo, SharedCusPermitHeaderValidation.EndDateBeforeStartDate);
			PermitHeader.CPH_EndDate = PermitHeader.CPH_StartDate;
			AssertNoError(PermitHeader.CPH_EndDateInfo, SharedCusPermitHeaderValidation.EndDateBeforeStartDate);

			var holder = Factory.New<OrgHeader>();
			var permit1 = GetNewPermitHeader(Factory);
			permit1.CPH_Number = "PERMIT";
			permit1.CPH_OH_PermitHolder = holder.PK;
			permit1.CPH_StartDate = ZDate.Today;
			permit1.CPH_EndDate = ZDate.Today.AddDays(1);
			var permit2 = GetNewPermitHeader(Factory);
			permit2.CPH_Number = "PERMIT";
			permit2.CPH_OH_PermitHolder = holder.PK;
			permit2.CPH_StartDate = ZDate.Today.AddDays(2);
			permit2.CPH_EndDate = ZDate.Today.AddDays(3);
			permit2.RunPreSaveValidation();
			AssertNoError(permit2.CPH_EndDateInfo, SharedCusPermitHeaderValidation.EndDateInRangeOfOtherPermit(permit1));
			permit2.CPH_StartDate = ZDate.Today.AddDays(-1);
			permit2.CPH_EndDate = ZDate.Today;
			AssertNoError("Only show on save and validate all", permit2.CPH_EndDateInfo, SharedCusPermitHeaderValidation.EndDateInRangeOfOtherPermit(permit1));
			permit2.RunPreSaveValidation();
			AssertHasError(permit2.CPH_EndDateInfo, SharedCusPermitHeaderValidation.EndDateInRangeOfOtherPermit(permit1));
		}

		public void TestCPH_StartDate()
		{
			var holder = Factory.New<OrgHeader>();
			var permit1 = GetNewPermitHeader(Factory);
			permit1.CPH_Number = "PERMIT";
			permit1.CPH_OH_PermitHolder = holder.PK;
			permit1.CPH_StartDate = ZDate.Today;
			permit1.CPH_EndDate = ZDate.Today.AddDays(1);
			var permit2 = GetNewPermitHeader(Factory);
			permit2.CPH_Number = "PERMIT";
			permit2.CPH_OH_PermitHolder = holder.PK;
			permit2.CPH_StartDate = ZDate.Today.AddDays(2);
			permit2.CPH_EndDate = ZDate.Today.AddDays(3);
			permit2.RunPreSaveValidation();
			AssertNoError(permit2.CPH_StartDateInfo, SharedCusPermitHeaderValidation.StartDateInRangeOfOtherPermit(permit1));
			permit2.CPH_StartDate = ZDate.Today;
			permit2.CPH_EndDate = ZDate.Today.AddDays(3);
			AssertNoError("Only show on save and validate all", permit2.CPH_StartDateInfo, SharedCusPermitHeaderValidation.StartDateInRangeOfOtherPermit(permit1));
			permit2.RunPreSaveValidation();
			AssertHasError(permit2.CPH_StartDateInfo, SharedCusPermitHeaderValidation.StartDateInRangeOfOtherPermit(permit1));

			AssertNoError(permit2.CPH_StartDateInfo, SharedCusPermitHeaderValidation.OtherPermitInDateRange(permit1));
			permit2.CPH_StartDate = ZDate.Today.AddDays(-1);
			permit2.CPH_EndDate = ZDate.Today.AddDays(3);
			AssertNoError("Only show on save and validate all", permit2.CPH_StartDateInfo, SharedCusPermitHeaderValidation.OtherPermitInDateRange(permit1));
			permit2.RunPreSaveValidation();
			AssertHasError(permit2.CPH_StartDateInfo, SharedCusPermitHeaderValidation.OtherPermitInDateRange(permit1));
		}

		public void TestCheckCPH_Type()
		{
			PermitHeader.CPH_Type = ZString.Empty;
			AssertHasErrorContaining(PermitHeader.CPH_TypeInfo, MandatoryValidation.MustBeEntered);
			PermitHeader.CPH_Type = "ZZZ";
			AssertHasErrorContaining(PermitHeader.CPH_TypeInfo, ListValidation.InvalidCodeError);
			PermitHeader.CPH_Type = "EXP";
			AssertNoErrorContaining(PermitHeader.CPH_TypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCPH_SubType()
		{
			PermitHeader.CPH_SubType = ZString.Empty;
			AssertNoErrorContaining(PermitHeader.CPH_SubTypeInfo, ListValidation.InvalidCodeError);
			PermitHeader.CPH_SubType = "ZZZ";
			AssertHasErrorContaining(PermitHeader.CPH_SubTypeInfo, ListValidation.InvalidCodeError);
			PermitHeader.CPH_SubType = "LVE";
			AssertNoErrorContaining(PermitHeader.CPH_SubTypeInfo, ListValidation.InvalidCodeError);
		}

		#region Implementation

		protected abstract TSharedCusPermitHeader GetNewPermitHeader(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			PermitHeader = GetNewPermitHeader(Factory);
		}

		protected TSharedCusPermitHeader PermitHeader { get; set; }

		#endregion
	}
}
