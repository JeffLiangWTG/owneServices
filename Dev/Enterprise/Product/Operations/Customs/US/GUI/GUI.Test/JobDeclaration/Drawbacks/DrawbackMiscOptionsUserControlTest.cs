using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class DrawbackMiscOptionsUserControlTest : CustomsUserControlBasherAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Drawback;

		protected override BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var result = (JobDeclaration)base.GetPopulatedDeclarationForFormBashing();
			result.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			return result;
		}

		protected override Type UserControlToBashType => typeof(DrawbackMiscOptionsUserControl);
	}
}
