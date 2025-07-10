<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BillOfLadingDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.LinerAndAgency.BillOfLadingDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Liner & Agency Bill of Lading Details</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ztextlabel id="ShippingBillOfLadingLabel" runat="server" CssClass="PageTitle">Liner & Agency Bill of Lading</edi:ztextlabel>
				<edi:ztextlabel id="BillOfLadingNumberLabel" runat="server" CssClass="PageTitle" BindTo="JS_HouseBill"></edi:ztextlabel>
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
						<asp:button id="EditButton" runat="server" Text="Edit Forwarding Instruction"></asp:button>&nbsp;
						<asp:button id="DuplicateBillOfLading" runat="server" Text="Copy Bill of Lading" onclick="DuplicateBillOfLading_Click"></asp:button>&nbsp;
						<asp:button id="ReverseBillOfLading" runat="server" Text="Reverse Bill of Lading" onclick="ReverseBillOfLading_Click"></asp:button>&nbsp; &nbsp;
						<edi_tracking:ZDocumentsMenu  id="DocsMenu" runat="server"/>
						<br /><br />
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
							<tr id="AvailabilityStorageDateRow">
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
							<tr id="LoadDischargePortRow">
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
							<tr id="EstimatedDepartureArrivalRow">
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
							<tr><td colspan="5"></td></tr>
							<tr><td colspan="5"></td></tr>
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
							<tr id="ShipmentNumberRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="OceanBillLabel" CssClass="DetailsItem">Shipment No.:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel runat="server" ID="OceanBill" BindTo="JS_UniqueConsignRef" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="BookingRefLabel" CssClass="DetailsItem">Booking Ref:</asp:Label>
								</td>
								<td style="white-space:nowrap">
								<edi:ZTextLabel runat="server" ID="BookingRef" BindTo="JS_CFSReference" />
								</td>
							</tr>
							<tr runat="server" id="ShipperCargoRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ShipperRefLabel" CssClass="DetailsItem">Shipper Ref:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextLabel runat="server" ID="ShipperRef" BindTo="JS_BookingReference" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="CargoTypeLabel" CssClass="DetailsItem">Cargo Type:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeLookupLabel runat="server" ID="PackingMode" BindTo="JS_PackingMode" DisplayStyle="CodeAndDescription" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr runat="server" id="PaymentReleaseRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="PaymentTermLabel" CssClass="DetailsItem">Payment Term:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeLookupLabel runat="server" ID="PaymentTerm" BindTo="JS_INCO" DisplayStyle="CodeAndDescription" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ReleaseTypeLabel" CssClass="DetailsItem">Release Type:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZCodeLookupLabel runat="server" ID="ReleaseType" BindTo="JS_ReleaseType" DisplayStyle="CodeAndDescription" />
								</td>
							</tr>
							<tr runat="server" id="BillsRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="OriginalBillsLabel" CssClass="DetailsItem">Original Bills:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZNumericLabel runat="server" ID="OriginalBills" BindTo="JS_NoOriginalBills" Decimals="0" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="CopyBillsLabel" CssClass="DetailsItem">Copy Bills:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZNumericLabel runat="server" ID="CopyBills" BindTo="JS_NoCopyBills" Decimals="0" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr runat="server" id="GoodsDescriptionRow">
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="GoodsDescriptionLabel" CssClass="DetailsItem">Goods Description:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZTextLabel runat="server" ID="GoodsDescription" BindTo="JS_GoodsDescription" />
								</td>
							</tr>
							<tr runat="server" id="CustomsStatusRow">
								<td>
									<asp:Label runat="server" ID="CustomsStatusLabel"  CssClass="DetailsItem">Customs Status:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZTextLabel runat="server" ID="CustomsStatus" />
								</td>
							</tr>
							<tr runat="server" id="MessageStatusRow">
								<td>
									<asp:Label runat="server" ID="MessageStatusLabel"  CssClass="DetailsItem">Message Status:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZTextLabel runat="server" ID="MessageStatus" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
						</table>
						<table id="Addresses" runat="server" class="ResultsTable">
							<tr>
								<td><asp:Label ID="ConsignorLabel" runat="server" CssClass="DetailsItem">Consignor</asp:Label></td>
								<td>&nbsp;</td>
								<td><asp:Label ID="ConsigneeLabel" runat="server" CssClass="DetailsItem">Consignee</asp:Label></td>
								<td>&nbsp;</td>
								<td><asp:Label ID="NotifyPartyLabel" runat="server" CssClass="DetailsItem">Notify Party</asp:Label></td>
							</tr>
							<tr>
								<td><edi:ZTextLabel ID="ConsignorCompanyName" runat="server" BindTo="ConsignorDocumentaryAddress+E2_CompanyName" /></td>
								<td>&nbsp;</td>
								<td><edi:ZTextLabel ID="ConsigneeCompanyName" runat="server" BindTo="ConsigneeDocumentaryAddress+E2_CompanyName" /></td>
								<td>&nbsp;</td>
								<td><edi:ZTextLabel ID="NotifyPartyCompanyName" runat="server" BindTo="NotifyPartyDocumentaryAddress+E2_CompanyName" /></td>
							</tr>
							<tr>
								<td><edi:ZTextLabel ID="ConsignorAddress1" runat="server" BindTo="ConsignorDocumentaryAddress+E2_Address1" /></td>
								<td>&nbsp;</td>
								<td><edi:ZTextLabel ID="ConsigneeAddress1" runat="server" BindTo="ConsigneeDocumentaryAddress+E2_Address1" /></td>
								<td>&nbsp;</td>
								<td><edi:ZTextLabel ID="NotifyPartyAddress1" runat="server" BindTo="NotifyPartyDocumentaryAddress+E2_Address1" /></td>
							</tr>
							<tr>
								<td><edi:ZTextLabel ID="ConsignorAddress2" runat="server" BindTo="ConsignorDocumentaryAddress+E2_Address2" /></td>
								<td>&nbsp;</td>
								<td><edi:ZTextLabel ID="ConsigneeAddress2" runat="server" BindTo="ConsigneeDocumentaryAddress+E2_Address2" /></td>
								<td>&nbsp;</td>
								<td><edi:ZTextLabel ID="NotifyPartyAddress2" runat="server" BindTo="NotifyPartyDocumentaryAddress+E2_Address2" /></td>
							</tr>
							<tr>
								<td>
									<edi:ZTextLabel ID="ConsignorCity" runat="server" BindTo="ConsignorDocumentaryAddress+E2_City" />
									<edi:ZTextLabel ID="ConsignorState" runat="server" BindTo="ConsignorDocumentaryAddress+E2_State" />
								</td>
								<td>&nbsp;</td>
								<td>
									<edi:ZTextLabel ID="ConsigneeCity" runat="server" BindTo="ConsigneeDocumentaryAddress+E2_City" />
									<edi:ZTextLabel ID="ConsigneeState" runat="server" BindTo="ConsigneeDocumentaryAddress+E2_State" />
								</td>
								<td>&nbsp;</td>
								<td>
									<edi:ZTextLabel ID="NotifyPartyCity" runat="server" BindTo="NotifyPartyDocumentaryAddress+E2_City" />
									<edi:ZTextLabel ID="NotifyPartyState" runat="server" BindTo="NotifyPartyDocumentaryAddress+E2_State" />
								</td>
							</tr>
							<tr>
								<td>
									<edi:ZTextLabel ID="ConsignorPostCode" runat="server" BindTo="ConsignorDocumentaryAddress+E2_Postcode" />
									<edi:ZTextLabel ID="ConsignorCountry" runat="server" BindTo="ConsignorDocumentaryAddress+Country+RN_Desc" />
								</td>
								<td>&nbsp;</td>
								<td>
									<edi:ZTextLabel ID="ConsigneePostCode" runat="server" BindTo="ConsigneeDocumentaryAddress+E2_Postcode" />
									<edi:ZTextLabel ID="ConsigneeCountry" runat="server" BindTo="ConsigneeDocumentaryAddress+Country+RN_Desc" />
								</td>
								<td>&nbsp;</td>
								<td>
									<edi:ZTextLabel ID="NotifyPartyPostCode" runat="server" BindTo="NotifyPartyDocumentaryAddress+E2_Postcode" />
									<edi:ZTextLabel ID="NotifyPartyCountry" runat="server" BindTo="NotifyPartyDocumentaryAddress+Country+RN_Desc" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
						</table>
					</div>
					<edi:ZGrid id="ContainersGrid" runat="server" CssClass="DetailsTable" BindTo="FCLContainers" Caption="Containers" DisableCollapsing="true"
						AllowAdd="False" AllowEdit="False" AllowDelete="False" AutoGenerateColumns="False"
						AllowPaging="False">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
					<edi:ZGrid id="PacksGrid" runat="server" CssClass="DetailsTable" BindTo="Cargo" Caption="Packs" DisableCollapsing="true"
						AutoGenerateColumns="False">
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
					<div id="DocumentsPanel" runat="server">
						<edi:ZTextLabel ID="DocumentsLabel" runat="server" CssClass="SectionTitle">Documents</edi:ZTextLabel>
						<asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="DocumentsUpdatePanel">			            
							<ContentTemplate>
								<div class="ContentSection">
									<edi:ZDataGrid id="DocumentsGrid" runat="server" CssClass="DetailsTable" 
										AutoGenerateColumns="False">
										<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
										<ItemStyle CssClass="DetailsCell"></ItemStyle>
										<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									</edi:ZDataGrid>
									<edi_tracking:eDocAttachPopup id="eDocsAddNewLink" runat="server" AutoPostBack="true"></edi_tracking:eDocAttachPopup>
								</div>
							</ContentTemplate>
						</asp:UpdatePanel>
					</div>
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
