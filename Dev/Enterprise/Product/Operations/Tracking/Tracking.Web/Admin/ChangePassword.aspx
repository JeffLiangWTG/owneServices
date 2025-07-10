<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" ViewStateEncryptionMode='Always' AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Enterprise.Tracking.Web.Admin.ChangePassword" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Change Password</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<edi:ZTextLabel ID="ChangePasswordHeadingLabel" runat="server" CssClass="PageTitle"
				Font-Bold="True">Change Password</edi:ZTextLabel><br />
			<br />

			<div id="contact-lists" class="login-row">
				<edi:ZTextLabel id="ChangePasswordInstructionsLabel" runat="server" Text="Your password controls access to the following accounts:"></edi:ZTextLabel>
				<div id="ContactsBox" class="table-wrapper" runat="server">
					<table>
						<thead>
						<tr>
							<th>
								Person
							</th>
							<th>
								Related Account(s)
							</th>
						</tr>
						</thead>
						<tbody>
						<tr style="background-color: #f7f7f7;">
							<td>
								<edi:ZTextLabel id="Person" runat="server" BindTo="Name"></edi:ZTextLabel> 
							</td>
							<td>
								<edi:ZTextLabelNoEncode id="RelatedAccounts" runat="server"></edi:ZTextLabelNoEncode> 
							</td>
						</tr>
						</tbody>
					</table>
				</div>
			</div>

			<div id="SetPasswordBox" runat="server">
				<table>
					<tr>
						<td align="right" style="height: 30px">
							<span style="color: #777777">Current Password:</span></td>
						<td style="height: 30px">&nbsp;<asp:TextBox ID="CurrentPassword" runat="server" MaxLength="40" TextMode="Password"
							Width="200px"></asp:TextBox></td>
					</tr>
					<tr>
						<td align="right" style="height: 30px">
							<span style="color: #777777">New Password:</span></td>
						<td style="height: 30px">&nbsp;<asp:TextBox ID="NewPassword" runat="server" MaxLength="40" TextMode="Password"
							Width="200px"></asp:TextBox></td>
					</tr>
					<tr>
						<td align="right" style="height: 30px">
							<span style="color: #777777">Confirm New Password:</span></td>
						<td style="height: 30px">&nbsp;<asp:TextBox ID="NewPasswordConfirm" runat="server" MaxLength="40" TextMode="Password"
							Width="200px"></asp:TextBox></td>
					</tr>
					<tr>
						<td style="height: 26px">&nbsp;</td>
						<td style="height: 26px">&nbsp;<asp:Button ID="Update" runat="server" CssClass="button" OnClick="Update_Click"
							Text="Change Password" /></td>
					</tr>
				</table>
			</div>
			<asp:Label ID="PasswordChangeMessage" runat="server" CssClass="ErrorMessage"></asp:Label>
			<br />
			<br />
		</div>
	</form>
</body>
</html>
