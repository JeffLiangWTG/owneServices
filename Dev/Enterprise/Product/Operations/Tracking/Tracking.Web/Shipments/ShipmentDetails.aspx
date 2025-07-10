<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Page language="c#" Codebehind="ShipmentDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.ShipmentDetails" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Shipment Details</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
						<edi:ztextlabel id="ShipmentDetailsLabel" runat="server" CssClass="PageTitle">Shipment #</edi:ztextlabel>
						<edi:ztextlabel id="Ztextlabel5" runat="server" CssClass="PageTitle" BindTo="JS_UniqueConsignRef"></edi:ztextlabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div id="EditButtonDiv" runat="server" class="ContentSection">
						<asp:button id="DuplicateShipment" runat="server" Text="Copy Shipment" onclick="DuplicateShipment_Click"></asp:button>&nbsp; &nbsp;			            
						<asp:button id="ReverseShipment" runat="server" Text="Reverse Shipment" onclick="ReverseShipment_Click"></asp:button>&nbsp; &nbsp;			            			            
						</div>				
					<div class="ContentSection">
						<div>
							<edi_tracking:ZDocumentsMenu  id="DocsMenu" runat="server"/>
						</div>
						<table class="ResultsTable">
							<tbody>
								<tr>
									<td valign="top">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="HouseBillRow">
												<td>
													<asp:label id="BillLabel" runat="server" CssClass="DetailsItem">Bill: </asp:label>
												</td>
												<td>
													<edi:ztextlabel id="HouseBill" runat="server" BindTo="JS_HouseBill"></edi:ztextlabel>
												</td>
											</tr>
											<tr id="ShippersRefRow">
												<td>
													<asp:label id="ClientShipperRefLabel" runat="server" CssClass="DetailsItem">Shipper's Ref#: </asp:label>
												</td>
												<td>
													<edi:ztextlabel id="ClientShipperRefData" runat="server" BindTo="JS_BookingReference"></edi:ztextlabel>
												</td>
										   </tr>
                                            <tr id="OwnersRefRow">
												<td>
													<asp:label id="ClientOwnerRefLabel" runat="server" CssClass="DetailsItem">Owner's Ref#: </asp:label>
												</td>
												<td>
													<edi:ztextlabel id="ClientOwnerRefData" runat="server" BindTo="OwnerReference"></edi:ztextlabel>
												</td>
										   </tr>
										 </table>
									</td>
									<td valign="top" style="padding-left:50px;">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="OriginRow" runat="server">
												<td>
													<asp:label id="OriginLabel" runat="server" CssClass="DetailsItem">Origin: </asp:label>
												</td>
												<td>
													<edi:zcodefindboxlabel id="Origin" runat="server" BindTo="JS_RL_NKOrigin" DisplayStyle="CodeAndDescription"
														BindToList="Lookups.RefUNLOCO_List"></edi:zcodefindboxlabel>
												</td>
											</tr>
											<tr id="ETDRow" runat="server">
												<td>
													<asp:label id="ETDLabel" runat="server" CssClass="DetailsItem">ETD: </asp:label>
												</td>
												<td>
													<edi:zdatetimelabel id="ETD" runat="server" BindTo="ETDWithSuppression" DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>											
										</table>								    
									</td>
									<td valign="top" style="padding-left:50px;">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="DestinationRow" runat="server">
												<td>
													<asp:label id="DestinationLabel" runat="server" CssClass="DetailsItem">Destination: </asp:label>
												</td>
												<td>
													<edi:zcodefindboxlabel id="Destination" runat="server" BindTo="JS_RL_NKDestination" DisplayStyle="CodeAndDescription"
														BindToList="Lookups.RefUNLOCO_List"></edi:zcodefindboxlabel>
												</td>
											</tr>
											<tr id="ETARow" runat="server">
												<td>
													<asp:label id="ETALabel" runat="server" CssClass="DetailsItem">ETA: </asp:label>
												</td>
												<td>
													<edi:zdatetimelabel id="ETA" runat="server" BindTo="ETAWithSuppression"></edi:zdatetimelabel>
												</td>
											</tr>											
										</table>								    
									</td>
								 </tr>
								 <tr>
									<td valign="top" colspan="3">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="OrderReferencesRow" runat="server">
												<td>
													<asp:label id="Label1" runat="server" CssClass="DetailsItem">Order Ref#: </asp:label>
												</td>
												<td style="word-wrap:break-word;">
													<edi:ztextlabel id="Ztextlabel1" runat="server" style="word-wrap:break-word;" BindTo="OrderReference"></edi:ztextlabel>
												</td>
											</tr>
										</table>
									</td>	
								 </tr>   							    
								 <tr>
									<td valign="top">
										<div id="ForAuthentifiedUserOnly" runat="server">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="ShipperRow">
												<td>
													<asp:label id="ShipperLabel" runat="server" CssClass="DetailsItem">Shipper: </asp:label>
												</td>
												<td>
													<edi:ZTextLabel id="Shipper" runat="server" BindTo="ConsignorDocumentaryAddress.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="ConsigneeRow">
												<td>
													<asp:label id="ConsigneeLabel" runat="server" CssClass="DetailsItem">Consignee: </asp:label>
												</td>
												<td>
													<edi:ZTextLabel id="Consignee" runat="server" BindTo="ConsigneeDocumentaryAddress.E2_CompanyName"></edi:ZTextLabel>
												</td>
										   </tr>
										   <tr>
												<td colspan="2">
													&nbsp;
												</td>
										   </tr>
											<tr id="SizeRow">
												<td>
													<asp:label id="SizeLabel" runat="server" CssClass="DetailsItem">Size: </asp:label>
												</td>
												<td>
													<edi:ZTextLabel ID="PlannedVolumeLabel" runat="server" BindTo="VolumeWithUnits"></edi:ZTextLabel>
												</td>
										   </tr>
											<tr id="WeightRow">
												<td>
													<asp:label id="WeightLabel" runat="server" CssClass="DetailsItem">Weight: </asp:label>
												</td>
												<td>
													<edi:ZTextLabel ID="PlannedWeightLabel" runat="server" BindTo="WeightWithUnits"></edi:ZTextLabel>
												</td>
										   </tr>
											<tr id="LoadingMetersRow" runat="server">
												<td>
													<asp:label id="LoadingMetersLabel" runat="server" CssClass="DetailsItem">Loading Meters: </asp:label>
												</td>
												<td>
													<edi:znumericlabel id="LoadingMeters" runat="server" BindTo="JS_LoadingMeters"></edi:znumericlabel>
												</td>
											</tr>
											<tr id="QuantityRow">
												<td>
													<asp:label id="QuantityLabel" runat="server" CssClass="DetailsItem">Quantity: </asp:label>
												</td>
												<td>
													<edi:znumericlabel id="OuterPackCount" runat="server" BindTo="JS_OuterPacks"></edi:znumericlabel><edi:ztextlabel id="PackType" runat="server" BindTo="JS_F3_NKPackType"></edi:ztextlabel>
												</td>
										   </tr>										   										   										   
										</table>							            
										</div>
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="ServiceLevelRow">
												<td>
													<asp:label id="ServiceLevelLabel" runat="server" CssClass="DetailsItem">Service Level: </asp:label>
												</td>
												<td>                                                    
													<edi:zcodefindboxlabel id="ServiceLevelDescription" runat="server" BindTo="JS_RS_NKServiceLevel" DisplayStyle="CodeAndDescription"></edi:zcodefindboxlabel>
												</td>
											</tr>
											<tr id="GoodsDescriptionRow">
												<td>
													<asp:label id="GoodsDescLabel" runat="server" CssClass="DetailsItem">Goods Description: </asp:label>
												</td>
												<td>                                                    
													<edi:ztextlabel id="GoodsDescription" runat="server" BindTo="JS_GoodsDescription" DESIGNTIMEDRAGDROP="160"></edi:ztextlabel>
												</td>
											</tr>
											<tr id="PayTermsRow" runat="server">
												<td style="white-space:nowrap">
													<asp:Label id="PayTermLabel" runat="server" CssClass="DetailsItem" Text="Payment Term:"></asp:Label>&nbsp;
												</td>
												<td style="white-space:nowrap">
													<edi:ZCodeLookupLabel id="PayTermCodeLookupLabel" runat="server" BindTo="JS_INCO" 
													DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
												</td>
											</tr>
											<tr id="AdditionalTermsRow" runat="server" >
												<td style="white-space:nowrap">
													<asp:Label runat="server" CssClass="DetailsItem">Additional Terms:</asp:Label>&nbsp;
												</td>
												<td>
													<edi:ztextlabel id="AdditionalTermsText" runat="server" BindTo="JS_AdditionalTerms" Width="100%"></edi:ztextlabel>
												</td>
											</tr>
											 <tr id="ReleaseTypeRow" runat="server" >
												<td style="white-space:nowrap">
													<asp:Label ID="Label13" runat="server" CssClass="DetailsItem">Release Type:</asp:Label>&nbsp;
												</td>
												<td>                                                    
													<edi:ZCodeLookupLabel id="ReleaseTypeCodeLookupLabel" runat="server" BindTo="JS_ReleaseType" DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
												</td>
											</tr>
											<tr id="OnBoardRow" runat="server" >
												<td style="white-space:nowrap">
													<asp:Label ID="Label14" runat="server" CssClass="DetailsItem">On Board:</asp:Label>&nbsp;
												</td>
												<td>
													<edi:ZCodeLookupLabel id="OnBoardCodeLookupLabel" runat="server" BindTo="JS_ShippedOnBoard" DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
												</td>
											</tr>
											<tr id="ChargesApplyRow" runat="server" >
												<td style="white-space:nowrap">
													<asp:Label ID="Label15" runat="server" CssClass="DetailsItem">Charges Apply:</asp:Label>&nbsp;
												</td>
												<td>
													<edi:ZCodeLookupLabel id="ChargesApplyCodeLookupLabel" runat="server" BindTo="JS_HBLAWBChargesDisplay" DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel>
												</td>
											</tr>
											<tr>
												<td colspan="2">
													&nbsp;
												</td>
											</tr>
										</table>
										<div id="ForAuthentifiedUserOnly4" runat="server">
											<table class="ResultsTable" cellSpacing="0" cellPadding="0">
												<tr id="PickupAgentRow">
													<td>
														<asp:label id="Label17" runat="server" CssClass="DetailsItem">Pickup Agent: </asp:label>
													</td>
													<td>
														<edi:ZTextLabel id="PickupAgent" runat="server" BindTo="PickupAgentFullName"></edi:ZTextLabel>
													</td>
												</tr>
												<tr id="DeliveryAgentRow">
													<td>
														<asp:label id="Label18" runat="server" CssClass="DetailsItem">Delivery Agent: </asp:label>
													</td>
													<td>
														<edi:ZTextLabel id="DeliveryAgent" runat="server" BindTo="DeliveryAgentFullName"></edi:ZTextLabel>
													</td>
												</tr>
											</table>
										</div>
									</td>
									<td valign="top" style="padding-left:50px;">
										<div id="ForAuthentifiedUserOnly2" runat="server">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="PickupFromRow">
												<td>
													<asp:label id="Label2" runat="server" CssClass="DetailsItem">Pickup From: </asp:label>
												</td>
												<td>                                                    
													<edi:ztextlabel id="Ztextlabel3" runat="server" BindTo="ConsignorPickupAddress.AddressAsASingleLine"></edi:ztextlabel>
												</td>
											</tr>
											<tr>
												<td colspan="2">
													&nbsp;
												</td>
											</tr>
											<tr>
												<td colspan="2">
													&nbsp;
												</td>
											</tr>                                        	
										</table>							        
										</div>
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr>
												<td colspan="2">
													&nbsp;
												</td>
											</tr>
											<tr id="StorageCommencesParallelRow" runat="server">
												<td colspan="2">
													&nbsp;
												</td>
											</tr>                                        									        
											<tr id="EstimatedPickupRow">
												<td>
													<asp:label id="Label3" runat="server" CssClass="DetailsItem">Estimated Pickup: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel1" runat="server" BindTo="DocsAndCartage.JP_EstimatedPickup" DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>                                        									        
											<tr id="PickupRequiredByRow">
												<td>
													<asp:label id="Label4" runat="server" CssClass="DetailsItem">Pickup Required By: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel2" runat="server" BindTo="DocsAndCartage.JP_PickupRequiredBy"
														DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>										        
											</tr>
											<tr id="PickupCartageAdvisedRow" runat="server">
												<td>
													<asp:label id="Label16" runat="server" CssClass="DetailsItem">Pickup Cartage Advised: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel9" runat="server" BindTo="DocsAndCartage.JP_PickupCartageAdvised"
														DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>                                 									        
											<tr id="GoodsPickedUpRow">
												<td>
													<asp:label id="Label5" runat="server" CssClass="DetailsItem">Goods Picked Up: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel3" runat="server" BindTo="DocsAndCartage.JP_PickupCartageCompleted"
														DateTimeFormat="Long"></edi:zdatetimelabel>	
												</td>												
											</tr>
										</table>                                        
									</td>
									<td valign="top" style="padding-left:50px;">
										<div id="ForAuthentifiedUserOnly3" runat="server">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="DeliverToRow">
												<td>
													<asp:label id="Label6" runat="server" CssClass="DetailsItem">Deliver To: </asp:label>
												</td>
												<td>                                                    
													<edi:ztextlabel id="Ztextlabel4" runat="server" BindTo="ConsigneeDeliveryAddress.AddressAsASingleLine"></edi:ztextlabel>
												</td>
											</tr>
											<tr id="AvailableAtRow">
												<td>
													<asp:Label ID="Label12" runat="server" CssClass="DetailsItem">Available At: </asp:Label>
												</td>
												<td>                                                    
													<edi:ZTextLabel ID="AvailableAtAddressAsTextLabel" runat="server" BindTo="AvailableAtAddressAsText"></edi:ZTextLabel>
												</td>
											</tr>    
											<tr>
												<td colspan="2">
													&nbsp;
												</td>
											</tr>                                        	                                    	
										</table>							        
										</div>
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr id="AvailabilityRow">
												<td>
													<asp:label id="Label7" runat="server" CssClass="DetailsItem">Availability: </asp:label>  
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel4" runat="server" BindTo="AvailableDate" DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>
											<tr id="StorageCommencesRow" runat="server">
												<td>
													<asp:label id="Label8" runat="server" CssClass="DetailsItem">Storage Commences: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel5" runat="server" BindTo="StorageDate" DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>
											<tr id="EstimatedDeliveryRow">
												<td>
													<asp:label id="Label9" runat="server" CssClass="DetailsItem">Estimated Delivery: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel6" runat="server" BindTo="DocsAndCartage.JP_EstimatedDelivery"
														DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>
											<tr id="DeliveryRequiredByRow">
												<td>
													<asp:label id="Label10" runat="server" CssClass="DetailsItem">Delivery Required By: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="Zdatetimelabel7" runat="server" BindTo="DocsAndCartage.JP_DeliveryRequiredBy"
														DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>
											<tr id="CartageAdvisedRow" runat="server">
												<td>
													<asp:label id="Label11" runat="server" CssClass="DetailsItem">Delivery Cartage Advised: </asp:label>
												</td>
												<td>
													<edi:zdatetimelabel id="Zdatetimelabel8" runat="server" BindTo="DocsAndCartage.JP_DeliveryCartageAdvised"
														DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>
											<tr id="GoodsDeliveredRow">
												<td>
													<asp:label id="DeliveredLabel" runat="server" CssClass="DetailsItem">Goods Delivered: </asp:label>
												</td>
												<td>                                                    
													<edi:zdatetimelabel id="ReceivedDate" runat="server" BindTo="DocsAndCartage.JP_DeliveryCartageCompleted"
														DateTimeFormat="Long"></edi:zdatetimelabel>
												</td>
											</tr>                                        	                                        	                                        	                                        	                                        	
										</table>                                        
									</td>
								</tr>
							</tbody>
						</table>
					</div>
					<div id="MasterShipmentArea" runat="server">
						<table class="ResultsTable">
							<tr>
								<td>
									<asp:label id="LabelMasterLead" runat="server" CssClass="DetailsItem">Master/Lead: </asp:label>
								</td>
								<td><edi:ZHyperlink ID="MasterLink" runat="server" BindTo="MasterShipmentNum"></edi:ZHyperlink></td>
							</tr>
						</table>
					</div>
					<edi:zgrid id="RelatedShipments" runat="server" CssClass="DetailsTable" BindTo="RelatedShipments" Caption="Related Shipments">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
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
					<edi:zgrid id="LocalChargesGrid" runat="server" CssClass="DetailsTable" BindTo="Invoiceloader.LocalChargesDetails" Caption="Local Charges" DisableCollapsing="true" Visible="false">
						<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						<FooterStyle CssClass="DetailsFooter"></FooterStyle>
					</edi:zgrid>
					<edi:zgrid id="TransportGrid" runat="server" Caption="Transport" CssClass="DetailsTable" BindTo="RelatedTransportsInLegOrder" DisableCollapsing="true">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<edi:zgrid id="PackLinesGrid" runat="server" Caption="Goods / Packs" CssClass="DetailsTable" BindTo="OuterPackLines" DisableCollapsing="true">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<edi:zgrid id="OrdersGrid" runat="server" Caption="Orders" CssClass="DetailsTable" BindTo="AttachedOrders" DisableCollapsing="true">
						<PagerStyle Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<edi:zgrid id="ContainerGrid" runat="server" CssClass="DetailsTable" BindTo="ContainersOnConsols" Caption="Containers" DisableCollapsing="true">
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<edi:ZGrid id="ReferenceDataGrid" runat="server" CssClass="DetailsTable" BindTo="Numbers" Caption="Reference Numbers"
						AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Label="Reference Numbers" DisableCollapsing="true"
						AllowPaging="False">
						<PagerStyle Mode="NumericPages"></PagerStyle>
						<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:ZGrid>
					<edi:zcollapsablepanel id="DeliveryPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True">
						<asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="DeliveryUnpdatePanel">
							<ContentTemplate>				
								<edi:zdatagrid id="DeliveryGrid" runat="server" Caption="Delivery Request" CssClass="DetailsTable" BindTo="DeliveryConfirms" AllowAdd="True" AllowDelete="True" AllowEdit="True" ShowFooter="False" AutoGenerateColumns="False">
									<PagerStyle Mode="NumericPages"></PagerStyle>
									<ItemStyle CssClass="DetailsCell"></ItemStyle>
									<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
								</edi:zdatagrid><asp:Button ID="SaveConfirmations" runat="server" onclick="SaveConfirmations_Click" Visible = "False" />
							</ContentTemplate>
						</asp:UpdatePanel>		                
					</edi:zcollapsablepanel>
					<edi:zgrid id="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="true">
						<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<br />
					<br />				        
					<edi:zgrid id="ChargesGrid" runat="server" CssClass="DetailsTable" BindTo="InvoiceLoader.Transactions" Caption="Related Invoices" DisableCollapsing="true">
						<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<div id="StatusHolder" runat="server" visible="false" class="ShipmentStatusHolder">
					</div>
					<edi:zgrid id="CustomsEntriesDataGrid" runat="server" CssClass="DetailsTable" DisableCollapsing="true" Caption="Customs entries"
						BindTo="LastDeclaration.CustomsEntryHeaders">
						<PagerStyle Mode="NumericPages"></PagerStyle>
						<ItemStyle CssClass="DetailsCell"></ItemStyle>
						<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					</edi:zgrid>
					<edi:zcollapsablepanel id="NotesPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Notes">
						<edi:znotescontrol id="notes" runat="server" BindTo="NotesHelper.VisibleNotes"></edi:znotescontrol>
					</edi:zcollapsablepanel>
				</div>
				<div id="NotFoundError" runat="server">
					<edi:ztextlabel id="ShipmentNotFoundLabel" runat="server" CssClass="PageTitle"></edi:ztextlabel>
				</div>
			</div>
		</form>
	</body>
</html>
