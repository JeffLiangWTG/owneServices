using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class OrderManagerRequestMapping : RegistryBusinessObjectTemplate
	{
		public OrderManagerRequestMapping() : base()
		{
		}

		public OrderManagerRequestMapping(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string Request = "Request";
			public const string RequestType = "RequestType";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrderManagerRequestMapping(fallbackLevel, factory);
		}

		#endregion

		[MaxLength(3)]
		[List(nameof(RequestTypeList))]
		public ZString RequestType
		{
			get => requestType;
			set
			{
				if (requestType != value)
				{
					SetNonPersistentPropertyValue(RequestTypeInfo, ref requestType, value, false);
					RequestTypeInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateRequestType();
					}
				}
			}
		}

		ZString requestType;

		public ExternalRequestTypeCollection RequestTypeList
		{
			get
			{
				requestTypeList ??= new ExternalRequestTypeCollection(CurrentFactory);
				return requestTypeList;
			}
		}

		ExternalRequestTypeCollection requestTypeList;

		public ZPropertyInfo RequestTypeInfo => GetZPropertyInfo(Schema.RequestType);

		void ValidateRequestType()
		{
			RequestTypeInfo.ClearAllNotifications();
			var collection = GetParentCollection(this, typeof(OrderManagerRequestMappingCollection));
			if (collection != null && collection.Cast<OrderManagerRequestMapping>().Any(x => x.RequestType != string.Empty && x != this && x.RequestType == RequestType))
			{
				RequestTypeInfo.AddError(DuplicatedCodesError);
			}
		}

		internal static string DuplicatedCodesError => ResString.GetMultilingualString("42414ee0-facc-4814-ae1b-6095cc063c5c", "The codes cannot be duplicated.");

		[ReadOnly(true)]
		public ZString Request
		{
			get => request;
			set => SetNonPersistentPropertyValue(RequestInfo, ref request, value, false);
		}

		ZString request;

		public ZPropertyInfo RequestInfo => GetZPropertyInfo(Schema.Request);

		public ZString RequestDescription
		{
			get
			{
				mappingList ??= new OrderManagerRequestMappingList();
				return mappingList.GetDescriptionFromCode(request);
			}
		}

		OrderManagerRequestMappingList mappingList;

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.RequestType, RequestType.ToString());
			writer.WriteElementString(Schema.Request, Request.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			requestType = new ZString(reader.ReadElementString(Schema.RequestType));
			request = new ZString(reader.ReadElementString(Schema.Request));
		}

		#endregion
	}
}
