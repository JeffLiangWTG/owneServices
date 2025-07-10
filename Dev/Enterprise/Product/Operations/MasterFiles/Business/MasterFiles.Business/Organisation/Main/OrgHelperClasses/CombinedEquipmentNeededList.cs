using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CombinedEquipmentNeededList : CodeDescriptionPairList
	{
		public CombinedEquipmentNeededList()
		{
			bool askClient = true;
			AddRangeOverwriteIfExists(new FCLEquipmentNeededList(!askClient));
			AddRangeOverwriteIfExists(new LCLAIREquipmentNeededList(askClient));

			Sort();
		}
	}
}
