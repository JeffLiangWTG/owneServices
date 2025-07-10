using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsolidatedAccountingCategoryItem))]
	class ConsolidatedAccountingCategoryItemTest : CodeDescriptionWithGroupTest
	{
		public void TestIsBeingFilledByParentCollection()
		{
			var item = new ConsolidatedAccountingCategoryItem();
			AssertEquals(string.Empty, item.OriginalCode);

			var itemRef = new ConsolidatedAccountingCategoryItem();
			itemRef.Code = "AAA";

			item.SetOriginalValue(itemRef);
			AssertEquals("AAA", item.OriginalCode);
		}

		public new void TestCanDelete()
		{
			base.TestCanDelete();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertCode(
				code: Constants.AccountsCategory.Unrelated,
				codeForChanging: "AAA",
				expectedOriginalCode: Constants.AccountsCategory.Unrelated,
				expectedCanDeleteWhenCodeInUsed: false
			);
			AssertCode(
				code: Constants.AccountsCategory.MinorityWithReporting,
				codeForChanging: "AAA",
				expectedOriginalCode: Constants.AccountsCategory.MinorityWithReporting,
				expectedCanDeleteWhenCodeInUsed: false
			);
			AssertCode(
				code: string.Empty,
				codeForChanging: "AAA",
				expectedOriginalCode: string.Empty,
				expectedCanDeleteWhenCodeInUsed: true
			);
			AssertCode(
				code: null,
				codeForChanging: "AAA",
				expectedOriginalCode: string.Empty,
				expectedCanDeleteWhenCodeInUsed: true
			);
			AssertCode(
				code: "   ",
				codeForChanging: "AAA",
				expectedOriginalCode: string.Empty,
				expectedCanDeleteWhenCodeInUsed: true
			);

			void AssertCode(ZString code, ZString codeForChanging, ZString expectedOriginalCode, bool expectedCanDeleteWhenCodeInUsed)
			{
				var itemRef = new ConsolidatedAccountingCategoryItem();
				itemRef.Code = code;
				AssertEquals("PreCondition", string.Empty, itemRef.OriginalCode);

				var item = new ConsolidatedAccountingCategoryItem();
				item.SetOriginalValue(itemRef);
				item.SystemDefined = false;
				AssertEquals("PreCondition", expectedOriginalCode, item.OriginalCode);

				org.CompanyData.OB_ARConsolidatedAccountingCategory = code;
				Factory.Save();

				item.Code = codeForChanging;
				AssertEquals("PreCondition, OriginalCode will not be changed when Code is changed.", expectedOriginalCode, item.OriginalCode);
				AssertItemCanDeleteWhenCodeInUsed(item, expectedCanDeleteWhenCodeInUsed);
				AssertItemCanDeleteWhenCodeInUsed(itemRef, expectedCanDelete: true);

				item.Code = code;
				AssertItemCanDeleteWhenCodeInUsed(item, expectedCanDeleteWhenCodeInUsed);
				AssertItemCanDeleteWhenCodeInUsed(itemRef, expectedCanDelete: true);

				org.CompanyData.OB_ARConsolidatedAccountingCategory = string.Empty;
				Factory.Save();
				Assert(item.CanDelete);
				AssertEquals("This is a system defined value and cannot be deleted.", item.ReasonForNotAbleToDelete);
				Assert(itemRef.CanDelete);
				AssertEquals("This is a system defined value and cannot be deleted.", itemRef.ReasonForNotAbleToDelete);

				item.SystemDefined = true;
				Assert(!item.CanDelete);
				AssertEquals("This is a system defined value and cannot be deleted.", item.ReasonForNotAbleToDelete);
				itemRef.SystemDefined = true;
				Assert(!itemRef.CanDelete);
				AssertEquals("This is a system defined value and cannot be deleted.", itemRef.ReasonForNotAbleToDelete);
			}

			void AssertItemCanDeleteWhenCodeInUsed(ICanDelete itemCanDelete, bool expectedCanDelete)
			{
				var expectedReason = expectedCanDelete
					? "This is a system defined value and cannot be deleted."
					: "This code cannot be deleted as it is in use by at least one organization record in the system.";

				AssertEquals(expectedCanDelete, itemCanDelete.CanDelete);
				AssertEquals(expectedReason, itemCanDelete.ReasonForNotAbleToDelete);
			}
		}

		public void TestValidateCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertCode(
				code: Constants.AccountsCategory.Unrelated,
				codeForChanging: "AAA",
				expectedOriginalCode: Constants.AccountsCategory.Unrelated,
				expectedValidationErrorWhenCodeInUsed: true
			);
			AssertCode(
				code: Constants.AccountsCategory.MinorityWithReporting,
				codeForChanging: "AAA",
				expectedOriginalCode: Constants.AccountsCategory.MinorityWithReporting,
				expectedValidationErrorWhenCodeInUsed: true
			);
			AssertCode(
				code: string.Empty,
				codeForChanging: "AAA",
				expectedOriginalCode: string.Empty,
				expectedValidationErrorWhenCodeInUsed: false
			);
			AssertCode(
				code: null,
				codeForChanging: "AAA",
				expectedOriginalCode: string.Empty,
				expectedValidationErrorWhenCodeInUsed: false
			);
			AssertCode(
				code: "   ",
				codeForChanging: "AAA",
				expectedOriginalCode: string.Empty,
				expectedValidationErrorWhenCodeInUsed: false
			);

			void AssertCode(ZString code, ZString codeForChanging, ZString expectedOriginalCode, bool expectedValidationErrorWhenCodeInUsed)
			{
				var itemRef = new ConsolidatedAccountingCategoryItem();
				itemRef.Code = code;
				AssertEquals("PreCondition", string.Empty, itemRef.OriginalCode);

				var item = new ConsolidatedAccountingCategoryItem();
				item.SetOriginalValue(itemRef);
				item.Code = itemRef.Code;
				itemRef.SystemDefined = false;
				AssertEquals("PreCondition", expectedOriginalCode, item.OriginalCode);

				org.CompanyData.OB_ARConsolidatedAccountingCategory = code;
				Factory.Save();
				AssertItemCodeValidation(item, codeForChanging, expectedOriginalCode, expectedValidationErrorWhenCodeInUsed);
				AssertItemCodeValidation(itemRef, codeForChanging, expectedOriginalCode: string.Empty, hasValidationError: false);

				org.CompanyData.OB_ARConsolidatedAccountingCategory = string.Empty;
				Factory.Save();
				AssertItemCodeValidation(item, codeForChanging, expectedOriginalCode, hasValidationError: false);
				AssertItemCodeValidation(itemRef, codeForChanging, expectedOriginalCode: string.Empty, hasValidationError: false);

				item.SystemDefined = true;
				AssertItemCodeValidation(item, codeForChanging, expectedOriginalCode, hasValidationError: false);
				AssertItemCodeValidation(itemRef, codeForChanging, expectedOriginalCode: string.Empty, hasValidationError: false);
			}

			void AssertItemCodeValidation(ConsolidatedAccountingCategoryItem item, ZString codeForChanging, ZString expectedOriginalCode, bool hasValidationError)
			{
				var currentCode = item.Code;
				AssertEquals(false, item.CodeInfo.HasError("This code cannot be changed as it is in use by at least one organization record in the system."));

				item.Code = codeForChanging;
				AssertEquals("PreCondition, OriginalCode will not be changed when Code is changed.", expectedOriginalCode, item.OriginalCode);
				AssertEquals(hasValidationError, item.CodeInfo.HasError("This code cannot be changed as it is in use by at least one organization record in the system."));

				item.Code = currentCode;
				AssertEquals(false, item.CodeInfo.HasError("This code cannot be changed as it is in use by at least one organization record in the system."));
			}
		}

		protected new ConsolidatedAccountingCategoryItem BizObj
		{
			get { return (ConsolidatedAccountingCategoryItem)base.BizObj; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var resultRef = (CodeDescriptionWithGroup)base.GetBusinessObjectToClone();
			AssertNotNullOrEmpty("PreCondition", resultRef.Code);

			var result = (ConsolidatedAccountingCategoryItem)GetNewBusinessObject();
			result.CodeMaxLength = 5;
			result.Code = resultRef.Code;
			result.Description = resultRef.Description;
			result.Group = resultRef.Group;
			result.SystemDefined = resultRef.SystemDefined;
			result.CodeList = resultRef.CodeList;
			result.SetOriginalValue(resultRef);

			CombineAssertions("PreCondition", () =>
			{
				AssertEquals(5, result.CodeMaxLength);
				AssertEquals(resultRef.Code, result.Code);
				AssertEquals(resultRef.Code, result.OriginalCode);
				AssertEquals(resultRef.Group, result.Group);
				AssertEquals(resultRef.SystemDefined, result.SystemDefined);
				AssertContainsExactElementsInAnyOrder(resultRef.CodeList, result.CodeList);
				AssertNotNull(result.GroupLookup);
			});

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			base.AssertCloneValues(clone1);

			var item = clone1 as ConsolidatedAccountingCategoryItem;
			AssertEquals(item.Code, item.OriginalCode);
		}
	}
}
