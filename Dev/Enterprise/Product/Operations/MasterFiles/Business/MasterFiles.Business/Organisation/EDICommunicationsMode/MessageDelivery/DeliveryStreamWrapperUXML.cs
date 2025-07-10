using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class DeliveryStreamWrapperUXML : DeliveryStreamWrapperAbstract
	{
		public DeliveryStreamWrapperUXML(SubStreamableStream content, IEntityInfo sourceInfo) : base(content, sourceInfo)
		{ }

		public DeliveryStreamWrapperUXML(IEntityInfo sourceInfo, ITopLevelDataObject dataObject, IXmlWriter xmlWriter, string xmlNamespace) : base(null, sourceInfo)
		{
			this.DataObject = Argument.NotNull(dataObject, nameof(dataObject));
			this.xmlWriter = Argument.NotNull(xmlWriter, nameof(xmlWriter));
			this.xmlNamespace = xmlNamespace;
		}

		//Use this constructor only if there is no need to link the EDIMessage to any BizObj
		public DeliveryStreamWrapperUXML(SubStreamableStream content) : base(content)
		{ }

		public readonly ITopLevelDataObject DataObject;
		readonly IXmlWriter xmlWriter;
		readonly string xmlNamespace;

		public override void PopulateStream(BusinessObjectFactory factory)
		{
			if (Content == null)
			{
				Content = factory.SubscribeForDispose((SubStreamableStream)new MemoryStream());
				xmlWriter?.WriteXML(DataObject, Content, xmlNamespace);
			}
		}

		public override void SetMessageNumber(MessageNumberType messageNumberType, ZString zString)
		{
			DataObject?.SetMessageNumber(messageNumberType, zString);
		}

		public override void SetTimestamp(long timestamp)
		{
			if (DataObject != null)
			{
				DataObject.DataContext.Timestamp = timestamp;
			}
		}
	}
}
