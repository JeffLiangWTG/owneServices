using System;
using System.Linq;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(TWDeclarationOperationalActionMethodProvider))]
	sealed class TWDeclarationOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			var provider = new TWDeclarationOperationalActionMethodProvider();
			AssertContainsExactElementsInAnyOrder(new Type[]
			{
				typeof(DeclarationMessageOperationalActionMethod),
				typeof(ExportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod),
				typeof(ExportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod),
				typeof(ExportCustomsDeclarationProofBatchDocumentsOperationalActionMethod),
				typeof(ExportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod),
				typeof(ImportCustomsDeclarationInformalBatchDocumentsOperationalActionMethod),
				typeof(ImportCustomsDeclarationFormalBatchDocumentsOperationalActionMethod),
				typeof(ImportCustomsDeclarationProofBatchDocumentsOperationalActionMethod),
				typeof(ImportCustomsDeclarationEnglishBatchDocumentsOperationalActionMethod)
			}, provider.NewMethods(null).Select(x => x.GetType()));
		}

		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.TWJobDeclaration;
	}
}
