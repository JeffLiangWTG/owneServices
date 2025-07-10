using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(OrgMatchApprovalType))]
	sealed class OrgMatchApprovalTypeTest : TestCaseWithFactory
	{
		public void TestAll()
		{
			AssertEquals("There should be approval types returned", true, OrgMatchApprovalType.All.Length > 2);
		}

		public void TestAdditionalClientSpecificMatchApprovalTypes()
		{
			TestOrgMatchApprovalType.RegisterAdditionalClientSpecificMatchApprovalTypes();
			try
			{
				bool foundClientSpecificMatchApprovalType = false;
				foreach (OrgMatchApprovalType type in OrgMatchApprovalType.All)
				{
					if (type is TestOrgMatchApprovalType)
					{
						foundClientSpecificMatchApprovalType = true;
					}
				}
				AssertEquals("Client specific match approval type should be located once registered", true, foundClientSpecificMatchApprovalType);
			}
			finally
			{
				TestOrgMatchApprovalType.UnregisterAdditionalClientSpecificMatchApprovalTypes();
			}
		}

		[ExpectNoExceptions]
		public void TestAllCodesUnique()
		{
			Hashtable codesSoFar = new Hashtable();
			foreach (OrgMatchApprovalType type in OrgMatchApprovalType.All)
			{
				// expect no exception
				codesSoFar.Add(type.Code, null);
			}
		}

		public void TestFromCode()
		{
			AssertEquals("FromCode should return the correct approval type object", "DUM", OrgMatchApprovalType.FromCode("DUM").Code);
		}

		public void TestMatchApprovalTypes()
		{
			AssertMatchApprovalTypeCorrect(OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignee, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>(), ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWBConsigneeMatchApproval>());
			AssertMatchApprovalTypeCorrect(OrgPatternMatchAddress.Constants.AddressType.AirCargoConsignor, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWB>(), ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusHAWBConsignorMatchApproval>());
			AssertMatchApprovalTypeCorrect("DUM", typeof(DummyBusinessObject), typeof(DummyOrgMatchApproval));
		}

		void AssertMatchApprovalTypeCorrect(string matchTypeCode, Type expectedParentType, Type expectedApprovalType)
		{
			OrgMatchApprovalType actualType = OrgMatchApprovalType.FromCode(matchTypeCode);
			AssertEquals("ParentType for match type '" + matchTypeCode + "'", expectedParentType, actualType.ParentType);
			AssertEquals("ApprovalType for match type '" + matchTypeCode + "'", expectedApprovalType, actualType.ApprovalType);

			OrgMatchApproval matchApproval = (OrgMatchApproval)Factory.New(actualType.ApprovalType);
			AssertEquals("MatchTypeCode property", matchTypeCode, matchApproval.MatchType.Code);
		}

		class TestOrgMatchApprovalType : OrgMatchApprovalType
		{
			public TestOrgMatchApprovalType(string code, Type parentType, Type approvalType) : base(code, parentType, approvalType)
			{
			}

			public static void RegisterAdditionalClientSpecificMatchApprovalTypes()
			{
				TestOrgMatchApprovalType testType = new TestOrgMatchApprovalType("TST", typeof(GlbStaff), typeof(DummyOrgMatchApproval));
				AdditionalClientSpecificMatchApprovalTypes = new TestOrgMatchApprovalType[] { testType };
			}

			public static void UnregisterAdditionalClientSpecificMatchApprovalTypes()
			{
				AdditionalClientSpecificMatchApprovalTypes = null;
			}
		}
	}
}
