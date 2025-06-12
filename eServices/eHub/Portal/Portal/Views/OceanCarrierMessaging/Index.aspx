<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.css")%>" rel="Stylesheet" type="text/css" />
    <link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <link href="<%: Url.Content("~/PlugIns/jqGrid460/ui.jqgrid.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <link href="<%: Url.Content("~/PlugIns/eHub/style.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    
    <script src="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.min.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/jqGrid460/grid.locale-en.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/jqGrid460/jquery.jqGrid.min.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/eHub/eHub.js")%>" type="text/javascript"></script>

    <script src="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/JSON/json2.js")%>" type="text/javascript"></script>

    <%--<link href="<%: Url.Content("~/PlugIns/jQueryUI-1.11.2/jquery-ui.min.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <script src="<%: Url.Content("~/PlugIns/jQueryUI-1.11.2/jquery-ui.min.js")%>" type="text/javascript"></script>--%>

    <style type="text/css">
		.query-sort {
			padding-left: 2px;
		}
	</style>
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>



<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="errorMessage" class="error" style="display: none"></div>
    <div id="loadingScreen"></div>
	<div id="typesFields">
		<select id="routingRuleTypeList" class="ui-widget ui-state-default ui-corner-all">
		    <% foreach (var option in Model.Options)
		       {
		           %>
		            <option value="<%: option.OptionID %>" <% if (option.OptionID == Model.Selected) { %> selected="selected" <% } %> ><%: option.Description%></option>
		    <% } %>
		</select>
	</div>
	<br />
    <div id="oceanFields">
		<table id="oceanTable"></table>
        <div id="oceanTablePager"></div>
    </div>

    <button id="oceanUpBtn" disabled="disabled" class="ui-widget">Move Up</button>
    <button id="oceanDownBtn" disabled="disabled" class="ui-widget">Move Down</button>
    <button id="setPosBtn" disabled="disabled" class="ui-widget">Set Position:</button>
    <input id="setPosInput" type="text" size="1"/>

    <div id="oceanCsvFields">
        <form action="<%: Url.Action("ExportCSV") %>" id="oceanExportCsvForm">
                <fieldset class="export data-transfer-container">
                        <legend>Export</legend>
                        <input type="submit" value="Export to CSV" id="input_exportcsv" class="ui-widget"/>
                </fieldset>
        </form>
        <form id="importCsvForm" action="<%: Url.Action("ImportCSV") %>" method="post" enctype="multipart/form-data" target="upload_target" class="inline-block">
                <fieldset class="import data-transfer-container">
                        <legend>Import</legend>
                        <div>
                        <input type="submit" value="Import CSV" class="ui-widget" id="input_importcsv" disabled="disabled" />
                        <input type="file" name="uploadFile" id="uploadFile" class="ui-widget ui-state-default ui-corner-all ui-button-text" size="55" />
                        <iframe id="upload_target" name="upload_target" src="" style="width:0;height:0;border:0 solid #fff;"></iframe>
                        </div>
                </fieldset>
        </form>
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

    <div id="selectProviderDialog" title="Select Provider" style="display: none">
        <div id="providersFields">
            <table id="providersTable"></table>
            <div id="providersTablePager"></div>
        </div>
        <form action="">
            <input type="hidden" name="providerID" id="providerID" />
        </form>
    </div>

    <div id="selectShpTypeDialog" title="Select Shipment Type" style="display: none">
        <div id="shpTypesFields">
            <table id="shpTypesTable"></table>
            <div id="shpTypesTablePager"></div>
        </div>
        <form action="">
            <input type="hidden" name="shpType" id="shpType" />
        </form>
    </div>

    <div id="selectDocNameDialog" title="Select Document Name" style="display: none">
        <div id="docNamesFields">
            <table id="docNamesTable"></table>
            <div id="docNamesTablePager"></div>
        </div>
        <form action="">
            <input type="hidden" name="docName" id="docName" />
        </form>
    </div>

    <div id="selectPartyToCopyDialog" title="Select Party To Copy" style="display: none">
        <div id="partiesToCopyFields">
            <table id="partiesToCopyTable"></table>
            <div id="partiesToCopyTablePager"></div>
        </div>
        <form action="">
            <input type="hidden" name="partyToCopy" id="partyToCopy" />
        </form>
    </div>

    <div id="selectSplitByDialog" title="Select Split By Options" style="display: none">
        <div id="splitByFields">
            <table id="splitByTable"></table>
            <div id="splitByTablePager"></div>
        </div>
        <form action="">
            <input type="hidden" name="splitBy" id="splitBy" />
        </form>
    </div>
    <div id="savingToDatabaseDialog" style="display:none">
        <p><span style="float:left; margin:12px 12px 20px 0;">Fail to save all data into database. Please find details in log file.</span></p>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        const DEFAULT_SORT_ORDER = 'asc';
        var lastSel, grid = $("#oceanTable");
        var edited = false, nextid = 0;

        $(window).bind('beforeunload', function () {
            if (edited)
                return "All your change will be lost in next 5 minute after you close this page, please save all before you exit.";
        });

        window.setInterval(function () {
            $.post('<%: Url.Action("ExtendSession") %>', function () {
                // Extend session
            });
        }, 240000); // 4 minute

        function isNullOrEmpty(string) {
            return string === null || string === undefined || string === "";
        }

        function goToCode(appCode) {
            if (appCode && appCode != '') {
                var index = '<%: Url.Action("Index") %>';
                var selected = '<%: Model.Selected %>';
                if (!isNullOrEmpty(selected) && appCode) {
                    top.location = index.indexOf(selected) >= 0 ? index.replace(new RegExp(selected + '$'), appCode) : index + "/" + appCode;
                }
            }
        }

        var RoutingRuleTypeChangeHandler = function () {
        	if (edited && !confirm("You haven't saved your changes in current page. Do you want to switch page without saving current changes?"))
        	    return;
        	$.post('<%: Url.Action("SelectType") %>', { id: $('#routingRuleTypeList').val() }, function (response) {
                ResetHeaders(response.rows);
	            ReloadAll();
	        });
        };
        $("#routingRuleTypeList").change(RoutingRuleTypeChangeHandler).keypress(RoutingRuleTypeChangeHandler);

        function ResetHeaders(rows) {
            $(grid.jqGrid('getGridParam', 'colModel')).each(function(index, colModel) {
                if (colModel.name == 'rn' || colModel.name == 'id' || colModel.name == 'act') {
                    // jqgrid's mandatory rows
                } else if (rows.indexOf(colModel.name) > -1) {
                    grid.jqGrid('showCol', colModel.name);
                    grid.jqGrid('setLabel', colModel.name, GetColumnName(colModel.name));
                } else {
                    grid.jqGrid('hideCol', colModel.name);
                }
            });
        }

        function GetColumnName(column) {
            return jQuery.ajax({
                url: encodeURI('<%: Url.Action("GetColumnName") %>' + '?column=' + column),
                async: false
            }).responseText;
        }

        function ReloadFromDatabase() {
            $.post('<%: Url.Action("ClearCache") %>', ReloadAll);
        }

        function ReloadAll() {
            grid.jqGrid('clearGridData');
            grid.trigger('reloadGrid');
            edited = false;
            nextid = 0;
        }

        $(document).ready(function () {
            $('#errorMessage').ajaxError(function (event, request, settings) {
                $(this).html("<div>Error retrieving data from " + settings.url + "<\/div>" + "<div>Status: " + request.status + "<\/div>" + "<div>Error: " + request.statusText + "<\/div>");
                $(this).show();
            }).ajaxStart(function () {
                $(this).hide();
            });

            grid.jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("Values") %>',
                editurl: '<%: Url.Action("ValuesEdit") %>',
                jsonReader: { root: 'oceanMessagingValues', id: 'id', repeatitems: false },
                colModel: [
                    { name: 'id', index: 'id', hidden: true, align: 'center' },
                    { name: 'client', index: 'client', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('client'),
                        edittype: 'custom', editoptions: {
                            custom_element: FormatDropdownEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.clientIdValue').val();
                            }
                        }
                    },
                    {
                        name: 'carrier', index: 'carrier', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('carrier')
                    },
                    {
                        name: 'carrierAgent', index: 'carrierAgent', hidden: true, width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('carrierAgent')
                    },
                    {
                        name: 'eventBranch', index: 'eventBranch', hidden: true, width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('eventBranch')
                    },
                    {
                        name: 'purpose', index: 'purpose', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('purpose')
                    },
                    {
                        name: 'port', index: 'port', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('port')
                    },
                    { name: 'shpType', index: 'shpType', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['bw', 'eq'] }, label: GetColumnName('shpType'),
                        edittype: 'custom', editoptions: {
                            custom_element: FormatDropdownEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.shpTypeValue').val();
                            }
                        }
                    },
                    { name: 'docName', index: 'docName', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('docName'),
                        edittype: 'custom', editoptions: {
                            custom_element: FormatDropdownEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.docNameValue').val();
                            }
                        }
                    },
                    {
                        name: 'shipNamespace', index: 'shipNamespace', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('shipNamespace')
                    },
                    { name: 'splitBy', index: 'splitBy', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('splitBy'),
                        edittype: 'custom', editoptions: {
                            custom_element: FormatDropdownEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.splitByValue').val();
                            }
                        }
                    },
                    { name: 'provider', index: 'provider', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('provider'),
                        edittype: 'custom', editoptions: {
                            custom_element: FormatDropdownEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.providerIdValue').val();
                            }
                        }
                    },
                    {
                        name: 'partyToCopy', index: 'partyToCopy', width: 150, classes: "wrapped", align: 'center', editable: true, search: true, sortable: true, searchoptions: { sopt: ['cn', 'bw', 'ew', 'eq'] }, label: GetColumnName('partyToCopy'),
                        edittype: 'custom', editoptions: {
                            custom_element: FormatDropdownEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.partyToCopy').val();
                            }
                        }
                    },
                    { name: 'act', index: 'act', width: 50, align: 'left', sortable: true, search: false, label: ' ' }
                ],
                onPaging: function (pgButton) {
                    $('#setPosInput').val('');
                },
                rowNum: 15,
                rowList: [15, 25, 50],
                pager: '#oceanTablePager',
                rownumbers: true,
                ignoreCase: true,
                scrollingRows: false,
                viewrecords: true,
                caption: 'Ocean Carrier Messaging',
                height: '380',
                sortorder: DEFAULT_SORT_ORDER,
                ondblClickRow: myOndblClickRow,
                onSelectRow: myOnSelectRow,
                afterInsertRow: myAfterInsertRow
            });

            $.post('<%: Url.Action("SelectType") %>', { id: $('#routingRuleTypeList').val() }, function (response) {
                ResetHeaders(response.rows);
            });

            grid.jqGrid('filterToolbar', {
                defaultSearch: 'cn',
                searchOnEnter: false,
                afterSearch: function () {
                    clear('search');
                }
            });

            $('.ui-search-toolbar').hide();

            grid.jqGrid('navGrid', '#oceanTablePager', {
                edit: true
            }, { // on edit
                width: 400,
                closeAfterEdit: true,
                closeOnEscape: true,
                recreateForm: true,
                viewPagerButtons: true,
                resize: true,
                beforeShowForm: function () {
                    if (!isEmptyInLineEditing()) {
                        $('<div><p class="ui-state-warning">Please Save All Inline-Editing rows before using edit form.<\/p><\/div>').dialog({
                            title: 'Warning',
                            modal: true,
                            resizable: false,
                            buttons: {
                                "Close": function () {
                                    $(this).dialog("close");
                                    $('#' + getSelectedId() + '_client_val').focus();
                                }
                            }
                        });
                    }
                },
                afterShowForm: function () {
                    if (!isEmptyInLineEditing()) {
                        $('#editmodoceanTable .ui-jqdialog-titlebar-close').click();
                    } else {
                        if (getSelectedId() == 'default') {
                            $('#carrier').addClass('readonly').attr("readonly", true);
                        }
                    }
                },
                afterSubmit: function (response) {
                    var data = eval('(' + response.responseText + ')');
                    if (data.success == true) {
                        edited = true;
                    }
                    return [data.success, data.message, data.id];
                }
            }, { // on add
                width: 400,
                editData: {
                    afterRowId: getAfterRowId(),
                    nextid: function () { return "n" + nextid; }
                },
                closeAfterAdd: true,
                closeOnEscape: true,
                recreateForm: true,
                viewPagerButtons: true,
                resize: true,
                reloadAfterSubmit: true,
                beforeShowForm: function () {
                    if (!isEmptyInLineEditing()) {
                        $('<div><p class="ui-state-warning">Please Save All Inline-Editing rows before using add form.<\/p><\/div>').dialog({
                            title: 'Warning',
                            modal: true,
                            resizable: false,
                            buttons: {
                                "Close": function () {
                                    $(this).dialog("close");
                                    $('#' + getSelectedId() + '_client_val').focus();
                                }
                            }
                        });
                    }
                },
                afterShowForm: function () {
                    if (!isEmptyInLineEditing()) {
                        $('#edithdoceanTable .ui-jqdialog-titlebar-close').click();
                    }
                },
                afterSubmit: function (response) {
                    var data = eval('(' + response.responseText + ')');
                    if (data.success == true) {
                        nextid++;
                        edited = true;
                    }
                    return [data.success, data.message, data.id];
                }
            }, {}, { // on search
                onReset: resetSortOrder,
                multipleSearch: true,
				showQuery: true,
                beforeShowSearch: modifySearchForm,
                onSearch: function () {
                    let sortColumn = $('#sort-column').val(),
                        sortOrder = $('#sort-order').val();
                    grid.jqGrid("setGridParam", {
                        sortname: sortColumn,
                        sortorder: sortOrder
                    });
                }
            }
            );

            grid.jqGrid('navButtonAdd', '#oceanTablePager', {
                id: "oceantable_filter",
                caption: "",
                title: "Toggle Search Toolbar",
                buttonicon: 'ui-icon-circle-zoomout',
                onClickButton: function () {
                    this.toggleToolbar();
                    if ($.isFunction(this.p._complete)) {
                        if ($('.ui-search-toolbar', this.grid.hDiv).is(':visible')) {
                            $('.ui-search-toolbar', this.grid.fhDiv).show();
                        } else {
                            $('.ui-search-toolbar', this.grid.fhDiv).hide();
                        }
                        this.p._complete.call(this);
                        window.fixPositionsOfFrozenDivs.call(this);
                    }
                }
            });

            grid.jqGrid('navButtonAdd', '#oceanTablePager', {
                id: "oceantable_saveAll",
                caption: "",
                title: "Save All to Server",
                buttonicon: "ui-icon-disk"
            });

            grid.jqGrid('navButtonAdd', '#oceanTablePager', {
                id: "inlineAdd",
                caption: "",
                title: "Add inline row",
                buttonicon: "ui-icon-circle-plus"
            });

            grid.jqGrid('navButtonAdd', '#oceanTablePager', {
                id: "oceantable_reload",
                caption: "",
                title: "Reload from Database",
                buttonicon: "ui-icon-refresh"
            });

            grid.jqGrid('navButtonAdd', '#oceanTablePager', {
                id: "oceantable_refresh",
                caption: "",
                title: "Refresh current cache(Not clear cache)",
                buttonicon: "ui-icon-arrowrefresh-1-w"
            });

            $('#input_exportcsv').button();
            $('#input_importcsv').button();
            $("#inlineAdd").insertBefore("#add_oceanTable");
            $("#oceantable_filter").insertAfter("#refresh_oceanTable");
            $("#search_oceanTable").insertAfter("#refresh_oceanTable");
            $('#oceanUpBtn').appendTo('#oceanTablePager_right');
            $('#oceanDownBtn').appendTo('#oceanTablePager_right');
            $('#setPosBtn').appendTo('#oceanTablePager_right');
            $('#setPosInput').appendTo('#oceanTablePager_right');
            $('#oceantable_refresh').insertBefore('#refresh_oceanTable');
            $('#refresh_oceanTable').hide();
            $('#oceanTablePager_right div').replaceWith(function () { return '<a id="totalRows" dir="ltr" style="text-align:right" class="ui-paging-info">' + this.innerHTML + '</a>'; });
            $('#oceanTablePager_right div').remove();
            $('#totalRows').insertAfter('#setPosInput');
            $('<a>/</a>').insertBefore('#totalRows');

            $("#inlineAdd").click(function () {
                var newId = nextid++;
                var newIdString = "n" + newId;
                var newData = { id: newIdString, afterRowId: getAfterRowId(), oper: "add" };
                $.post('<%: Url.Action("ValuesEdit") %>', newData, function () {
                    var selRow = parseInt(newData.afterRowId) || grid.find("tr:not([id='']):first")[0].id;

                    grid.addRowData(newIdString, newData, 'before', selRow);
                    setTimeout(function () { $('#il_edit_' + newIdString).click(); }, 50);
                    $('#oceanTable input').blur();
                    edited = true;
                });
            });

            $('#oceantable_reload').click(function () {
            	ReloadFromDatabase();
            });

            $('#oceantable_refresh').click(function () {
                $.post('<%: Url.Action("ReformatSessionRowID") %>', function () {
                    grid.trigger('reloadGrid');
                    nextid = 0;
                });
            });

           
            $('#oceantable_saveAll').click(function () {
                grid.trigger("reloadGrid", [{ page: grid.getGridParam('page')}]);
                $("#loadingScreen").addClass("loading");
                $.post('<%: Url.Action("SaveAll") %>', function (d) {
                    $("#loadingScreen").removeClass("loading");
                    if (d.message.startsWith('Fail to save all data into database.')) {
                        DisplayFailedSavingToDB();
                    }
                    $.ehubToast(d.message);
                    edited = false;
                });
            });

            $('#oceanUpBtn').click(function () {
                if (IsOnSearch()) {
                    $.ehubToast("Search/Filter mode is applied. Please remove all search/filder in order to move row position.");
                    return;
                }
                var id = getSelectedId();
                if (!id) return;
                var position = getSelectedPosition();
                var rowLimit = grid.jqGrid('getGridParam', 'rowNum');
                position--;

                if (position > 0) {
                    var rows = $('tr', grid);
                    $('#' + id).insertBefore(rows.eq(position));
                    myOnSelectedPosition(position);
                }
                $.post('<%: Url.Action("MoveRow") %>', { id: id, option: "up", rowLimit: rowLimit }, function (d) {
                    // Move all the upper code into here if there is bug
                    // Did move codes to before this post method to load faster in client side
                    if (position <= 0) {
                        grid.trigger("reloadGrid", [{ page: d.page}]);
                        selectRowAfterMove(id);
                        edited = true;
                    } else {
                        grid.trigger("reloadGrid", [{ current: true}]);
                        $('#setPosInput').val(calculatePosition(d.page, rowLimit, position));
                    }
                });
            });

            $('#oceanDownBtn').click(function () {
                if (IsOnSearch()) {
                    $.ehubToast("Search/Filter mode is applied. Please remove all search/filder in order to move row position.");
                    return;
                }
                var id = getSelectedId();
                if (!id) return;
                var position = getSelectedPosition();
                var rowLimit = grid.jqGrid('getGridParam', 'rowNum');
                position++;
                var rows = $('tr', grid);

                if (position < rows.length) {
                    $('#' + id).insertAfter(rows.eq(position));
                    myOnSelectedPosition(position);
                }
                $.post('<%: Url.Action("MoveRow") %>', { id: id, option: "down", rowLimit: rowLimit }, function (d) {
                    // Move all the upper code into here if there is bug
                    // Did move codes to before this post method to load faster in client side
                    if (position >= rows.length) {
                        grid.trigger("reloadGrid", [{ page: d.page}]);
                        selectRowAfterMove(id);
                        edited = true;
                    } else {
                        grid.trigger("reloadGrid", [{ current: true}]);
                        $('#setPosInput').val(calculatePosition(d.page, rowLimit, position));
                    }
                });
            });

            $('#setPosBtn').click(function () {
                if (IsOnSearch()) {
                    $.ehubToast("Search/Filter mode is applied. Please remove all search/filder in order to move row position.");
                    return;
                }
                var id = getSelectedId();
                if (!id) return;
                var newPosition = $('#setPosInput').val();
                var rowLimit = grid.jqGrid('getGridParam', 'rowNum');
                $.getJSON('<%: Url.Action("MoveRow") %>', { id: id, option: "set", newPosition: newPosition, rowLimit: rowLimit }, function (d) {
                    grid.trigger("reloadGrid", [{ page: d.page}]);
                    selectRowAfterMove(id);
                    edited = true;
                });
            });

            $('#setPosInput').keyup(function() {
                var value = $(this).val();
                grid.getGridParam('lastpage');

                if (!value) return;
                var max =  hasFallBackLastRow() ? totalRows() -1  : totalRows();
                
                try {
                    var newValue = parseInt(value);
                    if (!newValue)
                        $(this).val(1);
                    else if (value > max)
                        $(this).val(max);
                    else
                        $(this).val(newValue);
                }
                catch (err) {
                    $(this).val(1);
                }
            });

			function totalRows() {
				return parseInt($('#totalRows').html().replace(",", ""));
            }
            function hasFallBackLastRow() {
                var currentOption = $('#routingRuleTypeList > option:selected').val();
                switch(currentOption) {
                case "CarrierHandlingAgent":
                case "CarrierBookingAgent":
                case "DefaultCarrier":
                    return true;
                default:
                    return false;
                }
            }

            function modifySearchForm(searchForm) {
                let sortTable = $('<table/>');
                let headerRow = $('<tr/>');
                let addCell = $('<td/>');
                let sortColumns = $('td.columns').first().find('select').clone().attr('id', 'sort-column');
				sortColumns.prepend($('<option>', { value: '', text: 'Default', selected: true }));
                let sortOrder = $('<select>'
                    + '<option value="asc">Ascending</option >'
                    + '<option value="desc">Descending</option >'
                    + '</select >').attr('id', 'sort-order');
				sortColumns.change(function () {
					updateSortOrderInQuery();
					toggleSortOrderVisibilityIfNecessary(sortColumns, sortOrder);
				});
                sortOrder.change(updateSortOrderInQuery);
                headerRow.append('<th>Order By</th>');
                headerRow.append(addCell);
                sortTable.append(headerRow);
                sortTable.append($('<tr/>').append($('<td/>').append(sortColumns)).append($('<td/>').append(sortOrder)));
                searchForm.append(sortTable);

				toggleSortOrderVisibilityIfNecessary(sortColumns, sortOrder);
                return true;
            }
            
            function resetSortOrder() {
                $('#sort-order').val(DEFAULT_SORT_ORDER);
                $('#sort-column').val('');
                $('td.query-sort').html('');
                grid.jqGrid("setGridParam", {
                    sortname: '',
                    sortorder: DEFAULT_SORT_ORDER
                });
            }

			function updateSortOrderInQuery() {
				let sortColumn = $('#sort-column').val();
				let sortOrder = $('#sort-order').val();

                let sortQueryCell = ($('td.query-sort').length) ? $('td.query-sort') : $('<td class="query-sort"/>');
                if (sortColumn === '') {
                    sortQueryCell.html('');
                } else {
					sortQueryCell.html(`ORDER BY ${sortColumn} ${sortOrder.toUpperCase()}`);
				}

				$(sortQueryCell).insertAfter($('td.query').first());
            }

            function toggleSortOrderVisibilityIfNecessary(sortColumns, sortOrder) {
				if (sortColumns.val() === '') {
					sortOrder.closest('td').hide();
				} else {
					sortOrder.closest('td').show();
				}
			}
        });
	</script>

    <script type="text/javascript">
        var currentSelectedId;

        function myOndblClickRow(id) {
            var editButton = $('#il_edit_' + id);
            if (editButton.is(":visible"))
                editButton.click();
        }

        function myOnSelectRow(currentSelectedId) {
            var position = grid.jqGrid('getInd', currentSelectedId);
            var rowNumber = $('#' + currentSelectedId).children("td:first").text();
            $('#setPosInput').val(rowNumber);

            if (currentSelectedId != 'default') {
                $('#setPosBtn').removeAttr("disabled");
                if (!$('#del_oceanTable').is(":visible")) $('#del_oceanTable').show();
                myOnSelectedPosition(position);
            } else {
                $('#oceanUpBtn').attr("disabled", "true");
                $('#oceanDownBtn').attr("disabled", "true");
                $('#setPosBtn').attr("disabled", "true");
                $('#del_oceanTable').hide();
            }
        }

        function myOnSelectedPosition(position) {
            var lastposition = grid.jqGrid('getInd', 'default');
            var currentPage = grid.getGridParam('page');

            if (currentPage != 1 || position > 1)
                $('#oceanUpBtn').removeAttr("disabled");
            else
                $('#oceanUpBtn').attr("disabled", "true");

            // if lastpositon == null or not last position
            if (!lastposition || position < lastposition - 1)
                $('#oceanDownBtn').removeAttr("disabled");
            else
                $('#oceanDownBtn').attr("disabled", "true");
        }

        function getAfterRowId() {
            var selr = grid.jqGrid('getGridParam', 'selrow');
            return selr != null ? selr : 'default';
        }

        function getSelectedId() {
            return grid.getGridParam("selrow");
        }

        function getSelectedPosition() {
            return grid.jqGrid('getInd', getSelectedId());
        }

        function getLastPosition() {
            return grid.jqGrid('getInd', 'default');
        }

        function myAfterInsertRow(id, data, el) {
            addActionsOnRow(id);
            $('#jSaveButton_' + id).find("span").removeClass("ui-icon-disk")
                                                .addClass("ui-icon-check");
            if (id == "default") {
                grid.setRowData('default', false, 'unsortable');
                grid.jqGrid('setCell', 'default', 'client', '', 'not-editable-cell');
                grid.jqGrid('setCell', 'default', 'carrier', '', 'not-editable-cell');
                grid.jqGrid('setCell', 'default', 'carrierAgent', '', 'not-editable-cell');
                grid.jqGrid('setCell', 'default', 'port', '', 'not-editable-cell');
                grid.jqGrid('setCell', 'default', 'docName', '', 'not-editable-cell');
                grid.jqGrid('setCell', 'default', 'shpType', '', 'not-editable-cell');
                $('#jDeleteButton_default').remove();
            }
        }

        function selectRowAfterMove(id) {
            setTimeout(function () { grid.jqGrid("setSelection", id); }, 100);
        }

        function addActionsOnRow(cl) {
            be = "<input id='il_edit_" + cl + "' style='height:22px;width:20px;' type='button' class='ui-icon ui-icon-pencil inline-block' onclick=\"editRow('" + cl + "');\"  />";
            de = cl != 'default' ? "<input id='il_delete_" + cl + "'style='height:22px;width:20px;' type='button' class='ui-icon ui-icon-trash inline-block' onclick=\"deleteRow('" + cl + "');\"  />" : '';
            se = "<input id='il_save_" + cl + "'style='height:22px;width:20px;display: none;' type='button' class='ui-icon ui-icon-disk inline-block inlinesave' onclick=\"saveRow('" + cl + "');\"  />";
            ce = "<input id='il_cancel_" + cl + "'style='height:22px;width:20px;display: none;' type='button' class='ui-icon ui-icon-cancel inline-block' onclick=\"restoreRow('" + cl + "');\" />";
            grid.jqGrid('setRowData', cl, { act: be + de + se + ce });
        }

        function clear(options) {
            grid.jqGrid('setGridParam', { search: false });

            var postData = grid.jqGrid('getGridParam', 'postData');

            if (options == 'search') {
                $('#searchcntfbox_oceanTable :input').val("");
                $.extend(postData, { searchField: "", searchString: "", searchOper: "" });
            }
            else {
                $('#gs_client').val("");
                $('#gs_carrier').val("");
                $('#gs_provider').val("");
                $.extend(postData, { filters: "" });
            }

            grid.trigger("reloadGrid", [{ page: 1}]);
        }

        function myGridComplete() {
            var ids = grid.jqGrid('getDataIDs');
            for (var i = 0; i < ids.length; i++) {
                var cl = ids[i];
                addActionsOnRow(cl);
            }
        }

        function editRow(id) {
            grid.jqGrid("setSelection", id);
            grid.editRow(id, false,
                function (rowid) { //onEditFunc
                    $('.not-editable-cell', '#default').html("Default");
                },
                null, null, {}, function (response) { saveRow(id); }, {}, function (response) { restoreRow(id); });
            inlineToggle(id);
            edited = true;
        }

        function deleteRow(id) {
            grid.jqGrid('delRowData', id);
            $.post('<%: Url.Action("ValuesEdit") %>', { id: id, oper: "del" }, function (d) {
                // Move all the upper code into here if there is bug
                // Did move codes to before this post method to load faster in client side
                edited = true;
                $('#setPosInput').val('');
            });
        }

        function saveRow(id) {
            var rids = grid.jqGrid('getDataIDs');
            var afterRowId = rids[grid.jqGrid('getInd', id)];
            $('#' + id).find('input').blur();
            var nextSaveButton = $('#' + id).next('tr').find('.inlinesave');
            if (nextSaveButton.is(':visible')) nextSaveButton.click();
            grid.saveRow(id, {
                extraparam: { afterRowId: afterRowId },
                aftersavefunc: function (serverId, response) {
                    var data = eval('(' + response.responseText + ')');
                    if (data.success) {
                        inlineToggle(serverId);
                        edited = true;
                    } else {
                        inlineToggle(serverId);
                        editRow(serverId);
                        $.ehubToast(data.message);
                    }
                }
            });
        }

        function restoreRow(id) {
            var mandatoryField = "provider";
            grid.restoreRow(id);
            var mandatoryFieldValue = grid.getRowData(id)[mandatoryField];
            // If the data of provider is not empty, work normal, else remove the current row after adding a inline row.
            if (mandatoryFieldValue != '&nbsp;' && mandatoryFieldValue != '')
                inlineToggle(id);
            else
                deleteRow(id);
        }

        function emptyMandatoryField(id) {
            var mandatoryField = "provider";
            var val = $('#' + id + '_' + mandatoryField + '_val').val();
            if (val == '') {
                restoreRow(id);
                $.ehubToast('Fail to save inline change because of empty ' + mandatoryField);
                return true;
            }
            return false;
        }

        var toggleCount = 0;
        function inlineToggle(id) {
            $('#il_edit_' + id).toggle();
            $('#il_delete_' + id).toggle();
            $('#il_save_' + id).toggle();
            $('#il_cancel_' + id).toggle();
        }

        function isEmptyInLineEditing() {
            return !grid.find("span.editable")[0] && $('.inlinesave[style*="display: inline-block"]').length == 0;
        }

        function IsOnSearch() {
            var empty = !$('#gs_client').val() && !$('#gs_carrier').val() && !$('#gs_provider').val() & !$('#jqg1').val();
            return !empty;
        }
		var clientEditState = {
			includeNonProd: false,
			checkbox: null,
        };

		function FormatDropdownEditFields(value, options, includeNonProd) {
            var idVal = "";
            var readonly = "";
            var readonlyClass = "";
            switch (options.name) {
                case 'client':
                    idVal = 'clientIdValue';
                    break;
                case 'provider':
                    idVal = 'providerIdValue';
                    readonly = ' readonly="true"';
                    readonlyClass = ' readonly';
                    break;
                case 'shpType':
                    idVal = 'shpTypeValue';
                    readonly = ' readonly="true"';
                    readonlyClass = ' readonly';
                    break;
                case 'splitBy':
                    idVal = 'splitByValue';
                    readonly = ' readonly="true"';
                    readonlyClass = ' readonly';
                    break;
                case 'partyToCopy':
                    idVal = 'partyToCopy';
                    readonly = ' readonly="true"';
                    readonlyClass = ' readonly';
                    break;
                default:
                    idVal = 'docNameValue';
                    readonly = ' readonly="true"';
                    readonlyClass = ' readonly';
            }
            var selRow = getSelectedId();
            var elemStr;

            if (options.name == 'client') {
                var checkedAttribute = '';
                elemStr = '<span><input id="' + options.id + '_val" value="' + value +
                    '" class="FormElement ui-widget-content ui-corner-all ' + idVal + readonlyClass + '" role="textbox" type="text"' + readonly + '/>' +
                    ' <a class="fm-button ui-state-default ui-corner-all idSelect">Select</a>' +
                    ' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
                    ' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
                    '</span>';
                var elem = $(elemStr)[0];
				clientEditState.checkbox = $(elem).find('.clientIdCheckbox');
				clientEditState.checkbox.change(function () {
					clientEditState.includeNonProd = clientEditState.checkbox.is(":checked");
                });

                $(elem).children('.idSelect').click(function () {
                    DisplaySelectClientDialog($(elem).children('.' + idVal), includeNonProd);
				});
				return elem;
                    }
            else if (selRow == 'default' && options.name != 'provider' && options.name != 'client') {
                elemStr = '<span><input id="' + options.id + '_val" value="' + value +
                    '" class="FormElement ui-widget-content ui-corner-all readonly ' + idVal + '" role="textbox" type="text" readonly="true"/>';
                return $(elemStr)[0];
            }
            else {
                elemStr = '<span><input id="' + options.id + '_val" value="' + value +
                    '" class="FormElement ui-widget-content ui-corner-all ' + idVal + readonlyClass + '" role="textbox" type="text"' + readonly + '/>' +
                    ' <a class="fm-button ui-state-default ui-corner-all idSelect">Select</a>';
                var elem = $(elemStr)[0];
                $(elem).children('.idSelect').click(function () {
                    DisplaySelectDialog($(elem).children('.' + idVal), options.name);
                });
                return elem;
            }
        }

		function DisplaySelectDialog(elem, option) {
            if (option == "provider")
                DisplaySelectProviderDialog(elem);
            else if (option == "shpType")
                DisplaySelectShpTypeDialog(elem);
            else if (option == "docName")
                DisplaySelectDocNameDialog(elem);
            else if (option == "partyToCopy")
                DisplaySelectPartyToCopyDialog(elem);
            else if (option == "splitBy")
                DisplaySelectSplitByDialog(elem);
        }

			function DisplaySelectClientDialog(elem, includeNonProd) {
            $("#selectClientDialog").dialog({
                title: 'Select Client',
                width: "auto",
                position: { my: "left top", at: "left+100 top-100", of: $(elem) },
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
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });

				includeNonProd = clientEditState.includeNonProd;
				$("#clientsTable").jqGrid("setGridParam", {
					url: '<%: Url.Action("Clients") %>' + '?includeNonProd=' + includeNonProd,
	}).trigger("reloadGrid");

            $("#clientsTable").jqGrid({
                datatype: 'json',
				url: '<%: Url.Action("Clients") %>' + '?includeNonProd=' + includeNonProd,
                jsonReader: { root: 'eHubClients', id: 'CC_ID', repeatitems: false },
                colNames: ['Client ID', 'Name'],
                colModel: [
                        { name: 'CC_ID', width: 100, classes: "wrapped", sortable: true, search: true },
                        { name: 'CC_FriendlyName', width: 300, classes: "wrapped", sortable: true, search: true }
                    ],
                onSelectRow: function (id) {
                    $('#selectClientDialog #clientID').val(id);
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
        }
        var previousSelection = '';
        function DisplaySelectProviderDialog(elem) {
            $("#selectProviderDialog").dialog({
                title: 'Select Service Provider',
                width: "auto",
                position: { my: "left top", at: "left+100 top-100", of: $(elem) },
                resizable: false,
                modal: true,
                buttons: {
                    "Select": function () {
                        var selectID = $('#selectProviderDialog #providerID').val();
                        if (selectID == "") {
                            DisplayError("Please select a service provider.");
                        }
                        else {
                            $(elem).val(selectID);
                            $(this).dialog("close");
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });

            var selected = getSelectedId();
            
            $("#providersTable").jqGrid({
                datatype: 'json',
                url: GetProviderURL(selected),
                jsonReader: { root: 'serviceProvider', id: 'ID', repeatitems: false },
                colNames: ['Service Provider'],
                colModel: [
                        { name: 'ID', width: 200, classes: "wrapped", sortable: true, search: true }
                    ],
                onSelectRow: function (id) {
                    $('#selectProviderDialog #providerID').val(id);
                },
                height: '400',
                width: '200',
                rowNum: 15,
                rowList: [15, 30, 50],
                sortname: 'CC_ID',
                sortorder: 'asc',
                pager: '#providersTablePager'
            });

            if (previousSelection == '') {
                previousSelection = selected;
            } else if (selected == 'default' && previousSelection != selected) {
                previousSelection = selected;
                $('#providersTable').setGridParam({
                    url: GetProviderURL(selected)
                }).trigger("reloadGrid");
            } else if (selected != 'default' && previousSelection == 'default') {
                previousSelection = selected;
                $('#providersTable').setGridParam({
                    url: GetProviderURL(selected)
                }).trigger("reloadGrid");
            }

            $("#providersTable").jqGrid('navGrid', '#providersTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
            $("#providersTable").jqGrid('filterToolbar', { searchOnEnter: false });
        }

        function GetProviderURL(id) {
            if (id == 'default')
                return '<%: Url.Action("ServiceProviders") %>' + '?selectedID=' + id;
            return '<%: Url.Action("ServiceProviders") %>';
        }

        function DisplaySelectShpTypeDialog(elem) {
            $("#selectShpTypeDialog").dialog({
                title: 'Select Shipment Type',
                width: "auto",
                position: { my: "left top", at: "left+100 top-100", of: $(elem) },
                resizable: false,
                modal: true,
                buttons: {
                    "Select": function () {
                        var selectName = $('#selectShpTypeDialog #shpType').val();
                        if (selectName == "") {
                            DisplayError("Please select a document name.");
                        }
                        else {
                            $(elem).val(selectName);
                            $(this).dialog("close");
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });

            $("#shpTypesTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("ShipmentTypes") %>',
                jsonReader: { root: 'shipmentTypes', id: 'shpType', repeatitems: false },
                colNames: ['Shipment Type', 'Purpose'],
                colModel: [
                        { name: 'shpType', width: 200, classes: "wrapped", sortable: false, search: false },
                        { name: 'description', width: 200, classes: "wrapped", sortable: false, search: false }
                    ],
                onSelectRow: function (id) {
                    $('#selectShpTypeDialog #shpType').val(id);
                },
                height: '300',
                width: '300',
                loadonce: true, // Unchanging data only
                rowNum: 15,
                rowList: [15, 30, 50],
                sortname: 'shpType',
                sortorder: 'asc',
                pager: '#shpTypesTablePager'
            });
            $("#shpTypesTable").jqGrid('navGrid', '#shpTypesTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
        }

        function DisplaySelectDocNameDialog(elem) {
            $("#selectDocNameDialog").dialog({
                title: 'Select Document Name',
                width: "auto",
                position: { my: "left top", at: "left+100 top-100", of: $(elem) },
                resizable: false,
                modal: true,
                buttons: {
                    "Select": function () {
                        var selectName = $('#selectDocNameDialog #docName').val();
                        if (selectName == "") {
                            DisplayError("Please select a document name.");
                        }
                        else {
                            $(elem).val(selectName);
                            $(this).dialog("close");
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });

            $("#docNamesTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("DocumentNames") %>',
                jsonReader: { root: 'docNames', id: 'docName', repeatitems: false },
                colNames: ['Document Name'],
                colModel: [
                        { name: 'docName', width: 200, classes: "wrapped", sortable: true, search: true }
                    ],
                onSelectRow: function (id) {
                    $('#selectDocNameDialog #docName').val(id);
                },
                height: '400',
                width: '200',
                loadonce: true, // Unchanging data only
                rowNum: 15,
                rowList: [15, 30, 50],
                sortname: 'docName',
                sortorder: 'asc',
                pager: '#docNamesTablePager'
            });
            $("#docNamesTable").jqGrid('navGrid', '#docNamesTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
            $("#docNamesTable").jqGrid('filterToolbar', { searchOnEnter: false });
        }

        function DisplaySelectPartyToCopyDialog(elem) {
            $("#selectPartyToCopyDialog").dialog({
                title: 'Select Party To Copy',
                width: "auto",
                position: { my: "left top", at: "left+100 top-100", of: $(elem) },
                resizable: false,
                modal: true,
                buttons: {
                    "Select": function () {
                        var selectName = $('#selectPartyToCopyDialog #partyToCopy').val();
                        if (selectName == "") {
                            DisplayError("Please select a party.");
                        }
                        else {
                            $(elem).val(selectName);
                            $(this).dialog("close");
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });

            $("#partiesToCopyTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("PartiesToCopy") %>',
                jsonReader: { root: 'partiesToCopy', id: 'partyToCopy', repeatitems: false },
                colNames: ['Party'],
                colModel: [
                        { name: 'partyToCopy', width: 200, classes: "wrapped", sortable: true, search: true }
                    ],
                onSelectRow: function (id) {
                    $('#selectPartyToCopyDialog #partyToCopy').val(id);
                },
                height: '400',
                width: '200',
                loadonce: true, // Unchanging data only
                rowNum: 15,
                rowList: [15, 30, 50],
                sortname: 'partyToCopy',
                sortorder: 'asc',
                pager: '#partiesToCopyTablePager'
            });
            $("#partiesToCopyTable").jqGrid('navGrid', '#partiesToCopyTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
            $("#partiesToCopyTable").jqGrid('filterToolbar', { searchOnEnter: false });
        }

        function DisplaySelectSplitByDialog(elem) {
            $("#selectSplitByDialog").dialog({
                title: 'Select Split By Options',
                width: "auto",
                position: { my: "left top", at: "left+100 top-100", of: $(elem) },
                resizable: false,
                modal: true,
                buttons: {
                    "Select": function () {
                        var selectName = $('#selectSplitByDialog #splitBy').val();
                        if (selectName == "") {
                            DisplayError("Please select a document name.");
                        }
                        else {
                            $(elem).val(selectName);
                            $(this).dialog("close");
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });

            $("#splitByTable").jqGrid({
                datatype: 'json',
                url: '<%: Url.Action("SplitByOptions") %>',
                jsonReader: { root: 'splitByOptions', id: 'splitOption', repeatitems: false },
                colNames: ['Split By Options'],
                colModel: [
                    { name: 'splitOption', width: 200, classes: "wrapped", sortable: true, search: true }
                ],
                onSelectRow: function (id) {
                    $('#selectSplitByDialog #splitBy').val(id);
                },
                height: '400',
                width: '200',
                loadonce: true, // Unchanging data only
                rowNum: 15,
                rowList: [15, 30, 50],
                sortorder: 'asc',
                pager: '#splitByTablePager'
            });
            $("#splitByTable").jqGrid('navGrid', '#splitByTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
            $("#splitByTable").jqGrid('filterToolbar', { searchOnEnter: false });
        }

        function GetOption(id) {
            if (id == "provider")
                return id;
            return "client";
        }

        function DisplayError(errorMsg) {
            $('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
        }

        function calculatePosition(currentPage, rowNum, rowIndex) {
            return (currentPage - 1) * rowNum + rowIndex;
        }

        function DisplayFailedSavingToDB() {
            $("#savingToDatabaseDialog").dialog({
                title: 'Error',
                width: "auto",
                height: "auto",
                resizable: false,
                modal: true,
                buttons: {
                    "Retry": function () {
                        $('#oceantable_saveAll').click();
                        $(this).dialog("close");
                    },
                    "Export CSV": function () {
                        $('#oceanExportCsvForm').submit();
                        $(this).dialog("close");
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            });
        }

        $("#importCsvForm").submit(function (e) {
            e.preventDefault();

            var formData = new FormData();

            formData.append("uploadFile", $("#uploadFile")[0].files[0]);

            var xhr = new XMLHttpRequest();   // new HttpRequest instance 
            xhr.open("POST", $(this).attr("action"));
            xhr.send(formData);
            xhr.responseType = "json";
            xhr.onloadend = function () {
                if (xhr.status == 200) {
                    if (xhr.response["success"] == true) {
                        $.ehubToast("The csv file has been imported successfully.")
                    } else {
                        $.ehubToast("The csv file import has failed: " + xhr.response["message"]);
                    }
                } else {
                    $.ehubToast("Error Code: " + xhr.status + "Error Message: " + xhr.responseText);
                }
                grid.trigger("reloadGrid");
            }
        });
    </script>

    <script type="text/javascript">

        $("#uploadFile").change(UploadFileChanged);

        function UploadFileChanged() {
            if ($(this).val() == "") {
                $("#input_importcsv").button("disable");
            } else {
                $("#input_importcsv").button("enable");
            }
        };
    </script>
</asp:Content>
