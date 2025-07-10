<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditBooking.aspx.cs" Inherits="Enterprise.Tracking.Web.LinerAndAgency.EditBooking" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Edit Liner & Agency Booking</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ZTextLabel ID="ShippingBookingsLabel" runat="server" CssClass="PageTitle">Liner & Agency Booking</edi:ZTextLabel>
                <edi:ZTextLabel ID="BookingNumberLabel" runat="server" CssClass="PageTitle" BindTo="JS_UniqueConsignRef"></edi:ZTextLabel>
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <div id="NotFoundError" runat="server">
                    <edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="PageTitle"></edi:ZTextLabel>
                </div>
                <div id="DataContent" runat="server">
                    <div id="BookingDetailsDiv" runat="server" class="ContentSection">
                        <table class="ResultsTable">
                            <tr>
                                <td colspan="5"></td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="BookingRefLabel" CssClass="DetailsItem">Booking Ref:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZTextLabel runat="server" ID="BookingRef" BindTo="JS_CFSReference" />
                                </td>
                                <td>&nbsp;</td>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="BookingDateLabel" CssClass="DetailsItem">Booking Date:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZDateTimeLabel runat="server" ID="BookingDate" BindTo="JS_A_BKD" />
                                </td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="ShipperRefLabel" CssClass="DetailsItem">Shipper's Ref#:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZTextBox runat="server" ID="ShipperRef" BindTo="JS_BookingReference" />
                                </td>
                                <td>&nbsp;</td>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="StatusLabel" CssClass="DetailsItem">Status:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZCodeLookupLabel runat="server" ID="Status" BindTo="JS_ShipmentStatus" DisplayStyle="CodeAndDescription" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="5"></td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="CargoTypeLabel" CssClass="DetailsItem">Cargo Type:</asp:Label>
                                </td>
                                <td colspan="4" style="white-space: nowrap">
                                    <edi:ZDropDownList runat="server" ID="PackingMode" BindTo="JS_PackingMode" DisplayStyle="CodeAndDescription" AutoPostBack="true" OnSelectedIndexChanged="OnPackingModeChanged" />
                                </td>
                            </tr>
                            <tr runat="server" id="OriginDestinationRow">
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="OriginLabel" CssClass="DetailsItem">Origin:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZFindBox runat="server" ID="Origin" BindTo="JS_RL_NKOrigin" ModuleID="RefUNLOCOWeb" />
                                </td>
                                <td>&nbsp;</td>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="ETDLabel" CssClass="DetailsItem">ETD:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZDateEdit runat="server" ID="ETD" BindTo="JS_E_DEP" />
                                </td>
                            </tr>
                            <tr runat="server" id="EtdEtaRow">
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="DestinationLabel" CssClass="DetailsItem">Destination:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZFindBox runat="server" ID="Destination" BindTo="JS_RL_NKDestination" ModuleID="RefUNLOCOWeb" />
                                </td>
                                <td>&nbsp;</td>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="ETALabel" CssClass="DetailsItem">ETA:</asp:Label>
                                </td>
                                <td style="white-space: nowrap">
                                    <edi:ZDateEdit runat="server" ID="ETA" BindTo="JS_E_ARV" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="5"></td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="PaymentTermLabel" CssClass="DetailsItem">Payment Term:</asp:Label>
                                </td>
                                <td colspan="4" style="white-space: nowrap">
                                    <edi:ZDropDownList runat="server" ID="PaymentTerm" BindTo="JS_INCO" DisplayStyle="CodeAndDescription" ShowEmptyItem="true" />
                                </td>
                            </tr>
                            <tr runat="server" id="GoodsDescriptionRow">
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="Label1" CssClass="DetailsItem">Goods Description:</asp:Label>
                                </td>
                                <td colspan="4">
                                    <edi:ZTextBox runat="server" ID="GoodsDescription" BindTo="JS_GoodsDescription" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="5"></td>
                            </tr>
                        </table>
                        <table class="ResultsTable">
                            <tr runat="server">
                                <td style="white-space: nowrap">
                                    <asp:Label runat="server" ID="Label2" CssClass="DetailsItem">Detailed Goods Description:</asp:Label>
                                </td>
                                <td>
                                    <edi:ZTextBox runat="server" ID="DetailedGoodsDescription" BindTo="UserEditableNoteHelper.EditableNoteText" Width="100%" TextMode="MultiLine" Rows="4" Columns="100" />
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div runat="server" id="SailingPanel">
                        <edi:ZTextLabel ID="SailingLabel" runat="server" CssClass="SectionTitle">Sailing Details</edi:ZTextLabel>
                        <edi:ZGuidFindBox ID="Sailing" runat="server" BindTo="JS_JX" BindToList="Sailings" ModuleID="TrackingSailingSchedules" AutoPostBack="true" HideTextBox="true" OnTextChanged="OnSailingChanged" />
                        <div class="ContentSection">
                            <table class="ResultsTable">
                                <tr>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="VoyageNoLabel" CssClass="DetailsItem">Voyage No.:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZTextLabel runat="server" ID="VoyageFlight" BindTo="Sailing.JX_JV_VoyageFlight" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="VesselLabel" CssClass="DetailsItem">Vessel Name:</asp:Label>
                                    </td>
                                    <td>
                                        <edi:ZTextLabel runat="server" ID="Vessel" BindTo="Sailing.JX_JV_NKVessel" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="ReceivalStartDateLabel" CssClass="DetailsItem">Receival Start Date:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZDateTimeLabel runat="server" ID="ReceivalStartDate" BindTo="Sailing.JX_JA_CTOReceivalCommences" DateTimeFormat="Long" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="ReceivalEndDateLabel" CssClass="DetailsItem">Receival End Date:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZDateTimeLabel runat="server" ID="ReceivalEndDate" BindTo="Sailing.JX_JA_CTOCutOff" DateTimeFormat="Long" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="LoadPortLabel" CssClass="DetailsItem">Load:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZFindBox runat="server" ID="LoadPort" BindTo="JS_NKLoadPort" ModuleID="RefUNLOCOWeb" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="DischargePortLabel" CssClass="DetailsItem">Discharge:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZFindBox runat="server" ID="DischargePort" BindTo="JS_NKDischargePort" ModuleID="RefUNLOCOWeb" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="SailingETDLabel" CssClass="DetailsItem">ETD:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZDateTimeLabel runat="server" ID="SailingETD" BindTo="Sailing.JX_JA_E_DEP" DateTimeFormat="Long" />
                                    </td>
                                    <td>&nbsp;</td>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="SailingETALabel" CssClass="DetailsItem">ETA:</asp:Label>
                                    </td>
                                    <td style="white-space: nowrap">
                                        <edi:ZDateTimeLabel runat="server" ID="SailingETA" BindTo="Sailing.JX_JB_E_ARV" DateTimeFormat="Long" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="CarrierLabel" CssClass="DetailsItem">Carrier:</asp:Label>
                                    </td>
                                    <td colspan="4">
                                        <edi:ZFindBoxLabel runat="server" ID="Carrier" BindTo="BookedShippingLinePK" DisplayStyle="DescriptionOnly" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="white-space: nowrap">
                                        <asp:Label runat="server" ID="PrincipalLabel" CssClass="DetailsItem">Principal:</asp:Label>
                                    </td>
                                    <td colspan="4">
                                        <edi:ZGuidDropDownList runat="server" ID="Principal" BindTo="JS_OH_DeliveryAgent" DisplayStyle="DescriptionOnly" DataTextField="OH_FullName" DataValueField="PK" ShowEmptyItem="true" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                    <div runat="server" id="ContainersPanel">
                        <edi:ZTextLabel ID="ContainersLabel" runat="server" CssClass="SectionTitle">Containers</edi:ZTextLabel>
                        <div class="ContentSection">
                            <edi:ZDataGrid ID="BookedContainersGrid" runat="server" CssClass="DetailsTable" BindTo="FCLBookedContainers"
                                AllowAdd="true" AllowDelete="true" AllowEdit="true" AutoGenerateColumns="False"
                                AllowPaging="False">
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZDataGrid>
                        </div>
                    </div>
                    <div runat="server" id="PacksPanel">
                        <edi:ZTextLabel ID="PacksLabel" runat="server" CssClass="SectionTitle">Packs</edi:ZTextLabel>
                        <div class="ContentSection">
                            <edi:ZDataGrid ID="PacksGrid" runat="server" CssClass="DetailsTable" BindTo="Cargo"
                                AllowAdd="true" AllowDelete="true" AllowEdit="true" AutoGenerateColumns="False"
                                AllowPaging="False">
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZDataGrid>
                        </div>
                    </div>
                    <edi:ZCollapsablePanel ID="MilestonesPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Milestones">
                        <edi:ZDataGrid ID="MilestonesGrid" runat="server" CssClass="DetailsTable" AutoGenerateColumns="False">
                            <ItemStyle CssClass="DetailsCell"></ItemStyle>
                            <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                        </edi:ZDataGrid>
                    </edi:ZCollapsablePanel>
                    <div id="NotesPanel" runat="server">
                        <edi:ZTextLabel ID="NotesLabel" runat="server" CssClass="SectionTitle">Notes</edi:ZTextLabel>
                        <div class="ContentSection">
                            <edi:ZNotesControl ID="Notes" runat="server" BindTo="NotesHelper.VisibleNotes" />
                        </div>
                    </div>
                    <div id="WebUserNotePanel" runat="server" class="ContentSection">
                        <edi:ZTextLabel ID="WebUserNoteLabel" runat="server" CssClass="SectionTitle">To change other details on the booking please provide them below:</edi:ZTextLabel>
                        <br />
                        <edi:ZTextBox ID="WebUserNote" runat="server" Width="100%"
                            Rows="4" TextMode="MultiLine"></edi:ZTextBox>
                    </div>
                    <div class="ContentSection">
                        <asp:Button ID="SaveButton" runat="server" Text="Save Booking" ToolTip="Save booking into the system" OnClick="SaveButton_Click"></asp:Button>&nbsp;
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
