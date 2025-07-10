using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module.Testing
{
	abstract class OrgCodeMappingForeignBaseFilterValidationTest<T> : BusinessObjectValidationTestCase where T : OrgCodeMappingBaseModuleFilter
	{
		public void TestCheckRelationshipType()
		{
			Filter.RelationshipType = string.Empty;
			AssertNoErrors(Filter.RelationshipTypeInfo);
			Filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			AssertNoErrors(Filter.RelationshipTypeInfo);
			Filter.RelationshipType = "!!!";
			AssertHasError(Filter.RelationshipTypeInfo, "Enter a valid selection.");
			Filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Warehouse;
			AssertNoErrors(Filter.RelationshipTypeInfo);
		}

		public void TestCheckContext()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("PPP");
			validCodes.AddPair("QQQ");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				Filter.Context = string.Empty;
				AssertNoErrors(Filter.ContextInfo);
				Filter.Context = "PPP";
				AssertNoErrors(Filter.ContextInfo);
				Filter.Context = "XYZ";
				AssertHasError(Filter.ContextInfo, "Enter a valid selection.");
				Filter.Context = "QQQ";
				AssertNoErrors(Filter.ContextInfo);
			}
		}

		#region Implementation

		protected T Filter => filter ?? (filter = GetFilterForTest());
		T filter;

		protected abstract T GetFilterForTest();

		#endregion
	}
}
