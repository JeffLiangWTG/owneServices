using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(ManifestToOpenHeader))]
	class ManifestToOpenHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			return manifest;
		}

		public void TestHasBillNumber_NoBills()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			AssertEquals("when manifest does not have a bill", false, manifestHeader.HasBillNumber);
		}

		public void TestHasBillNumber_EmptyBillNumber()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			manifestHeader.Bills.AddNew();
			AssertEquals("when bill number is empty", false, manifestHeader.HasBillNumber);
		}

		public void TestHasBillNumber_BillNumberEntered()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.TPD_DocumentNumber = "1";
			AssertEquals("when bill number is entered", true, manifestHeader.HasBillNumber);
		}

		public void TestClusterKey_ParentIsNull()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			AssertEquals("Parent is null", 0, manifestHeader.ClusterKey);
		}

		public void TestClusterKey_ParentIsJobDeclaration()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClusterKey = 1;
			manifestHeader.CE_ParentID = declaration.PK;
			manifestHeader.CE_ParentTable = declaration.TableName;
			AssertEquals("Parent is JobDeclaration", 1, manifestHeader.ClusterKey);
		}

		public void TestIsSea_ParentIsNull()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			AssertEquals("Parent is null", false, manifestHeader.IsSea);
		}

		public void TestIsSea_ParentIsJobDeclaration_Sea()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			manifestHeader.CE_ParentID = declaration.PK;
			manifestHeader.CE_ParentTable = declaration.TableName;
			AssertEquals("Parent is JobDeclaration and JE_TransportMode is SEA", true, manifestHeader.IsSea);
		}

		public void TestIsSea_ParentIsJobDeclaration_NotSea()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.CE_ParentID = declaration.PK;
			manifestHeader.CE_ParentTable = declaration.TableName;
			AssertEquals("Parent is JobDeclaration and JE_TransportMode is not SEA", false, manifestHeader.IsSea);
		}

		public void TestValidation()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			AssertType<ManifestToOpenHeaderValidation>(manifestHeader.Validation);
		}

		public void TestCE_EntryNum()
		{
			var manifestHeader = Factory.New<ManifestToOpenHeader>();
			AssertHasCustomAttribute<MaxLengthAttribute>(manifestHeader.GetType(), "CE_EntryNum", false, attr => attr.MaxLength == 20);
		}

		public void TestDefaultCE_ExpiryDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			var testDate = new ZDateTime(2021, 10, 07, 09, 15, 30);
			manifest.CE_IssueDate = testDate;
			AssertEquals("Should be CE_IssueDate + 45 days when declaration is SEA", testDate.AddDays(45), manifest.CE_ExpiryDate);

			manifest.CE_IssueDate = testDate.AddDays(10);
			AssertEquals("Should not default when CE_ExpiryDate is not empty", testDate.AddDays(45), manifest.CE_ExpiryDate);

			manifest.CE_ExpiryDate = ZDateTime.Empty;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			manifest.CE_IssueDate = testDate;
			AssertEquals("Should be CE_IssueDate + 20 days when declaration is not SEA", testDate.AddDays(20), manifest.CE_ExpiryDate);
		}

		public void TestCE_EntryNum_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenHeader), nameof(ManifestToOpenHeader.CE_EntryNum), false, x => x.ShortCaption == "G.Man.Reg.No" && x.Caption == "Global Manifest Registration No");
		}

		public void TestCE_IssueDate_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenHeader), nameof(ManifestToOpenHeader.CE_IssueDate), false, x => x.ShortCaption == "G.Man.Reg.Date" && x.Caption == "Global Manifest Registration Date");
		}

		public void TestCE_ExpiryDate_Caption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ManifestToOpenHeader), nameof(ManifestToOpenHeader.CE_ExpiryDate), false, x => x.ShortCaption == "G.Man.Exp.Date" && x.Caption == "Global Manifest Expiration Date");
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			manifest.Bills.AddNew();

			AssertEquals(1, manifest.Bills.Count);

			manifest.Delete();

			AssertEquals(0, manifest.Bills.Count);
		}
	}
}
