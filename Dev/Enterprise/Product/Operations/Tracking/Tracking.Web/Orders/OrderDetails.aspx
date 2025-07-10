<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<%@ Page Language="c#" Codebehind="OrderDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Orders.OrderDetails" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Order Details</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="OrderNumberLabel" runat="server" CssClass="PageTitle">Order #</edi:ZTextLabel><edi:ZTextLabel
					ID="PageTitleLabel" runat="server" CssClass="PageTitle" BindTo="JD_OrderNumberAndSplit"></edi:ZTextLabel>&nbsp;</div>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
			</div>
			<div id="AuthorisedContent" runat="server">
				<div id="OrderContents" runat="server">
					<div id="OrderCancelledDiv" runat="server">
						<edi:ZTextLabel ID="OrderCancelledLabel" CssClass="SectionTitle" runat="server" ForeColor="Red">This order has been canceled</edi:ZTextLabel></div>
					<div id="EditButtonDiv" runat="server" class="ContentSection">
						<asp:Button ID="EditOrder" runat="server" Text="Edit Order" onclick="EditOrder_Click"></asp:Button>&nbsp;
						<asp:Button ID="CancelOrder" runat="server" Text="Cancel Order" onclick="CancelOrder_Click"></asp:Button>&nbsp;
			            <asp:button id="DuplicateOrder" runat="server" Text="Copy Order" onclick="DuplicateOrder_Click"></asp:button>&nbsp;
						<asp:Button ID="ViewShipmentDetailsButton" runat="server" Text="View Shipment Details" onclick="ViewShipmentDetailsButton_Click">
						</asp:Button>
					</div>
					<div style="display:block">
					<div class="ContentSection" style="display: inline;">
						<table class="ResultsTable" id="AuthArea1" runat="server" style="display: inline; float:left; vertical-align: top">
							<tbody>
								<tr id="SupplierRow">
									<td>
										<span class="DetailsItem">Supplier: </span>
									</td>
									<td>&nbsp;</td>
									<td>
										<edi:ZFindBoxLabel ID="SupplierLabel" runat="server" BindTo="SupplierPK" DisplayStyle="DescriptionOnly"
											BindToList="SupplierList"></edi:ZFindBoxLabel></td>
									<td style="width: 30px">
										&nbsp;</td>
								</tr>
								<tr id="PickupRow">
									<td>
										<span class="DetailsItem">Pickup From: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="PickupFromLabel" runat="server" BindTo="PickupAddressLine" /></td>
									<td style="width: 30px">
										&nbsp;</td>
								</tr>
								<tr><td>&nbsp;</td></tr>
								<tr id="BuyerRow">
									<td>
										<span class="DetailsItem">Buyer: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="BuyerLabel" runat="server" BindTo="Buyer.OH_FullName" /></td>
									<td style="width: 30px">
										&nbsp;</td>
								</tr>
								<tr id="DeliveryRow">
									<td>
										<span class="DetailsItem">Deliver To: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="DeliverToLabel" runat="server" BindTo="DeliverAddressLine" /></td>
									<td style="width: 30px">
										&nbsp;</td>
								</tr>
								<tr>
									<td>&nbsp;</td>
								</tr>
								<tr id="ControllingCustomerRow">
									<td>
										<span class="DetailsItem">Controlling Customer: </span>
									</td>
									<td>
										&nbsp;</td>
									<td colspan="5">
										<edi:ZTextLabel ID="ControllingCustomerLabel" runat="server" BindTo="ControllingCustomerDocAddress.E2_CompanyName" /></td>
									<td style="width: 30px">
										&nbsp;</td>
								</tr>
							</tbody>
						</table>
						<table class="ResultsTable" style="vertical-align: top">
							<tbody>
								<tr id="GoodsDescriptionRow">
									<td>
										<span class="DetailsItem">Good Description: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="GoodsDescLabel" runat="server" BindTo="JD_OrderGoodsDescription"></edi:ZTextLabel></td>
								</tr>
								<tr id="ConfirmationNumberRow">
									<td>
										<span class="DetailsItem">Confirmation No.: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="BookingConfRefLabel" runat="server" BindTo="JD_BookingConfRef"></edi:ZTextLabel></td>
								</tr>
								<tr>
									<td colspan="3">
										&nbsp;</td>
								</tr>
								<tr id="INCOTermsRow">
									<td>
										<span class="DetailsItem">INCO Terms: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="IncoTermsLabel" runat="server" BindTo="JD_IncoTerm"></edi:ZTextLabel></td>
								</tr>
								<tr id="AdditionalTermsRow">
									<td>
										<span class="DetailsItem">Additional Terms: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="AdditionalTermsLabel" runat="server" BindTo="JD_AdditionalTerms"></edi:ZTextLabel></td>
								</tr>
								<tr id="TransportModeRow">
									<td>
										<span class="DetailsItem">Transport Mode: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="TransportModeLabel" runat="server" BindTo="JD_TransportMode" /></td>
								</tr>
								<tr><td>&nbsp;</td></tr>
								<tr><td>&nbsp;</td></tr>
							</tbody>
						</table>
						<table class="ResultsTable" style="vertical-align: bottom">
							<tbody>
								<tr id="OrderDateRow">
									<td>
										<span class="DetailsItem">Order Date: </span>
									</td>
									<td>
										&nbsp;</td>
									<td colspan="5">
										<edi:ZDateTimeLabel ID="OrderDateLabel" runat="server" BindTo="JD_OrderDate"></edi:ZDateTimeLabel></td>
								</tr>
								<tr id="ExWorksRequiredByRow">
									<td>
										<span class="DetailsItem">Req. Ex Works: </span>
									</td>
									<td>
										&nbsp;</td>
									<td colspan="5">
										<edi:ZDateTimeLabel ID="ReqExWorks" runat="server" BindTo="JD_ExWorksRequiredBy" /></td>
								</tr>
								<tr id="DeliveryRequiredByRow">
									<td>
										<span class="DetailsItem">Req. In Store: </span>
									</td>
									<td>
										&nbsp;</td>
									<td colspan="5">
										<edi:ZDateTimeLabel ID="ReqInStore" runat="server" BindTo="JD_DeliveryRequiredBy" /></td>
								</tr>
							</tbody>
						</table>
					</div>
					</div>
					<div class="ContentSection" style="display: block;">
						<table class="ResultsTable">
						    <tbody>
						        <tr>
						            <td>
						                <edi:ZCollapsablePanel ID="AdditionalDetailPanel" runat="server" CssClass="SectionTitle" Label="Additional Details" DisableCollapsing="True">
    						                 <asp:Table ID="AdditionalDetailTable" runat="server" CssClass="DetailsTable" 
                                                HorizontalAlign="Left">
                                            </asp:Table>
					                    </edi:ZCollapsablePanel>
					                </td>
						        </tr>
						        <tr>
						            <td>
										<table border="0" cellpadding="0" cellspacing="0">
										<tr>
											<td align="left" valign="top">
												<edi_tracking:MilestonesControl id="Milestones" runat="server" CssClass="DetailsTable" BindTo="Milestones" ItemCssClass="DetailsCell" HeaderCssClass="DetailsHeader" DisplayInPanel="True"  PanelCssClass="SectionTitle">
												</edi_tracking:MilestonesControl>
											</td>
											<td align="left" valign="top">
												<edi_tracking:EventsControl id="TrackingEvents" runat="server" CssClass="DetailsTable" BindTo="TrackingEvents" ItemCssClass="DetailsCell" HeaderCssClass="DetailsHeader" DisplayInPanel="True"  PanelCssClass="SectionTitle">
												</edi_tracking:EventsControl>
											</td>
										</tr>
										</table>
						            </td>
						        </tr>
						    </tbody>
						</table>
					</div>
					<edi:zgrid ID="OrderLinesGrid" runat="server" CssClass="DetailsTable" BindTo="OrderLines" Caption="Order Lines" DisableCollapsing="True"
						AutoGenerateColumns="False" ShowFooter="False">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					
					<edi:ZCollapsablePanel ID="PlannedPacksPanel" runat="server" CssClass="SectionTitle" Label="Planning" DisableCollapsing="True">
					    <table class="ResultsTable">
							<tbody>
								<tr id="PlannedPacksRow">
									<td>
										<span class="DetailsItem">Packs:</span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="PlannedPacksLabel" runat="server" BindTo="JD_PacksWithUnits"></edi:ZTextLabel></td>
									<td>
										&nbsp;</td>
									<td>
										<span class="DetailsItem">Volume:</span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="PlannedVolumeLabel" runat="server" BindTo="JD_ActualVolumeWithUnits"></edi:ZTextLabel></td>
								</tr>
								<tr id="PlannedWeightRow">
								    <td colspan="4"></td>
									<td>
										<span class="DetailsItem">Weight: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="PlannedWeightLabel" runat="server" BindTo="JD_ActualWeightWithUnits"></edi:ZTextLabel></td>
								</tr>
							</tbody>
						</table>
					</edi:ZCollapsablePanel>

					<edi:ZCollapsablePanel ID="DetailsPanel" runat="server" CssClass="SectionTitle"
						Label="Planning" DisableCollapsing="True">
					<div style="display:inline">
						<table class="ResultsTable" style="float:left">
						<tr id="HouseBillRow">
							<td>
								<span class="DetailsItem">House Bill: </span>
							</td>
							<td>
								&nbsp;</td>
							<td>
								<edi:ZTextLabel ID="PlanningHouseBillLabel" runat="server" BindTo="JD_Waybill"></edi:ZTextLabel></td>
							<td style="width: 30px">
								&nbsp;</td>
						</tr>
						<tr id="MasterBillRow">
							<div id="AuthArea2" runat="server">
							<td>
								<span class="DetailsItem">Master Bill: </span>
							</td>
							<td>
								&nbsp;</td>
							<td>
								<edi:ZTextLabel ID="PlanningMasterBillLabel" runat="server" BindTo="JD_MasterWaybill"></edi:ZTextLabel></td>
							</div>
							<td>
								&nbsp;</td>
						</tr>
						<tr>
							<td colspan="4">
								&nbsp;</td>
						</tr>
						<tr id="OriginRow">
							<td>
								<span class="DetailsItem">Origin:</span></td>
							<td>
								&nbsp;</td>
							<td>
								<edi:ZCodeFindBoxLabel ID="ShipmentGoodsAvailableAtLabel" runat="server" BindTo="JD_RL_NKGoodsAvailableAt"
									DisplayStyle="DescriptionOnly" BindToList="JD_RL_List"></edi:ZCodeFindBoxLabel></td>
							<td>
								&nbsp;</td>
						</tr>
						<tr id="DestinationRow">
							<td>
								<span class="DetailsItem">Destination: </span>
							</td>
							<td>
								&nbsp;</td>
							<td>
								<edi:ZCodeFindBoxLabel ID="ShipmentGoodsDeliveredToLabel" runat="server" BindTo="JD_RL_NKGoodsDeliveredTo"
									DisplayStyle="DescriptionOnly" BindToList="JD_RL_List"></edi:ZCodeFindBoxLabel></td>
							<td>
								&nbsp;</td>
						</tr>
						<tr>
							<td colspan="4">
								&nbsp;</td>
						</tr>
						<tr id="PacksRow">
							<td>
								<span class="DetailsItem">Packs:</span>
							</td>
							<td>
								&nbsp;</td>
							<td>
								<edi:ZTextLabel ID="PacksLabel" runat="server" BindTo="ShipDecPacksWithUnits"></edi:ZTextLabel></td>
							<td>
								&nbsp;</td>
						</tr>
						<tr><td colspan="4"></td></tr>
						</table>
						<table class="ResultsTable">
							<tbody>
								<tr id="PortOfLoadingRow">
									<td>
										<span class="DetailsItem">Port of Loading: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZCodeFindBoxLabel ID="PortOfLoading" runat="server" BindTo="JD_RL_NKPortOfLoading"
											DisplayStyle="DescriptionOnly" BindToList="JD_RL_List"></edi:ZCodeFindBoxLabel></td>
								</tr>
								<tr id="PortOfDischargeRow">

									<td>
										<span class="DetailsItem">Port of Discharge: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZCodeFindBoxLabel ID="PortOfDischarge" runat="server" BindTo="JD_RL_NKPortOfDischarge"
											DisplayStyle="DescriptionOnly" BindToList="JD_RL_List"></edi:ZCodeFindBoxLabel></td>
								</tr>
								<tr>
									<td colspan="3">
										&nbsp;</td>
								</tr>
								<tr id="SendingAgentRow">
									<div id="SendingAgentArea" runat="server">
									<td>
										<span class="DetailsItem">Sending agent: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZFindBoxLabel ID="SendingAgent" runat="server" BindTo="JD_OH_SendingAgent" DisplayStyle="DescriptionOnly"
											BindToList="JD_OH_SendingAgents_List"></edi:ZFindBoxLabel></td>
									</div>
									<td>&nbsp;</td>
								</tr>
								<tr id="ReceivingAgentRow">
									<div id="ReceivingAgentArea" runat="server">
									<td>
										<span class="DetailsItem">Receiving agent: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZFindBoxLabel ID="ReceivingAgent" runat="server" BindTo="JD_OH_ReceivingAgent"
											DisplayStyle="DescriptionOnly" BindToList="JD_OH_ReceivingAgents_List"></edi:ZFindBoxLabel></td>
									</div>
									<td>&nbsp;</td>
								</tr>
								<tr>
									<td colspan="3">
										&nbsp;</td>
								</tr>
								<tr id="VolumeRow">
									<td>
										<span class="DetailsItem">Volume:</span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="VolumeLabel" runat="server" BindTo="ShipDecActualVolumeWithUnits"></edi:ZTextLabel></td>
								</tr>
								<tr id="WeightRow">
									<td>
										<span class="DetailsItem">Weight: </span>
									</td>
									<td>
										&nbsp;</td>
									<td>
										<edi:ZTextLabel ID="WeightLabel" runat="server" BindTo="ShipDecActualWeightWithUnits"></edi:ZTextLabel></td>
								</tr>
							</tbody>
						</table>
					</div>
					</edi:ZCollapsablePanel>
	                <edi:zgrid ID="PlannedContainersGrid" runat="server" CssClass="DetailsTable"
		                BindTo="PlannedContainers" AutoGenerateColumns="False" ShowFooter="False" Label="Planned Containers" DisableCollapsing="True">
		                <ItemStyle CssClass="DetailsCell" />
		                <HeaderStyle CssClass="DetailsHeader" />
                    </edi:zgrid>
	                <edi:zgrid ID="ContainersGrid" runat="server" CssClass="DetailsTable" BindTo="Shipment.Containers"
		                Label="Containers" DisableCollapsing="True" Visible="False">
		                <ItemStyle CssClass="DetailsCell" />
		                <HeaderStyle CssClass="DetailsHeader" />
	                </edi:zgrid>
			        <edi:zgrid ID="PlannedVoyagesGrid" runat="server" CssClass="DetailsTable" BindTo="PlannedVoyages" AutoGenerateColumns="False" ShowFooter="False" Label="Planned Voyages" DisableCollapsing="True">
		                <ItemStyle CssClass="DetailsCell"></ItemStyle>
		                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
	                </edi:zgrid>    						          
					<edi:ZCollapsablePanel ID="TransportPanel" runat="server" CssClass="SectionTitle"
						Label="Transport" DisableCollapsing="True" Visible="False">
						<edi:ZGrid ID="TransportGrid" runat="server" CssClass="DetailsTable" BindTo="Shipment.RelatedTransportsInLegOrder" DisableCollapsing="True">
							<ItemStyle CssClass="DetailsCell" />
							<HeaderStyle CssClass="DetailsHeader" />
						</edi:ZGrid>
						<div id="TimeLineLegendHolder">
							<br />
							<edi:ZTextLabel runat="server" ID="PendingLegendLabel" Text="Pending" CssClass="TimeLineLegend TimeLinePending" />
							&nbsp;
							<edi:ZTextLabel runat="server" ID="OverdueLegendLabel" Text="Overdue" CssClass="TimeLineLegend TimeLineOverdue" />
							&nbsp;
							<edi:ZTextLabel runat="server" ID="CompletedLegendLabel" Text="Completed" CssClass="TimeLineLegend TimeLineCompleted" />
							&nbsp;
							<edi:ZTextLabel runat="server" ID="CompletedLateLegendLabel" Text="Completed Late" CssClass="TimeLineLegend TimeLineOnIncompletedLate"/>
						</div>
					</edi:ZCollapsablePanel>
				    <edi:zgrid id="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="True">
					    <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
					    <ItemStyle CssClass="DetailsCell"></ItemStyle>
					    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				    </edi:zgrid>
                    <br />
                    <br />										
					<edi:ZCollapsablePanel ID="NotesPanel" runat="server" CssClass="SectionTitle" Label="Notes"
						DisableCollapsing="True">
						<edi:ZNotesControl ID="notes" runat="server" BindTo="NotesHelper.VisibleNotes">
						</edi:ZNotesControl>
					</edi:ZCollapsablePanel>
				</div>
				<div id="NotFoundError" class="ContentSection" runat="server">
					<edi:ZTextLabel ID="OrderNotFoundLabel" runat="server"></edi:ZTextLabel>
				</div>
			</div>
		</div>
	</form>
</body>
</html>
