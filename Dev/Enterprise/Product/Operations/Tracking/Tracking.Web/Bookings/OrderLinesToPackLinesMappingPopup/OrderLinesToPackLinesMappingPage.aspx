<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OrderLinesToPackLinesMappingPage.aspx.cs" Inherits="Enterprise.Tracking.Web.OrderLinesToPackLinesMappingPage" %>
<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls"	TagPrefix="edi" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Order Lines to Pack Lines Mapping</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
	<div id="OuterContentPane" runat="server">
		<div id="Title">
			<edi:ZTextLabel ID="OrderLinesLabel" runat="server" CssClass="PageTitle">Order Lines to Pack Lines Mapping</edi:ZTextLabel></div>
		<div id="UnauthorisedDiv" class="ContentSection" runat="server">
			<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel></div>
		<div id="AuthorisedContent" runat="server">
			<asp:UpdatePanel id="up" runat="Server">
				<ContentTemplate>
					<div id="NotFoundError" runat="server">
						<edi:ZTextLabel ID="NotFoundLabel" runat="server"></edi:ZTextLabel></div>
					<div id="LineContents" runat="server">
						<div class="ContentSection" runat="server">
							<edi:ZDataGrid ID="OrderLinesGrid" runat="server" CssClass="DetailsTable" BindTo="OrderLines" ShowFooter="False" AutoGenerateColumns="False" AllowMultiLineSelection="true" AllowEdit="True" DisplayAdditionalNewRow="false">
								<PagerStyle Mode="NumericPages"></PagerStyle>
								<ItemStyle CssClass="DetailsCell"></ItemStyle>
								<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
								<SelectedItemStyle CssClass="DetailsSelectedCell" />
							</edi:ZDataGrid>
						</div>
						<div class="ContentSection">
							<asp:Button ID="Undo" runat="server" Text="Undo" onclick="Undo_Click"></asp:Button>
							&nbsp;
							<asp:Button ID="Merge" runat="server" Text="Merge" onclick="Merge_Click"></asp:Button>
							&nbsp;
							<asp:Button ID="Finish" runat="server" Text="Finish" onclick="Finish_Click"></asp:Button>
							&nbsp;
							<asp:Button ID="Cancel" runat="server" Text="Cancel" onclick="Cancel_Click"></asp:Button>
						</div>
						<div class="ContentSection">
							<edi:ZDataGrid ID="DummyPackLinesGrid" runat="server" CssClass="DetailsTable" BindTo="DummyPackLines" ShowFooter="False" AutoGenerateColumns="False" DisableCollapsing="true" AllowMultiLineSelection="true" DisplayAdditionalNewRow="false">
								<PagerStyle Mode="NumericPages"></PagerStyle>
								<ItemStyle CssClass="DetailsCell"></ItemStyle>
								<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
								<SelectedItemStyle CssClass="DetailsSelectedCell" />
							</edi:ZDataGrid>
						</div>
					</div>
				</ContentTemplate>
			</asp:UpdatePanel>
		</div>
	</div>
	</form>
</body>
</html>
