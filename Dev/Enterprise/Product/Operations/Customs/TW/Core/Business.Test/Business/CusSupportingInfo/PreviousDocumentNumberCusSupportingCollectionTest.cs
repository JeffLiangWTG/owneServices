using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PreviousDocumentNumberCusSupportingCollection))]
	sealed class PreviousDocumentNumberCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousDocumentNumberCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PreviousDocumentNumberCusSupporting> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return new PreviousDocumentNumberCusSupportingCollection(messageHeader);
		}
	}
}
