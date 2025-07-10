<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page language="c#" Codebehind="ImporterSecurityFiling.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.ImporterSecurityFiling.ImporterSecurityFiling" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Importer Security Filing</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server" CssClass="PageTitle"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
				    <div id="Title">
					    <edi:ZTextLabel id="PageTitleLabel" runat="server" CssClass="PageTitle">Importer Security Filing</edi:ZTextLabel>
				    </div>
					<div id="SearchControlHolder" runat="server"></div>
				</div>
			</div>
		</form>
	</body>
</html>
