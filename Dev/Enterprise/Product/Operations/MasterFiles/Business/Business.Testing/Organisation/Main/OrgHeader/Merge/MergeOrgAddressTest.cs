using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MergeOrgAddress))]
	sealed class MergeOrgAddressTest : MergeOrgElementTest
	{
		public override void TestValidateFuzzyMatching()
		{
			MergeOrgAddress mergeOrgAddress = NewTestMergeElement("1 2 3 Some Street") as MergeOrgAddress;
			AssertEquals("Default action should be ADD", "ADD", mergeOrgAddress.Action);

			AddNewOrg(mergeOrgAddress, "1   2 3 some st");
			AssertEquals("Action should be Merge", "MRG", mergeOrgAddress.Action);
			OrgAddress matchingAddress = Factory.Load<OrgAddress>(mergeOrgAddress.NewObjectPK);
			AssertEquals("New Address1 should be '1   2 3 some st'", "1   2 3 some st", matchingAddress.OA_Address1);

			AddNewOrg(mergeOrgAddress, "1 4 3 some st");
			AssertEquals("Action should be Add", "ADD", mergeOrgAddress.Action);

			mergeOrgAddress = NewTestMergeElement("1/2 3, Some Street") as MergeOrgAddress;
			AssertEquals("Default action should be ADD", "ADD", mergeOrgAddress.Action);

			AddNewOrg(mergeOrgAddress, "1  / 2 3 , some st");
			AssertEquals("Action should be Merge", "MRG", mergeOrgAddress.Action);
			matchingAddress = Factory.Load<OrgAddress>(mergeOrgAddress.NewObjectPK);
			AssertEquals("New Address1 should be '1  / 2 3 , some st'", "1  / 2 3 , some st", matchingAddress.OA_Address1);

			mergeOrgAddress = NewTestMergeElement("1-2 3, Some Street") as MergeOrgAddress;
			AssertEquals("Default action should be ADD", "ADD", mergeOrgAddress.Action);
			AssertEquals("Default old code should be Code", "Code", mergeOrgAddress.OldAddressCode);

			AddNewOrg(mergeOrgAddress, " 1-  2 3 , some st");
			AssertEquals("Action should be Merge", "MRG", mergeOrgAddress.Action);
			matchingAddress = Factory.Load<OrgAddress>(mergeOrgAddress.NewObjectPK);
			AssertEquals("New Address1 should be ' 1-  2 3 , some st'", " 1-  2 3 , some st", matchingAddress.OA_Address1);
			AssertEquals("New Code should be 'Code'", "Code", matchingAddress.OA_Code);

			AddNewOrg(mergeOrgAddress, " 1-  2 3 ", " some st", "Code1");
			AssertEquals("Action should be ADD because code is different", "ADD", mergeOrgAddress.Action);

			AddNewOrg(mergeOrgAddress, " 1-  2 3 ", " some st", "Code");
			AssertEquals("Action should be Merge", "MRG", mergeOrgAddress.Action);
			matchingAddress = Factory.Load<OrgAddress>(mergeOrgAddress.NewObjectPK);
			AssertEquals("New Address1 should be ' 1-  2 3'", " 1-  2 3", matchingAddress.OA_Address1);
			AssertEquals("New Address2 should be ' some st'", " some st", matchingAddress.OA_Address2);
			AssertEquals("New Code should be 'Code", "Code", matchingAddress.OA_Code);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewTestMergeElement() as BusinessObject;
		}

		protected override IMergeOrgElement NewTestMergeElement(BusinessObjectCollection newOrgElementCollection)
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress newAddress = Factory.NewWithValidTestData<OrgAddress>();
			newAddress.OA_Address1 = "1 address of the new organization";
			newAddress.OA_OH = newOrg.PK;

			OrgAddress oldAddress = Factory.NewWithValidTestData<OrgAddress>();
			oldAddress.OA_Address1 = "1 address of the old organization";

			Factory.Save();

			return new MergeOrgAddress(Factory, oldAddress, newOrg, newOrgElementCollection);
		}

		protected override IMergeOrgElement NewTestMergeElement(ZString oldAddress1)
		{
			OrgAddress oldAddress = Factory.NewWithValidTestData<OrgAddress>();
			oldAddress.OA_Address1 = oldAddress1;
			oldAddress.OA_Code = "Code";

			Factory.Save();

			return new MergeOrgAddress(Factory, oldAddress, null);
		}

		void AddNewOrg(MergeOrgAddress testMergeAddress, ZString newAddress1)
		{
			AddNewOrg(testMergeAddress, newAddress1, "", "Code");
		}

		void AddNewOrg(MergeOrgAddress testMergeAddress, ZString newAddress1, ZString newAddress2, ZString code)
		{
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress newAddress = Factory.NewWithValidTestData<OrgAddress>();
			newAddress.OA_Address1 = newAddress1;
			newAddress.OA_Address2 = newAddress2;
			newAddress.OA_Code = code;
			newAddress.OA_OH = newOrg.PK;

			newOrg.Addresses.Add(newAddress);

			Factory.Save();

			testMergeAddress.NewOrganization = newOrg;
		}

		protected override BusinessObjectCollection GetMergeOrgElementCollection()
		{
			return new MergeOrgAddressCollection(Factory);
		}

		protected override SchemaColumn[] ColumnsFotMatching
		{
			get { return new SchemaColumn[] { OrgAddressSchema.OA_Address1 }; }
		}

		protected override Type MasterBusinessObjectType { get { return typeof(OrgAddress); } }
	}
}
