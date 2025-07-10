using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public class ACASHouseChecklistMessageSender : DocDataObjectMessageSender
	{
		protected override ZString MenuFilterDoesNotMatchMessageError => (NoResString)"The consol is not arriving in US.";  // ErrorMessage

		protected override ZString MenuItemMessageError => (NoResString)"Can not load ACAS House Checklist correctly.";  // ErrorMessage

		protected override bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (bizObj is ForwardingConsol consol)
			{
				return true;
			}

			notifications.AddMessageError((NoResString)"Can not load Consol correctly."); // ErrorMessage
			return false;
		}
	}
}
