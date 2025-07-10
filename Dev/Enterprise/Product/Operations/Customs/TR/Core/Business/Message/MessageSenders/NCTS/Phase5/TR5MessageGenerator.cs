using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class TR5MessageGenerator : Phase5NcstMessageGenerator
	{
		public TR5MessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.TR5;

		protected override ZString GetXMLMessage()
		{
			var sample = (NoResString)@"'{ 
    ""token"": ""DAqxy/Iae07LJf1GbEtBiedtQLlogIZvGyji+vurcok="", 
    ""firmId"": ""testUser"", 
    ""userId"": ""11111111101"", 
    ""msgType"": ""CC015C"", 
    ""signFlag"": false,
   ""msgContent"": { ""phase5XML"" : ""<xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx>"",
		""nationXML"" : ""<xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx>""}'";
			return sample;
		}
	}
}
