<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<%@ Page Language="c#" CodeBehind="EditImporterSecurityFiling.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.ImporterSecurityFiling.EditImporterSecurityFiling" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Importer Security Filing</title>
    <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body id="DefaultBody" runat="server">
    <form id="form" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <edi:ZTextLabel ID="TitleLabel" runat="server" CssClass="PageTitle">Importer Security Filing</edi:ZTextLabel>&nbsp;
	<edi:ZTextLabel ID="LabelBF_JobReference" runat="server" CssClass="PageTitle" BindTo="BF_JobReference"></edi:ZTextLabel>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <div id="NotFoundError" runat="server">
                    <edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="PageTitle"></edi:ZTextLabel>
                </div>
                <div id="EditControls" runat="server">
                    <br />
                    <div id="Entry_Type" runat="server">
                        <table class="ResultsTable">
                            <tr>
                                <td></td>
                                <td>Entry Type:</td>
                                <td>
                                    <edi:ZDropDownList ID="EntryTypeDropDown" runat="server" BindTo="BF_EntryType"
                                        DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False" OnTextChanged="RefreshVisible">
                                    </edi:ZDropDownList>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <asp:UpdatePanel ID="upDetails" runat="Server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <table title="Details">
                                <tr>
                                    <td>
                                        <div id="MainDetailsRegion" runat="server">
                                            <table class="ResultsTable">
                                                <tr>
                                                    <td class="SectionTitle" colspan="3">Details:</td>
                                                </tr>
                                                <tr id="ActionReasonCodeArea" runat="server">
                                                    <td></td>
                                                    <td>Action Reason Code:</td>
                                                    <td>
                                                        <edi:ZDropDownList ID="ActionReasonCodeZdropdownlist" runat="server" BindTo="BF_ActionReasonCode"
                                                            DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False" Width="100%">
                                                        </edi:ZDropDownList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>Shipment Type:</td>
                                                    <td>
                                                        <edi:ZDropDownList ID="ShipmentTypeDropDown" runat="server" BindTo="BF_ShipmentType"
                                                            DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False">
                                                        </edi:ZDropDownList>
                                                    </td>
                                                </tr>
                                                <tr id="TransportModeArea" runat="server">
                                                    <td></td>
                                                    <td>Transport Mode:</td>
                                                    <td>
                                                        <edi:ZDropDownList ID="TransportModeDropDown" runat="server" BindTo="BF_TransportMode"
                                                            DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False">
                                                        </edi:ZDropDownList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>Carrier SCAC:</td>
                                                    <td>
                                                        <edi:ZTextBox ID="SCACTextBox" runat="server" BindTo="BF_SCAC" Width="100%"></edi:ZTextBox></td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>Importer:</td>
                                                    <td>
                                                        <edi:ZGuidFindBox ID="ImporterFindBox" runat="server" AutoPostBack="True" BindTo="BF_OH_Importer" ModuleID="Organisation" BindToList="Lookups.Organisations" Width="100%"></edi:ZGuidFindBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>Owner Ref:</td>
                                                    <td>
                                                        <edi:ZTextBox ID="ZtextboxOwnerRef" runat="server" BindTo="BF_OwnerReference" Width="100%"></edi:ZTextBox></td>
                                                </tr>
                                            </table>
                                        </div>
                                    </td>
                                    <td valign="top">
                                        <div id="Reference" runat="server" class="ContentSection">
                                            <table class="ResultsTable">
                                                <tr>
                                                    <td colspan="2" align="left" class="SectionTitle">Bill Numbers:</td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>
                                                        <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="UpdatePanel3">
                                                            <ContentTemplate>
                                                                <edi:ZDataGrid ID="ReferenceGrid" runat="server" CssClass="DetailsTable" BindTo="BillNumbersReferences"
                                                                    AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
                                                                    AllowPaging="False" InitialRowsToDisplay="1">
                                                                    <PagerStyle Mode="NumericPages"></PagerStyle>
                                                                    <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                                                    <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                                                    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                                                </edi:ZDataGrid>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <table class="ResultsTable">
                                <tr>
                                    <td colspan="6" class="SectionTitle" align="left">Routing:</td>
                                </tr>
                                <tr>
                                    <td colspan="6"></td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td>ETD:</td>
                                    <td>
                                        <edi:ZDateEdit ID="ZDateEditETD" runat="server" DateTimeFormat="Long" BindTo="FirstTransport.JW_ETD"></edi:ZDateEdit>
                                    </td>
                                    <td></td>
                                    <td>Load Port:</td>
                                    <td>
                                        <edi:ZFindBox ID="ZfindboxLoadPort" runat="server" AutoPostBack="true" BindTo="FirstTransport.JW_RL_NKLoadPort" ModuleID="RefUNLOCOWeb"></edi:ZFindBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td>ETA:</td>
                                    <td>
                                        <edi:ZDateEdit ID="ZDateEditETA" runat="server" DateTimeFormat="Long" BindTo="FirstTransport.JW_ETA"></edi:ZDateEdit>
                                    </td>
                                    <td></td>
                                    <td>Discharge Port:</td>
                                    <td>
                                        <edi:ZFindBox ID="ZfindboxDischargePort" runat="server" AutoPostBack="true" BindTo="FirstTransport.JW_RL_NKDiscPort" ModuleID="RefUNLOCOWeb"></edi:ZFindBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td>Vessel:</td>
                                    <td>
                                        <edi:ZTextBox ID="ZtextboxVessel" runat="server" BindTo="FirstTransport.JW_Vessel"></edi:ZTextBox></td>
                                    <td></td>
                                    <td>Voyage:</td>
                                    <td>
                                        <edi:ZTextBox ID="ZtextboxVoyage" runat="server" BindTo="FirstTransport.JW_VoyageFlight"></edi:ZTextBox></td>
                                </tr>
                                <div id="ISF5Details" runat="server">
                                    <div id="LocationsRegion" runat="server">
                                        <tr>
                                            <td colspan="6"></td>
                                        </tr>
                                        <tr>
                                            <td colspan="6" class="SectionTitle" align="left">Locations:</td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Unload Port:</td>
                                            <td colspan="4">
                                                <edi:ZFindBox ID="UnloadPort" runat="server" AutoPostBack="true" BindTo="BF_RL_NKPortOfUnload" ModuleID="RefUNLOCOWeb"></edi:ZFindBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Delivery Port:</td>
                                            <td colspan="4">
                                                <edi:ZFindBox ID="DeliveryPort" runat="server" AutoPostBack="true" BindTo="BF_RL_NKPlaceOfDelivery" ModuleID="RefUNLOCOWeb"></edi:ZFindBox>
                                            </td>
                                        </tr>
                                    </div>
                                </div>
                                <div id="ISF10Details" runat="server" class="ContentSection">
                                    <div id="ImporterDetails" runat="server">
                                        <tr>
                                            <td colspan="6"></td>
                                        </tr>
                                        <tr>
                                            <td class="SectionTitle" colspan="6" align="left">Importer Details:</td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Importer ID Type:</td>
                                            <td colspan="4">
                                                <edi:ZDropDownList ID="ImporterIDTypeDropDown" runat="server" BindTo="BF_ImporterCodeType"
                                                    DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False">
                                                </edi:ZDropDownList></td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Importer ID:</td>
                                            <td colspan="4">
                                                <edi:ZTextBox ID="ImporterIDTextBox" runat="server" BindTo="BF_ImporterCode"></edi:ZTextBox></td>
                                        </tr>
                                        <tr runat="server" id="ImporterFullNameInfo">
                                            <td></td>
                                            <td>Full Legal Name:</td>
                                            <td colspan="4">
                                                <edi:ZTextBox ID="ImporterFullLeagalNameTextBox" runat="server" BindTo="BF_ImporterFullName"></edi:ZTextBox></td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>DOB:</td>
                                            <td colspan="4">
                                                <edi:ZDateEdit ID="DOBDate" runat="server" BindTo="BF_DateOfBirth"></edi:ZDateEdit>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Country of Issue:</td>
                                            <td colspan="4">
                                                <edi:ZFindBox ID="CountryOfIssueFindBox" runat="server" AutoPostBack="true" BindTo="BF_CountryOfIssue" ModuleID="RefCountry" BindToList="Lookups.Countries"></edi:ZFindBox>
                                            </td>
                                        </tr>
                                    </div>
                                    <div id="ConsigneeDetails" runat="server">
                                        <tr>
                                            <td colspan="6"></td>
                                        </tr>
                                        <tr>
                                            <td colspan="6" class="SectionTitle" align="left">Consignee Details:</td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Consignee ID Type:</td>
                                            <td colspan="4">
                                                <edi:ZDropDownList ID="ConsigneeIDTypeDropDown" runat="server" BindTo="BF_ConsigneeCodeType"
                                                    DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False">
                                                </edi:ZDropDownList></td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Consignee ID:</td>
                                            <td colspan="4">
                                                <edi:ZTextBox ID="ConsigneeIDTextBox" runat="server" BindTo="BF_ConsigneeCode"></edi:ZTextBox></td>
                                        </tr>
                                        <tr runat="server" id="ConsigneeFullNameInfo">
                                            <td></td>
                                            <td>Full Legal Name:</td>
                                            <td colspan="4">
                                                <edi:ZTextBox ID="ConsigneeFullLeagalNameTextBox" runat="server" BindTo="BF_ConsigneeFullName"></edi:ZTextBox></td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>DOB:</td>
                                            <td colspan="4">
                                                <edi:ZDateEdit ID="ConsigneeDOBDate" runat="server" BindTo="BF_ConsigneeDateOfBirth"></edi:ZDateEdit>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Country of Issue:</td>
                                            <td colspan="4">
                                                <edi:ZFindBox ID="ConsigneeCountryOfIssueFindBox" runat="server" AutoPostBack="true" BindTo="BF_ConsigneeCountryOfIssue" ModuleID="RefCountry" BindToList="Lookups.Countries"></edi:ZFindBox>
                                            </td>
                                        </tr>
                                    </div>
                                </div>
                            </table>
                            <div id="BondDetails" runat="server" class="ContentSection">
                                <table class="ResultsTable" title="BondDetails">
                                    <tr>
                                        <td class="SectionTitle">Bond Details:</td>
                                    </tr>
                                </table>
                                <table class="ResultsTable" title="BondDetails">
                                    <tr id="BondHolder" runat="server">
                                        <td></td>
                                        <td>Bond Holder:</td>
                                        <td colspan="2">
                                            <edi:ZTextBox ID="BondHolderTextBox" runat="server" BindTo="BF_BondNumberOrHolder"></edi:ZTextBox></td>
                                    </tr>
                                    <div id="BondISFChangesForCSMS09000148" runat="server">
                                        <tr>
                                            <td></td>
                                            <td>Bond Activity Code:</td>
                                            <td>
                                                <edi:ZDropDownList ID="BondActivityCodeZdropdownlist" runat="server" BindTo="BF_BondActivityCode"
                                                    DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False">
                                                </edi:ZDropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td></td>
                                            <td>Bond Type:</td>
                                            <td>
                                                <edi:ZDropDownList ID="BondTypeZdropdownlist" runat="server" BindTo="BF_BondType"
                                                    DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False">
                                                </edi:ZDropDownList>
                                            </td>
                                        </tr>
                                    </div>
                                    <tr id="SuretyCode" runat="server">
                                        <td></td>
                                        <td>Bond Surety Code:</td>
                                        <td>
                                            <edi:ZTextBox ID="BondSuretyCodeTextBox" runat="server" BindTo="BF_SuretyCode"></edi:ZTextBox></td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td>Entry Number:</td>
                                        <td style="width: 200px">
                                            <edi:ZTextBox ID="EntryNumberTextBox" runat="server" BindTo="BF_EntryNumber"></edi:ZTextBox></td>
                                    </tr>
                                </table>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div id="OrganisationsDetails" runat="server">
                        <table class="ResultsTable" title="Organisations">
                            <tr>
                                <td>
                                    <edi:ZDocAddressWebControl ID="ShipToPartyAddress" runat="server" BindTo="MainShipToParty" Caption="Ship To Party"
                                        SaveCheckboxCaption="Save Ship To Party" IsConsignor="false" IsConsignee="true" ResidentialCheckboxVisible="false"
                                        SaveCheckboxVisible="true" GovermentRegNoVisible="true" />
                                </td>
                                <td>&nbsp;</td>
                                <td>
                                    <edi:ZDocAddressWebControl ID="BuyingPartyAddress" runat="server" BindTo="BuyingParty" Caption="Buying Party"
                                        SaveCheckboxCaption="Save Buying Party" IsConsignor="false" IsConsignee="true" ResidentialCheckboxVisible="false"
                                        SaveCheckboxVisible="true" GovermentRegNoVisible="true" />
                                    <edi:ZDocAddressWebControl ID="BookingPartyAddress" runat="server" BindTo="BookingParty" Caption="Booking Party"
                                        IsConsignor="false" IsConsignee="true" ResidentialCheckboxVisible="false"
                                        SaveCheckboxVisible="false" GovermentRegNoVisible="true" />

                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ZDocAddressWebControl ID="SellingPartyAddress" runat="server" BindTo="SellingParty" Caption="Selling Party"
                                        SaveCheckboxCaption="Save Selling Party" IsConsignor="true" IsConsignee="false" ResidentialCheckboxVisible="false"
                                        SaveCheckboxVisible="true" GovermentRegNoVisible="true" />
                                </td>
                                <td>&nbsp;</td>
                                <td>
                                    <edi:ZDocAddressWebControl ID="StuffingLocation" runat="server" BindTo="StuffingLocation" Caption="Stuffing Location"
                                        IsConsignor="true" IsConsignee="true" ResidentialCheckboxVisible="false"
                                        SaveCheckboxVisible="false" GovermentRegNoVisible="true" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3">
                                    <edi:ZDocAddressWebControl ID="ConsolidatorAddress" runat="server" BindTo="Consolidator" Caption="Consolidator"
                                        IsConsignor="true" IsConsignee="true" ResidentialCheckboxVisible="false"
                                        SaveCheckboxVisible="false" GovermentRegNoVisible="true" />
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="LowValueDetails" runat="server" class="ContentSection">
                        <table class="ResultsTable" title="LowValueDetails">
                            <tr>
                                <td class="SectionTitle">Low-Value Details:</td>
                            </tr>
                        </table>
                        <table class="ResultsTable" title="LowValueDetails">
                            <tr>
                                <td>&nbsp;</td>
                                <td>Ship. Sub-Type:</td>
                                <td>
                                    <edi:ZDropDownList ID="ShipSubTypeDropDown" runat="server" BindTo="BF_ShipmentSubType"
                                        DataTextField="Description" AutoPostBack="False" ShowEmptyItem="True">
                                    </edi:ZDropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>Estimated Value:</td>
                                <td>
                                    <edi:ZNumericTextBox ID="EstimatedValueEditBox" runat="server" BindTo="BF_EstimatedValue"></edi:ZNumericTextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>Estimated Qty.:</td>
                                <td>
                                    <edi:ZNumericTextBox ID="EstimatedQtyEditBox" runat="server" BindTo="BF_EstimatedQuantity"></edi:ZNumericTextBox><edi:ZDropDownList ID="EstimatedQuantityUQDropDown" runat="server" BindTo="BF_EstimatedQuantityUQ"></edi:ZDropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>Estimated Weight:</td>
                                <td>
                                    <edi:ZNumericTextBox ID="EstimatedWeightEditBox" runat="server" BindTo="BF_EstimatedWeight"></edi:ZNumericTextBox><edi:ZDropDownList ID="EstimatedWeightUQDropDown" runat="server" BindTo="BF_EstimatedWeightUQ"></edi:ZDropDownList>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="Grids" runat="server" class="ContentSection">
                        <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="UpdatePanelAddresses">
                            <ContentTemplate>
                                <div id="Addresses" runat="server" class="ContentSection">
                                    <edi:ZDataGrid ID="AddressesDataGrid" runat="server" CssClass="DetailsTable" BindTo="ManufacturerAddresses"
                                        AllowAdd="true" AllowEdit="true" AllowDelete="True" PageSize="3" AutoGenerateColumns="False" AutoSizeColumns="True"
                                        AllowPaging="False" Caption="Manufacturer Addresses" InitialRowsToDisplay="1">
                                        <PagerStyle Mode="NumericPages"></PagerStyle>
                                        <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                        <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                    </edi:ZDataGrid>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <br />
                        <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="UpdatePanelLines">
                            <ContentTemplate>
                                <div id="Lines" runat="server" class="ContentSection">
                                    <edi:ZDataGrid ID="LinesDataGrid" runat="server" CssClass="DetailsTable" BindTo="Lines"
                                        AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
                                        AllowPaging="False" Caption="Lines" InitialRowsToDisplay="1" AutoSizeColumns="True">
                                        <PagerStyle Mode="NumericPages"></PagerStyle>
                                        <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                        <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                    </edi:ZDataGrid>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <div id="Containers" runat="server" class="ContentSection">
                            <br />
                            <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="UpdatePanel1">
                                <ContentTemplate>
                                    <edi:ZDataGrid ID="ContainersDataGrid" runat="server" CssClass="DetailsTable" BindTo="Equipments"
                                        AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
                                        AllowPaging="False" Caption="Containers">
                                        <PagerStyle Mode="NumericPages"></PagerStyle>
                                        <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                        <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                    </edi:ZDataGrid>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        <div id="ShipToAddresses" runat="server" class="ContentSection">
                            <asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="UpdatePanel2">
                                <ContentTemplate>
                                    <edi:ZDataGrid ID="ShipToAddressesDataGrid" runat="server" CssClass="DetailsTable" BindTo="DocAddressesExcludeManufacturer"
                                        AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
                                        AllowPaging="False" Caption="Extra Addresses">
                                        <PagerStyle Mode="NumericPages"></PagerStyle>
                                        <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
                                        <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                    </edi:ZDataGrid>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                    <div id="SaveButton">
                        <br />
                        <table class="ResultsTable" title="Buttons">
                            <tr>
                                <td>
                                    <asp:Button ID="SaveISF" runat="server" Text="Save" OnClick="SaveISF_Click" Style="min-width: 60px;"></asp:Button>
                                </td>
                                <td>
                                    <asp:Button ID="SendISF" runat="server" Text="Save And Send" OnClick="SendISF_Click" Style="min-width: 60px;"></asp:Button>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
