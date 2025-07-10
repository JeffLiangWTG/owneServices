<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" Codebehind="ContainerBatchUpdate.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.ContainerBatchUpdate" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Update Containers</title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR" />
	<meta content="C#" name="CODE_LANGUAGE" />
	<meta content="JavaScript" name="vs_defaultClientScript" />
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="ContainerBatchLabel" runat="server" CssClass="PageTitle">Update Containers</edi:ZTextLabel></div>
			<div id="UnauthorisedDiv" class="ContentSection" runat="server">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel></div>
			<div id="AuthorisedContent" class="ContentSection" runat="server">
				<div id="NotFoundError" runat="server">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server"></edi:ZTextLabel></div>
				<div id="BatchContents" runat="server">
					<div class="ContentSection">
						<table class="ResultsTable" id="ContainerBatchTable">
						    <tr>
						        <td>
									<edi:ZDataGrid ID="SelectedContainersGrid" runat="server" CssClass="DetailsTable"
										BindTo="Containers" ShowFooter="False" AllowEdit="True" 
										AutoGenerateColumns="False">
										<PagerStyle Mode="NumericPages"></PagerStyle>
										<ItemStyle CssClass="DetailsCell"></ItemStyle>
										<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
									</edi:ZDataGrid>
						        </td>
						    </tr>
						</table>
					</div>
					<div class="ContentSection">
					    <br />
					    <edi:ZTextLabel ID="ChangingDatesLabel" runat="server" CssClass="SectionTitle">Change Dates</edi:ZTextLabel>
					</div>
					<div class="ContentSection">
						<table class="ResultsTable" id="Table1">
						    <tr>
						        <td>Required Delivery</td>
						        <td nowrap="nowrap"><edi:ZDateEdit ID="RequiredDelivery" runat="server" BindTo="RequiredDeliveryDate" DateTimeFormat="Long" /></td>
						        <td>Confirmed Delivery</td>
						        <td nowrap="nowrap"><edi:ZDateEdit ID="ConfirmedDelivery" runat="server" BindTo="ConfirmedDeliveryDate" DateTimeFormat="Long" /></td>
						        <td>Actual Delivery</td>
						        <td nowrap="nowrap"><edi:ZDateEdit ID="ActualDelivery" runat="server" BindTo="ActualDeliveryDate" DateTimeFormat="Long" /></td>
						    </tr>
						    <tr>
						        <td>Estimated Dehire</td>
						        <td nowrap="nowrap"><edi:ZDateEdit ID="EstimatedDehire" runat="server" BindTo="EstimatedDehireDate" DateTimeFormat="Long" /></td>
						        <td>Pickup</td>
						        <td nowrap="nowrap"><edi:ZDateEdit ID="Pickup" runat="server" BindTo="EmptyPickup" DateTimeFormat="Long" /></td>
						        <td>Actual Dehire</td>
						        <td nowrap="nowrap"><edi:ZDateEdit ID="ActualDehire" runat="server" BindTo="ActualDehireDate" DateTimeFormat="Long" /></td>
						    </tr>
						    <tr>
						        <td colspan="5"></td>
						        <td>
						            <edi:ZButton ID="Apply" runat="server" Text="Apply to All" OnClick="Apply_Click" />
						        </td>
						    </tr>
						</table>
					</div>
					<div class="ContentSection">
						<asp:Button ID="SaveChanges" runat="server" Text="Save Changes" onclick="SaveChanges_Click"></asp:Button>
						&nbsp;
						<asp:Button ID="CancelChanges" runat="server" Text="Cancel Changes" onclick="CancelChanges_Click"></asp:Button>
					</div>
				</div>
			</div>
		</div>
	</form>
</body>
</html>
