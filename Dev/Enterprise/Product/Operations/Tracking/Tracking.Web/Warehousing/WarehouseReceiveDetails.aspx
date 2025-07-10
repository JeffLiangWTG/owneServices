<%@ Page Language="c#" Codebehind="WarehouseReceiveDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.WarehouseReceiveDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Warehouse Receipt Details</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsReceiveLabel" CssClass="PageTitle" runat="server">Warehouse Receipt</edi:ZTextLabel></div>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
			</div>
			<div id="AuthorisedContent" runat="server">
				<div id="EditButtonDiv" class="ContentSection" runat="server">
					<asp:Button ID="EditReceive" runat="server" Text="Edit Receipt" onclick="EditReceive_Click"></asp:Button>&nbsp;
					<asp:Button ID="CancelReceive" runat="server" Text="Cancel Receipt" onclick="CancelReceive_Click"></asp:Button>&nbsp;
					<asp:button id="DuplicateReceive" runat="server" Text="Copy Receipt" onclick="DuplicateReceive_Click"></asp:button>&nbsp;<br/>
					<edi:ZTextLabel ID="CanEditCancelLabel" runat="server"></edi:ZTextLabel>
				</div>
				<div id="NotFoundError" runat="server" class="ContentSection">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="DetailsItem"></edi:ZTextLabel></div>
				<div class="ContentSection" id="ReceiveContents" runat="server">
					<table class="ResultsTable" id="ReceiveDetailsTable">
						<tbody>
								<tr id="WarehouseRow">
									<td nowrap>
										<edi:ZTextLabel ID="WarehouseLabel" runat="server" CssClass="DetailsItem">Warehouse:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZFindBoxLabel ID="WarehouseDropDown" runat="server" 
											BindToList="WhsReceive+Lookups.Warehouses" BindTo="WhsReceive.WD_WW_Whs" displayStyle="DescriptionOnly">
										</edi:ZFindBoxLabel></td>
								</tr>
								<tr id="ReceiveRefNumberRow">
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveRefLabel" runat="server" CssClass="DetailsItem">Receive Ref #</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveRef" runat="server" BindTo="WhsReceive.WD_ExternalReference"></edi:ZTextLabel></td>
								</tr>
                                <tr id="CustomerRefNumberRow">
                                    <td nowrap>
                                        <edi:ZTextLabel ID="CustomerRefLabel" runat="server" CssClass="DetailsItem">Customer Ref #</edi:ZTextLabel></td>
                                    <td nowrap>
                                        <edi:ZTextLabel ID="CustomerRef" runat="server" BindTo="WhsReceive.WD_CustomerReference"></edi:ZTextLabel></td>
                                </tr>
								<tr id="ReceiptStatusRow">
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveStatusLabel" runat="server" CssClass="DetailsItem">Receipt Status:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveStatus" runat="server" BindTo="StatusDesc"></edi:ZTextLabel></td>
								</tr>
                                <tr id="BookingDateRow">
                                    <td nowrap>
                                        <edi:ZTextLabel ID="BookingDateLabel" runat="server" CssClass="DetailsItem">Booking Date:</edi:ZTextLabel></td>
                                    <td nowrap>
                                        <edi:ZDateTimeLabel ID="BookingDate" runat="server" BindTo="WhsReceive.WD_BookingDate" />
                                    </td>
                                </tr>
                 
                 <!-- ETA -->               
                 <tr id="ETARow">
									<td nowrap>
										<edi:ZTextLabel ID="ETALabel" runat="server" CssClass="DetailsItem">ETA:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZDateTimeLabel ID="ETA" runat="server" BindTo="WhsReceive.WD_ETA"></edi:ZDateTimeLabel>
									</td>
								</tr>

								<tr id="ArrivalDateRow">
									<td nowrap>
										<edi:ZTextLabel ID="ArrivalDateLabel" runat="server" CssClass="DetailsItem">Arrival Date:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZDateTimeLabel ID="ArrivalDate" runat="server" BindTo="WhsReceive.WD_ArrivalDate"></edi:ZDateTimeLabel>
									</td>
								</tr>
								<tr id="TotalUnitsRow">
									<td nowrap>
										<edi:ZTextLabel ID="TotalUnitsLabel" runat="server" CssClass="DetailsItem">Total Units:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZNumericLabel ID="TotalUnits" runat="server" BindTo="WhsReceive.WD_TotalUnits"></edi:ZNumericLabel></td>
								</tr>
                                <tr id="TotalPalletsRow">
                                    <td nowrap>
                                        <edi:ZTextLabel ID="TotalPalletsLabel" runat="server" CssClass="DetailsItem">Total Pallets:</edi:ZTextLabel></td>
                                    <td nowrap>
                                        <edi:ZNumericLabel ID="TotalPallets" runat="server" BindTo="WhsReceive.WD_TotalPallets"></edi:ZNumericLabel></td>
                                </tr>
						</tbody>
					</table>
				</div>
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
				<edi:ZGrid ID="WhsReceiveContainersGrid" runat="server" CssClass="DetailsTable" BindTo="WhsReceive.Containers"
					ShowFooter="False" DisableCollapsing="True" Label="Containers">
					<PagerStyle Mode="NumericPages"></PagerStyle>
					<ItemStyle CssClass="DetailsCell"></ItemStyle>
					<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				</edi:ZGrid>
				<edi:ZGrid ID="WhsReceiveInventoryGrid" runat="server" CssClass="DetailsTable" BindTo="Lines"
					ShowFooter="False" DisableCollapsing="True" Label="Lines">
					<PagerStyle Mode="NumericPages"></PagerStyle>
					<ItemStyle CssClass="DetailsCell"></ItemStyle>
					<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				</edi:ZGrid>
				<edi:ZCollapsablePanel ID="AdditionalDetailPanel" runat="server" CssClass="SectionTitle" Label="Additional Details" DisableCollapsing="True">
                    <asp:Table ID="AdditionalDetailTable" runat="server" CssClass="DetailsTable" HorizontalAlign="Left" />                
				</edi:ZCollapsablePanel>
			    <edi:ZGrid id="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="True">
				    <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
				    <ItemStyle CssClass="DetailsCell"></ItemStyle>
				    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
			    </edi:ZGrid>
				<edi:ZGrid ID="InvoicesGrid" runat="server" CssClass="DetailsTable" BindTo="InvoiceLoader.Transactions"
					DisableCollapsing="True" Label="Related Invoices">
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
    </form>
</body>
</html>
