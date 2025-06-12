<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<style type="text/css">
		#Password1 {
			width: 215px;
		}

		#Password2 {
			width: 215px;
		}

		.auto-style1 {
			width: 177px;
		}
	</style>
</head>
<body>
	<form id="form1" runat="server">
		<table style="width: 100%;">
			<tr>
				<td class="auto-style1">Username:</td>
				<td>
					<asp:TextBox ID="txtUsername" runat="server" Width="215px">HYETSTTST</asp:TextBox>
				</td>
			</tr>
			<tr>
				<td class="auto-style1">Password:</td>
				<td>
					<asp:TextBox ID="txtPassword" runat="server" Width="215px">test</asp:TextBox>
				</td>
			</tr>
			<tr>
				<td class="auto-style1">Test Method:</td>
				<td>
					<asp:RadioButtonList ID="TestMethod" runat="server">
						<asp:ListItem Selected="True">Evaluate</asp:ListItem>
						<asp:ListItem>EvaluateOCM</asp:ListItem>
					</asp:RadioButtonList>
				</td>
			</tr>
			<tr>
				<td class="auto-style1">Test Sender:</td>
				<td>
					<asp:TextBox ID="txtSender" runat="server" Width="215px"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td class="auto-style1">RuleId:</td>
				<td>
					<asp:TextBox ID="txtRuleId" runat="server" Width="215px"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td class="auto-style1">Test Message:</td>
				<td>
					<asp:FileUpload ID="fleMessage" runat="server" Width="777px" />
				</td>
			</tr>
		</table>
		<asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" />
		<br />
		<asp:Label ID="lblResult" runat="server"></asp:Label>
	</form>
</body>
</html>
