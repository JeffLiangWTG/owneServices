<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditFwdInstruction.aspx.cs" Inherits="Enterprise.Tracking.Web.LinerAndAgency.EditFwdInstruction" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Edit Forwarding Instruction</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ztextlabel id="ShippingBillOfLadingLabel" runat="server" CssClass="PageTitle">Forwarding Instruction</edi:ztextlabel>
				<edi:ztextlabel id="BillOfLadingNumberLabel" runat="server" CssClass="PageTitle" BindTo="JS_HouseBill"></edi:ztextlabel>
			</div>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
			</div>
			<div id="AuthorisedContent" runat="server">
			    <div id="NotFoundError" runat="server">
				    <edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="PageTitle"></edi:ZTextLabel>
				</div>
				<div id="DataContent" runat="server">
					<br />
					<edi:ZTextLabel id="SailingLabel" runat="server" CssClass="SectionTitle">Sailing Details</edi:ZTextLabel>
					<div class="ContentSection">
						<table class="ResultsTable">
							<tr>
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
							<tr>
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
							<tr>
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
							<tr>
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
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="CarrierLabel" CssClass="DetailsItem">Carrier:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZFindBoxLabel runat="server" ID="Carrier" BindTo="BookedShippingLinePK" DisplayStyle="DescriptionOnly" />
								</td>
							</tr>
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="PrincipalLabel" CssClass="DetailsItem">Principal:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZFindBoxLabel runat="server" ID="Principal" BindTo="JS_OH_DeliveryAgent" DisplayStyle="DescriptionOnly" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr><td colspan="5"></td></tr>
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="OriginLabel" CssClass="DetailsItem">Origin:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZFindBox runat="server" ID="Origin" BindTo="JS_RL_NKOrigin" ModuleID="RefUNLOCOWeb" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ETDLabel" CssClass="DetailsItem">ETD:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateEdit runat="server"  ID="ETD" BindTo="JS_E_DEP" />
								</td>
							</tr>
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="DestinationLabel" CssClass="DetailsItem">Destination:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZFindBox runat="server" ID="Destination" BindTo="JS_RL_NKDestination" ModuleID="RefUNLOCOWeb" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ETALabel" CssClass="DetailsItem">ETA:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDateEdit runat="server" ID="ETA" BindTo="JS_E_ARV" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr>
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
									<edi:ZTextLabel runat="server" ID="BookingRef" BindTo="JS_BookingReference" />
								</td>
							</tr>
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ShipperRefLabel" CssClass="DetailsItem">Shipper's Ref#:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZTextBox runat="server" ID="ShipperRef" BindTo="JS_BookingReference" />
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
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="PaymentTermLabel" CssClass="DetailsItem">Payment Term:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDropDownList runat="server" ID="PaymentTerm" BindTo="JS_INCO" DisplayStyle="CodeAndDescription" ShowEmptyItem="true" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="ReleaseTypeLabel" CssClass="DetailsItem">Release Type:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZDropDownList runat="server" ID="ReleaseType" BindTo="JS_ReleaseType" DisplayStyle="CodeAndDescription" />
								</td>
							</tr>
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="OriginalBillsLabel" CssClass="DetailsItem">Original Bills:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZNumericTextBox runat="server" ID="OriginalBills" BindTo="JS_NoOriginalBills" Decimals="0" Width="30px" />
								</td>
								<td>&nbsp;</td>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="CopyBillsLabel" CssClass="DetailsItem">Copy Bills:</asp:Label>
								</td>
								<td style="white-space:nowrap">
									<edi:ZNumericTextBox runat="server" ID="CopyBills" BindTo="JS_NoCopyBills" Decimals="0" Width="30px" />
								</td>
							</tr>
							<tr><td colspan="5"></td></tr>
							<tr>
								<td style="white-space:nowrap">
									<asp:Label runat="server" ID="GoodsDescriptionLabel" CssClass="DetailsItem">Goods Description:</asp:Label>
								</td>
								<td colspan="4">
									<edi:ZTextBox runat="server" ID="GoodsDescription" BindTo="JS_GoodsDescription" width="100%" />
								</td>
							</tr>
							<tr>
								<td colspan="5">
									<edi:ZTextLabel ID="DetailedGoodsDescriptionLabel" runat="server" CssClass="SectionTitle">Detailed Goods Description:</edi:ZTextLabel>
									<br />
									<edi:ZTextBox id="DetailedGoodsDescription" runat="server" Width="100%" 
										Rows="3" TextMode="MultiLine"></edi:ZTextBox>
								</td>
							</tr>
								<tr>
								<td colspan="5">
									<edi:ZTextLabel ID="MArksAndNumbersLabel" runat="server" CssClass="SectionTitle">Marks and Numbers:</edi:ZTextLabel>
									<br />
									<edi:ZTextBox id="MarksAndNumbers" runat="server" Width="100%" 
										Rows="3" TextMode="MultiLine"></edi:ZTextBox>
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
						<div id="Addresses" runat="server">
							<div id="ConsignorAddressHolder" runat="server" style="float: left; margin-right: 15px;">
								<edi:ZDocAddressWebControl id="ConsignorAddress" runat="server" BindTo="ConsignorDocumentaryAddress" Caption="Consignor"
									SaveCheckBoxCaption="Save Consignor Address" IsConsignor="true" IsConsignee="false" ResidentialCheckboxVisible="false"
									SaveCheckboxVisible="true" />
							</div>
							<div id="ConsigneeAddressHolder" runat="server" style="float: left; margin-right: 15px;">
								<edi:ZDocAddressWebControl id="ConsigneeAddress" runat="server" BindTo="ConsigneeDocumentaryAddress" Caption="Consignee"
									SaveCheckBoxCaption="Save Consignee Address" IsConsignor="false" IsConsignee="true" ResidentialCheckboxVisible="false"
									SaveCheckboxVisible="true" />
							</div>
							<div id="NotifyPartyAddressHolder" runat="server">
								<edi:ZDocAddressWebControl id="NotifyPartyAddress" runat="server" BindTo="NotifyPartyDocumentaryAddress" Caption="Notify Party"
									IsConsignor="true" IsConsignee="true" ResidentialCheckboxVisible="false"
									SaveCheckboxVisible="false" />
							</div>
						</div>
						<div style="clear: both;"></div>
					</div>
					<div runat="server" id="ContainersPanel">
						<edi:ZTextLabel id="ContainersLabel" runat="server" CssClass="SectionTitle">Containers</edi:ZTextLabel>
						<div class="ContentSection">
							<edi:ZDataGrid id="ContainersGrid" runat="server" CssClass="DetailsTable" BindTo="FCLContainers"
								AllowAdd="True" AllowEdit="True" AllowDelete="True" AutoGenerateColumns="False"
								AllowPaging="False">
								<ItemStyle CssClass="DetailsCell"></ItemStyle>
								<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
							</edi:ZDataGrid>
						</div>
					</div>
					<edi:ZTextLabel id="PacksLabel" runat="server" CssClass="SectionTitle">Packs</edi:ZTextLabel>
					<div class="ContentSection">
						<edi:ZDataGrid id="PacksGrid" runat="server" CssClass="DetailsTable" BindTo="Cargo"
							AllowAdd="True" AllowEdit="True" AllowDelete="True" AutoGenerateColumns="False"
							AllowPaging="False">
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZDataGrid>
					</div>
					<edi:zcollapsablepanel id="MilestonesPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Milestones">
						<edi:zdatagrid id="MilestonesGrid" runat="server" CssClass="DetailsTable" AutoGenerateColumns="False">
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:zdatagrid>
					</edi:zcollapsablepanel>						
					<div id="DocumentsPanel" runat="server">
						<edi:ZTextLabel ID="DocumentsLabel" runat="server" CssClass="SectionTitle">Documents</edi:ZTextLabel>
						<div class="ContentSection">
							<edi:ZDataGrid id="DocumentsGrid" runat="server" CssClass="DetailsTable" 
								AutoGenerateColumns="False">
								<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
								<ItemStyle CssClass="DetailsCell"></ItemStyle>
								<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
							</edi:ZDataGrid>
						</div>
					</div>
					<div id="NotesPanel" runat="server">
						<edi:ZTextLabel ID="NotesLabel" runat="server" CssClass="SectionTitle">Notes</edi:ZTextLabel>
						<div class="ContentSection">
							<edi:ZNotesControl id="Notes" runat="server" BindTo="NotesHelper.VisibleNotes" />
						</div>
					</div>
					<div id="WebUserNotePanel" runat="server" class="ContentSection">
						<edi:ZTextLabel ID="WebUserNoteLabel" runat="server" CssClass="SectionTitle">To change other details on the Forwarding Instruction please provide them below:</edi:ZTextLabel>
						<br />
						<edi:ZTextBox id="WebUserNote" runat="server" Width="100%" 
							Rows="4" TextMode="MultiLine"></edi:ZTextBox>
					</div>
			    </div>
				<div class="ContentSection">
					<asp:button id="SaveButton" runat="server" Text="Save Forwarding Instruction" ToolTip="Save Forwarding Instruction" onclick="SaveButton_Click"></asp:button>&nbsp;
				</div>
			</div>
		</div>
    </form>
</body>
</html>
