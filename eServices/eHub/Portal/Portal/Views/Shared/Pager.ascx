<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<MvcContrib.Pagination.IPagination>" %>
<%@ Import Namespace="MvcContrib.UI.Pager" %>
<p/>
  <%= Html.Pager(Model).First("First").Last("Last").Next("Next").Previous("Previous") %>
<p/>
