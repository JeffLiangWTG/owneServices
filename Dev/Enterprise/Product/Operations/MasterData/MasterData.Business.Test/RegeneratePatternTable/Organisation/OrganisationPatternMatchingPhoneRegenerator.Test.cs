using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrganisationPatternMatchingPhoneRegeneratorForTest : OrganisationPatternMatchingPhoneRegenerator
	{
		public OrganisationPatternMatchingPhoneRegeneratorForTest(PatternMatchingRecalculator<OrgHeader> recalculator) : base(recalculator)
		{
			patternMatchingRecalculator = recalculator;
		}

		public List<string> TablesPrefixListForTest => TablesPrefixList;
	}

	public class OrganisationPatternMatchingPhoneRegeneratorTest : PatternGeneratorForOrgHeaderTest<OrganisationPatternMatchingPhoneRegeneratorForTest, PatternMatchingPhone>
	{
		readonly List<string> propAddressList = new List<string> { OrgAddressSchema.Constants.OA_Phone, OrgAddressSchema.Constants.OA_Fax, OrgAddressSchema.Constants.OA_Mobile };
		readonly List<string> propContactList = new List<string> { OrgContactSchema.Constants.OC_Phone, OrgContactSchema.Constants.OC_Fax, OrgContactSchema.Constants.OC_Mobile, OrgContactSchema.Constants.OC_OtherPhone, OrgContactSchema.Constants.OC_HomePhone };

		protected override SchemaGuidColumn PatternMatchingOrgHeaderColumn { get { return PatternMatchingPhoneSchema.PMP_OH; } }

		protected override SchemaStringColumn PatternMatchingParentTableCodeColumn { get { return PatternMatchingPhoneSchema.PMP_ParentTableCode; } }

		public override void TestGenerate()
		{
			#region Set up data & Test Add New Data
			var testAddressDataList = new List<OrgAddress>();
			var testAddressCount = 4;
			OrgHeader.MainAddress.Address1 = "72 ORiordan Street";
			OrgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgHeader.MainAddress.OA_Phone = "+61449743938";
			OrgHeader.MainAddress.OA_Code = "DFS2";
			testAddressDataList.Add(OrgHeader.MainAddress);
			for (var i = 0; i < testAddressCount; i++)
			{
				var address = OrgHeader.Addresses.AddNew();
				address.OA_RN_NKCountryCode = "AU";
				address.OA_Code = "ACD" + (i + 1).ToString();
				address.OA_Address1 = "Bourke Street" + (i + 1).ToString();
				address.OA_Phone = "1455412" + (i + 1).ToString();
				address.OA_Fax = "9876544" + (i + 1).ToString();
				testAddressDataList.Add(address);
			}
			testAddressCount = testAddressDataList.Count;

			var testOrgContactCount = 3;
			var testOrgContactDataList = new List<OrgContact>();
			for (var i = 0; i < testOrgContactCount; i++)
			{
				var contact = OrgHeader.Contacts.AddNew();
				contact.OC_Email = string.Format(CultureInfo.InvariantCulture, "tom{0}@gmail.com", i);
				contact.OC_ContactName = "Tom" + i;
				contact.OC_Fax = "52984" + (i + 1);
				contact.OC_Mobile = "+6144974393" + (i + 1);
				contact.OC_Phone = "+6128432332" + (i + 1);
				contact.OC_OtherPhone = "+6130438982" + (i + 1);
				testOrgContactDataList.Add(contact);
			}

			testOrgContactDataList[0].OC_OtherPhone = "";

			Factory.Save();

			var patternMatchingRecalculator = new PatternMatchingRecalculator<OrgHeader>(OrgHeader);
			var patternMatchingPhoneGenerator = new OrganisationPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);

			patternMatchingPhoneGenerator.InitializeDataCount(OrgHeader, Factory);
			var numRegenerated = patternMatchingPhoneGenerator.Regenerate(OrgHeader, Factory);
			var query = new ZQuery(PatternMatchingPhoneSchema.PMP_OH, OrgHeader.PK);
			query.AddToFilter(PatternMatchingPhoneSchema.PMP_ParentTableCode, patternMatchingPhoneGenerator.TablesPrefixListForTest);
			var result = Factory.Load<PatternMatchingPhone>(query);
			AssertEquals("Expected: correct amount of phone patterns were created.", result.Length, (testOrgContactCount * 4 - 1) + (testAddressCount * 2 - 1));
			AssertEquals("Expected: correct amount of phone patterns were changed.", result.Length, numRegenerated);
			#endregion

			#region Change Data

			//delete data, total -= 4
			var delContact = Factory.Load<OrgContact>(testOrgContactDataList[testOrgContactCount - 1].PK);
			ZGuid delContactPK = delContact.PK;
			delContact.Delete();
			testOrgContactCount--;

			//update data, total -= 1
			var updateAddress = Factory.Load<OrgAddress>(testAddressDataList[testAddressCount - 1].PK);
			updateAddress.Address1 = "Bell Street CC";
			updateAddress.OA_Phone = "";
			updateAddress.OA_Fax = "";
			updateAddress.OA_Mobile = "5645245";
			updateAddress.OA_RN_NKCountryCode = "CN";

			//update data, total += 0
			var updateContact = Factory.Load<OrgContact>(testOrgContactDataList[testOrgContactCount - 1].PK);
			updateContact.OC_Phone = "";
			updateContact.OC_OtherPhone = "1515245";
			updateContact.OC_HomePhone = "568995";

			//update data, total += 1
			testOrgContactDataList[0].OC_OtherPhone = "+61304389821";

			//new data, total += 6
			for (var i = testAddressCount; i < testAddressCount + 2; i++)
			{
				var addAddress = Factory.NewWithValidTestData<OrgAddress>();
				addAddress.OA_OH = OrgHeader.PK;
				addAddress.OA_Code = "ACD" + (i + 1);
				addAddress.OA_RN_NKCountryCode = "AU";
				addAddress.OA_Phone = "1455412" + (i + 1);
				addAddress.OA_Fax = "9876544" + (i + 1);
				addAddress.OA_Mobile = "789545" + (i + 1);
				testAddressDataList.Add(addAddress);
			}

			Factory.Save();

			#endregion

			#region ReGenerate
			patternMatchingPhoneGenerator = new OrganisationPatternMatchingPhoneRegeneratorForTest(patternMatchingRecalculator);
			var totalCount = patternMatchingPhoneGenerator.InitializeDataCount(OrgHeader, Factory);
			var effectCount = patternMatchingPhoneGenerator.Regenerate(OrgHeader, Factory);
			#endregion

			#region Assert
			var result_recalculated = Factory.Load<PatternMatchingPhone>(query);

			AssertEquals("Expected: should regenerate correct amount of patterns ", result_recalculated.Length, result.Length + 2);
			AssertEquals("Expected: plan numbers must equal actual numbers ", totalCount, effectCount);

			foreach (var addr in testAddressDataList)
			{
				var matArray = result_recalculated.Where(a => a.PMP_ParentId.Equals(addr.PK)).ToArray();
				var notNullValuePropList = new List<string>();
				foreach (var prop in propAddressList)
				{
					var propValue = patternMatchingPhoneGenerator.GetObjectPropertyValue(addr, prop);
					if (!string.IsNullOrEmpty(propValue))
					{
						notNullValuePropList.Add(prop);
					}
				}
				AssertEquals("Expected: have equals matching pattern numbers ", notNullValuePropList.Count, matArray.Length);
				foreach (var prop in notNullValuePropList)
				{
					var propValue = TextStandardizerHelper.StandardizePhone(patternMatchingPhoneGenerator.GetObjectPropertyValue(addr, prop));
					var correctHashValue = TextStandardizerHelper.ComputeStringHashFast(propValue).ToString();
					var mat = result_recalculated.Where(a => a.PMP_ParentId.Equals(addr.PK) && a.HashedValue.ToString().Equals(correctHashValue) && a.PMP_RN_NKCountryCode.Equals(addr.OA_RN_NKCountryCode)).FirstOrDefault();
					AssertNotNull("Expected:should find matching patterns ", mat);
				}
			}

			foreach (var contact in testOrgContactDataList)
			{
				var matArray = result_recalculated.Where(a => a.PMP_ParentId.Equals(contact.PK)).ToArray();
				if (delContact.PK.Equals(contact.PK))
				{
					Assert("Expected: it has been deleted ", matArray == null || matArray.Length == 0);
					continue;
				}
				var notNullValuePropList = new List<string>();
				foreach (var prop in propContactList)
				{
					var propValue = patternMatchingPhoneGenerator.GetObjectPropertyValue(contact, prop);
					if (!string.IsNullOrEmpty(propValue))
					{
						notNullValuePropList.Add(prop);
					}
				}
				AssertEquals("Expected: have equals matching pattern numbers ", notNullValuePropList.Count, matArray.Length);
				foreach (var prop in notNullValuePropList)
				{
					var propValue = TextStandardizerHelper.StandardizePhone(patternMatchingPhoneGenerator.GetObjectPropertyValue(contact, prop));
					var correctCountryCode = OrgHeader.CountryCode;
					var correctHashValue = TextStandardizerHelper.ComputeStringHashFast(propValue).ToString();
					var mat = result_recalculated.Where(a => a.PMP_ParentId.Equals(contact.PK) && a.HashedValue.ToString().Equals(correctHashValue) && a.PMP_RN_NKCountryCode.Equals(correctCountryCode)).FirstOrDefault();
					AssertNotNull("Expected:should find matching patterns ", mat);
				}
			}
			#endregion
		}

		protected override void AssertResult(PatternMatchingPhone[] result)
		{
			AssertEquals("Expected: Placeholder phone is not added", 0, result.Length);
		}

		OrgContact orgContact;
		OrgAddress orgAddress;

		protected override void CreateExtraBusinessObjects()
		{
			orgContact = OrgHeader.Contacts.AddNew();
			orgAddress = OrgHeader.Addresses.AddNew();

			orgContact.OC_ContactName = "Edward He";
			orgContact.OC_Phone = "+1 (111) 1111-1111";

			orgAddress.OA_Address1 = "72 Bourke Street";
			orgAddress.OA_Phone = "+1 (111) 1111-1111";
		}

		protected override void CreateExtraPatternMatchingRecords()
		{
			var patternRecord = Factory.NewWithValidTestData<PatternMatchingPhone>();

			patternRecord.PMP_HashedValue = 2238232;
			patternRecord.PMP_IsActive = true;
			patternRecord.PMP_OH = OrgHeader.PK;
			patternRecord.PMP_ParentId = orgContact.PK;
			patternRecord.PMP_ParentTableCode = "OC";
			patternRecord.PMP_RN_NKCountryCode = OrgHeader.CountryCode;
		}
	}
}
