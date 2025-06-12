<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientPIMAView>" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>  
        <li><%: Html.ActionLink("Airline Messaging", "Index", "Service")%></li>
        <li><%: Html.ActionLink(Model.Client.CC_FriendlyName, "Details", "Service", new { id = Model.Client.CC_PK }, null)%></li>
        <li class="active">Manage PIMA</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div id="tabs">
     <ul>
        <li><a href="#prod">Production</a></li>
        <li><a href="#test">Test</a></li>
    </ul>
    <div id="prod">
         <%= Html.Grid(Model.ServiceProviderPIMAView)
            .Columns(column => {
                column.For(a => a.ServiceProviderName).InsertAt(0).Named("Provider").Encode(true);
                column.For(a => a.IATA).InsertAt(1).Named("IATA").Encode(true);
                column.For(a => a.PIMA).InsertAt(2).Named("PIMA").Encode(true);
                column.For(a => a.Password).InsertAt(3).Named("Password").Encode(true);
            })
            .Attributes(@class => "table-list").Empty("No PIMA was specified.")
        %>
        <div id="Div1">
            <div class="leftActionButton"><%: Html.ActionLink("Edit", "Edit", new { id = Model.Client.CC_PK, env = "prod" })%></div>
            <div class="rightActionButton"><%: Html.ActionLink("Back to Action List", "Details", "Service", new { id = Model.Client.CC_PK }, null)%></div>
            <div class="clear"></div>
        </div>
    </div>
    <div id="test">
         <%= Html.Grid(Model.ServiceProviderPIMAViewTest)
            .Columns(column => {
                column.For(a => a.ServiceProviderName).InsertAt(0).Named("Provider").Encode(true);
                column.For(a => a.IATA).InsertAt(1).Named("IATA").Encode(true);
                column.For(a => a.PIMA).InsertAt(2).Named("PIMA").Encode(true);
                column.For(a => a.Password).InsertAt(3).Named("Password").Encode(true);
            })
            .Attributes(@class => "table-list").Empty("No PIMA was specified.")
        %>
        <div id="actionBlock">
            <div class="leftActionButton"><%: Html.ActionLink("Edit", "Edit", new { id = Model.Client.CC_PK, env = "test" })%></div>
            <div class="rightActionButton"><%: Html.ActionLink("Back to Action List", "Details", "Service", new { id = Model.Client.CC_PK }, null)%></div>
            <div class="clear"></div>
        </div>
    </div>
</div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            var env = '<%: Model.Env %>';
            $("#tabs").tabs({ selected: env == 'prod' ? 0 : 1 });
        })
  </script>
</asp:Content>
