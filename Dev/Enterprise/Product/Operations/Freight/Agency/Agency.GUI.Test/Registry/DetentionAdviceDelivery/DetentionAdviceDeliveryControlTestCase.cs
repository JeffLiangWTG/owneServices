using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(DetentionAdviceDeliveryControl))]
	internal class DetentionAdviceDeliveryControlTestCase : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DetentionAdviceDelivery(Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((BusinessObject)businessEntity).ReadOnly;
		}
		#endregion
	}
}
