using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Registry
{
	[TestedType(typeof(DateAndReferenceControl))]
	class DateAndReferenceControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DateAndReferenceCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DateAndReferenceControl)control).DateAndReferenceGrid.ReadOnly;
		}
	}
}
