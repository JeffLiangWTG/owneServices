<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ Page Language="c#" CodeBehind="Quotation.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Quotes.Quotation" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>Get a Quotation</title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="TitleTitle" runat="server">
                <edi:ZTextLabel ID="TitleLabel" runat="server" CssClass="PageTitle">Get a Quotation</edi:ZTextLabel>
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <asp:UpdatePanel ID="up" runat="Server">
                    <ContentTemplate>
                        <div id="EditControls" runat="server" class="ContentSection">
                            <div id="Company" runat="server" class="ContentSection">
                                <edi:ZTextLabel ID="QuoteCompanyLabel" runat="server">Request Quote from: </edi:ZTextLabel>
                                <edi:ZGuidDropDownList ID="QuoteCompanyDropList" runat="server" AutoPostBack="True" DataValueField="PK"
                                    DataTextField="GC_Name" DescriptionFieldName="GC_Name" CodeFieldName="GC_Name" ValueFieldName="PK"
                                    ShowDescription="False" ReadOnly="True" BindToList="Companies" BindTo="Quote.TH_GC">
                                </edi:ZGuidDropDownList>
                            </div>
                            <br />
                            <div id="ShipmentDetails" runat="server" class="ContentSection">
                                <edi:ZTextLabel ID="ShipmentDetailsTitle" runat="server" CssClass="SectionTitle">Shipment Details</edi:ZTextLabel>
                                <table class="ResultsTable">
                                    <tr>
                                        <td>
                                            <edi:ZDocAddressWebControl ID="ConsignorAddress" runat="server" BindTo="ConsignorDocumentaryAddress"
                                                IsConsignor="true" NewOrgRelationType="Supplier" DependentPortControl="OriginPort_TextBox"
                                                Caption="Pickup From" SaveCheckboxCaption="Save Pickup Address" AutoPostBackOnPostalCodeChanged="True" />
                                        </td>
                                        <td>
                                            <edi:ZDocAddressWebControl ID="ConsigneeAddress" runat="server" BindTo="ConsigneeDocumentaryAddress"
                                                IsConsignee="true" NewOrgRelationType="Buyer" DependentPortControl="DestinationPort_TextBox"
                                                Caption="Deliver To" SaveCheckboxCaption="Save Delivery Address" AutoPostBackOnPostalCodeChanged="True" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:HiddenField ID="IsDomesticHiddenField" runat="server" />
                                            Origin:
                                            <edi:ZFindBox ID="OriginPort" runat="server" AutoPostBack="true" BindTo="Origin" ModuleID="RefUNLOCOWeb" />
                                        </td>
                                        <td>Destination:
                                            <edi:ZFindBox ID="DestinationPort" runat="server" AutoPostBack="true" BindTo="Destination" ModuleID="RefUNLOCOWeb" />
                                        </td>
                                    </tr>
                                </table>
                                <br />
                                <table class="ResultsTable">
                                    <tr>
                                        <td>Transport Mode:</td>
                                        <td>
                                            <edi:ZDropDownList ID="TransportMode" runat="server" BindTo="Mode" BindToList="ModesForWebTracker" AutoPostBack="true" ShowEmptyItem="true"></edi:ZDropDownList>
                                            <edi:ZCheckBox ID="IsCompareModeCheckBox" runat="server" BindTo="IsCompareMode" Text="Compare" AutoPostBack="true" />
                                            <br />
                                            <edi:ZMultiColumnCheckBoxSelection ID="TansportModeComparison" runat="server" CssClass="ComparisonTable" BindTo="ComparisonModes" BindToList="ComparisonModes" Columns="3" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <edi:ZTextLabel ID="IncoTermLabel" runat="server" Text="INCO Term:"></edi:ZTextLabel></td>
                                        <td>
                                            <edi:ZDropDownList ID="IncoTerm" runat="server" BindTo="PaymentTerms" BindToList="IncoTerms" ShowEmptyItem="true"></edi:ZDropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Service Level:</td>
                                        <td>
                                            <edi:ZDropDownList ID="ServiceLevel" runat="server" BindTo="ServiceLevel" BindToList="ServiceLevels"
                                                ShowEmptyItem="true" DataValueField="RS_Code" DataTextField="RS_Description" AutoPostBack="true">
                                            </edi:ZDropDownList>
                                            <edi:ZCheckBox ID="IsCompareServiceLevelCheckBox" runat="server" BindTo="IsCompareServiceLevel" Text="Compare" AutoPostBack="true" />
                                            <br />
                                            <edi:ZMultiColumnCheckBoxSelection ID="ServiceLevelComparison" runat="server" CssClass="ComparisonTable" BindTo="ComparisonServiceLevels" BindToList="ComparisonServiceLevels" Columns="3" />
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <br />
                            <div id="GoodsDetails" runat="server" class="ContentSection">
                                <edi:ZTextLabel ID="GoodsDetailsTitle" runat="server" CssClass="SectionTitle">Goods Details</edi:ZTextLabel>
                                <table class="ResultsTable">
                                    <tr>
                                        <td>
                                            <table class="ResultsTable">
                                                <tr>
                                                    <td>Weight:</td>
                                                    <td>
                                                        <edi:ZNumericTextBox ID="WeightAmount" runat="server" BindTo="Weight" AutoPostBack="True" Decimals="3"></edi:ZNumericTextBox>&nbsp;
													<edi:ZDropDownList ID="WeightDropDown" runat="server" BindTo="WeightUnit" BindToList="UnitOfWeightList" AutoPostBack="True"></edi:ZDropDownList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>Volume:</td>
                                                    <td>
                                                        <edi:ZNumericTextBox ID="VolumeAmount" runat="server" BindTo="Volume" AutoPostBack="True" Decimals="3"></edi:ZNumericTextBox>&nbsp;
													<edi:ZDropDownList ID="VolumeDropDown" runat="server" BindTo="VolumeUnit" BindToList="UnitOfVolumeList" AutoPostBack="true"></edi:ZDropDownList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>Commodity:</td>
                                                    <td>
                                                        <edi:ZFindBox ID="Commodity" runat="server" BindTo="Commodity" ModuleID="RefCommodityCodeWeb" BindToList="Quote.CurrentOneOffQuote.Lookups.Commodities" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>Chargeable:</td>
                                                    <td>
                                                        <edi:ZNumericLabel ID="Chargeable" runat="server" CssClass="DetailsItem" BindTo="Chargeable" BindToDecimals="" Decimals="3"></edi:ZNumericLabel>&nbsp;
													<edi:ZTextLabel ID="ChargeableUnit" runat="server" CssClass="DetailsItem" BindTo="ChargeableUnit"></edi:ZTextLabel>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td>
                                            <table class="ResultsTable">
                                                <tr>
                                                    <td>Goods Value:</td>
                                                    <td>
                                                        <edi:ZNumericTextBox ID="GoodsValueBox" runat="server" Width="70px" BindTo="GoodsValue" AutoPostBack="true"></edi:ZNumericTextBox>
                                                    </td>
                                                    <td>
                                                        <edi:ZFindBox ID="GoodsCurrencyBox" runat="server" Width="70px" BindTo="GoodsCurrency" ModuleID="RefCurrencyWeb" />
                                                    </td>
                                                </tr>
                                                <tr id="InsuranceValueRow" runat="server">
                                                    <td>Insurance Value:</td>
                                                    <td>
                                                        <edi:ZNumericTextBox ID="InsuranceValueBox" runat="server" Width="70px" BindTo="InsuranceValue" AutoPostBack="true"></edi:ZNumericTextBox>
                                                    </td>
                                                    <td>
                                                        <edi:ZFindBox ID="InsuranceCurrencyBox" runat="server" Width="70px" BindTo="InsuranceCurrency" ModuleID="RefCurrencyWeb" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <br />
                            <div id="Containers" runat="server" class="ContentSection">
                                <edi:ZDataGrid ID="ContainersDataGrid" runat="server" Caption="Containers" CssClass="DetailsTable" BindTo="QuoteContainers"
                                    AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False" AllowPaging="False">
                                    <PagerStyle Mode="NumericPages"></PagerStyle>
                                    <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                    <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                </edi:ZDataGrid>
                            </div>
                            <div id="LooseCargo" runat="server" class="ContentSection">
                                <edi:ZDataGrid ID="LooseCargoDataGrid" runat="server" Caption="Loose Cargo" CssClass="DetailsTable" BindTo="Quote.CurrentOneOffQuote.LooseCargo"
                                    AllowAdd="True" AllowEdit="True" AllowDelete="True" AutoGenerateColumns="False" AllowPaging="False">
                                    <PagerStyle Mode="NumericPages"></PagerStyle>
                                    <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                    <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                </edi:ZDataGrid>
                            </div>
                        </div>
                        <br />
                        <edi_tracking:ComparisonQuoteWebControl ID="ComparisonQuotes" runat="server" BindTo="ComparisonQuoteResults" CssClass="ComparisonQuotes" CaptionCssClass="ComparisonQuotesCaption" EstimateCssClass="ComparisonQuoteEstimate" Caption="Quote Estimates" />
                        <br />
                        <div id="EmailNotice" runat="server" class="ContentSection">
                        </div>
                        <div id="PendingDownload" class="ContentSection " style="visibility: hidden; background-color: lightyellow;">
                            <span class="SectionTitle">Please wait while the document is generated.</span>
                            <div>Upon completion the document will be downloaded by your browser.</div>
                        </div>
                        <br />
                        <div id="PreviewAndSave" runat="server" class="ContentSection">
                            <asp:Button ID="ClearEstimates" runat="server" Text="Clear Estimates" OnClick="ClearEstimates_Click" />
                            &nbsp;
						<asp:Button ID="PreviewQuote" runat="server" Text="Preview" OnClick="PreviewQuote_Click" />
                            &nbsp;
						<asp:Button ID="SaveQuote" runat="server" Text="Save" OnClick="SaveQuote_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </form>
</body>
</html>
