using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.NO.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	protected override Form GetFormToBashCore() => new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(CreateNewNctsHeader()));

	NctsHeader CreateNewNctsHeader()
	{
		var nctsHeader = Factory.NewMoq<NctsHeader>();
		nctsHeader.Setup(h => h.BH_ApplicationCode).Returns(CusInBondApplicationCodeList.Codes.NCTS5);
		nctsHeader.Setup(h => h.BH_HeaderType).Returns(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		return nctsHeader.Object;
	}
}
