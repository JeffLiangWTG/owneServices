using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{ 
	[TestedType(typeof(DeclarationCompletionFlag))]
	sealed class DeclarationCompletionFlagTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeclarationCompletionFlag();
		}

		public void Test_OtherFlagsAreFalse_When_OneFlagIsTrue()
		{
			var declarationCompletionFlag = (DeclarationCompletionFlag)GetNewBusinessObject();

			// Set IsExporter to true & Assert IsExporter is true and others are false
			declarationCompletionFlag.IsExporter = true;
			OptionallyAssertFlags(declarationCompletionFlag, true, false, false, false);

			// Set IsProducer to true & Assert IsProducer is true and others are false
			declarationCompletionFlag.IsProducer = true;
			OptionallyAssertFlags(declarationCompletionFlag, false, true, false, false);

			// Set IsAuthorizedOnBehalfExporter to true & Assert IsAuthorizedOnBehalfExporter is true and others are false
			declarationCompletionFlag.IsAuthorizedOnBehalfExporter = true;
			OptionallyAssertFlags(declarationCompletionFlag, false, false, true, false);

			// Set IsAuthorizedOnBehalfProducer to true & Assert IsAuthorizedOnBehalfProducer is true and others are false
			declarationCompletionFlag.IsAuthorizedOnBehalfProducer = true;
			OptionallyAssertFlags(declarationCompletionFlag, false, false, false, true);
		}

		void OptionallyAssertFlags(DeclarationCompletionFlag declarationCompletionFlag, bool expectedIsExporter, bool expectedIsProducer, bool expectedIsAuthorizedOnBehalfExporterr, bool expectedIsAuthorizedOnBehalfProducer)
		{
			AssertEquals(expectedIsExporter, declarationCompletionFlag.IsExporter);
			AssertEquals(expectedIsProducer, declarationCompletionFlag.IsProducer);
			AssertEquals(expectedIsAuthorizedOnBehalfExporterr, declarationCompletionFlag.IsAuthorizedOnBehalfExporter);
			AssertEquals(expectedIsAuthorizedOnBehalfProducer, declarationCompletionFlag.IsAuthorizedOnBehalfProducer);
		}
	}
}
