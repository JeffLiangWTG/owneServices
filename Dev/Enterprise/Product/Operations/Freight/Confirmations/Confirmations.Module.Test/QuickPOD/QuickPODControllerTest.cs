using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	[TestedType(typeof(QuickPODController))]
	public class QuickPODControllerTest : ZControllerBasherTest
	{
		[ExpectNoExceptions]
		public void TestGetForm()
		{
			using (IZForm form = new QuickPODController().ShowNewForm())
			{ }
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new QuickPODs(Factory);
		}

		#region Overrides

		protected override Type GetBusinessObjectType()
		{
			return typeof(QuickPODs);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.QuickPOD;
		}

		#endregion
	}
}
