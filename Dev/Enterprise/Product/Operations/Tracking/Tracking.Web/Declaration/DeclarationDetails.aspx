<%@ Page Language="c#" CodeBehind="DeclarationDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.DeclarationDetails" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>Declaration Details</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ZTextLabel ID="DeclarationTitleLabel" runat="server" CssClass="PageTitle">Declaration #</edi:ZTextLabel>
                <edi:ZTextLabel ID="ZTextLabel5" runat="server" CssClass="PageTitle" BindTo="Declaration.JE_DeclarationReference"></edi:ZTextLabel>
                <edi:ZTextLabel runat="server" ID="ReleaseStatusDescText" CssClass="PageTitle MessageStatus" BindTo="ReleaseStatusDesc" HideIfBlank="true" />
            </div>
            <div class="SubPageTitle">
                <asp:Label ID="EntryNumberLabel" runat="server" Font-Bold="true">Entry Number: </asp:Label><edi:ZTextLabel ID="EntryNumber" runat="server" BindTo="FormattedEntryNumber" />
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <div class="ContentSection">
                    <table class="ResultsTable" id="Table1">
                        <tbody>
                            <tr id="MasterHouseBillRow">
                                <td>
                                    <asp:Label ID="MasterBillLabel" runat="server" CssClass="DetailsItem">Master Bill: </asp:Label><edi:ZTextLabel ID="MasterBill2" runat="server" BindTo="Declaration.JE_MasterBillForGenericWrapper" /></td>
                                <td></td>
                                <td>
                                    <asp:Label ID="HouseBillLabel" runat="server" CssClass="DetailsItem">House Bill: </asp:Label><edi:ZTextLabel ID="HouseBill" runat="server" BindTo="Declaration.JE_HouseBillForGenericWrapper" /></td>
                            </tr>
                            <tr>
                                <td valign="top">
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="OwnerReferenceNumberRow">
                                            <td colspan="3">
                                                <asp:Label ID="Label1" runat="server" CssClass="DetailsItem">Owner's Ref#: </asp:Label><edi:ZTextLabel ID="ZTextLabel1" runat="server" BindTo="Declaration.JE_OwnerRef"></edi:ZTextLabel><br />
                                            </td>
                                        </tr>
                                        <tr id="OrderReferenceNumberRow">
                                            <td colspan="3">
                                                <asp:Label ID="Label4" runat="server" CssClass="DetailsItem">Order Ref#: </asp:Label><edi:ZTextLabel ID="ZTextLabel7" runat="server" BindTo="OrderReference"></edi:ZTextLabel>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">&nbsp;</td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">
                                                <div id="ForAuthentifiedUserOnly" runat="server">
                                                    <asp:Label ID="ShipperLabel" runat="server" CssClass="DetailsItem">Main Supplier: </asp:Label><edi:ZFindBoxLabel ID="Shipper" runat="server" BindTo="Declaration.JE_OH_Supplier" BindToList="Declaration.Lookups.SuppliersList"
                                                        DisplayStyle="DescriptionOnly"></edi:ZFindBoxLabel><br>
                                                    <asp:Label ID="ConsigneeLabel" runat="server" CssClass="DetailsItem">
											            Importer: </asp:Label><edi:ZFindBoxLabel ID="Consignee" runat="server" BindTo="Declaration.JE_OH_Importer" BindToList="Declaration.Lookups.ImportersList"
                                                            DisplayStyle="DescriptionOnly"></edi:ZFindBoxLabel><br>
                                                    <br>
                                                    <asp:Label ID="SizeLabel" runat="server" CssClass="DetailsItem">Size: </asp:Label><edi:ZNumericLabel ID="ActualVolume" runat="server" BindTo="Declaration.JE_TotalVolume"></edi:ZNumericLabel><edi:ZTextLabel ID="UnitOfVolume" runat="server" BindTo="Declaration.JE_TotalVolumeUnit"></edi:ZTextLabel><br>
                                                    <asp:Label ID="WeightLabel" runat="server" CssClass="DetailsItem">Weight: </asp:Label><edi:ZNumericLabel ID="ActualWeight" runat="server" BindTo="Declaration.JE_TotalWeight"></edi:ZNumericLabel><edi:ZTextLabel ID="UnitOfWeight" runat="server" BindTo="Declaration.JE_TotalWeightUnit"></edi:ZTextLabel><br>
                                                    <asp:Label ID="QuantityLabel" runat="server" CssClass="DetailsItem">Quantity: </asp:Label><edi:ZNumericLabel ID="OuterPackCount" runat="server" BindTo="Declaration.JE_TotalNoOfPacks"></edi:ZNumericLabel><edi:ZTextLabel ID="PackType" runat="server" BindTo="Declaration.JE_TotalNoOfPacksPackType"></edi:ZTextLabel>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">&nbsp;</td>
                                        </tr>
                                        <tr id="ServiceLevelRow">
                                            <td colspan="3">
                                                <asp:Label ID="ServiceLevelLabel" runat="server" CssClass="DetailsItem">Service Level: </asp:Label><edi:ZCodeFindBoxLabel ID="ServiceLevelDescription" runat="server" BindTo="Declaration.JE_RS_NKServiceLevel"
                                                    BindToList="Declaration.Lookups.ServiceLevels" DisplayStyle="CodeAndDescription"></edi:ZCodeFindBoxLabel>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td width="50"></td>
                                <td valign="top">
                                    <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                        <tr id="OriginDestinationRow">
                                            <td>
                                                <asp:Label ID="OriginLabel" runat="server" CssClass="DetailsItem">Origin: </asp:Label><edi:ZCodeFindBoxLabel ID="Origin" runat="server" BindTo="Declaration.JE_RL_NKOrigin" BindToList="Declaration.Lookups.Origins"
                                                    DisplayStyle="CodeAndDescription"></edi:ZCodeFindBoxLabel></td>
                                            <td width="50"></td>
                                            <td>
                                                <asp:Label ID="DestinationLabel" runat="server" CssClass="DetailsItem">Destination: </asp:Label><edi:ZCodeFindBoxLabel ID="Destination" runat="server" BindTo="Declaration.JE_RL_NKFinalDestination" BindToList="Declaration.Lookups.FinalDestinations"
                                                    DisplayStyle="CodeAndDescription"></edi:ZCodeFindBoxLabel></td>
                                        </tr>
                                        <tr id="EtaEtdRow">
                                            <td>
                                                <asp:Label ID="ETDLabel" runat="server" CssClass="DetailsItem">ETD: </asp:Label>
                                                <edi:ZDateTimeLabel ID="ETD" runat="server" BindTo="DateAtOriginWithSuppression" DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                            <td width="50"></td>
                                            <td>
                                                <asp:Label ID="ETALabel" runat="server" CssClass="DetailsItem">ETA: </asp:Label>
                                                <edi:ZDateTimeLabel ID="ETA" runat="server" BindTo="Declaration.JE_DateAtFinalDestination"></edi:ZDateTimeLabel></td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">&nbsp;</td>
                                        </tr>
                                        <tr id="ForAuthentifiedUserOnly2" runat="server">
                                            <td>
                                                <asp:Label ID="Label2" runat="server" CssClass="DetailsItem">Pickup from: </asp:Label><edi:ZTextLabel ID="ZTextLabel3" runat="server" BindTo="PickupAddressAsText"></edi:ZTextLabel></td>
                                            <td width="50"></td>
                                            <td>
                                                <asp:Label ID="Label6" runat="server" CssClass="DetailsItem">Deliver to: </asp:Label><edi:ZTextLabel ID="ZTextLabel4" runat="server" BindTo="DeliveryAddressAsText"></edi:ZTextLabel></td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">&nbsp;</td>
                                        </tr>
                                        <tr>
                                            <td colspan="3">
                                                <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                                    <tr id="AvailabilityRow">
                                                        <td>
                                                            <asp:Label ID="Label7" runat="server" CssClass="DetailsItem">Availability</asp:Label></td>
                                                        <td>
                                                            <asp:Label ID="Label5" runat="server" CssClass="DetailsItem">&nbsp;&nbsp;&nbsp;FCL:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel4" runat="server" BindTo="Declaration.DocsAndCartage.JP_FCLAvailable"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                        <td>
                                                            <asp:Label ID="Label9" runat="server" CssClass="DetailsItem">&nbsp;&nbsp;&nbsp;LCL:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel1" runat="server" BindTo="Declaration.DocsAndCartage.JP_LCLAvailable"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                    </tr>
                                                    <tr id="StorageCommencesRow">
                                                        <td>
                                                            <asp:Label ID="Label8" runat="server" CssClass="DetailsItem">Storage commences</asp:Label></td>
                                                        <td>
                                                            <asp:Label ID="Label3" runat="server" CssClass="DetailsItem">&nbsp;&nbsp;&nbsp;FCL:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel5" runat="server" BindTo="Declaration.DocsAndCartage.JP_FCLStorageCommences"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                        <td>
                                                            <asp:Label ID="Label10" runat="server" CssClass="DetailsItem">&nbsp;&nbsp;&nbsp;LCL:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel2" runat="server" BindTo="Declaration.DocsAndCartage.JP_LCLStorageCommences"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                    </tr>
                                                </table>
                                                <br>
                                                <table class="ResultsTable" cellspacing="0" cellpadding="0">
                                                    <tr id="EstimatedDeliveryRow">
                                                        <td>
                                                            <asp:Label ID="EstDeliveryLabel" runat="server" CssClass="DetailsItem">Est. delivery:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel6" runat="server" BindTo="Declaration.JE_EstimatedDeliveryOrPickup"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                    </tr>
                                                    <tr id="DeliveryRequiredByRow">
                                                        <td>
                                                            <asp:Label ID="DeliveryRequiredLabel" runat="server" CssClass="DetailsItem">Delivery req. by:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel7" runat="server" BindTo="Declaration.JE_DeliveryOrPickupRequiredBy"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                    </tr>
                                                    <tr id="CartageAdvisedRow">
                                                        <td style="height: 19px">
                                                            <asp:Label ID="CartageAdvisedLabel" runat="server" CssClass="DetailsItem">Cartage advised:</asp:Label>&nbsp;</td>
                                                        <td style="height: 19px">
                                                            <edi:ZDateTimeLabel ID="ZDateTimeLabel8" runat="server" BindTo="Declaration.JP_Calc_CartageAdvised"
                                                                DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                    </tr>
                                                    <tr id="GoodsDeliveredRow">
                                                        <td>
                                                            <asp:Label ID="GoodsDeliveredLabel" runat="server" CssClass="DetailsItem">Goods delivered:</asp:Label>&nbsp;</td>
                                                        <td>
                                                            <edi:ZDateTimeLabel ID="ReceivedDate" runat="server" BindTo="Declaration.JE_CartageCompleted" DateTimeFormat="Long"></edi:ZDateTimeLabel></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr id="GoodsDescriptionRow">
                                <td colspan="3">
                                    <asp:Label ID="GoodsDescLabel" runat="server" CssClass="DetailsItem">Goods Description: </asp:Label><edi:ZTextLabel ID="GoodsDescription" runat="server" BindTo="Declaration.JE_GoodsDescription"></edi:ZTextLabel></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <br />
                <div id="StatusHolder" runat="server" visible="false">
                </div>
                <table class="ResultsTable" id="GridsLayoutTable" runat="server">
                    <tr>
                        <td colspan="2">
                            <table border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td align="left" valign="top">
                                        <edi_tracking:MilestonesControl ID="Milestones" runat="server" CssClass="DetailsTable" BindTo="Milestones" ItemCssClass="DetailsCell" HeaderCssClass="DetailsHeader" DisplayInPanel="True" PanelCssClass="SectionTitle"></edi_tracking:MilestonesControl>
                                    </td>
                                    <td align="left" valign="top">
                                        <edi_tracking:EventsControl ID="TrackingEvents" runat="server" CssClass="DetailsTable" BindTo="TrackingEvents" ItemCssClass="DetailsCell" HeaderCssClass="DetailsHeader" DisplayInPanel="True" PanelCssClass="SectionTitle"></edi_tracking:EventsControl>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZGrid ID="CustomsEntriesDataGrid" runat="server" CssClass="DetailsTable" BindTo="Declaration.CustomsEntryHeaders"
                                DisableCollapsing="True" Label="Customs entries">
                                <PagerStyle Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZCollapsablePanel ID="TransportPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Transport">
                                <div id="AreaTransportControl" runat="server">
                                    <table class="DetailsTable" id="TransportTable">
                                        <tbody>
                                            <tr class="DetailsHeader">
                                                <td align="center">Mode</td>
                                                <td align="center">Master Bill</td>
                                                <td align="center">Vessel</td>
                                                <td align="center">Voyage/Flight</td>
                                                <td align="center">Folio</td>
                                                <td align="center">Load</td>
                                                <td align="center">Discharge</td>
                                                <td align="center">Export Date</td>
                                                <td align="center">Arrival Date</td>
                                                <td align="center">First Port Of Arrival</td>
                                                <td align="center">First Port ATA</td>
                                            </tr>
                                            <tr class="DetailsCell">
                                                <td>
                                                    <edi:ZTextLabel ID="TransportMode" runat="server" BindTo="Declaration.JE_TransportMode"></edi:ZTextLabel></td>
                                                <td>
                                                    <edi:ZTextLabel ID="MasterBill" runat="server" BindTo="Declaration.JE_MasterBill"></edi:ZTextLabel></td>
                                                <td>
                                                    <edi:ZTextLabel ID="Vessel" runat="server" BindTo="CurrentVessel"></edi:ZTextLabel>
                                                </td>
                                                <td>
                                                    <edi:ZTextLabel ID="VoyageFlightNo" runat="server" BindTo="CurrentVoyageWithSuppression"></edi:ZTextLabel>
                                                </td>
                                                <td>
                                                    <edi:ZTextLabel ID="Folio" runat="server" BindTo="FolioWithSuppression"></edi:ZTextLabel>
                                                </td>
                                                <td>
                                                    <edi:ZCodeFindBoxLabel ID="PortOfLoading" runat="server" BindTo="Declaration.JE_RL_NKPortOfLoading"
                                                        BindToList="Declaration.Lookups.PortOfLoadings" DisplayStyle="CodeAndDescription"></edi:ZCodeFindBoxLabel>
                                                </td>
                                                <td>
                                                    <edi:ZCodeFindBoxLabel ID="PortOfArrival" runat="server" BindTo="Declaration.JE_RL_NKPortOfArrival"
                                                        BindToList="Declaration.Lookups.PortOfArrivals" DisplayStyle="CodeAndDescription"></edi:ZCodeFindBoxLabel>
                                                </td>
                                                <td>
                                                    <edi:ZDateTimeLabel ID="ExportDate" runat="server" BindTo="ExportDateWithSuppression"></edi:ZDateTimeLabel>
                                                </td>
                                                <td>
                                                    <edi:ZDateTimeLabel ID="DateOfArrival" runat="server" BindTo="DateOfArrivalWithSuppression"></edi:ZDateTimeLabel>
                                                </td>
                                                <td>
                                                    <edi:ZCodeFindBoxLabel ID="PortOfFirstArrival" runat="server" BindTo="Declaration.JE_RL_NKPortOfFirstArrival"
                                                        BindToList="Declaration.Lookups.PortOfFirstArrivals" DisplayStyle="CodeAndDescription"></edi:ZCodeFindBoxLabel>
                                                </td>
                                                <td>
                                                    <edi:ZDateTimeLabel ID="DateOfFirstArrival" runat="server" BindTo="DateOfFirstArrivalWithSuppression"></edi:ZDateTimeLabel>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                                <edi:ZGrid ID="TransportsGrid" runat="server" CssClass="DetailsTable" BindTo="TransportsIncludingRelated" DisableCollapsing="true">
                                    <PagerStyle Mode="NumericPages"></PagerStyle>
                                    <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                </edi:ZGrid>
                            </edi:ZCollapsablePanel>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <edi:ZGrid ID="HouseBillsGrid" runat="server" CssClass="DetailsTable" BindTo="Declaration.Bills"
                                DisableCollapsing="True" Label="Bills of Lading">
                                <PagerStyle Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <edi:ZGrid ID="OrdersGrid" runat="server" CssClass="DetailsTable" BindTo="Declaration.AttachedOrders"
                                DisableCollapsing="True" Label="Orders">
                                <PagerStyle Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                        <td>
                            <edi:ZGrid ID="ContainerGrid" runat="server" CssClass="DetailsTable" BindTo="Declaration.CusContainers"
                                DisableCollapsing="True" Label="Containers">
                                <PagerStyle Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZGrid ID="InvoicesGrid" runat="server" CssClass="DetailsTable" BindTo="Invoices"
                                DisableCollapsing="True" Label="Commercial Invoices">
                                <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZGrid ID="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="True">
                                <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZGrid ID="LocalChargesGrid" runat="server" CssClass="DetailsTable" BindTo="InvoiceLoader.LocalChargesDetails"
                                DisableCollapsing="True" Label="Local Charges" Visible="False">
                                <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                                <FooterStyle CssClass="DetailsFooter"></FooterStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZGrid ID="ChargesGrid" runat="server" CssClass="DetailsTable" BindTo="InvoiceLoader.Transactions"
                                DisableCollapsing="True" Label="Related Invoices">
                                <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
                                <ItemStyle CssClass="DetailsCell"></ItemStyle>
                                <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
                            </edi:ZGrid>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <edi:ZCollapsablePanel ID="Zcollapsablepanel5" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Notes">
                                <edi:ZNotesControl ID="notes" runat="server" BindTo="NotesHelper.VisibleNotes"></edi:ZNotesControl>
                            </edi:ZCollapsablePanel>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="NotFoundError" runat="server">
                <edi:ZTextLabel ID="DeclarationNotFoundLabel" runat="server"></edi:ZTextLabel>
            </div>
        </div>
    </form>
</body>
</html>
