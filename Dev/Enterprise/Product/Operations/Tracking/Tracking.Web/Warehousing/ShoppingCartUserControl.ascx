<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ShoppingCartUserControl.ascx.cs" Inherits="Enterprise.Tracking.Web.ShoppingCartUserControl" %>
<DIV id="Title">
	<edi:ZTextLabel id="TitleLabel" runat="server" CssClass="DetailsItem"> Allocated Order Lines</edi:ZTextLabel>
</DIV>
<div align="left" class="ContentSection">
	<asp:button id="NewButton" class="Button NewButton" runat="server"></asp:button>
	&nbsp;
	<asp:button id="ClearButton" class="Button ClearButton" runat="server"></asp:button>
	&nbsp;
</div>
<DIV runat="server" id="OrderLinesGridDiv" class="ContentSection Scrollable">
	<edi:ZDataGrid id="OrderLinesGrid" runat="server" />
</DIV>
