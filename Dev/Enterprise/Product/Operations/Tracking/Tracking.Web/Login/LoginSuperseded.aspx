<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" EnableViewStateMac="false" CodeBehind="LoginSuperseded.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.LoginSuperseded" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Login Superseded</title>
    <meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1" />
    <meta name="CODE_LANGUAGE" content="C#" />
    <meta name="vs_defaultClientScript" content="JavaScript" />
    <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge;" />
</head>
<body id="DefaultBody" runat="server">
<form id="Form1" method="post" runat="server">
	<div id="OuterContentPane" runat="server">
		<div id="InstructionsBox">
			<h1>User Account Update</h1>
			<edi:ZTextLabel ID="SupersededInstructionsLabel" runat="server" Text="User Account changes have occurred which affect the way you login into Web Tracker.<br>One or more of your existing user accounts will no longer be accessible, but don't worry, you'll still have access to Web Tracker.<br><br>First, a new password is required, that will be applied to all linked user accounts.<br><br>"></edi:ZTextLabel>
			<asp:button ID="SetMasterPasswordButton" runat="server" text="Continue" onclick="SetMasterPasswordButton_Click"></asp:button>
			<edi:ZTextLabel ID="ErrorMessage" runat="server" cssclass="ErrorMessage" />
		</div>
	</div>
</form>
</body>
</html>
