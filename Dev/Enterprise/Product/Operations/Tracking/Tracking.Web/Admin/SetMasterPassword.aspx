<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ page language="C#" viewstateencryptionmode='Always' autoeventwireup="true" codebehind="SetMasterPassword.aspx.cs" inherits="Enterprise.Tracking.Web.Admin.SetMasterPassword" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Set Master Password</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="contact-lists">
				<h1>Web Tracker Password</h1>
				<edi:ZTextLabel id="SetMasterPasswordHeadingLabel" runat="server" Text="Please enter a new password. This will apply to the following linked user account(s)."></edi:ZTextLabel>
				<div id="ContactsBox" class="table-wrapper" runat="server">
					<table>
						<thead>
						<tr>
							<th>
								Company Code
							</th>
							<th>
								Company Name
							</th>
							<th>
								Email
							</th>
							<th>
								Primary Workplace
							</th>
						</tr>
						</thead>
						<tbody>
						<edi:zrepeater id="LoginContactsRepeater" runat="server" bindto="LoginContacts">
							<HeaderTemplate>
							</HeaderTemplate>
							<ItemTemplate>
								<tr>
									<td>
										<edi:ZTextLabel id="CompanyCode" runat="server" Text='<%# Eval("OrganisationCode") %>'></edi:ZTextLabel> 
									</td>
									<td>
										<edi:ZTextLabel id="CompanyName" runat="server" Text='<%# Eval("WorkingAddressCompanyName") %>'></edi:ZTextLabel> 
									</td>
									<td>
										<edi:ZTextLabel id="Email" runat="server" Text='<%# Eval("OC_Email") %>'></edi:ZTextLabel> 
									</td>
									<td style="text-align: center">
										<edi:ZTextLabel id="PrimaryWorkplace" runat="server" Text='<%# Eval("OC_IsPrimaryContact") %>'></edi:ZTextLabel>
									</td>
								</tr>
							</ItemTemplate>
							<FooterTemplate>
							</FooterTemplate>
						</edi:zrepeater>
						</tbody>
					</table>
				</div>
			</div>
			<br />
			<br />
			<table id="setTable" runat="server">
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
