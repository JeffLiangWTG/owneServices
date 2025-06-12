using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.CACustoms.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Encoder)]
	[Guid("54EF7CB3-41BA-442C-AFDB-38F14DD5EE0A")]
	public class CACInsertSubscription : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent

		public string Description
		{
			get { return "Insert Subscription For CA Customs Message"; }
		}

		public string Name
		{
			get { return "Insert Subscription For CA Customs Message"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		public IntPtr Icon { get; }

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			pInMsg.Context.Write("CorrelationToken", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "UseDeliveryNotificationUpdateStatus");
			if (!Enabled) return pInMsg;

			var messageContent = pInMsg.BodyPart.GetOriginalDataStream().ReadToEnd();
			var eHubSenderId = pInMsg.Context.ReadPropertyString<BTS.SourceParty>();
			var eHubRecipientId = pInMsg.Context.ReadPropertyString<BTS.DestinationParty>()?.Replace("MQ", string.Empty);

			if (isXmlContent)
			{
				var xmlDoc = XDocument.Parse(messageContent);
				var reference = xmlDoc.XPathSelectElement("/*[local-name()='DocumentMetaData']/*[local-name()='CommunicationMetaData']/*[local-name()='ApplicationReferenceID']")?.Value;
				var declarantId = xmlDoc.XPathSelectElement("/*[local-name()='DocumentMetaData']/*[local-name()='Declaration']/*[local-name()='Declarant']/*[local-name()='ID']")?.Value;
				var importerId = xmlDoc.XPathSelectElement("/*[local-name()='DocumentMetaData']/*[local-name()='Declaration']/*[local-name()='Importer']/*[local-name()='ID']")?.Value;

				if (string.IsNullOrEmpty(reference))
					throw new InvalidOperationException("Unable to find ApplicationReferenceID.");

				var emailSubject = pInMsg.Context.ReadPropertyString<CargoWise.eHub.Core.PropertySchemas.OverrideEmailSubject>();
				var accessor = GetDataModelAccessor();
				accessor.InsertSubscriptionValueWithRetries("CACMSG", eHubRecipientId, eHubSenderId, reference, null, emailSubject);
				if (!string.IsNullOrEmpty(declarantId) && declarantId.Length >= 9)
					accessor.InsertSubscriptionValueWithRetries("CACMSG", eHubRecipientId, eHubSenderId, declarantId.Substring(0, 9), "DeclarantID", emailSubject);
				if (!string.IsNullOrEmpty(importerId))
					accessor.InsertSubscriptionValueWithRetries("CACMSG", eHubRecipientId, eHubSenderId, importerId, "ImporterID", emailSubject);
			}
			else
			{
				if (messageContent.Contains("UNG+GOVCBR+"))
				{
					var uNBMatch = UNBRegex.Match(messageContent);
					var accessor = GetDataModelAccessor();
					if (uNBMatch.Success)
					{
						var uNBSplits = uNBMatch.Value.Split('+').ToList();
						if (uNBSplits.Count > 4)
						{
							var senderId = uNBSplits[2].Split(':')[0];
							var customsId = uNBSplits[3].Split(':')[0];

							var uNHUNTMatches = UNHUNTRegex.Matches(messageContent);
							foreach (Match match in uNHUNTMatches)
							{
								var msgBody = match.Value;
								var bgmElemMatch = BGMRegex.Match(msgBody);
								if (bgmElemMatch.Success)
								{
									var nodeBGMElems = bgmElemMatch.Value.Split('+');
									if (nodeBGMElems.Length >= 3)
									{

										var scribValue = customsId + "+" + senderId + "+" + nodeBGMElems[2];
										accessor.InsertSubscriptionValueWithRetries("IIDMSG", eHubRecipientId, eHubSenderId, scribValue, msgBody, "IIDMSG");
									}
								}
							}
						}
					}
				}
			}
			pInMsg.BodyPart.Data.SeekBegin();
			return pInMsg;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("54EF7CB3-41BA-442C-AFDB-38F14DD5EE0A");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object var = null;
			try { propertyBag.Read("Enabled", out var, errorLog); }
			catch { }
			if (var != null) Enabled = (bool)var;

			try { propertyBag.Read("IsXmlContent", out var, errorLog); }
			catch { }
			if (var != null) IsXmlContent = (bool)var;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);

			val = IsXmlContent;
			propertyBag.Write("IsXmlContent", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}
		bool enabled = true;

		public bool IsXmlContent
		{
			get { return isXmlContent; }
			set { isXmlContent = value; }

		}
		bool isXmlContent = false;
		#endregion

		static readonly Regex UNBRegex = new Regex(@"UNB\+{1}.*?'", RegexOptions.Compiled);
		static readonly Regex UNHUNTRegex = new Regex(@"UNH\+{1}.*?UNT\+{1}.*?'", RegexOptions.Compiled);
		static readonly Regex BGMRegex = new Regex(@"BGM\+{1}.*?'", RegexOptions.Compiled);

		internal virtual DataModelAccessor GetDataModelAccessor()
		{
			return new DataModelAccessor();
		}
	}
}