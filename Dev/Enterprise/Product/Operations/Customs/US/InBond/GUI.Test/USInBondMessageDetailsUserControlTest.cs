using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusInBondMoveHeader = Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class USInBondMessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsAvailability()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var messagesTabPage = form.Controls.Find("MessagesTabPage", true)[0] as ZTabPage;
				messagesTabPage.Show();
				var inBondMessageDetailsUserControl = messagesTabPage.Controls.Find("InBondMessageDetailsUserControl", true)[0];
				var movementHeadersMessagesGrid = inBondMessageDetailsUserControl.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
				// AIR
				header.BH_ImportTransportMode = "40";
				AssertNull("BM_BTAIndicator is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_BTAIndicator]);
				AssertNull("BM_ExportLadenOn is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ExportLadenOn]);
				AssertNull("BM_TOLDate is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLDate]);
				AssertNull("TOLCarrierOrgPK is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.TOLCarrierOrgPK]);
				AssertNull("BM_OA_TOLCarrier is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_OA_TOLCarrier]);
				AssertNull("BM_TOLCarrierCode is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLCarrierCode]);
				AssertNull("BM_TOLCarrierID is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLCarrierID]);
				AssertNull("BM_TOLCityName is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLCityName]);
				AssertNull("BM_TOLStateCode is hidden", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLStateCode]);
				AssertNotNull("BM_CustomsStatus is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_CustomsStatus]);
				AssertNotNull("BM_MessageStatus is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_MessageStatus]);
				AssertNotNull("MovementUniqueCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.MovementUniqueCode]);
				AssertNotNull("BM_InBondEntryType is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_InBondEntryType]);
				AssertNotNull("InBondCarrierOrgPK is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.InBondCarrierOrgPK]);
				AssertNotNull("BM_OA_InBondCarrier is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_OA_InBondCarrier]);
				AssertNotNull("BM_InBondCarrierID is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_InBondCarrierID]);
				AssertNotNull("BM_InBondCarrierSCAC is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_InBondCarrierSCAC]);
				AssertNotNull("BM_DestinationPortCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_DestinationPortCode]);
				AssertNotNull("BM_ForeignDestPortKCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ForeignDestPortKCode]);
				AssertNotNull("BM_MonetaryValue is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_MonetaryValue]);
				AssertNotNull("BM_CustomsStatusDescription is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_CustomsStatusDescription]);
				AssertNotNull("BM_MessageStatusDescription is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_MessageStatusDescription]);
				AssertNotNull("BM_ArrivalDate is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ArrivalDate]);
				AssertNotNull("BM_ExportDate is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ExportDate]);
				AssertNotNull("BM_ExportTransportMode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ExportTransportMode]);
				// NON-AIR
				header.BH_ImportTransportMode = "10";
				AssertNotNull("BM_BTAIndicator is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_BTAIndicator]);
				AssertNotNull("BM_ExportLadenOn is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ExportLadenOn]);
				AssertNotNull("BM_TOLDate is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLDate]);
				AssertNotNull("TOLCarrierOrgPK is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.TOLCarrierOrgPK]);
				AssertNotNull("BM_OA_TOLCarrier is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_OA_TOLCarrier]);
				AssertNotNull("BM_TOLCarrierCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLCarrierCode]);
				AssertNotNull("BM_TOLCarrierID is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLCarrierID]);
				AssertNotNull("BM_TOLCityName is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLCityName]);
				AssertNotNull("BM_TOLStateCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_TOLStateCode]);
				AssertNotNull("BM_CustomsStatus is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_CustomsStatus]);
				AssertNotNull("BM_MessageStatus is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_MessageStatus]);
				AssertNotNull("MovementUniqueCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.MovementUniqueCode]);
				AssertNotNull("BM_InBondEntryType is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_InBondEntryType]);
				AssertNotNull("InBondCarrierOrgPK is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.InBondCarrierOrgPK]);
				AssertNotNull("BM_OA_InBondCarrier is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_OA_InBondCarrier]);
				AssertNotNull("BM_InBondCarrierID is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_InBondCarrierID]);
				AssertNotNull("BM_InBondCarrierSCAC is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_InBondCarrierSCAC]);
				AssertNotNull("BM_DestinationPortCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_DestinationPortCode]);
				AssertNotNull("BM_ForeignDestPortKCode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ForeignDestPortKCode]);
				AssertNotNull("BM_MonetaryValue is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_MonetaryValue]);
				AssertNotNull("BM_CustomsStatusDescription is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_CustomsStatusDescription]);
				AssertNotNull("BM_MessageStatusDescription is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_MessageStatusDescription]);
				AssertNotNull("BM_ArrivalDate is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ArrivalDate]);
				AssertNotNull("BM_ExportDate is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ExportDate]);
				AssertNotNull("BM_ExportTransportMode is shown", movementHeadersMessagesGrid.Columns[CusInBondMoveHeader.Schema.BM_ExportTransportMode]);
			}
		}

		ZForm GetTestForm(CusInBondHeader header, bool isJobDeclarationForm)
		{
			if (isJobDeclarationForm)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = USJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				header.BH_ParentID = declaration.PK;
				header.BH_ParentTableCode = declaration.TablePrefix;
				return new JobDeclarationForm(declaration);
			}

			return new USInBondForm(header);
		}

		ZUserControl GetBillsAndMovementsDetailsUserControl(ZForm form)
		{
			switch (form)
			{
				case JobDeclarationForm _:
					{
						var inBondTabPage = form.FindSingle<ZTabPage>("In-BondTabPage");
						((ZTabControl)inBondTabPage.Parent).SelectedTab = inBondTabPage;
						var inBondUserControl = (USInBondUserControl)inBondTabPage.Controls[0];
						inBondUserControl.MainTabControl.SelectedTab = inBondUserControl.AirBillsAndMovementsDetailsTabPage;
						return (USAirInBondBillsAndMovementsDetailsUserControl)inBondUserControl.AirBillsAndMovementsDetailsTabPage.Controls[0];
					}

				case USInBondForm _:
					{
						var billsAndMovementDetailsTabPage = form.FindSingle<ZTabPage>("AirBillsAndMovementsDetailsTabPage");
						billsAndMovementDetailsTabPage.Show();
						return (USAirInBondBillsAndMovementsDetailsUserControl)billsAndMovementDetailsTabPage.Controls[0];
					}

				default:
					return null;
			}
		}

		ZUserControl GetInBondMessageDetailsUserControl(ZForm form)
		{
			switch (form)
			{
				case JobDeclarationForm _:
					{
						var inBondTabPage = form.FindSingle<ZTabPage>("In-BondTabPage");
						((ZTabControl)inBondTabPage.Parent).SelectedTab = inBondTabPage;
						var inBondUserControl = (USInBondUserControl)inBondTabPage.Controls[0];
						inBondUserControl.MainTabControl.SelectedTab = inBondUserControl.MessagesTabPage;
						return (USInBondMessageDetailsUserControl)inBondUserControl.MessagesTabPage.Controls[0];
					}

				case USInBondForm _:
					{
						var messagesTabPage = form.FindSingle<ZTabPage>("MessagesTabPage");
						messagesTabPage.Show();
						return (USInBondMessageDetailsUserControl)messagesTabPage.Controls[0];
					}

				default:
					return null;
			}
		}

		ZUserControl GetInBondHeaderDetailsUserControl(ZForm form)
		{
			switch (form)
			{
				case JobDeclarationForm _:
					{
						var inBondTabPage = form.FindSingle<ZTabPage>("In-BondTabPage");
						((ZTabControl)inBondTabPage.Parent).SelectedTab = inBondTabPage;
						var inBondUserControl = (USInBondUserControl)inBondTabPage.Controls[0];
						inBondUserControl.MainTabControl.SelectedTab = inBondUserControl.DetailsTabPage;
						return (USInBondHeaderDetailUserControl)inBondUserControl.DetailsTabPage.Controls[0];
					}

				case USInBondForm _:
					{
						var movementDetailsTabPage = form.FindSingle<ZTabPage>("MainTabPage");
						movementDetailsTabPage.Show();
						return (USInBondHeaderDetailUserControl)movementDetailsTabPage.Controls[0];
					}

				default:
					return null;
			}
		}

		ZUserControl GetInBond7512DataUserControl(ZForm form)
		{
			switch (form)
			{
				case JobDeclarationForm _:
					{
						var inBondTabPage = form.FindSingle<ZTabPage>("In-BondTabPage");
						((ZTabControl)inBondTabPage.Parent).SelectedTab = inBondTabPage;
						var inBondUserControl = (USInBondUserControl)inBondTabPage.Controls[0];
						inBondUserControl.MainTabControl.SelectedTab = inBondUserControl.DocumentTabPage;
						return (USInBond7512DataUserControl)inBondUserControl.DocumentTabPage.Controls[0];
					}

				case USInBondForm _:
					{
						var cbp7512TabPage = form.FindSingle<ZTabPage>("CBP7512TabPage");
						cbp7512TabPage.Show();
						return (USInBond7512DataUserControl)cbp7512TabPage.Controls[0];
					}

				default:
					return null;
			}
		}

		public void TestSeparateBillsCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = "40"; // AIR
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					var bill1 = header.Bills.AddNew();
					bill1.B0_MasterBillNumber = "100000";
					var bill2 = header.Bills.AddNew();
					bill2.B0_MasterBillNumber = "200000";
					var bill3 = header.Bills.AddNew();
					bill3.B0_MasterBillNumber = "300000";
					var inBondBillsAndMovementsDetailsUserControl = GetBillsAndMovementsDetailsUserControl(form);
					var billsGrid = inBondBillsAndMovementsDetailsUserControl.FindSingle<ZGrid>("BillsGrid");
					var descriptorForMasterBillNumber = billsGrid.ListManager.GetItemProperties()["B0_MasterBillNumber"];
					billsGrid.List.ApplySort(descriptorForMasterBillNumber, ListSortDirection.Ascending);
					var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
					var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
					headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
					var billsOfLadingMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("BillsOfLadingMessagesGrid");
					billsOfLadingMessagesGrid.List.ApplySort(descriptorForMasterBillNumber, ListSortDirection.Descending);
					AssertEquals("300000", billsOfLadingMessagesGrid[0, 1]);
					AssertEquals("100000", billsGrid[0, 1]);
				}
			}
		}

		public void TestSeparateMovementHeadersCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = "40"; // AIR
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierID = "100000";
			moveHeader1.BM_InBondEntryType = "62";
			moveHeader1.BM_PortOfPresentationCode = "CCC";
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					var moveHeader2 = header.MovementHeaders.AddNew();
					moveHeader2.BM_InBondCarrierID = "300000";
					moveHeader2.BM_InBondEntryType = "61";
					moveHeader2.BM_PortOfPresentationCode = "BBB";
					var moveHeader3 = header.MovementHeaders.AddNew();
					moveHeader3.BM_InBondCarrierID = "200000";
					moveHeader3.BM_InBondEntryType = "63";
					moveHeader3.BM_PortOfPresentationCode = "AAA";
					var inBondHeaderDetailsUserControl = GetInBondHeaderDetailsUserControl(form);
					var movementHeadersGrid = inBondHeaderDetailsUserControl.FindSingle<ZGrid>("MovementHeadersGrid");
					var descriptorForDetailsTab = movementHeadersGrid.ListManager.GetItemProperties()["BM_InBondCarrierID"];
					movementHeadersGrid.List.ApplySort(descriptorForDetailsTab, ListSortDirection.Ascending);
					var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
					var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
					headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("MovementHeadersTabPage");
					var movementHeadersMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("MovementHeadersMessagesGrid");
					var descriptorForMessagesTab = movementHeadersMessagesGrid.ListManager.GetItemProperties()["BM_InBondEntryType"];
					movementHeadersMessagesGrid.List.ApplySort(descriptorForMessagesTab, ListSortDirection.Ascending);
					var inBond7512DataUserControl = GetInBond7512DataUserControl(form);
					var cbp7512MoveHeaderGrid = inBond7512DataUserControl.FindSingle<ZGrid>("CBP7512MoveHeaderGrid");
					var descriptorFor7512Tab = cbp7512MoveHeaderGrid.ListManager.GetItemProperties()["BM_PortOfPresentationCode"];
					cbp7512MoveHeaderGrid.List.ApplySort(descriptorFor7512Tab, ListSortDirection.Ascending);
					AssertEquals("100000", movementHeadersGrid[0, 7]);
					AssertEquals("61", movementHeadersMessagesGrid[0, 3]);
					AssertEquals("AAA", cbp7512MoveHeaderGrid[0, 1]);
				}
			}
		}

		public void TestCBP7512MoveDetailGrid()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.MovementDetails.AddNew();
			moveHeader1.MovementDetails.AddNew();
			moveHeader1.MovementDetails.AddNew();
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					var moveHeader2 = header.MovementHeaders.AddNew();
					moveHeader2.MovementDetails.AddNew();
					var inBond7512DataUserControl = GetInBond7512DataUserControl(form);
					var cbp7512MoveHeaderGrid = inBond7512DataUserControl.FindSingle<ZGrid>("CBP7512MoveHeaderGrid");
					var cbp7512MoveDetailGrid = inBond7512DataUserControl.FindSingle<ZGrid>("CBP7512MoveDetailGrid");
					cbp7512MoveHeaderGrid.SelectSingleElement(moveHeader1);
					AssertEquals("MovementDetails count of moveHeader1", 3, cbp7512MoveDetailGrid.List.Count);
					cbp7512MoveHeaderGrid.SelectSingleElement(moveHeader2);
					AssertEquals("MovementDetails count of moveHeader2", 1, cbp7512MoveDetailGrid.List.Count);
				}
			}
		}

		public void TestCBP7512MoveLineGrid()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.CBP7512Lines.AddNew();
			moveDetail1.CBP7512Lines.AddNew();
			moveDetail1.CBP7512Lines.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.CBP7512Lines.AddNew();
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					var inBond7512DataUserControl = GetInBond7512DataUserControl(form);
					var cbp7512MoveDetailGrid = inBond7512DataUserControl.FindSingle<ZGrid>("CBP7512MoveDetailGrid");
					var cbp7512MoveLineGrid = inBond7512DataUserControl.FindSingle<ZGrid>("CBP7512MoveLineGrid");
					cbp7512MoveDetailGrid.SelectSingleElement(moveDetail1);
					AssertEquals("CBP7512Lines count of moveDetail1", 3, cbp7512MoveLineGrid.List.Count);
					cbp7512MoveDetailGrid.SelectSingleElement(moveDetail2);
					AssertEquals("CBP7512Lines count of moveDetail2", 1, cbp7512MoveLineGrid.List.Count);
				}
			}
		}

		public void TestWarehouseDetailsGrid()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.WarehouseDetails.AddNew();
			moveDetail1.WarehouseDetails.AddNew();
			moveDetail1.WarehouseDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.WarehouseDetails.AddNew();
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					var inBond7512DataUserControl = GetInBond7512DataUserControl(form);
					var cbp7512MoveDetailGrid = inBond7512DataUserControl.FindSingle<ZGrid>("CBP7512MoveDetailGrid");
					var warehouseDetailsGrid = inBond7512DataUserControl.FindSingle<ZGrid>("WarehouseDetailsGrid");
					cbp7512MoveDetailGrid.SelectSingleElement(moveDetail1);
					AssertEquals("WarehouseDetails count of moveDetail1", 3, warehouseDetailsGrid.List.Count);
					cbp7512MoveDetailGrid.SelectSingleElement(moveDetail2);
					AssertEquals("WarehouseDetails count of moveDetail2", 1, warehouseDetailsGrid.List.Count);
				}
			}
		}

		public void TestCusInBondMoveHeaderMessagesGrid_BillsofLadingTabPage()
		{
			var header = Factory.New<CusInBondHeader>();
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					header.Bills.DeleteAll();
					var bill1 = header.Bills.AddNew();
					bill1.Messages.AddNew(typeof(MQEDIMessage));
					bill1.Messages.AddNew(typeof(MQEDIMessage));
					bill1.Messages.AddNew(typeof(MQEDIMessage));
					var bill2 = header.Bills.AddNew();
					bill2.Messages.AddNew(typeof(MQEDIMessage));
					var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
					var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
					headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
					var billsOfLadingMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("BillsOfLadingMessagesGrid");
					var cusInBondMoveHeaderMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("CusInBondMoveHeaderMessagesGrid");
					billsOfLadingMessagesGrid.SelectSingleElement(bill1);
					AssertEquals("Messages count of bill1", 3, cusInBondMoveHeaderMessagesGrid.List.Count);
					billsOfLadingMessagesGrid.SelectSingleElement(bill2);
					AssertEquals("Messages count of bill2", 1, cusInBondMoveHeaderMessagesGrid.List.Count);
				}
			}
		}

		public void TestCusInBondMoveHeaderMessagesGrid_MovementHeadersTabPage()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.Messages.AddNew(typeof(MQEDIMessage));
			moveHeader1.Messages.AddNew(typeof(MQEDIMessage));
			moveHeader1.Messages.AddNew(typeof(MQEDIMessage));
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.Messages.AddNew(typeof(MQEDIMessage));
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				foreach (var changingFromDifferentTab in new[] { false, true })
				{
					using (var form = GetTestForm(header, isJobDeclarationForm))
					{
						form.Show();
						var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
						var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
						if (changingFromDifferentTab)
						{
							headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
						}

						headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("MovementHeadersTabPage");
						var movementHeadersMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("MovementHeadersMessagesGrid");
						var cusInBondMoveHeaderMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("CusInBondMoveHeaderMessagesGrid");
						movementHeadersMessagesGrid.SelectSingleElement(moveHeader1);
						AssertEquals("Messages count of moveHeader1", 3, cusInBondMoveHeaderMessagesGrid.List.Count);
						movementHeadersMessagesGrid.SelectSingleElement(moveHeader2);
						AssertEquals("Messages count of moveHeader2", 1, cusInBondMoveHeaderMessagesGrid.List.Count);
					}
				}
			}
		}

		public void TestCusInBondMoveHeaderMessageDetailsTextBox_BillsofLadingTabPage()
		{
			var header = Factory.New<CusInBondHeader>();
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					header.Bills.DeleteAll();
					var bill = header.Bills.AddNew();
					var message1 = bill.Messages.AddNew(typeof(MQEDIMessage));
					message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message1.EM_MessageText = "B013307M34IS                                               5611                 " +
						"R1                                       ANZ                     0008 081211    " +
						"SC0860008 08121108657068233 0000100000                       08657068233        " +
						"Y  3307M34IS00002";
					var message2 = bill.Messages.AddNew(typeof(MQEDIMessage));
					message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message2.EM_MessageText = "B013307M34IS                                               5611                 " +
						"R1                                       ANZ                     0008 081211    " +
						"SC0860008 08121108657068233 0000100000                       09257068271        " +
						"Y  3307M34IS00002";
					var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
					var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
					headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
					var cusInBondMoveHeaderMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("CusInBondMoveHeaderMessagesGrid");
					var cusInBondMoveHeaderMessageDetailsTextBox = inBondMessageDetailsUserControl.FindSingle<ZTextBox>("CusInBondMoveHeaderMessageDetailsTextBox");
					cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message1);
					AssertContains("Inbond Number of message1", "08657068233", cusInBondMoveHeaderMessageDetailsTextBox.Text);
					cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message2);
					AssertContains("Inbond Number of message2", "09257068271", cusInBondMoveHeaderMessageDetailsTextBox.Text);
				}
			}
		}

		public void TestCusInBondMoveHeaderMessageDetailsTextBox_MovementHeadersTabPage()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var message1 = moveHeader.Messages.AddNew(typeof(MQEDIMessage));
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "B013307M34IS                                               5611                 " +
				"R1                                       ANZ                     0008 081211    " +
				"SC0860008 08121108657068233 0000100000                       08657068233        " +
				"Y  3307M34IS00002";
			var message2 = moveHeader.Messages.AddNew(typeof(MQEDIMessage));
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = "B013307M34IS                                               5611                 " +
				"R1                                       ANZ                     0008 081211    " +
				"SC0860008 08121108657068233 0000100000                       09257068271        " +
				"Y  3307M34IS00002";
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				foreach (var changingFromDifferentTab in new[] { false, true })
				{
					using (var form = GetTestForm(header, isJobDeclarationForm))
					{
						form.Show();
						var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
						var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
						if (changingFromDifferentTab)
						{
							headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
						}

						headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("MovementHeadersTabPage");
						var cusInBondMoveHeaderMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("CusInBondMoveHeaderMessagesGrid");
						var cusInBondMoveHeaderMessageDetailsTextBox = inBondMessageDetailsUserControl.FindSingle<ZTextBox>("CusInBondMoveHeaderMessageDetailsTextBox");
						cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message1);
						AssertContains("Inbond Number of message1", "08657068233", cusInBondMoveHeaderMessageDetailsTextBox.Text);
						cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message2);
						AssertContains("Inbond Number of message2", "09257068271", cusInBondMoveHeaderMessageDetailsTextBox.Text);
					}
				}
			}
		}

		public void TestCusInBondMoveHeaderMessageTextTextBox_BillsofLadingTabPage()
		{
			var header = Factory.New<CusInBondHeader>();
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				using (var form = GetTestForm(header, isJobDeclarationForm))
				{
					form.Show();
					header.Bills.DeleteAll();
					var bill = header.Bills.AddNew();
					var message1 = bill.Messages.AddNew(typeof(MQEDIMessage));
					message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message1.EM_MessageText = "B013307M34IS                                               5611                 " +
						"R1                                       ANZ                     0008 081211    " +
						"SC0860008 08121108657068233 0000100000                       08657068233        " +
						"Y  3307M34IS00002";
					var message2 = bill.Messages.AddNew(typeof(MQEDIMessage));
					message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message2.EM_MessageText = "B013307M34IS                                               5611                 " +
						"R1                                       ANZ                     0008 081211    " +
						"SC0860008 08121108657068233 0000100000                       09257068271        " +
						"Y  3307M34IS00002";
					var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
					var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
					headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
					var cusInBondMoveHeaderMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("CusInBondMoveHeaderMessagesGrid");
					var cusInBondMoveHeaderMessageTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("CusInBondMoveHeaderMessageTabControl");
					cusInBondMoveHeaderMessageTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("CusInBondMoveHeaderMessageTextTabPage");
					var cusInBondMoveHeaderMessageTextTextBox = inBondMessageDetailsUserControl.FindSingle<ZTextBox>("CusInBondMoveHeaderMessageTextTextBox");
					cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message1);
					AssertContains("Inbond Number of message1", "08657068233", cusInBondMoveHeaderMessageTextTextBox.Text);
					cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message2);
					AssertContains("Inbond Number of message2", "09257068271", cusInBondMoveHeaderMessageTextTextBox.Text);
				}
			}
		}

		public void TestCusInBondMoveHeaderMessageTextTextBox_MovementHeadersTabPage()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var message1 = moveHeader.Messages.AddNew(typeof(MQEDIMessage));
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "B013307M34IS                                               5611                 " +
				"R1                                       ANZ                     0008 081211    " +
				"SC0860008 08121108657068233 0000100000                       08657068233        " +
				"Y  3307M34IS00002";
			var message2 = moveHeader.Messages.AddNew(typeof(MQEDIMessage));
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = "B013307M34IS                                               5611                 " +
				"R1                                       ANZ                     0008 081211    " +
				"SC0860008 08121108657068233 0000100000                       09257068271        " +
				"Y  3307M34IS00002";
			foreach (var isJobDeclarationForm in new[] { true, false })
			{
				foreach (var changingFromDifferentTab in new[] { false, true })
				{
					using (var form = GetTestForm(header, isJobDeclarationForm))
					{
						form.Show();
						var inBondMessageDetailsUserControl = GetInBondMessageDetailsUserControl(form);
						var headersMessagesTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("HeadersMessagesTabControl");
						if (changingFromDifferentTab)
						{
							headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("BillsofLadingTabPage");
						}

						headersMessagesTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("MovementHeadersTabPage");
						var cusInBondMoveHeaderMessagesGrid = inBondMessageDetailsUserControl.FindSingle<ZGrid>("CusInBondMoveHeaderMessagesGrid");
						var cusInBondMoveHeaderMessageTabControl = inBondMessageDetailsUserControl.FindSingle<ZTabControl>("CusInBondMoveHeaderMessageTabControl");
						cusInBondMoveHeaderMessageTabControl.SelectedTab = inBondMessageDetailsUserControl.FindSingle<ZTabPage>("CusInBondMoveHeaderMessageTextTabPage");
						var cusInBondMoveHeaderMessageTextTextBox = inBondMessageDetailsUserControl.FindSingle<ZTextBox>("CusInBondMoveHeaderMessageTextTextBox");
						cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message1);
						AssertContains("Inbond Number of message1", "08657068233", cusInBondMoveHeaderMessageTextTextBox.Text);
						cusInBondMoveHeaderMessagesGrid.SelectSingleElement(message2);
						AssertContains("Inbond Number of message2", "09257068271", cusInBondMoveHeaderMessageTextTextBox.Text);
					}
				}
			}
		}
	}
}
