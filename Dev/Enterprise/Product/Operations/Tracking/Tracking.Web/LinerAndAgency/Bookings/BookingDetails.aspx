<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BookingDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.LinerAndAgency.BookingDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Liner & Agency Booking Details</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ztextlabel id="ShippingBookingsLabel" runat="server" CssClass="PageTitle">Liner & Agency Booking</edi:ztextlabel>
				<edi:ztextlabel id="BookingNumberLabel" runat="server" CssClass="PageTitle" BindTo="JS_UniqueConsignRef"></edi:ztextlabel>
			</div>
			<br />
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				<br />
			</div>
			<div id="AuthorisedContent" runat="server">
			    <div id="NotFoundError" runat="server">
				    <edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="PageTitle"></edi:ZTextLabel>
				</div>
				<div id="DataContent" runat="server">
					<div id="EditButtonDiv" runat="server" class="ContentSection">
						<asp:button id="EditBooking" runat="server" Text="Edit Booking"></asp:button>&nbsp;
						<asp:button id="CancelBooking" runat="server" Text="Cancel Booking" OnClientClick="javascript:return confirm('Are you sure you want to cancel this Booking?');" onclick="CancelReActivateBooking_Click"></asp:button>&nbsp;
						<asp:button id="DuplicateBooking" runat="server" Text="Copy Booking" onclick="DuplicateBooking_Click"></asp:button>&nbsp;			            
						<asp:button id="ReverseBooking" runat="server" Text="Reverse Booking" onclick="ReverseBooking_Click"></asp:button>&nbsp; &nbsp;
						<asp:button id="ConvertBooking" runat="server" Text="Convert to Forwarding Instruction"></asp:button>
					</div>
					<div id="CancelledBookingDiv" runat="server" class="ContentSection">
						<asp:Label id="BookingIsCancelled" runat="server" CssClass="ErrorMessage" Text="This Booking is Canceled"></asp:Label> 
						<asp:button id="ReActivateBooking" runat="server" Text="Re-Activate Booking" OnClientClick="javascript:return confirm('Are you sure you want to re-activate this Booking?');" onclick="CancelReActivateBooking_Click"></asp:button>
					</div>
					<div id="BookingDetailsDiv" runat="server" class="ContentSection">
						<table class="ResultsTable">
							<tr><td colspan="5"></td></tr>
							<tr id="BookingReferenceRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="BookingRefLabel" CssClass="DetailsItem">Booking Ref:</asp:Label>
								</td>
								<td style="white-space:nowrap">
								<edi:ZTextLabel runat="server" ID="BookingRef" BindTo="JS_CFSReference" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="BookingDateLabel" CssClass="DetailsItem">Booking Date:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="BookingDate" BindTo="JS_A_BKD" />
								</td>
							</tr>
							<tr id="ShippersReferenceRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ShipperRefLabel" CssClass="DetailsItem">Shipper's Ref#:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel runat="server" ID="ShipperRef" BindTo="JS_BookingReference" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="StatusLabel" CssClass="DetailsItem">Status:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeLookupLabel runat="server" ID="Status" BindTo="JS_ShipmentStatus" DisplayStyle="CodeAndDescription" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr runat="server" id="CargoTypeRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="CargoTypeLabel" CssClass="DetailsItem">Cargo Type:</asp:Label>
								</td>
								<td colspan="4" style="white-space:nowrap">
									<edi:ZCodeLookupLabel runat="server" ID="PackingMode" BindTo="JS_PackingMode" DisplayStyle="CodeAndDescription" />
								</td>
							</tr>
							<tr id="OriginRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="OriginLabel" CssClass="DetailsItem">Origin:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeFindBoxLabel runat="server" ID="Origin" BindTo="JS_RL_NKOrigin" DisplayStyle="DescriptionOnly" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ETDLabel" CssClass="DetailsItem">ETD:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="ETD" BindTo="JS_E_DEP" />
								</td>
							</tr>
							<tr id="DestinationRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="DestinationLabel" CssClass="DetailsItem">Destination:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeFindBoxLabel runat="server" ID="Destination" BindTo="JS_RL_NKDestination" DisplayStyle="DescriptionOnly" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ETALabel" CssClass="DetailsItem">ETA:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="ETA" BindTo="JS_E_ARV" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr runat="server" id="PaymentTermRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="PaymentTermLabel" CssClass="DetailsItem">Payment Term:</asp:Label>
								</td>
								<td colspan="4" style="white-space:nowrap">
									<edi:ZCodeLookupLabel runat="server" ID="PaymentTerm" BindTo="JS_INCO" DisplayStyle="CodeAndDescription" />
								</td>
							</tr>
							<tr runat="server" id="GoodsDescriptionRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="Label1" CssClass="DetailsItem">Goods Description:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZTextLabel runat="server" ID="GoodsDescription" BindTo="JS_GoodsDescription" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
						</table>
					</div>
					<edi:ZTextLabel id="SailingLabel" runat="server" CssClass="SectionTitle">Sailing Details</edi:ZTextLabel>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr id="VoyageVesselRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="VoyageNoLabel" CssClass="DetailsItem">Voyage No.:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel runat="server" ID="VoyageFlight" BindTo="Sailing.JX_JV_VoyageFlight" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="VesselLabel" CssClass="DetailsItem">Vessel Name:</asp:Label>
								</td>
								<td>
									<edi:ZTextLabel runat="server" ID="Vessel" BindTo="Sailing.JX_JV_NKVessel" />
								</td>
							</tr>
							<tr id="ReceivalDatesRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ReceivalStartDateLabel" CssClass="DetailsItem">Receival Start Date:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="ReceivalStartDate" BindTo="Sailing.JX_JA_CTOReceivalCommences" DateTimeFormat="Long" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ReceivalEndDateLabel" CssClass="DetailsItem">Receival End Date:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="ReceivalEndDate" BindTo="Sailing.JX_JA_CTOCutOff" DateTimeFormat="Long" />
								</td>
							</tr>
							<tr id="AvailabilityStorageDatesRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="AvailabilityDateLabel" CssClass="DetailsItem">Availability Date:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="AvailabilityDate" BindTo="Sailing.JX_DepotAvailabilityDate" DateTimeFormat="Long" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="StorageDateLabel" CssClass="DetailsItem">Storage Date:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="StorageDate" BindTo="Sailing.JX_DepotStorageDate" DateTimeFormat="Long" />
								</td>
							</tr>
							<tr id="LoadDischargePortsRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="LoadPortLabel" CssClass="DetailsItem">Load:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeFindBoxLabel runat="server" ID="LoadPort" BindTo="JS_NKLoadPort" DisplayStyle="DescriptionOnly" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="DischargePortLabel" CssClass="DetailsItem">Discharge:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeFindBoxLabel runat="server" ID="DischargePort" BindTo="JS_NKDischargePort" DisplayStyle="DescriptionOnly" />
								</td>
							</tr>
							<tr id="EstimatedDepartureArrivalDatesRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="SailingETDLabel" CssClass="DetailsItem">ETD:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="SailingETD" BindTo="Sailing.JX_JA_E_DEP" DateTimeFormat="Long" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="SailingETALabel" CssClass="DetailsItem">ETA:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateTimeLabel runat="server" ID="SailingETA" BindTo="Sailing.JX_JB_E_ARV" DateTimeFormat="Long" />
								</td>
							</tr>
							<tr id="CarrierRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="CarrierLabel" CssClass="DetailsItem">Carrier:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZFindBoxLabel runat="server" ID="Carrier" BindTo="BookedShippingLinePK" DisplayStyle="DescriptionOnly" />
								</td>
							</tr>
							<tr id="PrincipalRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="PrincipalLabel" CssClass="DetailsItem">Principal:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZFindBoxLabel runat="server" ID="Principal" BindTo="JS_OH_DeliveryAgent" DisplayStyle="DescriptionOnly" />
								</td>
							</tr>
						</table>
					</div>
					<edi:ZGrid id="BookedContainersGrid" runat="server" CssClass="DetailsTable" BindTo="FCLBookedContainers"
						AllowAdd="False" AllowEdit="False" AllowDelete="False" DisableCollapsing="True" Label="Containers"
						AllowPaging="False">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
					<edi:ZGrid id="PacksGrid" runat="server" CssClass="DetailsTable" BindTo="Cargo" DisableCollapsing="True" Label="Packs">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
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
					<edi:ZGrid id="DocumentsGrid" runat="server" CssClass="DetailsTable" 
						AutoGenerateColumns="False">
						<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
					<div id="NotesPanel" runat="server">
						<edi:ZTextLabel ID="NotesLabel" runat="server" CssClass="SectionTitle">Notes</edi:ZTextLabel>
						<div class="ContentSection">
							<edi:ZNotesControl id="Notes" runat="server" BindTo="NotesHelper.VisibleNotes" />
						</div>
					</div>
			    </div>
			</div>
		</div>
    </form>
</body>
</html>
