<%@ Page language="c#" Codebehind="Shipments.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Shipments" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Shipments</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
					<edi:ZTextLabel id="PageTitleLabel" runat="server" CssClass="PageTitle">Shipments</edi:ZTextLabel>
					&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					<asp:HyperLink id="SwitchHyperLink" runat="server">Hyperlink</asp:HyperLink>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
					<div id="SearchControlHolder" runat="server"></div>
					
				</div>
				<div id="UnshippedOrdersContentPane" runat="server">
					<DIV id="Title">
						<edi:ZTextLabel id="UnshippedOrdersTitleLabel" runat="server" CssClass="PageTitle">Unshipped Orders</edi:ZTextLabel>
					</DIV>
					<br>
					<div id="UnshippedOrdersSearchControlHolder" runat="server"></div>
				</div>
			</div>
		</form>
	</body>
</html>
