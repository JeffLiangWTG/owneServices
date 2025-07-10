using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EInvoicingPendingTransactionsNotificationGroup : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string GroupPK = nameof(GroupPK);
			public const string DateType = nameof(DateType);
			public const string Days = nameof(Days);
		}

		#endregion

		#region Bound Properties

		#region Group

		[ResourceStringData("2974ED4F-D2D9-4258-A0E5-1D662A011D52", Caption = "Group")]
		[List("GroupCollection")]
		public ZGuid GroupPK
		{
			get => groupPK;
			set
			{
				SetNonPersistentPropertyValue(GroupPKInfo, ref groupPK, value);
				ValidateAll();
			}
		}
		ZGuid groupPK;

		public ZPropertyInfo GroupPKInfo => GetZPropertyInfo(Schema.GroupPK);

		public GlbGroupCollection GroupCollection
		{
			get { return groupCollection ?? (groupCollection = new GlbGroupCollection(CurrentFactory)); }
		}

		GlbGroupCollection groupCollection;

		public GlbGroup GlbGroup
		{
			get => CurrentFactory.Load<GlbGroup>(GroupPK);
		}

		#endregion

		#region Date

		[ResourceStringData("037178DB-6681-418F-9D60-0839A24633EA", Caption = "Date")]
		[List("DateTypeCollection")]
		public ZString DateType
		{
			get => dateType;
			set
			{
				SetNonPersistentPropertyValue(DateTypeInfo, ref dateType, value);
				ValidateAll();
			}
		}
		ZString dateType;

		public ZPropertyInfo DateTypeInfo => GetZPropertyInfo(Schema.DateType);

		public class DateTypeList : CodeDescriptionPairList
		{
			public const string PostDate = "PST";
			public const string InvoiceDate = "INV";

			public ZString PostDateDescription = ResString.GetMultilingualString("7158BEE3-D9AC-4BFA-9481-0C520469E8ED", "Post Date");
			public ZString InvoiceDateDescription = ResString.GetMultilingualString("111539A0-D514-46AD-9A77-C6670FFB5D08", "Invoice Date");

			public DateTypeList()
			{
				AddPair(PostDate, PostDateDescription);
				AddPair(InvoiceDate, InvoiceDateDescription);
			}
		}

		public DateTypeList DateTypeCollection
		{
			get { return dateTypeCollection ?? (dateTypeCollection = new DateTypeList()); }
		}

		DateTypeList dateTypeCollection;

		#endregion

		#region Days

		[ResourceStringData("1512B61E-4266-451D-AF5D-B590E7081418", Caption = "Days")]
		[MaxLength(3)]
		public ZInt Days
		{
			get => days;
			set
			{
				SetNonPersistentPropertyValue(DaysInfo, ref days, value);
				ValidateAll();
			}
		}
		ZInt days;

		public ZPropertyInfo DaysInfo => GetZPropertyInfo(Schema.Days);

		#endregion

		#endregion

		#region Validation

		void ValidateAll()
		{
			ValidateGroupPK();
			ValidateDateType();
			ValidateDays();
		}

		void ValidateGroupPK()
		{
			if (!IsValidationSuspended)
			{
				GroupPKInfo.ClearAllNotifications();
				if (GroupPK.IsEmpty)
				{
					GroupPKInfo.AddError(ResString.GetMultilingualString("46583A83-DE0F-4EC0-ABFC-3873DDB520A3", "Please enter a Group."));
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(GroupPKInfo);
				}
			}
		}

		void ValidateDateType()
		{
			if (!IsValidationSuspended)
			{
				DateTypeInfo.ClearAllNotifications();
				if (DateType.IsEmpty)
				{
					DateTypeInfo.AddError(ResString.GetMultilingualString("135B4128-E21F-46FA-81F3-C598800F5FAE", "Please select the date criteria."));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(DateTypeInfo);
				}
			}
		}

		void ValidateDays()
		{
			if (!IsValidationSuspended)
			{
				DaysInfo.ClearAllNotifications();
				if (Days < 0 || Days > 365)
				{
					DaysInfo.AddError(ResString.GetMultilingualString("F50608B0-DE0B-49A0-944C-2DF4CEF82F3E", "The day value must be less than or equal to the maximum 365."));
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EInvoicingPendingTransactionsNotificationGroup();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.GroupPK, GroupPK.ToString());
			writer.WriteElementString(Schema.DateType, DateType);
			writer.WriteElementString(Schema.Days, Days.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			GroupPK = ZGuid.ParseSafe(reader.ReadElementString(Schema.GroupPK));
			DateType = reader.ReadElementString(Schema.DateType);
			Days = ZInt.ParseEmptyAsZero(reader.ReadElementString(Schema.Days));
		}

		#endregion
	}
}
