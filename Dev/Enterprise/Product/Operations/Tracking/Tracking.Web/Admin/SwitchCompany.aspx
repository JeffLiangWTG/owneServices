<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SwitchCompany.aspx.cs" Inherits="Enterprise.Tracking.Web.Admin.SwitchCompany" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Switch Company</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="form1" runat="server">
	   <div id="OuterContentPane" runat="server">
			<edi:ZTextLabel ID="SwitchCompanyHeadingLabel" runat="server" CssClass="PageTitle">Switch Current Company</edi:ZTextLabel>
			<div class="ContentSection">
			    This page lets you quickly switch between your related companies.<br>
			    You are currently logged in as <edi:ZTextLabel ID="CurrentCompanyLabel" runat="server" CssClass="DetailsItem" BindTo="SiteUser.LoggedInOrganisation.OH_FullName"></edi:ZTextLabel>.<br>
			    You can switch to one of your related companies by clicking on one of the links below.
			    <p></p>
			    <edi:zdatagrid id="CompaniesDataGrid" runat="server" CssClass="DetailsTable" AutoGenerateColumns="False"
						    BindTo="SiteUser.AllUserRelatedOrgs">
				    <PagerStyle Mode="NumericPages"></PagerStyle>
				    <ItemStyle CssClass="DetailsCell"></ItemStyle>
				    <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
   			    </edi:zdatagrid>
   			</div>
   			<p></p>
         </div>
     </form>
</body>
</html>