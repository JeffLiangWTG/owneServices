<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.MessageTypeListContainerViewModel>" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>
<%@ Import Namespace="CargoWise.eHub.Portal.Models.eHubTransactions"%>
<%= Html.Grid(Model.PagedList)
    .Columns(column => {
        column.For(a => Html.ActionLink(a.DT_Code, "Details", new { id = a.DT_PK })).InsertAt(0).Named("Code").Encode(false);
        column.For(a => a.DT_IsEDI.Equals(true) ? "EDI" : (a.DT_IsFlatFile.Equals(true) ? "FlatFile" : "XML")).InsertAt(1).Named("Format").Encode(true);
    })
    .Attributes(@class => "table-list")
 %>
