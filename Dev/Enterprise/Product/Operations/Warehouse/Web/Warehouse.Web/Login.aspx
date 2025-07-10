<%@ Page Language="C#" MasterPageFile="~/WarehouseWeb.Master" AutoEventWireup="true" Codebehind="Login.aspx.cs" Inherits="Enterprise.Warehouse.Web.Login" Title="Login" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
		<div id="loginBox">
			<table>
				<tr>
					<td><span>Username:</span></td>
					<td>
						<edi:ZTextBox ID="LoginNameTextBox" runat="server" BindTo="Username" MaxLength="40"></edi:ZTextBox></td>
				</tr>
				<tr>
					<td><span>Password:</span></td>
					<td>
						<edi:ZTextBox ID="PasswordTextBox" TextMode="password" runat="server" BindTo="Password"
							MaxLength="40"></edi:ZTextBox></td>
				</tr>
				<tr>
					<td><span>Warehouse:</span></td>
					<td>
						<edi:ZDropDownList ID="WarehouseDropDown" runat="server" BindTo="Warehouse" BindToList="Warehouses">
						</edi:ZDropDownList></td>
				</tr>
				<tr>
					<td>&nbsp;</td>
					<td>
						<edi:ZCheckBox ID="RememberMeCheckBox" runat="server" CssClass="RememberMeBox" BindTo="RememberMe"
							Text="Remember me"></edi:ZCheckBox></td>
				</tr>
				<tr>
					<td>&nbsp;</td>
					<td>
						<asp:Button ID="LoginButton" runat="server" CssClass="button" Text="Login"></asp:Button></td>
				</tr>
			</table>
			<asp:Label ID="Message" runat="server" CssClass="ErrorMessage" />
		</div>
</asp:Content>
