<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<CargoWise.eHub.Portal.Models.eHubTransactions.eHubTransformationSet>>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="filterSection">
        <%Html.RenderPartial("TransformationFilter"); %>
    </div>

    <div id="gridSection">
        <%Html.RenderPartial("TransformationSetGrid", ViewData.Model); %>
    </div>
    <% if (false && Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false")) { %>
        <div id="actionAdd"><%=Html.ActionLink("Add Transformation Set", "Create")%></div>
    <% } %>
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <script src="<%:Url.Content("~/PlugIns/Watermark/jquery.watermark.min.js")%>" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
   
       <!-- Populate WebGrid -->
    <script type="text/javascript">
        function PopulateGrid(str) {
            if (str == "") {
                $("#gridSection").html("");
            }
            else {
                $.ajax({ url: '<%:Url.Action("Index")%>/' + str, type: "POST", dataType: "html",

                    success: function (html) {
                        $("#gridSection").html(html);
                    }
                });
            }
        }
    </script>


</asp:Content>
    