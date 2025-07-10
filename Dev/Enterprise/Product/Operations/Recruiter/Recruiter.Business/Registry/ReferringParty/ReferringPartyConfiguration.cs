using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class ReferringPartyConfiguration : RegistryBusinessObject
	{
		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Domain = "Domain";
			public const string ReferringParty = "ReferringParty";
			public const string OrganizationPK = "OrganizationPK";
			public const string DefaultReferringSource = "DefaultReferringSource";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReferringPartyConfiguration();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var cfg = (ReferringPartyConfiguration)clone;
			cfg.Domain = Domain;
			cfg.ReferringParty = ReferringParty;
			cfg.OrganizationPK = OrganizationPK;
			cfg.DefaultReferringSource = DefaultReferringSource;
		}

		protected override bool IsCodeMandatory => false;

		#region Domain

		[ResourceStringData("NPBO:Enterprise.Recruiter.Business.ReferringPartyConfiguration|Domain", Caption = "Email / Domain")]
		[MaxLength(256)]
		public ZString Domain
		{
			get { return domain; }
			set
			{
				CheckMaximumLength(DomainInfo, value);
				SetNonPersistentPropertyValue(DomainInfo, ref domain, value);
				if (!IsValidationSuspended)
				{
					ValidateDomain();
				}
			}
		}

		ZString domain;

		public ZPropertyInfo DomainInfo => GetZPropertyInfo(Schema.Domain);

		#endregion Domain

		#region ReferringParty

		[List("ReferringParties")]
		[MaxLength(3)]
		public ZString ReferringParty
		{
			get { return referringParty; }
			set
			{
				CheckMaximumLength(ReferringPartyInfo, value);
				SetNonPersistentPropertyValue(ReferringPartyInfo, ref referringParty, value);

				if (OrganizationPK_ReadOnly)
				{
					OrganizationPK = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					ValidateReferringParty();
				}
			}
		}

		ZString referringParty;

		public ZPropertyInfo ReferringPartyInfo => GetZPropertyInfo(Schema.ReferringParty);

		#endregion ReferringParty

		#region ReferringPartyDescription

		[ResourceStringData("NPBO:Enterprise.Recruiter.Business.ReferringPartyConfiguration|ReferringPartyDescription", Caption = "Referring Party")]
		[List("ReferringParties")]
		public ZString ReferringPartyDescription
		{
			get { return referringPartyDescription ?? ReferringParties.GetDescriptionFromCode(ReferringParty); }
			set
			{
				CheckMaximumLength(ReferringPartyDescriptionInfo, value);
				referringPartyDescription = value;
				if (!IsValidationSuspended)
				{
					ValidateReferringPartyDescription();
				}

				if (!ReferringPartyDescriptionInfo.HasErrors())
				{
					ReferringParty = ReferringParties.GetCodeFromDescription(value);
				}
				ReferringPartyDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReferringPartyDescriptionInfo => GetZPropertyInfo(nameof(ReferringPartyDescription));

		ZString? referringPartyDescription;

		#endregion ReferringPartyDescription

		#region OrganizationPK

		[ResourceStringData("NPBO:Enterprise.Recruiter.Business.ReferringPartyConfiguration|OrganizationPK", Caption = "Organization")]
		[List("Organizations")]
		public ZGuid OrganizationPK
		{
			get { return organizationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganizationPKInfo, ref organizationPK, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganizationPK();
				}
			}
		}

		ZGuid organizationPK;

		public ZPropertyInfo OrganizationPKInfo => GetZPropertyInfo(Schema.OrganizationPK);

		public bool OrganizationPK_ReadOnly => ReferringParty != OrgHeaderSchema.Constants.Prefix;

		#endregion OrganizationPK

		#region DefaultReferringSource

		[List("ReferringSources")]
		[MaxLength(3)]
		public ZString DefaultReferringSource
		{
			get { return defaultReferringSource; }
			set
			{
				CheckMaximumLength(DefaultReferringSourceInfo, value);
				SetNonPersistentPropertyValue(DefaultReferringSourceInfo, ref defaultReferringSource, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultReferringSource();
				}
			}
		}

		ZString defaultReferringSource;

		public ZPropertyInfo DefaultReferringSourceInfo => GetZPropertyInfo(Schema.DefaultReferringSource);

		#endregion DefaultReferringSource

		#region Lists

		public CodeDescriptionPairList ReferringSources
		{
			get
			{
				if (referringSources == null)
				{
					referringSources = new CodeDescriptionPairList();
					foreach (CodeDescriptionBool item in RecruiterDataRegistry.Instance.ReferringSourcesTypes.Value)
					{
						if (item.Bool)
						{
							referringSources.AddPair(item.Code, item.Description);
						}
					}
				}

				return referringSources;
			}
		}

		CodeDescriptionPairList referringSources;

		public CodeDescriptionPairList AllReferringSources
		{
			get
			{
				if (allReferringSources == null)
				{
					allReferringSources = new CodeDescriptionPairList();
					foreach (CodeDescriptionBool item in RecruiterDataRegistry.Instance.ReferringSourcesTypes.Value)
					{
						allReferringSources.AddPair(item.Code, item.Description);
					}
				}

				return allReferringSources;
			}
		}

		CodeDescriptionPairList allReferringSources;

		public CodeDescriptionPairList ReferringParties
		{
			get
			{
				if (referringParties == null)
				{
					referringParties = new CodeDescriptionPairList();
					referringParties.AddPair(GlbStaffSchema.Constants.Prefix, Res.GetString("e8ecd35b-6655-4bc5-a96c-01efedb7dfa8", "Staff"));
					referringParties.AddPair(OrgHeaderSchema.Constants.Prefix, Res.GetString("540d3d61-f490-488c-bfbc-76c9dc26af03", "Organization"));
				}

				return referringParties;
			}
		}

		CodeDescriptionPairList referringParties;

		public OrgHeaderCollection Organizations => fOrganizations ?? (fOrganizations = new OrgHeaderCollection(Factory ?? new BusinessObjectFactory()));

		OrgHeaderCollection fOrganizations;

		#endregion Lists

		#region Validations

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDomain();
			ValidateReferringParty();
			ValidateReferringPartyDescription();
			ValidateOrganizationPK();
			ValidateDefaultReferringSource();
		}

		void ValidateDomain()
		{
			DomainInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DomainInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(DomainInfo);

			if (!DomainInfo.HasErrors() && !EmailAddressValidation.IsEmailAddressValid(Domain.StartsWith("@", StringComparison.OrdinalIgnoreCase) ? ZString.Format((NoResString)"username{0}", Domain) : Domain)) // make an email address from domain name, not translatable.
			{
				DomainInfo.AddError(Res.GetString("77ff7b76-1249-4c29-ba11-3c04559c7033", "Please enter a valid email address or a valid domain name. (e.g. username@example.com / @example.com)"));
			}
		}

		void ValidateReferringParty()
		{
			ReferringPartyInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReferringPartyInfo);
			ListValidation.ErrorIfInvalidCode(ReferringPartyInfo);
		}

		void ValidateReferringPartyDescription()
		{
			ReferringPartyDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ReferringPartyDescriptionInfo);
			if (string.IsNullOrEmpty(ReferringParties.GetCodeFromDescription(ReferringPartyDescriptionInfo.Value.ToString())))
			{
				ReferringPartyDescriptionInfo.AddError(Res.GetString("b3947002-e6bc-45c8-8b68-fc1f04f37e97", "Enter a valid selection."));
			}
		}

		void ValidateOrganizationPK()
		{
			OrganizationPKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(OrganizationPKInfo);
			if (!OrganizationPK_ReadOnly)
			{
				MandatoryValidation.CheckEntered(OrganizationPKInfo);
			}
		}

		void ValidateDefaultReferringSource()
		{
			DefaultReferringSourceInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DefaultReferringSourceInfo);
			ListValidation.ErrorIfInvalidCode(DefaultReferringSourceInfo, AllReferringSources);
		}

		#endregion Validations

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			Domain = new ZString(reader.ReadElementString(Schema.Domain));
			ReferringParty = new ZString(reader.ReadElementString(Schema.ReferringParty));
			OrganizationPK = new ZGuid(reader.ReadElementString(Schema.OrganizationPK));
			DefaultReferringSource = new ZString(reader.ReadElementString(Schema.DefaultReferringSource));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.Domain, Domain);
			writer.WriteElementString(Schema.ReferringParty, ReferringParty);
			writer.WriteElementString(Schema.OrganizationPK, OrganizationPK.ToString());
			writer.WriteElementString(Schema.DefaultReferringSource, DefaultReferringSource);
		}

		#endregion Xml Serialisation
	}
}
