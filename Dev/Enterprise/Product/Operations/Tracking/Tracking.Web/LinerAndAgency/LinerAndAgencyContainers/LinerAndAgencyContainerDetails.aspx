<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<%@ Page Language="c#" CodeBehind="LinerAndAgencyContainerDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.LinerAndAgency.LinerAndAgencyContainerDetails" %>
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
                <div id="ContainerContents" runat="server">
                    <div id="Title">
                        <edi:ztextlabel id="ContainerDetailsLabel" runat="server" cssclass="PageTitle">Container #</edi:ztextlabel>
                        <edi:ztextlabel id="Ztextlabel5" runat="server" cssclass="PageTitle" bindto="JC_ContainerNum"></edi:ztextlabel>
                    </div>
                    <div id="EditButtonDiv" runat="server" class="ContentSection">
                        <asp:Button ID="EditContainer" runat="server" Text="Edit Container" OnClick="EditContainer_Click"></asp:Button>
                        &nbsp;
                    </div>
                    <table class="ResultsTable">
                        <tbody>
                            <tr></tr>
                            <tr id="DynamicNumberRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="DynamicCaption" runat="server" cssclass="DetailsItem">Dynamic Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="DynamicLabel" runat="server" bindto="DynamicNumber" ></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="JobNumberCaption" runat="server" cssclass="DetailsItem">Job Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="JobNumberLabel" runat="server" bindto="ShipmentNumbers"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="ContainerStatusRow">
                                 <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ContainerStatusCaption" runat="server" cssclass="DetailsItem">Container Status:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="ContainerStatusLabel" runat="server" bindto="ContainerStatus"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                               <td style="width: 134px; height: 20px;">
                                    <edi:ztextlabel id="TypeCaption" runat="server" cssclass="DetailsItem">Type:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:ztextlabel id="TypeLabel" runat="server" bindto="TypeDescription"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="PackagesRow">
                               <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="PackagesCaption" runat="server" cssclass="DetailsItem">Packages:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px;">
                                    <edi:ztextlabel id="PackagesLabel" runat="server" bindto="Packs"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                               <td>
                                    <edi:ztextlabel id="ContainerModeCaption" runat="server" cssclass="DetailsItem">Container Mode:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="ContainerModeLabel" runat="server" bindto="Mode"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="DeliveryModeRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="DeliveyModeCaption" runat="server" cssclass="DetailsItem">Delivery Mode:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="DeliveryModeLabel" runat="server" bindto="JC_DeliveryMode"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="CommodityCaption" runat="server" cssclass="DetailsItem">Commodity:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ZCodeFindBoxLabel ID="CommodityLabel" runat="server" BindTo="JC_RH_NKContainerCommodityCode" BindToList="ContainerCommodityCode_List"
															DisplayStyle="CodeAndDescription" />
                                </td>
                            </tr>
                            <tr id="GoodsValueRow">
                                <td style="width: 134px; height: 20px;">
                                    <edi:ztextlabel id="GoodValueCaption" runat="server" cssclass="DetailsItem">Goods Value:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:ztextlabel id="GoodValueLabel" runat="server" bindto="JC_GoodsValue"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="CurrencyCaption" runat="server" cssclass="DetailsItem">Currency:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="CurrencyLabel" runat="server" bindto="JC_RX_NKGoodsCurrency"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="NetWeightRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="NetWeightCaption" runat="server" cssclass="DetailsItem">Net Weight:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="NetWeightLabel" runat="server" bindto="JC_Calc_NetWeight"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="GrossWeighCaption" runat="server" cssclass="DetailsItem">Gross Weight:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="GrossWeightLabel" runat="server" bindto="WeightWithUnits"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="TareWeightRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="TareWeightCaption" runat="server" cssclass="DetailsItem">Tare Weight:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="TareWeightLabel" runat="server" bindto="TareWeightUQ"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td>
                                    <edi:ZCheckBox ID="IsShipperCheckBox" runat="server" AutoPostBack="true" BindTo="JC_IsShipperOwned" Text="Is Shipper" />
                                </td>
                                 <td>
                                </td>
                            </tr>
                            <tr id="IsEmptyContainerRow">
                                <td style="white-space: nowrap;">
                                    <edi:ZCheckBox ID="ISEmptyCheckBox" runat="server" AutoPostBack="true" BindTo="JC_IsEmptyContainer" Text="Is Empty" />
                                </td>
                                <td style="width: 322px">
                                </td>
                                <td style="height: 21px"></td>
                                <td>
                                    <edi:ZCheckBox ID="IsDamagedCheckBox" runat="server" AutoPostBack="true" BindTo="JC_IsDamaged" Text="Is Damaged" />
                                </td>
                                <td>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px">
                                </td>
                                <td style="width: 320px; height: 21px">
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                </td>
                                <td style="width: 322px; height: 21px">
                                </td>
                            </tr>
                             <tr id="LoadDischargePortRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="LoadPortCaption" runat="server" cssclass="DetailsItem">Load Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="LoadPortLabel" runat="server" bindto="PortOfLoading"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="DischargePortCaption" runat="server" cssclass="DetailsItem">Discharge Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="DischargePortLabel" runat="server" bindto="PortOfDischarge"></edi:ztextlabel>
                                </td>
                            </tr id="OriginDestinationPortRow">
                              <tr>
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="OriginPortCaption" runat="server" cssclass="DetailsItem">Origin Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="OriginPortLabel" runat="server" bindto="PortOfOrigin"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="DestinationPortCaption" runat="server" cssclass="DetailsItem">Destination Port:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="DestinationPortLabel" runat="server" bindto="PortOfDestination"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="VesselVoyageRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="VesselCaption" runat="server" cssclass="DetailsItem">Vessel Name:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="VesselLabel" runat="server" bindto="VesselName"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="VoyageCaption" runat="server" cssclass="DetailsItem">Voyage No:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="VoyageLabel" runat="server" bindto="Voyage"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="PickupDeliverRow">
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
                                    <edi:ztextlabel id="DeliverLabel" runat="server" bindto="ConsigneesExtended"></edi:ztextlabel>
                                </td>
                            </tr>
                             <tr id="PaymentTermRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="PaymentTermCaption" runat="server" cssclass="DetailsItem">Payment Term:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="PaymentTermLabel" runat="server" bindto="PaymentTerm"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ServiceLevelLabel" runat="server" cssclass="DetailsItem">Service Level:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="ServiceLevelCaption" runat="server" bindto="ServiceLevel" />
                                </td>
                            </tr>
                            <tr id="ShippersRefRow">
                                <td style="width: 134px; height: 21px">
                                    <edi:ztextlabel id="ShipperRefCaption" runat="server" cssclass="DetailsItem">Shipper's Ref:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="ShipperRefLabel" runat="server" bindto="ShippersRef"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="OrderRefCaption" runat="server" cssclass="DetailsItem">Order refs:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="OrderRefLabel" runat="server" bindto="OrderRefs" />
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px"></td>
                                <td style="width: 320px; height: 21px"></td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;"></td>
                                <td style="width: 322px; height: 21px"></td>
                            </tr>
                            <tr id="GoodDescriptionRow">
                                <td style="vertical-align: top">
                                    <edi:ztextlabel id="GoodsDescriptionCaption" runat="server" cssclass="DetailsItem">Goods Description:</edi:ztextlabel>
                                </td>
                                <td colspan="4">
                                    <edi:ztextlabel id="GoodsDescriptionLabel" runat="server" bindto="GoodsDescription" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <br />
                    <table class="ResultsTable">
                        <tbody>
                            <tr>
                                <td colspan="4">
                                    <edi:ztextlabel id="RefrigrationCaption" runat="server" cssclass="DetailsItem">Refrigeration:</edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="4">
                                    <br />
                                </td>
                            </tr>
                            <tr id="TempControlledRow">
                                <td style="width: 134px; height: 21px;">
                                    <edi:ZCheckBox ID="IsControlledAtmosphereCheckBox" runat="server" AutoPostBack="true" BindTo="JC_IsControlledAtmosphere" Text="Temp Controlled" />
                                </td>
                                <td style="width: 320px; height: 21px">
                                </td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ZCheckBox ID="IsChillerCheckBox" runat="server" AutoPostBack="true" BindTo="IsChiller" Text="Chiller" />

                                </td>
                                <td style="width: 320px; height: 21px">
                                </td>
                            </tr>
                            <tr id="IsFreezerRow">
                                <td style="white-space: nowrap;">
                                    <edi:ZCheckBox ID="IsFreezerCheckBox" runat="server" AutoPostBack="true" BindTo="IsFreezer" Text="Frozen" />
                                </td>
                                <td style="width: 322px">
                                </td>
                                <td style="height: 21px;">
                                    <edi:ztextlabel id="TemperatureCaption" runat="server" cssclass="DetailsItem">Temperature Set Point:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="TemperatureLabel" runat="server" bindto="SetPointTempUQ"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="HumidityPercentRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="HumidityCaption" runat="server" cssclass="DetailsItem">Humidity Percent:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px">
                                    <edi:ztextlabel id="HumidityLabel" runat="server" bindto="JC_HumidityPercent"></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="TempRecordCaption" runat="server" cssclass="DetailsItem">Temp Rec. Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="TempRecordLabel" runat="server" bindto="JC_TempRecorderSerialNo"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="AirVentSettingRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="AirVentCaption" runat="server" cssclass="DetailsItem">Air Vent Setting:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px" colspan="3">
                                    <edi:ztextlabel id="AirVentLabel" runat="server" bindto="AirVentFlowUQ"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="ClipOnUnitNumberRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="ClipOnUnitNumberCaption" runat="server" cssclass="DetailsItem">Clip On Unit Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px" colspan="3">
                                    <edi:ztextlabel id="ClipOnUnitNumberLabel" runat="server" bindto="JC_RefrigGeneratorID"></edi:ztextlabel>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <table class="ResultsTable">
                        <tbody>
                            <tr>
                                <td colspan="4">
                                    <edi:ztextlabel id="ExportProcessCaption" runat="server" cssclass="DetailsItem">Export Process:</edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="4">
                                    <br />
                                </td>
                            </tr>
                            <tr id="EmptyRequiredByRow">
                                 <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyRequiredCaption" runat="server" cssclass="DetailsItem">Empty Required By:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdatetimelabel id="EmptyRequiredLabel" runat="server" bindto="JC_EmptyRequired" datetimeformat="Long" />
                                </td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyPickupFromCaption" runat="server" cssclass="DetailsItem">Empty Pickup From:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                    <edi:ztextlabel id="EmptyPickupFromLabel" runat="server" bindto="JC_Calc_DepartureContainerYardAddressCode"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="EmptyReleaseRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReleaseNumberCaption" runat="server" cssclass="DetailsItem">Empty Release Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                    <edi:ztextlabel id="EmptyReleaseNumberLabel" runat="server" bindto="JC_ReleaseNum"></edi:ztextlabel>
                                </td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReleaseFromCaption" runat="server" cssclass="DetailsItem">Empty Release From:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:ztextlabel id="EmptyReleaseFromLabel" runat="server" bindto="JC_ContainerYardEmptyPickupGateOut"></edi:ztextlabel>
                                </td>
                            </tr>
                              <tr id="EstimatedPickupRow">
                                <td>
                                    <edi:ztextlabel id="EstimatedFullPickupCaption" runat="server" cssclass="DetailsItem">Estimated Full Pickup:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdatetimelabel id="EstimatedFullPickupLabel" runat="server" bindto="JC_DepartureEstimatedPickup" datetimeformat="Long" />
                                </td>
                                <td style="height: 20px">
                                    <edi:ztextlabel id="PickupFromCustomerCaption" runat="server" cssclass="DetailsItem">Pickup From Customer:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:zdatetimelabel id="PickupFromCustomerLabel" runat="server" bindto="JC_DepartureCartageComplete" datetimeformat="Long" />
                                </td>
                            </tr>
                             <tr id="FCLWharfGateInRow">
                                <td>
                                    <edi:ztextlabel id="FCLWharfGateInCaption" runat="server" cssclass="DetailsItem">FCL Wharf Gate In:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                  <edi:ztextlabel id="FCLWharfGateInLabel" runat="server" bindto="JC_FCLWharfGateIn"></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 20px">
                                  <edi:ztextlabel id="SlotDateCaption" runat="server" cssclass="DetailsItem">Slot Date:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                   <edi:zdatetimelabel id="SlotDateLabel" runat="server" bindto="SlotDate" datetimeformat="Long" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <table class="ResultsTable">
                        <tbody>
                            <tr>
                                <td colspan="4">
                                    <edi:ztextlabel id="ImportProcessCaption" runat="server" cssclass="DetailsItem">Import Process:</edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="4">
                                    <br />
                                </td>
                            </tr>
                            <tr id="EstimatedDeliveryRow">
                                <td>
                                    <edi:ztextlabel id="EstimatedFullDeliveryCaption" runat="server" cssclass="DetailsItem">Estimated Full Delivery:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdatetimelabel id="EstimatedFullDeliveryLabel" runat="server" bindto="RequiredDelivery" datetimeformat="Long" />
                                </td>
                                <td style="width: 134px; height: 20px">
                                    <edi:ztextlabel id="DeliveryCaption" runat="server" cssclass="DetailsItem">Delivery:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:zdatetimelabel id="DeliveryLabel" runat="server" bindto="ActualDelivery" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr id="EmptyReturnedToRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReturnedToCaption" runat="server" cssclass="DetailsItem">Empty Returned To:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                   <edi:ztextlabel id="EmptyReturnedToLabel" runat="server" bindto="JC_Calc_ArrivalContainerYardAddressCode"></edi:ztextlabel>
                                </td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReturnReqByCaption" runat="server" cssclass="DetailsItem">Empty Return Req. By:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdatetimelabel id="EmptyReturnReqByLabel" runat="server" bindto="EmptyReturnRequired" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr id="EmptyReturnedOnRow">
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReturnedOnCaption" runat="server" cssclass="DetailsItem">Empty Returned On:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                    <edi:zdatetimelabel id="EmptyReturnedOnLabel" runat="server" bindto="ActualDehire" datetimeformat="Long" />
                                </td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReadyForReturnCaption" runat="server" cssclass="DetailsItem">Empty Ready for Return:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdatetimelabel id="EmptyReadyForReturnLabel" runat="server" bindto="EmptyReady" datetimeformat="Long" />
                                </td>
                            </tr>
                             <tr id="WharfGateOutRow">
                                <td>
                                    <edi:ztextlabel id="WharfGateOutCaption" runat="server" cssclass="DetailsItem">Wharf Gate Out:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="WharfGateOutLabel" runat="server" bindto="JC_FCLWharfGateOut"></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 20px">
                                </td>
                                <td style="width: 320px; height: 20px">
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <table class="ResultsTable" headercssclass="DetailsHeader">
                        <tbody>
                            <tr>
                                <td colspan="4">
                                    <edi:ztextlabel id="VGMSpecificationCaption" runat="server" cssclass="DetailsItem">VGM Specification:</edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="4">
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
                                    <edi:ztextlabel id="VerifiedMethodLabel" runat="server" bindto="VerifiedMethod"></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="VerifiedByCompanyCaption" runat="server" cssclass="DetailsItem">Verified By Company:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="VerifiedByCompanyLabel" runat="server" bindto="VerifiedByCompany"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr id="VerifiedByRow">
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

                    <edi:zgrid id="DocumentsGrid" runat="server" caption="Documents" cssclass="DetailsTable" bindto="DocumentHelper.Documents" disablecollapsing="True">
							<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:zgrid>
                </div>
            </div>
            <div id="NotFoundError" runat="server">
                    <edi:ztextlabel id="NotFoundLabel" runat="server" cssclass="PageTitle"></edi:ztextlabel>
            </div>
        </div>
    </form>
</body>
</html>
