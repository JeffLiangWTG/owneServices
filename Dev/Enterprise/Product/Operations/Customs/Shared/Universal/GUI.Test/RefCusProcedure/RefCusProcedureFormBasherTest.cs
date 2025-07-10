using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCusProcedureForm))]
	class RefCusProcedureFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var tariff = factory.New<RefCusProcedure>();
			return new RefCusProcedureForm(tariff);
		}
	}
}
