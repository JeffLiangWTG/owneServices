<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ page language="C#" viewstateencryptionmode='Always' autoeventwireup="true" codebehind="ResetPassword.aspx.cs" inherits="Enterprise.Tracking.Web.Admin.ResetPassword" %>
<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Reset Password</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<edi:ztextlabel id="ChangePasswordHeadingLabel" runat="server" cssclass="PageTitle"
				font-bold="True">Reset Password</edi:ztextlabel>
			<div id="HeadingMessageDiv" runat="server">
				<div>
					<edi:ZTextLabel id="PasswordExpiredMessageLabel" runat="server" Text="Your password has expired due to an enforced password rotation policy. Please enter a new password." Visible="False"></edi:ZTextLabel>
				</div>
			</div>
			<br />
			<br />
			<table id="resetTable" runat="server">
				<tr>
					<td align="right" style="height: 30px">
						<span style="color: #777777">Company Code:</span></td>
					<td style="height: 30px">
						<cc1:ZDropDownList ID="OrgCodeDropDownList" runat="server" DataTextField="OH_FullName" DataValueField="OH_Code" BindToList="OrgHeaders" BindTo="CompanyCode" DisplayStyle="CodeAndDescription" Width="200" EnableViewState="False">
						</cc1:ZDropDownList>
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
					<td style="height: 26px"><asp:button id="Update" runat="server" cssclass="button" onclick="Update_Click" text="Reset Password" /></td>
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
