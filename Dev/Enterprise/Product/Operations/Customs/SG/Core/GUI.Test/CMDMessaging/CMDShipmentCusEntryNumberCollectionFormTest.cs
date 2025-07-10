using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(CMDShipmentCusEntryNumberCollectionForm))]
	sealed class CMDShipmentCusEntryNumberCollectionFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (CMDShipmentCusEntryNumberCollectionForm form = new CMDShipmentCusEntryNumberCollectionForm(ShipmentWrapper))
			{
				AssertEquals("Please Enter Permit or Exemption Details", form.FormHeading);
			}
		}

		public void TestCMDDataEnteredForBindingIsLoadedInFormConstructor()
		{
			object lazyLoadCusEntryNumbersForBinding = ShipmentWrapper.CMDDataValuesForBinding;
			Assert("Should be loaded in the lazy getter", ShipmentWrapper.CMDDataValuesForBinding.IsLoaded);
			var cmdData = Factory.New<CMDPermitNumber>();
			cmdData.CY_ParentID = ShipmentWrapper.Shipment.PK;
			cmdData.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cmdData.CY_Code = CustomsEntryTypeList.Singapore.Permit;
			cmdData.CY_Data = "OU3402000K";
			AssertEquals("Pre-condition", 0, ShipmentWrapper.CMDDataValuesForBinding.Count);
			using (CMDShipmentCusEntryNumberCollectionForm form = new CMDShipmentCusEntryNumberCollectionForm(ShipmentWrapper))
			{
				AssertEquals("Should be reloaded in the constructor", 1, ShipmentWrapper.CMDDataValuesForBinding.Count);
			}
		}

		public void TestOKButtonClicked()
		{
			using (CMDShipmentCusEntryNumberCollectionForm form = new CMDShipmentCusEntryNumberCollectionForm(ShipmentWrapper))
			{
				AssertEquals("Pre-condition", 0, Shipment.CusEntryNumbers.Count);
				AssertEquals("Pre-condition", "", ShipmentWrapper.PermitAndExemptionDetails);
				form.Closed += Form_Closed;
				form.Show();
				CMDDataValueWrapper entry1 = InsertNewCusEntryNumWrapper("*&#", "");
				form.OKButton.PerformClick();
				Assert("Should not be closed, there are validation errors", !formClosed);
				AssertEquals("Should not be updated", 0, Shipment.CusEntryNumbers.Count);
				AssertEquals("Should not be updated", "", ShipmentWrapper.PermitAndExemptionDetails);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("There are errors that need to be fixed", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				entry1.PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
				entry1.PermitNumberOrExemptionRemarks = "PMT001";
				entry1.HasChanges = true;
				form.OKButton.PerformClick();
				Assert("Should be closed now", formClosed);
				AssertEquals("Should be updated", "PMT: PMT001", ShipmentWrapper.PermitAndExemptionDetails);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelled()
		{
			using (CMDShipmentCusEntryNumberCollectionForm form = new CMDShipmentCusEntryNumberCollectionForm(ShipmentWrapper))
			{
				AssertEquals("Pre-condition", 0, Shipment.CusEntryNumbers.Count);
				AssertEquals("Pre-condition", "", ShipmentWrapper.PermitAndExemptionDetails);
				form.Show();
				CMDDataValueWrapper entry1 = InsertNewCusEntryNumWrapper("*&#", "");
				form.CloseButton.PerformClick();
				AssertEquals("Should not be updated", 0, Shipment.CusEntryNumbers.Count);
				AssertEquals("Should not be updated", "", ShipmentWrapper.PermitAndExemptionDetails);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}

			using (CMDShipmentCusEntryNumberCollectionForm form = new CMDShipmentCusEntryNumberCollectionForm(ShipmentWrapper))
			{
				form.Show();
				CMDDataValueWrapper entry1 = InsertNewCusEntryNumWrapper(CustomsEntryTypeList.Singapore.Permit, "PMT001");
				entry1.HasChanges = true;
				form.CloseButton.PerformClick();
				AssertEquals("Should not be updated", 0, Shipment.CusEntryNumbers.Count);
				AssertEquals("Should not be updated", "", ShipmentWrapper.PermitAndExemptionDetails);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestGridId()
		{
			var wrapper = new CMDShipmentWrapper(Factory.New<ForwardingShipment>());
			using (var control = new CMDShipmentCusEntryNumberCollectionForm(wrapper))
			{
				var grid = (ZGrid)control.Controls.Find("CusEntryNumbersGrid", true).Single();
				AssertEquals("a394a13f-2089-4f8f-a8a5-c0fa22806aaf", grid.GridId);
			}
		}

		protected override Form GetFormToBashCore() => new CMDShipmentCusEntryNumberCollectionForm(ShipmentWrapper);

		CMDDataValueWrapper InsertNewCusEntryNumWrapper(string entryType, string entryNum)
		{
			var result = ShipmentWrapper.CMDDataValuesForBinding.AddNew();
			result.PermitOrExemptionType = entryType;
			result.PermitNumberOrExemptionRemarks = entryNum;
			return result;
		}

		CMDShipmentWrapper ShipmentWrapper
		{
			get
			{
				if (fShipmentWrapper == null)
				{
					fShipmentWrapper = new CMDShipmentWrapper(Shipment);
				}

				return fShipmentWrapper;
			}
		}

		ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<ForwardingShipment>();
				}

				return fShipment;
			}
		}

		void Form_Closed(object sender, EventArgs e)
		{
			formClosed = true;
		}

		CMDShipmentWrapper fShipmentWrapper;
		ForwardingShipment fShipment;
		bool formClosed;
	}
}
