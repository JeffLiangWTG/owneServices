using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressRequirementTest : TestCaseWithFactory
	{
		public void TestRequirementsHandling()
		{
			AssertEquals("Manager should have no Requirements.", 0, EmptyManager.Requirements.Length);
			AssertEquals("Manager should have 1 Requirements.", 1, Manager.Requirements.Length);
			EmptyManager.AddRequirement(DocAddressRequirement);
			AssertEquals("Manager should have 1 Requirements.", 1, EmptyManager.Requirements.Length);
			Manager.AddRequirement(DocAddressRequirement2);
			AssertEquals("Manager should have 2 Requirements.", 2, Manager.Requirements.Length);
		}

		public void TestCodePairLists()
		{
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain no codes.", 0, CodeList.Count);

			DocAddressRequirement = new JobDocAddressRequirement(DefDocAddressType);
			DocAddressRequirement.AddLinkedRequirement(DocAddressTypesSupported[0]);
			DocAddressRequirement.AddLinkedRequirement(DocAddressTypesSupported[1]);
			EmptyManager.AddRequirement(DocAddressRequirement);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));

			CodeList = Manager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));

			EmptyManager.AddRequirement(DocAddressRequirement2);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain 2 Codes.", 2, CodeList.Count);
			AssertEquals("CodePairList should contain default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain default Code 'LC2'", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.DefaultDocAddressType)));

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = ZGuid.NewZGuid();
			DocAddresses.Add(docAddress);

			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain 2 Codes.", 2, CodeList.Count);
			AssertEquals("CodePairList should contain default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain default Code 'LC2'", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.DefaultDocAddressType)));
			CodeList = Manager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));

			docAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DefDocAddressType);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain supported codes.", 3, CodeList.Count);
			AssertEquals("CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));
			AssertEquals("List should contain default Code 'LC2'", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.DefaultDocAddressType)));
			CodeList = Manager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain supported codes.", 2, CodeList.Count);
			AssertEquals("CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			JobDocAddress docAddress2 = Factory.New<JobDocAddress>();
			docAddress2.E2_OA_Address = ZGuid.NewZGuid();
			DocAddresses.Add(docAddress2);

			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain supported codes.", 3, CodeList.Count);
			AssertEquals("CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));
			AssertEquals("CodePairList should contain default Code 'LC2'", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.DefaultDocAddressType)));
			CodeList = Manager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain supported codes.", 2, CodeList.Count);
			AssertEquals("CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressRequirement2.DefaultDocAddressType);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain supported codes.", 4, CodeList.Count);
			AssertEquals("CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should not contain default Code 'LC2'", true, !CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.DefaultDocAddressType)));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.SupportedDocAddressTypes[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressRequirement2.SupportedDocAddressTypes[1])));
			CodeList = Manager.GetApplicableCodeList();
			AssertEquals("CodePairList should contain supported codes.", 2, CodeList.Count);
			AssertEquals("CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));
		}

		public void TestOnGoingCodePairListThroughManager()
		{
			JobDocAddressRequirement newDocAddressRequirement = new JobDocAddressRequirement(DefDocAddressType);
			newDocAddressRequirement.AddLinkedRequirement(DocAddressTypesSupported[0]);
			newDocAddressRequirement.AddLinkedRequirement(DocAddressTypesSupported[1]);
			EmptyManager.AddRequirement(newDocAddressRequirement);
			CodeList = EmptyManager.GetApplicableCodeList();

			AssertEquals("Initial CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = ZGuid.NewZGuid();
			DocAddresses.Add(docAddress);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));

			docAddress.E2_AddressType = "FFF";
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));

			docAddress.E2_AddressType = DefDocAddressTypeCode;
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain supported codes.", 2, CodeList.Count);
			AssertEquals("Initial CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			JobDocAddress docAddress2 = Factory.New<JobDocAddress>();
			docAddress2.E2_OA_Address = ZGuid.NewZGuid();
			DocAddresses.Add(docAddress2);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain supported codes.", 2, CodeList.Count);
			AssertEquals("Initial CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = "FFF";
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain supported codes.", 2, CodeList.Count);
			AssertEquals("Initial CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0]);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain remaining supported codes.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1]);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain remaining supported code.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])));

			JobDocAddress docAddress3 = Factory.New<JobDocAddress>();
			docAddress3.E2_OA_Address = ZGuid.NewZGuid();
			DocAddresses.Add(docAddress3);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain remaining supported code.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should not contain default Code 'LC1'", true, !CodeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, CodeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])));

			docAddress3.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0]);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain no codes.", 0, CodeList.Count);

			DocAddresses.Remove(docAddress);
			CodeList = EmptyManager.GetApplicableCodeList();
			AssertEquals("Initial CodePairList should contain only one Code.", 1, CodeList.Count);
			AssertEquals("Initial CodePairList should contain only default Code 'LC1'", true, CodeList.ContainsCode(DefDocAddressTypeCode));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocAddressRequirement = new JobDocAddressRequirement();
			OrgHeader hdr = Factory.New<OrgHeader>();
			DefDocAddressType = DocAddressType.LocalCartageAddress1;
			DefDocAddressTypeCode = DocAddressTypes.GetCode(Factory, DefDocAddressType);
			DocAddressTypesSupported = new DocAddressType[] { DocAddressType.LocalCartageAddress2, DocAddressType.LocalCartageAddress3 };

			DocAddressRequirement2 = new JobDocAddressRequirement(DocAddressType.LocalCartageCTO);
			DocAddressRequirement2.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.LocalCartageCFS));
			DocAddressRequirement2.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.LocalCartageYard));

			DocAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			EmptyManager = new JobDocAddressManager(DocAddresses);
			Manager = new JobDocAddressManager(DocAddresses);
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.LocalCartageAddress1);
			requirement.AddLinkedRequirement(DocAddressType.LocalCartageAddress2);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.LocalCartageAddress3));
			Manager.AddRequirement(requirement);
		}

		JobDocAddressRequirement DocAddressRequirement;
		JobDocAddressRequirement DocAddressRequirement2;
		DocAddressType DefDocAddressType;
		ZString DefDocAddressTypeCode;
		DocAddressType[] DocAddressTypesSupported = new DocAddressType[] { DocAddressType.LocalCartageAddress2, DocAddressType.LocalCartageAddress3 };

		JobDocAddressManager EmptyManager;
		JobDocAddressManager Manager;

		CodeDescriptionPairList CodeList;
		JobDocAddressDependentCollection DocAddresses;
	}
}
