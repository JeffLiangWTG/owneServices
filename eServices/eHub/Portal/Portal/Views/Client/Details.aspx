<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientView>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">   
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Clients", "Index")%></li>
        <li class="active"><%:Model.Client.CC_FriendlyName %></li>
    </ol>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <%Html.RenderPartial("ClientDetails", Model); %>
    <div id="actionBlock">
        <div class="leftActionButton">
            <% if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false"))
               { %>
            <%: Html.ActionLink("Edit", "Edit", new { id = Model.Client.CC_PK })%>
            <%: Html.ActionLink("Delete", "Delete", new { id = Model.Client.CC_PK })%>
            <% } %>
            <% if (Model.Client.CC_OwnerCategory != "Service Provider" && Model.Client.CC_OwnerCategory != "Service")
               { %>
            <%: Ajax.ActionLink("Services", "Index", "ServiceProvider", new { Provider = Model.Client.CC_ID }, new AjaxOptions { HttpMethod = "Get", Url = Url.Action("GetProvidersWithServices") })%>
            <% } %>
        </div>
        <div class="rightActionButton">
            <%: Html.ActionLink("Back to List", "Index")%>
        </div>
        <div class="clear">
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
