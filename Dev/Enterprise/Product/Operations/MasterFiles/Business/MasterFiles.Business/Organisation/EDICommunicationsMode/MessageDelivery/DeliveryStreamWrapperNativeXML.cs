using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class DeliveryStreamWrapperNativeXML : DeliveryStreamWrapperAbstract
	{
		public DeliveryStreamWrapperNativeXML(SubStreamableStream content, IEntityInfo sourceInfo) : base(content, sourceInfo)
		{ }

		public DeliveryStreamWrapperNativeXML(IEntityInfo sourceInfo, BusinessObject businessObject, IDataContextDataObject dataContext) : base(null, sourceInfo)
		{
			MessageNumberCollection = new List<IMessageNumber>();
			this.businessObject = businessObject;
			this.dataContext = dataContext;
		}

		//Use this constructor only if there is no need to link the EDIMessage to any BizObj
		public DeliveryStreamWrapperNativeXML(SubStreamableStream content) : base(content)
		{ }

		List<IMessageNumber> MessageNumberCollection { get; set; }
		BusinessObject businessObject { get; set; }
		IDataContextDataObject dataContext { get; set; }

		public override void PopulateStream(BusinessObjectFactory factory)
		{
			if (Content == null)
			{
				var nativeXmlSerializer = ObjectFactory.Get<IBusinessObjectWithDataContextInfoSerializer>("NativeXmlSerializer");
				Content = factory.SubscribeForDispose(nativeXmlSerializer.SerializeToStream(businessObject, dataContext, MessageNumberCollection));
			}
		}

		public override void SetMessageNumber(MessageNumberType messageNumberType, ZString zString)
		{
			if (MessageNumberCollection != null)
			{
				var messageNumber = MessageNumberCollection.FirstOrDefault(mn => mn.Type == messageNumberType);
				if (messageNumber == null)
				{
					MessageNumberCollection.Add(messageNumber = new MessageNumber { Type = messageNumberType });
				}
				messageNumber.Value = zString;
			}
		}

		public override void SetTimestamp(long timestamp)
		{
			if (dataContext != null)
			{
				dataContext.Timestamp = timestamp;
			}
		}
	}
}
