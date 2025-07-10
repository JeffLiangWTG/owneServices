<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<%@ Page Language="c#" CodeBehind="EditLinerAndAgencyContainer.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.LinerAndAgency.EditLinerAndAgencyContainer" %>

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
                            <tr></tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="DynamicCaption" runat="server" cssclass="DetailsItem">Dynamic Number:</edi:ztextlabel>
                                </td>
                                <td>
                                    <edi:ztextlabel id="DynamicLabel" runat="server" bindto="DynamicNumber"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td>
                                    <edi:ztextlabel id="JobNumberCaption" runat="server" cssclass="DetailsItem">Job Number:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px;">
                                    <edi:ztextlabel id="JobNumberLabel" runat="server" bindto="ShipmentNumbers"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="ContainerStatusCaption" runat="server" cssclass="DetailsItem">Container Status:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px;">
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
                            <tr>
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
                            <tr>
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
                                    <edi:zcodefindboxlabel id="CommodityLabel" runat="server" bindto="JC_RH_NKContainerCommodityCode" bindtolist="ContainerCommodityCode_List"
                                        displaystyle="CodeAndDescription" />
                                </td>
                            </tr>
                            <tr>
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
                            <tr>
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
                            <tr>
                                <td style="width: 134px; height: 21px;">
                                    <edi:ztextlabel id="TareWeightCaption" runat="server" cssclass="DetailsItem">Tare Weight:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="TareWeightLabel" runat="server" bindto="TareWeightUQ"></edi:ztextlabel>
                                </td>
                                <td style="height: 21px"></td>
                                <td>
                                    <edi:zcheckbox id="IsShipperCheckBox" runat="server" autopostback="true" bindto="JC_IsShipperOwned" text="Is Shipper" />
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap;">
                                    <edi:zcheckbox id="ISEmptyCheckBox" runat="server" autopostback="true" bindto="JC_IsEmptyContainer" text="Is Empty" />
                                </td>
                                <td style="width: 322px"></td>
                                <td style="height: 21px"></td>
                                <td>
                                    <edi:zcheckbox id="IsDamagedCheckBox" runat="server" autopostback="true" bindto="JC_IsDamaged" text="Is Damaged" />
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <td style="width: 134px; height: 21px"></td>
                                <td style="width: 320px; height: 21px"></td>
                                <td style="height: 21px"></td>
                                <td style="white-space: nowrap;"></td>
                                <td style="width: 322px; height: 21px"></td>
                            </tr>
                            <tr>
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
                            </tr>
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
                            <tr>
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
                            <tr>
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
                            <tr>
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
                            <tr>
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
                            <tr>
                                <td style="vertical-align: top">
                                    <edi:ztextlabel id="GoodsDescriptionCaption" runat="server" cssclass="DetailsItem">Goods Description:</edi:ztextlabel>
                                </td>
                                <td colspan="4">
                                    <edi:ztextbox id="GoodsDescriptionTextbox" runat="server" bindto="UserEditableNoteHelper.EditableNoteText"
                                        rows="4" textmode="MultiLine" width="100%"></edi:ztextbox>
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
                            <tr>
                                <td style="width: 134px; height: 21px;">
                                    <edi:zcheckbox id="IsControlledAtmosphereCheckBox" runat="server" autopostback="true" bindto="JC_IsControlledAtmosphere" text="Temp Controlled" />
                                </td>
                                <td style="width: 320px; height: 21px"></td>
                                <td style="width: 134px; height: 21px;">
                                    <edi:zcheckbox id="IsChillerCheckBox" runat="server" autopostback="true" bindto="IsChiller" text="Chiller" />

                                </td>
                                <td style="width: 320px; height: 21px"></td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap;">
                                    <edi:zcheckbox id="IsFreezerCheckBox" runat="server" autopostback="true" bindto="IsFreezer" text="Frozen" />
                                </td>
                                <td style="width: 322px"></td>
                                <td style="height: 21px;">
                                    <edi:ztextlabel id="TemperatureCaption" runat="server" cssclass="DetailsItem">Temperature Set Point:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="TemperatureLabel" runat="server" bindto="JC_SetPointTemp"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
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
                            <tr>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="AirVentCaption" runat="server" cssclass="DetailsItem">Air Vent Setting:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px" colspan="3">
                                    <edi:ztextlabel id="AirVentLabel" runat="server" bindto="JC_AirVentFlow"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
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
                            <tr>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyRequiredCaption" runat="server" cssclass="DetailsItem">Empty Required By:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdateedit id="EmptyRequiredDateEditBox" runat="server" bindto="JC_EmptyRequired" datetimeformat="Long" />
                                </td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyPickupFromCaption" runat="server" cssclass="DetailsItem">Empty Pickup From:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                    <edi:ztextlabel id="EmptyPickupFromLabel" runat="server" bindto="JC_Calc_DepartureContainerYardAddressCode"></edi:ztextlabel>
                                </td>
                            </tr>
                            <tr>
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
                            <tr>
                                <td>
                                    <edi:ztextlabel id="EstimatedFullPickupCaption" runat="server" cssclass="DetailsItem">Estimated Full Pickup:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdateedit id="EstimatedFullPickupDateEditBox" runat="server" bindto="JC_DepartureEstimatedPickup" datetimeformat="Long" />
                                </td>
                                <td style="height: 20px">
                                    <edi:ztextlabel id="PickupFromCustomerCaption" runat="server" cssclass="DetailsItem">Pickup From Customer:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:zdateedit id="PickupFromCustomerDateEditBox" runat="server" bindto="JC_DepartureCartageComplete" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
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
                                   <edi:zdateedit id="SlotDaterDateEditBox" runat="server" bindto="SlotDate" datetimeformat="Long" />
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
                            <tr>
                                <td>
                                    <edi:ztextlabel id="EstimatedFullDeliveryCaption" runat="server" cssclass="DetailsItem">Estimated Full Delivery:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:zdateedit id="EstimatedFullDeliveryDateEditBox" runat="server" bindto="RequiredDelivery" datetimeformat="Long" />
                                </td>
                                <td style="width: 134px; height: 20px">
                                    <edi:ztextlabel id="DeliveryCaption" runat="server" cssclass="DetailsItem">Delivery:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 20px">
                                    <edi:zdateedit id="DeliveryDateEditBox" runat="server" bindto="ActualDelivery" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
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
                                    <edi:zdateedit id="EmptyReturnReqByDateEditBox" runat="server" bindto="EmptyReturnRequired" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReturnedOnCaption" runat="server" cssclass="DetailsItem">Empty Returned On:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 20px">
                                    <edi:zdateedit id="EmptyReturnedOnDateEditBox" runat="server" bindto="ActualDehire" datetimeformat="Long" />
                                </td>
                                <td style="white-space: nowrap;">
                                    <edi:ztextlabel id="EmptyReadyForReturnCaption" runat="server" cssclass="DetailsItem">Empty Ready for Return:</edi:ztextlabel>
                                </td>
                                <td style="width: 322px; height: 21px">
                                    <edi:zdateedit id="EmptyReadyForReturnDateEditBox" runat="server" bindto="EmptyReady" datetimeformat="Long" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <edi:ztextlabel id="WharfGateOutCaption" runat="server" cssclass="DetailsItem">Wharf Gate Out:</edi:ztextlabel>
                                </td>
                                <td style="width: 320px; height: 21px">
                                    <edi:ztextlabel id="WharfGateOutLabel" runat="server" bindto="JC_FCLWharfGateOut"></edi:ztextlabel>
                                </td>
                                <td style="width: 134px; height: 20px"></td>
                                <td style="width: 320px; height: 20px"></td>
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
                    <edi:zcollapsablepanel id="DocumentsPanel" runat="server" cssclass="SectionTitle" disablecollapsing="True" label="Documents">
                        <edi:ZDataGrid ID="DocumentsGrid" runat="server" AutoGenerateColumns="False" BindTo="DocumentHelper.Documents"
                            CssClass="DetailsTable">
                            <HeaderStyle CssClass="DetailsHeader" />
                            <PagerStyle Mode="NumericPages" PageButtonCount="20" />
                            <ItemStyle CssClass="DetailsCell" />
                        </edi:ZDataGrid>
                    </edi:zcollapsablepanel>
                    <div class="ContentSection">
                        <asp:Button ID="SaveContainer" runat="server" Text="Save" OnClick="SaveContainer_Click" Style="min-width: 60px;"></asp:Button>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
