<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteModal.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.eHubTransactions.eHubMessageType>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%Html.RenderPartial("MessageTypeDetails", Model); %>
    <% if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false")) { %>
        <div  id="actionBlock">
            <%: Html.ActionLink("Edit", "EditModal", new {id=Model.DT_PK}) %>
        </div>
    <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>

