using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeaderSubAccount))]
	sealed class AccGLHeaderSubAccountTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 8, 8, 8, 8, 8)]
		public void TestCanDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "newOrg";
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";
			Factory.Save();

			var accGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var header1 = CreateAccTransactionHeader(company1.PK, accGLHeader.PK);
			var line1 = CreateAccTransactionLines(header1.PK, accGLHeader.PK);
			var header2 = CreateAccTransactionHeader(company2.PK, accGLHeader.PK);
			var line2 = CreateAccTransactionLines(header2.PK, accGLHeader.PK);
			Factory.Save();

			var subAccountType = accGLHeader.SubAccountTypes.AddNew();
			subAccountType.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			Assert("Not In Database", !subAccountType.IsInDatabase);
			Assert("Can Delete", subAccountType.CanDelete);
			Factory.Save();
			AssertEquals(OrgHeaderSchema.Constants.Prefix, subAccountType.ASA_SubClass);
			Assert("In Database", subAccountType.IsInDatabase);
			Assert("Has no Changes", !subAccountType.ASA_SubClassInfo.HasChanges);
			AssertNullOrEmpty(subAccountType.ReasonForNotAbleToDelete);
			Assert("Can Delete", subAccountType.CanDelete);

			CreateAccTransactionLineSubAccount(line1.PK, org.PK, OrgHeaderSchema.Constants.Prefix);
			CreateAccTransactionLineSubAccount(line2.PK, org.PK, OrgHeaderSchema.Constants.Prefix);
			Factory.Save();
			Assert("In Database", subAccountType.IsInDatabase);
			Assert("Has no Changes", !subAccountType.ASA_SubClassInfo.HasChanges);
			Assert("Used", !subAccountType.Validation.IsUsedByTransactionSubAccountType(subAccountType.ASA_SubClass).IsNullOrEmpty());
			Assert("Can not Delete", !subAccountType.CanDelete);
			AssertEquals("Sub account type can not be deleted because this account is currently in use. It is used by transaction lines in company AAA posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00, transaction lines in company BBB posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00.", subAccountType.ReasonForNotAbleToDelete.ToString());

			subAccountType.ASA_SubClassDisplayName = Core.Constants.SubAccountType.SalesGroup;
			Assert("In Database", subAccountType.IsInDatabase);
			Assert("Has Changes", subAccountType.ASA_SubClassInfo.HasChanges);
			Assert("Can not Delete", !subAccountType.CanDelete);
			AssertEquals("Sub account type can not be deleted because this account is currently in use. It is used by transaction lines in company AAA posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00, transaction lines in company BBB posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00.", subAccountType.ReasonForNotAbleToDelete.ToString());
			AssertHasError(subAccountType.ASA_SubClassDisplayNameInfo, "Sub account type can not be changed from 'ORG' to 'SEG' because this account is currently in use. It is used by transaction lines in company AAA posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00, transaction lines in company BBB posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00.");

			subAccountType.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			CreateAccTransactionHeaderSubAccount(header1.PK, org.PK, OrgHeaderSchema.Constants.Prefix);
			CreateAccTransactionHeaderSubAccount(header2.PK, org.PK, OrgHeaderSchema.Constants.Prefix);
			Factory.Save();
			Assert("In Database", subAccountType.IsInDatabase);
			Assert("Has no Changes", !subAccountType.ASA_SubClassInfo.HasChanges);
			Assert("Used", !subAccountType.Validation.IsUsedByTransactionSubAccountType(subAccountType.ASA_SubClass).IsNullOrEmpty());
			Assert("Can not Delete", !subAccountType.CanDelete);
			AssertEquals("Sub account type can not be deleted because this account is currently in use. It is used by transaction lines in company AAA posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00, transaction lines in company BBB posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00, transaction headers in company AAA posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00, transaction headers in company BBB posted between 08-Aug-20 08:08:00 and 08-Aug-20 08:08:00.", subAccountType.ReasonForNotAbleToDelete.ToString());
		}

		AccTransactionHeader CreateAccTransactionHeader(ZGuid companyPK, ZGuid accGLHeaderPK)
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_GC = companyPK;
			header.AH_PostDate = ZDateTime.Now;
			header.AH_AG = accGLHeaderPK;
			return header;
		}

		AccTransactionLines CreateAccTransactionLines(ZGuid headerPK, ZGuid accGLHeaderPK)
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = headerPK;
			line.AL_AG = accGLHeaderPK;
			line.AL_PostDate = ZDateTime.Now;
			return line;
		}

		void CreateAccTransactionLineSubAccount(ZGuid headerPK, ZGuid parentId, String parentTableCode)
		{
			var accTransactionHeaderSubAccount = Factory.New<AccTransactionLineSubAccount>();
			accTransactionHeaderSubAccount.AL1_AL = headerPK;
			accTransactionHeaderSubAccount.AL1_SubClassParentId = parentId;
			accTransactionHeaderSubAccount.AL1_SubClassParentTableCode = parentTableCode;
		}

		void CreateAccTransactionHeaderSubAccount(ZGuid headerPK, ZGuid parentId, String parentTableCode)
		{
			var accTransactionHeaderSubAccount = Factory.New<AccTransactionHeaderSubAccount>();
			accTransactionHeaderSubAccount.AHS_AH = headerPK;
			accTransactionHeaderSubAccount.AHS_SubClassParentId = parentId;
			accTransactionHeaderSubAccount.AHS_SubClassParentTableCode = parentTableCode;
		}

		public void TestASA_Sequence()
		{
			var gsHeaderSubAccount = (AccGLHeaderSubAccount)GetNewBusinessObject();

			gsHeaderSubAccount.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffGroup;
			AssertEquals("The sequence should be 4 when type is 'SGP'.", 4, gsHeaderSubAccount.ASA_Sequence);

			gsHeaderSubAccount.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffAndResources;
			AssertEquals("The sequence should be 3 when type is 'STR'.", 3, gsHeaderSubAccount.ASA_Sequence);

			gsHeaderSubAccount.ASA_SubClassDisplayName = Core.Constants.SubAccountType.SalesGroup;
			AssertEquals("The sequence should be 2 when type is 'SEG'.", 2, gsHeaderSubAccount.ASA_Sequence);

			gsHeaderSubAccount.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;
			AssertEquals("The sequence should be 1 when type is 'ORG'.", 1, gsHeaderSubAccount.ASA_Sequence);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccGLHeaderSubAccount>();
		}
	}
}
