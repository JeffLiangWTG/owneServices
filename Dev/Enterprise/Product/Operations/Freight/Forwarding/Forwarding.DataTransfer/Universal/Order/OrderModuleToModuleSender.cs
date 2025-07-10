using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderModuleToModuleSender : ModuleToModuleSender<Order>
	{
		protected override DataContextType EntityTypeToLoad
		{
			get { return DataContextType.WarehouseReceive; }
		}

		protected override ZString ErrorPrefix
		{
			get { return Res.GetString("671a56ea-3191-4f26-bf4e-7726a95d41a1", "Failed to create Warehouse Receive:"); }
		}

		protected override UniversalEvent[] GetUniversalEvents(Order parentEntity)
		{
			var factory = new BusinessObjectFactory();
			UniversalEvent[] events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, parentEntity.Warehouse, new[] { RecipientRoleType.WIN }, parentEntity).ToArray();
				factory.Save();
			}
			return events;
		}
	}
}
