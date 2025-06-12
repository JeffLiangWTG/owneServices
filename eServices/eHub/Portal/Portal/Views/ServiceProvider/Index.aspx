<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.css")%>"
        rel="Stylesheet" type="text/css" />
    <link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>"
        rel="Stylesheet" type="text/css" media="screen" />
    <link href="<%: Url.Content("~/PlugIns/jqGrid460/ui.jqgrid.css")%>" rel="Stylesheet"
        type="text/css" media="screen" />
    <script src="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.min.js")%>"
        type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/jqGrid460/grid.locale-en.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/jqGrid460/jquery.jqGrid.min.js")%>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.js")%>"
        type="text/javascript"></script>
    <script src="<%: Url.Content("~/PlugIns/JSON/json2.js")%>" type="text/javascript"></script>
    
    <style type="text/css">
		.query-sort {
			padding-left: 2px;
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="errorMessage" class="error" style="display: none">
    </div>
    <div>
        <br />
        <div id="servicesFields">
            <table id="servicesTable">
            </table>
            <div id="servicesTablePager">
            </div>
        </div>
    </div>
    <div id="selectProviderDialog" title="Select Provider" style="display: none">
        <div id="providersFields">
            <table id="providersTable">
            </table>
            <div id="providersTablePager">
            </div>
        </div>
        <form action="">
        <input type="hidden" name="providerID" id="providerID" />
        </form>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        const DEFAULT_SORT_COLUMN = 'Provider';
        const DEFAULT_SORT_ORDER = 'asc';
    
        $(document).ready(function () {
            var clientPK = "";
            var colNames = [];
            var colModel = [];

            var getUrlParameter = function getUrlParameter(sParam) {
                var sPageURL = decodeURIComponent(window.location.search.substring(1)),
                sURLVariables = sPageURL.split('&'),
                sParameterName,
                i;

                for (i = 0; i < sURLVariables.length; i++) {
                    sParameterName = sURLVariables[i].split('=');

                    if (sParameterName[0] === sParam) {
                        return sParameterName[1] === undefined ? true : sParameterName[1];
                    }
                }
            };

            var providerFromUrl = getUrlParameter('Provider');

            $.getJSON('<%: Url.Action("GetColumnNames") %>', function (d) {
                colNames.pushArray(d.names);
                for (i = 0; i < d.names.length; i++) {
                    if (d.names[i] === "Provider") {
                        colModel.unshift({ name: d.names[i], index: d.names[i], width: 100, classes: "wrapped", search: true, key: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true, sortable: true, edittype: 'custom', editoptions: {
                            custom_element: FormatEditFields,
                            custom_value: function (elem) {
                                return $(elem).find('.providerIdValue').val();
                            }
                        }
                        }
                        );
                    }
                    else {
                        colModel.push({ name: d.names[i], index: d.names[i], width: 80, formatter: 'checkbox', edittype: 'checkbox', align: 'center', sortable: true, search: true, searchoptions: { sopt: ['eq'], value: ":All;1:Yes;0:No" }, stype: 'select', editable: true });
                    }
                }
                LoadGrid();
            });

            $('#errorMessage').ajaxError(function (event, request, settings, exception) {
                $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
                $(this).show();
            }).ajaxComplete(function () {
                $(this).hide()
            });

            function FormatEditFields(value, options) {
                console.log(options);
                var idVal = "providerIdValue";
                var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
								'" class="FormElement ui-widget-content ui-corner-all ' + idVal + '" role="textbox" type="text" readonly="true"/>' +
								' <a class="fm-button ui-state-default ui-corner-all idSelect">Select</a>';
                var elem = $(elemStr)[0];
                $(elem).children('.idSelect').click(function () {
                    DisplaySelectDialog($(elem).children('.' + idVal), options.id);
                });
                return elem;
            }

            function DisplaySelectDialog(elem) {
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
                $("#providersTable").jqGrid({
                    datatype: 'json',
                    url: '<%: Url.Action("CW1Clients") %>',
                    jsonReader: { root: 'serviceProvider', id: 'Provider_CC_ID', repeatitems: false },
                    colNames: ['Service Provider'],
                    colModel: [
						    { name: 'Provider_CC_ID', width: 200, classes: "wrapped", sortable: true, search: true }
					    ],
                    onSelectRow: function (id) {
                        $('#selectProviderDialog #providerID').val(id)
                    },
                    height: '400',
                    width: '200',
                    rowNum: 15,
                    rowList: [15, 30, 50],
                    sortname: DEFAULT_SORT_COLUMN,
                    sortorder: DEFAULT_SORT_ORDER,
                    pager: '#providersTablePager'
                });
                $("#providersTable").jqGrid('navGrid', '#providersTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
                $("#providersTable").jqGrid('filterToolbar', { searchOnEnter: false });
            }

            function ReloadServices() {
                $('#servicesTable').clearGridData();
                $("#servicesTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": ""} });
                $('#servicesTable').setGridParam({
                    datatype: 'json',
                    url: '<%: Url.Action("GetProvidersWithServices") %>',
                    editurl: '<%: Url.Action("ServiceProviderEdit") %>'
                }).trigger("reloadGrid");
            }

            function ReloadProviders() {
                $('#providersTable').clearGridData();
                $("#providersTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": ""} });
                $('#providersTable').setGridParam({
                    datatype: 'json',
                    url: '<%: Url.Action("ServiceProviders") %>'
                }).trigger("reloadGrid");
            }

            function LoadGrid() {
                $("#servicesTable").jqGrid({
                    datatype: 'local',
                    jsonReader: { root: 'Services', id: 'id', repeatitems: false },
                    colNames: colNames,
                    colModel: colModel,
                    height: '500',
                    width: '400',
                    rowNum: 20,
                    rowList: [20, 50, 100],
                    caption: "Services",
                    sortable: true,
                    sortname: DEFAULT_SORT_COLUMN,
                    sortorder: DEFAULT_SORT_ORDER,
                    shrinkToFit: false,
                    pager: '#servicesTablePager'
                });
                $('#servicesTable').setGridParam({
                    datatype: 'json',
                    url: '<%: Url.Action("GetProvidersWithServices") %>',
                    editurl: '<%: Url.Action("ServiceProviderEdit") %>',
                    postData: { providerFromUrl: providerFromUrl }
                }).trigger("reloadGrid");
                $("#servicesTable").jqGrid('filterToolbar', { searchOnEnter: false });
                $("#servicesTable").jqGrid('navGrid', '#servicesTablePager', {},
				{
				    recreateForm: true,
				    width: 400,
				    closeAfterEdit: true,
				    closeOnEscape: true,
				    recreateForm: true,
				    viewPagerButtons: true,
				    resize: true,
				    beforeShowForm: function (form) { $('a.idSelect', form).hide(); },
				    afterSubmit: function (response, postdata) {
				        ReloadServices();
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
				        ReloadServices();
				        ReloadProviders();
				        var data = eval('(' + response.responseText + ')');
				        return [data.success, data.message, data.id];
				    }
				},
				{
				    closeOnEscape: true,
				    afterSubmit: function (response, postdata) {
				        ReloadServices();
				        var data = eval('(' + response.responseText + ')');
				        return [data.success, data.message, data.id];
				    }
				},
                {
                    onReset: resetSortOrder,
                    multipleSearch: true,
                    showQuery: true,
                    beforeShowSearch: modifySearchForm,
                    onSearch: function () {
                        let sortColumn = $('#sort-column').val(),
                            sortOrder = $('#sort-order').val();
                        $('#servicesTable').jqGrid("setGridParam", {
                            sortname: sortColumn,
                            sortorder: sortOrder
                        });
                    }
                }
			);
            }
        });

        function modifySearchForm(searchForm) {
            let sortTable = $('<table/>');
            let headerRow = $('<tr/>');
            let addCell = $('<td/>');
            let sortColumns = $('td.columns').first().find('select').clone().attr('id', 'sort-column');
            sortColumns.change(updateSortOrderInQuery);
            let sortOrder = $('<select>'
                + '<option value="asc">Ascending</option >'
                + '<option value="desc">Descending</option >'
                + '</select >').attr('id', 'sort-order');
            sortOrder.change(updateSortOrderInQuery);
            headerRow.append('<th>Order By</th>');
            headerRow.append(addCell);
            sortTable.append(headerRow);
            sortTable.append($('<tr/>').append($('<td/>').append(sortColumns)).append($('<td/>').append(sortOrder)));
            searchForm.append(sortTable);

            return true;
        }

        function resetSortOrder() {
            $('#sort-order').val(DEFAULT_SORT_ORDER);
            $('#sort-column').val(DEFAULT_SORT_COLUMN);
            $('td.query-sort').html('');
            $("#servicesTable").jqGrid("setGridParam", {
                sortname: DEFAULT_SORT_COLUMN,
                sortorder: DEFAULT_SORT_ORDER
            });
        }

        function updateSortOrderInQuery() {
            let sortColumn = $('#sort-column').val();
            let sortOrder = $('#sort-order').val();
            let sortQueryCell = ($('td.query-sort').length) ? $('td.query-sort') : $('<td class="query-sort"/>');
            sortQueryCell.html(`ORDER BY ${sortColumn} ${sortOrder.toUpperCase()}`);
            $(sortQueryCell).insertAfter($('td.query').first());
        }

        Array.prototype.pushArray = function (arr) {
            this.push.apply(this, arr);
        }
    </script>
</asp:Content>
