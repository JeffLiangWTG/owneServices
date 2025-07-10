using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UpdateUENReferences))]
	sealed class UpdateUENReferencesTest : DataLoadTestCase<UpdateUENReferences>
	{
		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			DirectoryInfo directory = new DirectoryInfo(System.Environment.CurrentDirectory);
			string dummyFilePath = directory.FullName + "\\non-existant file";
			Loader.UpdateUENReferenceData(dummyFilePath);
		}

		public void TestValidationOfContent_GarbageHeader()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine("This is the header for the file.");
					sw.WriteLine("This is the first part - invalid text");
					sw.Flush();
				}
				Loader.UpdateUENReferenceData(testFileName.Filename);
				AssertEquals(2, Loader.RunCounters.RecordsToImportPlusHeader);
				AssertEquals(0, Loader.RunCounters.RecsCreated);
				AssertEquals(0, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);
				AssertEquals(3, Loader.Log.Count);
				AssertEquals("FileHeaderIsValid", false, Loader.FileHeaderIsValid);
			}
		}

		public void TestImportUENReferences()
		{
			using (TempFile testFileName = TempFile.New())
			{
				using (StreamWriter sw = new StreamWriter(testFileName.Filename))
				{
					sw.WriteLine(String.Format("Previous Entity Registration No Mappings"));
					sw.WriteLine(String.Format("Previous Entity Registration No,Entity Name,UEN,UEN Status,Issuance Agency,Entity Type,Registered Address Street Name,Remarks"));
					sw.WriteLine(String.Format("10002870000D,FORESPAND FOOD ENTERPRISE PTE LTD,198700002E,R,ACRA,LC,TOH GUAN ROAD EAST,record found"));
					sw.Flush();
				}

				OrgHeader forespand = Factory.NewWithValidTestData<OrgHeader>();
				OrgCusCode cRN = forespand.CustomsCodes.AddNew();
				cRN.OK_CodeType = OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber;
				cRN.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				cRN.OK_CustomsRegNo = "10002870000D";

				Factory.Save();

				Loader.UpdateUENReferenceData(testFileName.Filename);
				AssertEquals(1, Loader.RunCounters.RecsUpdated);
				AssertEquals(0, Loader.RunCounters.RecsExcluded);

				ZQuery uENCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber);
				uENCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, cRN.OK_RN_NKCodeCountry);
				uENCodeFilter.AddToFilter(OrgCusCodeSchema.OK_OH, forespand.PK);
				uENCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, "198700002E");
				OrgCusCode uENOrgCusCode = Factory.LoadTop1<OrgCusCode>(uENCodeFilter);
				AssertNotNull("UEN reference should have been added to this organisation", uENOrgCusCode);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Singapore);
			Loader = new UpdateUENReferences();
		}

		protected override UpdateUENReferences GetNewDataLoader()
		{
			return new UpdateUENReferences();
		}

		UpdateUENReferences Loader;

		#endregion
	}
}
