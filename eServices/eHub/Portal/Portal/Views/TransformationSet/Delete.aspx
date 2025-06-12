<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.eHubTransactions.eHubTransformationSet>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li> 
        <li><%: Html.ActionLink("Transformation Sets", "Index", "TransformationSet")%></li>   
        <li><%: Html.ActionLink(Model.TS_Name, "Details", new { id = Model.TS_PK})%></li>
        <li class="active">Delete</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div>
    <%: Html.ValidationSummary(false, "Please correct the errors and try again.") %>    
    </div>
    <div id="deleteMessage">
        Please confirm you want to delete the Transformation Set named: <i> <%:Model.TS_Name%> </i>
    </div>
    
    <% using (Html.BeginForm()) {%>
        <div id="actionBlock">
            <div class="leftActionButton"><input class="actionButton" name="confirmButton" type="submit" value="Delete" />        </div>
            <div class="rightActionButton"><%: Html.ActionLink("Cancel", "Details", new {id=Model.TS_PK}) %></div>
            <div class="clear"></div>
        </div>

    <% } %>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
