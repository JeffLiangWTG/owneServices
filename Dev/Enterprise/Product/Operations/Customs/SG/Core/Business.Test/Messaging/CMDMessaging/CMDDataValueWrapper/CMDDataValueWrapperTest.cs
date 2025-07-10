using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(CMDDataValueWrapper))]
	class CMDDataValueWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCommitChangesToCusCodeData()
		{
			AssertEquals("Pre-condition", "CMD", Entry.CY_Type);
			AssertEquals("Pre-condition", "", Entry.CY_Code);
			AssertEquals("Pre-condition", "", Entry.CY_Data);
			EntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.SGExemption.Codes.AT;
			EntryWrapper.PermitNumberOrExemptionRemarks = "MEH";
			EntryWrapper.HasChanges = false;
			EntryWrapper.CommitChangesToCMDData();
			AssertEquals("HasChanges is false, should not be updated", "", Entry.CY_Code);
			AssertEquals("HasChanges is false, should not be updated", "", Entry.CY_Data);
			EntryWrapper.HasChanges = true;
			EntryWrapper.CommitChangesToCMDData();
			AssertEquals(CustomsEntryTypeList.Singapore.SGExemption.Codes.AT, Entry.CY_Code);
			AssertEquals("MEH", Entry.CY_Data);
		}

		public void TestPermitOrExemptionType()
		{
			Entry.CY_Code = CustomsEntryTypeList.Singapore.SGExemption.Codes.AT;
			AssertEquals("Default should be from CMDDataValues.CE_EntryType", CustomsEntryTypeList.Singapore.SGExemption.Codes.AT, EntryWrapper.PermitOrExemptionType);
			Assert("Pre-condition", !EntryWrapper.PermitOrExemptionTypeInfo.HasErrors());
			EntryWrapper.PermitOrExemptionType = "";
			AssertEquals("", EntryWrapper.PermitOrExemptionType);
			Assert("Should call validate in the setter", EntryWrapper.PermitOrExemptionTypeInfo.HasErrors());
		}

		public void TestPermitNumberOrExemptionRemarks()
		{
			Entry.CY_Data = "123";
			AssertEquals("Default should be from CMDDataValues.CY_Data", "123", EntryWrapper.PermitNumberOrExemptionRemarks);
			Assert("Pre-condition", !EntryWrapper.PermitNumberOrExemptionRemarksInfo.HasErrors());
			EntryWrapper.PermitNumberOrExemptionRemarks = "";
			AssertEquals("", EntryWrapper.PermitNumberOrExemptionRemarks);
			Assert("Should call validate in the setter", EntryWrapper.PermitNumberOrExemptionRemarksInfo.HasErrors());
		}

		public void TestIsTDBExemption()
		{
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.AT, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.CA, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.CD, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.DP, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.HC, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.HT, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.MD, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.MF, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.PM, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.PP, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.PT, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.SP, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.TS, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.UA, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.SGExemption.Codes.ZZ, true);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.Permit, false);
			AssertIsTDBExemption(CustomsEntryTypeList.Singapore.Certificate, false);
		}

		public void TestValidation()
		{
			CMDDataValueWrapperValidation validation = EntryWrapper.Validation;
			AssertNotNull(validation);
			AssertNotEquals("Should not be cached", validation, EntryWrapper.Validation);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CMDDataValueWrapperLookups), EntryWrapper.Lookups.GetType());
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CMDDataValueWrapper(Entry);
		}

		void AssertIsTDBExemption(ZString permitOrExemptionCode, bool expectedIsTDBExemption)
		{
			EntryWrapper.PermitOrExemptionType = permitOrExemptionCode;
			AssertEquals(expectedIsTDBExemption, EntryWrapper.IsTDBExemption);
		}

		CMDDataValueWrapper EntryWrapper
		{
			get
			{
				if (fEntryWrapper == null)
				{
					fEntryWrapper = new CMDDataValueWrapper(Entry);
				}

				return fEntryWrapper;
			}
		}

		CMDPermitNumber Entry
		{
			get
			{
				if (fEntry == null)
				{
					fEntry = Factory.New<CMDPermitNumber>();
					fEntry.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				}

				return fEntry;
			}
		}

		CMDDataValueWrapper fEntryWrapper;
		CMDPermitNumber fEntry;
		#endregion
	}
}
