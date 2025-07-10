using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class UCMEDIMessageTestType : RegistryBusinessObjectTemplate
	{
		public UCMEDIMessageTestType()
		{
		}

		public UCMEDIMessageTestType(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string ApplicationCode = "ApplicationCode";
			public const string UCKDelayTimeInMilliseconds = "UCKDelayTimeInMilliseconds";
			public const string UCQDelayTimeInMilliseconds = "UCQDelayTimeInMilliseconds";
			public const string UCUDelayTimeInMilliseconds = "UCUDelayTimeInMilliseconds";
			public const string ShouldMessageBeProcessedInASeparateFactory = "ShouldMessageBeProcessedInASeparateFactory";
		}

		#region Properties

		#region ApplicationCode

		[MaxLength(EDIMessage.Schema.EM_ApplicationCodeMaxLength)]
		public ZString ApplicationCode
		{
			get { return applicationCode; }
			set
			{
				SetNonPersistentPropertyValue(ApplicationCodeInfo, ref applicationCode, value);
			}
		}
		ZString applicationCode;

		public ZPropertyInfo ApplicationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ApplicationCode); }
		}

		#endregion

		#region UCKDelayTimeInMilliseconds

		public ZInt UCKDelayTimeInMilliseconds
		{
			get { return uckDelayTimeInMilliseconds; }
			set
			{
				SetNonPersistentPropertyValue(UCKDelayTimeInMillisecondsInfo, ref uckDelayTimeInMilliseconds, value);
			}
		}
		ZInt uckDelayTimeInMilliseconds;

		public ZPropertyInfo UCKDelayTimeInMillisecondsInfo
		{
			get { return GetZPropertyInfo(Schema.UCKDelayTimeInMilliseconds); }
		}

		#endregion

		#region UCQDelayTimeInMilliseconds

		public ZInt UCQDelayTimeInMilliseconds
		{
			get { return ucqDelayTimeInMilliseconds; }
			set
			{
				SetNonPersistentPropertyValue(UCQDelayTimeInMillisecondsInfo, ref ucqDelayTimeInMilliseconds, value);
			}
		}
		ZInt ucqDelayTimeInMilliseconds;

		public ZPropertyInfo UCQDelayTimeInMillisecondsInfo
		{
			get { return GetZPropertyInfo(Schema.UCQDelayTimeInMilliseconds); }
		}

		#endregion

		#region UCUDelayTimeInMilliseconds

		public ZInt UCUDelayTimeInMilliseconds
		{
			get { return ucuDelayTimeInMilliseconds; }
			set
			{
				SetNonPersistentPropertyValue(UCUDelayTimeInMillisecondsInfo, ref ucuDelayTimeInMilliseconds, value);
			}
		}
		ZInt ucuDelayTimeInMilliseconds;

		public ZPropertyInfo UCUDelayTimeInMillisecondsInfo
		{
			get { return GetZPropertyInfo(Schema.UCUDelayTimeInMilliseconds); }
		}

		#endregion

		#region ShouldMessageBeProcessedInASeparateFactory

		public ZBool ShouldMessageBeProcessedInASeparateFactory
		{
			get { return shouldMessageBeProcessedInASeparateFactory; }
			set
			{
				SetNonPersistentPropertyValue(ShouldMessageBeProcessedInASeparateFactoryInfo, ref shouldMessageBeProcessedInASeparateFactory, value);
			}
		}
		ZBool shouldMessageBeProcessedInASeparateFactory;

		public ZPropertyInfo ShouldMessageBeProcessedInASeparateFactoryInfo
		{
			get { return GetZPropertyInfo(Schema.ShouldMessageBeProcessedInASeparateFactory); }
		}

		#endregion

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new UCMEDIMessageTestType(factory);

			result.ApplicationCode = ApplicationCode;
			result.UCKDelayTimeInMilliseconds = UCKDelayTimeInMilliseconds;
			result.UCQDelayTimeInMilliseconds = UCQDelayTimeInMilliseconds;
			result.UCUDelayTimeInMilliseconds = UCUDelayTimeInMilliseconds;
			result.ShouldMessageBeProcessedInASeparateFactory = ShouldMessageBeProcessedInASeparateFactory;

			return result;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ApplicationCode, ApplicationCode);
			writer.WriteElementString(Schema.UCKDelayTimeInMilliseconds, UCKDelayTimeInMilliseconds.ToString());
			writer.WriteElementString(Schema.UCQDelayTimeInMilliseconds, UCQDelayTimeInMilliseconds.ToString());
			writer.WriteElementString(Schema.UCUDelayTimeInMilliseconds, UCUDelayTimeInMilliseconds.ToString());
			writer.WriteElementString(Schema.ShouldMessageBeProcessedInASeparateFactory, ShouldMessageBeProcessedInASeparateFactory.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ApplicationCode = reader.ReadElementString(Schema.ApplicationCode);
			UCKDelayTimeInMilliseconds = reader.ReadElementStringAsZInt(Schema.UCKDelayTimeInMilliseconds);
			UCQDelayTimeInMilliseconds = reader.ReadElementStringAsZInt(Schema.UCQDelayTimeInMilliseconds);
			UCUDelayTimeInMilliseconds = reader.ReadElementStringAsZInt(Schema.UCUDelayTimeInMilliseconds);
			ShouldMessageBeProcessedInASeparateFactory = reader.ReadElementStringAsZBool(Schema.ShouldMessageBeProcessedInASeparateFactory);
		}

		#endregion
	}
}
