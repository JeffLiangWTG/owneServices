using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Core.PipelineComponents
{
    /// <summary>
    /// Sets the internal tracking ID (if not already set in the context) using the BizTalk interchange ID.  Also, optionally,
    /// sets the sender tracking ID to the internal tracking ID if the pipeline is configured to do so. 
    /// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("9C077BC9-B0FF-47fc-81F4-75952844483D")]
	public class TrackingIDAssignmentComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
            get { return "Assigns an internal tracking ID + an optional sender tracking ID if not supplied in the message"; }
		}

		public string Name
		{
			get { return "Tracking ID Assignment"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
            Tracer.TraceStart(pipelineContext, message);
            try
            {
                Tracer.TraceInfo("AssignSenderTrackingID: {0}", this.AssignSenderTrackingID);
				Tracer.TraceInfo("AssignNewInternalTrackingID: {0}", this.AssignNewInternalTrackingID);

				// Set the internal tracking ID if it doesn't already exist
				string internalTrackingID = message.Context.ReadPropertyString<InternalTrackingID>();
                if (String.IsNullOrEmpty(internalTrackingID))
                {
					var orginalStream = message.BodyPart.GetOriginalDataStream();
					long originalPosition = orginalStream.Position;

					try
					{
						var xmlReader = new XmlTextReader(orginalStream);

						if (xmlReader != null)
						{
							var xpaths = new XPathCollection();
							xpaths.Add("/*[local-name()='TypedPolling' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectInboxMessagesByStatus']/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectInboxMessagesByStatus']/*[local-name()='TypedPollingResultSet0' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectInboxMessagesByStatus']/*[local-name()='InboxPK' and namespace-uri()='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SelectInboxMessagesByStatus']");
							var xPathReader = new XPathReader(xmlReader, xpaths);
							if (xPathReader.ReadUntilMatch())
							{
								xmlReader.Read();
								internalTrackingID = xmlReader.Value;
							}
						}
					}
					catch (XmlException) 
					{ 
						//If message content is not xml we can't read it but we don't want to terminate a message.
					}

					if (String.IsNullOrEmpty(internalTrackingID))
					{
						string interchangeID = message.Context.ReadPropertyString<BTS.InterchangeID>();
						internalTrackingID = AssignNewInternalTrackingID ? InternalTrackingID().ToString().ToUpper() : new Guid(interchangeID).ToString().ToUpper();
					}

					message.Context.WriteProperty<InternalTrackingID>(internalTrackingID);
					orginalStream.Position = originalPosition;
                }

                Tracer.TraceInfo("InternalTrackingID: {0}", internalTrackingID);

                // Set the sender tracking ID to the internal tracking ID
                if (AssignSenderTrackingID)
                {
                    message.Context.WriteProperty<MessageTrackingID>(internalTrackingID);
                }

                Tracer.TraceInfo("SenderTrackingID: {0}", message.Context.ReadPropertyString<MessageTrackingID>());
                Tracer.TraceEnd();

				if(LogMessageDetails)
				{
					LogMessageContentAndContextDetails(message);
				}

                return message;
            }
            catch (Exception ex)
            {
                Tracer.TraceError(ex);
                throw;
            }
		}

		void LogMessageContentAndContextDetails(IBaseMessage message)
		{
			var logger = LoggerHelpers.GetPipelineLogger(message, "TrackingIdAssignment");
			var originalStream = message.BodyPart.GetOriginalDataStream();
			long originalPosition = originalStream.Position;

			logger.Info("================================================= MESSAGE DETAILS =================================================");
			var msgBldr = new StringBuilder("------------------------------------------------- MESSAGE CONTEXT -------------------------------------------------").AppendLine().AppendLine();
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				string strName, strNamespace;
				object value = message.Context.ReadAt(i, out strName, out strNamespace);
				msgBldr.AppendFormat("{0}#{1} = {2}", strNamespace, strName, value).AppendLine();
			}
			logger.Info(msgBldr.ToString());
			logger.Info("----------------------------------------------- END MESSAGE CONTEXT -----------------------------------------------" + Environment.NewLine);

			var sr = new StreamReader(originalStream);
			logger.Info("----------------------------------------------- START MESSAGE DATA ------------------------------------------------"
				+ Environment.NewLine + Environment.NewLine + sr.ReadToEnd() + Environment.NewLine);
			logger.Info("------------------------------------------------ END MESSAGE DATA -------------------------------------------------");
			logger.Info("===================================================================================================================" + Environment.NewLine);

			originalStream.Position = originalPosition;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("9C077BC9-B0FF-47fc-81F4-75952844483D");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;

			try
			{
				propertyBag.Read("AssignSenderTrackingID", out var, errorLog);
			}
			catch { }
			if (var != null) AssignSenderTrackingID = (bool)var;

			try
			{
				propertyBag.Read("AssignNewInternalTrackingID", out var, errorLog);
			}
			catch { }
			if (var != null) AssignNewInternalTrackingID = (bool)var;

			try
			{
				propertyBag.Read("LogMessageDetails", out var, errorLog);
			}
			catch { }
			if (var != null) LogMessageDetails = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = AssignSenderTrackingID;
			propertyBag.Write("AssignSenderTrackingID", ref val);
			val = AssignNewInternalTrackingID;
			propertyBag.Write("AssignNewInternalTrackingID", ref val);
			val = LogMessageDetails;
			propertyBag.Write("LogMessageDetails", ref val);
		}

		#endregion

		#region Properties

        /// <summary>
        /// Should this instance generate a unique message ID on behalf of the sender?  Used when no envelope available to provide
        /// a context around the message
        /// </summary>
		public bool AssignSenderTrackingID { get; set; }

		/// <summary>
		/// New Internal Tracking ID
		/// </summary>
		public bool AssignNewInternalTrackingID { get; set; }

		/// <summary>
		/// Enabled logging of Message details
		/// </summary>
		public bool LogMessageDetails { get; set; }

		internal Func<Guid> InternalTrackingID = () => Guid.NewGuid();

		#endregion
	}
}
