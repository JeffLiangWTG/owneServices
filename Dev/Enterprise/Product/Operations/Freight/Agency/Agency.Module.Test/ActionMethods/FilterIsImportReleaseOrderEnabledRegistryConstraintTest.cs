using System;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business.Testing;

namespace Enterprise.Freight.Agency.Module.Testing
{
	public class FilterIsImportReleaseOrderEnabledRegistryConstraintTest : ConstraintTest<FilterIsImportReleaseOrderEnabledRegistryConstraint>
	{
		public override void TestGetValue()
		{
			var portMessagingPortCollection = new PortMessagingPortCollection { new PortMessagingPort { Port = "NZAKL", Enabled = false, SenderID = "1" } };

			var branchNZ = Factory.NewWithValidTestData<GlbBranch>();
			branchNZ.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Core.Constants.CountryCodes.NewZealand).PK;
			branchNZ.Company.GC_RN_NKCountryCode = "NZ";
			Factory.Save();

			using (branchNZ.SetAsTemporaryContext())
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(branchNZ.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				AssertEquals("N", Constraint.GetValue());
			}

			var portMessagingPortCollection2 = new PortMessagingPortCollection { new PortMessagingPort { Port = "NZAKL", Enabled = true, SenderID = "1" } };

			using (branchNZ.SetAsTemporaryContext())
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection2))
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(branchNZ.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				AssertEquals("Y", Constraint.GetValue());
			}

			portMessagingPortCollection.OfType<PortMessagingPort>().FirstOrDefault().Enabled = true;

			using (branchNZ.SetAsTemporaryContext())
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(branchNZ.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				AssertEquals("Y", Constraint.GetValue());
			}

			var branchBE = Factory.NewWithValidTestData<GlbBranch>();
			branchBE.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "BBB", Core.Constants.CountryCodes.Belgium).PK;
			branchBE.Company.GC_RN_NKCountryCode = "BE";
			Factory.Save();

			using (branchBE.SetAsTemporaryContext())
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(branchBE.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				AssertEquals("N", Constraint.GetValue());
			}
		}

		#region Implementation

		protected override string ExpectedName => "IsImportReleaseOrderEnabled";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		#endregion
	}
}
