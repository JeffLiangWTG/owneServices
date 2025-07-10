using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceConfiguration))]
	sealed class ComplianceNumberSequenceConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCodeMaxLength()
		{
			AssertEquals(3, BizObj.CodeInfo.MaxLength);
		}

		public void TestCodeReadOnly()
		{
			PreparedUsedConfiguration();

			AssertEquals(true, BizObj.Code_ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			var config = new ComplianceNumberSequenceConfiguration();
			config.CurrentFallbackLevel = NewFallbackLevel();
			Assert(!config.IsMandatory);
			Assert(!config.Code_ReadOnly);
			Assert(!config.Description_ReadOnly);

			config.Code = "MPC";
			Assert(config.IsMandatory);
			Assert(config.Code_ReadOnly);
			Assert(config.Description_ReadOnly);
		}

		public void TestDelete()
		{
			PreparedUsedConfiguration();

			AssertEquals(false, BizObj.CanDelete);
			AssertEquals("This code is already used thus cannot be deleted.", BizObj.ReasonForNotAbleToDelete);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			var config = new ComplianceNumberSequenceConfiguration();
			config.CurrentFallbackLevel = NewFallbackLevel();
			config.Code = "MPC";
			Assert(config.IsMandatory);
			Assert(!config.CanDelete);
			AssertEquals("This code is mandatory thus cannot be deleted.", config.ReasonForNotAbleToDelete);
		}

		public void TestIsMandatory()
		{
			var config = new ComplianceNumberSequenceConfiguration();
			AssertEquals("pre-condition", string.Empty, config.Code);
			Assert("config is not mandatory when config code is empty", !config.IsMandatory);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			config.Code = "MPC";
			Assert("MPC config is mandatory for portugal companies", config.IsMandatory);

			config = new ComplianceNumberSequenceConfiguration();
			config.Code = "OTH";
			Assert("only MPC config is mandatory for portugal companies", !config.IsMandatory);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.VietNam);
			config = new ComplianceNumberSequenceConfiguration();
			config.Code = "MPC";
			Assert("MPC config is mandatory only for portugal companies", !config.IsMandatory);
		}

		public void TestGetFormatedComplianceDocumentNumber()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Indonesia);
			var complianceNumberSequenceMock = new Mock<IComplianceNumberSequence>();
			complianceNumberSequenceMock.SetupGet(x => x.ComplianceSubType).Returns("T01");
			complianceNumberSequenceMock.SetupGet(x => x.IsCorrected).Returns(true);
			complianceNumberSequenceMock.SetupGet(x => x.ComplianceTransactionType).Returns("INV");
			complianceNumberSequenceMock.SetupGet(x => x.ComplianceDocumentDate).Returns(new ZDateTime(2019, 5, 6));
			complianceNumberSequenceMock.SetupGet(x => x.InvoiceDate).Returns(new ZDateTime(2019, 1, 2));
			complianceNumberSequenceMock.SetupGet(x => x.PostDate).Returns(new ZDateTime(2019, 3, 4));

			var configurationCollection = new ComplianceNumberSequenceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			var configuration1 = configurationCollection.AddNew();
			configuration1.Code = "AAA";
			configuration1.Description = "TestAAA";
			configuration1.Elements.Cast<ComplianceNumberSequenceCustomisationElement>().ForEach(x => x.Include = true);
			configuration1.Elements[ElementNames.CustomElement1].DigitCode = "CUS1";
			configuration1.Elements[ElementNames.CustomElement2].DigitCode = "CUS2";
			configuration1.Elements[ElementNames.OriginalAmendmentStatus].DigitCode = "a/b";
			configuration1.Elements[ElementNames.BlankSpace].Length = 2;
			configuration1.Elements[ElementNames.ComplianceDateYearOfIssue].Length = 1;
			configuration1.Elements[ElementNames.InvoiceDateYearOfIssue].Length = 2;
			configuration1.Elements[ElementNames.PostDateYearOfIssue].Length = 4;

			var formatedNumber = configuration1.GetFormatedComplianceDocumentNumber(complianceNumberSequenceMock.Object, "999", "AB");
			AssertEquals("01b  AB0605904032019020119CUS1CUS2T01999", formatedNumber);
		}

		public void TestGetTotalLengthOfIncludedElements()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Indonesia);

			ComplianceSubTypeDependencyConfigurationCollection collection = new ComplianceSubTypeDependencyConfigurationCollection();
			ComplianceSubTypeDependencyConfiguration item = collection.AddNew();
			item.Country = "ID";
			item.ParentSubType = "T01";
			item.ChildSubType = "T02";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			var configurationCollection = new ComplianceNumberSequenceConfigurationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			var configuration1 = configurationCollection.AddNew();
			configuration1.Code = "AAA";
			configuration1.Description = "TestAAA";
			configuration1.Elements.Cast<ComplianceNumberSequenceCustomisationElement>().ForEach(x =>
			{
				if (x.ElementName != ElementNames.SeriesPrefix && x.ElementName != ElementNames.TaxStatusCode && x.ElementName != ElementNames.ComplianceSubType)
				{
					x.Include = true;
				}
			});
			configuration1.Elements[ElementNames.CustomElement1].DigitCode = "CUS1";
			configuration1.Elements[ElementNames.CustomElement2].DigitCode = "CUS2";
			configuration1.Elements[ElementNames.OriginalAmendmentStatus].DigitCode = "a/b";
			configuration1.Elements[ElementNames.TransactionType].DigitCode = "c/d/e";
			configuration1.Elements[ElementNames.BlankSpace].Length = 2;
			configuration1.Elements[ElementNames.ComplianceDateYearOfIssue].Length = 1;
			configuration1.Elements[ElementNames.InvoiceDateYearOfIssue].Length = 2;
			configuration1.Elements[ElementNames.PostDateYearOfIssue].Length = 4;
			AssertEquals(36, configuration1.GetTotalLengthOfIncludedElements(5, 4, "T01"));

			configuration1.Elements[ElementNames.SeriesPrefix].Include = true;
			AssertEquals(40, configuration1.GetTotalLengthOfIncludedElements(5, 4, "T01"));

			configuration1.Elements[ElementNames.SeriesPrefix].Include = false;
			configuration1.Elements[ElementNames.ComplianceSubType].Include = true;
			AssertEquals(39, configuration1.GetTotalLengthOfIncludedElements(5, 4, "T01"));

			configuration1.Elements[ElementNames.ComplianceSubType].Include = false;
			configuration1.Elements[ElementNames.TaxStatusCode].Include = true;
			AssertEquals(38, configuration1.GetTotalLengthOfIncludedElements(5, 4, "T01"));
		}

		void PreparedUsedConfiguration()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_SequenceClass = "TXC";
			sequence.XD_Prefix = "AA";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 102;
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_NextNumber = 1;
			sequence.XD_NumberFormat = "AAA";
			Factory.Save();

			BizObj.Code = "AAA";
			BizObj.Description = "Test";
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new ComplianceNumberSequenceConfiguration BizObj
		{
			get
			{
				var obj = (ComplianceNumberSequenceConfiguration)base.BizObj;
				obj.CurrentFallbackLevel = NewFallbackLevel();
				return obj;
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ComplianceNumberSequenceConfiguration();
		}

		#endregion
	}
}
