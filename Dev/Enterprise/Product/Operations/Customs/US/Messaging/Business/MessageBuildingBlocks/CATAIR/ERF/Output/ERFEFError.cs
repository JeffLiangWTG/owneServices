namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	//this is here just to ensure that responses as indicated do not yield error message emails....because of failed processing
	//B018888XJ5FR                                               11333                
	//EFDB ERR = XRJK8/LOCKG/14       DATABASE ERROR                                  
	//F111    DSAFASDFASDFD                      99                                   
	//F411                            0D6FACILITY NAME NOT ON FILE                    
	//Y  8888XJ5FR00003

	[OutputBlock("E")]
	public partial class ERFEError : MessageBlock
	{
		public ERFEError()
			: base("E")
		{
		}

		[MessageBlockString(79, 2, "C")]
		public ZString NarrativeMessage;
	}
}