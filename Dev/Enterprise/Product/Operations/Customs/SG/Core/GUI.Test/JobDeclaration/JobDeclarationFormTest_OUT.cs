using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationFormTest_OUT : JobDeclarationFormAbstractTest
	{
		public override ZString MessageTypeForFormBashing => MessageTypeCodeList.Codes.OUT;
	}
}
