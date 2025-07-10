using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationLookups))]
	sealed class JobDeclarationLookupsBaseOnlyTest : JobDeclarationLookupsAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobDeclarationLookups GetLookups() => new JobDeclarationLookups(jobDeclaration);
	}
}
