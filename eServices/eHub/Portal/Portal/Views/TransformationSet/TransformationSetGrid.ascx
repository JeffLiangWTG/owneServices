<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<CargoWise.eHub.Portal.Models.eHubTransactions.eHubTransformationSet>>"%>

<%if(ViewData.Model != null) 
  {
    foreach(var item in ViewData.Model) 
    {
        var name = item.TS_Name as string;%>
        <div class="item">
        <%=Html.ActionLink(name == "" ? "No Name" : name , "Details", new { id = item.TS_PK })%>
        </div>
  <%}%>
<%}%>   
