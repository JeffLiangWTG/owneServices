<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientEditView>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Airline Messaging", "Index", "Service")%></li>
        <li><%: Html.ActionLink(Model.Client.CC_FriendlyName, "Details", "Service", new { id = Model.Client.CC_PK }, null)%></li>
        <li><%: Html.ActionLink("Manage Message Routing", "Index", new { id = Model.Client.CC_PK })%></li>
        <li class="active">Edit SP for Clients</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm())
   {%>

    <%:Html.ValidationSummary(false, "Please correct the errors and try again.")%>    

    <div class="blockData">
        Default SP for the client  &nbsp;

        <%:Html.DropDownListFor(model => model.Client.CC_AirServiceProvider, new SelectList(Model.AirServiceProvider, "CC_PK", "CC_ID"), "None")%>
        <%:Html.ValidationMessageFor(model => model.Client.CC_AirServiceProvider)%>
    </div>

     <div id="actionBlock">
        <div class="leftActionButton"><input type="submit" value="Save" class="actionButton"  /></div>
        <div class="rightActionButton">
                <%:Html.ActionLink("Cancel", "Index", "MessageRouting", new { id = Model.Client.CC_PK }, null)%>
        </div>
        <div class="clear"></div>
    </div>

<% } %>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
