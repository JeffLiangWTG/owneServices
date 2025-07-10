using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	public class ETerminalReleaseMessageSender : DocDataObjectMessageSender
	{
		protected override ZString MenuFilterDoesNotMatchMessageError => (NoResString)"The consol does not match eTerminal menu filter.";  // ErrorMessage

		protected override ZString MenuItemMessageError => (NoResString)"Can not load eTerminal correctly.";  // ErrorMessage

		protected override bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (!(bizObj is ForwardingConsol))
			{
				notifications.AddMessageError((NoResString)"Can not load consol correctly."); // ErrorMessage
				return false;
			}

			return true;
		}
	}
}
