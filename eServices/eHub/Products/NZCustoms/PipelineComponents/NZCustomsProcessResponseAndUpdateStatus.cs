using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.NZCustoms.PipelineComponents
{
	[Guid("9595BE39-A95F-4203-AF9F-A4E7CFF26C93")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	public class NZCustomsProcessResponseAndUpdateStatus : ComponentBase, IComponent
	{
		#region Properties

		protected override Guid ClassID
		{
			get { return new Guid("9595be39-a95f-4203-af9f-a4e7cff26c93"); }
		}


		protected override string DisplayName
		{
			get { return "Process Response And Update Status"; }
		}

		#endregion

		#region Execute

		public IBaseMessage Execute(IPipelineContext context, IBaseMessage message)
		{
			var logger = LoggerHelpers.GetPipelineLogger(message);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				new PipelineHelpers().CreateSeekableMessageStream(context, message, logger);
				var xDoc = XDocument.Load(message.BodyPart.GetOriginalDataStream());
				if (logger.IsDebugEnabled) logger.Debug(xDoc.ToString());

				logger.Debug("Updating status");

				var isSuccess = Boolean.Parse(SafeGet(xDoc, "/*[local-name()='SendLodgementResponse']/*[local-name()='SendLodgementResult']/*[local-name()='IsSuccess']", "SendLodgementResponse/SendLodgementResult/IsSuccess"));
				var messageTrackingID = SafeGet(xDoc, "/*[local-name()='SendLodgementResponse']/*[local-name()='SendLodgementResult']/*[local-name()='MessageTrackingID']", "SendLodgementResponse/SendLodgementResult/MessageTrackingID");
				if (isSuccess)
				{
					GetOutboxAccessor().UpdateInboxMessageDistributionStatus(messageTrackingID.ToString());
					logger.Debug("Message is successful. Trackingid: " + messageTrackingID);
				}
				else
				{
					var errorMessage = Regex.Replace(SafeGet(xDoc, "/*[local-name()='SendLodgementResponse']/*[local-name()='SendLodgementResult']/*[local-name()='ErrorMessage']", "SendLodgementResponse/SendLodgementResult/ErrorMessage"), "(?<!\r)\n", "\r\n");
					GetExceptionsAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "NZC", "NZC", errorMessage, Guid.Empty, Guid.Empty, Guid.Parse(messageTrackingID), Guid.Empty, null);
					logger.Debug("Message is fail. Trackingid: " + messageTrackingID);
				}
				return null;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw ex;
			}
			finally
			{
				LoggerHelpers.LogComponentEnd(logger, this);
			}
		}

		private string SafeGet(XNode xdoc, string xpath, string fieldName)
		{
			var element = xdoc.XPathSelectElement(xpath);
			if (element == null || string.IsNullOrEmpty(element.Value))
			{
				throw new ArgumentException(fieldName + " is expected in SOAP response but could not be found!");
			}
			else
			{
				return element.Value;
			}
		}

		#endregion

		#region Methods

		public virtual IOutboxAccessor GetOutboxAccessor()
		{
			return DataAccessFactories.NewOutboxAccessorInstance();
		}

		public virtual IExceptionsAccessor GetExceptionsAccessor()
		{
			return DataAccessFactories.NewExceptionsAccessorInstance();
		}

		#endregion
	}
}
