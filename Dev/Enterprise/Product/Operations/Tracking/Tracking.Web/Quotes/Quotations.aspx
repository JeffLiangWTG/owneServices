<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Quotations.aspx.cs" Inherits="Enterprise.Tracking.Web.Quotes.Quotations" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Spot Quotes</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form2" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
					<edi:ztextlabel id="QuotationsLabel" runat="server" CssClass="PageTitle">Spot Quotes</edi:ztextlabel>					
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
					<div id="EmailNotice" runat="server"  class="ContentSection"></div>
					<div id="SearchControlHolder" runat="server"></div>						
				</div>
			</div>
		</form>
	</body>
</html>