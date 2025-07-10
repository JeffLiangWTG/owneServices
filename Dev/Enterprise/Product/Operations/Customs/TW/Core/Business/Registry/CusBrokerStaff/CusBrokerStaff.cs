using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class CusBrokerStaff : RegistryBusinessObjectTemplate
	{
		#region Constructors and Schema

		public CusBrokerStaff() : base()
		{
		}

		public CusBrokerStaff(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory ?? new BusinessObjectFactory { NameForDebugging = "TWCustomsRegistryCusBrokerStaff" };

		public abstract class Schema
		{
			public const string BrokerStaffCode = "BrokerStaffCode";
			public const string Mailbox = "Mailbox";
			public const int BrokerStaffCodeMaxLength = 3;
			public const int MailboxMaxLength = 16;
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CusBrokerStaff(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			BrokerStaffCode = reader.ReadElementString(Schema.BrokerStaffCode);
			Mailbox = reader.ReadElementString(Schema.Mailbox);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BrokerStaffCode, BrokerStaffCode);
			writer.WriteElementString(Schema.Mailbox, Mailbox);
		}

		#endregion

		#region Properties
		#region BrokerStaff
		[Mandatory]
		[MaxLength(Schema.BrokerStaffCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusBrokerStaffLookups.BrokerStaffList))]
		[RelatedBusinessObject(nameof(BrokerStaff))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusBrokerStaff|BrokerStaff", Caption = "Broker Staff")]
		public ZString BrokerStaffCode
		{
			get => fBrokerStaffCode;
			set
			{
				CheckMaximumLength(BrokerStaffCodeInfo, value);
				if (BrokerStaffCode != value)
				{
					SetNonPersistentPropertyValue(BrokerStaffCodeInfo, ref fBrokerStaffCode, value);
					Mailbox = ZString.Empty;
					if (!IsValidationSuspended)
					{
						Validation.ValidateBrokerStaffCode();
					}
					BrokerStaffCodeInfo.RefreshBinding();
				}
			}
		}
		ZString fBrokerStaffCode;

		public ZPropertyInfo BrokerStaffCodeInfo => GetZPropertyInfo(Schema.BrokerStaffCode);

		public GlbStaff BrokerStaff => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, BrokerStaffCode);
		#endregion

		#region Mailbox
		[Mandatory]
		[MaxLength(Schema.MailboxMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusBrokerStaffLookups.MailboxList))]
		[ReadOnlyMember(nameof(MailboxReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusBrokerStaff|Mailbox", Caption = "Mail Box")]
		public ZString Mailbox
		{
			get => fMailbox;
			set
			{
				if (Mailbox != value)
				{
					CheckMaximumLength(MailboxInfo, value);
					SetNonPersistentPropertyValue(MailboxInfo, ref fMailbox, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateMailbox();
					}
					MailboxInfo.RefreshBinding();
				}
			}
		}
		ZString fMailbox;

		public ZPropertyInfo MailboxInfo => GetZPropertyInfo(Schema.Mailbox);

		public ZBool MailboxReadOnly => BrokerStaffCode.IsEmpty;
		#endregion
		#endregion

		#region Validation
		CusBrokerStaffValidation fValidation;

		public CusBrokerStaffValidation Validation => fValidation ?? (fValidation = new CusBrokerStaffValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Lookup
		CusBrokerStaffLookups fLookups;

		public CusBrokerStaffLookups Lookups => fLookups ?? (fLookups = new CusBrokerStaffLookups(this));
		#endregion
	}
}
