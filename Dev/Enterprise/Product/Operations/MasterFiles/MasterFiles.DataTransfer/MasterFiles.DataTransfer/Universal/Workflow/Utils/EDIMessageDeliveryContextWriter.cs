using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.DataTransfer
{
	public static class EDIMessageDeliveryContextWriter
	{
		public static void PopulateMessageDeliveryContextValues(IUniversalActionInfo action, List<Context> contextCollection)
		{
			if (action is IEDIMessageDeliveryContextProvider provider)
			{
				var applicator = ObjectFactory.Get<IEDIMessageDeliveryContextEvaluator>();
				var contextValues = applicator.GetValues(action.FactoryForProcessing, provider, action.Notifications);
				foreach (var value in contextValues)
				{
					contextCollection.Add(new Context()
					{
						Type = new ContextType() { Type = value.ContextType, Description = value.Description.IsEmpty ? null : value.Description },
						Value = value.ContextValue
					});
				}
			}
		}
	}
}
