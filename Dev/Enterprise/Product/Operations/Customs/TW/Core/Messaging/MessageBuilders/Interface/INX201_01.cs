using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface INX201_01Declaration : INXDeclaration
	{
		ZString ID { get; }

		ZDateTime IssueDateTime { get; }

		IDeclarationAdditionalDocument AdditionalDocument { get; }

		IAdditionalInformation AdditionalInformation { get; }

		IGoodsShipment GoodsShipment { get; }

		ZString AdditionalDeclarationID { get; }
	}
}
