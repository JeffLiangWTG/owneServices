using Enterprise.Tracking.Business.ImporterSecurityFiling;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCusISFHeader))]
	sealed class TrackingCusISFHeaderBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingCusISFHeader>
	{
		protected override TrackingCusISFHeader GetNewBizOForNotification()
		{
			var header = Factory.New<TrackingCusISFHeader>();
			header.ReferenceDatas.AddNew();
			var line = header.Lines.AddNew();
			line.BL_TextProductCode = "NO!";
			line.BL_RN_NKGoodsOrigin = "DE";
			var equip = header.Equipments.AddNew();
			equip.BE_EquipCode = "CD";
			equip.BE_ContainerNum = "abc111";
			equip.BE_ContainerISO = "1234";
			header.DocAddresses.AddNew();
			return header;
		}
	}
}
