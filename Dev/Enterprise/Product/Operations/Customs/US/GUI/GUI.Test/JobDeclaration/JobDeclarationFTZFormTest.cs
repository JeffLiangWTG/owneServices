using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationFTZFormTest : JobDeclarationFormAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.FTZ;
	}
}
