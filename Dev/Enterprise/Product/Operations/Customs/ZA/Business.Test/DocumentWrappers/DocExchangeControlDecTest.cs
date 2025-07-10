using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocExchangeControlDec))]
	sealed class DocExchangeControlDecTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUnformattedGoodsDescription()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();
			AssertEquals("Description is empty", ZString.Empty, exchangeControlDec.UnformattedGoodsDescription);

			declaration.JE_GoodsDescription = "Goods description";
			AssertEquals("Description is not empty", "Goods description", exchangeControlDec.UnformattedGoodsDescription);

			StmNote note = declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note.ST_Table = declaration.TableName;
			note.ST_ParentID = declaration.PK;
			note.ST_NoteDataAsText = "The detailed goods desription on the dec";
			AssertEquals("Description is detailed goods description", "The detailed goods desription on the dec", exchangeControlDec.UnformattedGoodsDescription);
		}

		public void TestUnformattedMarksAndNumbers()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();
			AssertEquals("UnformattedMarksAndNumbers is empty", ZString.Empty, exchangeControlDec.UnformattedMarksAndNumbers);

			StmNote note = declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_Table = declaration.TableName;
			note.ST_ParentID = declaration.PK;
			note.ST_NoteDataAsText = "Marks and numbers Line One\nLine Two";
			AssertEquals("UnformattedMarksAndNumbers is not empty", "Marks and numbers Line One\nLine Two", exchangeControlDec.UnformattedMarksAndNumbers);
		}

		public void TestPackageTypeDescription()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();
			declaration.JE_TotalNoOfPacksPackType = "";
			AssertEquals("PackageTypeDescription is empty", ZString.Empty, exchangeControlDec.PackageTypeDescription);

			declaration.JE_TotalNoOfPacksPackType = "PLT";
			AssertEquals("PackageTypeDescription is not empty", "Pallet", exchangeControlDec.PackageTypeDescription);
		}

		public void TestPackCount()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();
			AssertEquals("PackCount is empty", 0, exchangeControlDec.PackCount);

			declaration.JE_TotalNoOfPacks = 123;
			AssertEquals("PackCount is not empty", 123, exchangeControlDec.PackCount);
		}

		public void TestContainers()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "2";

			CusContainer container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "3";

			CusContainer container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "4";

			AssertEquals("Containers have four element", 4, exchangeControlDec.Containers.Count);
		}

		public void TestContainerSectionHeading()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();
			AssertEquals("Container section heading", "CONTAINER        SEAL                     TYPE         ", exchangeControlDec.ContainerSectionHeading);
		}

		public void TestShowContainerAdditionalDetails()
		{
			DocExchangeControlDec exchangeControlDec = (DocExchangeControlDec)GetNewBusinessObject();
			AssertEquals("ShowContainerAdditionalDetails", ZBool.False, exchangeControlDec.ShowContainerAdditionalDetails);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();

			DocJobComInvoiceHeader invoiceHeaderWrapper = DocJobComInvoiceHeader.New(invoiceHeader, Factory);
			return new DocExchangeControlDec(invoiceHeaderWrapper);
		}

		ZString storedCountry;

		JobDeclaration declaration;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.SouthAfrica);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}

		#endregion
	}
}
