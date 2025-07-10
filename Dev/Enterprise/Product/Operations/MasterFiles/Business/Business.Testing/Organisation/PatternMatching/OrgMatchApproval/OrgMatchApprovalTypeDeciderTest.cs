using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgMatchApprovalTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetInstance()
		{
			OrgMatchApprovalTypeDecider typeDecider = OrgMatchApprovalTypeDecider.GetInstance();
			AssertEquals("Should return the base type decider by default", typeof(OrgMatchApprovalTypeDecider), typeDecider.GetType());

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			{
				OrgMatchApprovalTypeDecider overriddenTypeDecider = OrgMatchApprovalTypeDecider.GetInstance();
				AssertEquals("TypeDecider should be able to be overridden on the client hook", typeof(TestOverriddenOrgMatchApprovalTypeDecider), overriddenTypeDecider.GetType());
			}
		}

		public void TestGetTypeForNew()
		{
			Type typeUsedForAddNewCancelBinding = new OrgMatchApprovalTypeDecider().GetTypeForNew();
			AssertEquals(
				"This type cannot be abstract but is required when the CurrencyManager does an AddNew/Cancel to get the PropertyDescriptors for binding",
				false, typeUsedForAddNewCancelBinding.IsAbstract);
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(OrgMatchApproval), TypeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);

			OrgMatchApprovalTypeDecider typeDecider = new OrgMatchApprovalTypeDecider();
			DataRow row = ((INeedRow)matchApproval).Row;
			Type matchApprovalType = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Should return the correct type of match approval", typeof(DummyOrgMatchApproval), matchApprovalType);
		}

		public void TestGetTypeForLoad_ReturnsEmptyMatchApprovalTypeWhenCodeIsUnknown()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			matchApproval.P2_MatchType = "XXX";

			OrgMatchApproval emptyMatchApproval = Factory.Load<OrgMatchApproval>(matchApproval.PK);
			AssertEquals("A Factory.Load should give us an empty match approval", true, emptyMatchApproval.GetType() == typeof(EmptyOrgMatchApproval));
		}

		class TestClientOverride : ClientHook
		{
			public override ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(OrgMatchApproval), new TestOverriddenOrgMatchApprovalTypeDecider());
					return new TypeDeciderDictionary(result);
				}
			}

			public override Clients Client
			{
				get { return Clients.EDI; }
			}

			public override string ClientDisplayName
			{
				get { return "For Test"; }
			}
		}

		class TestOverriddenOrgMatchApprovalTypeDecider : OrgMatchApprovalTypeDecider
		{
		}

		readonly OrgMatchApprovalTypeDecider TypeDecider = new OrgMatchApprovalTypeDecider();
	}
}
