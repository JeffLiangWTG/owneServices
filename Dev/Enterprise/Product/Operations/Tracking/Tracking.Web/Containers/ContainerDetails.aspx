<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ register tagprefix="edi_tracking" namespace="Enterprise.Tracking.Web" assembly="Enterprise.Tracking.Web" %>

<%@ page language="c#" codebehind="ContainerDetails.aspx.cs" autoeventwireup="True" inherits="Enterprise.Tracking.Web.ContainerDetails" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Container Details</title>
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
                    <div id="EditButtonDiv" runat="server" class="ContentSection">
                        <asp:button id="EditContainer" runat="server" text="Edit Container" onclick="EditContainer_Click"></asp:button>
                        &nbsp;
                    </div>
                    <table class="ResultsTable">
                        <tbody>
                            <tr id="MasterBillRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="MasterBillCaption" runat="server" cssclass="DetailsItem">Master Bill:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="MasterBillLabel" runat="server" bindto="MasterBillNumber"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="Ztextlabel30" runat="server" cssclass="DetailsItem">Status:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="Ztextlabel8" runat="server" bindto="StatusDescription"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="ContainerStatusRow">
                                <td colspan="3" style="height: 21px;">&nbsp;</td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ContainerStatusCaption" runat="server" cssclass="DetailsItem">Container Status:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="ContainerStatusLabel" runat="server" bindto="ContainerStatus"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="ConsolNumberRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="ConsoleNumberCaption" runat="server" cssclass="DetailsItem">Consol Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="ConsoleNumberLabel" runat="server" bindto="ConsolNumber"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="PlannedWeightCaption" runat="server" cssclass="DetailsItem">Weight:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="PlannedWeightLabel" runat="server" bindto="WeightWithUnits"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="TypeRow">
                                <td style="width: 134px; height: 20px;">
                                    <edi:ztextlabel id="Ztextlabel19" runat="server" cssclass="DetailsItem">Type:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:ztextlabel id="Ztextlabel3" runat="server" bindto="TypeDescription"></edi:ztextlabel>
                                </td>
                                <td style="height: 20px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="QuarantineCaption" runat="server" cssclass="DetailsItem">Quarantine:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">&nbsp;<edi:ztextlabel id="QuarantineLabel" runat="server" bindto="QuarantineCode"></edi:ztextlabel></td>
                            </tr>
                            <tr id="Packagesow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="Ztextlabel20" runat="server" cssclass="DetailsItem">Packages:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px;">
                                    <edi:ztextlabel id="Ztextlabel4" runat="server" bindto="Packs"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="PortTransportRefCaption" runat="server" cssclass="DetailsItem">Port Transport Ref:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="PortTransportRefLabel" runat="server" bindto="JC_DepartureCartageRef"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="ModeRow">
                                <td>
                                    <edi:ztextlabel id="ZTextLabel31" runat="server" cssclass="DetailsItem">Mode:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="ZTextLabel32" runat="server" bindto="Mode"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="SequenceCaption" runat="server" cssclass="DetailsItem">Delivery Sequence:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:znumericlabel id="SequenceLabel" runat="server" bindto="JC_DeliverySequence"></edi:znumericlabel>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px"></td>
                                <td style="width: 320px; height: 21px"></td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;"></td>
                                <td style="width: 322px; height: 21px"></td>
                            </tr>
                            <tr id="RequiredDeliveryRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="ZTextLabel37" runat="server" cssclass="DetailsItem">Required Delivery:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdatetimelabel id="RequiredDelivery" runat="server" bindto="RequiredDelivery" datetimeformat="Long" />
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ZTextLabel17" runat="server" cssclass="DetailsItem">Empty Ready for Pickup:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdatetimelabel id="EmptyReady" runat="server" bindto="EmptyReady" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr id="TransportBookedRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="ZTextLabel38" runat="server" cssclass="DetailsItem">Transport Booked:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdatetimelabel id="ConfirmedDelivery" runat="server" bindto="ConfirmedDelivery" datetimeformat="Long" />
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ZTextLabel35" runat="server" cssclass="DetailsItem">Empty Return By:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdatetimelabel id="EmptyPickup" runat="server" bindto="EmptyPickup" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr id="ActualDeliveryRow">
                                <td style="width: 134px; height: 20px">
                                    <edi:ztextlabel id="ZTextLabel39" runat="server" cssclass="DetailsItem">Actual Delivery:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:zdatetimelabel id="ActualDelivery" runat="server" bindto="ActualDelivery" datetimeformat="Long" />
                                </td>
                                <td style="height: 20px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ZTextLabel36" runat="server" cssclass="DetailsItem">Empty Returned On:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                    <edi:zdatetimelabel id="ActualDehire" runat="server" bindto="ActualDehire" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px"></td>
                                <td style="width: 320px; height: 21px"></td>
                                <td style="height: 21px"></td>
                                <td></td>
                                <td style="width: 322px; height: 21px"></td>
                            </tr>
                            <tr id="LoadPortRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="Ztextlabel21" runat="server" cssclass="DetailsItem">Load Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="Ztextlabel6" runat="server" bindto="PortOfLoading"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="Ztextlabel26" runat="server" cssclass="DetailsItem">Discharge Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="Ztextlabel11" runat="server" bindto="PortOfDischarge"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="VesselRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="Ztextlabel22" runat="server" cssclass="DetailsItem">Vessel:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="Ztextlabel7" runat="server" bindto="VesselName"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="Ztextlabel25" runat="server" cssclass="DetailsItem">Voyage:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="Ztextlabel12" runat="server" bindto="Voyage"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="PickupRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="PickupCaption" runat="server" cssclass="DetailsItem">Pickup:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="PickupLabel" runat="server" bindto="ConsignorsExtended"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="DeliverCaption" runat="server" cssclass="DetailsItem">Deliver:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="DeliverLabel" runat="server" bindto="Consignees"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px"></td>
                                <td style="width: 320px; height: 21px"></td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;"></td>
                                <td style="width: 322px; height: 21px"></td>
                            </tr>
                            <tr id="SpecialInstructionsRow">
                                <td style="vertical-align: top">
                                    <edi:ztextlabel id="SpecialInstructionCaption" runat="server" cssclass="DetailsItem">Special Instructions:</edi:ztextlabel>
                                </td>
                                <td colspan="4">
                                    <edi:ztextlabel id="SpecialInstructionLabel" runat="server" bindto="UserEditableNoteHelper.EditableNoteText" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <br />
                    <br />
                    <table class="ResultsTable" >
                        <tbody>
                            <tr id="VGMSpecificationRow">
                                <td colspan="4" >
                                   <edi:ztextlabel id="VGMSpecificationCaption" runat="server" cssclass="DetailsItem">VGM Specification:</edi:ztextlabel>
                                </td>
                            </tr>
                             <tr>
                                <td colspan="4" >
                                    <br />
                                </td>
                            </tr>
                            <tr id="VerifiedWeightRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedWeightCaption" runat="server" cssclass="DetailsItem">Verified Weight:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="VerifiedWeightLabel" runat="server" bindto="WeightWithUnits"></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedDateCaption" runat="server" cssclass="DetailsItem">Verified Date:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdatetimelabel id="VerifiedDateLabel" runat="server" bindto="JC_GrossWeightVerificationDateTime" datetimeformat="Long"></edi:zdatetimelabel>
                                </td>
                            </tr>
                             <tr id="VerifiedMethodRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="VerifiedMethodCaption" runat="server" cssclass="DetailsItem">Verified Method:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="VerifiedMethodLabel" runat="server" bindto="VerifiedMethod" ></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedByCompanyCaption" runat="server" cssclass="DetailsItem">Verified By Company:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="VerifiedByCompanyLabel" runat="server" bindto="VerifiedByCompany"></edi:ztextlabel>
                                </td>
                            </tr>
                             <tr id="VerifiedByPersonRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="VerifiedByPersonCaption" runat="server" cssclass="DetailsItem">Verified By Person:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="VerifiedByPersonLabel" runat="server" bindto="VerifiedByPerson"></edi:ztextlabel>
                                </td>
                                 <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedByPhoneCaption" runat="server" cssclass="DetailsItem">Verified By Phone:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="VerifiedByPhoneLabel" runat="server" bindto="VerifiedByPhone"></edi:ztextlabel>
                                </td>
                            </tr>
                             <tr id="VerifiedByEmailRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="VerifiedByEmailCaption" runat="server" cssclass="DetailsItem">Verified By Email:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px" colspan="3">
                                    <edi:ztextlabel id="VerifiedByEmailLabel" runat="server" bindto="VerifiedByEmail"></edi:ztextlabel>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <table border="0" cellpadding="0" cellspacing="0">
                        <tr>
                            <td align="left" valign="top">
                                <edi_tracking:milestonescontrol id="Milestones" runat="server" cssclass="DetailsTable" bindto="Milestones" itemcssclass="DetailsCell" headercssclass="DetailsHeader" displayinpanel="True" panelcssclass="SectionTitle">
							</edi_tracking:milestonescontrol>
                            </td>
                            <td align="left" valign="top">
                                <edi_tracking:eventscontrol id="TrackingEvents" runat="server" cssclass="DetailsTable" bindto="TrackingEvents" itemcssclass="DetailsCell" headercssclass="DetailsHeader" displayinpanel="True" panelcssclass="SectionTitle">
							</edi_tracking:eventscontrol>
                            </td>
                        </tr>
                    </table>
                    <edi:zgrid id="OrdersGrid" runat="server" autogeneratecolumns="False" bindto="OrderLines" disablecollapsing="True" label="Orders"
                        cssclass="DetailsTable">
							<HeaderStyle CssClass="DetailsHeader" />
							<PagerStyle Mode="NumericPages" PageButtonCount="20" />
							<ItemStyle CssClass="DetailsCell" />
						</edi:zgrid>
                    <br />
                    <edi:zgrid id="DocumentsGrid" runat="server" caption="Documents" cssclass="DetailsTable" bindto="DocumentHelper.Documents" disablecollapsing="True">
							<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:zgrid>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
