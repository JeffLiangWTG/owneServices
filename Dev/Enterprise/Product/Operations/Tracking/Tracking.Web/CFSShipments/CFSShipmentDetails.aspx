<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<%@ page language="c#" codebehind="CFSShipmentDetails.aspx.cs" autoeventwireup="True" inherits="Enterprise.Tracking.Web.CFSShipmentDetails" %>

<%@ register tagprefix="edi_tracking" namespace="Enterprise.Tracking.Web" assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>CFS Shipment Details</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ztextlabel id="ShipmentDetailsLabel" runat="server" cssclass="PageTitle">Shipment #</edi:ztextlabel>
                <edi:ztextlabel id="Ztextlabel5" runat="server" cssclass="PageTitle" bindto="JS_UniqueConsignRef"></edi:ztextlabel>
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <div class="ContentSection">
                    <div>
                        <edi_tracking:zdocumentsmenu id="DocsMenu" runat="server" />
                    </div>
                    <table class="ResultsTable">
                        <tbody>
                            <tr>
                                <td valign="top">
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="HouseBillRow">
                                            <td>
                                                <asp:label id="BillLabel" runat="server" cssclass="DetailsItem">Bill: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="HouseBill" runat="server" bindto="JS_HouseBill"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                         <tr id="OceanBillRow">
                                            <td>
                                                <asp:label id="MasterBillLabel" runat="server" cssclass="DetailsItem">Ocean Bill: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="MasterBilltextLabel" runat="server" bindto="MasterBill"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="ShipperRefRow">
                                            <td>
                                                <asp:label id="ClientShipperRefLabel" runat="server" cssclass="DetailsItem">Shipper's Ref#: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="ClientShipperRefData" runat="server" bindto="JS_BookingReference"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="ClientRefRow">
                                            <td>
                                                <asp:label id="ClientRefLabel" runat="server" cssclass="DetailsItem">Clients' Ref#: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="ClientReftextlabel" runat="server" bindto="JS_ConsolReference"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="InterimReceiptRow">
                                            <td>
                                                <asp:label id="InterimReceiptLabel" runat="server" cssclass="DetailsItem">Interim Receipt: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="InterimReceipttextlabel" runat="server" bindto="JS_InterimReceipt"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="WarehouseReceiptRow">
                                            <td>
                                                <asp:label id="WhReceiptLabel" runat="server" cssclass="DetailsItem">Whs. Receipt: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="WhsReceipttextlabel" runat="server" bindto="JS_A_RCV"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td valign="top" style="padding-left: 50px;">
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="OriginRow" runat="server">
                                            <td>
                                                <asp:label id="OriginLabel" runat="server" cssclass="DetailsItem">Origin: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zcodefindboxlabel id="Origin" runat="server" bindto="JS_RL_NKOrigin" displaystyle="CodeAndDescription"
                                                    bindtolist="Lookups.RefUNLOCO_List"></edi:zcodefindboxlabel>
                                            </td>
                                        </tr>
                                        <tr id="ETDRow" runat="server">
                                            <td>
                                                <asp:label id="ETDLabel" runat="server" cssclass="DetailsItem">ETD: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="ETD" runat="server" bindto="ETDWithSuppression" datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td valign="top" style="padding-left: 50px;">
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="DestinationRow" runat="server">
                                            <td>
                                                <asp:label id="DestinationLabel" runat="server" cssclass="DetailsItem">Destination: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zcodefindboxlabel id="Destination" runat="server" bindto="JS_RL_NKDestination" displaystyle="CodeAndDescription"
                                                    bindtolist="Lookups.RefUNLOCO_List"></edi:zcodefindboxlabel>
                                            </td>
                                        </tr>
                                        <tr id="ETARow" runat="server">
                                            <td>
                                                <asp:label id="ETALabel" runat="server" cssclass="DetailsItem">ETA: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="ETA" runat="server" bindto="ETAWithSuppression"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top">
                                    <div id="ForAuthentifiedUserOnly" runat="server">
                                        <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                            <tr id="ShipperRow">
                                                <td>
                                                    <asp:label id="ShipperLabel" runat="server" cssclass="DetailsItem">Shipper: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="Shipper" runat="server" bindto="ConsignorDocumentaryAddress.E2_CompanyName"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr id="ConsigneeRow">
                                                <td>
                                                    <asp:label id="ConsigneeLabel" runat="server" cssclass="DetailsItem">Consignee: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="Consignee" runat="server" bindto="ConsigneeDocumentaryAddress.E2_CompanyName"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">&nbsp;
                                                </td>
                                            </tr>
                                            <tr id="SizeRow">
                                                <td>
                                                    <asp:label id="SizeLabel" runat="server" cssclass="DetailsItem">Size: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="PlannedVolumeLabel" runat="server" bindto="VolumeWithUnits"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr id="WeightRow">
                                                <td>
                                                    <asp:label id="WeightLabel" runat="server" cssclass="DetailsItem">Weight: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="PlannedWeightLabel" runat="server" bindto="WeightWithUnits"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr id="QuantityRow">
                                                <td>
                                                    <asp:label id="QuantityLabel" runat="server" cssclass="DetailsItem">Quantity: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:znumericlabel id="OuterPackCount" runat="server" bindto="JS_OuterPacks"></edi:znumericlabel>
                                                    <edi:ztextlabel id="PackType" runat="server" bindto="JS_F3_NKPackType"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="ServiceLevelRow">
                                            <td>
                                                <asp:label id="ServiceLevelLabel" runat="server" cssclass="DetailsItem">Service Level: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zcodefindboxlabel id="ServiceLevelDescription" runat="server" bindto="JS_RS_NKServiceLevel" displaystyle="CodeAndDescription"></edi:zcodefindboxlabel>
                                            </td>
                                        </tr>
                                        <tr id="WarehouseLocationRow">
                                            <td>
                                                <asp:label id="WhsLocationLabel" runat="server" cssclass="DetailsItem">Whs. Location: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="WhsLocationZcodefindboxlabel" runat="server" bindto="JS_WarehouseLocation"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="GoodsDescriptionRow">
                                            <td>
                                                <asp:label id="GoodsDescLabel" runat="server" cssclass="DetailsItem">Goods Description: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="GoodsDescription" runat="server" bindto="JS_GoodsDescription" designtimedragdrop="160"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="CustomsEntryNumberRow">
                                            <td>
                                                <asp:label id="EntryNoLabel" runat="server" cssclass="DetailsItem">Entry No.: </asp:label>
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="EntryNotextlabel" runat="server" bindto="CustomsEntryNumber" designtimedragdrop="160"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="PayTermsRow" runat="server">
                                            <td style="white-space: nowrap">
                                                <asp:label id="PayTermLabel" runat="server" cssclass="DetailsItem" text="Payment Term:"></asp:label>
                                                &nbsp;
                                            </td>
                                            <td style="white-space: nowrap">
                                                <edi:zcodelookuplabel id="PayTermCodeLookupLabel" runat="server" bindto="JS_INCO"
                                                    displaystyle="DescriptionOnly"></edi:zcodelookuplabel>
                                            </td>
                                        </tr>
                                        <tr id="AdditionalTermsRow" runat="server">
                                            <td style="white-space: nowrap">
                                                <asp:label runat="server" cssclass="DetailsItem">Additional Terms:</asp:label>
                                                &nbsp;
                                            </td>
                                            <td>
                                                <edi:ztextlabel id="AdditionalTermsText" runat="server" bindto="JS_AdditionalTerms" width="100%"></edi:ztextlabel>
                                            </td>
                                        </tr>
                                        <tr id="ReleaseTypeRow" runat="server">
                                            <td style="white-space: nowrap">
                                                <asp:label id="Label13" runat="server" cssclass="DetailsItem">Release Type:</asp:label>
                                                &nbsp;
                                            </td>
                                            <td>
                                                <edi:zcodelookuplabel id="ReleaseTypeCodeLookupLabel" runat="server" bindto="JS_ReleaseType" displaystyle="DescriptionOnly"></edi:zcodelookuplabel>
                                            </td>
                                        </tr>
                                        <tr id="OnBoardRow" runat="server">
                                            <td style="white-space: nowrap">
                                                <asp:label id="Label14" runat="server" cssclass="DetailsItem">On Board:</asp:label>
                                                &nbsp;
                                            </td>
                                            <td>
                                                <edi:zcodelookuplabel id="OnBoardCodeLookupLabel" runat="server" bindto="JS_ShippedOnBoard" displaystyle="DescriptionOnly"></edi:zcodelookuplabel>
                                            </td>
                                        </tr>
                                        <tr id="ChargesApplyRow" runat="server">
                                            <td style="white-space: nowrap">
                                                <asp:label id="Label15" runat="server" cssclass="DetailsItem">Charges Apply:</asp:label>
                                                &nbsp;
                                            </td>
                                            <td>
                                                <edi:zcodelookuplabel id="ChargesApplyCodeLookupLabel" runat="server" bindto="JS_HBLAWBChargesDisplay" displaystyle="DescriptionOnly"></edi:zcodelookuplabel>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">&nbsp;
                                            </td>
                                        </tr>
                                    </table>
                                    <div id="ForAuthentifiedUserOnly4" runat="server">
                                        <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                            <tr id="PickupAgentRow">
                                                <td>
                                                    <asp:label id="Label17" runat="server" cssclass="DetailsItem">Pickup Agent: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="PickupAgent" runat="server" bindto="PickupAgentFullName"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr id="DeliveryAgentRow">
                                                <td>
                                                    <asp:label id="Label18" runat="server" cssclass="DetailsItem">Delivery Agent: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="DeliveryAgent" runat="server" bindto="DeliveryAgentFullName"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                                <td valign="top" style="padding-left: 50px;">
                                    <div id="ForAuthentifiedUserOnly2" runat="server">
                                        <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                            <tr id="ConsignorPickupRow">
                                                <td>
                                                    <asp:label id="Label2" runat="server" cssclass="DetailsItem">Pickup From: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="Ztextlabel3" runat="server" bindto="ConsignorPickupAddress.AddressAsASingleLine"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">&nbsp;
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">&nbsp;
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr>
                                            <td colspan="2">&nbsp;
                                            </td>
                                        </tr>
                                        <tr id="StorageCommencesParallelRow" runat="server">
                                            <td colspan="2">&nbsp;
                                            </td>
                                        </tr>
                                        <tr id="EstimatedPickupRow">
                                            <td>
                                                <asp:label id="Label3" runat="server" cssclass="DetailsItem">Estimated Pickup: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel1" runat="server" bindto="DocsAndCartage.JP_EstimatedPickup" datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="PickupRequiredByRow">
                                            <td>
                                                <asp:label id="Label4" runat="server" cssclass="DetailsItem">Pickup Required By: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel2" runat="server" bindto="DocsAndCartage.JP_PickupRequiredBy"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="PickupCartageAdvisedRow" runat="server">
                                            <td>
                                                <asp:label id="Label16" runat="server" cssclass="DetailsItem">Pickup Cartage Advised: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel9" runat="server" bindto="DocsAndCartage.JP_PickupCartageAdvised"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="GoodsPickedUpRow">
                                            <td>
                                                <asp:label id="Label5" runat="server" cssclass="DetailsItem">Goods Picked Up: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel3" runat="server" bindto="DocsAndCartage.JP_PickupCartageCompleted"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td valign="top" style="padding-left: 50px;">
                                    <div id="ForAuthentifiedUserOnly3" runat="server">
                                        <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                            <tr id="ConsigneeDeliverToRow">
                                                <td>
                                                    <asp:label id="Label6" runat="server" cssclass="DetailsItem">Deliver To: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="Ztextlabel4" runat="server" bindto="ConsigneeDeliveryAddress.AddressAsASingleLine"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr id="AvailableAtRow">
                                                <td>
                                                    <asp:label id="Label12" runat="server" cssclass="DetailsItem">Available At: </asp:label>
                                                </td>
                                                <td>
                                                    <edi:ztextlabel id="AvailableAtAddressAsTextLabel" runat="server" bindto="AvailableAtAddressAsText"></edi:ztextlabel>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">&nbsp;
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="AvailabilityRow">
                                            <td>
                                                <asp:label id="Label7" runat="server" cssclass="DetailsItem">Availability: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel4" runat="server" bindto="AvailableDate" datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="StorageCommencesRow" runat="server">
                                            <td>
                                                <asp:label id="Label8" runat="server" cssclass="DetailsItem">Storage Commences: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel5" runat="server" bindto="StorageDate" datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="EstimatedDeliveryRow">
                                            <td>
                                                <asp:label id="Label9" runat="server" cssclass="DetailsItem">Estimated Delivery: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel6" runat="server" bindto="DocsAndCartage.JP_EstimatedDelivery"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="DeliveryRequiredByRow">
                                            <td>
                                                <asp:label id="Label10" runat="server" cssclass="DetailsItem">Delivery Required By: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel7" runat="server" bindto="DocsAndCartage.JP_DeliveryRequiredBy"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="CartageAdvisedRow" runat="server">
                                            <td>
                                                <asp:label id="Label11" runat="server" cssclass="DetailsItem">Delivery Cartage Advised: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="Zdatetimelabel8" runat="server" bindto="DocsAndCartage.JP_DeliveryCartageAdvised"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                        <tr id="GoodsDeliveredRow">
                                            <td>
                                                <asp:label id="DeliveredLabel" runat="server" cssclass="DetailsItem">Goods Delivered: </asp:label>
                                            </td>
                                            <td>
                                                <edi:zdatetimelabel id="ReceivedDate" runat="server" bindto="DocsAndCartage.JP_DeliveryCartageCompleted"
                                                    datetimeformat="Long"></edi:zdatetimelabel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
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
                <edi:zgrid id="TransportGrid" runat="server" caption="Transport" cssclass="DetailsTable" bindto="RelatedTransportsInLegOrder" disablecollapsing="true">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				</edi:zgrid>
                  <edi:zgrid id="DeliveryInformationGrid" runat="server" caption="Delivery Information" cssclass="DetailsTable" bindto="RelatedCommonPickupDeliveryConfirm" disablecollapsing="true">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				</edi:zgrid>
                <edi:zgrid id="PackLinesGrid" runat="server" caption="Goods / Packs" cssclass="DetailsTable" bindto="OuterPackLines" disablecollapsing="true">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>

                <edi:zgrid id="DocumentsGrid" runat="server" caption="Documents" cssclass="DetailsTable" bindto="DocumentHelper.Documents" disablecollapsing="true">
						<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
                <br />
                <br />
                <div id="StatusHolder" runat="server" visible="false" class="ShipmentStatusHolder">
                </div>
                <edi:zcollapsablepanel id="NotesPanel" runat="server" cssclass="SectionTitle" disablecollapsing="True" label="Notes">
						<edi:znotescontrol id="notes" runat="server" BindTo="NotesHelper.VisibleNotes"></edi:znotescontrol>
					</edi:zcollapsablepanel>
            </div>
            <div id="NotFoundError" runat="server">
                <edi:ztextlabel id="ShipmentNotFoundLabel" runat="server" cssclass="PageTitle"></edi:ztextlabel>
            </div>
        </div>
    </form>
</body>
</html>
