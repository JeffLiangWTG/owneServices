using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(CMMFlagsControl))]
	internal class CMMFlagsControlTest : RegistryZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CMMFlags();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new CMMFlagsControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CMMFlags)businessEntity).ReadOnly;
		}
		#endregion
	}
}
