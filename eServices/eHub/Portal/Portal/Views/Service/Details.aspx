<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ServiceView>" %>
<%@ Import Namespace="CargoWise.eHub.Portal" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
     <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>  
        <li><%: Html.ActionLink("Airline Messaging", "Index", "Service")%></li>
        <li class="active"><%: Model.Client.CC_FriendlyName%></li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h3>Actions:</h3>
    <% if (!Model.AirServiceActive){ %>
        <p><%:Model.Client.CC_ID%> is not currently configured for Airline Messaging</p>
    <% } %>
<div id="serviceAction">
    <div class="serviceAction"><%=Html.ActionLink("Manage PIMA", "Index", "ClientPIMA", new { id = Model.Client.CC_PK }, null)%></div>
    <div class="serviceAction"><%=Html.ActionLink("Manage Message Routing", "Index", "MessageRouting", new { id = Model.Client.CC_PK }, null)%></div>
</div>

<div id="actionBlock">
    <div class="leftActionButton">
        <% if (Model.AirServiceActive){ %>
            <%: Html.ActionLink("Delete Service", "Delete", new { id = Model.Client.CC_PK })%>
        <% } else { %>
            <%:Model.Client.CC_ID%> is not currently configured for Airline Messaging
        <% } %>
    </div>
    <div class="rightActionButton"><%: Html.ActionLink("Back to client selection", "Index", new { id = Model.Client.CC_PK })%></div>
    <div class="clear"></div>
</div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
