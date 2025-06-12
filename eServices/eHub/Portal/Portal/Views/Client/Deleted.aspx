<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li>
            <a href="/">
                <i class="glyphicon glyphicon-home"></i>
            </a>
        </li>       
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>     
        <li><%: Html.ActionLink("Clients", "Index")%></li>
        <li class="active">Deleted</li>
    </ol>
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div>
    <p>Your Client was successfully deleted.</p>
</div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
