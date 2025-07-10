<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ page language="C#" viewstateencryptionmode='Always' autoeventwireup="true" codebehind="SetPassword.aspx.cs" inherits="Enterprise.Tracking.Web.Admin.SetPassword" %>
<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Set Password</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<edi:ZTextLabelNoEncode id="SetPasswordHeadingLabel" runat="server" cssclass="PageTitle"
				style="font-weight: normal">Set Password</edi:ZTextLabelNoEncode>
			<br />
			<br />
			<table id="setTable" runat="server">
				<tr>
					<td align="right" style="height: 30px">
						<span style="color: #777777">Company Code:</span></td>
					<td style="height: 30px">
                        <cc1:ZTextLabel ID="OrgCodeText" runat="server" Text=""></cc1:ZTextLabel>
					</td>
				</tr>
				<tr>
					<td align="right" style="height: 30px">
						<span style="color: #777777">New Password:</span></td>
					<td style="height: 30px"><asp:textbox id="NewPassword" runat="server" maxlength="40" textmode="Password" width="200px"></asp:textbox></td>
				</tr>
				<tr>
					<td align="right" style="height: 30px">
						<span style="color: #777777">Confirm New Password:</span></td>
					<td style="height: 30px"><asp:textbox id="NewPasswordConfirm" runat="server" maxlength="40" textmode="Password" width="200px"></asp:textbox></td>
				</tr>
				<tr>
					<td style="height: 26px">&nbsp;</td>
					<td style="height: 26px"><asp:button id="Update" runat="server" cssclass="button" onclick="Update_Click" text="Set Password" /></td>
				</tr>
			</table>
			<asp:label id="PasswordChangeMessage" runat="server" cssclass="ErrorMessage"></asp:label>

			<br />
			<br />
			<span runat="server" id="back" Visible="False">Go <asp:Hyperlink id="BackLink"  NavigateUrl="../Login/Login.aspx" runat="server">back</asp:Hyperlink> to the login page.</span>
		</div>
	</form>
</body>
</html>
