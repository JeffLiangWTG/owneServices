<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<%@ page language="c#" codebehind="EditContainer.aspx.cs" autoeventwireup="True" inherits="Enterprise.Tracking.Web.EditContainer" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Edit Container</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="UnauthorisedDiv" class="ContentSection" runat="server">
                <edi:ztextlabel id="UnauthorisedLabel" runat="server" cssclass="PageTitle"></edi:ztextlabel>
            </div>
            <div id="AuthorisedContent" runat="server" class="ContentSection">
                <div id="NotFoundError" runat="server">
                    <edi:ztextlabel id="NotFoundLabel" runat="server" cssclass="PageTitle"></edi:ztextlabel>
                </div>
                <div id="ContainerContents" runat="server">
                    <div id="Title">
                        <edi:ztextlabel id="ContainerDetailsLabel" runat="server" cssclass="PageTitle">Container #</edi:ztextlabel>
                        <edi:ztextlabel id="Ztextlabel5" runat="server" cssclass="PageTitle" bindto="JC_ContainerNum"></edi:ztextlabel>
                    </div>
                    <table class="ResultsTable">
                        <tbody>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel29" runat="server" cssclass="DetailsItem">Master Bill:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel9" runat="server" bindto="MasterBillNumber"></edi:ztextlabel>
                                </td>
                                <td>&nbsp;&nbsp;&nbsp;&nbsp;
                                </td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel30" runat="server" cssclass="DetailsItem">Status:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px;">
                                    <edi:ztextlabel id="Ztextlabel8" runat="server" bindto="StatusDescription"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3">&nbsp;</td>
                                <td>
                                    <edi:ztextlabel id="ContainerStatusCaption" runat="server" cssclass="DetailsItem">Container Status:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px;">
                                    <edi:ztextlabel id="ContainerStatus" runat="server" bindto="ContainerStatus"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel18" runat="server" cssclass="DetailsItem">Consol Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="Ztextlabel2" runat="server" bindto="ConsolNumber"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel28" runat="server" cssclass="DetailsItem">Weight:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="PlannedWeightLabel" runat="server" bindto="WeightWithUnits"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel19" runat="server" cssclass="DetailsItem">Type:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="Ztextlabel3" runat="server" bindto="TypeDescription"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel33" runat="server" cssclass="DetailsItem">Quarantine:</edi:ztextlabel>
                                </td>
                                <td>&nbsp;<edi:ztextlabel id="ZTextLabel34" runat="server" bindto="QuarantineCode"></edi:ztextlabel></td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel20" runat="server" cssclass="DetailsItem">Packages:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="Ztextlabel4" runat="server" bindto="Packs"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="ZtextlabelPortTransportRef" runat="server" cssclass="DetailsItem">Port Transport Ref:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextbox id="ZTextBox" runat="server" bindto="JC_DepartureCartageRef"></edi:ztextbox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel31" runat="server" cssclass="DetailsItem">Mode:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="ZTextLabel32" runat="server" bindto="Mode"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel15" runat="server" cssclass="DetailsItem">Delivery Sequence:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:znumerictextbox id="Sequence" runat="server" bindto="JC_DeliverySequence" />
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td style="width: 123px"></td>
                                <td></td>
                                <td></td>
                                <td></td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel36" runat="server" cssclass="DetailsItem">Required Delivery:</edi:ztextlabel>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:zdateedit id="RequiredDelivery" runat="server" bindto="RequiredDelivery" datetimeformat="Long" />
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel1" runat="server" cssclass="DetailsItem">Empty Ready for Pickup:</edi:ztextlabel>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:zdateedit id="EmptyReady" runat="server" bindto="EmptyReady" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel37" runat="server" cssclass="DetailsItem">Transport Booked:</edi:ztextlabel>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:zdateedit id="ConfirmedDelivery" runat="server" bindto="ConfirmedDelivery" datetimeformat="Long" />
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel17" runat="server" cssclass="DetailsItem">Empty Return By:</edi:ztextlabel>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:zdateedit id="EmptyPickup" runat="server" bindto="EmptyPickup" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel38" runat="server" cssclass="DetailsItem">Actual Delivery:</edi:ztextlabel>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:zdateedit id="ActualDelivery" runat="server" bindto="ActualDelivery" datetimeformat="Long" />
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel35" runat="server" cssclass="DetailsItem">Empty Returned On:</edi:ztextlabel>
                                </td>
                                <td nowrap="nowrap">
                                    <edi:zdateedit id="ActualDehire" runat="server" bindto="ActualDehire" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td></td>
                                <td></td>
                                <td></td>
                                <td></td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel21" runat="server" cssclass="DetailsItem">Load Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="Ztextlabel6" runat="server" bindto="PortOfLoading"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel26" runat="server" cssclass="DetailsItem">Discharge Port:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel11" runat="server" bindto="PortOfDischarge"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel22" runat="server" cssclass="DetailsItem">Vessel:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="Ztextlabel7" runat="server" bindto="VesselName"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel25" runat="server" cssclass="DetailsItem">Voyage:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel12" runat="server" bindto="Voyage"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel23" runat="server" cssclass="DetailsItem">Pickup:</edi:ztextlabel>
                                </td>
                                <td style="width: 123px">
                                    <edi:ztextlabel id="ZTextLabel16" runat="server" bindto="ConsignorsExtended"></edi:ztextlabel>
                                </td>
                                <td></td>
                                <td>
                                    <edi:ztextlabel id="Ztextlabel24" runat="server" cssclass="DetailsItem">Deliver:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel13" runat="server" bindto="ConsigneesExtended"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td style="width: 123px"></td>
                                <td style="height: 21px"></td>
                                <td></td>
                                <td></td>
                            </tr>
                            <tr>
                                <td style="vertical-align: top;">
                                    <edi:ztextlabel id="ZTextLabel39" runat="server" cssclass="DetailsItem">Special Instructions:</edi:ztextlabel>
                                </td>
                                <td colspan="4">
                                    <edi:ztextbox id="SpecialInstructions" runat="server" bindto="UserEditableNoteHelper.EditableNoteText"
                                        rows="4" textmode="MultiLine" width="100%"></edi:ztextbox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="5">
                                    <br />
                                    <br />
                                    <br />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="5">
                                    <edi:ztextlabel id="VGMSpecificationCaption" runat="server" cssclass="DetailsItem">VGM Specification:</edi:ztextlabel>
                                    <br />
                                    <br />
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedWeightCaption" runat="server" cssclass="DetailsItem">Verified Weight:</edi:ztextlabel>
                                </td>
                                <td nowrap colspan="4">
                                    <edi:znumerictextbox id="VerifiedWeightEdit" runat="server" bindto="JC_GrossWeight" width="88px" autopostback="true"></edi:znumerictextbox>
                                    <edi:zdropdownlist id="VerifiedWeightUQDropDown" runat="server" bindto="WeightUnitForBinding" autopostback="true" onselectedindexchanged="DropDown_SelectedIndexChanged"></edi:zdropdownlist>
                                </td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="VerifiedMethodCaption" runat="server" cssclass="DetailsItem">Verified Method:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px" colspan="4">
                                    <edi:zdropdownlist id="VerifiedMethodDropDownList" runat="server"
                                        bindto="JC_GrossWeightVerificationType" datavaluefield="Code" datatextfield="Description"
                                        showemptyitem="True" autopostback="true" onselectedindexchanged="DropDown_SelectedIndexChanged">
									  </edi:zdropdownlist>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedDateCaption" runat="server" cssclass="DetailsItem">Verified Date:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px" colspan="4">
                                    <edi:zdateedit id="VerifiedDateEdit" runat="server" bindto="JC_GrossWeightVerificationDateTime" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="5">
                                    <edi:zdocaddresswebcontrol id="VerifiedCompanyAddress" runat="server" bindto="GrossWeightVerifiedByAddress" caption="Verified Company Address"
                                        savecheckboxcaption="Save Verified Address" isconsignor="false" isconsignee="false" residentialcheckboxvisible="false"
                                        savecheckboxvisible="true" dependentportcontrol="OriginPort_TextBox" />
                                    <br>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <edi:zcollapsablepanel id="OrdersPanel" runat="server" cssclass="SectionTitle" disablecollapsing="True" label="Orders">
                        <edi:ZDataGrid ID="OrdersGrid" runat="server" AutoGenerateColumns="False" BindTo="OrderLines"
                            CssClass="DetailsTable">
                            <HeaderStyle CssClass="DetailsHeader" />
                            <PagerStyle Mode="NumericPages" PageButtonCount="20" />
                            <ItemStyle CssClass="DetailsCell" />
                        </edi:ZDataGrid>
                    </edi:zcollapsablepanel>
                    <br />
                    <edi:zcollapsablepanel id="DocumentsPanel" runat="server" cssclass="SectionTitle" disablecollapsing="True" label="Documents">
                        <edi:ZDataGrid ID="DocumentsGrid" runat="server" AutoGenerateColumns="False" BindTo="DocumentHelper.Documents"
                            CssClass="DetailsTable">
                            <HeaderStyle CssClass="DetailsHeader" />
                            <PagerStyle Mode="NumericPages" PageButtonCount="20" />
                            <ItemStyle CssClass="DetailsCell" />
                        </edi:ZDataGrid>
                    </edi:zcollapsablepanel>
                    <div class="ContentSection">
                        <asp:button id="SaveContainer" runat="server" text="Save" onclick="SaveContainer_Click" style="min-width: 60px;"></asp:button>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
