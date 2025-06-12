<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">

    <link href="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.css")%>" rel="Stylesheet" type="text/css" />
    <link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <link href="<%: Url.Content("~/PlugIns/jqGrid460/ui.jqgrid.css")%>" rel="Stylesheet" type="text/css" media="screen" />

    <script src="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.min.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/jqGrid460/grid.locale-en.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/jqGrid460/jquery.jqGrid.min.js")%>" type="text/javascript"></script>

    <script src="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/JSON/json2.js")%>" type="text/javascript"></script>

</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Client Authorisation</h2>
	
    <div id="errorMessage" class="error" style="display: none"></div>
    <div>
        <div id="authorisationFields">
            <table id="clientAuthorisationTable"></table>
            <div id="clientAuthorisationTablePager"></div>
        </div>
    </div>

    <div id="selectClientDialog" title="Select Client" style="display: none">
        <div id="clientsFields">
            <table id="clientsTable"></table>
            <div id="clientsTablePager"></div>
        </div>
        <form action="">
            <input type="hidden" name="clientID" id="clientID" />
        </form>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

        $(document).ready(function () {

            $('#errorMessage').ajaxError(function (event, request, settings, exception) {
                $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
                $(this).show();
            }).ajaxComplete(function () {
                $(this).hide()
            });

            function ReloadClientAuthorisations() {
                $('#clientAuthorisationTable').trigger("reloadGrid");
            }

            $("#clientAuthorisationTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("GetClientAuthorisation", "ClientAuthorisation") %>',
                editurl: '<%: Url.Action("ClientAuthorisationEdit","ClientAuthorisation") %>',
                jsonReader: { root: 'eHubClientAuthorisations', id: 'id', repeatitems: false },
                colNames: [ 'SenderID', 'RecepientID'],
                colModel: [
                    {
                        name: 'CA_CC_Sender',
                        index: 'CA_CC_Sender',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientIdValue').val();
                            }
                        }
                    },
                    {
                        name: 'CA_CC_Recipient', index: 'CA_CC_Recipient', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientIdValue').val();
                            }
                        }
                    },
                ],
                height: '500',
                width: '400',
                rowNum: 20,
                rowList: [20, 50, 100],
                caption: "Client Authorisation",
                sortable: true,
                sortorder: 'asc',
                pager: '#clientAuthorisationTablePager'
            });
            $("#clientAuthorisationTable").jqGrid('navGrid', '#clientAuthorisationTablePager', {},
                {
                    width: 400,
                    closeAfterEdit: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    viewPagerButtons: true,
                    resize: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                {
                    width: 400,
                    closeAfterAdd: false,
                    closeOnEscape: true,
                    recreateForm: true,
                    resize: true,
                    afterSubmit: function (response, postdata) {
                        ReloadClientAuthorisations();
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                {
                    closeOnEscape: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                }
            );

            function FormatClientEditFields(value, options) {
                var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
                    '" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>' +
                    ' <a class="fm-button ui-state-default ui-corner-all clientIdSelect">Select</a>';
                var elem = $(elemStr)[0];
                $(elem).children('.clientIdSelect').click(function () {
                    DisplaySelectClientDialog($(elem).children('.clientIdValue'));
                });
                return elem;
            }
            function DisplaySelectClientDialog(elem) {
                $("#clientsTable").jqGrid({
                    datatype: 'json',
                    url: '<%: Url.Action("Clients", "ClientAuthorisation") %>',
                    jsonReader: { root: 'eHubClients', id: 'CC_ID', repeatitems: false },
                    colNames: ['Client ID', 'Name'],
                    colModel: [
                        { name: 'CC_ID', width: 100, classes: "wrapped", sortable: true, search: true },
                        { name: 'CC_FriendlyName', width: 300, classes: "wrapped", sortable: true, search: true }
                    ],
                    onSelectRow: function (id) {
                        $('#selectClientDialog #clientID').val(id)
                    },
                    height: '400',
                    width: '400',
                    rowNum: 15,
                    rowList: [15, 30, 50],
                    sortname: 'CC_ID',
                    sortorder: 'asc',
                    pager: '#clientsTablePager'
                });
                $("#clientsTable").jqGrid('navGrid', '#clientsTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
                $("#clientsTable").jqGrid('filterToolbar', { searchOnEnter: false });
                $("#selectClientDialog").dialog({
                    title: 'Select Client',
                    width: "auto",
                    height: "auto",
                    resizable: false,
                    modal: true,
                    buttons: {
                        "Select": function () {
                            var selectID = $('#selectClientDialog #clientID').val();
                            if (selectID == "") {
                                DisplayError("Please select a client.");
                            }
                            else {
                                $(elem).val(selectID);
                                $(this).dialog("close");
                            }
                        },
                        "Cancel": function () {
                            $(this).dialog("close");
                        }
                    }
                });
            }

            function DisplayError(errorMsg) {
                $('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
            }
        });

	</script>


</asp:Content>


