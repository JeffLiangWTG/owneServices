using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public sealed class DraftBOLLinkedProcessor : LinkedDocumentMessageProcessor, IDraftBOLLinkedProcessor
	{
		protected override bool ShouldProcess(IEDIMessage message) => message
			?.EM_MessageText
			.Contains((NoResString)"<DocumentName>Draft Bill Of Lading</DocumentName>") // programmatic constant
			?? false;

		protected override ZGuid MenuItemPK => new ZGuid("7b075444-49bc-49a9-8af7-04d935a0892a"); // Draft of Lading system defined menu item

		protected override ZInt MenuItemDocumentCount => 1;

		#region DraftBOLLinkedProcessor members

		void IDraftBOLLinkedProcessor.Process(IBusiness bizObj, IStmALog log) => Process(bizObj, log);

		#endregion
	}
}
