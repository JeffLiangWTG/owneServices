<%@ Page language="c#" Codebehind="BookingDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Bookings.BookingDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<%@ Register TagPrefix="bookings" TagName="WebScheduleChooserControl" Src="WebScheduleChooserControl.ascx" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
		<title>Booking</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
				<edi:ztextlabel id="TitleLabel" runat="server" CssClass="PageTitle">Booking</edi:ztextlabel>&nbsp;
					<edi:ztextlabel id="Ztextlabel1" runat="server" CssClass="PageTitle" BindTo="UniqueConsignRef"></edi:ztextlabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" class="ContentSection">
					<div id="BookingCancelledDiv" runat="server">
						<edi:ztextlabel id="BookingCancelledLabel" runat="server" CssClass="SectionTitle" ForeColor="Red">This booking has been canceled</edi:ztextlabel>
					</div>
					<div id="EditButtonDiv" runat="server" class="ContentSection">
						<asp:button id="EditBooking" runat="server" Text="Edit Booking" onclick="EditBooking_Click"></asp:button>&nbsp;
						<asp:button id="CancelBooking" runat="server" Text="Cancel Booking" onclick="CancelBooking_Click"></asp:button>&nbsp; &nbsp;
						<asp:button id="DuplicateBooking" runat="server" Text="Copy Booking" onclick="DuplicateBooking_Click"></asp:button>&nbsp; &nbsp;
						<asp:button id="ReverseBooking" runat="server" Text="Reverse Booking" onclick="ReverseBooking_Click"></asp:button>&nbsp; &nbsp;
						<edi_tracking:ZDocumentsMenu  id="DocsMenu" runat="server"/>
					</div>
					<div id="EditControls" runat="server" class="ContentSection">
						<table class="ResultsTable">
							<tr id="ConsignorConsigneeRow" runat="server">
								<td style="white-space:nowrap">
									<asp:Label id="ConsignorLabel" runat="server" CssClass="DetailsItem">Consignor:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel id="ConsignorTextLabel" runat="server" BindTo="ConsignorPickupAddress.E2_CompanyName"></edi:ZTextLabel>
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label id="Label20" runat="server" CssClass="DetailsItem">Consignee:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel id="ConsigneeTextLabel" runat="server" BindTo="ConsigneeDeliveryAddress.E2_CompanyName"></edi:ZTextLabel>
								</td>
							</tr>
							<tr id="ConsignorConsigneeContactRow" runat="server">
								<td style="white-space:nowrap">
									<asp:label id="ConsignorContactLabel" runat="server" CssClass="DetailsItem">Consignor Contact:</asp:label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel id="ConsignorContact" runat="server" BindTo="ConsignorPickupAddress.E2_Contact"></edi:ZTextLabel>
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label id="Label19" runat="server" CssClass="DetailsItem">Consignee Contact:</asp:Label></td>
								<td colspan="2" style="white-space:nowrap">
									<edi:ZTextLabel id="ConsigneeContact" runat="server" BindTo="ConsigneeDeliveryAddress.E2_Contact"></edi:ZTextLabel>
								</td>
							</tr>
							<tr id="OriginDestinationRow" runat="server">
								<td style="white-space:nowrap"><asp:label id="Label1" runat="server" CssClass="DetailsItem">Origin:</asp:label></td>
								<td style="white-space:nowrap"><edi:zcodefindboxlabel id="OriginPort" runat="server" BindTo="Origin" DisplayStyle="DescriptionOnly"></edi:zcodefindboxlabel></td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap"><asp:label id="Label7" runat="server" CssClass="DetailsItem">Destination:</asp:label></td>
								<td style="white-space:nowrap"><edi:zcodefindboxlabel id="DestinationPort" runat="server" BindTo="Destination" DisplayStyle="DescriptionOnly"></edi:zcodefindboxlabel></td>
							</tr>
							<tr id="ETDETARow">
								<td style="white-space:nowrap"><asp:label id="Label4" runat="server" CssClass="DetailsItem">ETD:</asp:label></td>
								<td style="white-space:nowrap"><edi:zdatetimelabel id="ETD" runat="server" BindTo="ETDWithSuppression"></edi:zdatetimelabel></td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap"><asp:label id="Label18" runat="server" CssClass="DetailsItem">Dest. ETA:</asp:label></td>
								<td style="white-space:nowrap"><edi:zdatetimelabel id="FinalDestinationETA" runat="server" BindTo="ETAWithSuppression"></edi:zdatetimelabel></td>
							</tr>
							<tr id="TransportModeRow">
								<td style="white-space:nowrap"><asp:label id="Label3" runat="server" CssClass="DetailsItem">Transport Mode:</asp:label></td>
								<td style="white-space:nowrap"><edi:zcodelookuplabel id="TransportMode" runat="server" BindTo="Mode"
										DisplayStyle="DescriptionOnly" ></edi:zcodelookuplabel></td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap"><asp:label id="Label23" runat="server" CssClass="DetailsItem">Booking Date:</asp:label></td>
								<td style="white-space:nowrap"><edi:zdatetimelabel id="Zdatetimelabel1" runat="server" BindTo="A_BKD"></edi:zdatetimelabel></td>
							</tr>
							<tr id="ShippersRefRow">
								<td style="white-space:nowrap"><asp:label id="Label5" runat="server" CssClass="DetailsItem">Shipper's Ref#:</asp:label></td>
								<td colspan="4" style="white-space:nowrap"><edi:ztextlabel id="ShippersRefTextBox" runat="server" BindTo="BookingReference"></edi:ztextlabel></td>
							</tr>
							<tr id="OrderReferencesRow">
								<td style="white-space:nowrap">
										<asp:Label id="OrderReferencesLabel" runat="server" CssClass="DetailsItem">Order Ref#:</asp:Label>
								</td>
								<td colspan="4" style="white-space:nowrap">
									<edi:ztextlabel id="OrderReferencesTextBox" runat="server" BindTo="OrderItemsAsString" Width="100%"
										TextMode="MultiLine"></edi:ztextlabel>
								</td>
							</tr>
							<tr>
								<td style="white-space:nowrap" colspan="2">
									<edi:zgrid id="AttachedOrdersGrid" runat="server" Caption="Attached Orders" CssClass="DetailsTable" BindTo="AttachedOrders"
									AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" DisableCollapsing="true"
									AllowPaging="False">
										<PagerStyle Mode="NumericPages"></PagerStyle>
										<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
										<ItemStyle CssClass="DetailsCell"></ItemStyle>
										<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									</edi:zgrid>
								</td>
							</tr>
							<tr id="GoodDescriptionRow">
								<td style="white-space:nowrap"><asp:label id="Label6" runat="server" CssClass="DetailsItem">Goods Description:</asp:label></td>
								<td colspan="4" style="white-space:nowrap"><edi:ztextlabel id="DescriptionTextBox" runat="server" BindTo="GoodsDescription"></edi:ztextlabel></td>
							</tr>
						</table>
						<table class="ResultsTable" id="DetailsGoodsDescription" runat="server">
							<tr>
								<td style="white-space:nowrap"><asp:label id="Label2" runat="server" CssClass="DetailsItem">Detailed Goods Description:</asp:label></td>
								<td><edi:ztextlabel id="DetailedGoodsDescriptionTextBox" runat="server" BindTo="DetailedGoodsDescriptionNoteHelper.EditableNoteText"></edi:ztextlabel></td>
							</tr>
						</table>
						<div id="notFCL" runat="server">
							<table class="ResultsTable" id="ForAuthenticatedUserOnly" runat="server">
								<tr id="PacksRow">
									<td style="white-space:nowrap"><asp:label id="Label11" runat="server" CssClass="DetailsItem">Packs:</asp:label></td>
									<td style="white-space:nowrap"><edi:znumericlabel id="PacksEdit" runat="server" BindTo="OuterPacks"></edi:znumericlabel>&nbsp;<edi:ztextlabel id="WeightDropDown" runat="server" BindTo="OuterPacksPackType"></edi:ztextlabel></td>
								</tr>
								<tr id="WeightRow">
									<td style="white-space:nowrap"><asp:label id="Label10" runat="server" CssClass="DetailsItem">Weight:</asp:label></td>
									<td style="white-space:nowrap">
										<edi:ZTextLabel ID="PlannedWeightLabel" runat="server" BindTo="WeightWithUnits"></edi:ZTextLabel>
									</td>
								</tr>
								<tr id="VolumeRow">
									<td style="white-space:nowrap"><asp:label id="Label9" runat="server" CssClass="DetailsItem">Volume:</asp:label></td>
									<td style="white-space:nowrap">
										<edi:ZTextLabel ID="PlannedVolumeLabel" runat="server" BindTo="VolumeWithUnits"></edi:ZTextLabel>
									</td>
								</tr>
							</table>
						</div>
						<edi:zcollapsablepanel id="SchedulesPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True">
							<bookings:WebScheduleChooserControl id="ScheduleChooser" runat="server" BindTo="QuotedBooking" ReadOnly = "True"/>
						</edi:zcollapsablepanel>
					<table border="0" cellpadding="0" cellspacing="0">
					<tr>
						<td align="left" valign="top">
							<edi_tracking:MilestonesControl id="Milestones" runat="server" CssClass="DetailsTable" BindTo="Milestones" ItemCssClass="DetailsCell" HeaderCssClass="DetailsHeader" DisplayInPanel="True"  PanelCssClass="SectionTitle">
							</edi_tracking:MilestonesControl>
						</td>
						<td align="left" valign="top">
							<edi_tracking:EventsControl id="TrackingEvents" runat="server" CssClass="DetailsTable" BindTo="TrackingEvents" ItemCssClass="DetailsCell" HeaderCssClass="DetailsHeader" DisplayInPanel="True"  PanelCssClass="SectionTitle">
							</edi_tracking:EventsControl>
						</td>
					</tr>
					</table>
						<edi:zgrid id="ContainersDataGrid" runat="server" CssClass="DetailsTable" BindTo="Containers"
							AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Caption="Containers" DisableCollapsing="true"
							AllowPaging="False">
							<PagerStyle Mode="NumericPages"></PagerStyle>
							<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:zgrid>
						<edi:zgrid id="PackLinesGrid" runat="server" CssClass="DetailsTable" BindTo="OuterPackLines" Caption="Goods / Packs" DisableCollapsing="true"
							AutoGenerateColumns="False">
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:zgrid>
						<edi:zgrid id="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="true">
							<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:zgrid>
						<div id="Additional" class="ContentSection">
							<table class="ResultsTable" id="ForAuthenticatedUserOnly2" runat="server">
								<tr>
									<td colspan="4">&nbsp;</td>
								</tr>
								<tr id="GoodsValueRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label13" runat="server" CssClass="DetailsItem">Goods Value:</asp:Label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZNumericLabel id="GoodsValueBox" runat="server" BindTo="GoodsValue"></edi:ZNumericLabel>
									</td>
									<td>
										<edi:ZCodeFindBoxLabel id="Currency" runat="server" BindTo="GoodsValueCurr" DisplayStyle="CodeOnly"></edi:ZCodeFindBoxLabel>
									</td>
									<td>&nbsp;</td>
								</tr>
								<tr id="InsuranceValueRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="Label29" runat="server" CssClass="DetailsItem">Insurance Value:</asp:Label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZNumericLabel id="InsuranceValueBox" runat="server" BindTo="InsuranceValue"></edi:ZNumericLabel>
									</td>
									<td>
										<edi:ZCodeFindBoxLabel id="Currency1" runat="server" BindTo="InsuranceCurrency" DisplayStyle="CodeOnly"></edi:ZCodeFindBoxLabel>
									</td>
									<td>&nbsp;</td>
								</tr>
								<tr id="ShipperCODRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label27" runat="server" CssClass="DetailsItem">Shipper COD Amount:</asp:Label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZNumericLabel id="ShipperCODAmountBox" runat="server" BindTo="ShipperCODAmount"></edi:ZNumericLabel>&nbsp;
									</td>
									<td style="white-space:nowrap" colspan="2">
										<edi:ZCodeLookupLabel id="ShipperCODTypeLabel" runat="server" BindTo="ShipperCODPayMethod" 
										DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
									</td>
								</tr>
							</table>
							<table class="ResultsTable">
								<tr>
									<td colspan="4">&nbsp;</td> 
								</tr>
								<tr id="WarehouseRecRow" runat="server">
									<td style="height: 21px;white-space:nowrap">
										<asp:Label id="Label12" runat="server" CssClass="DetailsItem">Warehouse Rec.:</asp:Label>
									</td>
									<td style="height: 21px;white-space:nowrap">
										<edi:ZDateTimeLabel id="WarehouseRecDdateEdit" runat="server" BindTo="A_RCV"></edi:ZDateTimeLabel>
									</td>
									<td colspan="2">&nbsp;</td>
								</tr>
								<tr id="PickupAddressRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="Label14" runat="server" CssClass="DetailsItem">Pickup Address:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZTextLabel id="PickupAddressLabel" runat="server" BindTo="ConsignorPickupAddress.AddressAsASingleLine"></edi:ZTextLabel>
									</td>
								</tr>
								<tr id="PickupEstimateRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label22" runat="server" CssClass="DetailsItem">Estimated Pickup:</asp:Label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZDateTimeLabel id="PickupFromDateTimeLabel" runat="server" BindTo="EstimatedPickup" DateTimeFormat="Long"></edi:ZDateTimeLabel>
									</td>
									<td style="white-space:nowrap">
										<asp:Label id="Label26" runat="server" CssClass="DetailsItem">Pickup Required By:</asp:Label>
									</td>
									<td>
										<edi:ZDateTimeLabel id="PickupByDateTimeLabel" runat="server" BindTo="PickupRequiredBy" DateTimeFormat="Long"></edi:ZDateTimeLabel>
									</td>
								</tr>
								<tr id="PickupEquipmenRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label21" runat="server" CssClass="DetailsItem">Pickup Equipment:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZCodeLookupLabel id="PickupEquipmentNeededDropDownList" runat="server" BindTo="FCLPickupEquipmentNeeded"
											DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
									</td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr id="DeliveryAddressRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="Label15" runat="server" CssClass="DetailsItem">Delivery Address:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZTextLabel id="DeliveryAddressLabel" runat="server" BindTo="ConsigneeDeliveryAddress.AddressAsASingleLine"></edi:ZTextLabel>
									</td>
								</tr>
								<tr id="DeliveryEstimateRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label31" runat="server" CssClass="DetailsItem">Estimated Delivery:</asp:Label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZDateTimeLabel id="DeliveryOnDateTimeLabel" runat="server" BindTo="EstimatedDelivery" DateTimeFormat="Long"></edi:ZDateTimeLabel>
									</td>
									<td style="white-space:nowrap">
										<asp:Label id="Label32" runat="server" CssClass="DetailsItem">Delivery Required By:</asp:Label>&nbsp;
									</td>
									<td>
										<edi:ZDateTimeLabel id="DeliveryByDateTimeLabel" runat="server" BindTo="DeliveryRequiredBy" DateTimeFormat="Long"></edi:ZDateTimeLabel>
									</td>
								</tr>
								<tr id="CartageDropModeRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label33" runat="server" CssClass="DetailsItem">Cartage Drop Mode:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZCodeLookupLabel id="CartageDropModeCodeLookupLabel" runat="server" BindTo="FCLDeliveryEquipmentNeeded"
											DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
									</td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr id="ServiceLevelRow">
									<td style="white-space:nowrap">
										<asp:Label id="Label25" runat="server" CssClass="DetailsItem">Service Level:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZCodeFindBoxLabel id="ServiceLeveCodeLookupLabel" runat="server" BindTo="ServiceLevel"
											DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel>
									</td>
								</tr>
								<tr id="PaymentTermsRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="PayTermLabel" runat="server" CssClass="DetailsItem" Text="Payment Term:"></asp:Label>&nbsp;
									</td>
									<td style="white-space:nowrap">
										<edi:ZCodeLookupLabel id="PayTermCodeLookupLabel" runat="server" BindTo="INCO" 
										DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
									</td>
									<td style="white-space:nowrap">
										<asp:Label id="AdditionalTermsLabel" runat="server" CssClass="DetailsItem">Additional Terms:</asp:Label>&nbsp;
									</td>
									<td>
										<edi:ztextlabel id="AdditionalTermsText" runat="server" BindTo="AdditionalTerms" Width="100%"></edi:ztextlabel>
									</td>
								</tr>
								<tr id="BillingPartyRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="ThirdPartyDocAddressLabel" runat="server" CssClass="DetailsItem">Req. Billing Party:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZTextLabel id="ThirdPartyDocAddressTextLabel" runat="server" BindTo="ThirdPartyDocAddress.AddressAsASingleLine"></edi:ZTextLabel>
									</td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr id="ChargesApplyRow">
									<td style="white-space:nowrap">
										<asp:Label id="ChargesApplyLabel" runat="server" CssClass="DetailsItem">Charges Apply:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZCodeFindBoxLabel id="ChargesApplyCodeFindBoxLabel" runat="server" BindTo="ChargesApply"
											DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel>
									</td>
								</tr>
								<tr id="ReleaseTypeRow">
									<td style="white-space:nowrap">
										<asp:Label id="ReleaseTypeLabel" runat="server" CssClass="DetailsItem">Release Type:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZCodeFindBoxLabel id="ReleaseTypeCodeFindBoxLabel" runat="server" BindTo="ReleaseType"
											DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel>
									</td>
								</tr>
								<tr id="OnBoardRow">
									<td style="white-space:nowrap">
										<asp:Label id="OnBoardLabel" runat="server" CssClass="DetailsItem">On Board:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZCodeFindBoxLabel id="OnBoardCodeFindBoxLabel" runat="server" BindTo="OnBoard"
											DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel>
									</td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr id="PickupAgentRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="PickupAgentLabel" runat="server" CssClass="DetailsItem">Pickup Agent:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZTextLabel id="PickupAgent" runat="server" BindTo="PickupAgentFullName"></edi:ZTextLabel>
									</td>
								</tr>
								<tr id="DeliveryAgentRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="DeliveryAgentLabel" runat="server" CssClass="DetailsItem">Delivery Agent:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ZTextLabel id="DeliveryAgent" runat="server" BindTo="DeliveryAgentFullName"></edi:ZTextLabel>
									</td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr>
									<td colspan="4">
										<div class="ContentSection">
											<edi:ZGrid id="ReferenceDataGrid" runat="server" CssClass="DetailsTable" BindTo="AdditionalReferenceNumbers"
												AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Label="Reference Numbers" DisableCollapsing="true"
												AllowPaging="False">
												<PagerStyle Mode="NumericPages"></PagerStyle>
												<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
												<ItemStyle CssClass="DetailsCell"></ItemStyle>
												<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
											</edi:ZGrid>
										</div>
									</td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr id="CustomsEntryRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="Label16" runat="server" CssClass="DetailsItem">Customs#:</asp:Label>
									</td>
									<td style="white-space:nowrap" colspan="3">
										<edi:ztextlabel id="Ztextbox1" runat="server" BindTo="CustomsEntryNumber"></edi:ztextlabel>
									</td>
								</tr>
								<tr id="MarksAndNumbersRow" runat="server">
									<td style="white-space:nowrap">
										<asp:Label id="Label17" runat="server" CssClass="DetailsItem">Marks & Numbers:</asp:Label>
									</td>
									<td colspan="3" style="white-space:nowrap">
										<edi:ztextlabel id="Ztextbox2" runat="server" BindTo="MarksAndNumbers" Width="100%" TextMode="MultiLine"></edi:ztextlabel>
									</td>
								</tr>
								<tr id="SpecialInstructionsRow" runat="server">
									<td style="white-space:nowrap" ><asp:Label id="Label24" runat="server" CssClass="DetailsItem">Special instructions:</asp:Label></td>
									<td colspan="3">
										<edi:ztextlabel id="SpecialInstructions" runat="server" Width="100%" BindTo="UserEditableNoteHelper.EditableNoteText"
											TextMode="MultiLine"></edi:ztextlabel></td>
								</tr>
								<tr>
									<td colSpan="4">&nbsp;</td>
								</tr>
								<tr id="VesselRow">
									<td noWrap>
										<span class="DetailsItem">Vessel:</span>
									</td>
									<td colspan="3">
										<edi:ztextlabel id="VesselLabel" runat="server" BindTo="Vessel" 
											EnableHtmlEncoding="True"></edi:ztextlabel>
									</td>
								</tr>
								<tr id="VoyageFlightRow">
									<td noWrap>
										<span class="DetailsItem">Voyage/Flight:</span>
									</td>
									<td colspan="3">
										<edi:ztextlabel id="VoyageLabel" runat="server" BindTo="VoyageFlightWithSuppression"
											EnableHtmlEncoding="True"></edi:ztextlabel>
									</td>
								</tr>
							</table>
						</div>
					</div>
					<div id="EmailNotice" runat="server"></div>
				</div>
			</div>
		</form>
	</body>
</html>
