using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.eServices.Encryption.Client.Encryptor;
using Enterprise.Telematics.Business.Registry;
using WTG.Telematics.Common;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	class DataSender : IDataSender
	{
		public DataSender(IEHubMessageSender eHubMessageSender)
		{
			this.eHubMessageSender = eHubMessageSender ?? throw new ArgumentNullException(nameof(eHubMessageSender));
		}

		public void Send(BusinessObjectFactory factory, IEnumerable<IPortionedData> data)
		{
			_ = data ?? throw new ArgumentNullException(nameof(data));
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var messages = data.Select(portion => XmlDataSerializer.SerializeToTelematicsXmlData(
				new TelematicsXtRimMessage
				{
					TcaBatchId = portion.BatchId,
					TcaAddress = TelematicsConfigurationRegistry.Instance.TcaRimUrl.Value,
					TcaUsername = TelematicsConfigurationRegistry.Instance.TcaRimUsername.Value,
					TcaPassword = EhubClientEncryptor.Encrypt(TelematicsConfigurationRegistry.Instance.TcaRimPassword.Value),
					JsonData = portion.Message,
				}));

			eHubMessageSender.Send(
				factory,
				messages,
				new[] { TelematicsConfigurationRegistry.Instance.XtRecipient.Value });
		}

		readonly IEHubMessageSender eHubMessageSender;
	}
}
