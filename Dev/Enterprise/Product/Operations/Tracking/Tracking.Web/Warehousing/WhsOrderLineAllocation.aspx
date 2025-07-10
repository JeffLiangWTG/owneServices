<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WhsOrderLineAllocation.aspx.cs" Inherits="Enterprise.Tracking.Web.WhsOrderLineAllocation" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Order Line Allocation</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsOrderLabel" runat="server" CssClass="PageTitle">Warehouse Order Line Allocation</edi:ZTextLabel>
			</div>
			<div id="UnauthorisedDiv" class="ContentSection" runat="server">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server" />
			</div>
			<div id="AuthorisedContent" class="ContentSection" runat="server">
				<div id="NotFoundError" runat="server">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server" />
			    </div> 
				<div id="OrderLineContents" runat="server">
	                <edi:ZGuidFindBox ID="InventorySelector" runat="server" BindTo="" BindToList="WhsOrderLine+Lookups.PossibleInventory"   
	                    ModuleID="TrackingInventoryDetails" OnTextChanged="InventorySelected" AutoPostBack="true" style="visibility: hidden; height: 0px" />
					<div class="ContentSection">
						<table class="ResultsTable" id="OrderDetailsTable">
							<tbody>
                                <tr>
                                    <td class="DetailsItem" style="vertical-align: middle;">Product:</td>
                                    <td colspan="2">
                                        <edi:ZFindBoxLabel ID="Product" runat="server" BindTo="WhsOrderLine.WE_OP" BindToList="WhsOrderLine+Lookups.SupplierParts" DisplayStyle="CodeOnly" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td colspan="3" class="DetailsItem" style="vertical-align: middle;">
                                        <edi:ZTextLabel ID="ProductDescription" runat="server" BindTo="ProductDescription" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="DetailsItem" style="vertical-align: middle;">Packs:</td>
                                    <td style="text-align: right">
                                        <edi:ZNumericLabel ID="Packs" runat="server" BindTo="WhsOrderLine.WE_PackQuantity" />
                                    </td>
                                    <td>
                                        <edi:ZCodeLookupLabel ID="PacksUQ" runat="server" BindTo="WhsOrderLine.WE_F3_NKPackType" BindToList="WhsOrderLine+Lookups.PackTypes" DisplayStyle="DescriptionOnly" />
                                    </td>
				                    <td colspan="4"></td>
				                </tr>
				                <tr>
                                    <td class="DetailsItem" style="vertical-align: middle;">Quantity:</td>
                                    <td style="text-align: right">
                                        <edi:ZNumericLabel ID="Quantity" runat="server" BindTo="WhsOrderLine.WE_TransactionQuantity" BindToDecimals="WhsOrderLine+SupplierPart+OP_CountDecimalPlaces" />
                                    </td>
                                    <td style="vertical-align: middle;">
                                        <edi:ZCodeLookupLabel ID="UnitsUQ" runat="server" BindTo="WhsOrderLine.ProductUQ" BindToList="WhsOrderLine+Lookups.PackTypes" DisplayStyle="DescriptionOnly" />
                                    </td>
                                    <td></td>
                                    <td class="DetailsItem" style="vertical-align: middle;">Reserved Qty:</td>
                                    <td style="text-align: right">
                                        <edi:ZNumericLabel ID="AllocatedQuantity" runat="server" BindTo="WhsOrderLine.WE_CrossDockQuantity" BindToDecimals="WhsOrderLine+SupplierPart+OP_CountDecimalPlaces"/>
                                     </td>
                                    <td style="vertical-align: middle;">
                                        <edi:ZCodeLookupLabel ID="UnitsUQ12" runat="server" BindTo="WhsOrderLine.ProductUQ" BindToList="WhsOrderLine+Lookups.PackTypes"  DisplayStyle="DescriptionOnly" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="DetailsItem" style="vertical-align: middle;">Shortfall Qty:</td>
                                    <td style="text-align: right">
                                        <edi:ZNumericLabel ID="ShortFallQuantity" runat="server" BindTo="WhsOrderLine.WE_ShortfallQuantityCached" BindToDecimals="WhsOrderLine+SupplierPart+OP_CountDecimalPlaces" />
                                    </td>
                                    <td style="vertical-align: middle;">
                                        <edi:ZCodeLookupLabel ID="UnitsUQ1" runat="server" BindTo="WhsOrderLine.ProductUQ" BindToList="WhsOrderLine+Lookups.PackTypes"  DisplayStyle="DescriptionOnly" />
                                    </td>
                                    <td></td>
                                    <td class="DetailsItem" style="vertical-align: middle;">Unallocated Qty:</td>
                                    <td style="text-align: right">
                                        <edi:ZNumericLabel ID="UnallocatedQuantity" runat="server" BindTo="UnallocatedQuantity" BindToDecimals="WhsOrderLine+SupplierPart+OP_CountDecimalPlaces" />
                                    </td>
                                    <td style="vertical-align: middle;">
                                        <edi:ZCodeLookupLabel ID="UnitsUQ13" runat="server" BindTo="WhsOrderLine.ProductUQ" BindToList="WhsOrderLine+Lookups.PackTypes"  DisplayStyle="DescriptionOnly" />
                                    </td>
                                </tr>
                                <tr id="CustomAttr12Row" runat="server" >
                                    <td class="DetailsItem" style="vertical-align: middle;">
                                        <asp:Label id="CustomAttr1Label" runat="server" />
                                    </td>
                                    <td>
                                        <edi:ZTextLabel ID="CustomAttr1" runat="server" BindTo="WhsOrderLine.WE_PartAttrib1" />
                                    </td>
                                    <td colspan="2"></td>
                                    <td class="DetailsItem" style="vertical-align: middle;">
                                        <asp:Label id="CustomAttr2Label" runat="server" />
                                    </td>
                                    <td colspan="2">
                                        <edi:ZTextLabel id="CustomAttr2" runat="server" BindTo="WhsOrderLine.WE_PartAttrib2" />
                                    </td>
                                </tr>
                                <tr id="CustomAttr3Row" runat="server" >
                                    <td class="DetailsItem" style="vertical-align: middle;">
                                        <asp:Label id="CustomAttr3Label" runat="server" />
                                    </td>
                                    <td>
                                        <edi:ZTextLabel ID="CustomAttr3" runat="server" BindTo="WhsOrderLine.WE_PartAttrib3" />
                                    </td>
                                    <td colspan="2"></td>
                                    <td class="DetailsItem" style="vertical-align: middle;">
                                        <asp:Label id="SerialNumberLabel" runat="server" />
                                    </td>
                                    <td colspan="2">
                                        <edi:ZTextLabel id="SerialNumber" runat="server" BindTo="WhsOrderLine.WE_SerialNumber" />
                                    </td>
                                </tr>
							</tbody>
						</table>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
								<td><edi:ZTextLabel ID="WhsOrderLineCrossDocks" runat="server" CssClass="DetailsItem">Order Line Cross Docks: </edi:ZTextLabel></td>
							</tr>
							<tr>
								<td>
                                    <edi:ZDataGrid ID="CrossDocksGrid" runat="server" CssClass="DetailsTable" BindTo="WhsOrderLine.ReservedPickLines" AllowAdd="false" ShowFooter="false" />
								</td>
							</tr>
							<tr id="AttachButtonRow" runat="server">
							    <td>
							        <asp:Button ID="AttachInventory" runat="server" Text="Attach" />
						        </td>
							</tr>
						</table>
					</div>
					<div class="ContentSection" id="CloseButtonDiv" runat="server">
						<asp:Button ID="CloseWindow" runat="server" Text="Close Window" OnClick="Save_Click"></asp:Button>
					</div>
				</div>
			</div>
        </div>
    </form>
</body>
</html>
