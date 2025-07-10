using Enterprise.Customs.SE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.SE.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
	}
}
