using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

[XmlSerializerAssembly("Enterprise.Customs.NL.Business.XmlSerializers")]
public class SenderInfo : RegistryBusinessObjectTemplate
{
	public SenderInfo()
		: base()
	{
	}

	public SenderInfo(SenderInfoCollection parentCollection)
		: base()
	{
		Collection = parentCollection;
	}

	public SenderInfo(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public SenderInfo(FallbackLevel fallbackLevel, BusinessObjectFactory factory, SenderInfoCollection collection) : base(fallbackLevel, factory)
	{
		this.Collection = collection;
	}

	[BusinessObjectTestExclude()]
	public SenderInfoCollection Collection { get; set; }

	public static class Schema
	{
		public const string OrganizationPK = "OrganizationPK";
		public const string SenderID = "SenderID";
		public const string DefaultSenderID = "DefaultSenderID";
	}

	#region OrganizationPK

	[List(nameof(Organizations))]
	[RelatedBusinessObject("Organization")]
	[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.NL.Business.SenderInfo|OrganizationPK", Caption = "Organization", ShortCaption = "Org.")]
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

	public ZPropertyInfo OrganizationPKInfo
	{
		get { return GetZPropertyInfo(Schema.OrganizationPK); }
	}

	void ValidateOrganizationPK()
	{
		OrganizationPKInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(OrganizationPKInfo);
		ListValidation.ErrorIfInvalidPK(OrganizationPKInfo, Organizations);

		if (Collection != null && Collection.OfType<SenderInfo>().Any(x => x != this && x.organizationPK == OrganizationPK))
		{
			OrganizationPKInfo.AddError(DuplicateEntryStatusCode);
		}
	}
	public string DuplicateEntryStatusCode = ResString.GetMultilingualString("AE608A99-F415-4852-A584-1509844ECE39", "You have already entered this organization");

	public OrgHeader Organization
	{
		get { return CurrentFactory.Load<OrgHeader>(OrganizationPK); }
	}

	public OrgHeaderCollection Organizations
	{
		get { return new OrgHeaderCollection(CurrentFactory); }
	}

	#endregion

	#region SenderID
	[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.NL.Business.SenderInfo|SenderID", Caption = "Sender ID", ShortCaption = "Sender ID")]
	public ZString SenderID
	{
		get { return senderInfoID; }
		set
		{
			SetNonPersistentPropertyValue(SenderIDInfo, ref senderInfoID, value);

			if (!IsValidationSuspended)
			{
				ValidateSenderID();
			}

			SenderIDInfo.RefreshBinding();
		}
	}
	ZString senderInfoID;

	public ZPropertyInfo SenderIDInfo
	{
		get { return GetZPropertyInfo(Schema.SenderID); }
	}
	#endregion

	void ValidateSenderID()
	{
		SenderIDInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(SenderIDInfo);
	}

	#region DefaultSenderID
	[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.NL.Business.SenderInfo|DefaultSenderID", Caption = "Default", ShortCaption = "Default")]
	public ZBool DefaultSenderID
	{
		get { return defaultSenderID; }
		set
		{
			SetNonPersistentPropertyValue(DefaultSenderIDInfo, ref defaultSenderID, value);

			if (!IsValidationSuspended)
			{
				ValidateDefaultFlag();
			}
		}
	}
	ZBool defaultSenderID;

	public ZPropertyInfo DefaultSenderIDInfo
	{
		get { return GetZPropertyInfo(Schema.DefaultSenderID); }
	}

	void ValidateDefaultFlag()
	{
		DefaultSenderIDInfo.ClearAllNotifications();

		if (Collection != null && Collection.OfType<SenderInfo>().Count(x => x.DefaultSenderID) > 1)
		{
			DefaultSenderIDInfo.AddError(MultipleDefaults);
		}
	}
	public string MultipleDefaults = ResString.GetMultilingualString("7D1E89D5-CF44-4608-B580-8B7B35A54942", "You can only set one Sender ID as default");

	#endregion

	#region Lists
	public OrganisationsFindBoxCollection Organisations => organisations ?? (organisations = new OrganisationsFindBoxCollection(Factory));
	OrganisationsFindBoxCollection organisations;
	#endregion

	#region Overrides
	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();
		ValidateOrganizationPK();
		ValidateSenderID();
		ValidateDefaultFlag();
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		var clonedSenderInfo = new SenderInfo(fallbackLevel, factory, Collection);
		clonedSenderInfo.Collection = Collection;
		return clonedSenderInfo;
	}

	#endregion
	protected sealed override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.OrganizationPK, OrganizationPK.ToString());
		writer.WriteElementString(Schema.SenderID, SenderID);
		writer.WriteElementString(Schema.DefaultSenderID, DefaultSenderID.ToString());
	}

	protected sealed override void ReadElements(XmlReaderWrapper reader)
	{
		OrganizationPK = ZGuid.TryParse(reader.ReadElementString(Schema.OrganizationPK), out var orgPKValue) ? orgPKValue : ZGuid.Empty;
		SenderID = reader.ReadElementString(Schema.SenderID);
		DefaultSenderID = reader.ReadElementStringAsZBool(Schema.DefaultSenderID);
	}
}
