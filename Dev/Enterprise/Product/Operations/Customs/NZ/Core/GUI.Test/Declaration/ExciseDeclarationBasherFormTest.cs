using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	[TestedType(typeof(DeclarationBasherForm))]
	sealed class ExciseDeclarationBasherFormTest : DeclarationBasherFormTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
	}
}
