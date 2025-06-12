<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.eHubTransactions.eHubMessageType>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
     <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Message Types", "Index")%></li>
        <li><%: Html.ActionLink(Model.DT_Code, "Details", new { id = Model.DT_PK }, null)%></li>
        <li class="active">Edit</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm()) {%>
    <%: Html.ValidationSummary(false, "Please correct the errors and try again.") %>    

    <%Html.RenderPartial("MessageTypeEdit", Model); %>

    <div id="actionBlock">
        <div class="leftActionButton"><input type="submit" value="Save" class="actionButton"  /></div>
        <div class="rightActionButton"><%: Html.ActionLink("Cancel", "Details", new { id = Model.DT_PK })%></div>
        <div class="clear"></div>
    </div>

<% } %>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="//ajax.microsoft.com/ajax/jQuery.Validate/1.7/jQuery.Validate.min.js" type="text/javascript"></script>
    <script src="//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.validate.unobtrusive.min.js" type="text/javascript"></script>
</asp:Content>

