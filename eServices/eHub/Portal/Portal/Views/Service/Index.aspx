<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ServiceView>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="filterSection">
        <%
            using (Html.BeginForm("Index", "Service", FormMethod.Get))
            {%>
                <div class="labelText">Select client</div>
                <div class="textBox">
                <%if (Model.Client != null)
                {%>
                     <%=Html.TextBox("client", Model.Client.CC_ID)%>
                <%}
                else
                {%>
                     <%=Html.TextBox("client")%><%
                }%>
                </div>
                <div class="clear"></div>
            <%}%>
    </div>

    <%if (Model.Client != null) {%>
        <div id="gridSection">
            <%=Html.ActionLink("Details", "Details", new { id = Model.Client.CC_PK })%>
        </div>
    <% } %>
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%:Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <script src="<%:Url.Content("~/PlugIns/Watermark/jquery.watermark.min.js")%>" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

<!-- Watermark -->
    <script type="text/javascript">
       $(function () {
           $("#client").watermark("Type a letter");
           $("#clientFocus").click(
			function () {
			    $("#client")[0].focus();
			}
		);
       });
   </script>

 <!-- autocomplete block -->
    <script type="text/javascript">
        $("#client").autocomplete
         ({
             source: function (request, response) {
                 $.ajax({ url: '<%:Url.Action("Find", "Client")%>/' + $("#client").val(),
                     type: "POST", dataType: "json",
                     success: function (data) {
                         response($.map(data, function (item) { return { label: item.Name + " (" + item.Code + ")", value: item.Id }; }));
                     }
                 })
             },
             minLength: 1,
             select: function (event, ui) {
                 $("#client").val(ui.item.label);

                 if (ui.item.value != "")
                     document.location = '<%:Url.Action("Details", "Service")%>/' + ui.item.value;
                 return false;
             }
         });

    </script> 
   
</asp:Content>

