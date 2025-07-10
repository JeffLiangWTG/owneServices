using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressManagerTest : TestCaseWithFactory
	{
		public void TestCanOverride()
		{
			var requirement = new JobDocAddressRequirement();
			Assert(requirement.CanOverride);

			requirement.CanOverride = false;
			Assert(!requirement.CanOverride);
		}

		public void TestClone()
		{
			JobDocAddressRequirement originalParent = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, AddressType.PIC, ContactType.Consignor);
			JobDocAddressRequirement originalChild = new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress, AddressType.OFC, ContactType.Consignor);
			originalParent.AddLinkedRequirement(originalChild);

			JobDocAddressRequirement clonedParent = originalParent.Clone();

			AssertEquals("clone should be a different instance", false, object.ReferenceEquals(clonedParent, originalParent));
			AssertEquals("parent DefaultDocAddressType", originalParent.DefaultDocAddressType, clonedParent.DefaultDocAddressType);
			AssertEquals("parent DefaultAddressType", originalParent.DefaultAddressType, clonedParent.DefaultAddressType);
			AssertEquals("parent DefaultContactType", originalParent.DefaultContactType, clonedParent.DefaultContactType);

			AssertEquals("related requirements", 1, clonedParent.AdditionalRequirements.Count);
			JobDocAddressRequirement clonedChild = clonedParent.AdditionalRequirements[originalChild.DefaultDocAddressType];
			AssertEquals("cloned child should be a different instance", false, object.ReferenceEquals(originalChild, clonedChild));
			AssertEquals("child DefaultDocAddressType", originalChild.DefaultDocAddressType, clonedChild.DefaultDocAddressType);
			AssertEquals("child DefaultAddressType", originalChild.DefaultAddressType, clonedChild.DefaultAddressType);
			AssertEquals("child DefaultContactType", originalChild.DefaultContactType, clonedChild.DefaultContactType);
		}

		public void TestJobDocAddressRequirementDefaultAddressType()
		{
			AssertEquals(AddressType.OFC, DocAddressRequirement.DefaultAddressType);
		}

		public void TestJobDocAddressRequirementDefaultDocAddressType()
		{
			AssertEquals(DocAddressType.None, DocAddressRequirement.DefaultDocAddressType);
		}

		public void TestJobDocAddressRequirementDefaultContactType()
		{
			AssertEquals(ContactType.NoContactType, DocAddressRequirement.DefaultContactType);
		}

		public void TestJobDocAddressRequirementDefaultSupportedDocAddressTypes()
		{
			AssertEquals(0, DocAddressRequirement.AdditionalRequirements.Count);
		}

		public void TestJobDocAddressRequirementDefaults()
		{
			AssertEquals(DocAddressType.None, DocAddressRequirement.DefaultDocAddressType);
			AssertEquals(0, DocAddressRequirement.AdditionalRequirements.Count);
			AssertEquals(AddressType.OFC, DocAddressRequirement.DefaultAddressType);
			AssertEquals(ContactType.NoContactType, DocAddressRequirement.DefaultContactType);
			DocAddressRequirement = new JobDocAddressRequirement(DefDocAddressType, AddressType.DLV, ContactType.NotifyParty);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement2);
			AssertEquals(DefDocAddressType, DocAddressRequirement.DefaultDocAddressType);
			AssertEquals(2, DocAddressRequirement.AdditionalRequirements.Count);
			AssertEquals(AddressType.DLV, DocAddressRequirement.DefaultAddressType);
			AssertEquals(ContactType.NotifyParty, DocAddressRequirement.DefaultContactType);
		}

		public void TestInitialCodePairList()
		{
			JobDocAddressRequirement newDocAddressRequirement = new JobDocAddressRequirement(DefDocAddressType);
			newDocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			newDocAddressRequirement.AddLinkedRequirement(LinkedRequirement2);
			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			CodeDescriptionPairList codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain only one Code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should contain only default Code 'LC1'.", true, codeList.ContainsCode(DefDocAddressTypeCode));
		}

		public void TestOnGoingCodePairList()
		{
			JobDocAddressRequirement newDocAddressRequirement = new JobDocAddressRequirement(DefDocAddressType);
			newDocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			newDocAddressRequirement.AddLinkedRequirement(LinkedRequirement2);
			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			CodeDescriptionPairList codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);

			AssertEquals("Initial Pair List should contain only one Code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should contain only default Code 'LC1'.", true, codeList.ContainsCode(DefDocAddressTypeCode));

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = ZGuid.NewZGuid();
			docAddresses.Add(docAddress);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain only one Code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should contain only default Code 'LC1'.", true, codeList.ContainsCode(DefDocAddressTypeCode));

			docAddress.E2_AddressType = "FFF";
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain only one Code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should contain only default Code 'LC1'.", true, codeList.ContainsCode(DefDocAddressTypeCode));

			docAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DefDocAddressType);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain supported codes.", 2, codeList.Count);
			AssertEquals("Initial Pair List should not contain default Code 'LC1'.", true, !codeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			JobDocAddress docAddress2 = Factory.New<JobDocAddress>();
			docAddress2.E2_OA_Address = ZGuid.NewZGuid();
			docAddresses.Add(docAddress2);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain supported codes.", 2, codeList.Count);
			AssertEquals("Initial Pair List should not contain default Code 'LC1'.", true, !codeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = "FFF";
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain supported codes.", 2, codeList.Count);
			AssertEquals("Initial Pair List should not contain default Code 'LC1'.", true, !codeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])) && codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0]);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain remaining supported code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should not contain default Code 'LC1'.", true, !codeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1])));

			docAddress2.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[1]);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain remaining supported code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should not contain default Code 'LC1'.", true, !codeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])));

			JobDocAddress docAddress3 = Factory.New<JobDocAddress>();
			docAddress3.E2_OA_Address = ZGuid.NewZGuid();
			docAddresses.Add(docAddress3);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain remaining supported code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should not contain default Code 'LC1'.", true, !codeList.ContainsCode(DefDocAddressTypeCode));
			AssertEquals("CodePairList should contain supported codes.", true, codeList.ContainsCode(DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0])));

			docAddress3.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressTypesSupported[0]);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain no codes.", 0, codeList.Count);

			docAddresses.Remove(docAddress);
			codeList = newDocAddressRequirement.GetApplicableCodeList(docAddresses);
			AssertEquals("Initial Pair List should contain only one Code.", 1, codeList.Count);
			AssertEquals("Initial Pair List should contain only default Code 'LC1'.", true, codeList.ContainsCode(DefDocAddressTypeCode));
		}

		public void TestNoDuplicatesInSupportedList()
		{
			DocAddressRequirement = new JobDocAddressRequirement();
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement2);
			AssertEquals("List should contain original values, because no duplicates.", 2, DocAddressRequirement.AdditionalRequirements.Count);
			DocAddressRequirement = new JobDocAddressRequirement(DocAddressType.LocalCartageAddress1);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement2);
			AssertEquals("List should contain original values, because no duplicates.", 2, DocAddressRequirement.AdditionalRequirements.Count);
			DocAddressRequirement = new JobDocAddressRequirement(DocAddressType.LocalCartageAddress2);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement2);
			AssertEquals("List should contain original values less LC2, becase of duplicate LC2.", 1, DocAddressRequirement.AdditionalRequirements.Count);
			DocAddressRequirement = new JobDocAddressRequirement(DocAddressType.LocalCartageAddress2);
			DocAddressRequirement.AddLinkedRequirement(LinkedRequirement1);
			AssertEquals("List should contain no values", 0, DocAddressRequirement.AdditionalRequirements.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DocAddressRequirement = new JobDocAddressRequirement();
			OrgHeader hdr = Factory.New<OrgHeader>();
			DefDocAddressType = DocAddressType.LocalCartageAddress1;
			DefDocAddressTypeCode = DocAddressTypes.GetCode(Factory, DefDocAddressType);
			DocAddressTypesSupported = new DocAddressType[] { DocAddressType.LocalCartageAddress2, DocAddressType.LocalCartageAddress3 };
			LinkedRequirement1 = new JobDocAddressRequirement(DocAddressType.LocalCartageAddress2);
			LinkedRequirement2 = new JobDocAddressRequirement(DocAddressType.LocalCartageAddress3);
		}

		JobDocAddressRequirement DocAddressRequirement;
		DocAddressType DefDocAddressType;
		ZString DefDocAddressTypeCode;
		JobDocAddressRequirement LinkedRequirement1;
		JobDocAddressRequirement LinkedRequirement2;
		DocAddressType[] DocAddressTypesSupported;

		#endregion
	}
}
