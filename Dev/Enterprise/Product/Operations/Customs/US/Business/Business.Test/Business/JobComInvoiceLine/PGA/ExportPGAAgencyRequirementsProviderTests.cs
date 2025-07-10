using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ExportPGAAgencyRequirementCollection))]
	public class ExportPGAAgencyRequirementsProviderTests : NonPersistentBusinessObjectCollectionTestCase<ExportPGAAgencyRequirementCollection>
	{
		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		public void TestPopulateRequirements()
		{
			var collection = GetCollectionToTest();
			AssertEquals("AMS, EPA, NMFS, ATF, DEA, FWS, TTB are allowed for Export currectly", 7, collection.Count);
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.AMS));
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.EPA));
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.NMFS));
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.ATF));
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.DEA));
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FWS));
			AssertNotNull(collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.TTB));
		}

		protected override ExportPGAAgencyRequirementCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var collection = new ExportPGAAgencyRequirementCollection(new ExportPGAInvoiceLineRequirementsProvider(invoiceLine));
			collection.Populate();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new InvalidOperationException();
		}
	}
}
