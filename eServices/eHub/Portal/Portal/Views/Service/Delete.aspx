<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ServiceView>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Airline Messaging", "Index", "Service")%></li>
        <li><%: Html.ActionLink(Model.Client.CC_FriendlyName, "Details", new { id = Model.Client.CC_PK })%></li>
        <li class="active">Delete</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div>
        Please confirm you want to delete the Airline Messaging configuration for <%:Model.Client.CC_ID%>
    </div>
    
    <% using (Html.BeginForm()) {%>
        <%: Html.ValidationSummary(false, "Please correct the errors and try again.") %>    
        <div id="actionBlock">
            <div class="leftActionButton"><input class="actionButton" name="confirmButton" type="submit" value="Delete" /></div>
            <div class="rightActionButton"><%: Html.ActionLink("Cancel", "Details", new { id = Model.Client.CC_PK })%></div>
            <div class="clear"></div>
        </div>
    <% } %>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
