using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusProcedureController))]
	public class ZZRefCusProcedureControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A#", "B$", "C%", "12A,12B", "Descr", "EXP");
			BusinessObject testObject = procedure;
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Universal.ZZRefCusProcedure;
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ZZRefCusProcedureController);
			}
		}
	}
}
