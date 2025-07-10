using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class CRMSecurityProviderTestStandAlone : TestCaseWithFactory
	{
		public void TestOSMGFilterWithInvalidStaffCode()
		{
			var provider = new BaseDummyCRMSecurityProvider();
			provider.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			var filters = new ModuleFilterCollection();
			provider.AddCRMSecurityFilterStrips(Factory, filters);
			var filter = filters["Org. Security Group Security"] as ModuleNkFilter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "XXX";
			Assert("Invalid staff code so the query should be empty", filter.Query.IsEmpty);
		}

		public void TestHasRestrictions()
		{
			var provider = GetSecurityProvider(false, false, false);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(false, false, true);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(false, true, false);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(false, true, true);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(true, false, false);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(true, false, true);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(true, true, false);
			Assert(provider.HasRestrictions);

			provider = GetSecurityProvider(true, true, true);
			Assert(!provider.HasRestrictions);
		}

		public void TestDeniedSecurityCheckpointsPaths()
		{
			var provider = GetSecurityProvider(false, false, false);
			AssertEquals(@"viewParent -> Search and View Records Assigned to Other Login Staff
taskASNParent -> Permit Unconditional access regardless of Task assignment
osmgParent -> Permit Unconditional access regardless of Org. Security Groups", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(false, false, true);
			AssertEquals(@"viewParent -> Search and View Records Assigned to Other Login Staff
taskASNParent -> Permit Unconditional access regardless of Task assignment", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(false, true, false);
			AssertEquals(@"viewParent -> Search and View Records Assigned to Other Login Staff
osmgParent -> Permit Unconditional access regardless of Org. Security Groups", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(false, true, true);
			AssertEquals(@"viewParent -> Search and View Records Assigned to Other Login Staff", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(true, false, false);
			AssertEquals(@"taskASNParent -> Permit Unconditional access regardless of Task assignment
osmgParent -> Permit Unconditional access regardless of Org. Security Groups", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(true, false, true);
			AssertEquals(@"taskASNParent -> Permit Unconditional access regardless of Task assignment", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(true, true, false);
			AssertEquals(@"osmgParent -> Permit Unconditional access regardless of Org. Security Groups", string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));

			provider = GetSecurityProvider(true, true, true);
			AssertEquals(string.Empty, string.Join(System.Environment.NewLine, provider.DeniedSecurityCheckpointsPaths));
		}

		BaseDummyCRMSecurityProvider GetSecurityProvider(bool viewByStaffNotAssignedAllowed, bool ignoreTaskAssignmentAllowed, bool ignoreOSMGAllowed)
		{
			var newSecurityProvider = new BaseDummyCRMSecurityProvider();
			newSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = viewByStaffNotAssignedAllowed;
			newSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = ignoreTaskAssignmentAllowed;
			newSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = ignoreOSMGAllowed;
			return newSecurityProvider;
		}

		public void TestRelatedOrgHeaderColumnsNotSupportedWithEnhancedSecurityLevel()
		{
			AssertNotSupportedWithEnhancedSecurityLevel(new DummyCRMSecurityProviderWithRelatedOrgHeaderColumns());
		}

		public void TestRelatedOrgAddressColumnsNotSupportedWithEnhancedSecurityLevel()
		{
			AssertNotSupportedWithEnhancedSecurityLevel(new DummyCRMSecurityProviderWithRelatedOrgAddressColumns());
		}

		public void TestModuleNotSupportedWithEnhancedSecurityLevel()
		{
			AssertNotSupportedWithEnhancedSecurityLevel(new BaseDummyCRMSecurityProvider());
		}

		void AssertNotSupportedWithEnhancedSecurityLevel(BaseDummyCRMSecurityProvider provider)
		{
			provider.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			provider.OSMGSecurityLevelRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.OSMGSecurityLevels.Enhanced);
			var filters = new ModuleFilterCollection();
			provider.AddCRMSecurityFilterStrips(Factory, filters);
			AssertInnermostException("Should throw a NotSupportedException when OSMGSecurityLevelRegistryItem is set to Enhanced", typeof(NotSupportedException), "OSMG with Enhanced Security Level has not been implemented for this module yet.", () => Factory.Load<DummyBusinessObject>(filters.GetFilterQuery(filters)));
		}

		#region Implementation

		class DummyCRMSecurityProviderWithRelatedOrgHeaderColumns : BaseDummyCRMSecurityProvider
		{
			protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => new SchemaGuidColumn[] { DummyBizoSchema.PK };
		}

		class DummyCRMSecurityProviderWithRelatedOrgAddressColumns : BaseDummyCRMSecurityProvider
		{
			protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => new SchemaGuidColumn[] { DummyBizoSchema.PK };
		}

		class BaseDummyCRMSecurityProvider : CRMSecurityProvider<DummyBusinessObject>
		{
			public override CRMSecurity CRMSecurity
			{
				get
				{
					if (crmSecurity == null)
					{
						var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
						var viewParent = new SecurityCheckpoint("viewParent", (NoResString)"viewParent", null, security);
						var editParent = new SecurityCheckpoint("editParent", (NoResString)"editParent", null, security);
						var osmgParent = new SecurityCheckpoint("osmgParent", (NoResString)"osmgParent", null, security);
						var taskASNParent = new SecurityCheckpoint("taskASNParent", (NoResString)"taskASNParent", null, security);

						crmSecurity = new CRMSecurityForTest(security);
						crmSecurity.CreateViewCheckpointForTest(viewParent);
						crmSecurity.CreateEditCheckpointsForTest(editParent);
						crmSecurity.CreateOSMGCheckpointForTest(osmgParent);
						crmSecurity.CreateTaskAssignmentCheckpointForTest(taskASNParent);
					}
					return crmSecurity;
				}
			}
			CRMSecurityForTest crmSecurity;

			protected override IEnumerable<SchemaGuidColumn> RelatedOrgHeaderColumns => Enumerable.Empty<SchemaGuidColumn>();

			protected internal override IEnumerable<SchemaGuidColumn> RelatedOrgAddressColumns => Enumerable.Empty<SchemaGuidColumn>();

			protected override IEnumerable<SchemaColumn> RelatedGlbStaffColumns => Enumerable.Empty<SchemaGuidColumn>();

			protected override bool ShouldCheckJobHeader => false;

			public override CodePairRegistryItem OSMGSecurityLevelRegistryItem => osmgSecurityLevelRegistryItem ?? (osmgSecurityLevelRegistryItem = new CodePairRegistryItem("DummyOSMGSecurityLevel",
						(NoResString)"Dummy Category",
						(NoResString)"Dummy Caption",
						(NoResString)"Dummy description",
						OLookUpEditType.OSMGSecurityLevel,
						allowBlank: false,
						validate: true,
						new ComboBoxRegistryEditorInfo(OLookUpEditType.OSMGSecurityLevel, true),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Core.Constants.OSMGSecurityLevels.Standard,
						useDefaultDefaultValue: false));

			CodePairRegistryItem osmgSecurityLevelRegistryItem;
		}

		#endregion
	}
}
