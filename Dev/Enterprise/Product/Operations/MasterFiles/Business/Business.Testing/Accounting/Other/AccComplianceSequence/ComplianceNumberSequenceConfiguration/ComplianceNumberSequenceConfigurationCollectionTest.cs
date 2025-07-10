using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceConfigurationCollection))]
	public sealed class ComplianceNumberSequenceConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceNumberSequenceConfigurationCollection>
	{
		public void TestAllowNewOrRemove()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			var configurationCollection = new ComplianceNumberSequenceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			Assert("we cannot add new config when there is a default mandatory config", !configurationCollection.AllowNew);
			Assert("we can remove config when there is a default mandatory config", configurationCollection.AllowRemove);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.VietNam);
			configurationCollection = new ComplianceNumberSequenceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			Assert(configurationCollection.AllowNew);
			Assert(configurationCollection.AllowRemove);
		}

		public void TestAddMandatoryComplianceNumberSequenceConfiguration()
		{
			var configurationCollection = new ComplianceNumberSequenceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			AssertEquals(0, configurationCollection.Count);
			configurationCollection.AddMandatoryComplianceNumberSequenceConfiguration(Factory, GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals(0, configurationCollection.Count);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			configurationCollection.AddMandatoryComplianceNumberSequenceConfiguration(Factory, GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals(1, configurationCollection.Count);
			var config = configurationCollection.Cast<ComplianceNumberSequenceConfiguration>().First();
			AssertEquals("MPC", config.Code);
			AssertEquals("Mandatory Portugal Configuration", config.Description);

			config.Description = "new description";
			AssertEquals("new description", config.Description);
			configurationCollection.AddMandatoryComplianceNumberSequenceConfiguration(Factory, GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals(1, configurationCollection.Count);
			config = configurationCollection.Cast<ComplianceNumberSequenceConfiguration>().First();
			AssertEquals("MPC", config.Code);
			AssertEquals("Mandatory Portugal Configuration", config.Description);

			config.Code = "OTH";
			configurationCollection.AddMandatoryComplianceNumberSequenceConfiguration(Factory, GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals(2, configurationCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "MPC", "OTH" }, configurationCollection.Cast<ComplianceNumberSequenceConfiguration>().Select(x => x.Code));
		}

		public static ComplianceNumberSequenceConfigurationCollection GetConfigurationCollectionForTest(BusinessObjectFactory factory)
		{
			var configurationCollection = new ComplianceNumberSequenceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), factory);
			var configuration1 = configurationCollection.AddNew();
			configuration1.Code = "AAA";
			configuration1.Description = "TestAAA";

			configuration1.Elements[ElementNames.ComplianceDateDayOfIssue].Include = true;
			configuration1.Elements[ElementNames.ComplianceDateDayOfIssue].Order = 1;

			configuration1.Elements[ElementNames.InvoiceDateYearOfIssue].Include = true;
			configuration1.Elements[ElementNames.InvoiceDateYearOfIssue].Order = 2;
			configuration1.Elements[ElementNames.InvoiceDateYearOfIssue].DigitCode = "2";

			configuration1.Elements[ElementNames.OriginalAmendmentStatus].Include = true;
			configuration1.Elements[ElementNames.OriginalAmendmentStatus].Order = 3;
			configuration1.Elements[ElementNames.OriginalAmendmentStatus].DigitCode = "a/b";

			configuration1.Elements[ElementNames.CustomElement1].Include = true;
			configuration1.Elements[ElementNames.CustomElement1].Order = 4;
			configuration1.Elements[ElementNames.CustomElement1].DigitCode = "C";

			configuration1.Elements[ElementNames.BlankSpace].Include = true;
			configuration1.Elements[ElementNames.BlankSpace].Order = 5;
			configuration1.Elements[ElementNames.BlankSpace].Length = 1;

			configuration1.Elements[ElementNames.SeriesPrefix].Include = true;
			configuration1.Elements[ElementNames.SeriesPrefix].Order = 6;

			var configuration2 = configurationCollection.AddNew();
			configuration2.Code = "BBB";
			configuration2.Description = "TestBBB";

			configuration2.Elements[ElementNames.CustomElement1].Include = true;
			configuration2.Elements[ElementNames.CustomElement1].Order = 1;
			configuration2.Elements[ElementNames.CustomElement1].DigitCode = "XX";
			configuration2.Elements[ElementNames.CustomElement2].Include = true;
			configuration2.Elements[ElementNames.CustomElement2].Order = 2;
			configuration2.Elements[ElementNames.CustomElement2].DigitCode = "6";

			var configuration3 = configurationCollection.AddNew();
			configuration3.Code = "CCC";
			configuration3.Description = "TestCCC";

			configuration3.Elements[ElementNames.ComplianceSubType].Include = true;
			configuration3.Elements[ElementNames.ComplianceSubType].Order = 1;

			configuration3.Elements[ElementNames.BlankSpace].Include = true;
			configuration3.Elements[ElementNames.BlankSpace].Order = 2;
			configuration3.Elements[ElementNames.BlankSpace].Length = 5;

			configuration3.Elements[ElementNames.SeriesPrefix].Include = true;
			configuration3.Elements[ElementNames.SeriesPrefix].Order = 3;

			return configurationCollection;
		}

		#region Implementation

		protected override ComplianceNumberSequenceConfigurationCollection GetCollectionToTest()
		{
			return new ComplianceNumberSequenceConfigurationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceNumberSequenceConfiguration(Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
