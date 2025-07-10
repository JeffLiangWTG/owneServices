<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ page language="C#" viewstateencryptionmode='Always' autoeventwireup="true" codebehind="ResetMasterPassword.aspx.cs" inherits="Enterprise.Tracking.Web.Admin.ResetMasterPassword" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Reset Master Password</title>
	<script>
		if (!Element.prototype.matches) Element.prototype.matches = Element.prototype.msMatchesSelector;
		if (!Element.prototype.closest) Element.prototype.closest = function (selector) {
			var el = this;
			while (el) {
				if (el.matches(selector)) {
					return el;
				}
				el = el.parentElement;
			}
		};

		function updateContactListRowSelected() {
			var radios = document.querySelectorAll("#contact-lists input[type=radio]");
			for (var r = 0; r < radios.length; r++) {
				radios[r].checked ? radios[r].closest("tr").classList.add("selected") : radios[r].closest("tr").classList.remove("selected");
			}
		}

		document.addEventListener("DOMContentLoaded", function () {
			var radios = document.querySelectorAll("#contact-lists input[type=radio]");
			for (var r = 0; r < radios.length; r++) {
				radios[r].addEventListener("change", updateContactListRowSelected);
				radios[r].closest("tr").addEventListener("click", (function (radio) {
					return function () {
						var radios = document.querySelectorAll("#contact-lists input[type=radio]");
						for (var r = 0; r < radios.length; r++) {
							radios[r].checked = false;
						}
						radio.checked = true;
						updateContactListRowSelected();
					};
				})(radios[r]));
			}
			updateContactListRowSelected();
		});
	</script>
</head>
<body id="DefaultBody" runat="server">
	<form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="contact-lists">
				<h1>Web Tracker Password</h1>
				<div id="HeadingMessageDiv" runat="server">
					<div>
						<edi:ZTextLabel id="PasswordExpiredMessageLabel" runat="server" Text="Your password has expired due to an enforced password rotation policy. Please enter a new password." Visible="False"></edi:ZTextLabel>
					</div>
					<div>
						<edi:ZTextLabel id="ResetMasterPasswordHeadingLabel" runat="server" Text="Please select the Person Account you would like to set the new password for." Visible="False"></edi:ZTextLabel>
					</div>
				</div>
				<div id="ContactsBox" class="table-wrapper" runat="server">
					<table>
						<thead>
						<tr>
							<th></th>
							<th>
								Person
							</th>
							<th>
								Related Account(s)
							</th>
						</tr>
						</thead>
						<tbody>
						<edi:zrepeater id="LoginContactsRepeater" runat="server" bindto="PersonsForBinding">
							<HeaderTemplate>
							</HeaderTemplate>
							<ItemTemplate>
								<tr>
									<td>
										<edi:ZRadioButton id="Checked" Enabled="true" runat="server" GroupName="LoginCheckBoxes"></edi:ZRadioButton>
									</td>
									<td>
										<edi:ZTextLabel id="Name" runat="server" Text='<%# Eval("Name") %>'></edi:ZTextLabel> 
									</td>
									<td>
										<edi:ZTextLabelNoEncode id="RelatedAccounts" runat="server" Text='<%# Eval("RelatedAccounts") %>'></edi:ZTextLabelNoEncode> 
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
