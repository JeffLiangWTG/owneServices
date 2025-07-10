using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingListDocumentSupporter))]
	public class CusPackingListDocumentSupporterTest : DocumentSupporterTest
	{
		public virtual void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be ", BusinessContext.CusPackingList, documentSupporter.BusinessContext);
		}

		public virtual void TestSupportedDataContexts()
		{
			AssertEquals("Enterprise.Core.Constants.DataContext.CusPackingList is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusPackingList)));
		}

		protected virtual ZString GetMainNameSpace()
		{
			return "Enterprise.DocumentWrappers.Customs." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + ".";
		}

		public virtual void TestGetBODocDataProviders()
		{
			GlbCompany.CurrentCompany.SetCountry(TestCountryCode);
			var mainNameSpace = GetMainNameSpace();

			var dataContextValue = new DataContextValueForTesting(Core.Constants.DataContext.CusPackingList);
			var bODocDataProviders = documentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("Document wrapper for data context of CusPackingList is of type DocCusPackingList", "Enterprise.Customs.TW.Business.DocCusPackingList", bODocDataProviders[0].GetType().ToString());
		}

		public virtual void TestShowReasonForNotPrinting()
		{
			AssertEquals(true, documentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.CusPackingList, null));
		}

		#region Implementation

		protected override ZString TestCountryCode
		{
			get { return Core.Constants.CountryCodes.Taiwan; }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			return declaration.LoadOrCreateCusPackingList(Factory);
		}

		protected virtual CusPackingListDocumentSupporter GetDocumentSupporterToTest() => new CusPackingListDocumentSupporter(Factory.New<CusPackingList>());

		protected override void SetUp()
		{
			base.SetUp();
			documentSupporter = GetDocumentSupporterToTest();
		}
		#endregion

		protected CusPackingListDocumentSupporter documentSupporter;
	}
}
