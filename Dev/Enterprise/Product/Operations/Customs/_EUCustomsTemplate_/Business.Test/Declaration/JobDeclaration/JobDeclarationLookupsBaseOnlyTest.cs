using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationLookups))]
	class JobDeclarationLookupsBaseOnlyTest : JobDeclarationLookupsAbstractTest<JobDeclarationLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobDeclarationLookups GetLookups() => new JobDeclarationLookups(jobDeclaration);
	}
}
