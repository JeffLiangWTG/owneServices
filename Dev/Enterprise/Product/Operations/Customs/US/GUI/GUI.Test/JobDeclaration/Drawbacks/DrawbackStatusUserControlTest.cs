using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class DrawbackStatusUserControlTest : CustomsUserControlBasherAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Drawback;

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var result = (JobDeclaration)base.GetPopulatedDeclarationForFormBashing();
			result.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			return result;
		}

		protected override Type UserControlToBashType => typeof(DrawbackStatusUserControl);
	}
}
