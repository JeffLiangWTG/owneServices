<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" Codebehind="ContainerSummary.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.ContainerSummary" %>
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
				&nbsp;</div>
			<div id="UnauthorisedDiv" class="ContentSection" runat="server">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel></div>
			<div id="AuthorisedContent" class="ContentSection" runat="server">
				<div id="BatchContents" runat="server">
					<div class="ContentSection">
						<table class="ResultsTable" id="ContainerBatchTable">
						    <tr>
						        <td>
									<edi:ZDataGrid ID="SelectedContainersGrid" runat="server" CssClass="DetailsTable"
										BindTo="SummaryLines" ShowFooter="False"
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
						&nbsp;</div>
				</div>
			</div>
		</div>
	</form>
</body>
</html>
