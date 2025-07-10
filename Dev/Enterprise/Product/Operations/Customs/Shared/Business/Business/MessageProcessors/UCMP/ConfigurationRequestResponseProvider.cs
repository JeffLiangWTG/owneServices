using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public class ConfigurationRequestResponseProvider : IRequestResponseProvider<ConfigurationRequest, ConfigurationMessageResponse>
	{
		public ConfigurationRequest GetRequestMessage(EDIInterchange outgoingInterchange)
		{
			using var reader = new StringReader(outgoingInterchange.EI_BodyText);
			return new ConfigurationRequest(reader.DeserializeToConfiguration());
		}

		public ConfigurationMessageResponse ParseResponseMessage(EDIMessage message)
		{
			var xDocument = XDocument.Parse(message.EM_MessageText);
			XDocument successfulXML = null;
			UniversalEventWrapper errorUniversalEventWrapper = null;
			if (xDocument.Root.Name.LocalName.Equals(Constant.Configuration, StringComparison.InvariantCultureIgnoreCase))
			{
				successfulXML = xDocument;
			}
			else
			{
				var universalEventElement = xDocument.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals(Constant.UniversalEvent, StringComparison.InvariantCultureIgnoreCase));
				if (universalEventElement != null)
				{
					errorUniversalEventWrapper = new UniversalEventWrapper(universalEventElement.ToString());
				}
			}
			return new ConfigurationMessageResponse(successfulXML, errorUniversalEventWrapper);
		}

		public object GetLinkedObjectFromRequest(EDIInterchange outgoingInterchange)
		{
			var configuration = GetRequestMessage(outgoingInterchange).Request;
			var factory = outgoingInterchange.Factory;

			var companyCode = configuration.Group.Cast<Group>()
						.SelectMany(g => g.Items.Cast<Group>())
						.FirstOrDefault(i => i.Type == Constants.GroupTypes.CompanyType)?.Reference;

			if (!string.IsNullOrEmpty(companyCode))
			{
				return factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, companyCode));
			}
			return null;
		}
	}

	public class ConfigurationRequest : ICFGMessageRequest
	{
		public ConfigurationRequest(Configuration request)
		{
			Request = request;
		}
		public Configuration Request;
	}

	public class ConfigurationMessageResponse : ICFGMessageResponse
	{
		public ConfigurationMessageResponse(XDocument successfulXML, UniversalEventWrapper errorUniversalEventWrapper)
		{
			if ((successfulXML != null && errorUniversalEventWrapper != null) || (successfulXML == null && errorUniversalEventWrapper == null))
			{
				throw new ArgumentException("The response Configuration Message should be either successful XML or error UniversalEventWrapper");
			}
			this.successfulXML = successfulXML;
			this.errorUniversalEventWrapper = errorUniversalEventWrapper;
		}
		readonly XDocument successfulXML;
		readonly UniversalEventWrapper errorUniversalEventWrapper;

		public XDocument SuccessfulXML { get => successfulXML; }

		public UniversalEventWrapper ErrorUniversalEventWrapper { get => errorUniversalEventWrapper; }

		public bool IsSuccessful => SuccessfulXML != null;

		public string ErrorReason => ErrorUniversalEventWrapper?.Reason;
	}
}
