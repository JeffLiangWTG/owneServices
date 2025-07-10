<%@ Page Language="C#" MasterPageFile="~/WarehouseWeb.Master" AutoEventWireup="true" Codebehind="Error.aspx.cs" Inherits="Enterprise.Warehouse.Web.Error" Title="Error" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div id="Title">
		<edi:ztextlabel id="PageTitle" runat="server" cssclass="PageTitle">Application Error</edi:ztextlabel>
	</div>
	<div>
		<asp:Literal ID="MessageDescription" runat="server" Text="A problem has been encountered with the Web Application. Sorry for any inconvenience this may have caused."></asp:Literal>
	</div>
	<div>
		<asp:HyperLink ID="HomeLink" runat="server" />
	</div>
</asp:Content>
