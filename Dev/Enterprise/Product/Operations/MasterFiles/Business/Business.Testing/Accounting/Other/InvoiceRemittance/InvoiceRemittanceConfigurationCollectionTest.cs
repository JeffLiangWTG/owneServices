using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ElementNames = Enterprise.Registry.Business.InvoiceRemittanceCustomisationElement.ElementNames;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRemittanceConfigurationCollection))]
	public sealed class InvoiceRemittanceConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceRemittanceConfigurationCollection>
	{
		public void TestGetMatchInvoiceRemittanceConfiguration()
		{
			var collection = GetConfigurationCollectionForTest(Factory);
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var matchPaymentReferenceCodeConfiguration = collection.GetMatchInvoiceRemittanceConfiguration(org);
			AssertEquals(2, matchPaymentReferenceCodeConfiguration.Length);
			AssertCollectionContains("AAA", matchPaymentReferenceCodeConfiguration.Select(x => x.Code));
			AssertCollectionContains("BBB", matchPaymentReferenceCodeConfiguration.Select(x => x.Code));

			var bestMatchInvoiceRemittanceConfiguration = collection.GetBestMatchInvoiceRemittanceConfiguration(org);
			AssertNotNull(bestMatchInvoiceRemittanceConfiguration);
			AssertEquals("BBB", bestMatchInvoiceRemittanceConfiguration.Code);
		}

		public static InvoiceRemittanceConfigurationCollection GetConfigurationCollectionForTest(BusinessObjectFactory factory)
		{
			var configurationCollection = new InvoiceRemittanceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), factory);
			var configuration1 = configurationCollection.AddNew();
			configuration1.Code = "AAA";
			configuration1.Description = "TestAAA";
			configuration1.DebtorLocation = "ALL";

			configuration1.Elements[ElementNames.CustomCode1].Include = true;
			configuration1.Elements[ElementNames.CustomCode1].Order = 1;
			configuration1.Elements[ElementNames.CustomCode1].DigitCode = "NumberForAAA";

			var configuration2 = configurationCollection.AddNew();
			configuration2.Code = "BBB";
			configuration2.Description = "TestBBB";
			configuration2.BillerCode = "PAY";
			configuration2.BillerAccountNumber = "123456";
			configuration2.Message = "This is description";
			configuration2.DebtorLocation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			configuration2.Elements[ElementNames.CustomCode1].Include = true;
			configuration2.Elements[ElementNames.CustomCode1].Order = 1;
			configuration2.Elements[ElementNames.CustomCode1].DigitCode = "XX";
			configuration2.Elements[ElementNames.BillerAccountNumber].Include = true;
			configuration2.Elements[ElementNames.BillerAccountNumber].Order = 2;
			configuration2.Elements[ElementNames.BillerAccountNumber].DigitCode = "6";

			var configuration3 = configurationCollection.AddNew();
			configuration3.Code = "CCC";
			configuration3.Description = "TestCCC";
			configuration3.DebtorLocation = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RN_Code;

			configuration3.Elements[ElementNames.CustomCode1].Include = true;
			configuration3.Elements[ElementNames.CustomCode1].Order = 1;
			configuration3.Elements[ElementNames.CustomCode1].DigitCode = "NumberForCCC";

			return configurationCollection;
		}

		public static InvoiceRemittanceConfigurationCollection GetConfigurationCollectionnWithExceedMaxLengthElementForTest(BusinessObjectFactory factory)
		{
			var configurationCollection = new InvoiceRemittanceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), factory);
			var configuration1 = configurationCollection.AddNew();
			configuration1.Code = "AAA";
			configuration1.Description = "TestAAA";
			configuration1.DebtorLocation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			configuration1.Elements[ElementNames.CustomCode1].Include = true;
			configuration1.Elements[ElementNames.CustomCode1].Order = 1;
			configuration1.Elements[ElementNames.CustomCode1].DigitCode = new string('A', 121);

			return configurationCollection;
		}

		#region Implementation

		protected override InvoiceRemittanceConfigurationCollection GetCollectionToTest()
		{
			return new InvoiceRemittanceConfigurationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceRemittanceConfiguration(Factory);
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
