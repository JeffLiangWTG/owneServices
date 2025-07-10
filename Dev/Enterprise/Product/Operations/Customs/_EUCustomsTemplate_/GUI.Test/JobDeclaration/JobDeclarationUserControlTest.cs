using Enterprise.Customs._EUCustomsTemplate_.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
	}
}
