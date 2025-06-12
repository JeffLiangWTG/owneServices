<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteModal.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientView>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm()) {%>
    <%: Html.ValidationSummary(false, "Please correct the errors and try again.") %>    

    <%Html.RenderPartial("ClientEdit", Model); %>

   <div id="actionBlock">
        <div class="leftActionButton"><input type="submit" value="Save" class="actionButton"  /></div>
        <div class="clear"></div>
    </div>

<% } %>



</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="//ajax.microsoft.com/ajax/jQuery.Validate/1.7/jQuery.Validate.min.js" type="text/javascript"></script>
    <script src="//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.validate.unobtrusive.min.js" type="text/javascript"></script>

    <script type="text/javascript">
            $("#cboxClose", window.parent.document).attr("itemId", "");
            $("#cboxClose", window.parent.document).attr("itemName", "");
    </script>

</asp:Content>

