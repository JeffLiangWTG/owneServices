using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatchOverride))]
	sealed class OrgPatternMatchOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TesteNettGenericChargeCodesContainsMiscCharges()
		{
			Assert("eNettGenericChargeCodes contains MISC1", Match.eNettGenericChargeCodes.ContainsCode("MISC1"));
			Assert("eNettGenericChargeCodes contains MISC2", Match.eNettGenericChargeCodes.ContainsCode("MISC2"));
			Assert("eNettGenericChargeCodes contains MISC3", Match.eNettGenericChargeCodes.ContainsCode("MISC3"));
			Assert("eNettGenericChargeCodes contains MISC4", Match.eNettGenericChargeCodes.ContainsCode("MISC4"));
			Assert("eNettGenericChargeCodes contains MISC5", Match.eNettGenericChargeCodes.ContainsCode("MISC5"));
			Assert("eNettGenericChargeCodes contains MISC6", Match.eNettGenericChargeCodes.ContainsCode("MISC6"));
			Assert("eNettGenericChargeCodes contains MISC7", Match.eNettGenericChargeCodes.ContainsCode("MISC7"));
			Assert("eNettGenericChargeCodes contains MISC8", Match.eNettGenericChargeCodes.ContainsCode("MISC8"));
			Assert("eNettGenericChargeCodes contains MISC9", Match.eNettGenericChargeCodes.ContainsCode("MISC9"));
			Assert("eNettGenericChargeCodes contains MISC10", Match.eNettGenericChargeCodes.ContainsCode("MISC10"));
			Assert("eNettGenericChargeCodes contains MISC11", Match.eNettGenericChargeCodes.ContainsCode("MISC11"));
			Assert("eNettGenericChargeCodes contains MISC12", Match.eNettGenericChargeCodes.ContainsCode("MISC12"));
			Assert("eNettGenericChargeCodes contains MISC13", Match.eNettGenericChargeCodes.ContainsCode("MISC13"));
			Assert("eNettGenericChargeCodes contains MISC14", Match.eNettGenericChargeCodes.ContainsCode("MISC14"));
			Assert("eNettGenericChargeCodes contains MISC15", Match.eNettGenericChargeCodes.ContainsCode("MISC15"));
			Assert("eNettGenericChargeCodes contains MISC16", Match.eNettGenericChargeCodes.ContainsCode("MISC16"));
			Assert("eNettGenericChargeCodes contains MISC17", Match.eNettGenericChargeCodes.ContainsCode("MISC17"));
			Assert("eNettGenericChargeCodes contains MISC18", Match.eNettGenericChargeCodes.ContainsCode("MISC18"));
			Assert("eNettGenericChargeCodes contains MISC19", Match.eNettGenericChargeCodes.ContainsCode("MISC19"));
			Assert("eNettGenericChargeCodes contains MISC20", Match.eNettGenericChargeCodes.ContainsCode("MISC20"));
			Assert("eNettGenericChargeCodes contains SHPMCORFEE", Match.eNettGenericChargeCodes.ContainsCode("SHPMCORFEE"));
		}

		public void TestOORelationship_List()
		{
			AssertEquals("OO_Relationship_List.Count", 17, Match.Lookups.OO_Relationship_List.Count);
		}

		public void TestTableDescriptiveNameIsCorrect()
		{
			var descriptiveTableName = DbErrorMatch.GetTableDescriptiveName(Match.TableName);
			AssertMultilineASCIIEquals("OrgPatternMatchOverride table should show as 'Organization EDI Code Mapping'", "Organization EDI Code Mapping", descriptiveTableName);
		}

		public void TestClearOfLocalGuid()
		{
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery());
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			Match.OO_LocalGuid = country.PK;

			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Country;
			AssertEquals("Value Cleared", ZGuid.Empty, Match.OO_LocalGuid);
		}

		public void TestLocalFieldsReadonlyOnChangingRelationship()
		{
			Match.OO_Relationship = "";
			Assert("OO_LocalCode should be readonly", Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should be readonly", Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			Assert("OO_LocalCode should be readonly", Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should not be readonly", !Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Currency;
			Assert("OO_LocalCode should be readonly", Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should not be readonly", !Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			Assert("OO_LocalCode should not be readonly", !Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should be readonly", Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			Assert("OO_LocalCode should be readonly", Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should not be readonly", !Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			Assert("OO_LocalCode should not be readonly", !Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should be readonly", Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.PackageType;
			Assert("OO_LocalCode should not be readonly", !Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should be readonly", Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.EventCode;
			Assert("OO_LocalCode should not be readonly", !Match.OO_LocalCodeInfo.ReadOnly);
			Assert("OO_LocalGuid should be readonly", Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Commodities;
			AssertEquals("OO_LocalCode should be readonly", true, Match.OO_LocalCodeInfo.ReadOnly);
			AssertEquals("OO_LocalGuid should not be readonly", false, Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.DropMode;
			AssertEquals("OO_LocalCode should not be readonly", false, Match.OO_LocalCodeInfo.ReadOnly);
			AssertEquals("OO_LocalGuid should be readonly", true, Match.OO_LocalGuidInfo.ReadOnly);

			Match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Equipment;
			AssertEquals("OO_LocalCode should be readonly", true, Match.OO_LocalCodeInfo.ReadOnly);
			AssertEquals("OO_LocalGuid should not be readonly", false, Match.OO_LocalGuidInfo.ReadOnly);
		}

		public void TestGetLocalCode()
		{
			Match.OO_Relationship = "";
			AssertEquals("LocalCode should be empty by default", ZString.Empty, Match.GetLocalCode());

			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			Match.OO_LocalCode = "FRT";

			AssertEquals("When the relationship is a code, LocalCode should return OO_LocalCode", "FRT", Match.GetLocalCode());

			Match.OO_Relationship = "XXX";

			AssertEquals("Invalid code cannot be matched", ZString.Empty, Match.GetLocalCode());

			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "20ME";
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			Match.OO_LocalGuid = containerType.PK;

			AssertEquals("When the relationship is a GUID, LocalCode should be the code property the BizObj", CodePropertyAttribute.CodeFromBusinessObject(containerType), Match.GetLocalCode());

			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			Match.OO_LocalCode = Constants.IncoTerms.FreeOnBoard;

			AssertEquals(Constants.IncoTerms.FreeOnBoard, Match.GetLocalCode());

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			Match.OO_LocalGuid = orgHeader.PK;

			AssertEquals(CodePropertyAttribute.CodeFromBusinessObject(orgHeader), Match.GetLocalCode());

			var docType = Factory.NewWithValidTestData<RefDocType>();
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DocumentType;
			Match.OO_LocalGuid = docType.PK;
			AssertEquals(CodePropertyAttribute.CodeFromBusinessObject(docType), Match.GetLocalCode());
		}

		public void TestOrgCoNameOrGuid()
		{
			Match.OO_Relationship = "CHC";
			Match.OO_LocalCode = "WHSORD";
			AssertEquals("Should show the code", "WHSORD", Match.OrgCoNameorGuid);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "BMW";
			Match.OO_Relationship = "ORG";
			Match.OO_LocalGuid = orgHeader.PK;
			Factory.Save();

			AssertEquals("Should show the guid", Match.OO_LocalGuid.ToString(), Match.OrgCoNameorGuid);

			var code = RelatedBusinessObjectAttribute.GetCodeForGuid(Match.OrgCoNameorGuidInfo);
			AssertEquals("OrgCoNameorGuidInfo should be organisation PK", orgHeader.PK, new ZGuid(Match.OrgCoNameorGuidInfo.Value));
			AssertEquals("Ensure that the RelatedBusinessObjectAttribute is correct", "BMW", code);
		}

		[ExpectNoExceptions]
		public void TestGetLocalCode_AllValidRelationshipsAreMappedToLocalCode()
		{
			Match.OO_Relationship = "";
			AssertEquals("LocalCode should be empty by default", ZString.Empty, Match.GetLocalCode());

			foreach (CodeDescriptionPair pair in Match.Lookups.OO_Relationship_List)
			{
				Match.OO_Relationship = pair.Code;

				Assert("Pre-condition: changing the relationship type should clear these fields", Match.OO_LocalGuid.IsEmpty);
				Assert("Pre-condition: changing the relationship type should clear these fields", Match.OO_LocalCode.IsEmpty);

				Assert("If this throws an exception you've probably haven't added your new GUID relationship type to GetLocalBusinessObject. Should be empty as there's nothing to map to yet", Match.GetLocalCode().IsEmpty);
			}
		}

		public void TestOO_ForeignCodeFieldType()
		{
			Match.OO_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertEquals("OO_ForeignCodeFieldType", nameof(FieldType.Text), Match.OO_ForeignCodeFieldType);

			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			AssertEquals("OO_ForeignCodeFieldType", nameof(FieldType.Text), Match.OO_ForeignCodeFieldType);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsENettOrganisation(It.IsAny<ZGuid>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("OO_ForeignCodeFieldType", nameof(FieldType.TextDropEdit), Match.OO_ForeignCodeFieldType);
			}
		}

		public void TestOrgCoFieldType()
		{
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Country;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Commodities;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Equipment;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Warehouse;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ServiceLevel;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.IntZone;
			AssertEquals("OrgCoFieldType", nameof(FieldType.Guid), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			AssertEquals("OrgCoFieldType", nameof(FieldType.TextCodeFindBox), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DropMode;
			AssertEquals("OrgCoFieldType", nameof(FieldType.TextDropEdit), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			AssertEquals("OrgCoFieldType", nameof(FieldType.TextDropEdit), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.PackageType;
			AssertEquals("OrgCoFieldType", nameof(FieldType.TextDropEdit), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.EventCode;
			AssertEquals("OrgCoFieldType", nameof(FieldType.TextDropEdit), Match.OrgCoFieldType);
			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel;
			AssertEquals("OrgCoFieldType", nameof(FieldType.TextDropEdit), Match.OrgCoFieldType);
		}

		public void TestIsComPayMapping()
		{
			Match.OO_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertEquals(false, Match.IsComPayMapping);

			Match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			AssertEquals(false, Match.IsComPayMapping);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsENettOrganisation(It.IsAny<ZGuid>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals(true, Match.IsComPayMapping);
			}
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldEDIMappingValue = Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed;

			try
			{
				OrgPatternMatchOverride testOverride = OrgInDB.CreatePatternMatchOverrideForTest();

				Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testOverride.OO_ForeignCodeInfo.ReadOnly);
				testOverride.OO_Relationship = "INC";
				Assert("Access Allowed - Not ReadOnly", !testOverride.OO_LocalCodeInfo.ReadOnly);
				testOverride.OO_Relationship = "ORG";
				Assert("Access Allowed - Not ReadOnly", !testOverride.OO_LocalGuidInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testOverride.OO_OHInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testOverride.OO_RelationshipInfo.ReadOnly);

				Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testOverride.OO_ForeignCodeInfo.ReadOnly);
				testOverride.OO_Relationship = "INC";
				Assert("Access NOT Allowed - ReadOnly", testOverride.OO_LocalCodeInfo.ReadOnly);
				testOverride.OO_Relationship = "ORG";
				Assert("Access NOT Allowed - ReadOnly", testOverride.OO_LocalGuidInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testOverride.OO_OHInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testOverride.OO_RelationshipInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConfigModifyEDICodeMapping.IsAllowed = oldEDIMappingValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region TestLocalCodeGuidHumanReadableName

		public void TestLocalCodeGuidHumanReadableName()
		{
			OrgPatternMatchOverride match = Factory.New<OrgPatternMatchOverride>();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;

			AssertEquals("Organization", match.OO_LocalCodeInfo.HumanReadableName);
			AssertEquals("Organization", match.OO_LocalGuidInfo.HumanReadableName);

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Country;

			AssertEquals("Country/Region", match.OO_LocalCodeInfo.HumanReadableName);
			AssertEquals("Country/Region", match.OO_LocalGuidInfo.HumanReadableName);
		}

		#endregion

		#region TestNoAuditLogForOrgPatternMatchOverride

		public void TestNoAuditLogForOrgPatternMatchOverride()
		{
			var match = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			Factory.Save();

			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ServiceLevel;
			Factory.Save();

			match.Delete();
			Factory.Save();

			var auditLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, match.PK));
			AssertEquals(0, auditLogs.Length);
		}

		#endregion

		#region Implementation

		OrgPatternMatchOverride Match;

		protected override void SetUp()
		{
			base.SetUp();
			Match = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
		}

		#endregion
	}
}
