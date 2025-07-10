using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class TWJobDocAddressDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		class TWJobDocAddressDataObjectWriterForTest : JobDocAddressDataObjectWriter
		{
			public TWJobDocAddressDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
			{
			}

			public OrganizationAddress PopulateDataObjectForTest(JobDocAddress docAddressBO)
			{
				return base.PopulateDataObject(docAddressBO);
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateCountrySpecificData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				var orgHeader = Factory.New<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				var writer = new TWJobDocAddressDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				AssertPopulateCountrySpecificData(declaration.SupplierDocumentaryAddress);
				AssertPopulateCountrySpecificData(declaration.ImporterDocumentaryAddress);
				AssertPopulateCountrySpecificData(declaration.SupplierPickupAddress);
				AssertPopulateCountrySpecificData(declaration.ImporterDeliveryAddress);
				void AssertPopulateCountrySpecificData(JobDocAddress jobDocAddress)
				{
					var twJobDocAddress = jobDocAddress as TWJobDocAddress;
					twJobDocAddress.E2_AddressOverride = true;
					twJobDocAddress.E2_AdditionalAddressInformation = "E2_AdditionalAddressInformation";
					var data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.AdditionalAddressInformation, Is.EqualTo("E2_AdditionalAddressInformation").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection, Is.EqualTo(default(System.Collections.Generic.List<Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber>)));
					NUnit.Framework.Assert.That(data.GovRegNumType, Is.EqualTo(default(RegistrationNumberType)), "GovRegNumType - should be [null]");
					NUnit.Framework.Assert.That(data.GovRegNum.ToString(), Is.Null.Or.Empty);
					twJobDocAddress.IDCodeType = "VAT";
					twJobDocAddress.IDCode = "12345678";
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection, Is.EqualTo(default(System.Collections.Generic.List<Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber>)));
					NUnit.Framework.Assert.That(data.GovRegNumType.Code, Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(data.GovRegNum, Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
					twJobDocAddress.AEOCode = "22345678";
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Count, Is.EqualTo(1));
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "AEO" && c.Value.GetValueOrDefault() == "22345678"), Is.True);
					NUnit.Framework.Assert.That(data.GovRegNumType.Code, Is.EqualTo("VAT").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(data.GovRegNum, Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
					twJobDocAddress.IDCodeType = ZString.Empty;
					twJobDocAddress.IDCode = ZString.Empty;
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Count, Is.EqualTo(1));
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "AEO" && c.Value.GetValueOrDefault() == "22345678"), Is.True);
					NUnit.Framework.Assert.That(data.GovRegNumType, Is.EqualTo(default(RegistrationNumberType)), "GovRegNumType - should be [null]");
					NUnit.Framework.Assert.That(data.GovRegNum.ToString(), Is.Null.Or.Empty);
					twJobDocAddress.TPCCode = "32345678";
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Count, Is.EqualTo(2));
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "AEO" && c.Value.GetValueOrDefault() == "22345678"), Is.True);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "TPC" && c.Value.GetValueOrDefault() == "32345678"), Is.True);
					NUnit.Framework.Assert.That(data.GovRegNumType, Is.EqualTo(default(RegistrationNumberType)), "GovRegNumType - should be [null]");
					NUnit.Framework.Assert.That(data.GovRegNum.ToString(), Is.Null.Or.Empty);
					twJobDocAddress.IDCodeType = "PAS";
					twJobDocAddress.IDCode = "42345678";
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Count, Is.EqualTo(2));
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "AEO" && c.Value.GetValueOrDefault() == "22345678"), Is.True);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "TPC" && c.Value.GetValueOrDefault() == "32345678"), Is.True);
					NUnit.Framework.Assert.That(data.GovRegNumType.Code, Is.EqualTo("PAS").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(data.GovRegNum, Is.EqualTo("42345678").Using(CustomComparers.TypeComparison));
					twJobDocAddress.IDCodeType = "PID";
					twJobDocAddress.IDCode = "52345678";
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Count, Is.EqualTo(3));
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "PID" && c.Value.GetValueOrDefault() == "52345678"), Is.True);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "AEO" && c.Value.GetValueOrDefault() == "22345678"), Is.True);
					NUnit.Framework.Assert.That(data.RegistrationNumberCollection.Any(c => c.Type.Code.GetValueOrDefault() == "TPC" && c.Value.GetValueOrDefault() == "32345678"), Is.True);
					NUnit.Framework.Assert.That(data.GovRegNumType, Is.EqualTo(default(RegistrationNumberType)), "GovRegNumType - should be [null]");
					NUnit.Framework.Assert.That(data.GovRegNum.ToString(), Is.Null.Or.Empty);
					twJobDocAddress.E2_AddressOverride = false;
					address.PrimaryOrgAddressAdditionalInfoDetail = "OA_AdditionalAddressInformation";
					twJobDocAddress.E2_OA_Address = address.PK;
					data = writer.PopulateDataObjectForTest(twJobDocAddress);
					NUnit.Framework.Assert.That(data.AdditionalAddressInformation, Is.EqualTo("OA_AdditionalAddressInformation").Using(CustomComparers.TypeComparison));
				}
			}
		}
	}
}
