<%@ Page language="c#" Codebehind="Orders.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Orders.Orders" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Orders</title>
	</HEAD>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<DIV id="Title">
					<edi:ztextlabel id="OrdersLabel" runat="server" CssClass="PageTitle">Orders</edi:ztextlabel>
					&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					<asp:LinkButton id="SwitchHyperLink" runat="server">Hyperlink</asp:LinkButton>
				</DIV>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
					<div id="SearchControlHolder" runat="server"></div>
					<div id="TimeLineLegendHolder" runat="server" visible="false">
						<br/>
						<edi:ZTextLabel runat="server" ID="PendingLegendLabel" Text="Pending" CssClass="TimeLineLegend"/>  &nbsp;
						<edi:ZTextLabel runat="server" ID="OverdueLegendLabel" Text="Overdue" CssClass="TimeLineLegend"/> &nbsp;
						<edi:ZTextLabel runat="server" ID="CompletedLegendLabel" Text="Completed" CssClass="TimeLineLegend"/> &nbsp;
						<edi:ZTextLabel runat="server" ID="CompletedLateLegendLabel" Text="Completed Late" CssClass="TimeLineLegend"/>
					</div>
				</div>
			</div>
		</form>
	</body>
</HTML>
