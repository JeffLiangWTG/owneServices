<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ Page Language="c#" CodeBehind="EditOrder.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.EditOrder" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Add/Edit Order </title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ZTextLabel ID="OrderLabel" runat="server" CssClass="PageTitle">New Order</edi:ZTextLabel>
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <table class="ResultsTable" id="OrderDetailsTable">
                    <tbody>
                        <tr>
                            <td>
                                <edi:ZTextLabel ID="SupplierLabel" runat="server">Supplier:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZFindBox ID="Supplier" runat="server" AutoPostBack="True" ModuleID="OrgSupplierWebTracking" BindTo="SupplierCode"></edi:ZFindBox>
                            </td>
                            <td>&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="BuyerOrderNumberLabel" runat="server">Order Number:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZTextBox ID="BuyerOrderNumber" runat="server" BindTo="JD_OrderNumber"></edi:ZTextBox></td>
                            <td>&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="OrderDateLabel" runat="server">Order Date:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDateEdit ID="OrderDate" runat="server" BindTo="JD_OrderDate"></edi:ZDateEdit>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <edi:ZTextLabel ID="SupplierAddressLabel" runat="server">Pickup From:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDropEditList ID="GoodsAvailableAt" BindTo="PickupAddressLine" DataValueField="OA_Code"
                                    DataTextField="OA_Code" BindToList="PickupAddress_List" runat="server" Width="250px" ShowEmptyItem="True" />
                            </td>
                            <td>&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="CurrencyLabel" runat="server">Currency: </edi:ZTextLabel></td>
                            <td>
                                <edi:ZFindBox ID="Currency" runat="server" BindTo="JD_RX_NKOrderCurrency" ModuleID="RefCurrencyWeb" BindToList="Lookups.OrderCurrencies"></edi:ZFindBox>
                            </td>
                            <td>&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="ReqExWorksLabel" runat="server">Req. Ex Works:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDateEdit ID="ReqExWorksDate" runat="server" BindTo="JD_ExWorksRequiredBy" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3">&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="ServiceLevelLabel" runat="server">Service Level:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDropEditList ID="ServiceLavel" runat="server" BindTo="JD_RS_NKServiceLevel_NI" BindToList="JD_RS_List" Width="160px" DescriptionFieldName="RS_Description" CodeFieldName="RS_Code"></edi:ZDropEditList>
                            </td>
                            <td>&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="ReqInStoreLabel" runat="server">Req. In Store:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDateEdit ID="ReqInStoreDate" runat="server" BindTo="JD_DeliveryRequiredBy" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <edi:ZTextLabel ID="BuyerLabel" runat="server">Buyer:</edi:ZTextLabel></td>
                            <td colspan="2">
                                <edi:ZTextLabel ID="Buyer" runat="server" BindTo="Buyer.OH_FullName" /></td>
                            <td>
                                <edi:ZTextLabel ID="INCOTermsLabel" runat="server">INCO Terms:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDropEditList ID="INCOTerms" runat="server" BindTo="JD_IncoTerm" BindToList="JD_IncoTerm_List" Width="160px" />
                            </td>
                            <td colspan="3">&nbsp;</td>
                        </tr>
                        <tr>
                            <td>
                                <edi:ZTextLabel ID="BuyerAddressLabel" runat="server">Deliver To:</edi:ZTextLabel></td>
                            <td colspan="2">
                                <edi:ZDropEditList ID="GoodsDeliveredTo" BindTo="DeliverAddressLine" DataValueField="OA_Code"
                                    DataTextField="OA_Code" BindToList="DeliverAddress_List" runat="server" Width="250px" ShowEmptyItem="True" />
                            </td>
                            <td>
                                <edi:ZTextLabel ID="AdditionalTermsLabel" runat="server">Additional Terms:</edi:ZTextLabel></td>
                            <td colspan="4">
                                <edi:ZTextBox ID="AdditionalTerms" runat="server" BindTo="JD_AdditionalTerms" Width="100%"></edi:ZTextBox></td>
                        </tr>
                        <tr>
                            <td colspan="3">&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="TransportModeLabel" runat="server">Transport Mode:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDropDownList ID="TransportMode" runat="server" AutoPostBack="True" BindTo="JD_TransportMode" Width="160px"
                                    DataTextField="Description" ShowEmptyItem="true"
                                    OnSelectedIndexChanged="WeightVolumeDropDown_SelectedIndexChanged">
                                </edi:ZDropDownList>
                            </td>
                            <td colspan="3">&nbsp;</td>
                        </tr>
                        <tr>
                            <td colspan="3">&nbsp;</td>
                            <td>
                                <edi:ZTextLabel ID="ContainerModeLabel" runat="server">Container Mode:</edi:ZTextLabel></td>
                            <td>
                                <edi:ZDropEditList ID="ContainerMode" runat="server" BindTo="JD_ContainerMode" BindToList="JD_ContainerMode_List" Width="160px"></edi:ZDropEditList>
                            </td>
                            <td colspan="3">&nbsp;</td>
                        </tr>
                    </tbody>
                </table>
                <div class="ContentSection">
                    <edi:ZTextLabel ID="OrderLineDetails" runat="server" CssClass="SectionTitle">Order Lines</edi:ZTextLabel><edi:ZDataGrid ID="OrderLinesGrid"
                        runat="server" CssClass="DetailsTable" BindTo="OrderLines" AllowAdd="True" AllowDelete="True" AllowEdit="True" ShowFooter="False"
                        AutoGenerateColumns="False">
                        <PagerStyle Mode="NumericPages"></PagerStyle>
                        <ItemStyle CssClass="DetailsCell"></ItemStyle>
                        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                    </edi:ZDataGrid>
                </div>
                <div class="ContentSection">
                    <edi:ZTextLabel ID="Planning" runat="server" CssClass="SectionTitle">Planning</edi:ZTextLabel>
                    <table class="ResultsTable">
                        <tbody>
                            <tr>
                                <td>Port of Loading:</td>
                                <td>
                                    <edi:ZFindBox ID="PortOfLoadFindBox" runat="server" BindTo="JD_RL_NKPortOfLoading" ModuleID="LocationWeb" />
                                </td>
                            </tr>
                            <tr>
                                <td>Port of Discharge:</td>
                                <td>
                                    <edi:ZFindBox ID="PortOfDischargeFindBox" runat="server" BindTo="JD_RL_NKPortOfDischarge" ModuleID="LocationWeb" />
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                            </tr>
                            <tr>
                                <td nowrap="nowrap">Packs:</td>
                                <td nowrap="nowrap">
                                    <edi:ZNumericTextBox ID="Packs" runat="server" BindTo="JD_Packs" Width="78px"></edi:ZNumericTextBox><edi:ZDropDownList
                                        ID="PackTypesDropDown" runat="server" BindTo="JD_F3_NKPackType" BindToList="JD_F3_NKPackType_List">
                                    </edi:ZDropDownList></td>
                            </tr>
                            <tr>
                                <td nowrap="nowrap">Volume:</td>
                                <td nowrap="nowrap">
                                    <edi:ZNumericTextBox ID="Volume" runat="server" BindTo="JD_ActualVolume" Width="78px" AutoPostBack="true"></edi:ZNumericTextBox>
                                    <edi:ZDropDownList ID="VolumeDropDown" runat="server" BindTo="JD_UnitOfVolume" BindToList="JD_UnitOfVolume_List" AutoPostBack="true" OnSelectedIndexChanged="WeightVolumeDropDown_SelectedIndexChanged"></edi:ZDropDownList></td>
                            </tr>
                            <tr>
                                <td nowrap="nowrap">Weight:</td>
                                <td nowrap="nowrap">
                                    <edi:ZNumericTextBox ID="Weight" runat="server" BindTo="JD_ActualWeight" Width="78px" AutoPostBack="true"></edi:ZNumericTextBox>
                                    <edi:ZDropDownList ID="WeightDropDown" runat="server" BindTo="JD_UnitOfWeight" BindToList="JD_UnitOfWeight_List" AutoPostBack="true" OnSelectedIndexChanged="WeightVolumeDropDown_SelectedIndexChanged"></edi:ZDropDownList></td>
                            </tr>
                            <tr>
                                <td nowrap="nowrap" colspan="2">Special instructions:</td>
                            </tr>
                            <tr>
                                <td nowrap="nowrap" colspan="2">
                                    <edi:ZTextBox ID="SpecialInstructions" runat="server" Width="100%" Rows="4" BindTo="UserEditableNoteHelper.EditableNoteText"
                                        TextMode="MultiLine"></edi:ZTextBox></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="ContentSection">
                    <asp:Button ID="SaveOrder" runat="server" Text="Save" OnClick="SaveOrder_Click" Style="min-width: 60px;"></asp:Button>
                    <asp:Button ID="CancelOrder" runat="server" Text="Cancel" OnClick="CancelOrder_Click" Style="min-width: 60px;"></asp:Button>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
