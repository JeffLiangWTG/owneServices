<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewTermsAndConditions.aspx.cs" Inherits="Enterprise.Tracking.Web.ViewTermsAndConditions" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Terms and Conditions</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel id="TitleLabel" runat="server" CssClass="PageTitle">Terms and Conditions</edi:ZTextLabel>
			</div>
			<div id="TermsAndConditionsContent" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="TermsAndConditionsText" runat="server"></edi:ZTextLabel>
			</div>
		</div>
	</form>
</body>
</html>
