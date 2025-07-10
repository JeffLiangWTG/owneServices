using Enterprise.Core;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class TransportModeList : CodeDescriptionPairList
	{
		public TransportModeList()
		{
			AddPair("", ResString.GetMultilingualString("27E91502-3F00-4E60-9139-9E4CDDB8C3E6", "Transport mode not applicable"));
			AddPair(Constants.TransportModes.Air, ResString.GetMultilingualString("27FC33B7-C123-4646-B442-8808095FD7EA", "Air"));
			AddPair(Constants.TransportModes.Sea, ResString.GetMultilingualString("88FA2745-B1B3-47F4-A668-348E01292053", "Maritime"));
			AddPair(Constants.TransportModes.Road, ResString.GetMultilingualString("CB614D0C-ADB2-427F-8A25-64C7CFA8180A", "Road"));
			AddPair(Constants.TransportModes.Rail, ResString.GetMultilingualString("6A0FE538-AAC2-4BAE-938C-0AD56AE1EF84", "Rail"));
			AddPair(Constants.TransportModes.Mail, ResString.GetMultilingualString("8A6B65FD-69B2-457B-B41B-AE0869939DD9", "Post"));
			AddPair(Constants.TransportModes.FixedTransportInstallations, ResString.GetMultilingualString("B00A4DD5-818A-4639-B6F4-430F81DA9F88", "Fixed Installations"));
			AddPair(Constants.TransportModes.Other, ResString.GetMultilingualString("E29CCC4C-BAEB-461F-B444-453233EF2B8B", "Transport mode not specified"));
		}
	}

	public class RemovalTransportModeList : TransportModeList
	{
		public RemovalTransportModeList() : base()
		{
		}
	}
}
