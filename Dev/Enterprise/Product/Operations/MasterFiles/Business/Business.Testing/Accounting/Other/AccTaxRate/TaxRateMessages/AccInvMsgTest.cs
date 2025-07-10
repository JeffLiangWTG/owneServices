using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccInvMsg))]
	sealed class AccInvMsgTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccInvMsg>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestRestrictedFilteredItemAttribute()
		{
			AssertEquals("Attribute is required to get business objects from code with filters (countryCode) on ZPopupFindBox", true, GetExpectedBusinessObjectType().GetCustomAttributes(typeof(RestrictedFilteredItemAttribute), true).Any());
		}

		public void TestConstructorPopulatesCountryCode()
		{
			var msg = Factory.NewWithValidTestData<AccInvMsg>();
			Assert(!msg.IsInDatabase);
			AssertEquals(Env.CurrentCompany.Country.Code, msg.A9_RN_NKCountryCode);
		}

		public void TestHumanReadableNameCore()
		{
			var msg = Factory.NewWithValidTestData<AccInvMsg>();
			msg.A9_Code = "Testing";
			msg.A9_Description = " Testing, One, Two, Three";
			Factory.Save();

			AssertEquals("Invoice Tax Message - Testing - Testing, One, Two, Three", msg.HumanReadableName);
		}

		public void TestGetTaxGroupCodesFromBinaryValues()
		{
			var taxGroupCode = "N1";
			var taxGroupDesription = "Description N1";
			var testValue = new CodeDescriptionBoolRelatedItemCollection();
			testValue.Add(taxGroupCode, (NoResString)taxGroupDesription, true, "N1.0");

			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testValue);

			var query = new ZQuery(StmDataSchema.SD_Name, "TaxMessageGroupsManagement");
			query.AddToFilter(StmDataSchema.SD_Owner, GlbCompany.CurrentCompany.PK.ToGuid());
			var registryRecord = Factory.LoadTop1<StmData>(query);

			AssertNotNull(registryRecord);

			var result = AccInvMsg.GetTaxGroupCodesFromBinaryValues(registryRecord.SD_BinaryValue);
			AssertEquals("The result list should have 1 item", 1, result.Count);
			AssertCollectionContains(new CodeDescriptionPair(taxGroupCode, taxGroupDesription), result);
		}

		public void TestNoStmALogs()
		{
			var msg = Factory.NewWithValidTestData<AccInvMsg>();
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, msg.PK);

				AssertEquals("No StmALog should be generated after creating an AccInvMsg.", 0, Factory.Load<StmALog>(query).Length);

				msg.A9_Description = "Happy everyday";
				Factory.Save();

				AssertEquals("No StmALog should be generated after editing an AccInvMsg.", 0, Factory.Load<StmALog>(query).Length);

				msg.Delete();
				Factory.Save();

				AssertEquals("No StmALog should be generated after deleting an AccInvMsg.", 0, Factory.Load<StmALog>(query).Length);
			});
		}
	}
}
