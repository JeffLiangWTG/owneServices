using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI.Testing
{
	abstract class ImportCustomsUserControlBasherAbstractTest : CustomsUserControlBasherAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		public override Type FormToBashType => typeof(JobDeclarationForm);
	}
}
