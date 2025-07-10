<%@ Page language="c#" Codebehind="ImporterSecurityFilingDetails.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.ImporterSecurityFiling.ImporterSecurityFilingDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title>Importer Security Filing</title>
        <meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" /> 
		<meta content="C#" name="CODE_LANGUAGE" />
		<meta content="JavaScript" name="vs_defaultClientScript"/>
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
    </head>
    <body id="DefaultBody" runat="server">
        <form id="form1" method="post" runat="server">
            <div id="OuterContentPane" runat="server">
            	<div id="Title">
				    <edi:ZTextLabel id="TitleLabel" runat="server" CssClass="PageTitle">ISF #</edi:ZTextLabel>&nbsp;
					<edi:ZTextLabel id="ZtextlabelJobRefNo" runat="server" CssClass="PageTitle" BindTo="BF_JobReference"></edi:ZTextLabel>
					<edi:ZTextLabel runat="server" id="CustomsStatusDescription" CssClass="PageTitle MessageStatus" BindTo="BF_CustomsStatusDescription" HideIfBlank="true" />
				</div>
				<br />
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" class="ContentSection">
					<div id="ButtonsDiv" runat="server" class="ContentSection">
			            <asp:button id="EditISF" runat="server" Text="Edit ISF" onclick="EditISF_Click"/>&nbsp;
			            <asp:button id="DeleteISF" runat="server" Text="Delete ISF" onclick="DeleteISF_Click"/>&nbsp;
			            <asp:button id="DuplicateISF" runat="server" Text="Copy ISF" onclick="DuplicateISF_Click"></asp:button>&nbsp;
                        <edi_tracking:ZDocumentsMenu  id="DocsMenu" runat="server"/>
					</div>
					<div id="DetailsControls" runat="server" class="ContentSection">
					<table class="ResultsTable">
						<tr>
							<td>
								<div id="SharedDetails" runat="server">
									<table class="ResultsTable">
									<tr><td class="SectionTitle" colspan="3" align="left">Customs Details</td></tr>
									<tr id="CustomsReferenceRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="CustomsRefLabel" runat="server"  CssClass="SectionTitle">Customs Ref:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="ZTextLabel1" runat="server" BindTo="BF_CustomsReference"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="CustomsStatusRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="StatusLabel" runat="server"  CssClass="SectionTitle">Status:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="ZTextLabel2" runat="server" BindTo="BF_CustomsStatusDescription"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="ActionReasonCodeArea" runat="server">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="ActionReasonCodeLabel" runat="server"  CssClass="SectionTitle">Action Reason Code:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="ActionReasonCodeZTextLabel3" runat="server" BindTo="ActionReasonCodeDescription"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="FirstAcceptedRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="FirstAcceptedLabel" runat="server"  CssClass="SectionTitle">First Accepted:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZDateTimeLabel id="ZTextLabel3" runat="server" BindTo="BF_FirstAcceptedDate" DateTimeFormat="Short"></edi:ZDateTimeLabel>
										</td>
									</tr>
									<tr id="LastAcceptedRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="LastAcceptedLabel" runat="server"  CssClass="SectionTitle">Last Accepted:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZDateTimeLabel id="ZDateTimeLabel1" runat="server" BindTo="BF_LastAcceptedDate" DateTimeFormat="Short"></edi:ZDateTimeLabel>
										</td>
									</tr>
								</table>
								</div>
							</td>
							<td valign="top" rowspan="3">
                            <div class="ContentSection">
								<edi:ZGrid id="ReferenceDataGrid" runat="server" CssClass="DetailsTable" BindTo="ReferenceDatas"
									AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Label="References" DisableCollapsing="true"
									AllowPaging="False">
									<PagerStyle Mode="NumericPages"></PagerStyle>
									<SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
									<ItemStyle CssClass="DetailsCell"></ItemStyle>
									<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
								</edi:ZGrid>
                            </div>
							</td>
						</tr>
						<tr><td colspan="3">&nbsp;</td></tr>
						<tr>
							<td>
								<table id="ISFDetails" class="ResultsTable">									
									<tr><td class="SectionTitle" colspan="3" align="left">ISF Details</td></tr>
									<tr id="EntryTypeRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="EntryTypeLabel" runat="server" CssClass="DetailsItem">Entry Type:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="EntryTypeTextLabel" runat="server" BindTo="BF_EntryType"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="ShipmentTypeRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="ShipmentTypeLabel" runat="server" CssClass="DetailsItem">Shipment Type:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="ShipmentTypeTextLabel" runat="server" BindTo="BF_ShipmentType"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="TransportModeRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="TransportModeLabel" runat="server" CssClass="DetailsItem">Transport Mode:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="TransportModeTextLabel" runat="server" BindTo="TransportModeCodeDescription"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="CarrierSCACRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="CarrierSCACLabel" runat="server" CssClass="DetailsItem">Carrier SCAC:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="CarrierSCACTextLabel" runat="server" BindTo="BF_SCAC"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="ImporterRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="ImporterLabel" runat="server" CssClass="DetailsItem">Importer:</asp:label>
										</td>
										<td style="white-space:nowrap">
											<edi:ZTextLabel id="ImporterZTextLabel" runat="server" BindTo="Importer.OH_FullName"></edi:ZTextLabel>
										</td>
									</tr>
									<tr id="OwnerReferenceRow">
										<td></td>
										<td style="white-space:nowrap">
											<asp:label id="LabelOwnerRef" runat="server" CssClass="DetailsItem">Owner Ref:</asp:label>
										</td>
										<td><edi:ZTextLabel id="ZTextLabelOwnerRef" runat="server" BindTo="BF_OwnerReference"></edi:ZTextLabel></td>
									 </tr>
								</table>
							</td>
						</tr>
						<tr><td colspan="2">&nbsp;</td></tr>
						<tr>
							<td valign="top">
								<div id="RoutingDetails" runat="server" style="vertical-align:top;">
								<table id="RoutingDetailsTable" class="ResultsTable">
								<tr><td class="SectionTitle" colspan="3" align="left">Routing Details</td></tr>
								<tr id="ETDRow">
									<td></td>
									<td style="white-space:nowrap">
										<asp:label id="LabelETD" runat="server" CssClass="DetailsItem">ETD:</asp:label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZDateTimeLabel id="ZTextLabelETD" runat="server" BindTo="FirstTransport.JW_ETD"></edi:ZDateTimeLabel>
									</td>
									<td></td>
									<td style="white-space:nowrap">
										<asp:label id="LabelLoadPort" runat="server" CssClass="DetailsItem">Load Port:</asp:label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZTextLabel id="ZTextLabelLoadPort" runat="server" BindTo="FirstTransport.JW_RL_NKLoadPort"></edi:ZTextLabel>
									</td>
								</tr>
								<tr id="ETARow">
									<td></td>
									<td style="white-space:nowrap">
										<asp:label id="LabelETA" runat="server" CssClass="DetailsItem">ETA:</asp:label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZDateTimeLabel id="ZTextLabelETA" runat="server" BindTo="FirstTransport.JW_ETA"></edi:ZDateTimeLabel>
									</td>
									<td></td>
									<td style="white-space:nowrap">
										<asp:label id="LabelDiscPort" runat="server" CssClass="DetailsItem">Disc Port:</asp:label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZTextLabel id="ZTextLabelDiscPort" runat="server" BindTo="FirstTransport.JW_RL_NKDiscPort"></edi:ZTextLabel>
									</td>
								</tr>
								<tr id="VesselRow">
									<td></td>
									<td style="white-space:nowrap">
										<asp:label id="LabelVessel" runat="server" CssClass="DetailsItem">Vessel:</asp:label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZTextLabel id="ZTextLabelVessel" runat="server" BindTo="FirstTransport.JW_Vessel"></edi:ZTextLabel>
									</td>
									<td></td>
									<td style="white-space:nowrap">
										<asp:label id="LabelVoyageFlight" runat="server" CssClass="DetailsItem">Voyage Flight:</asp:label>
									</td>
									<td style="white-space:nowrap">
										<edi:ZTextLabel id="ZTextLabelVoyageFlight" runat="server" BindTo="FirstTransport.JW_VoyageFlight"></edi:ZTextLabel>
									</td>
								</tr>
								</table>
								<br />
								</div>							
								<div id="ISF10Details" runat="server" style="vertical-align:top;">
									<table style="border:none; padding:0; margin:0;" cellpadding="0" cellspacing="0" runat="server" id="ImporterDetailsTable">
										<tr>
											<td id="ImporterDetails" runat="server" valign="top">
												<table class="ResultsTable">
													<tr><td class="SectionTitle" colspan="3" align="left">Importer Details</td></tr>
													<tr id="ImporterCodeTypeRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:Label id="ImporterIdTypeLabel" runat="server" CssClass="DetailsItem">Importer ID Type:</asp:Label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="BF_ImporterCodeType" runat="server" BindTo="BF_ImporterCodeType"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="ImporterCodeRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="ImporterIDLabel" runat="server" CssClass="DetailsItem">Importer ID:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="BF_ImporterCode" runat="server" BindTo="BF_ImporterCode"></edi:ZTextLabel>
														</td>
													</tr>
													<tr runat="server" id="ImporterFullNameInfo">								
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="ImporterFullNameLabel" runat="server" CssClass="DetailsItem">Full Legal Name:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="ImporterFullName" runat="server" BindTo="BF_ImporterFullName"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="ImporterDateOfBirthRow">
														<td></td>
				    									<td style="white-space:nowrap">
															<asp:label id="DOBLabel" runat="server" CssClass="DetailsItem">DOB:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZDateTimeLabel id="BF_DateOfBirth" runat="server" BindTo="BF_DateOfBirth" DateTimeFormat="Short"></edi:ZDateTimeLabel>
														</td>				        
													</tr>
													<tr id="ImporterCountryOfIssueRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="CountryOfIssueLabel" runat="server" CssClass="DetailsItem">Country Of Issue:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="BF_CountryOfIssue" runat="server" BindTo="BF_CountryOfIssue"></edi:ZTextLabel>
														</td>
													</tr>
													<tr><td colspan="3">&nbsp;</td></tr>
												</table>
											</td>
											<td id="CneeDetails" runat="server" valign="top">
												<table class="ResultsTable">
													<tr>								
														<td class="SectionTitle" colspan="3" align="left">Consignee Details</td>
													</tr>
													<tr id="ConsigneeCodeTypeRow">
			        									<td></td>
		   												<td style="white-space:nowrap">
															<asp:label id="CneeIdType" runat="server" CssClass="DetailsItem">Consignee ID Type:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="CneeIdTypeTextLabel" runat="server" BindTo="BF_ConsigneeCodeType"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="ConsigneeCodeRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="CneeID" runat="server" CssClass="DetailsItem">Consignee ID:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="CneeIDTextLabel" runat="server" BindTo="BF_ConsigneeCode"></edi:ZTextLabel>
														</td>
													</tr>
													<tr runat="server" id="ConsigneeFullNameInfo">								
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="Label1" runat="server" CssClass="DetailsItem">Full Legal Name:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="ZTextLabel4" runat="server" BindTo="BF_ConsigneeFullName"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="ConsigneeDateOfBirthRow">
														<td></td>
				    									<td style="white-space:nowrap">
															<asp:label id="Label2" runat="server" CssClass="DetailsItem">DOB:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZDateTimeLabel id="ZDateTimeLabel2" runat="server" BindTo="BF_ConsigneeDateOfBirth" DateTimeFormat="Short"></edi:ZDateTimeLabel>
														</td>				        
													</tr>
													<tr id="ConsigneeCountryOfIssueRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="Label3" runat="server" CssClass="DetailsItem">Country Of Issue:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="ZTextLabel5" runat="server" BindTo="BF_ConsigneeCountryOfIssue"></edi:ZTextLabel>
														</td>
													</tr>
													<tr><td colspan="3">&nbsp;</td></tr>
												</table>
											</td>
										</tr>
										<tr id="BondDetails" runat="server">
											<td colspan="2" runat="server" valign="top">
												<table class="ResultsTable">
													<tr><td class="SectionTitle" colspan="3" align="left">Bond Details</td></tr>
													<tr id="BondHolder" runat="server">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="BondHolderLabel" runat="server" CssClass="DetailsItem">Bond Holder:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="BF_BondNumberOrHolder" runat="server" BindTo="BF_BondNumberOrHolder"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="BondActivityCodeRow">
														<td></td>
														<td style="white-space:nowrap">
														<asp:label id="BondActivityCodeLabel" runat="server" CssClass="DetailsItem">Bond Activity Code:</asp:label>
														</td>
														<td style="white-space:nowrap">
														<edi:ZTextLabel id="BondActivityCodeTextLabel" runat="server" BindTo="BondActivityCodeDescription"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="BondTypeRow">
														<td></td>
														<td style="white-space:nowrap">
														<asp:label id="BondTypeLabel" runat="server" CssClass="DetailsItem">Bond Type:</asp:label>
														</td>
														<td style="white-space:nowrap">
														<edi:ZTextLabel id="BondTypeTextLabel" runat="server" BindTo="BondTypeDescription"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="SuretyCodeRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="BondSuretyCodeLabel" runat="server" CssClass="DetailsItem">Surety Code:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="BF_SuretyCode" runat="server" BindTo="BF_SuretyCode"></edi:ZTextLabel>
														</td>
													</tr>
													<tr id="BondEntryNumberRow">
														<td></td>
														<td style="white-space:nowrap">
															<asp:label id="BondEntryNumber" runat="server" CssClass="DetailsItem">Entry Number:</asp:label>
														</td>
														<td style="white-space:nowrap">
															<edi:ZTextLabel id="BondEntryNumberTextLabel" runat="server" BindTo="BF_EntryNumber"></edi:ZTextLabel>
														</td>
													</tr>
												</table>
											</td>
										</tr>
									</table>
								</div>
								<div id="ISF5Details" runat="server">
									<div id="Locations" runat="server">
										<table class="ResultsTable">
										<tr><td class="SectionTitle" colspan="4" align="left">Locations</td></tr>
										<tr id="UnloadPortRow">
											<td></td>
											<td style="white-space:nowrap">
												<asp:label id="UnloadPortLabel" runat="server" CssClass="DetailsItem">Unload Port:</asp:label>
											</td>
											<td style="white-space:nowrap">
												<edi:ZTextLabel id="UnloadPortTextLabel" runat="server" BindTo="BF_RL_NKPortOfUnload"></edi:ZTextLabel>
											</td>
										</tr>
										<tr id="DeliveryPortRow">
											<td></td>
											<td style="white-space:nowrap">
												<asp:label id="DeliveryPortLabel" runat="server" CssClass="DetailsItem">Delivery Port:</asp:label>
											</td>
											<td style="white-space:nowrap">
												<edi:ZTextLabel id="DeliveryPortTextLabel" runat="server" BindTo="BF_RL_NKPlaceOfDelivery"></edi:ZTextLabel>
											</td>
										</tr>  
										</table>
									</div> 
								</div>
							</td>
							<td valign=top>
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
							</td>
						</tr>					
					</table>
                    </div>
                    <br />
					<div id="OrganisationsDetails" runat="server">
						<table class="ResultsTable" title="Organisations">
							<tr>
								<td>
									<div id="ShipToPartyAddressHolder" runat="server" class="ContentSection">
									<table class="ResultsTable" id="ShipToPartyTable">
											<tr>
												<asp:label align="center" id="ShipToLabel" runat="server" CssClass="DetailsItem">Ship To:</asp:label>
											</tr>
											<tr>													
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyName" runat="server" BindTo="MainShipToParty.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyContact" runat="server" BindTo="MainShipToParty.E2_Contact"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyAddress1" runat="server" BindTo="MainShipToParty.E2_Address1"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyAddress2" runat="server" BindTo="MainShipToParty.E2_Address2"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyCity" runat="server" BindTo="MainShipToParty.E2_City"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyState" runat="server" BindTo="MainShipToParty.E2_State"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyPost" runat="server" BindTo="MainShipToParty.E2_Postcode"></edi:ZTextLabel>
    											</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyCountryCode" runat="server" BindTo="MainShipToParty.E2_RN_NKCountryCode"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="ShipToGovRegNoArea" runat="server">
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyRegNoType" runat="server" BindTo="MainShipToParty.E2_GovRegNumType"></edi:ZTextLabel>
												</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ShipToPartyRegNum" runat="server" BindTo="MainShipToParty.E2_SocialSecurityNumberOrGovRegNum"></edi:ZTextLabel>
												</td>
											</tr>
										</table>
									</div>
								</td>
								<td>&nbsp;</td>
								<td>
									<div id="BuyingPartyAddressHolder" runat="server" class="ContentSection">
										<table class="ResultsTable" id="BuyingPartyTable">
											<tr>
												<asp:label align="center" id="BuyingPartyLabel" runat="server" CssClass="DetailsItem">Buying Party:</asp:label>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyName" runat="server" BindTo="BuyingParty.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyContact" runat="server" BindTo="BuyingParty.E2_Contact"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyAddress1" runat="server" BindTo="BuyingParty.E2_Address1"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyAddress2" runat="server" BindTo="BuyingParty.E2_Address2"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyCity" runat="server" BindTo="BuyingParty.E2_City"></edi:ZTextLabel>
    											</td>
    											<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyState" runat="server" BindTo="BuyingParty.E2_State"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyPost" runat="server" BindTo="BuyingParty.E2_Postcode"></edi:ZTextLabel>
    											</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyCountryCode" runat="server" BindTo="BuyingParty.E2_RN_NKCountryCode"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="BuyingPartyGovRegNoArea" runat="server">
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyRegNoType" runat="server" BindTo="BuyingParty.E2_GovRegNumType"></edi:ZTextLabel>
												</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BuyungPartyRegNo" runat="server" BindTo="BuyingParty.E2_SocialSecurityNumberOrGovRegNum"></edi:ZTextLabel>
												</td>
											</tr>
										</table>
									</div>
								</td>
								<td>&nbsp;</td>
								<td>
									<div id="BookingPartyAddressHolder" runat="server" class="ContentSection">
										<table class="ResultsTable" id="BookingPartyTable">
											<tr>
												<asp:label align="center" id="BookingPartyLabel" runat="server" CssClass="DetailsItem">Booking Party:</asp:label>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyName" runat="server" BindTo="BookingParty.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyContact" runat="server" BindTo="BookingParty.E2_Contact"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyAddress1" runat="server" BindTo="BookingParty.E2_Address1"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyAddress2" runat="server" BindTo="BookingParty.E2_Address2"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyCity" runat="server" BindTo="BookingParty.E2_City"></edi:ZTextLabel>
    											</td>
    											<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyState" runat="server" BindTo="BookingParty.E2_State"></edi:ZTextLabel>
    											</td>
    											<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyPost" runat="server" BindTo="BookingParty.E2_Postcode"></edi:ZTextLabel>
    											</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyCountryCode" runat="server" BindTo="BookingParty.E2_RN_NKCountryCode"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="BookingPartyGovRegNoArea" runat="server">
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyRegNoType" runat="server" BindTo="BookingParty.E2_GovRegNumType"></edi:ZTextLabel>
												</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="BookingPartyRegNo" runat="server" BindTo="BookingParty.E2_SocialSecurityNumberOrGovRegNum"></edi:ZTextLabel>
												</td>
											</tr>
										</table>
									</div>
								</td>    
								</tr>
							<tr>
								<td>
									<div id="SellingPartyAddressHolder" runat="server" class="ContentSection">
										<table class="ResultsTable" id="SellingPartyTable">
											<tr>
												<asp:label align="center" id="SellingPartyLabel" runat="server" CssClass="DetailsItem">Selling Party:</asp:label>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyName" runat="server" BindTo="SellingParty.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyContact" runat="server" BindTo="SellingParty.E2_Contact"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyAddressLine1" runat="server" BindTo="SellingParty.E2_Address1"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyAddressLine2" runat="server" BindTo="SellingParty.E2_Address2"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyCity" runat="server" BindTo="SellingParty.E2_City"></edi:ZTextLabel>
    											</td>
    											<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyState" runat="server" BindTo="SellingParty.E2_State"></edi:ZTextLabel>
    											</td>
    											<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyPost" runat="server" BindTo="SellingParty.E2_Postcode"></edi:ZTextLabel>
    											</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="SellingPartyContryCode" runat="server" BindTo="SellingParty.E2_RN_NKCountryCode"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="SellingPartyGovRegNoArea" runat="server">
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="RegNoType" runat="server" BindTo="SellingParty.E2_GovRegNumType"></edi:ZTextLabel>
												</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="RegNoCode" runat="server" BindTo="SellingParty.E2_SocialSecurityNumberOrGovRegNum"></edi:ZTextLabel>
												</td>
											</tr>
										</table>
									</div>
								</td>
								<td>&nbsp;</td>
								<td>
									<div id="StuffingLocationHolder" runat="server" class="ContentSection">
										<table class="ResultsTable" id="StuffingLocationTable">
											<tr>
												<asp:label align="center" id="StuffingLocationLabel" runat="server" CssClass="DetailsItem">Stuffing Location:</asp:label>
											</tr>
											<tr>													
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationName" runat="server" BindTo="StuffingLocation.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationContact" runat="server" BindTo="StuffingLocation.E2_Contact"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationAddress1" runat="server" BindTo="StuffingLocation.E2_Address1"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationAddress2" runat="server" BindTo="StuffingLocation.E2_Address2"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationCity" runat="server" BindTo="StuffingLocation.E2_City"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationState" runat="server" BindTo="StuffingLocation.E2_State"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationPost" runat="server" BindTo="StuffingLocation.E2_Postcode"></edi:ZTextLabel>
    											</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationCountryCode" runat="server" BindTo="StuffingLocation.E2_RN_NKCountryCode"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="StuffingLocationGovRegNoArea" runat="server">
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationRegNoType" runat="server" BindTo="StuffingLocation.E2_GovRegNumType"></edi:ZTextLabel>
												</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="StuffingLocationRegNo" runat="server" BindTo="StuffingLocation.E2_SocialSecurityNumberOrGovRegNum"></edi:ZTextLabel>
												</td>
											</tr>
										</table>
									</div>
								</td>
								<td>&nbsp;</td>
								<td colspan="2">
									<div id="ConsolidatorAddressHolder" runat="server" class="ContentSection">
										<table class="ResultsTable" id="ConsolidatorTable">
											<tr>
												<asp:label align="center" id="ConsolidatorLabel" runat="server" CssClass="DetailsItem">Consolidator:</asp:label>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorName" runat="server" BindTo="Consolidator.E2_CompanyName"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorContact" runat="server" BindTo="Consolidator.E2_Contact"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorAddress1" runat="server" BindTo="Consolidator.E2_Address1"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorAddress2" runat="server" BindTo="Consolidator.E2_Address2"></edi:ZTextLabel>
												</td>
											</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorCity" runat="server" BindTo="Consolidator.E2_City"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorPost" runat="server" BindTo="Consolidator.E2_Postcode"></edi:ZTextLabel>
    											</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorState" runat="server" BindTo="Consolidator.E2_State"></edi:ZTextLabel>
    											</td>
    										</tr>
											<tr>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorCountryCode" runat="server" BindTo="Consolidator.E2_RN_NKCountryCode"></edi:ZTextLabel>
												</td>
											</tr>
											<tr id="ConsolidatorGovRegNoArea" runat="server">
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorRegNoType" runat="server" BindTo="Consolidator.E2_GovRegNumType"></edi:ZTextLabel>
												</td>
												<td style="white-space:nowrap">
													<edi:ZTextLabel id="ConsolidatorRegNo" runat="server" BindTo="Consolidator.E2_SocialSecurityNumberOrGovRegNum"></edi:ZTextLabel>
												</td>
											</tr>
										</table>
									</div>
								</td>
							</tr>
							<tr>
								<td colspan="8"></td>
							</tr>
						</table>
					</div>
				    <div id="LowValueDetails" runat="server" class="ContentSection">
				    <table class="ResultsTable" title="LowValueDetails">
						    <tr><td class="SectionTitle">Low-Value Details:</td></tr>
				    </table>
				    <table class="ResultsTable" title="LowValueDetails">
					    <tr id="ShipmentSubTypeRow">
						    <td>&nbsp;</td>
						    <td>Ship. Sub-Type:</td>
						    <td>
							    <edi:ztextlabel id="ShipSubTypeDropDown" runat="server" BindTo="ShipmentSubTypeDescription"></edi:ztextlabel>
						    </td>
					    </tr>
					    <tr id="EstimatedValueRow">
						    <td>&nbsp;</td>
						    <td>Estimated Value:</td>
						    <td>
                                <edi:znumericlabel id="EstimatedValueEditBox" runat="server" BindTo="BF_EstimatedValue"></edi:znumericlabel>
						    </td>
					    </tr>
					    <tr id="EstimatedQtyRow">
						    <td>&nbsp;</td>
						    <td>Estimated Qty.:</td>
						    <td>
                                <edi:znumericlabel id="EstimatedQtyEditBox" runat="server" BindTo="BF_EstimatedQuantity"></edi:znumericlabel>&nbsp;<edi:ztextlabel id="EstimatedQuantityUQDropDown" runat="server" BindTo="BF_EstimatedQuantityUQ"></edi:ztextlabel>
						    </td>
					    </tr>
					    <tr id="EstimatedWeightRow">
						    <td>&nbsp;</td>
						    <td>Estimated Weight:</td>
						    <td>
                                <edi:znumericlabel id="EstimatedWeightEditBox" runat="server" BindTo="BF_EstimatedWeight"></edi:znumericlabel>&nbsp;<edi:ztextlabel id="EstimatedWeightUQDropDown" runat="server" BindTo="BF_EstimatedWeightUQ"></edi:ztextlabel>
                            </td>
					    </tr>
				    </table>
				    </div>
					<div id="Grids" runat="server" class="ContentSection">
					    <edi:ZGrid id="AddressesDataGrid" runat="server" CssClass="DetailsTable" BindTo="ManufacturerAddresses"
						    AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Label="Manufacturer Addresses" DisableCollapsing="True"
						    AllowPaging="False">
						    <PagerStyle Mode="NumericPages"></PagerStyle>
						    <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
						    <ItemStyle CssClass="DetailsCell"></ItemStyle>
						    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					    </edi:ZGrid>
				        <edi:ZGrid id="LinesDataGrid" runat="server" CssClass="DetailsTable" BindTo="Lines" Label="Lines" DisableCollapsing="True"
					        AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" AutoGenerateColumns="False"
					        AllowPaging="False">
					        <PagerStyle Mode="NumericPages"></PagerStyle>
					        <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
					        <ItemStyle CssClass="DetailsCell"></ItemStyle>
					        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				        </edi:ZGrid>
					    <edi:ZGrid id="ContainersDataGrid" runat="server" CssClass="DetailsTable" BindTo="Equipments"
						    AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Label="Containers" DisableCollapsing="True"
						    AllowPaging="False">
						    <PagerStyle Mode="NumericPages"></PagerStyle>
						    <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
						    <ItemStyle CssClass="DetailsCell"></ItemStyle>
						    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					    </edi:ZGrid>
					    <edi:ZGrid id="AdditionalShipToAddressesDataGrid" runat="server" CssClass="DetailsTable" BindTo="DocAddressesExcludeManufacturer"
						    AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" Label="Additional Addresses" DisableCollapsing="True"
						    AllowPaging="False">
						    <PagerStyle Mode="NumericPages"></PagerStyle>
						    <SelectedItemStyle BackColor="#E0E0E0"></SelectedItemStyle>
						    <ItemStyle CssClass="DetailsCell"></ItemStyle>
						    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
					    </edi:ZGrid>
				        <edi:ZGrid id="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="true" >
					        <PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
					        <ItemStyle CssClass="DetailsCell"></ItemStyle>
					        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				        </edi:ZGrid>
					</div>					
				</div>
            </div>
        </form>
    </body>
</html>
