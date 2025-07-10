using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.DataTransfer;

namespace Enterprise.Rating.GUI
{
	public class FullRatingTransferDirector : XmlDataTransferDirector
	{
		public FullRatingTransferDirector()
			: base(new FullClientRatesValueObjectDataAdapter(), false)
		{
		}

		public override XmlValueObjectSerializer Serializer
		{
			get
			{
				return new FullRatingXmlValueObjectSerializer(Adapter.ValueObjectType);
			}
		}
	}
}

