using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMAWBDocumentSupporter))]
	public class CusMAWBDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			CusMAWB mawb = GetNewCusMAWB();
			CusMAWBDocumentSupporter documentSupporter = GetNewCusMAWBDocumentSupporter(mawb);
			AssertEquals("This shipment is not associated with a MAWB data.", documentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJob), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			CusMAWB mawb = GetNewCusMAWB();
			CusMAWBDocumentSupporter documentSupporter = GetNewCusMAWBDocumentSupporter(mawb);
			AssertEquals(true, documentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.GenericFreightJob, null));
			AssertEquals(false, documentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.None, null));
		}

		public void TestDotCusMAWBDataContext()
		{
			CusMAWB mawb = GetNewCusMAWB();
			CusMAWBDocumentSupporter documentSupporter = GetNewCusMAWBDocumentSupporter(mawb);
			DataContextValue cusMAWBBODataContext = new DataContextValue(".CusMAWB");
			AssertEquals("documentSupporter.IsDataContextSupported(CusMAWBBODataContext)", true, documentSupporter.IsDataContextSupported(cusMAWBBODataContext));
			IBODocDataProvider[] providers = documentSupporter.GetBODocDataProviders(cusMAWBBODataContext, null);
			AssertEquals("providers.Length", 1, providers.Length);
			AssertEquals("providers[0]", mawb, BODocDataProvider.GetBusinessObject(providers[0]));
		}

		public void TestDotCusHAWBDataContext()
		{
			CusMAWB mawb = GetNewCusMAWB();
			CusHAWB hawb1 = mawb.ChildBills.AddNew();
			CusHAWB hawb2 = mawb.ChildBills.AddNew();
			CusMAWBDocumentSupporter documentSupporter = GetNewCusMAWBDocumentSupporter(mawb);
			DataContextValue cusHAWBBODataContext = new DataContextValue(".CusHAWB");
			AssertEquals("documentSupporter.IsDataContextSupported(CusHAWBBODataContext)", true, documentSupporter.IsDataContextSupported(cusHAWBBODataContext));
			IBODocDataProvider[] providers = documentSupporter.GetBODocDataProviders(cusHAWBBODataContext, null);
			AssertEquals("providers.Length", 2, providers.Length);
			AssertEquals("providers[0]", hawb1, BODocDataProvider.GetBusinessObject(providers[0]));
			AssertEquals("providers[1]", hawb2, BODocDataProvider.GetBusinessObject(providers[1]));
		}

		#region Implementation
		protected virtual CusMAWBDocumentSupporter GetNewCusMAWBDocumentSupporter(CusMAWB mawb)
		{
			return new CusMAWBDocumentSupporter(mawb);
		}

		protected virtual CusMAWB GetNewCusMAWB()
		{
			return Factory.New<CusMAWB>();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return GetNewCusMAWB();
		}
		#endregion
	}
}
