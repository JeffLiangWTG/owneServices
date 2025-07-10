using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryNumberBizObj))]
	sealed class EntryNumberBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateEntryNumber()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			entrySummaryQueryObject.CriteriaCode = CriteriaCodeList.Codes.AII;
			var entryNumber = entrySummaryQueryObject.EntryNumbers.AddNew();
			entryNumber.EntryNumber = "1";

			AssertHasMessageError(entryNumber.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

			entrySummaryQueryObject.CriteriaCode = "";
			entryNumber.EntryNumber = "1";
			AssertNoMessageError(entryNumber.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.NotRequired);

			entryNumber.EntryNumber = "";
			AssertHasMessageError(entryNumber.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);

			entryNumber.EntryNumber = "894573212";
			AssertNoMessageError(entryNumber.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.ValueRequired);
			AssertHasWarning(entryNumber.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.EntryNumberLength);

			entryNumber.EntryNumber = "89457321";
			AssertNoWarning(entryNumber.EntryNumberInfo, ValidationConstants.EntrySummaryQuery.EntryNumberLength);
		}

		public void TestValidateEntryFilerCode()
		{
			var entrySummaryQueryObject = new EntrySummaryQueryBizObj(Factory);
			var entryNumber = entrySummaryQueryObject.EntryNumbers.AddNew();
			entryNumber.EntryFilerCode = "";
			AssertHasMessageErrorContaining(entryNumber.EntryFilerCodeInfo, ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);

			entryNumber.EntryFilerCode = "SV9";
			AssertNoMessageErrorContaining(entryNumber.EntryFilerCodeInfo, ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);

			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "XJ5";
			GlbDepartment.CurrentDepartment.GE_Import = true;

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			var entryNumber1 = entrySummaryQueryObject.EntryNumbers.AddNew();
			AssertEquals("XJ5", entryNumber1.EntryFilerCode);
			AssertNoMessageErrorContaining(entryNumber1.EntryFilerCodeInfo, ValidationConstants.EntrySummaryQuery.FilerCodeIsEmpty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntrySummaryQueryBizObj(Factory).EntryNumbers.AddNew();
		}
	}
}
