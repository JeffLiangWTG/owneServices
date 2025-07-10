<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExchangeRates.aspx.cs" Inherits="Enterprise.Tracking.Web.ExchangeRates" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Customs Exchange Rates</title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
	<meta content="C#" name="CODE_LANGUAGE">
	<meta content="JavaScript" name="vs_defaultClientScript">
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title"><edi:ztextlabel id="TitleLabel" runat="server" CssClass="PageTitle">Customs Exchange Rates</edi:ztextlabel></div>
			<div id="NotFoundError" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="DetailsItem"></edi:ZTextLabel></div>
			<div class="ContentSection" id="OrderContents" runat="server">
				<table class="ResultsTable" id="OrderDetailsTable">
					<tbody>
						<tr>
						    <td>Country:
						    </td>
						    <td colspan="2">
							    <edi:ztextlabel id="CountryName" runat="server" CssClass="SectionTitle" BindTo="CountryName"></edi:ztextlabel>
						    </td>
						</tr>
						<tr>
						    <td>Currency:
						    </td>
						    <td>
							    <edi:ztextlabel id="CurrencyCode" runat="server" CssClass="SectionTitle" BindTo="CurrencyCode"></edi:ztextlabel>
						    </td>
						    <td>
							    <edi:ztextlabel id="CurrencyDescription" runat="server" CssClass="SectionTitle" BindTo="CurrencyDescription"></edi:ztextlabel>
						    </td>
						</tr>
						<tr>
						    <td colspan="3">
						        <div id="FCL" runat="server" class="ContentSection">
							        <edi:ztextlabel id="Ztextlabel1" runat="server" CssClass="SectionTitle">Exchange Rates</edi:ztextlabel>
						            <edi:ZDataGrid ID="ExchangeRatesGrid" runat="server" BindTo="ExchangeRates" CssClass="DetailsTable" 
									    AllowAdd="False" AllowEdit="False" AllowDelete="False" PageSize="3" AutoGenerateColumns="False"
									    AllowPaging="False">
								    </edi:ZDataGrid>
								</div>
						    </td>
						</tr>
						<tr></tr>
					</tbody>
				</table>
           </div>
        </div>
    </form>
</body>
</html>
