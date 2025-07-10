using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ISF.ServiceTasks.Testing
{
	[TestedType(typeof(ImporterSecurityFilingXMLImporter))]
	sealed class ImporterSecurityFilingXMLImporterTest : ServiceTaskTestCase<ImporterSecurityFilingXMLImporter>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[TestDate(2009, 6, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatingViaTransactionNumber()
		{
			using (TempDirectory tempDirector = new TempDirectory())
			{
				AssertEquals(ImporterSecurityFilingXMLImporter.RegistryHasNotBeenConfigured, ImporterSecurityFilingXMLImporter.CheckRegistryHasBeenConfigured());
				ISFRegistry.Instance.ImporterSecurityFilingDataImportDirectory.SetValue(Guid.Empty, initialUserContext.Branch.PK, Guid.Empty, tempDirector.DirectoryName);
				var xmlFile = Path.Combine(tempDirector.DirectoryName, "UpdatingViaTransactionNumberImporterSecurityFiling.xml");
				var xmlFile2 = Path.Combine(tempDirector.DirectoryName, "ABCFirst.xml");
				File.Copy(BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\ISF\DataTransfer.Test\TestFiles\UpdatingViaTransactionNumberImporterSecurityFiling.xml", xmlFile);
				File.SetAttributes(xmlFile, FileAttributes.Normal);
				var xmlData = File.ReadAllText(xmlFile);
				Assert(xmlData.Contains("BM56846559"));
				xmlData.Replace("BM56846559", "BM36587794");
				File.WriteAllText(xmlFile2, xmlData);
				var noOfISFs = Factory.GetDatabaseCount(typeof(CusISFHeader));
				AssertEquals(string.Empty, ImporterSecurityFilingXMLImporter.CheckRegistryHasBeenConfigured());
				var header1 = Factory.New<CusISFHeader>();
				header1.BF_CustomsReference = "XJ5-20089367423";
				Factory.Save();
				AssertEquals(true, File.Exists(xmlFile));
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var importerTask = new ImporterSecurityFilingXMLImporter();
				InitialiseTaskSchedule(importerTask);
				using (new User.IsBatchProcessorOverride(Env.CurrentUser))
				{
					RunTaskSchedule(importerTask);
				}

				AssertEquals(false, File.Exists(xmlFile));
				AssertEquals(false, File.Exists(xmlFile2));
				var newFactory = new BusinessObjectFactory();
				AssertEquals("No new ISF created", noOfISFs + 1, newFactory.GetDatabaseCount(typeof(CusISFHeader)));
				var newHeader = newFactory.Load<CusISFHeader>(header1.PK);
				AssertNotNull(newHeader.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.HouseBillOfLading, "BM56846559"]);
				AssertNull(newHeader.ReferenceDatas[Common.US.ISF.BillTypeList.Codes.HouseBillOfLading, "BM36587794"]);
				AssertEquals(3, newHeader.Lines.Count);
				AssertEquals(ImporterCodeTypeList.Codes.IRS, newHeader.BF_ImporterCodeType);
				AssertEquals("91-013199000", newHeader.BF_ImporterCode);
				AssertEquals(SubmissionTypeList.Codes.ISF10, newHeader.BF_EntryType);
				AssertEquals(ShipmentTypeList.Codes.StandardOrRegularFilings, newHeader.BF_ShipmentType);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) =>
				{
					return emailToMatched.Subject.StartsWith("ISF XML File Import");
				}));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals(staffZ1.GS_EmailAddress, email.Recipients[0].Email);
				var expectedErrorsEmail = "Importing Importer Security Filing XML file: " + xmlFile + System.Environment.NewLine;
				Assert(email.Body.Contains(expectedErrorsEmail));
			}
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(ImporterSecurityFilingXMLImporter).GetMethod(nameof(ImporterSecurityFilingXMLImporter.CheckRegistryHasBeenConfigured));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			initialUserContext = Env.CurrentUserContext;
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "dong@pretend.email.com";
			Factory.Save();
			ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.SetValue(Guid.Empty, initialUserContext.Branch.PK, Guid.Empty, groupZZ1.PK.ToGuid());
		}

		GlbGroup groupZZ1;
		GlbStaff staffZ1;
		IUserContext initialUserContext;
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDownCore()
		{
			Env.SetUserContext(initialUserContext);
			base.TearDownCore();
		}
	}
}
