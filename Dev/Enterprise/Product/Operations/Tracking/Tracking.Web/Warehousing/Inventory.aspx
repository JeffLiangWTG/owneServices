<%@ Page language="c#" Codebehind="Inventory.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Inventory" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Inventory</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1" />
		<meta name="CODE_LANGUAGE" Content="C#" />
		<meta name="vs_defaultClientScript" content="JavaScript" />
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<DIV id="Title">
					<edi:ZTextLabel id="TitleLabel" runat="server" CssClass="PageTitle">Inventory</edi:ZTextLabel>
					&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
					<asp:HyperLink id="SwitchHyperLink" runat="server" />
				</DIV>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
					<div id="SearchControlHolder" runat="server"></div>
				</div>
			</div>
		</form>
	</body>
</html>
