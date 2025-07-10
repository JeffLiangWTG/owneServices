using System.Collections;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class DocDataObjectReportSendingProvider
	{
		protected DocDataObjectReportSendingProvider(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		protected abstract string DataContext { get; }

		protected abstract ZGuid MenuItemPK { get; }

		public abstract ModuleIdentifier ModuleIdentifier { get; }

		public string MessageType => (NoResString)"Advanced Air Cargo Report"; // Event Reference

		public IStmMenuItem MenuItem => menuItem ?? (menuItem = factory.Load<VisualizerMenuItem>(MenuItemPK));
		IStmMenuItem menuItem;

		public IDocDataObjectMessageSender MessageSender
		{
			get
			{
				if (messageSender == null)
				{
					var processorDict = ObjectFactory.Get<Hashtable>("DocDataObjectMessageSendersProvider");
					messageSender = ((ObjectHandle)processorDict[DataContext])?.GetObject() as IDocDataObjectMessageSender;
				}
				return messageSender;
			}
		}
		IDocDataObjectMessageSender messageSender;

		public bool SendMessage(BusinessObject bizObj, INotifications notifications)
		{
			if (MessageSender == null)
			{
				notifications.AddMessageError(Res.GetString("10700C12-7381-4453-948C-A7337950A346", "Message Sender Provider can't be loaded with context: {0}", DataContext));
				return false;
			}

			if (bizObj == null)
			{
				notifications.AddMessageError(Res.GetString("a66aff9a-d33a-4977-9d36-b98984a4cf58", "Message can't be sent without providing the object for sending", DataContext));
				return false;
			}

			return MessageSender.SendMessage(bizObj, MenuItem, notifications);
		}
	}
}
