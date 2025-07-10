<%@ Page Language="c#" Codebehind="WarehouseOrderDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.WarehouseOrderDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Warehouse Order Details</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsOrderLabel" CssClass="PageTitle" runat="server">Warehouse Order</edi:ZTextLabel></div>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
			</div>
			<div id="AuthorisedContent" runat="server">
				<div id="EditButtonDiv" class="ContentSection" runat="server">
					<asp:Button ID="EditOrder" runat="server" Text="Edit Order" onclick="EditOrder_Click"></asp:Button>&nbsp;
					<asp:Button ID="CancelOrder" runat="server" Text="Cancel Order" onclick="CancelOrder_Click"></asp:Button>&nbsp;
					<asp:button id="DuplicateOrder" runat="server" Text="Copy Order" onclick="DuplicateOrder_Click"></asp:button>&nbsp;<br/>
					<edi:ZTextLabel ID="CanEditCancelLabel" runat="server"></edi:ZTextLabel>
				</div>
				<div id="NotFoundError" runat="server" class="ContentSection">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="DetailsItem"></edi:ZTextLabel></div>
				<div class="ContentSection" id="OrderContents" runat="server">
					<table class="ResultsTable" id="OrderDetailsTable">
						<tbody>
							<tr id="WarehouseRow">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="WarehouseLabel" runat="server" CssClass="DetailsItem">Warehouse:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
									<edi:ZFindBoxLabel ID="Warehouse" runat="server" BindToList="WhsOrder+Lookups.Warehouses"
										BindTo="WhsOrder.WD_WW_Whs" DisplayStyle="DescriptionOnly"></edi:ZFindBoxLabel></td>
							</tr>
							<tr id="OrderNumberRow">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="OrderNumberLabel" runat="server" CssClass="DetailsItem">Order Number:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="OrderNumber" runat="server" BindTo="WhsOrder.WD_ExternalReference"></edi:ZTextLabel></td>
							</tr>
							<tr id="CustomerRefRow">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="CustomerRefLabel" runat="server" CssClass="DetailsItem">Customer Ref.:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="CustomerRef" runat="server" BindTo="WhsOrder.WD_CustomerReference"></edi:ZTextLabel></td>
							</tr>							
							<tr id="OrderStatusRow">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="OrderStatusLabel" runat="server" CssClass="DetailsItem">Order Status:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
									<div runat="server" id="OrderStatusControls">
										<edi:ZTextLabel ID="OrderStatus" runat="server" BindTo="StatusDesc"></edi:ZTextLabel></div>
								</td>
							</tr>
							<tr id="RequiredDateRow">
								<td style="height: 21px" nowrap="nowrap">
									<edi:ZTextLabel ID="RequiredDateLabel" runat="server" CssClass="DetailsItem">Required Date:</edi:ZTextLabel></td>
								<td style="height: 21px" nowrap="nowrap">
									<edi:ZDateTimeLabel ID="RequiredByDate" runat="server" BindTo="TrackingRequiredDate"></edi:ZDateTimeLabel></td>
							</tr>
							<tr id="TotalUnitsRow">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="TotalUnitsLabel" runat="server" CssClass="DetailsItem">Total Units:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
									<edi:ZNumericLabel ID="TotalUnits" runat="server" BindTo="WhsOrder.WD_TotalUnits"></edi:ZNumericLabel></td>
							</tr>

							<tr id="CneeArea" runat="server">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="ConsigneeLabel" runat="server" CssClass="DetailsItem">Consignee:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
										<edi:ZTextLabel id="ConsigneeTextLabel" runat="server" BindTo="WhsOrder.ConsigneeDocAddress.E2_CompanyName"></edi:ZTextLabel>
								</td>
							</tr>
							<tr id="CneeAddressArea" runat="server">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="Ztextlabel1" runat="server" CssClass="DetailsItem">Consignee Address:</edi:ZTextLabel></td>
								<td nowrap="nowrap">
										<edi:ZTextLabel id="ConsigneeAddressTestLabel" runat="server" BindTo="WhsOrder.ConsigneeDocAddress.AddressSummary"></edi:ZTextLabel>
								</td>
							</tr>

							<!-- Transport Co. -->							
							<tr id="TransportCoArea" runat="server">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="TransportCoLabel" runat="server" CssClass="DetailsItem">Transport Co.:</edi:ZTextLabel>
								</td>
								<td nowrap="nowrap">
										<edi:ZTextLabel id="TransportCo" runat="server" BindTo="WhsOrder.TransportCoDocAddress.E2_CompanyName"></edi:ZTextLabel>
								</td>
							</tr>

							<!-- Transport Ref. -->							
							<tr id="TransportRefRow">
								<td nowrap="nowrap">
									<edi:ZTextLabel ID="TransportRefLabel" runat="server" CssClass="DetailsItem">Transport Ref.:</edi:ZTextLabel>
								</td>
								<td nowrap="nowrap">
										<edi:ZHyperlink id="TransportRef" runat="server" BindTo="WhsOrder.WD_TransportReference"></edi:ZHyperlink>
								</td>
							</tr>
							
						</tbody>
					</table>
					<edi:ZGrid ID="WhsOrderReferencesGrid" runat="server" CssClass="DetailsTable"
						BindTo="WhsOrder.References" AllowAdd="False" AllowDelete="False" AllowEdit="False" ShowFooter="False"
						DisableCollapsing="True" Caption="References: ">
						<PagerStyle Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
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
					<div class="ContentSection" runat="server" id="WhsOrderLineSummaryContainer">
						<table class="ResultsTable">
							<tbody>
								<tr>
									<td nowrap="nowrap" colspan="2">
										<edi:ZTextLabel ID="WhsOrderLineSummary" runat="server" CssClass="DetailsItem">Order Lines Summary: </edi:ZTextLabel>
										<asp:HyperLink id="ShowLinesDetail" runat="server" Text="Show Details" />
									</td>
								</tr>
								<tr>
									<td nowrap="nowrap" colspan="2">
										<edi:ZGrid ID="WhsOrderSummaryLinesGrid" runat="server" CssClass="DetailsTable" BindTo="SummaryLines"
											DisableCollapsing="True" ShowFooter="False" AllowEdit="False" AllowDelete="False"
											AllowAdd="False">
											<PagerStyle Mode="NumericPages"></PagerStyle>
											<ItemStyle CssClass="DetailsCell"></ItemStyle>
											<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
										</edi:ZGrid>
									</td>
								</tr>
							</tbody>
						</table>					
					</div>
					<div class="ContentSection" runat="server" id="WhsOrderLineDetailsContainer">					
						<table class="ResultsTable">
							<tbody>
								<tr>
									<td nowrap="nowrap" colspan="2">
										<edi:ZTextLabel ID="WhsOrderLineDetails" runat="server" CssClass="DetailsItem">Order Lines: </edi:ZTextLabel>
										<asp:HyperLink id="ShowLinesSummary" runat="server" Text="Show Summary" />										
									</td>
								</tr>
								<tr>
									<td nowrap="nowrap" colspan="2">
										<edi:ZGrid ID="WhsOrderLinesGrid" runat="server" CssClass="DetailsTable" BindTo="Lines"
											DisableCollapsing="True" ShowFooter="False" AllowEdit="False" AllowDelete="False"
											AllowAdd="False">
											<PagerStyle Mode="NumericPages"></PagerStyle>
											<ItemStyle CssClass="DetailsCell"></ItemStyle>
											<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
										</edi:ZGrid>
									</td>
								</tr>
							</tbody>
						</table>
					</div>
					<edi:ZCollapsablePanel ID="AdditionalDetailPanel" runat="server" CssClass="SectionTitle" Label="Additional Details" DisableCollapsing="True">
                        <asp:Table ID="AdditionalDetailTable" runat="server" CssClass="DetailsTable" HorizontalAlign="Left" />                
				    </edi:ZCollapsablePanel>
				    <edi:ZGrid id="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="True">
					    <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
					    <ItemStyle CssClass="DetailsCell"></ItemStyle>
					    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				    </edi:ZGrid>
					<edi:ZGrid ID="ChargesGrid" runat="server" CssClass="DetailsTable" BindTo="InvoiceLoader.Transactions"
						DisableCollapsing="true" Caption="Related Invoices">
						<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
					<edi:ZCollapsablePanel ID="NotesPanel" runat="server" CssClass="SectionTitle" Label="Notes"
						DisableCollapsing="True">
						<edi:ZNotesControl ID="notes" runat="server" BindTo="NotesHelper.VisibleNotes">
						</edi:ZNotesControl>
					</edi:ZCollapsablePanel>
				</div>
			</div>
		</div>
	</form>
</body>
</html>
