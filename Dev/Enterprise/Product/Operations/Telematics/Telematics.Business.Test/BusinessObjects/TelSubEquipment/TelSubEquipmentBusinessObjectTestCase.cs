using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelSubEquipment))]
	class TelSubEquipmentBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var telSubEquipment = (TelSubEquipment)base.GetNewBusinessObjectForDeleteTest(factory);
			telSubEquipment.TSE_Type = "A";
			telSubEquipment.TSE_Configuration = "<EmptyXml />";
			return telSubEquipment;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var telSubEquipment = (TelSubEquipment)base.GetBusinessObjectForFetchForLoad();
			telSubEquipment.TSE_Type = "A";
			telSubEquipment.TSE_Configuration = "<EmptyXml />";
			return telSubEquipment;
		}
	}
}
