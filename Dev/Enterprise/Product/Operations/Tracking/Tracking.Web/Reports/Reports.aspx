<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="Enterprise.Tracking.Web.Reports" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html >
<head>
    <title>Reports</title>
</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
					<edi:ztextlabel id="ReportsLabel" runat="server" CssClass="PageTitle">Reports</edi:ztextlabel><br />
					<br />
				</div>
				<div id="AuthorisedContent" runat="server" class="ContentSection">
					<table id="Table1" runat="server">
					    <tr>
					        <td class="ReportFilterGroupTitle">Select a Report</td>
					    </tr>
						<tr>
							<td>
								<edi:ZGuidDropDownList ID="ReportsDropDownList"
													   runat="server" BindTo="ReportPK"
													   BindToList="WebReports"
													   DataTextField="SU_MenuNameMultilingual" 
													   DataValueField="PK" 
													   AutoPostBack="True"
													   OnSelectedIndexChanged="ReportsDropDownList_SelectedIndexChanged"
													   ShowEmptyItem="True">
								</edi:ZGuidDropDownList>
								<br />
								<div id="FilterControlHolder" runat="server"></div>
							</td>
						</tr>
					</table>
				</div>
				<div id="UnauthorisedDiv" runat="server">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>&nbsp;
				</div>
			</div>
		</form>
	</body>
</html>

