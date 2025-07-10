using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummaryQueryBizObj))]
	sealed class EntrySummaryQueryBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateEntryNumber()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
			entrySummaryQueryObject.EntryNumber = "1";
			AssertHasMessageError(entrySummaryQueryObject.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

			entrySummaryQueryObject.CriteriaCode = "";
			entrySummaryQueryObject.EntryNumber = "1";
			AssertNoMessageError(entrySummaryQueryObject.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

			entrySummaryQueryObject.EntryNumber = "";
			AssertHasMessageError(entrySummaryQueryObject.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);

			entrySummaryQueryObject.EntryNumber = "894573212";
			AssertNoMessageError(entrySummaryQueryObject.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);
			AssertHasWarning(entrySummaryQueryObject.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.EntryNumberLength);

			entrySummaryQueryObject.EntryNumber = "89457321";
			AssertNoWarning(entrySummaryQueryObject.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.EntryNumberLength);
		}

		public void TestValidateEntryFilerCode()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.EntryFilerCode = "";
			AssertHasMessageErrorContaining(entrySummaryQueryObject.EntryFilerCodeInfo, ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);

			entrySummaryQueryObject.EntryFilerCode = "SV9";
			AssertNoMessageErrorContaining(entrySummaryQueryObject.EntryFilerCodeInfo, ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);

			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "XJ5";
			GlbDepartment.CurrentDepartment.GE_Import = true;

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			AssertEquals("XJ5", entrySummaryQueryObject.EntryFilerCode);
			AssertNoMessageErrorContaining(entrySummaryQueryObject.EntryFilerCodeInfo, ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);
		}

		public void TestEntryFilerCode()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);

			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			AssertEquals("SV9", entrySummaryQueryObject.EntryFilerCode);
		}

		public void TestValidateCriteriaCode()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = "";
			AssertNoErrors(entrySummaryQueryObject.CriteriaCodeInfo);

			entrySummaryQueryObject.CriteriaCode = "ABC";
			AssertHasMessageErrorContaining(entrySummaryQueryObject.CriteriaCodeInfo, ListValidation.InvalidCodeMessageError);

			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.PSC;
			AssertNoMessageErrorContaining(entrySummaryQueryObject.CriteriaCodeInfo, ListValidation.InvalidCodeMessageError);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, false))
			{
				entrySummaryQueryObject.EntryNumber = "123456789";
				entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
				AssertHasMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

				entrySummaryQueryObject.EntryNumber = "";
				AssertNoMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

				entrySummaryQueryObject.CriteriaCode = "";
				AssertHasMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);

				entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.RCN;
				AssertNoMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				var entryNumber = entrySummaryQueryObject.EntryNumbers.AddNew();
				entryNumber.EntryNumber = "123456789";
				entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
				AssertHasMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

				entryNumber.EntryNumber = "";
				AssertNoMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

				entrySummaryQueryObject.CriteriaCode = "";
				AssertHasMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);

				entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.RCN;
				AssertNoMessageError(entrySummaryQueryObject.CriteriaCodeInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);
			}
		}

		public void TestDateAcceptedFrom()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = "";
			AssertNoErrors(entrySummaryQueryObject.DateFromInfo);

			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
			entrySummaryQueryObject.DateFrom = ZDateTime.Empty;
			AssertHasMessageError(entrySummaryQueryObject.DateFromInfo, EntrySummaryQueryBizObj.DateRangeRequired);

			entrySummaryQueryObject.DateFrom = ZDateTime.Today;
			AssertNoMessageError(entrySummaryQueryObject.DateFromInfo, EntrySummaryQueryBizObj.DateRangeRequired);

			entrySummaryQueryObject.CriteriaCode = "";
			entrySummaryQueryObject.DateFrom = ZDateTime.Today;
			AssertHasMessageError(entrySummaryQueryObject.DateFromInfo, EntrySummaryQueryBizObj.DateRangeNotRequired);

			entrySummaryQueryObject.DateFrom = ZDateTime.Empty;
			AssertNoMessageError(entrySummaryQueryObject.DateFromInfo, EntrySummaryQueryBizObj.DateRangeNotRequired);
		}

		public void TestDateAcceptedTo()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = "";
			AssertNoErrors(entrySummaryQueryObject.DateToInfo);

			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
			entrySummaryQueryObject.DateTo = ZDateTime.Empty;
			AssertHasMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.DateRangeRequired);

			entrySummaryQueryObject.DateTo = ZDateTime.Today;
			AssertNoMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.DateRangeRequired);

			entrySummaryQueryObject.CriteriaCode = "";
			entrySummaryQueryObject.DateTo = ZDateTime.Today;
			AssertHasMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.DateRangeNotRequired);

			entrySummaryQueryObject.DateTo = ZDateTime.Empty;
			AssertNoMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.DateRangeNotRequired);

			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
			entrySummaryQueryObject.DateFrom = ZDateTime.Today.AddDays(-3);
			entrySummaryQueryObject.DateTo = ZDateTime.Today.AddDays(-4);
			AssertHasMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.ToDateInvalid);

			entrySummaryQueryObject.DateFrom = ZDateTime.Today.AddDays(-35);
			entrySummaryQueryObject.DateTo = ZDateTime.Today.AddDays(-2);
			AssertNoMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.ToDateInvalid);
			AssertHasMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.ToDateOutOfRange);

			entrySummaryQueryObject.DateTo = ZDateTime.Today.AddDays(-5);
			AssertNoMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.ToDateOutOfRange);
		}

		public void TestValidateTimeRange()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
			entrySummaryQueryObject.DateTo = ZDateTime.Today;
			entrySummaryQueryObject.DateFrom = ZDateTime.Today;

			entrySummaryQueryObject.TimeFrom = new ZDateTime(2011, 1, 1, 14, 0, 0);
			entrySummaryQueryObject.TimeTo = new ZDateTime(2011, 1, 1, 13, 0, 0);
			AssertHasMessageError(entrySummaryQueryObject.TimeFromInfo, EntrySummaryQueryBizObj.TimeFromIsInvalid);

			entrySummaryQueryObject.TimeTo = new ZDateTime(2011, 1, 1, 14, 2, 0);
			AssertNoMessageError(entrySummaryQueryObject.TimeFromInfo, EntrySummaryQueryBizObj.TimeFromIsInvalid);
		}

		public void TestValidateFutureDates()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;

			entrySummaryQueryObject.DateFrom = ZDateTime.Now.AddDays(1);
			entrySummaryQueryObject.TimeFrom = new ZDateTime(entrySummaryQueryObject.DateFrom.Year, 1, 1, 03, 56, 0);
			AssertHasMessageError(entrySummaryQueryObject.DateFromInfo, EntrySummaryQueryBizObj.CannotBeFutureDate);

			entrySummaryQueryObject.DateTo = ZDateTime.Now.AddDays(1);
			entrySummaryQueryObject.TimeTo = new ZDateTime(entrySummaryQueryObject.DateFrom.Year, 1, 1, 03, 56, 0);
			AssertHasMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.CannotBeFutureDate);

			entrySummaryQueryObject.DateFrom = ZDateTime.Now.AddDays(-1);
			entrySummaryQueryObject.TimeFrom = new ZDateTime(entrySummaryQueryObject.DateFrom.Year, 1, 1, 10, 20, 0);
			AssertNoMessageError(entrySummaryQueryObject.DateFromInfo, EntrySummaryQueryBizObj.CannotBeFutureDate);

			entrySummaryQueryObject.DateTo = ZDateTime.Now.AddDays(-1);
			entrySummaryQueryObject.TimeTo = new ZDateTime(entrySummaryQueryObject.DateFrom.Year, 1, 1, 10, 22, 0);
			AssertNoMessageError(entrySummaryQueryObject.DateToInfo, EntrySummaryQueryBizObj.CannotBeFutureDate);
		}

		public void TestSettingTodayToDateTo()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;

			var current = ZDateTime.Now;
			entrySummaryQueryObject.DateTo = ZDateTime.Today;
			var past = new ZDateTime(entrySummaryQueryObject.DateTo.Year, 1, 1, current.Hour, current.Minute, current.Second);
			current = ZDateTime.Now;
			var future = new ZDateTime(entrySummaryQueryObject.DateTo.Year, 1, 1, current.Hour, current.Minute, current.Second);
			AssertEquals(true, past <= entrySummaryQueryObject.TimeTo);
			AssertEquals(true, entrySummaryQueryObject.TimeTo <= future);
		}

		public void TestValidateCollectionBillInformationCode()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);

			entrySummaryQueryObject.CollectionBillInformationCode = "ABC";
			AssertHasMessageErrorContaining(entrySummaryQueryObject.CollectionBillInformationCodeInfo, ListValidation.InvalidCodeMessageError);

			entrySummaryQueryObject.CollectionBillInformationCode = CollectionBillInformationCodeList.Codes._01;
			AssertNoMessageErrorContaining(entrySummaryQueryObject.CollectionBillInformationCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestFieldsReadonly()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			Assert("Readonly", !entrySummaryQueryObject.ConsumptionEntrySummariesInfo.ReadOnly);
			Assert("Readonly", !entrySummaryQueryObject.FTAReconSummariesInfo.ReadOnly);
			Assert("Readonly", !entrySummaryQueryObject.OtherReconSummariesInfo.ReadOnly);
			Assert("Readonly", !entrySummaryQueryObject.DrawbackSummariesInfo.ReadOnly);
			Assert("Readonly", !entrySummaryQueryObject.NAFTADutyDeferralSummariesInfo.ReadOnly);
		}
	}
}
