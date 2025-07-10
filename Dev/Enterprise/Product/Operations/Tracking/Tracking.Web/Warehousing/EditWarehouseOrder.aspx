<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" Codebehind="EditWarehouseOrder.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.EditWarehouseOrder" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Add/Edit Warehouse Order</title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
	<meta content="C#" name="CODE_LANGUAGE">
	<meta content="JavaScript" name="vs_defaultClientScript">
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsOrderLabel" runat="server" CssClass="PageTitle">New Warehouse Order</edi:ZTextLabel></div>
			<div id="UnauthorisedDiv" class="ContentSection" runat="server">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel></div>
			<div id="AuthorisedContent" class="ContentSection" runat="server">
				<div id="NotFoundError" runat="server">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server"></edi:ZTextLabel></div>
				<div id="OrderContents" runat="server">
					<div class="ContentSection">
						<table class="ResultsTable" id="OrderDetailsTable">
							<tbody>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="WarehouseLabel" runat="server" CssClass="DetailsItem">Warehouse:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZGuidDropDownList ID="WarehouseDropDown" runat="server" EmptyItemText="" ShowEmptyItem="True"
											HasChanges="False" DataValueField="PK" ValueFieldName="PK" DataTextField="WW_WarehouseName"
											BindToList="WhsOrder+Lookups.Warehouses" BindTo="WhsOrder.WD_WW_Whs">
										</edi:ZGuidDropDownList></td>
								</tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="OrderNumberLabel" runat="server" CssClass="DetailsItem">Order Number:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextBox ID="OrderNumber" runat="server" BindTo="WhsOrder.WD_ExternalReference"></edi:ZTextBox></td>
								</tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="CustomerRefLabel" runat="server" CssClass="DetailsItem">Customer Ref.:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextBox ID="CustomerRef" runat="server" BindTo="WhsOrder.WD_CustomerReference"></edi:ZTextBox></td>
								</tr>								
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="OrderStatusLabel" runat="server" CssClass="DetailsItem">Order Status:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZTextLabel ID="OrderStatus" runat="server" BindTo="StatusDesc"></edi:ZTextLabel></td>
								</tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="RequiredDateLabel" runat="server" CssClass="DetailsItem">Required Date:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZDateEdit ID="RequiredByDate" runat="server" BindTo="TrackingRequiredDate"></edi:ZDateEdit>
									</td>
								</tr>
								<tr>
									<td nowrap>
										<edi:ZTextLabel ID="TotalUnitsLabel" runat="server" CssClass="DetailsItem">Total Units:</edi:ZTextLabel></td>
									<td nowrap>
										<edi:ZNumericTextBox ID="TotalUnits" runat="server" BindTo="WhsOrder.WD_TotalUnits"></edi:ZNumericTextBox></td>
								</tr>
								<tr>
									<td colspan="2" nowrap>
                                        <edi:ZDocAddressWebControl ID="ConsigneeAddress" runat="server" BindTo="WhsOrder.ConsigneeDocAddress"  Caption="Consignee" SaveCheckboxCaption="Save Delivery Address" AddressCaption="Delivery Address" ContactCaption="Consignee Contact" ResidentialCheckboxVisible="true" IsConsignee="true" IsConsignor="false"/>
	                                </td>
									<td colspan="2" nowrap>
                                        <edi:ZDocAddressWebControl ID="GoodsBilledToDocAddress" runat="server" BindTo="WhsOrder.GoodsBillToDocAddress"  Caption="Goods Billed To" SaveCheckboxCaption="Save Goods Billed To Address" IsOptional="true"/>
	                                </td>	                                
								</tr>
							</tbody>
						</table>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
								<td nowrap>
								    <asp:UpdatePanel ID="OrderReferencesUpdatePanel" runat="server" UpdateMode="Conditional">
								    <ContentTemplate>
									    <edi:ZDataGrid ID="WhsOrderReferencesGrid" runat="server" Caption="References" CssClass="DetailsTable"
										    BindTo="WhsOrder.References" AllowAdd="True" AllowDelete="True" AllowEdit="True" ShowFooter="False"
										    AutoGenerateColumns="False">
										    <PagerStyle Mode="NumericPages"></PagerStyle>
										    <ItemStyle CssClass="DetailsCell"></ItemStyle>
										    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									    </edi:ZDataGrid>
								    </ContentTemplate>
								    </asp:UpdatePanel>									    
							    </td>
							</tr>
						</table>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
								<td nowrap>
								    <asp:UpdatePanel ID="OrderLinesUpdatePanel" runat="server" UpdateMode="Conditional">
								    <ContentTemplate>
									    <edi:ZDataGrid ID="WhsOrderLinesGrid" runat="server" Caption="Order Lines" CssClass="DetailsTable" BindTo="Lines"
										    AllowAdd="True" AllowDelete="True" AllowEdit="True" ShowFooter="False" AutoGenerateColumns="False">
										    <PagerStyle Mode="NumericPages"></PagerStyle>
										    <ItemStyle CssClass="DetailsCell"></ItemStyle>
										    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									    </edi:ZDataGrid>
								    </ContentTemplate>
								    </asp:UpdatePanel>									    
								</td>
							</tr>
						</table>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
								<td><span class="DetailsItem">Special instructions:</span></td>
							</tr>
							<tr>
								<td><edi:ZTextBox ID="SpecialInstructions" runat="server" Width="550px" Rows="4" 
								        BindTo="UserEditableNoteHelper.EditableNoteText" TextMode="MultiLine"></edi:ZTextBox></td>
							</tr>
						</table>
					</div>
					<div class="ContentSection">
						<asp:Button ID="SaveOrder" runat="server" Text="Update Order" onclick="SaveOrder_Click"></asp:Button>
						&nbsp;
						<asp:Button ID="CancelOrder" runat="server" Text="Cancel Order" onclick="CancelOrder_Click"></asp:Button>
						<table class="ResultsTable">
                            <tr>
                                <td>
            					    <edi:ZTextLabel ID="SaveErrorMessage" runat="server" CssClass="ErrorMessage" Visible="false" EnableHtmlEncoding="True" />
                                </td>
                            </tr>
						</table>
					</div>
				</div>
			</div>
		</div>
		<div>
		</div>
	</form>
</body>
</html>
