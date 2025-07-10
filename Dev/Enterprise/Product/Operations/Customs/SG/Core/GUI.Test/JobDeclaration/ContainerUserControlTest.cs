using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class ContainerUserControlTest : Customs.GUI.Testing.BaseCustomsCusContainersWithTrackingUserControlTest
	{
		protected override ZString DefaultImportMessageType => MessageTypeCodeList.Codes.IPT;

		protected override ZString DefaultExportMessageType => MessageTypeCodeList.Codes.OUT;

		protected override ZString DefaultOtherMessageType => MessageTypeCodeList.Codes.TNP;
	}
}
