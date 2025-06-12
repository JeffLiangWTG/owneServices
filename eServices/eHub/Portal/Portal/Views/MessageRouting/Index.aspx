<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.MessageRoutingView>" %>

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
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>
        <li><%: Html.ActionLink("Airline Messaging", "Index", "Service")%></li>
        <li><%: Html.ActionLink(Model.Client.CC_FriendlyName, "Details", "Service", new { id = Model.Client.CC_PK }, null)%></li>
        <li class="active">Manage Message Routing</li>
    </ol>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h3>Client SP</h3>
    <div class="routingBlock">
        <%if (Model.ClientProvider.ServiceProviderName != null)
          {%>
        Send all messages using  <b><%: Model.ClientProvider.ServiceProviderName%></b>.
        <% } else {%>
        There is no default SP for client.
        <% } %>
    </div>
    <div class="actionBlock">
        <%: Html.ActionLink("Edit", "EditClientProvider", new { id = Model.Client.CC_PK })%>
    </div>

    <h3>Message Type SP</h3>
    <div class="routingBlock">
        <div id="errorMessage" class="error" style="display: none"></div>

        <div id="messageTypeProvidersFields">
            <table id="messageTypeProvidersTable"></table>
            <div id="messageTypeProvidersTablePager"></div>
        </div>
    </div>

    <h3>Airline SP</h3>
    <div>
        <table id="airlineServiceProvidersMappingTable"></table>
        <div id="airlineServiceProvidersMappingTablePager"></div>
    </div>
    <div id="airlineServiceProvidersMappingExportCsvPart" style="display:block">
        <br />
        <table>
            <tr>
                <td valign="top" style="border-right:1px solid #999; padding-right:5px">
                    <form id="airlineServiceProvidersMappingExportCsvForm" action="<%: Url.Action("AirServiceProviderMappingExportCsv") %>">
                        <input type="hidden" id="selectedClientPK" name="selectedClientPK" value="" />
					    <input type="submit" id="airlineServiceProvidersMappingExportCsv" value="Export to CSV" />
                    </form>
                </td>
                <td style="padding-left:5px">
						<form id="airlineServiceProvidersMappingImportCsvForm" method="post" action="<%: Url.Action("AirServiceProviderMappingImportCsv") %>" enctype="multipart/form-data" target="upload_target" >
                    
                        <input type="hidden" id="selectedClientPKForImport" name="selectedClientPKForImport" value="" />
							<input type="submit" name="airlineServiceProvidersMappingImportCsv" id="airlineServiceProvidersMappingImportCsv" value="Import from CSV" />
							<select name="option" id="registrationsInportOption" class="ui-state-disabled ui-corner-all ui-widget" >
								<option value="replace">Replace Only</option>
							</select>
							<br />
							<input type="file" name="uploadFile" id="uploadFile" class="ui-widget ui-state-default ui-corner-all ui-button-text" size="75" />
							<iframe id="upload_target" name="upload_target" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>
						</form>
					</td>
            </tr>
        </table>
        <div id="dialog" title="Import CSV" style="width:800px;height:400px;">
              <p>Import CSV data successed. Please refresh webpage to see the latest data.</p>
        </div>
    </div>
    <div id="actionBlock">
        <div class="leftActionButton"><%: Html.ActionLink("Back to Action List", "Details", "Service", new { id = Model.Client.CC_PK }, null)%></div>
        <div class="clear"></div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

    <script type="text/javascript">

        $(document).ready(function () {

            var clientID = window.location.pathname.split('/').pop();

            $("#airlineServiceProvidersMappingExportCsv").button();
            $("#airlineServiceProvidersMappingImportCsv").button();
            var specialServiceProviderNames = getSpecialServiceProviderNames().split(";");



            $('#errorMessage').ajaxError(function (event, request, settings, exception) {
                $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
                $(this).show();
            }).ajaxComplete(function () {
                $(this).hide()
            });

            $("#messageTypeProvidersTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("MessageTypeProviders") %>' + '?clientID=' + clientID,
                editurl: '<%: Url.Action("MessageTypeProviderEdit") %>' + '?clientID=' + clientID,
                jsonReader: { root: 'messageTypeProviders', id: 'rowId', repeatitems: true },
                colNames: ['rowId', 'Message Type', 'Service Provider'],
                colModel: [
                    {
                        name: 'rowId',
                        hidden: true
                    },
                    {
                        name: 'MessageType',
                        index: 'MessageType',
                        classes: "wrapped",
                        width: 100,
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: "select",
                        editoptions: { value: getMessageTypes(), style: "width:100px" },
                        editrules: { required: true, edithidden: true },
                        sortable: true
                    },
                    {
                        name: 'ServiceProvider',
                        index: 'ServiceProvider',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: "select",
                        editoptions: { value: getServiceProviders(), style: "width:100px" },
                        editrules: { required: true, edithidden: true },
                        sortable: true
                    },
                ],
                height: '222',
                width: '400',
                rowNum: 10,
                rowList: [10, 20, 50],
                caption: "Message Type Providers",
                sortable: true,
                sortname: 'MessageType',
                sortorder: 'asc',
                pager: '#messageTypeProvidersTablePager'
            });

            $("#messageTypeProvidersTable").jqGrid('navGrid', '#messageTypeProvidersTablePager', {},
                { // edit option
                    width: 300,
                    closeAfterEdit: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    viewPagerButtons: false,
                    resize: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                { // add option
                    width: 300,
                    closeAfterAdd: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    resize: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                { // del option
                    closeOnEscape: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
            );

            $("#airlineServiceProvidersMappingTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("AirlineServiceProviderMapping") %>' + '?clientID=' + clientID,
                editurl: '<%: Url.Action("AirlineServiceProviderMappingEdit") %>' + '?clientID=' + clientID,
                jsonReader: { root: 'airlineServiceProvidersMapping', id: 'RowId', repeatitems: true },
                colNames: [
                    'RowId',
                    'Message Type',
                    'Airline',
                    'Service Provider',
                    'Recipient Address',
                    'Client PMA',
                    'Message Priority',
                    'Double Signature Code',
                    'Shipment Origin'],
                colModel: [
                    {
                        name: 'RowId',
                        hidden: true
                    },
                    {
                        name: 'MessageType',
                        index: 'MessageType',
                        classes: "wrapped",
                        width: 450,
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: "select",
                        editoptions: { value: getMessageTypesForAirLineServiceProvider(), style: "width:100px" },
                        editrules: { required: true, edithidden: true },
                        sortable: true
                    },
                    {
                        name: 'Airline',
                        index: 'Airline',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: "select",
                        editoptions: { value: getAirlines(), style: "width:100px" },
                        editrules: { required: true, edithidden: true },
                        sortable: true
                    },
                    {
                        name: 'ServiceProvider',
                        index: 'ServiceProvider',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: "select",
                        editoptions: {
                            value: getServiceProviders(),
                            style: "width:100px",
                            dataEvents: [{
                                type: "change",
                                fn: function (e) {
                                    CheckFieldNeedByProvider();
                                    }
                             }]
                        },
                        editrules: { required: true, edithidden: true },
                        sortable: true
                    },
                    {
                        name: 'RecipientAddress',
                        index: 'RecipientAddress',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        editoptions: { style: "width:100px" },
                        sortable: true
                    },
                    {
                        name: 'ClientPIMA',
                        index: 'ClientPIMA',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        editoptions: { style: "width:100px" },
                        sortable: true
                    },
                    {
                        name: 'MessagePriority',
                        index: 'MessagePriority',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: "select",
                        editoptions: { value: getMessagePriority(), style: "width:100px" },
                        editrules: { required: true, edithidden: true },
                        sortable: true
                    },
                    {
                        name: 'DoubleSignatureCode',
                        index: 'DoubleSignatureCode',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        editoptions: { maxlength: 2, style: "width:100px" },
                        sortable: true
                    },

                    {
                        name: 'ShipmentOrigin',
                        index: 'ShipmentOrigin',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        editoptions: { maxlength: 3, style: "width:100px" },
                        sortable: true
                    }
                ],
                height: '422',
                width: '1000',
                rowNum: 20,
                rowList: [20, 50, 100],
                caption: "Airline Service Providers",
                sortable: true,
                sortname: 'MessageType',
                sortorder: 'asc',
                pager: '#airlineServiceProvidersMappingTablePager'
            });

            $("#airlineServiceProvidersMappingTable").jqGrid('navGrid', '#airlineServiceProvidersMappingTablePager', {},
                { // edit option
                    width: 500,
                    closeAfterEdit: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    viewPagerButtons: false,
                    resize: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                { // add option
                    width: 500,
                    closeAfterAdd: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    resize: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                { // del option
                    closeOnEscape: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
            );

            function getMessageTypes() {
                var messageTypes = jQuery.ajax({
                    url: '<%: Url.Action("MessageTypes") %>',
                    async: false
                }).responseText;
                return messageTypes;
            }

            function getMessageTypesForAirLineServiceProvider() {
                var messageTypes = jQuery.ajax({
                    url: '<%: Url.Action("AirLineServiceProviderMessageTypes") %>',
                    async: false
                }).responseText;
                return messageTypes;
            }

            function getAirlines() {
                var airlines = jQuery.ajax({
                    url: '<%: Url.Action("Airlines") %>',
                    async: false
                }).responseText;
                return airlines;
            }

            function getServiceProviders() {
                var serviceProviders = jQuery.ajax({
                    url: '<%: Url.Action("ServiceProviders") %>',
                    async: false
                }).responseText;
                return serviceProviders;
            }

            function getSpecialServiceProviderNames() {
                var serviceProviders = jQuery.ajax({
                    url: '<%: Url.Action("GetSpecialServiceProviderNames") %>',
                    async: false
                }).responseText;
                return serviceProviders;
            }

            function getMessagePriority() {
                var priorities = jQuery.ajax({
                    url: '<%: Url.Action("GetMessagePriorities") %>',
                    async: false
                }).responseText;
                return priorities;
            }

            function exportAirlineMappingCSVHandler() {
                $("#airlineServiceProvidersMappingExportCsvForm input[name*='selectedClientPK']").each(function () { $(this).val(clientID); });
                return;
            }

            function importAIrlineMappingCSVHandler() {
                $("#airlineServiceProvidersMappingImportCsvForm input[name*='selectedClientPKForImport']").each(function () { $(this).val(clientID); });
                return;
            }


            $('.ui-icon-pencil').click(function () {
                console.log("open edit grid");
                setTimeout(function () {
                    CheckFieldNeedByProvider();
                }, 200);
            });

            function CheckFieldNeedByProvider() {
                var serviceProvider = $("#ServiceProvider option:selected").text();
                console.log("serviceProvider: " + serviceProvider);

                console.log("specialServiceProviders: " + specialServiceProviderNames);
                if (specialServiceProviderNames.includes(serviceProvider) === true) {
                    console.log("match");
                    $("#RecipientAddress").attr("disabled", false);
                    $("#RecipientAddress").attr("readonly", false);
                    $("#ClientPIMA").attr("disabled", false);
                    $("#ClientPIMA").attr("readonly", false);

                    $("#DoubleSignatureCode").attr("disabled", false);
                    $("#DoubleSignatureCode").attr("readonly", false);
                }
                else {
                    console.log("not match");
                    $("#RecipientAddress").attr("disabled", true);
                    $("#RecipientAddress").attr("readonly", true);
                    $("#RecipientAddress").val('');

                    $("#ClientPIMA").attr("disabled", true);
                    $("#ClientPIMA").attr("readonly", true);
                    $("#ClientPIMA").val('');

                    $("#DoubleSignatureCode").attr("disabled", true);
                    $("#DoubleSignatureCode").attr("readonly", true);
                    $("#DoubleSignatureCode").val('');
                }
            }

            exportAirlineMappingCSVHandler();
            importAIrlineMappingCSVHandler();

            $(function () {
                $("#dialog").dialog({ autoOpen: false, modal: true, show: "blind", hide: "blind" });
            });

            $("#airlineServiceProvidersMappingImportCsv").click(function () {
                if ($("#uploadFile").val() === '') {
                    $("#dialog").html("Please select CSV file to upload first.");
                    $("#dialog").dialog("option", "width", 500);
                    $("#dialog").dialog("open");
                    return false;
                }
                $.ajax({
                    method: 'POST',
                    type: "POST",
                    contentType: false,
                    processData: false,
                    dataType: "text",
                    success: function (data) {
                        var responseStr = $("#upload_target").contents().find("pre").text();
                        var response = JSON.parse(responseStr);
                        var message = response["message"];
                        if (response["success"] == false) {
                            message = "<p style='color:red'>" + message + "</p>";
                        }
                        $("#dialog").html(message);
                        $("#dialog").dialog("option", "width", 500);
                        $("#dialog").dialog("open");
                    },
                    error: function (errorData) {
                        ("#dialog").html("Some issue happened dule unknown reason. Please check again later.");
                        $("#dialog").dialog("option", "width", 500);
                        $("#dialog").dialog("open");
                    },
                    cache: false
                });
            });
        });
    </script>

</asp:Content>
