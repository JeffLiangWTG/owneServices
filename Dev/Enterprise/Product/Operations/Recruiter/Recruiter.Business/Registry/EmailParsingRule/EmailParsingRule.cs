using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class EmailParsingRule : AutoEmailParsingRule
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new EmailParsingRule();

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Recruiter.Business.EmailParsingRule|Description", Caption = "Referring Party")]
		public ZString ReferringPartyDescription => ReferringParties.GetDescriptionFromCode(ReferringPartyCode);

		EmailParsingRuleReferringParties ReferringParties => referringParties ?? (referringParties = new EmailParsingRuleReferringParties());
		EmailParsingRuleReferringParties referringParties;

		public override ZBool AllowParseAttachments
		{
			get => base.AllowParseAttachments;
			set
			{
				base.AllowParseAttachments = value;

				if (!AllowParseAttachments && AllowFallbackToEmailBody)
				{
					AllowFallbackToEmailBody = false;
				}
			}
		}

		public bool AllowFallbackToEmailBody_ReadOnly => !AllowParseAttachments;
	}
}

