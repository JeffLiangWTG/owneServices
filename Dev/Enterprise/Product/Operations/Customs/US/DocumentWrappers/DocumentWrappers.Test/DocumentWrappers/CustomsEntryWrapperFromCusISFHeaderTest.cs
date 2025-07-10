using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperFromCusISFHeader))]
	sealed class CustomsEntryWrapperFromCusISFHeaderTest : CustomsEntryWrapperTest
	{
		public override void TestWrapperMappingFull()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "XJ54345678";
			header.BF_FirstAcceptedDate = new ZDateTime(2009, 2, 2);
			header.BF_HouseBill = "Info";
			CustomsEntryWrapper wrapperFull = new CustomsEntryWrapperFromCusISFHeader(header, Factory);
			AssertEquals("wrapperFull.ToString()", "XJ54345678", wrapperFull.ToString());
			AssertEquals("wrapperFull.EntryNumber", "XJ54345678", wrapperFull.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "ISF", wrapperFull.EntryType.Code);
			AssertEquals("wrapperFull.EntryType.CodeAndDescription", "ISF - Importer Security Filing", wrapperFull.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", ZString.Empty, wrapperFull.EntryCategory);
			AssertEquals("wrapperFull.Information", "Info", wrapperFull.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 2, 2), wrapperFull.IssueDate);
		}

		public override void TestWrapperMappingsEmpty()
		{
			CustomsEntryWrapper wrapperEmpty = (CustomsEntryWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.EntryNumber", ZString.Empty, wrapperEmpty.EntryNumber);
			AssertEquals("wrapperEmpty.EntryType.Code", "ISF", wrapperEmpty.EntryType.Code);
			AssertEquals("wrapperEmpty.EntryType.CodeAndDescription", "ISF - Importer Security Filing", wrapperEmpty.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", ZString.Empty, wrapperEmpty.EntryCategory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
EntryType : ISF - Importer Security Filing
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			return new CustomsEntryWrapperFromCusISFHeader(header, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CustomsEntryWrapperFromCusISFHeader(null, Factory);
		}
	}
}
