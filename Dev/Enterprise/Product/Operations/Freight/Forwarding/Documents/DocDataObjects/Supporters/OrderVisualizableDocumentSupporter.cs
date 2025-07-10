using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class OrderVisualizableDocumentSupporter : ForwardingVisualizableDocumentSupporter<Order>
	{
		public OrderVisualizableDocumentSupporter(Order order)
			: base(order)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainConsolCustomiseForms;

		#region GetMessageEventsProcessor

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		#endregion

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters) => documentName;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		public override Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => new Either<string, object>((object)null);

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext)
		{
			switch (dataContext)
			{
				case DataContext.UXML:
					yield return new FreightLibrary();
					yield return new ForwardingLibrary();
					break;
			}
		}

		public override IEnumerable<ICommand> GetCustomCommands(string dataContext) => Enumerable.Empty<ICommand>();
	}
}
