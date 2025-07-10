using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CMCertificateOfOriginCusSupportingCollection))]
	sealed class CMCertificateOfOriginCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CMCertificateOfOriginCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<CMCertificateOfOriginCusSupporting> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			return new CMCertificateOfOriginCusSupportingCollection(messageHeader);
		}
	}
}
