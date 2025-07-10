<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MAWBDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.MAWBDetails" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
	<head>
		<title>MAWB Details</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
						<edi:ztextlabel id="ShipmentDetailsLabel" runat="server" CssClass="PageTitle">MAWB #</edi:ztextlabel>
						<edi:ztextlabel id="Ztextlabel5" runat="server" CssClass="PageTitle" BindTo="EH_WayBillNumber"></edi:ztextlabel>
						<edi:ztextlabel id="ZtextlabelStatus" runat="server" CssClass="PageTitle MessageStatus" BindTo="AWBMessagingStatusDescription"></edi:ztextlabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div id="JumpButtons" runat="server" class="ContentSection">
						<asp:LinkButton ID="GoToHAWB" runat="server" Text="Attached HAWBs" />
						|
						<asp:LinkButton ID="ViewMessages" runat="server" Text="View Messages" />
					</div>
					<div class="ContentSection">
					    <div>
                            <edi_tracking:ZDocumentsMenu  id="DocsMenu" runat="server"/>
					    </div>
                        <br /><br />
						<div>
							<edi_tracking:AWBControl id="AWBControl" runat="server"></edi_tracking:AWBControl>
						</div>
                        <br /><br />
					</div>
					<div id="EditButtonDiv" runat="server" class="ContentSection">
						<asp:button ID="SaveMAWB" runat="server" Text="Save MAWB" OnClick="SaveMAWB_Click" />
						<asp:button ID="SendFWBFHL" runat="server" Text="Send All" OnClick="SendFWBFHL_Click" />
						<asp:button ID="ResendFWBFHL" runat="server" Text="Re-send All" OnClick="ResendFWBFHL_Click" />
						<asp:button ID="SendFWB" runat="server" Text="Re-send This Bill" OnClick="SendFWB_Click" />
					</div>
				</div>
				<div id="NotFoundError" runat="server">
					<edi:ztextlabel id="ShipmentNotFoundLabel" runat="server" CssClass="PageTitle"></edi:ztextlabel>
				</div>
			</div>
		</form>
	</body>
</html>
