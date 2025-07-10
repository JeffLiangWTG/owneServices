using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrderWorkflowCondition2CodeList : CodeDescriptionPairList
	{
		public OrderWorkflowCondition2CodeList()
		{
			AddPair(Core.Constants.TransportModes.Air, Res.GetString("25117f93-5340-4230-8d07-d0334ed55391", "Air"));
			AddPair(Core.Constants.TransportModes.Sea, Res.GetString("9ad54e70-839b-439e-a58e-9fa28f1a527f", "Sea"));
			AddPair(Core.Constants.TransportModes.Road, Res.GetString("f70d5152-a680-47d8-9a86-e3b75d7d8519", "Road"));
			AddPair(Core.Constants.TransportModes.Rail, Res.GetString("65abc67e-b710-4fa8-bd18-563cdacbda5b", "Rail"));
			AddPair(Core.Constants.TransportModes.Mail, Res.GetString("8d0ee117-2bd3-4605-ac46-0433f213f1f7", "Post"));
			AddPair("", "");
			AddPair(Core.Constants.ContainerModes.LCL, Res.GetString("3e9d0d3f-c627-4722-b997-14dd60dadd59", "LCL Cargo"));
			AddPair(Core.Constants.ContainerModes.FCL, Res.GetString("99b1070f-8e85-452c-bda8-e79cdc2cb5e5", "FCL Cargo"));
		}
	}
}
