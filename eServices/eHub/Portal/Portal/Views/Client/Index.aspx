<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.ClientListContainerViewModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <% Html.RenderPartial("SearchFilters", Model.FilterViewModel); %>
    <p></p>
    <% Html.RenderPartial("SearchResults", Model); %>
    <% Html.RenderPartial("Pager", Model.PagedList); %>
    <% if (false && Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false")) { %>
        <div id="actionAdd"><%=Html.ActionLink("Add", "Create")%></div>
    <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>
