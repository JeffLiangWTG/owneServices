<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InventoryDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.InventoryDetails" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>Edit Warehouse Inventory</title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
    <style type="text/css">
        .style1 {
            height: 23px;
        }
    </style>
</head>
<body id="DefaultBody" runat="server">
    <form runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ZTextLabel ID="WhsOrderLabel" runat="server" CssClass="PageTitle">Warehouse Inventory Details</edi:ZTextLabel>
            </div>
            <div id="UnauthorisedDiv" class="ContentSection" runat="server">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server" />
            </div>
            <div id="AuthorisedContent" class="ContentSection" runat="server">
                <div id="NotFoundError" runat="server">
                    <edi:ZTextLabel ID="NotFoundLabel" runat="server" />
                </div>
                <div id="InventoryContents" runat="server" class="ContentSection">
                    <table class="ResultsTable">
                        <tbody>
                            <tr id="WarehouseRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Warehouse:</span>
                                </td>
                                <td nowrap="nowrap" colspan="5">
                                    <edi:ZFindBoxLabel runat="server" ID="WarehouseLabel" BindToList="Lookups.Warehouses" BindTo="WI_WW_Whs" DisplayStyle="DescriptionOnly" />
                                </td>
                            </tr>
                            <tr id="ProductRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Product:</span> </td>
                                <td nowrap="nowrap" colspan="5">
                                    <edi:ZHyperlink ID="ProductLink" runat="server" BindTo="ProductCode" />
                                </td>
                            </tr>
                            <tr id="CommodityRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Commodity:</span>
                                </td>
                                <td nowrap="nowrap" colspan="5">
                                    <edi:ZCodeFindBoxLabel ID="CommodityLabel" runat="server" BindTo="CommodityCode" BindToList="SupplierPart+Lookups+CommodityCodes"
                                        DisplayStyle="DescriptionOnly" />
                                </td>
                            </tr>
                            <tr id="InventoryStatusRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Current Status:</span>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZCodeLookupLabel runat="server" ID="StatusLabel" BindTo="WI_InventoryStatus" BindToList="Lookups+InventoryStatuses" DisplayStyle="DescriptionOnly" />
                                </td>
                                <td>&nbsp;</td>
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Receipt Quantity:</span>
                                </td>
                                <td nowrap="nowrap" align="right">
                                    <edi:ZNumericLabel runat="server" ID="ReceiptQuantityLabel" BindTo="WI_InDocketLineUnits" BindToDecimals="SupplierPart.OP_CountDecimalPlaces" />
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZCodeLookupLabel runat="server" BindTo="WI_UnitsUQ" BindToList="Lookups+PackTypesWithStandardUnits" DisplayStyle="DescriptionOnly" ID="UQLabel1" />
                                </td>
                            </tr>
                            <tr id="ArrivalDateRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Arrival Date:</span>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZDateTimeLabel ID="ArrivalDateLabel" runat="server" BindTo="WI_ArrivalDate" />
                                </td>
                                <td>&nbsp;</td>
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Total Quantity:</span>
                                </td>
                                <td nowrap="nowrap" align="right">
                                    <edi:ZNumericLabel runat="server" BindTo="WI_TotalUnits" ID="TotalQuantityLabel" BindToDecimals="SupplierPart.OP_CountDecimalPlaces" />
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZCodeLookupLabel runat="server" BindTo="WI_UnitsUQ" BindToList="Lookups+PackTypesWithStandardUnits" DisplayStyle="DescriptionOnly" ID="TotalQuantityUQLabel" />
                                </td>
                            </tr>
                            <tr id="ReceiptRefRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Receipt Ref:</span>
                                </td>
                                <td>
                                    <edi:ZHyperlink ID="ReceiptRefLink" runat="server" BindTo="ReceiptReference" />
                                    <edi:ZTextLabel ID="DocketRef" runat="server" BindTo="ReceiptReference" />
                                </td>
                                <td>&nbsp;</td>
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Reserved Quantity:</span>
                                </td>
                                <td nowrap="nowrap" align="right">
                                    <edi:ZNumericLabel runat="server" BindTo="WI_CrossDockQuantity" BindToDecimals="SupplierPart.OP_CountDecimalPlaces"
                                        ID="AllocatedQuantityLabel" />
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZCodeLookupLabel runat="server" BindTo="WI_UnitsUQ" BindToList="Lookups+PackTypesWithStandardUnits" DisplayStyle="DescriptionOnly" ID="UQLabel3" />
                                </td>
                            </tr>
                            <tr id="LocationRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Location:</span>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZTextLabel runat="server" ID="LocationLabel" BindTo="CurrentLocationString" />
                                </td>
                                <td>&nbsp;</td>
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Committed Quantity:</span>
                                </td>
                                <td nowrap="nowrap" align="right">
                                    <edi:ZNumericLabel runat="server" BindTo="InternalsProxy+CommittedToTransactionQuantity" BindToDecimals="SupplierPart.OP_CountDecimalPlaces"
                                        ID="CommittedQuantityLabel" />
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZCodeLookupLabel runat="server" BindTo="WI_UnitsUQ" BindToList="Lookups+PackTypesWithStandardUnits" DisplayStyle="DescriptionOnly" ID="UQLabel4" />
                                </td>
                            </tr>
                            <tr id="LocationAreaRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Area/Type:</span>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZTextLabel ID="AreaTypeLabel" runat="server" BindTo="LocationAreaName" />
                                </td>
                                <td>&nbsp;</td>
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Available Pick Quantity:</span>
                                </td>
                                <td nowrap="nowrap" align="right">
                                    <edi:ZNumericLabel runat="server" BindTo="WI_AvailableToPickQuantity" BindToDecimals="SupplierPart.OP_CountDecimalPlaces"
                                        ID="AvailablePickQuantityLabel" />
                                </td>
                                <td nowrap="nowrap">
                                    <edi:ZCodeLookupLabel runat="server" runat="server" BindTo="WI_UnitsUQ" BindToList="Lookups+PackTypesWithStandardUnits" DisplayStyle="DescriptionOnly" ID="UQLabel5" />
                                </td>
                            </tr>
                            <tr id="PickAllocationRow">
                                <td nowrap="nowrap">
                                    <span class="DetailsItem">Pick Allocation:</span>
                                </td>
                                <td nowrap="nowrap" colspan="5">
                                    <edi:ZTextLabel runat="server" BindTo="PickAllocationsAsString" ID="ZTextLabel1" />
                                </td>
                            </tr>
                            <tr id="CustomAttr12Row" runat="server">
                                <td class="DetailsItem" style="vertical-align: middle;">
                                    <asp:Label ID="CustomAttr1Label" runat="server" />
                                </td>
                                <td>
                                    <edi:ZTextLabel ID="CustomAttr1" runat="server" BindTo="WI_PartAttrib1" />
                                </td>
                                <td>&nbsp;</td>
                                <td class="DetailsItem" style="vertical-align: middle;">
                                    <asp:Label ID="CustomAttr2Label" runat="server" />
                                </td>
                                <td colspan="2">
                                    <edi:ZTextLabel ID="CustomAttr2" runat="server" BindTo="WI_PartAttrib2" />
                                </td>
                            </tr>
                            <tr id="CustomAttr3Row" runat="server">
                                <td class="DetailsItem" style="vertical-align: middle;">
                                    <asp:Label ID="CustomAttr3Label" runat="server" />
                                </td>
                                <td>
                                    <edi:ZTextLabel ID="CustomAttr3" runat="server" BindTo="WI_PartAttrib3" />
                                </td>
                                <td>&nbsp;</td>
                                <td class="DetailsItem" style="vertical-align: middle;">
                                    <asp:Label ID="SerialNumberLabel" runat="server" />
                                </td>
                                <td colspan="2">
                                    <edi:ZTextLabel ID="SerialNumber" runat="server" BindTo="WI_SerialNumber" />
                                </td>
                            </tr>
                            <tr id="ExpiryPackingDatesRow" runat="server" visible="false">
                                <td class="DetailsItem" style="vertical-align: middle;">
                                    <asp:Label ID="PackingDateLabel" runat="server" Text="Packing Date" Visible="false" />
                                </td>
                                <td>
                                    <edi:ZDateTimeLabel ID="PackingDate" runat="server" BindTo="WI_PackingDate" Visible="false" />
                                </td>
                                <td>&nbsp;</td>
                                <td class="DetailsItem" style="vertical-align: middle;">
                                    <asp:Label ID="ExpiryDateLabel" runat="server" Text="Expiry Date" Visible="false" />
                                </td>
                                <td colspan="2">
                                    <edi:ZDateTimeLabel ID="ExpiryDate" runat="server" BindTo="WI_ExpiryDate" Visible="false" />
                                </td>
                            </tr>

                        </tbody>
                    </table>
                    <edi:ZCollapsablePanel ID="AdditionalDetailPanel" runat="server" CssClass="SectionTitle" Label="Additional Details" DisableCollapsing="True">
                        <asp:Table ID="AdditionalDetailTable" runat="server" CssClass="DetailsTable" HorizontalAlign="Left" />
                    </edi:ZCollapsablePanel>
                    <edi:ZGrid ID="CrossDockedOrderLinesGrid" runat="server" Caption="Cross Docked Order Lines" CssClass="DetailsTable" BindTo="ReservedPickLines" AllowAdd="False" ShowFooter="False" AutoGenerateColumns="False">
                        <PagerStyle Mode="NumericPages" />
                        <ItemStyle CssClass="DetailsCell" />
                        <HeaderStyle CssClass="DetailsHeader" />
                    </edi:ZGrid>
                    <edi:ZCollapsablePanel ID="CustomsDataPanel" runat="server" CssClass="SectionTitle" Label="Customs Related Data" DisableCollapsing="True">
                        <table class="ResultsTable">
                            <tbody>
                                <tr id="EntryKeyRow">
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Entry No. (WRN):</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZTextLabel runat="server" BindTo="CustomsData+WB_EntryKey" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Country of Origin:</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZTextLabel runat="server" BindTo="CustomsData+WB_RN_NKCountryOfOrigin" />
                                    </td>
                                </tr>
                                <tr id="EntryLineRow">
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Entry Line No. (WRL):</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZNumericLabel runat="server" BindTo="CustomsData+WB_EntryLineNo" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Customs Quantity:</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZNumericLabel runat="server" BindTo="CustomsData+WB_CustomsQty" />
                                        <edi:ZTextLabel runat="server" BindTo="CustomsData+WB_CustomsUnitOfQty" />
                                    </td>
                                </tr>
                                <tr id="EntryDateRow">
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Entry Date:</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZDateTimeLabel runat="server" BindTo="CustomsData+WB_EntryDate" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Value For Duty:</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZNumericLabel runat="server" BindTo="CustomsData+WB_ValueForDuty" />
                                    </td>
                                </tr>
                                <tr id="DeclarationReferenceRow">
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Declaration Reference:</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZTextLabel runat="server" BindTo="CustomsData+WB_DeclarationReference" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">TILV:</span>
                                    </td>
                                    <td nowrap="nowrap">
                                        <edi:ZNumericLabel runat="server" BindTo="CustomsData+WB_TILV" />
                                    </td>
                                </tr>
                                <tr id="AdditionalInformationRow">
                                    <td nowrap="nowrap">
                                        <span class="DetailsItem">Additional Information:</span>
                                    </td>
                                    <td colspan="4">
                                        <edi:ZTextLabel runat="server" BindTo="CustomsData+WB_AddInfo" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </edi:ZCollapsablePanel>
                    <edi:ZGrid ID="DocumentsGrid" runat="server" CssClass="DetailsTable" BindTo="DocumentHelper+Documents" Label="Documents" DisableCollapsing="True">
                        <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages" />
                        <ItemStyle CssClass="DetailsCell" />
                        <HeaderStyle CssClass="DetailsHeader" />
                    </edi:ZGrid>
                    <edi:ZCollapsablePanel runat="server" CssClass="SectionTitle" Label="Notes" DisableCollapsing="True">
                        <edi:ZNotesControl runat="server" BindTo="NotesHelper+VisibleNotes" />
                    </edi:ZCollapsablePanel>
                    <div class="ContentSection">
                        <asp:Button ID="SaveInventory" runat="server" Text="Save" OnClick="SaveInventory_Click" Style="min-width: 60px;" />
                        &nbsp;
						<asp:Button ID="CancelOrder" runat="server" Text="Cancel" OnClick="CancelInventory_Click" Style="min-width: 60px;" />
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
