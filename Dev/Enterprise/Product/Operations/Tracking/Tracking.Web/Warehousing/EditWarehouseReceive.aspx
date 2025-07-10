<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" Codebehind="EditWarehouseReceive.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.EditWarehouseReceive" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
	<title>Add/Edit Warehouse Receipt</title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
	<meta content="C#" name="CODE_LANGUAGE">
	<meta content="JavaScript" name="vs_defaultClientScript">
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsReceiveLabel" runat="server" CssClass="PageTitle">New Warehouse Receipt</edi:ZTextLabel></div>
			<div id="UnauthorisedDiv" class="ContentSection" runat="server">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel></div>
			<div id="AuthorisedContent" class="ContentSection" runat="server">
				<div id="NotFoundError" runat="server">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server"></edi:ZTextLabel></div>
				<div id="ReceiveContents" runat="server">
					<div class="ContentSection">
						<table class="ResultsTable" id="ReceiveDetailsTable">
							<tbody>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="WarehouseLabel" runat="server" CssClass="DetailsItem">Warehouse:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZGuidDropDownList ID="WarehouseDropDown" runat="server" EmptyItemText="" ShowEmptyItem="True"
											HasChanges="False" DataValueField="PK" ValueFieldName="PK" DataTextField="WW_WarehouseName"
											BindToList="WhsReceive+Lookups.Warehouses" BindTo="WhsReceive.WD_WW_Whs">
										</edi:ZGuidDropDownList></td>
								</tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveRefLabel" runat="server" CssClass="DetailsItem">Receive Ref #</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextBox ID="ReceiveRef" runat="server" BindTo="WhsReceive.WD_ExternalReference"></edi:ZTextBox></td>
								</tr>
                                <tr>
                                    <td nowrap>
                                        <edi:ZTextLabel ID="CustomerRefLabel" runat="server" CssClass="DetailsItem">Customer Ref #</edi:ZTextLabel></td>
                                    <td nowrap>
                                        <edi:ZTextBox ID="CustomerRef" runat="server" BindTo="WhsReceive.WD_CustomerReference"></edi:ZTextBox></td>
                                </tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveStatusLabel" runat="server" CssClass="DetailsItem">Receipt Status:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextLabel ID="ReceiveStatus" runat="server" BindTo="StatusDesc"></edi:ZTextLabel></td>
								</tr>
                                <tr>
                                    <td nowrap>
                                        <edi:ZTextLabel ID="BookingDateLabel" runat="server" CssClass="DetailsItem">Booking Date:</edi:ZTextLabel></td>
                                    <td nowrap>
                                        <edi:ZDateEdit ID="BookingDate" runat="server" BindTo="WhsReceive.WD_BookingDate" />
                                    </td>
                                </tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="ETALabel" runat="server" CssClass="DetailsItem">ETA:</edi:ZTextLabel>
									</td>
									<td nowrap>
										<edi:ZDateEdit ID="ETA" runat="server" BindTo="WhsReceive.WD_ETA"></edi:ZDateEdit>
									</td>
								</tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="TotalUnitsLabel" runat="server" CssClass="DetailsItem">Total Units:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZNumericTextBox ID="TotalUnits" runat="server" BindTo="WhsReceive.WD_TotalUnits"></edi:ZNumericTextBox></td>
								</tr>
                                <tr>
                                    <td nowrap>
                                        <edi:ZTextLabel ID="TotalPalletsLabel" runat="server" CssClass="DetailsItem">Total Pallets:</edi:ZTextLabel></td>
                                    <td nowrap>
                                        <edi:ZNumericTextBox ID="TotalPallets" runat="server" BindTo="WhsReceive.WD_TotalPallets"></edi:ZNumericTextBox></td>
                                </tr>
							</tbody>
						</table>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
								<td nowrap>
									<edi:ZTextLabel ID="ReceiveContainersLabel" runat="server" CssClass="DetailsItem">Containers: </edi:ZTextLabel></td>
							</tr>
							<tr>
								<td nowrap>
									<edi:ZDataGrid ID="WhsReceiveContainersGrid" runat="server" CssClass="DetailsTable" BindTo="WhsReceive.Containers"
										AllowAdd="True" AllowDelete="True" AllowEdit="True" ShowFooter="False" AutoGenerateColumns="False">
										<PagerStyle Mode="NumericPages"></PagerStyle>
										<ItemStyle CssClass="DetailsCell"></ItemStyle>
										<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									</edi:ZDataGrid>
								</td>
							</tr>
						</table>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
								<td nowrap>
									<edi:ZTextLabel ID="ReceiveInventoryLabel" runat="server" CssClass="DetailsItem">Lines: </edi:ZTextLabel></td>
							</tr>
							<tr>
								<td nowrap>
									<edi:ZDataGrid ID="WhsReceiveInventoryGrid" runat="server" CssClass="DetailsTable" BindTo="Lines"
										AllowAdd="True" AllowDelete="True" AllowEdit="True" ShowFooter="False" AutoGenerateColumns="False">
										<PagerStyle Mode="NumericPages"></PagerStyle>
										<ItemStyle CssClass="DetailsCell"></ItemStyle>
										<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									</edi:ZDataGrid>
								</td>
							</tr>
						</table>
					</div>
					<div class="ContentSection">
						<asp:Button ID="SaveReceive" runat="server" Text="Update Receipt" onclick="SaveReceive_Click"></asp:Button>
						&nbsp;
						<asp:Button ID="CancelReceive" runat="server" Text="Cancel Receipt" onclick="CancelReceive_Click"></asp:Button>
					</div>
				</div>
			</div>
		</div>
		<div>
		</div>
	</form>
</body>
</html>
