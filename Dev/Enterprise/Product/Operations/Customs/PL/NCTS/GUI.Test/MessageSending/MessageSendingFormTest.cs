using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.PL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	protected override Form GetFormToBashCore() => new MessageSendingForm(new MessageSendingObjectParent(GetNewDepartureNctsHeader()));

	public void TestBinding()
	{
		using (var form = new MessageSendingForm(new MessageSendingObjectParent(GetNewDepartureNctsHeader())))
		{
			AssertEquals("DataSourceType", typeof(MessageSendingObjectParent), form.DataSourceType);
		}
	}

	public void TestAdditionalDetailsUserControl()
	{
		using (var form = new MessageSendingForm(new MessageSendingObjectParent(GetNewDepartureNctsHeader())))
		{
			form.Show();
			var additionalDetailsUserControl = form.FindSingle<AdditionalDetailsUserControl>("AdditionalDetailsUserControl");

			CombineAssertions(() =>
			{
				AssertNotNull("AdditionalDetailsUserControl", additionalDetailsUserControl);
				AssertEquals("Should be visible", true, additionalDetailsUserControl.Visible);
			});
		}
	}

	public void TestAdditionalDetailsUserControlLayoutPanel_Arrival()
	{
		using (var form = new MessageSendingForm(new MessageSendingObjectParent(GetNewArrivalNctsHeader())))
		{
			AssertType<ArrivalAdditionalDetailsLayout>(form.GetNewAdditionalDetailsUserControlPanelLayoutProvider());
		}
	}

	public void TestAdditionalDetailsUserControlLayoutPanel_Departure()
	{
		using (var form = new MessageSendingForm(new MessageSendingObjectParent(GetNewDepartureNctsHeader())))
		{
			AssertType<DepartureAdditionalDetailsLayout>(form.GetNewAdditionalDetailsUserControlPanelLayoutProvider());
		}
	}

	NctsHeader GetNewDepartureNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		return nctsHeader;
	}

	NctsHeader GetNewArrivalNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		return nctsHeader;
	}
}
