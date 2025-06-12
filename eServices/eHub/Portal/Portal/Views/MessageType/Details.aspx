<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.eHubTransactions.eHubMessageType>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Message Types", "Index")%></li>
        <li class="active"><%:Model.DT_Code %></li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%Html.RenderPartial("MessageTypeDetails", Model); %>

    <div id="Div1">
        <% if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false")) { %>
            <div class="leftActionButton">
            <%: Html.ActionLink("Edit", "Edit", new {id=Model.DT_PK}) %>
            </div>
        <% } %>
        <div class="rightActionButton">
        <%: Html.ActionLink("Back to List", "Index")%>
        </div>
        <div class="clear"></div>
    </div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>

