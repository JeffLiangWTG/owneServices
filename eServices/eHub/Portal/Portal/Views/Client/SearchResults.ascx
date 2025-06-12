<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.ClientListContainerViewModel>" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>
<%@ Import Namespace="CargoWise.eHub.Portal.Models.eHubTransactions"%>
<%= Html.Grid(Model.PagedList)
    .Columns(column => {
        column.For(a => Html.ActionLink(a.CC_ID, "Details", new { id = a.CC_PK })).InsertAt(0).Named("eHub ID").Encode(false);
        column.For(a => a.CC_FriendlyName).InsertAt(1).Named("Name").Encode(true);
        column.For(a => a.CC_AirlineCode).InsertAt(2).Named("ACode").Encode(true);
        column.For(a => a.CC_AirlinePrefix).InsertAt(3).Named("APrefix").Encode(true);
    })
    .Attributes(@class => "table-list")
 %>
