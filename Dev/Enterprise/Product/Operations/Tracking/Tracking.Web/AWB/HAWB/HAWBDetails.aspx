<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HAWBDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.HAWBDetails" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
	<head>
		<title>HAWB Details</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
						<edi:ztextlabel id="ShipmentDetailsLabel" runat="server" CssClass="PageTitle">HAWB #</edi:ztextlabel>
						<edi:ztextlabel id="Ztextlabel5" runat="server" CssClass="PageTitle" BindTo="EH_WayBillNumber"></edi:ztextlabel>
						<edi:ztextlabel id="ZtextlabelStatus" runat="server" CssClass="PageTitle MessageStatus" BindTo="AWBMessagingStatusDescription"></edi:ztextlabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div id="EditButtonDiv" runat="server" class="ContentSection">
						<asp:LinkButton ID="ParentBill" runat="server" Text="" />
						|
						<asp:LinkButton ID="ViewMessages" runat="server" Text="View Messages" />
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
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="Label3" runat="server">HAWB #: </asp:label>
												</td>
												<td>
													<edi:ztextbox id="HAWBText" runat="server" BindTo="EH_WayBillNumber"></edi:ztextbox>
												</td>
											</tr>
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="OriginLabel" runat="server">Origin: </asp:label>
												</td>
												<td>
													<edi:ztextbox id="OriginLabelText" runat="server" BindTo="EH_AWBOriginCode"></edi:ztextbox>
												</td>
											</tr>
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="DestinationLabel" runat="server">Destination: </asp:label>
												</td>
												<td>
													<edi:ztextbox id="DestinationLabelText" runat="server" BindTo="EH_AirportOfDestinationCode"></edi:ztextbox>
												</td>
											</tr>
										 </table>
								    </td>
									<td valign="top" style="padding-left:50px;">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="Label1" runat="server">Pieces: </asp:label>
												</td>
												<td>
													<edi:ZTextBox id="Ztextlabel23" runat="server" BindTo="NumberOfPieces"></edi:ZTextBox>
												</td>
											</tr>
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="Label2" runat="server">Actual Weight: </asp:label>
												</td>
												<td>
													<edi:ZNumericTextBox Decimals="3" id="Ztextlabel2" runat="server" BindTo="ActualWeight"></edi:ZNumericTextBox>
													<edi:ZDropDownList ID="WeightInLBsOrKGs" runat="server" BindTo="ActualWeightUnit" BindToList="RateUQList"></edi:ZDropDownList>
												</td>
											</tr>
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="Label4" runat="server">SLAC: </asp:label>
												</td>
												<td>
													<edi:ZNumericTextBox id="Ztextlabel1" runat="server" BindTo="EH_ShippingLoadAndCount"></edi:ZNumericTextBox>
												</td>
											</tr>
										 </table>
									</td>
								</tr>
								<tr class="HAWBDetailsSection">
									<td>
										<edi_tracking:AWBAddressControl CssClass = "HAWBAddress" runat="server" AddressLineCssClass="HAWBAddressLine" id="Shipper" Caption="Shipper:" AddressType="Shipper" UseCaptionForCompany="true"></edi_tracking:AWBAddressControl>
									</td>
									<td>
										<edi_tracking:AWBAddressControl CssClass = "HAWBAddress" runat="server" AddressLineCssClass="HAWBAddressLine" id="Consignee" Caption="Consignee:" AddressType="Consignee" UseCaptionForCompany="true"></edi_tracking:AWBAddressControl>
									</td>
								</tr>
								<tr>
									<td valign="top" colspan="2">
										<table class="ResultsTable" cellSpacing="0" cellPadding="0">
											<tr>
												<td class="HAWBSectionTitle">
													<asp:label id="Label35" runat="server">Goods&nbsp; Description:</asp:label>
												</td>
												<td>
													<%--<edi:ZTextBox id="Ztextbox2" Rows="8" runat="server" Width="600px" BindTo="NatureAndQtyOfGoods" TextMode="MultiLine"></edi:ZTextBox>--%>
													<edi:ZTextBox id="Ztextbox1" runat="server" Width="600px" BindTo="AWBRateLine1.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox2" runat="server" Width="600px" BindTo="AWBRateLine2.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox3" runat="server" Width="600px" BindTo="AWBRateLine3.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox4" runat="server" Width="600px" BindTo="AWBRateLine4.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox5" runat="server" Width="600px" BindTo="AWBRateLine5.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox6" runat="server" Width="600px" BindTo="AWBRateLine6.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox7" runat="server" Width="600px" BindTo="AWBRateLine7.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox8" runat="server" Width="600px" BindTo="AWBRateLine8.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
													<edi:ZTextBox id="Ztextbox9" runat="server" Width="600px" BindTo="AWBRateLine9.ER_NatureAndQtyOfGoods"></edi:ZTextBox><br />
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</tbody>
						</table>
					</div>
					<div id="Div1" runat="server" class="ContentSection">
						<asp:button ID="SaveHAWB" runat="server" Text="Save HAWB" OnClick="SaveHAWB_Click" />
						<asp:button ID="SendFHL" runat="server" Text="Re-send This Bill" OnClick="SendFHL_Click" />
					</div>
				</div>

				<div id="NotFoundError" runat="server">
					<edi:ztextlabel id="ShipmentNotFoundLabel" runat="server" CssClass="PageTitle"></edi:ztextlabel>
				</div>
			</div>
		</form>
	</body>
</html>
