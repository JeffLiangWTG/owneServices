<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="bookings" TagName="WebScheduleChooserControl" Src="WebScheduleChooserControl.ascx" %>

<%@ Page Language="c#" CodeBehind="EditBooking.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Bookings.EditBooking" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Booking</title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
	<meta content="C#" name="CODE_LANGUAGE" />
	<meta content="JavaScript" name="vs_defaultClientScript" />
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<edi:ZTextLabel ID="TitleLabel" runat="server" CssClass="PageTitle">Booking</edi:ZTextLabel>&nbsp;
					<edi:ZTextLabel ID="Ztextlabel4" runat="server" CssClass="PageTitle" BindTo="UniqueConsignRef"></edi:ZTextLabel>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
			</div>
			<div id="NoBookingDiv" runat="server" visible="false">
                <edi:ZTextLabel ID="NoBookingLabel" runat="server" CssClass="ErrorMessage"></edi:ZTextLabel>
            </div>
			<div id="AuthorisedContent" runat="server">
				<asp:UpdatePanel ID="up" runat="Server">
					<ContentTemplate>
						<div id="EditControls" runat="server">
							<table class="ResultsTable">
								<tr>
									<td colspan="5">
										<edi:ZCheckBox ID="IsDomesticCheckBox" runat="server" AutoPostBack="true" BindTo="IsDomesticFreight" Text="Domestic Freight" />
									</td>
								</tr>
								<tr>
									<td colspan="2">
										<edi:ZDocAddressWebControl ID="ConsignorAddress" runat="server" BindTo="ConsignorPickupAddress" Caption="Pickup Address"
											SaveCheckboxCaption="Save Pickup Address" IsConsignor="true" IsConsignee="false" ResidentialCheckboxVisible="false"
											SaveCheckboxVisible="true" DependentPortControl="OriginPort_TextBox" />
										<br>
									</td>
									<td>&nbsp;</td>
									<td colspan="2">
										<edi:ZDocAddressWebControl ID="ConsigneeAddress" runat="server" BindTo="ConsigneeDeliveryAddress" Caption="Delivery Address"
											SaveCheckboxCaption="Save Delivery Address" IsConsignor="false" IsConsignee="true" ResidentialCheckboxVisible="false"
											SaveCheckboxVisible="true" DependentPortControl="DestinationPort_TextBox" />
										<br>
									</td>
								</tr>
								<tr>
									<td colspan="2">
										<div id="Origin" runat="server" class="ContentSection">
											<table class="ResultsTable">
												<tr>
													<td>Origin:</td>
													<td>
														<edi:ZFindBox ID="OriginPort" runat="server" AutoPostBack="true" BindTo="Origin" ModuleID="RefUNLOCOWeb"></edi:ZFindBox>
													</td>
												</tr>
											</table>
										</div>
									</td>
									<td>&nbsp;</td>
									<td colspan="2">
										<div id="Destination" runat="server" class="ContentSection">
											<table class="ResultsTable">
												<tr>
													<td>Destination:</td>
													<td>
														<edi:ZFindBox ID="DestinationPort" runat="server" AutoPostBack="true" BindTo="Destination" ModuleID="RefUNLOCOWeb"></edi:ZFindBox>
													</td>
												</tr>
											</table>
										</div>
									</td>
								</tr>
							</table>
							<div class="ContentSection">
								<table class="ResultsTable">
									<tr>
										<td>
											<table class="ResultsTable">
												<tr>
													<td nowrap>Mode:</td>
													<td nowrap>
														<edi:ZDropDownList ID="ContainerMode" runat="server" BindTo="Mode"
															DataTextField="Description" AutoPostBack="True" ShowEmptyItem="False"
															OnSelectedIndexChanged="WeightVolumeDropDown_SelectedIndexChanged">
														</edi:ZDropDownList></td>

												</tr>
												<tr>
													<td nowrap>Shipper's Ref#:</td>
													<td nowrap>
														<edi:ZTextBox ID="ShippersRefTextBox" runat="server" Width="100%" BindTo="BookingReference"></edi:ZTextBox></td>
												</tr>
												<tr>
													<td nowrap>
														<div runat="server" id="OrderReferencesLabelDiv">Order Ref#:</div>
													</td>
													<td nowrap>
														<edi:ZTextBox ID="OrderReferencesTextBox" runat="server" Width="100%" BindTo="OrderItemsAsString"></edi:ZTextBox></td>
												</tr>
												<tr>
													<td nowrap>Goods Description:</td>
													<td nowrap>
														<edi:ZTextBox ID="DescriptionTextBox" runat="server" BindTo="GoodsDescription" Width="100%"></edi:ZTextBox></td>
												</tr>
											</table>
										</td>
										<td>
											<div id="notFCL" runat="server">
												<table class="ResultsTable">
													<tr>
														<td nowrap>Packs:</td>
														<td nowrap>
															<edi:ZNumericTextBox ID="PacksEdit" runat="server" BindTo="OuterPacks" Width="88px"></edi:ZNumericTextBox>
															<edi:ZDropDownList ID="PackTypeDropDown" runat="server" BindTo="OuterPacksPackType"></edi:ZDropDownList></td>
													</tr>
													<tr>
														<td nowrap>Weight:</td>
														<td nowrap>
															<edi:ZNumericTextBox ID="ActualWeightEdit" runat="server" BindTo="ActualWeight" Width="88px" AutoPostBack="true"></edi:ZNumericTextBox>
															<edi:ZDropDownList ID="ActualWeightUQDropDown" runat="server" BindTo="UnitOfWeight" AutoPostBack="true" OnSelectedIndexChanged="WeightVolumeDropDown_SelectedIndexChanged"></edi:ZDropDownList></td>
													</tr>
													<tr>
														<td nowrap>Volume:</td>
														<td nowrap>
															<edi:ZNumericTextBox ID="ActualVolumeEdit" runat="server" BindTo="ActualVolume" Width="88px" AutoPostBack="true"></edi:ZNumericTextBox>
															<edi:ZDropDownList ID="ActualVolumeUQDropDown" runat="server" BindTo="UnitOfVolume" AutoPostBack="true" OnSelectedIndexChanged="WeightVolumeDropDown_SelectedIndexChanged"></edi:ZDropDownList></td>
													</tr>
												</table>
											</div>
										</td>
									</tr>
								</table>
								<table class="ResultsTable">
									<tr>
										<td>Detailed Goods Description:</td>
									</tr>
									<tr>
										<td>
											<edi:ZTextBox ID="DetailedGoodsDescriptionTextBox" runat="server" BindTo="DetailedGoodsDescriptionNoteHelper.EditableNoteText" Width="100%" TextMode="MultiLine" Rows="4" Columns="100"></edi:ZTextBox>
										</td>
									</tr>
									<tr>
										<td>&nbsp;</td>
									</tr>
								</table>
								<table class="ResultsTable">
									<tr>
										<td colspan="2">
											<asp:UpdatePanel ID="ShedulesUP" runat="Server" UpdateMode="Conditional">
												<Triggers>
													<asp:AsyncPostBackTrigger ControlID="ContainerMode" EventName="SelectedIndexChanged" />
												</Triggers>
												<ContentTemplate>
													<bookings:WebScheduleChooserControl id="ScheduleChooser" runat="server" BindTo="QuotedBooking" />
												</ContentTemplate>
											</asp:UpdatePanel>
										</td>
									</tr>
								</table>
							</div>
							<div id="FCL" runat="server" class="ContentSection">
								<div class="ContentSection">
									<edi:ZDataGrid ID="ContainersDataGrid" runat="server" Caption="Containers" CssClass="DetailsTable" BindTo="Containers"
										AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
										AllowPaging="False">
										<PagerStyle Mode="NumericPages"></PagerStyle>
										<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
										<ItemStyle CssClass="DetailsCell"></ItemStyle>
										<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									</edi:ZDataGrid>
								</div>
							</div>
							<div id="AttachedOrdersDiv" runat="server" class="ContentSection">
								<edi:ZDataGrid ID="AttachedOrdersGrid" runat="server" Caption="Attached Orders" CssClass="DetailsTable" BindTo="AttachedOrderLinks"
									AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
									AllowPaging="False">
									<PagerStyle Mode="NumericPages"></PagerStyle>
									<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
									<ItemStyle CssClass="DetailsCell"></ItemStyle>
									<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
								</edi:ZDataGrid>
							</div>
							<div id="PackLines" runat="server" class="ContentSection">
								<edi:ZDataGrid ID="PackLinesGrid" runat="server" Caption="Goods / Packs" CssClass="DetailsTable" BindTo="OuterPackLines"
									AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
									AllowPaging="False">
									<PagerStyle Mode="NumericPages"></PagerStyle>
									<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
									<ItemStyle CssClass="DetailsCell"></ItemStyle>
									<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
								</edi:ZDataGrid>
							</div>
							<div class="ContentSection">
								<edi:ZTextLabel ID="Ztextlabel5" runat="server" CssClass="SectionTitle">Goods Value</edi:ZTextLabel>
								<div class="ContentSection">
									<table class="ResultsTable">
										<tr>
											<td>Goods Value:</td>
											<td>
												<edi:ZNumericTextBox ID="GoodsValueBox" runat="server" Width="70px" BindTo="GoodsValue" AutoPostBack="true"></edi:ZNumericTextBox>
											</td>
											<td>
												<edi:ZFindBox ID="Currency" runat="server" Width="70px" BindTo="GoodsValueCurr" ModuleID="RefCurrencyWeb" />
											</td>
										</tr>
										<tr id="InsuranceValueRow" runat="server">
											<td>Insurance Value:</td>
											<td>
												<edi:ZNumericTextBox ID="InsuranceValueBox" runat="server" Width="70px" BindTo="InsuranceValue" AutoPostBack="true"></edi:ZNumericTextBox>
											</td>
											<td>
												<edi:ZFindBox ID="Currency1" runat="server" Width="70px" BindTo="InsuranceCurrency" ModuleID="RefCurrencyWeb" />
											</td>
										</tr>
										<tr>
											<td>Shipper COD Amount:</td>
											<td>
												<edi:ZNumericTextBox ID="ShipperCODAmountBox" runat="server" Width="70px" BindTo="ShipperCODAmount"></edi:ZNumericTextBox>
											</td>
											<td>
												<edi:ZDropDownList ID="ShipperCODTypeDropDownList" runat="server" BindTo="ShipperCODPayMethod" ShowEmptyItem="True"></edi:ZDropDownList>
											</td>
										</tr>
									</table>
								</div>
							</div>

							<div class="ContentSection">
								<edi:ZTextLabel ID="Ztextlabel2" runat="server" CssClass="SectionTitle">Additional Information</edi:ZTextLabel>
								<div class="ContentSection">
									<table class="ResultsTable">
										<tr id="WarehouseRecRow" runat="server">
											<td nowrap>Warehouse Rec.:</td>
											<td nowrap>
												<edi:ZDateEdit ID="WarehouseRecDateEdit" runat="server" BindTo="A_RCV"></edi:ZDateEdit>
											</td>
										</tr>
										<tr>
											<td nowrap>Estimated Pickup:</td>
											<td nowrap>
												<edi:ZDateEdit ID="PickupFromDateEdit" runat="server" BindTo="EstimatedPickup" DateTimeFormat="Long" />
											</td>
											<td nowrap>Pickup Required By:</td>
											<td nowrap>
												<edi:ZDateEdit ID="PickupByDateEdit" runat="server" BindTo="PickupRequiredBy" DateTimeFormat="Long" />
											</td>
										</tr>
										<tr>
											<td nowrap>Pickup Equipment:</td>
											<td nowrap>
												<edi:ZDropDownList ID="PickupEquipmentNeededDropDownList"
													runat="server" DataValueField="Code" DataTextField="Description"
													BindTo="FCLPickupEquipmentNeeded" ShowEmptyItem="True">
												</edi:ZDropDownList>
											</td>
										</tr>
										<tr>
											<td colspan="4">&nbsp;</td>
										</tr>
										<tr>
											<td nowrap>Estimated Delivery:</td>
											<td nowrap>
												<edi:ZDateEdit ID="DeliveryOnDateEdit" runat="server" BindTo="EstimatedDelivery" DateTimeFormat="Long" />
											</td>
											<td nowrap>Delivery Required By:</td>
											<td nowrap>
												<edi:ZDateEdit ID="DeliveryByDateEdit" runat="server" BindTo="DeliveryRequiredBy" DateTimeFormat="Long" />
											</td>
										</tr>
										<tr>
											<td nowrap>Cartage Drop Mode:</td>
											<td nowrap>
												<edi:ZDropDownList ID="CartageDropModeDropDownList" runat="server"
													DataValueField="Code" DataTextField="Description" BindTo="FCLDeliveryEquipmentNeeded"
													ShowEmptyItem="True" Enabled="False">
												</edi:ZDropDownList>
											</td>
										</tr>
										<tr>
											<td colspan="4">&nbsp;</td>
										</tr>
										<tr>
											<td nowrap>Service Level:</td>
											<td nowrap>
												<edi:ZDropDownList ID="ServiceLevelDropDownList" runat="server"
													BindTo="ServiceLevel" DataValueField="RS_Code" DataTextField="RS_DescriptionMultilingual"
													ShowEmptyItem="True">
												</edi:ZDropDownList>
											</td>
											<td>
												<asp:Label ID="PayTermLabel" runat="server" Text="Payment Term:"></asp:Label></td>
											<td>
												<edi:ZDropDownList ID="PayTermDropDownList" runat="server" BindTo="INCO" ShowEmptyItem="True" AutoPostBack="true"></edi:ZDropDownList>
											</td>
										</tr>
										<tr id="AdditionalTermsRow" runat="server">
											<td nowrap colspan="2">&nbsp;</td>
											<td>Additional Terms:</td>
											<td>
												<edi:ZTextBox ID="AdditionalTerms" runat="server" BindTo="AdditionalTerms" Width="100%"></edi:ZTextBox>
											</td>
										</tr>
										<tr>
											<td colspan="4">&nbsp;</td>
										</tr>
										<tr>
											<td nowrap>Charges Apply:</td>
											<td colspan="3" nowrap>
												<edi:ZDropDownList ID="ChargesApplyDropDownList" runat="server"
													BindTo="ChargesApply" DataValueField="Code" DataTextField="Description"
													ShowEmptyItem="True">
												</edi:ZDropDownList>
											</td>
										</tr>
										<tr>
											<td nowrap>Release Type:</td>
											<td colspan="3" nowrap>
												<edi:ZDropDownList ID="ReleaseTypeDropDownList" runat="server"
													BindTo="ReleaseType" DataValueField="Code" DataTextField="Description"
													ShowEmptyItem="True">
												</edi:ZDropDownList>
											</td>
										</tr>
										<tr>
											<td nowrap>On Board:</td>
											<td colspan="3" nowrap>
												<edi:ZDropDownList ID="OnBoardDropDownList" runat="server"
													BindTo="OnBoard" DataValueField="Code" DataTextField="Description"
													ShowEmptyItem="True">
												</edi:ZDropDownList>
											</td>
										</tr>
										<tr>
											<td colspan="4">&nbsp;</td>
										</tr>
										<tr>
											<td nowrap colspan="4">
												<div id="Reference" runat="server" class="ContentSection">
													<edi:ZDataGrid ID="ReferenceGrid" runat="server" Caption="Reference Numbers" CssClass="DetailsTable" BindTo="AdditionalReferenceNumbers"
														AllowAdd="True" AllowEdit="True" AllowDelete="True" PageSize="3" AutoGenerateColumns="False"
														AllowPaging="False" InitialRowsToDisplay="1">
														<PagerStyle Mode="NumericPages"></PagerStyle>
														<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
														<ItemStyle CssClass="DetailsCell"></ItemStyle>
														<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
													</edi:ZDataGrid>
												</div>
											</td>
										</tr>
									</table>
									<edi:ZDiv ID="PanelThirdParyAddress" runat="server" HideButtonAfterClick="true">
										<edi:ZOrgAddressControl ID="ThirdPartyAddress" BindTo="ThirdPartyAddressPK" runat="server" OrganisationCaption="Req. Billing Party" ModuleID="OrgReceivablesTracking" AddressModuleID="OrgAddressReceivablesTracking" />
									</edi:ZDiv>
									<table class="ResultsTable">
										<tr>
											<td colspan="4">&nbsp;</td>
										</tr>
										<tr id="CustomsEntryRow" runat="server">
											<td nowrap>Customs#:</td>
											<td colspan="4">
												<edi:ZTextBox ID="CustomsEntryNumber" runat="server" BindTo="CustomsEntryNumber" Width="100%"></edi:ZTextBox>
											</td>
										</tr>
										<tr>
											<td nowrap>Marks &amp; Numbers:</td>
											<td nowrap colspan="4">
												<edi:ZTextBox ID="Ztextbox2" runat="server" Width="600px" BindTo="MarksAndNumbers" TextMode="MultiLine"></edi:ZTextBox>
											</td>
										</tr>
										<tr>
											<td nowrap colspan="5">Special instructions:</td>
										</tr>
										<tr>
											<td nowrap colspan="5">
												<edi:ZTextBox ID="SpecialInstructions" runat="server" Width="100%"
													Rows="4" BindTo="UserEditableNoteHelper.EditableNoteText" TextMode="MultiLine"></edi:ZTextBox>
											</td>
										</tr>
									</table>
								</div>
							</div>
						</div>
						<div class="ContentSection">
							<asp:Button ID="MakeBooking" runat="server" Text="Save Booking" ToolTip="Save booking into the system" OnClick="MakeBooking_Click"></asp:Button>&nbsp;
						</div>
					</ContentTemplate>
				</asp:UpdatePanel>
			</div>
		</div>
	</form>
</body>
</html>
