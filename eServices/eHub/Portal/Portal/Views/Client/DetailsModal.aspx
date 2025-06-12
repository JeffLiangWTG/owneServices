<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteModal.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientView>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <%Html.RenderPartial("ClientDetails", Model); %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        $("#cboxClose", window.parent.document).attr("itemId", "<%:Model.Client.CC_PK%>");
        $("#cboxClose", window.parent.document).attr("itemName", "<%:Model.Client.DisplayName%>");
    </script>
</asp:Content>

