using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OGAAgencyRequirementCollection))]
	public class OGAAgencyRequirementCollectionCollectionTests : NonPersistentBusinessObjectCollectionTestCase<OGAAgencyRequirementCollection>
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

		public void TestTSCAPopulate()
		{
			var collection = GetCollectionToTest();
			Assert(collection.Cast<OGAAgencyRequirement>().Any(x => x.AgencyCode != GovernmentAgencyProgramCodeList.Codes.TSCA));
			var element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.TSCA);
			AssertEquals("EPA - TSCA - Toxic Substance Control Act", element.AgencyCodeWithDescription);
		}

		public void TestPopulate()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				var collection = GetCollectionToTest();
				var element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.DOT);
				AssertNull("DOT - OGA - Department of Transportation", element);

				using (ZZCustomsFunctionality.TemporarilySetupFWSEffective(false))
				{
					AssertNotNull(collection.OfType<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.DDTC));
					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes._370);
					AssertEquals("NMFS - 370 - National Marine Fisheries Service - Tuna, Tuna Products", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.AMR);
					AssertEquals("NMFS - AMR - National Marine Fisheries Service - Antarctic Marine Living Resources", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.COA);
					AssertEquals("NMFS - COA - National Marine Fisheries Service - Certificate of Admissibility", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.HMS);
					AssertEquals("NMFS - HMS - National Marine Fisheries Service - Highly Migratory Species", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.SIMP);
					AssertEquals("NMFS - SIMP - National Marine Fisheries Service - Seafood Import Monitoring Program", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.DDTC);
					AssertEquals("DDTC - Directorate of Defense Trade Controls", element.AgencyCodeWithDescription);

					AssertNull("FCC - should not show in ACE", collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FCC));

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FDA);
					AssertEquals("FDA - PGA - Food And Drug Administration", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FSIS);
					AssertEquals("USDA - FSIS - Food Safety and Inspection Service", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.ODS);
					AssertEquals("EPA - ODS - Ozone Depleting Substances", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.PST);
					AssertEquals("EPA - PST - Pesticides (FIFRA)", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.HFC);
					AssertEquals("EPA - HFC - Hydrofluorocarbons", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.VNE);
					AssertEquals("EPA - VNE - Vehicles and Engines", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.TTB);
					AssertEquals("TTB - PGA - U.S. Department of the Treasury, Alcohol and Tobacco Tax and Trade Bureau", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.APHIS);
					AssertEquals("APHIS - Animal and Plant Health Inspection Service", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.TSCA);
					AssertEquals("EPA - TSCA - Toxic Substance Control Act", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.AMS);
					AssertEquals("USDA - AMS - Agricultural Marketing Service", element.AgencyCodeWithDescription);

					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.NOP);
					AssertEquals("USDA - AMS - NOP - National Organic Program", element.AgencyCodeWithDescription);

					AssertNotNull("FWS - U.S. Fish and Wildlife Service", collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FWS));
					AssertNotNull(collection.OfType<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.NHTSA));

					collection = GetCollectionToTest();
					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.Lacey);
					AssertEquals("Lacey - PGA - Lacey Act", element.AgencyCodeWithDescription);

					collection = GetCollectionToTest();
					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.APHIS);
					AssertNotNull("APHIS should be exposed. The registry is publicly accessible", element);
					AssertEquals(24, collection.Count);

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
					{
						collection = GetCollectionToTest();
						element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.OMC);
						AssertEquals("OMC - Office of Marine Conservation", element.AgencyCodeWithDescription);
					}
				}

				using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
				{
					collection = GetCollectionToTest();
					element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FWS);
					AssertEquals("FWS - U.S. Fish and Wildlife Service", element.AgencyCodeWithDescription);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
					{
						collection = GetCollectionToTest();
						element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.CPSC);
						AssertEquals("CPSC - U.S. Consumer Product Safety Commission", element.AgencyCodeWithDescription);

						collection = GetCollectionToTest();
						element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.DEA);
						AssertEquals("DEA - Drug Enforcement Administration", element.AgencyCodeWithDescription);
					}
				}
			}
		}

		public void TestPopulateForACSCertificationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.OGAAgencyRequirements;

			var element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.DOT);
			AssertEquals("DOT - OGA - Department of Transportation", element.AgencyCodeWithDescription);

			element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FDA);
			AssertEquals("This is an ACS job", "FDA - OGA - Food And Drug Administration", element.AgencyCodeWithDescription);

			element = collection.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == GovernmentAgencyProgramCodeList.Codes.FCC);
			AssertEquals("FCC - OGA - Federal Communications Commission", element.AgencyCodeWithDescription);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new InvalidOperationException();
		}

		protected override OGAAgencyRequirementCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var collection = new OGAAgencyRequirementCollection(new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine));
			collection.Populate();
			return collection;
		}
	}
}
