<%@ page language="c#" codebehind="CFSShipments.aspx.cs" autoeventwireup="True" inherits="Enterprise.Tracking.Web.CFSShipments" %>

<%@ register tagprefix="edi" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>CFS Shipments</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ztextlabel id="PageTitleLabel" runat="server" cssclass="PageTitle">CFS Shipments</edi:ztextlabel>
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <div id="SearchControlHolder" runat="server"></div>

            </div>
        </div>
    </form>
</body>
</html>
