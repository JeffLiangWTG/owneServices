<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientThirdPartyPartner>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Clients", "Index")%></li>
        <li class="active">Create Partner Account</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Create Integration Account</h2>

<% using (Html.BeginForm()) { %>
<%: Html.ValidationSummary(true, "The eHub Client could not be created") %>
<% if (TempData["Success"] != null)
   { %>
 <p class="alert alert-success" id="successMessage"><%: TempData["Success"] %></p>
<% } %>

<div id="details">
    <fieldset>
        
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Id) %>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Id) %>
            <%: Html.ValidationMessageFor(model => model.Id) %>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Password) %>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Password) %>
            <%: Html.ValidationMessageFor(model => model.Password) %>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.OrgCode) %>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.OrgCode) %>
            <%: Html.ValidationMessageFor(model => model.OrgCode) %>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Email) %>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Email) %>
            <%: Html.ValidationMessageFor(model => model.Email) %>
        </div>
        <div class="clear"></div>
    </fieldset>
</div>

<div id="actionBlock">
    <div class="leftActionButton"><input type="submit" value="Create" class="actionButton"  /></div>
</div>
<% } %>


</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="<%: Url.Content("~/Scripts/jquery.validate.min.js") %>" type="text/javascript"></script>
<script src="<%: Url.Content("~/Scripts/jquery.validate.unobtrusive.min.js") %>" type="text/javascript"></script>
</asp:Content>


