<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EDIMessages.aspx.cs" Inherits="Enterprise.Tracking.Web.Messaging.EDIMessages" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
	<head>
		<title>Messages</title>
	</head>
	<body id="DefaultBody" runat="server" class="popup">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
						<edi:ztextlabel id="ShipmentDetailsLabel" runat="server" CssClass="PageTitle">Messages for</edi:ztextlabel>
						<edi:ztextlabel id="Ztextlabel5" runat="server" CssClass="PageTitle" BindTo="ParentReferenceNumber"></edi:ztextlabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div class="ContentSection">
							<edi:zgrid id="EDIMessagesGrid" runat="server" CssClass="DetailsTable" BindTo="EDIMessages" Collapsed="false" DisableCollapsing="true">
								<ItemStyle CssClass="DetailsCell"></ItemStyle>
								<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
							</edi:zgrid>
					</div>
				</div>
			</div>    
		</form>
	</body>
</html>
