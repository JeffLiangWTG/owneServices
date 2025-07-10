using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer
{
	public class PortMessagingImporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System generated file name format.")]
		public IEnumerable<INotification> Import(BusinessObject obj, PortMessagingManager.MessageType messageType, string purpose)
		{
			Argument.NotNull(obj, "obj");
			Argument.NotNull(purpose, "purpose");

			var datawriter = GetDataWriter(obj, messageType, purpose);
			string filename = string.Format(CultureInfo.InvariantCulture,
											"{0}_{1:yyyyMMddHHmmssZ}.txt"
											, GetJobNumber(obj)
											, ZDateTime.UtcNow);
			var processor = ObjectFactory.New<IUniversalShipmentXmlWriter>(datawriter, obj, PortMessagingManager.DakosyRecipientCode, filename);
			var logger = new NotificationCollection();

			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(logger, replaceThisTokenEventuallyQuestionMarkExclamationMark);

			return logger;
		}

		string GetJobNumber(BusinessObject obj)
		{
			return (obj as ForwardingConsol)?.JK_UniqueConsignRef ?? (obj as ForwardingShipment)?.JobNumber ?? string.Empty;
		}

		ITopLevelDataObjectWriter GetDataWriter(BusinessObject obj, PortMessagingManager.MessageType messageType, string purpose)
		{
			if (obj is ForwardingConsol)
			{
				return new PortMessagingConsolDataObjectWriter(new DataWritingManager(new ActionInfo(null, obj) { PurposeCode = purpose }), messageType, purpose);
			}
			if (obj is ForwardingShipment)
			{
				return new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, obj) { PurposeCode = purpose }), true, true, messageType, purpose);
			}

			throw new NotSupportedException("Importer supports only Shipment and Consol");
		}
	}
}
