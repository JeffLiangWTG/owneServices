using System;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	public sealed class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(JobDeclarationController);
			}
		}

		protected override bool CountryHasExWarehouse => false;
	}
}
