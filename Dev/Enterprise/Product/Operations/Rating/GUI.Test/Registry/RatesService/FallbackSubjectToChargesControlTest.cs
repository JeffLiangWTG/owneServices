using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(FallbackSubjectToChargesControl))]
	class FallbackSubjectToChargesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new FallbackSubjectToChargesCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((FallbackSubjectToChargesControl)control).IsControlOrBusinessEntityReadOnly;
		}
	}
}
