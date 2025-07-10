using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UnmatchOrgRecordsTest : SerializableNoteTextTest<UnmatchOrgRecords>
	{
		public void TestOverlengthFieldsGetTruncatedSoThatExceptionsDontOccurWhenCreatingAnOrganisation()
		{
			var longString = "".PadRight(200, 'X');
			var orgRecord = new UnmatchOrgRecord()
			{
				OrganisationType = longString,
				OrganisationSubType = longString,
				OrganisationName = longString,
				EDICode = longString,
				OwnerCode = longString,
				AddressLine1 = longString,
				AddressLine2 = longString,
				City = longString,
				PostCode = longString,
				Country = longString,
				StateOrProvince = longString,
			};

			var orgDefaults = new OrganisationDefaults();
			orgRecord.SetDefaultsForNewChildFromUnmatchOrgRecord(orgDefaults);

			CombineAssertions(delegate
			{
				Assert("Should be at least half a dozen fields in the OrgDefaults.", orgDefaults.Count > 5);
				foreach (OrgFieldDefault orgDefault in orgDefaults)
				{
					Assert("Default value for " + orgDefault.FieldName + " should be trimmed to the maxlength of the target field.", orgDefault.Value.ToString().Length < 200);
				}
			});
		}

		public void TestFindUnmatchOrgByOrgType()
		{
			UnmatchOrgRecord result = OrgRecords.FindUnmatchOrgByOrgType(OrganisationTypes.Debtor);
			AssertNull("OrgRecords doesn't contain debtor detail", result);

			result = OrgRecords.FindUnmatchOrgByOrgType(OrganisationTypes.Consignee);
			AssertNotNull("OrgRecords contain consignee detail", result);
			AssertEquals("Name of OrgRecord returned", Consignee.OrganisationName, result.OrganisationName);
			AssertEquals("Organisation type of OrgRecord returned", Consignee.OrganisationType, result.OrganisationType);

			var unmatchOrgCriterial = new UnmatchOrgRecordCriteria();
			unmatchOrgCriterial.OrganisationType = OrganisationTypes.Forwarder;
			unmatchOrgCriterial.OrganisationSubType = OrganisationsSubTypeList.Descriptions.SendingForwarder;
			result = OrgRecords.FindUnmatchOrg(unmatchOrgCriterial);
			AssertNull("orgRecords doesn't contain sending forwarder detail", result);

			unmatchOrgCriterial.OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder;
			result = OrgRecords.FindUnmatchOrg(unmatchOrgCriterial);
			AssertNotNull("orgRecords contain receiving forwarder detail", result);
			AssertEquals("Organisation type of OrgRecord returned", ReceivingForwarder.OrganisationType, result.OrganisationType);
			AssertEquals("Name of OrgRecord returned", ReceivingForwarder.OrganisationName, result.OrganisationName);
			AssertEquals("Organisation type of OrgRecord returned", ReceivingForwarder.OrganisationSubType, result.OrganisationSubType);
		}

		public void TestSetDefaultsForNewChildFromUnmatchOrgRecord()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			var unmatchOrgCriterial = new UnmatchOrgRecordCriteria();

			unmatchOrgCriterial.OrganisationType = OrganisationTypes.Competitor;
			OrgRecords.SetFilterBusinessObjectFromUnmatchOrgRecord(unmatchOrgCriterial, orgDefaults);
			AssertEquals("OrgRecords don't have organisation of type competitor, orgDefaults is not defaulted from orgrecord", 0, orgDefaults.Count);

			unmatchOrgCriterial.OrganisationType = OrganisationTypes.Consignee;
			orgRecords.SetFilterBusinessObjectFromUnmatchOrgRecord(unmatchOrgCriterial, orgDefaults);
			AssertEquals("OrgRecords contain consignee detail, orgDefaults should have defaults from orgrecord", true, orgDefaults.Count > 0);

			OrgFieldDefault orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgHeader.Schema.OH_FullName);
			AssertNotNull(orgDefaultFromConsignee);
			AssertEquals("Name", Consignee.OrganisationName, orgDefaultFromConsignee.Value);

			orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_State);
			AssertNull(orgDefaultFromConsignee);

			orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_Address1);
			AssertNotNull(orgDefaultFromConsignee);
			AssertEquals("Address line 1", Consignee.AddressLine1, orgDefaultFromConsignee.Value);

			orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_Address2);
			AssertNotNull(orgDefaultFromConsignee);
			AssertEquals("Address line 2", Consignee.AddressLine2, orgDefaultFromConsignee.Value);

			orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_City);
			AssertNotNull(orgDefaultFromConsignee);
			AssertEquals("city", Consignee.City, orgDefaultFromConsignee.Value);

			orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_PostCode);
			AssertNotNull(orgDefaultFromConsignee);
			AssertEquals("post Code", Consignee.PostCode, orgDefaultFromConsignee.Value);

			orgDefaultFromConsignee = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_RN_NKCountryCode);
			AssertNotNull(orgDefaultFromConsignee);
			AssertEquals("Country", Consignee.Country, orgDefaultFromConsignee.Value);

			// receiving forwarder
			orgDefaults = new OrganisationDefaults();

			unmatchOrgCriterial.OrganisationType = OrganisationTypes.Forwarder;
			unmatchOrgCriterial.OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder;
			orgRecords.SetFilterBusinessObjectFromUnmatchOrgRecord(unmatchOrgCriterial, orgDefaults);
			AssertEquals("OrgRecords contain receiving forwarder detail, orgDefaults should have defaults from orgrecord", true, orgDefaults.Count > 0);

			OrgFieldDefault orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgHeader.Schema.OH_FullName);
			AssertNotNull(orgDefaultFromCForwarder);
			AssertEquals("Name", ReceivingForwarder.OrganisationName, orgDefaultFromCForwarder.Value);

			orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_State);
			AssertNull(orgDefaultFromCForwarder);

			orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_Address1);
			AssertNotNull(orgDefaultFromCForwarder);
			AssertEquals("Address line 1", ReceivingForwarder.AddressLine1, orgDefaultFromCForwarder.Value);

			orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_Address2);
			AssertNull(orgDefaultFromCForwarder);

			orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_City);
			AssertNull(orgDefaultFromCForwarder);

			orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_PostCode);
			AssertNotNull(orgDefaultFromCForwarder);
			AssertEquals("post Code", ReceivingForwarder.PostCode, orgDefaultFromCForwarder.Value);

			orgDefaultFromCForwarder = orgDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_RN_NKCountryCode);
			AssertNotNull(orgDefaultFromCForwarder);
			AssertEquals("Country", ReceivingForwarder.Country, orgDefaultFromCForwarder.Value);
		}

		protected override ZString NoteInXmlForTesting
		{
			get { return "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Consignee</OrganisationType><OrganisationSubType>Consignee</OrganisationSubType><OwnerCode>OWNER</OwnerCode><EDICode>EDICODE</EDICode><OrganisationName>Some Organisation Name</OrganisationName><AddressLine1>Address line 1</AddressLine1><AddressLine2>Address line 2</AddressLine2><PostCode>PostCode</PostCode><StateOrProvince>State</StateOrProvince><City>City</City></UnmatchOrgRecord></UnmatchOrgRecords>"; }
		}

		protected override ZString ExpectedResultAsHumanReadableText
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.Append("Organisation Type: Consignee");
				builder.Append("Owner Code: OWNER");
				builder.Append("EDI Code: EDICODE");
				builder.Append("Organisation Name: Some Organisation Name");
				builder.Append("Address Line 1: Address line 1");
				builder.Append("Address Line 2: Address line 2");
				builder.Append("Post Code: PostCode");
				builder.Append("State or Province: State");
				builder.Append("City: City");
				builder.Append(" ");

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		UnmatchOrgRecords OrgRecords
		{
			get
			{
				if (orgRecords == null)
				{
					orgRecords = new UnmatchOrgRecords(Factory);
					orgRecords.OrgDetailsList = new UnmatchOrgRecord[]
					{
							Consignee,
							ReceivingForwarder
					};
				}

				return orgRecords;
			}
		}
		UnmatchOrgRecords orgRecords;

		UnmatchOrgRecord Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = new UnmatchOrgRecord() { OrganisationType = "Consignee", OrganisationSubType = "Consignee", AddressLine1 = "Xxx", AddressLine2 = "yyy", EDICode = "consignee", OrganisationName = "consigneeName", City = "sydney", PostCode = "1234", Country = "AU" };
				}
				return consignee;
			}
		}
		UnmatchOrgRecord consignee;

		UnmatchOrgRecord ReceivingForwarder
		{
			get
			{
				if (receivingForwarder == null)
				{
					receivingForwarder = new UnmatchOrgRecord() { OrganisationType = nameof(OrganisationTypes.Forwarder), AddressLine1 = "yyy", EDICode = "recForwarder", OrganisationName = "recForwarderName", OrganisationSubType = OrganisationsSubTypeList.Descriptions.ReceivingForwarder, PostCode = "9999", Country = "NZ" };
				}
				return receivingForwarder;
			}
		}
		UnmatchOrgRecord receivingForwarder;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
