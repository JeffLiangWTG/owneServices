using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentSupportingReason))]
	sealed class ComplianceDocumentSupportingReasonTest : RegistryBusinessObjectTest
	{
		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 80, BizObj.MaxDescriptionLengthInternal);
		}

		public void TestValidateDescription()
		{
			var item = new ComplianceDocumentSupportingReason();
			item.Code = "AAA";
			item.Description = (NoResString)"Test code";

			AssertNoErrors("should not have errors", item.CodeInfo);
			AssertNoErrors("should not have errors", item.DescriptionInfo);

			item.Description = (NoResString)"";
			AssertHasErrors("should have errors", item.DescriptionInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BizObj.Code = "TST";
			BizObj.Description = (NoResString)"Test code";

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new ComplianceDocumentSupportingReason BizObj
		{
			get { return (ComplianceDocumentSupportingReason)base.BizObj; }
		}

		protected override void AssertCloneValues(RegistryBusinessObject close)
		{
			ComplianceDocumentSupportingReason clone = close as ComplianceDocumentSupportingReason;
			AssertEquals("Code", "TST", clone.Code);
			AssertEquals("Description", "Test code", clone.Description);
			AssertEquals("CodeMaxLength", 3, clone.CodeMaxLength);
		}

		#endregion
	}
}
