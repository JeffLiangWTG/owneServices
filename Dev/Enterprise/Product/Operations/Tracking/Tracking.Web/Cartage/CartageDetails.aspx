<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CartageDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.CartageDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Transport Job Details</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="PageForm" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title"><edi:ztextlabel id="TransportJobTitleLabel" runat="server" CssClass="PageTitle">Transport Job#</edi:ztextlabel><edi:ztextlabel id="Ztextlabel5" runat="server" CssClass="PageTitle" BindTo="JJ_ConsignmentID"></edi:ztextlabel></div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div id="NotFoundError" runat="server">
						<edi:ZTextLabel id="NotFoundLabel" runat="server"></edi:ZTextLabel>
					</div>
					<div id="CartageDetailsContent" runat="server" class="ContentSection">
						<table id="PageTable" class="ResultsTable">
							<tr>
								<td valign="top">
									<table class="ResultsTable">
										<tr id="JobTypeRow">
											<td></td>
											<td><asp:label id="LabelJobType" runat="server" CssClass="DetailsItem">Job Type:</asp:label>&nbsp;</td>
											<td><edi:ZCodeFindBoxLabel id="JJ_CartageTypeDropEdit" runat="server" class="DetailsItem" BindTo="JJ_JobType" 
												DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel></td>
										</tr>
										<tr id="MainJobPanel" runat="server">
											<td></td>
											<td><asp:label id="LabelLinkedToJob"  runat="server" CssClass="DetailsItem">Linked To Job:</asp:label>&nbsp;</td>
											<td><edi:ZHyperlink id="AddressesLinkedToJobLinkLabel" runat="server" CssClass="DetailsItem" BindTo=""></edi:ZHyperlink></td>
										</tr>
									</table>
								</td>
								<td valign="top">
									<table class="ResultsTable">
										<tr id="DropModeRow">
											<td></td>
											<td><asp:label id="LabelDropMode" CssClass="DetailsItem">Drop Mode:</asp:label>&nbsp;</td>
											<td><edi:ZCodeFindBoxLabel id="JJ_ContainerDropModeDropDownEdit" runat="server" CssClass="DetailsItem" BindTo="JJ_DropMode" 
												DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel></td>
										</tr>
										<tr id="ServiceLevelRow">
											<td></td>
											<td><asp:label id="LabelServiceLevel" CssClass="DetailsItem">Service Level:</asp:label>&nbsp;</td>
											<td><edi:ZCodeFindBoxLabel id="JJ_RS_NKServiceLevelFindBox" runat="server" class="DetailsItem" BindTo="JJ_RS_NKServiceLevel" 
													DisplayStyle="DescriptionOnly"></edi:ZCodeFindBoxLabel></td>
										</tr>
									</table>
								</td>
								<td valign="top">
									<table class="ResultsTable">
										<tr id="EstimatedPickupRow">
											<td></td>
											<td><asp:label id="LabelEstPickup" CssClass="DetailsItem">Est. Pickup:</asp:label>&nbsp;</td>
											<td><edi:zdatetimelabel id="JJ_EstimatedPickupDateEdit" runat="server" BindTo="JJ_EstimatedPickup"
													DateTimeFormat="Long"></edi:zdatetimelabel></td>
										</tr>
										<tr id="EstimatedDeliveryRow">
											<td></td>
											<td><asp:label id="LabelEstDelivery" CssClass="DetailsItem">Est. Delivery:</asp:label>&nbsp;</td>
											<td><edi:zdatetimelabel id="JJ_EstimatedDeliveryDateEdit" runat="server" BindTo="JJ_EstimatedDelivery"
													DateTimeFormat="Long"></edi:zdatetimelabel></td>
										</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="top">
									<table id="Details" class="ResultsTable">
									<tr>
										<td colspan="3" class="SectionTitle">Details</td>
									</tr>
									<tr id="WaybillNumberRow">
										<td></td>
										<td><asp:label id="LabelWaybillNo" CssClass="DetailsItem">Waybill #:</asp:label>&nbsp;</td>
										<td><edi:ztextlabel id="JJ_WaybillNumberTextBox" runat="server" BindTo="JJ_WaybillNumber"></edi:ztextlabel></td>
									</tr>
									<tr id="ReferenceNumberRow">
										<td></td>
										<td><asp:label id="LabelRefNo" CssClass="DetailsItem">Ref #:</asp:label>&nbsp;</td>
										<td><edi:ztextlabel id="JJ_OrderReferenceNumberTextBox" runat="server" BindTo="JJ_OrderReferenceNumber"></edi:ztextlabel></td>
									</tr>
									<tr id="GoodsDescriptionRow">
										<td></td>
										<td><asp:label id="LabelDescription" CssClass="DetailsItem">Description:</asp:label>&nbsp;</td>
										<td><edi:ztextlabel id="JJ_GoodsDescriptionTextBox" runat="server" BindTo="JJ_GoodsDescription"></edi:ztextlabel></td>
									</tr>
									<tr id="QuoteNumberRow">
										<td></td>
										<td><asp:label id="LabelQuoteNo" CssClass="DetailsItem">Quote #:</asp:label>&nbsp;</td>
										<td><edi:ztextlabel id="JJ_QuoteNumberTextBox" runat="server" BindTo="JJ_QuoteNumber"></edi:ztextlabel></td>
									</tr>
									<tr id="CompletedRow">
										<td></td>
										<td><asp:label id="LabelCompleted" CssClass="DetailsItem">Completed:</asp:label>&nbsp;</td>
										<td><edi:zdatetimelabel id="JJ_A_JCLDateEdit" runat="server" BindTo="JJ_A_JCL"
											DateTimeFormat="Long"></edi:zdatetimelabel></td>
									</tr>
									</table>
								</td>
								<td valign="top">
									<table id="Totals" class="ResultsTable">
										<tr>
											<td colspan="4" class="SectionTitle">Totals</td>
										</tr>
										<tr id="GoodsPacksRow">
											<td></td>
											<td><asp:label id="LabelTotalGoodsPacks" CssClass="DetailsItem">Goods Packs:</asp:label>&nbsp;</td>
											<td><edi:ZNumericLabel runat="server" id="JJ_OuterPacksCalcDropEdit" CssClass="DetailsItem" BindTo="JJ_OuterPacks" Decimals="0"></edi:ZNumericLabel></td>
											<td><edi:ztextlabel id="JJ_OuterPacksCalcDropEditUQ" runat ="server" CssClass="DetailsItem" BindTo="JJ_F3_NKPackType"></edi:ztextlabel></td>
										</tr>
										<tr id="GoodsWeightRow">
											<td></td>
											<td><asp:label id="LabelTotalGoodsWeight" CssClass="DetailsItem">Goods Weight:</asp:label>&nbsp;</td>
											<td><edi:ZNumericLabel id="JJ_WeightCalcDropEdit" runat="server" CssClass="DetailsItem" BindTo="JJ_Weight" Decimals="2"></edi:ZNumericLabel></td>
											<td><edi:ztextlabel id="JJ_WeightCalcDropEditUQ" runat="server" CssClass="DetailsItem" BindTo="JJ_WeightUQ"></edi:ztextlabel></td>
										</tr>
										<tr id="GoodsVolumeRow">
											<td></td>
											<td><asp:label id="LabelTotalGoodsVolume" CssClass="DetailsItem">Goods Volume:</asp:label>&nbsp;</td>
											<td><edi:ZNumericLabel id="JJ_VolumeCalcDropEdit" runat="server" CssClass="DetailsItem" BindTo="JJ_Volume" Decimals="2"></edi:ZNumericLabel></td>
											<td><edi:ztextlabel id="JJ_VolumeCalcDropEditUQ" runat="server" CssClass="DetailsItem" BindTo="JJ_VolumeUQ"></edi:ztextlabel></td>
										</tr>
										<tr id="AreaGrossWeight" runat="server">
											<td></td>
											<td><asp:label id="LabelTotalGrossWeight" CssClass="DetailsItem">Gross Weight:</asp:label>&nbsp;</td>
											<td><edi:ZNumericLabel id="GrossWeightCalcDropEdit" runat="server" CssClass="DetailsItem" BindTo="GrossWeight" Decimals="2"></edi:ZNumericLabel></td>
											<td><edi:ztextlabel id="GrossWeightCalcDropEditUQ" runat="server" CssClass="DetailsItem" BindTo="JJ_WeightUQ"></edi:ztextlabel></td>
										</tr>
									</table>
								</td>
								<td valign="top">
									
								</td>
							</tr>
							<tr>
								<td valign="top">
									<edi:ZDocAddressWebControl id="FirstDocAddressControl" runat="server" BindTo="FirstDocAddress"
										ResidentialCheckboxVisible=false SaveCheckboxVisible=false GovermentRegNoVisible=false AllowEdit=false/>
								</td>
								<td valign="top">
									<edi:ZDocAddressWebControl id="SecondDocAddressControl" runat="server" BindTo="SecondDocAddress"
										ResidentialCheckboxVisible=false SaveCheckboxVisible=false GovermentRegNoVisible=false AllowEdit=false/>
								</td>
								<td valign="top">
									<edi:ZDocAddressWebControl id="ThirdDocAddressControl" runat="server" BindTo="ThirdDocAddress"
										ResidentialCheckboxVisible=false SaveCheckboxVisible=false GovermentRegNoVisible=false AllowEdit=false/>
								</td>
							</tr>
							<tr>
								<td valign="top">
									<edi:ZDocAddressWebControl id="FourthDocAddressControl" runat="server" BindTo="FourthDocAddress"
										ResidentialCheckboxVisible=false SaveCheckboxVisible=false GovermentRegNoVisible=false AllowEdit=false/>
								</td>
								<td>
								</td>
								<td>
								</td>
							</tr>
						</table>
						<br />
						<div id="Grids">
						<edi:ZGrid id="LegsDataGrid" runat="server" CssClass="DetailsTable" BindTo="FilteredCartageLegs"
							AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" DisableCollapsing="True" Label="Legs"
							AllowPaging="False">
							<PagerStyle Mode="NumericPages"></PagerStyle>
							<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZGrid>
						<edi:ZGrid id="ContainersDataGrid" runat="server" CssClass="DetailsTable" BindTo="Containers"
							AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" DisableCollapsing="True" Label="Containers"
							AllowPaging="False">
							<PagerStyle Mode="NumericPages"></PagerStyle>
							<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZGrid>
						<edi:ZGrid id="LooseBookingGrid" runat="server" CssClass="DetailsTable" BindTo="LooseBookedMoves"
							AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" DisableCollapsing="True" Label="Loose Moves"
							AllowPaging="False">
							<PagerStyle Mode="NumericPages"></PagerStyle>
							<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
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
						<edi:ZGrid id="DocumentsGrid" runat="server" CssClass="DetailsTable" BindTo="DocumentHelper.Documents"
							DisableCollapsing="True" Label="Documents">
							<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZGrid>
						<edi:zcollapsablepanel id="ZcollapsablepanelNotes" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Notes">
							<edi:znotescontrol id="notes" runat="server" BindTo="Notes.ClientVisibleNotes"></edi:znotescontrol>
						</edi:zcollapsablepanel>
					</div>
					</div>
				</div>
		    </div>
		</form>
	</body>
</html>
