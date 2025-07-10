<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ page language="c#" codebehind="ForgotPassword.aspx.cs" autoeventwireup="True" inherits="Enterprise.Tracking.Web.ForgotPassword" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Forgot Password</title>
	<meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1" />
	<meta name="CODE_LANGUAGE" content="C#" />
	<meta name="vs_defaultClientScript" content="JavaScript" />
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="ReminderBox" class="loginBox">
				E-mail:<br />
				<edi:ztextbox id="emailTextBox" runat="server" bindto="EmailAddress" maxlength="40"></edi:ztextbox>
				<br />
				<br />
				<asp:button id="RemindBtn" runat="server" text="Send Password Reset Link" onclick="RemindBtn_Click"></asp:button>
				<br />
				<asp:label id="Message" runat="server" cssclass="ErrorMessage" />
			</div>
		</div>
	</form>
</body>
</html>
