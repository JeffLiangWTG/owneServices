using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier)]
	partial class AABIX0
	{
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier)]
	public class AABIX01 : MessageBlock
	{
		public AABIX01()
			: base(id)
		{
		}

		const string id = "X0_";
		public static string Get80ByteStringWithMandatoryCharacter(AABIX0 x0)
		{
			return (id + x0.ReferenceDataText).PadRight(80);
		}

		[MessageBlockString(4, 4, "")]
		public ZString ProcessingDistrictPortCode;

		[MessageBlockString(3, 9, "")]
		public ZString FilerCode;

		[MessageBlockString(2, 13, "")]
		public ZString ProcessingFilerOfficeCode;

		[MessageBlockString(2, 16, "")]
		public ZString ApplicationIdentifierCode;

		[MessageBlockString(20, 19, "")]
		public ZString UserData;
	}
}
