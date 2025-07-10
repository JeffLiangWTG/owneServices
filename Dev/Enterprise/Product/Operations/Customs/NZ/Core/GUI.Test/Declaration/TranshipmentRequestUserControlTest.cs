using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class TranshipmentRequestUserControlTest : TestCaseWithFactory
	{
		public void TestMutexLock()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Factory.Save();
			using (var form = new ZForm())
			using (var control = new TranshipmentRequestUserControl_ForTesting())
			{
				form.Controls.Add(control);
				form.SetDataBinding(dec, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible", control.MainPanel.Visible);
				Assert("CoveringLabel Not Visible", !control.CoveringLabel.Visible);
				var newFactory = new BusinessObjectFactory();
				var decInNewFactory = newFactory.Load<JobDeclaration>(dec.PK);
				using (var form2 = new ZForm())
				using (var control2 = new TranshipmentRequestUserControl_ForTesting())
				{
					form2.Controls.Add(control2);
					form2.SetDataBinding(decInNewFactory, ".");
					form2.Show();
					control2.RefreshBinding();
					Assert("MainPanel Not Visible", !control2.MainPanel.Visible);
					Assert("CoveringLabel Visible", control2.CoveringLabel.Visible);
					AssertEquals("CoveringLabel Text", TranshipmentRequestUserControl.MutexLockText, control2.CoveringLabel.Text);
				}

				Factory.Save();
				using (var form3 = new ZForm())
				using (var control3 = new TranshipmentRequestUserControl_ForTesting())
				{
					form3.Controls.Add(control3);
					form3.SetDataBinding(decInNewFactory, ".");
					form3.Show();
					control3.RefreshBinding();
					Assert("MainPanel Visible", control3.MainPanel.Visible);
					Assert("CoveringLabel Not Visible", !control3.CoveringLabel.Visible);
				}
			}
		}

		public void TestMutexLockForMultipleHAWBS()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB1";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB2";
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HB3";
			var hawb4 = mawb.ChildBills.AddNew();
			hawb4.CS_HAWB = "HB4";
			Factory.Save();
			using (var form = new ZForm())
			using (var control = new TranshipmentRequestUserControl_ForTesting())
			{
				form.Controls.Add(control);
				form.SetDataBinding(hawb1, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible - HAWB1 should be able to create a Transhipment Request", control.MainPanel.Visible);
				Assert("CoveringLabel (Mutex in process) Not Visible", !control.CoveringLabel.Visible);
				form.SetDataBinding(hawb3, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible - HAWB3 should also be able to create a Transhipment Request", control.MainPanel.Visible);
				Assert("CoveringLabel (Mutex in process) Not Visible", !control.CoveringLabel.Visible);
				var newFactory = new BusinessObjectFactory();
				var hawb1InNewFactory = newFactory.Load<CusHAWB>(hawb1.PK);
				var hawb3InNewFactory = newFactory.Load<CusHAWB>(hawb3.PK);
				using (var form2 = new ZForm())
				using (var control2 = new TranshipmentRequestUserControl_ForTesting())
				{
					form2.Controls.Add(control2);
					form2.SetDataBinding(hawb1InNewFactory, ".");
					form2.Show();
					control2.RefreshBinding();
					Assert("MainPanel Not Visible", !control2.MainPanel.Visible);
					Assert("CoveringLabel Visible", control2.CoveringLabel.Visible);
					AssertEquals("CoveringLabel Text - Mutex activated - HAWB1 cannot be actioned by another user presently", TranshipmentRequestUserControl.MutexLockText, control2.CoveringLabel.Text);
					form.SetDataBinding(hawb3InNewFactory, ".");
					form.Show();
					control2.RefreshBinding();
					Assert("MainPanel Not Visible", !control2.MainPanel.Visible);
					Assert("CoveringLabel Visible", control2.CoveringLabel.Visible);
					AssertEquals("CoveringLabel Text - Mutex activated - HAWB3 cannot be actioned by another user presently", TranshipmentRequestUserControl.MutexLockText, control2.CoveringLabel.Text);
				}

				form.SetDataBinding(hawb2, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible - HAWB2 can create a Transhipment Request - no Mutex for Hawb2 as yet", control.MainPanel.Visible);
				Assert("CoveringLabel (Mutex in process) Not Visible", !control.CoveringLabel.Visible);
				Factory.Save();
				using (var form3 = new ZForm())
				using (var control3 = new TranshipmentRequestUserControl_ForTesting())
				{
					form3.Controls.Add(control3);
					form3.SetDataBinding(hawb1InNewFactory, ".");
					form3.Show();
					control3.RefreshBinding();
					Assert("MainPanel Visible - transhipment request was saved - mutex was released - Hawb1 transhipment request can be accessed by another user", control3.MainPanel.Visible);
					Assert("Mutex CoveringLabel Not Visible", !control3.CoveringLabel.Visible);
				}
			}
		}

		public void TestMutexLockForMultipleSCAHouseS()
		{
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			var houseBill1 = oceanBill.HouseBills.AddNew();
			houseBill1.CA_HouseBill = "HB1";
			var houseBill2 = oceanBill.HouseBills.AddNew();
			houseBill2.CA_HouseBill = "HB2";
			var houseBill3 = oceanBill.HouseBills.AddNew();
			houseBill3.CA_HouseBill = "HB3";
			var houseBill4 = oceanBill.HouseBills.AddNew();
			houseBill4.CA_HouseBill = "HB4";
			Factory.Save();
			using (var form = new ZForm())
			using (var control = new TranshipmentRequestUserControl_ForTesting())
			{
				form.Controls.Add(control);
				form.SetDataBinding(houseBill1, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible - HAWB1 should be able to create a Transhipment Request", control.MainPanel.Visible);
				Assert("CoveringLabel (Mutex in process) Not Visible", !control.CoveringLabel.Visible);
				form.SetDataBinding(houseBill3, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible - HAWB3 should also be able to create a Transhipment Request", control.MainPanel.Visible);
				Assert("CoveringLabel (Mutex in process) Not Visible", !control.CoveringLabel.Visible);
				var newFactory = new BusinessObjectFactory();
				var houseBill1InNewFactory = newFactory.Load<CusSCAHouse>(houseBill1.PK);
				var houseBill3InNewFactory = newFactory.Load<CusSCAHouse>(houseBill3.PK);
				using (var form2 = new ZForm())
				using (var control2 = new TranshipmentRequestUserControl_ForTesting())
				{
					form2.Controls.Add(control2);
					form2.SetDataBinding(houseBill1InNewFactory, ".");
					form2.Show();
					control2.RefreshBinding();
					Assert("MainPanel Not Visible", !control2.MainPanel.Visible);
					Assert("CoveringLabel Visible", control2.CoveringLabel.Visible);
					AssertEquals("CoveringLabel Text - Mutex activated - HAWB1 cannot be actioned by another user presently", TranshipmentRequestUserControl.MutexLockText, control2.CoveringLabel.Text);
					form.SetDataBinding(houseBill3InNewFactory, ".");
					form.Show();
					control2.RefreshBinding();
					Assert("MainPanel Not Visible", !control2.MainPanel.Visible);
					Assert("CoveringLabel Visible", control2.CoveringLabel.Visible);
					AssertEquals("CoveringLabel Text - Mutex activated - HAWB3 cannot be actioned by another user presently", TranshipmentRequestUserControl.MutexLockText, control2.CoveringLabel.Text);
				}

				form.SetDataBinding(houseBill2, ".");
				form.Show();
				control.RefreshBinding();
				Assert("MainPanel Visible - HAWB2 can create a Transhipment Request - no Mutex for Hawb2 as yet", control.MainPanel.Visible);
				Assert("CoveringLabel (Mutex in process) Not Visible", !control.CoveringLabel.Visible);
				Factory.Save();
				using (var form3 = new ZForm())
				using (var control3 = new TranshipmentRequestUserControl_ForTesting())
				{
					form3.Controls.Add(control3);
					form3.SetDataBinding(houseBill1InNewFactory, ".");
					form3.Show();
					control3.RefreshBinding();
					Assert("MainPanel Visible - transhipment request was saved - mutex was released - Hawb1 transhipment request can be accessed by another user", control3.MainPanel.Visible);
					Assert("Mutex CoveringLabel Not Visible", !control3.CoveringLabel.Visible);
				}
			}
		}

		public void TestIncomingOutgoingCraftDetailsVisibility()
		{
			TranshipmentRequestControlForJobDeclarationTestRunner((control, dec) =>
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("Invisible when not Sea/Air", !control.InOutCraftDetailsGroupBox.Visible);
				Assert("Invisible when not Sea", !control.VesselCodeFindBox.Visible);
				Assert("Invisible when not Sea", !control.LloydsIMOTextBox.Visible);
				Assert("Invisible when not Sea", !control.VoyageTextBox.Visible);
				Assert("Invisible when not Air", !control.FlightTextBox.Visible);
				var transhipmentRequest = TranshipmentRequest.Create(dec);
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				control.SetDataBinding(dec, string.Empty);
				control.RefreshBinding();
				Assert("Visible when Sea", control.InOutCraftDetailsGroupBox.Visible);
				Assert("Visible when Sea", control.VesselCodeFindBox.Visible);
				Assert("Visible when Sea", control.LloydsIMOTextBox.Visible);
				Assert("Visible when Sea", control.VoyageTextBox.Visible);
				Assert("Invisible when not Air", !control.FlightTextBox.Visible);
				AssertEquals("ArrivalDateEdit Location.Y when Sea", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 93, true), control.ArrivalDateEdit.Location);
				AssertEquals("DepartureDateEdit Location.Y when Sea", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 93, true), control.DepartureDateEdit.Location);
				AssertEquals("VesselAndFlightLinkLabel Location.Y when Sea", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 73, true), control.VesselAndFlightLinkLabel.Location);
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
				Assert("Visible when Air", control.InOutCraftDetailsGroupBox.Visible);
				Assert("Invisible when not Sea", !control.VesselCodeFindBox.Visible);
				Assert("Invisible when not Sea", !control.LloydsIMOTextBox.Visible);
				Assert("Invisible when not Sea", !control.VoyageTextBox.Visible);
				Assert("Visible when Air and is Export writeoff", control.FlightTextBox.Visible);
				AssertEquals("ArrivalDateEdit Location.Y when Air", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 49, true), control.ArrivalDateEdit.Location);
				AssertEquals("DepartureDateEdit Location.Y when Air", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 49, true), control.DepartureDateEdit.Location);
				AssertEquals("VesselAndFlightLinkLabel Location.Y when Air", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 26, true), control.VesselAndFlightLinkLabel.Location);
				dec.TranshipmentRequest.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
				Assert("Not Visible for Domestic Transhipment", !control.InOutCraftDetailsGroupBox.Visible);
				Assert("Not Visible for Domestic Transhipment", !control.InOutTransportModeDropEdit.Visible);
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
				Assert("Not Visible for Air and is Import writeoff (ICR)", !control.FlightTextBox.Visible);
			}

			);
		}

		public void TestFields()
		{
			TranshipmentRequestControlForJobDeclarationTestRunner((control, dec) =>
			{
				Assert(control.Controls.Find("TransferTransportModeDropEdit", true).Length > 0);
				Assert(control.Controls.Find("InOutTransportModeDropEdit", true).Length > 0);
				Assert(control.Controls.Find("InOutCraftDetailsGroupBox", true).Length > 0);
				Assert(control.Controls.Find("VesselCodeFindBox", true).Length > 0);
				Assert(control.Controls.Find("LloydsIMOTextBox", true).Length > 0);
				Assert(control.Controls.Find("VoyageTextBox", true).Length > 0);
				Assert(control.Controls.Find("FlightTextBox", true).Length > 0);
				Assert(control.Controls.Find("ArrivalDateEdit", true).Length > 0);
				Assert(control.Controls.Find("VesselAndFlightLinkLabel", true).Length > 0);
			}

			);
		}

		public void TestOutgoingTransportDetailsVisibilityForDTR()
		{
			TranshipmentRequestControlForAirCargoTestRunner((control, bill) =>
			{
				var transhipmentRequest = TranshipmentRequest.Create(bill);
				bill.TranshipmentRequest.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
				bill.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				control.SetDataBinding(bill, string.Empty);
				control.RefreshBinding();
				Assert("Not Visible for Domestic Transhipment", !control.InOutCraftDetailsGroupBox.Visible);
				Assert("Not Visible for Domestic Transhipment", !control.InOutTransportModeDropEdit.Visible);
			}

			);
		}

		public void TestVesselAndFlightLinkLabel()
		{
			TranshipmentRequestControlForJobDeclarationTestRunner((control, dec) =>
			{
				WebUrlLauncher.ClearLastUrlLaunched();
				AssertEquals("Precondition", string.Empty, WebUrlLauncher.LastUrlLaunched);
				var transhipmentRequest = TranshipmentRequest.Create(dec);
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				Application.DoEvents();
				control.VesselAndFlightLinkLabel.PerformClick_ForTest();
				AssertEquals("VesselAndFlightLink", NZEDIMenu.UrlCustomsFindVesselOrFlight, WebUrlLauncher.LastUrlLaunched);
			}

			);
		}

		public void TestVisibilityForICR()
		{
			TranshipmentRequestControlForJobDeclarationTestRunner((control, dec) =>
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				var transhipmentRequest = TranshipmentRequest.Create(dec);
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				Assert("Invisible when not Import ICR", !control.TransitDestinationGroupBox.Visible);
			}

			);
			TranshipmentRequestControlForJobDeclarationTestRunner((control, dec) =>
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				var transhipmentRequest = TranshipmentRequest.Create(dec);
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				control.SetDataBinding(dec, string.Empty);
				control.RefreshBinding();
				Assert("Visible when Import ICR for ITR", control.TransitDestinationGroupBox.Visible);
			}

			);
		}

		public void TestTransitDestinationVisiblity()
		{
			TranshipmentRequestControlForJobDeclarationTestRunner((control, dec) =>
			{
				var transhipmentRequest = TranshipmentRequest.Create(dec);
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("Invisible when not Import ICR", !control.TransitDestinationGroupBox.Visible);
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Assert("Invisible when not Import Write-of (ICR)", !control.TransitDestinationGroupBox.Visible);
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				dec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
				control.SetDataBinding(dec, string.Empty);
				control.RefreshBinding();
				Assert("Visible when ICR", control.TransitDestinationGroupBox.Visible);
				AssertEquals("TransitDestinationGroupBox location for ITR", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 76, true), control.TransitDestinationGroupBox.Location);
				Assert("Invisible when transportmode of declaration is not sea", !control.TransitDestinationPortFindBox.Visible);
				dec.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				Assert("Invisible when transportmode of declaration is not sea", !control.TransitDestinationPortFindBox.Visible);
				dec.JE_TransportMode = JobTransportModeList.Codes.Sea;
				Assert("Visible when transportmode of declaration is sea", control.TransitDestinationPortFindBox.Visible);
			}

			);
			TranshipmentRequestControlForAirCargoTestRunner((control, bill) =>
			{
				var transhipmentRequest = TranshipmentRequest.Create(bill);
				bill.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				control.SetDataBinding(bill, string.Empty);
				control.RefreshBinding();
				Assert("TransitDestinationGroupBox is visible for ICR", control.TransitDestinationGroupBox.Visible);
				Assert("Invisible for Aircargo", !control.TransitDestinationPortFindBox.Visible);
			}

			);
			TranshipmentRequestControlForSeaCargoTestRunner((control, bill) =>
			{
				var transhipmentRequest = TranshipmentRequest.Create(bill);
				control.SetDataBinding(bill, string.Empty);
				control.RefreshBinding();
				Assert("TransitDestinationGroupBox is visible for ICR", control.TransitDestinationGroupBox.Visible);
				Assert("Visible for Seacargo", control.TransitDestinationPortFindBox.Visible);
			}

			);
		}

		public void TestTranshipmentRequestValidationOnOff()
		{
			void actionAssertions(TranshipmentRequestUserControl_ForTesting control, BusinessObject bo)
			{
				Factory.Save();
				control.SetDataBinding(bo, "");
				control.RefreshBinding();
				var transhipmentRequest = control.TranshipmentRequestBO;
				AssertNotEquals("Transhipment request should always be created", null, transhipmentRequest);
				AssertEquals("Transhipment request should be in the initial state and no validation needed", true, transhipmentRequest.IsValidationSuspended);
				AssertEquals("Transhipment request should be in the initial state and to be deleted", false, transhipmentRequest.IsSavedByFactory);
				transhipmentRequest.C4_ModeOfMovement = "A";
				AssertEquals("Transhipment request validation should be working on changes", false, transhipmentRequest.IsValidationSuspended);
				AssertEquals("Transhipment request validation should be saved on changes", true, transhipmentRequest.IsSavedByFactory);
				transhipmentRequest.C4_ModeOfMovement = null;
				transhipmentRequest.C4_TranshipModeOfMovement = "A";
				AssertEquals("Transhipment request validation should be working on changes", false, transhipmentRequest.IsValidationSuspended);
				AssertEquals("Transhipment request validation should be saved on changes", true, transhipmentRequest.IsSavedByFactory);
			}

			CombineAssertions(() =>
			{
				TranshipmentRequestControlForAirCargoTestRunner(actionAssertions);
				TranshipmentRequestControlForSeaCargoTestRunner(actionAssertions);
				TranshipmentRequestControlForJobDeclarationTestRunner(actionAssertions);
			}

			);
		}

		void TranshipmentRequestControlForJobDeclarationTestRunner(Action<TranshipmentRequestUserControl_ForTesting, JobDeclaration> action)
		{
			var dec = Factory.New<JobDeclaration>();
			using (var form = new ZForm())
			using (var control = new TranshipmentRequestUserControl_ForTesting())
			{
				form.Controls.Add(control);
				form.SetDataBinding(dec, ".");
				form.Show();
				action(control, dec);
			}
		}

		void TranshipmentRequestControlForAirCargoTestRunner(Action<TranshipmentRequestUserControl_ForTesting, CusHAWB> action)
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mawb.CM_RL_NKLoadPort = "CN123";
			mawb.CM_RL_NKDischargePort = "NZ123";
			var bill = mawb.ChildBills.AddNew();
			using (var form = new ZForm())
			using (var control = new TranshipmentRequestUserControl_ForTesting())
			{
				form.Controls.Add(control);
				form.SetDataBinding(bill, ".");
				form.Show();
				action(control, bill);
			}
		}

		void TranshipmentRequestControlForSeaCargoTestRunner(Action<TranshipmentRequestUserControl_ForTesting, CusSCAHouse> action)
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			ocean.CB_RL_NKPortOfDischarge = "NZ123";
			ocean.CB_RL_NKPortOfLoading = "CN123";
			var bill = ocean.HouseBills.AddNew();
			using (var form = new ZForm())
			using (var control = new TranshipmentRequestUserControl_ForTesting())
			{
				form.Controls.Add(control);
				form.SetDataBinding(bill, ".");
				form.Show();
				action(control, bill);
			}
		}
	}

	public class TranshipmentRequestUserControl_ForTesting : TranshipmentRequestUserControl
	{
		public TranshipmentRequestUserControl_ForTesting()
		{
		}

		public new ZCodeFindBox VesselCodeFindBox => base.VesselCodeFindBox;
		public new ZArchitecture.ZTextBox LloydsIMOTextBox => base.LloydsIMOTextBox;
		public new ZArchitecture.ZTextBox VoyageTextBox => base.VoyageTextBox;
		public new ZArchitecture.ZTextBox FlightTextBox => base.FlightTextBox;
		public new ZGroupBox InOutCraftDetailsGroupBox => base.InOutCraftDetailsGroupBox;
		public new ZGroupBox TransitDestinationGroupBox => base.TransitDestinationGroupBox;
		public new ZLinkLabel VesselAndFlightLinkLabel => base.VesselAndFlightLinkLabel;
		public new ZDateEdit ArrivalDateEdit => base.ArrivalDateEdit;
		public new ZDateEdit DepartureDateEdit => base.DepartureDateEdit;
		public new ZArchitecture.ZLabel CoveringLabel => base.CoveringLabel;
		public new ZPanel MainPanel => base.MainPanel;
		public new ZCodeFindBox TransitDestinationPortFindBox => base.TransitDestinationPortFindBox;
		public new ZDropEdit InOutTransportModeDropEdit => base.InOutTransportModeDropEdit;
		public new TranshipmentRequest TranshipmentRequestBO => base.TranshipmentRequestBO;
	}
}
