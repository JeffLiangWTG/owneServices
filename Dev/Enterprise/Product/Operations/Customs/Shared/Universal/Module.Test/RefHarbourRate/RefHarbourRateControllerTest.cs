using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefHarbourRateController))]
	public class RefHarbourRateControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.Universal.RefHarbourRate;

		public override Type ControllerToBashType => typeof(RefHarbourRateController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var record = helper.CreateHarbourRate("IMP", "123", RefHarbourRateModeList.Codes.CON, "", new ZDate(2024, 5, 15), new ZDate(2024, 5, 23), "[ABC]*2", "FR", "");
			BusinessObject testObject = record;
			Factory.Save();
			return testObject;
		}
	}
}
