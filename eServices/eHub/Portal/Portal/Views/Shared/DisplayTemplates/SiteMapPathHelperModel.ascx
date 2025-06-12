<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl`1[[MvcSiteMapProvider.Web.Html.Models.SiteMapPathHelperModel,MvcSiteMapProvider]]" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="MvcSiteMapProvider.Web.Html.Models" %>

<% foreach (var node in Model) { %>
    <li <% if (node == Model.Last()) { %> class='active' <% } %>><%=Html.DisplayFor(m => node)%></li>
<% } %>