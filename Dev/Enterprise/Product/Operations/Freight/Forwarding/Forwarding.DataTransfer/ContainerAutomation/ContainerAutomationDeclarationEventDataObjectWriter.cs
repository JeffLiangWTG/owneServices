using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ContainerAutomationDeclarationEventDataObjectWriter : ContainerAutomationConsolEventDataObjectWriter
	{
		public ContainerAutomationDeclarationEventDataObjectWriter(IDataWritingManager writeManager, ITransportParent parent)
			: base(writeManager, parent)
		{
		}

		protected override void PopulateDataObject(BaseStmALog logBO, Event logData)
		{
			base.PopulateDataObject(logBO, logData);

			if (Parent is IContainerParent containerParent)
			{
				_ = logData.AdditionalContextCollection ?? (logData.AdditionalContextCollection = new List<AdditionalContext>());
				logData.AdditionalContextCollection.AddRange(
					containerParent.Containers.Select(container =>
					{
						var containerDataContextManager = container.GetUniversalDataContextManager() as ContainerDataContextManager;
						if (containerDataContextManager == null)
						{
							return null;
						}
						containerDataContextManager.ShouldAddParentContextValues = false;

						return containerDataContextManager;
					})
					.Where(containerDataContextManager => containerDataContextManager != null)
					.Select(containerDataContextManager => new AdditionalContext
					{
						DataContext = DataContextFactory.New(containerDataContextManager, writeManager.Schema.Namespace),
						ContextCollection = GetContexts(containerDataContextManager.EventContextValues)
					}));
			}
		}

		static List<Context> GetContexts(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return contextValues
				?.Select(o =>
					new Context
					{
						Type = new ContextType { Type = o.Key.Type, Description = o.Key.Description },
						Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(o.Value)
					}).ToList()
				?? new List<Context>();
		}
	}
}
