using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthority : PortMessage, IPortAuthorityIssueListener
	{
		public PortAuthority(JobVoyage voyage)
			: base(voyage)
		{
		}

		const string ThirdPartySettingsKey = "Enterprise.Freight.Agency.GUI.PortAuthorityFilterDialog";

		#region Properties

		public override ZString Port
		{
			get
			{
				return base.Port;
			}
			set
			{
				base.Port = value;
				DefaultPrincipalPK();
			}
		}

		public override ZString Direction
		{
			get { return base.Direction; }
			set
			{
				base.Direction = value;

				ISailingEndPoint endPoint = TryGetEndPoint();

				if (endPoint == null)
				{
					MessageType = "";
				}
				else
				{
					EDIMessage message = endPoint.Messages.GetLastMessage(EDIMessage.ApplicationCodes.PortAuthority);

					DefaultPrincipalPK();

					if (message == null || message.EM_MessageSubType == PortMessageTypeList.Codes.Cancellation)
					{
						MessageType = PortMessageTypeList.Codes.Original;
					}
					else
					{
						MessageType = PortMessageTypeList.Codes.Replace;
					}
				}
			}
		}

		[List("Lookups.Version_List")]
		public override ZString Version
		{
			get { return base.Version; }
			set { base.Version = value; }
		}

		public override ZBool DeliverTo3rdParty
		{
			get { return base.DeliverTo3rdParty; }
			set
			{
				base.DeliverTo3rdParty = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePort();
					Validation.ValidateDirection();
				}
			}
		}

		[List("Lookups.LinkedPrincipals")]
		public override ZGuid PrincipalPK
		{
			get
			{
				return base.PrincipalPK;
			}
			set
			{
				base.PrincipalPK = value;
			}
		}

		void DefaultPrincipalPK()
		{
			if (Lookups.LinkedPrincipals.Count == 1)
			{
				PrincipalPK = Lookups.LinkedPrincipals[0].PK;
			}
		}

		#endregion

		#region Related Business Objects

		public PortAuthoritySetting Setting
		{
			get { return ((PortAuthorityLookups)Lookups).Settings_List.FindPortSetting(Port); }
		}

		#endregion

		protected override PortMessageLookups GetNewLookups()
		{
			return new PortAuthorityLookups(this);
		}

		public IPortAuthorityMessagingData ExtractData()
		{
			Issues.RemoveAndDeleteAll();

			return VoyageMessagingData.New(this, new BusinessObjectFactory());
		}

		public ISailingEndPoint GetEndPoint()
		{
			ISailingEndPoint endPoint = TryGetEndPoint() ?? throw new InvalidOperationException("Cant find the endPoint");

			return endPoint;
		}

		public ISailingEndPoint TryGetEndPoint()
		{
			switch (Direction)
			{
				case Constants.PortDirection.Load:
					return Voyage.Origins.GetOriginFromLoading(Port);
				case Constants.PortDirection.Discharge:
					return Voyage.Destinations.GetDestinationFromDischarge(Port);
				default:
					return null;
			}
		}

		public void Persist3rdPartySettings()
		{
			var writer = new StringWriter(CultureInfo.InvariantCulture);
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.WriteStartElement("PortAuthorityFilter");
				((IXmlSerializable)this).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				Env.Registry.SetFilterCriteria(ThirdPartySettingsKey, writer.ToString());
			}
		}

		public void Restore3rdPartySettings()
		{
			string xml = Env.Registry.GetFilterCriteria(ThirdPartySettingsKey);

			if (!string.IsNullOrEmpty(xml))
			{
				var reader = new StringReader(xml);
				using (var xmlReader = new XmlTextReader(reader))
				{
					xmlReader.MoveToContent();
					((IXmlSerializable)this).ReadXml(xmlReader);
				}
			}
		}

		#region Implementation

		protected override PortMessageValidation GetNewValidation()
		{
			if (DeliverTo3rdParty)
			{
				return new PortAuthority3rdPartyValidation(this);
			}
			else
			{
				return new PortAuthorityValidation(this);
			}
		}

		public PortAuthorityMessageFunction GetMessageFunction()
		{
			switch (MessageType)
			{
				case PortMessageTypeList.Codes.Cancellation:
					return PortAuthorityMessageFunction.Cancelation;
				case PortMessageTypeList.Codes.Replace:
					return PortAuthorityMessageFunction.Replace;
				case PortMessageTypeList.Codes.Original:
					return PortAuthorityMessageFunction.Original;
				default:
					throw new InvalidOperationException("Unknown Function: " + MessageType);
			}
		}

		#endregion

		#region IPortAuthorityIssueListener Members

		void IPortAuthorityIssueListener.Notify(ZGuid businessObjectPK, ZString tablePrefix, ZString errorText, ZString detail)
		{
			Issues.AddNew(businessObjectPK, tablePrefix, errorText, detail);
		}

		#endregion
	}
}



