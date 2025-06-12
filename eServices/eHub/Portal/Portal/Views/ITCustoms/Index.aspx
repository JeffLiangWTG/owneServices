<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
	<link href="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.css")%>" rel="Stylesheet" type="text/css" />
	<link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
	<link href="<%: Url.Content("~/PlugIns/jqGrid460/ui.jqgrid.css")%>" rel="Stylesheet" type="text/css" media="screen" />
	<link href="<%: Url.Content("~/PlugIns/jquery-ui-timepicker-addon/1.6.3/jquery-ui-timepicker-addon.min.css")%>" rel="Stylesheet" type="text/css" media="screen" />
	<style type="text/css">
		.ui-timepicker-div dl dd {
			margin: 0 0px 0px 0%;
			width: inherit;
			padding-left: 1%;
		}
		.ui_tpicker_time_label {
			padding-top: 2% !important;
		}
	</style>

	<script src="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.min.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jqGrid460/grid.locale-en.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jqGrid460/jquery.jqGrid.min.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/JSON/json2.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jquery-ui-timepicker-addon/1.6.3/jquery-ui-timepicker-addon.min.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jquery-ui-timepicker-addon/1.6.3/i18n/jquery-ui-timepicker-addon-i18n.min.js")%>" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	
	<div id="errorMessage" class="error" style="display: none"></div>
	
	<div>
		<div id="jobStatusFields">
			<table id="jobStatusTable"></table>
			<div id="jobStatusTablePager"></div>
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

	<div id="selectClientSystemDialog" title="Select Client System" style="display: none">
		<div id="clientSystemFields">
			<table id="clientSystemTable"></table>
			<div id="clientSystemTablePager"></div>
		</div>
		<form action="">
			<input type="hidden" name="clientSystemID" id="clientSystemID" />
		</form>
	</div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
	<script type="text/javascript">

		$(document).ready(function () {
			const DEFAULT_SORT_COLUMN = 'PollingStart';
			const DEFAULT_SORT_ORDER = 'asc';

			$('#errorMessage').ajaxError(function (event, request, settings, exception) {
				$(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
				$(this).show();
			}).ajaxComplete(function () {
				$(this).hide()
			});

			function ReloadJobStatus() {
				$('#jobStatusTable').trigger("reloadGrid");
			}

			$("#jobStatusTable").jqGrid({
				datatype: 'json',
				url: '<%: Url.Action("JobStatus") %>',
				editurl: '<%: Url.Action("AddOrUpdateOrDeleteOnJobStatus") %>',
				jsonReader: { root: 'eHubITCustomsJobStatus', id: 'IT_PK', repeatitems: false },
				colNames: ['Sender ID', 'Polling Start LocalTime', 'Client System ID', 'Prod', 'File Name', 'Job ID', 'File Last Modified LocalTime', 'Last Status', "Message Type", 'Reference ID', 'Message Tracking ID', 'DeclarationContent', 'Notified Invalid Profile'],
				colModel: [
					{
						name: 'SenderID', index: 'SenderID', search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, align: 'center', sortable: true, editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatClientEditFields,
							custom_value: function (elem) {
								return $(elem).children('.clientIdValue').val();
							}
						},
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'PollingStart', index: 'PollingStart',
						search: false,
						formatter: 'date',
						formatoptions: { srcformat: 'Y-m-d H:i:s', newformat: 'Y-m-d H:i:s' },
						align: 'center',
						editoptions: {
							dataInit: function (el) {
								$(el).datetimepicker({
									controlType: 'select',
									dateFormat: "yy-mm-dd",
									timeFormat: "HH:mm:ss",

								})
							}
						},
						label: "Begin Date",
						sortable: true,
						editable: true
					},
					{
						name: 'ClientSystemID', index: 'ClientSystemID',
						search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						align: 'center',
						sortable: true,
						editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatClientSystemEditFields,
							custom_value: function (elem) {
								return $(elem).children('.clientSystemIdValue').val();
							}
						},
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'ProdInd', index: 'ProdInd', search: false, formatter: 'checkbox', align: 'center',
						sortable: false,
						edittype: 'checkbox',
						editoptions: { value: 'true:false', defaultValue: 'true' },
						editable: true
					},
					{
						name: 'FileName',
						index: 'FileName',
						search: true,
						classes: 'odd',
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						align: 'center',
						sortable: true,
						editable: true,
						searchrules: { custom: true, custom_func: validation_check }
					},
					{ name: 'JobID', index: 'JobID', search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, align: 'center', sortable: true, editable: true, searchrules: { custom: true, custom_func: validation_check } },
					{
						name: 'FileLastModified',
						index: 'FileLastModified',
						search: false, formatter: 'date',
						formatoptions: { srcformat: 'Y-m-d H:i:s', newformat: 'Y-m-d H:i:s' },
						align: 'center',
						sortable: false,
						editable: true,
						editoptions: {
							dataInit: function (el) {
								$(el).datetimepicker({
									controlType: 'select',
									dateFormat: "yy-mm-dd",
									timeFormat: "HH:mm:ss",

								})
							}
						},
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'LastStatus',
						index: 'LastStatus', search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						align: 'center',
						edittype: 'text',
						editoptions: { maxlength: 3, size: 3 },
						classes: 'odd',
						sortable: true,
						editable: true,
						searchrules: { custom: true, custom_func: validation_check },
					},
					{
						name: 'MessageType',
						index: 'MessageType', search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						align: 'center',
						classes: 'odd',
						sortable: true,
						editable: true,
						searchrules: { custom: true, custom_func: validation_check },
					},
					{ name: 'ReferenceID', index: 'ReferenceID', search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, align: 'center', sortable: true, editable: true, searchrules: { custom: true, custom_func: validation_check } },
					{
						name: 'MessageTrackingID',
						index: 'MessageTrackingID',
						search: true,
						searchoptions: { sopt: ['eq'] },
						align: 'center',
						editrules: { required: true },
						sortable: true,
						editable: true,
						classes: 'odd',
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'DeclarationContent',
						index: 'DeclarationContent',
						search: false,
						align: 'center',
						sortable: false,
						editable: true,
						classes: 'odd'
					},
					{
						name: 'NotifiedInvalidProfile', index: 'NotifiedInvalidProfile',
						search: false,
						formatter: 'checkbox',
						align: 'center',
						sortable: false,
						editable: true,
						edittype: 'checkbox',
						editoptions: { value: 'true:false', defaultValue: 'true' },
					}
				],
				height: '450',
				width: '1300',
				rowNum: 20,
				rowList: [20, 50, 100],
				caption: "IT Customs Job Status",
				sortname: DEFAULT_SORT_COLUMN,
				sortorder: DEFAULT_SORT_ORDER,
				pager: '#jobStatusTablePager'
			});

			$("#jobStatusTable").jqGrid('navGrid', '#jobStatusTablePager',
				{
				},
				{
					width: 700,
					closeAfterEdit: true,
					closeOnEscape: true,
					recreateForm: true,
					viewPagerButtons: false,
					afterSubmit: function (response, postdata) {
						var data = eval('(' + response.responseText + ')');
						return [data.success, data.message, data.id];
					},
					beforeShowForm: function () {
					}
				},
				{
					width: 700,
					closeAfterAdd: true,
					closeOnEscape: true,
					recreateForm: true,
					afterSubmit: function (response, postdata) {
						ReloadJobStatus();
						var data = eval('(' + response.responseText + ')');
						return [data.success, data.message, data.id];
					},
					beforeShowForm: function () {

					}
				},
				{
					closeOnEscape: true,
					afterSubmit: function (response, postdata) {
						var data = eval('(' + response.responseText + ')');
						return [data.success, data.message, data.id];
					}
				},
				{
					multipleSearch: true,
					showQuery: true,
					onReset: ResetSortOrder,
					beforeShowSearch: function (searchForm) {
						ModifySearchForm(searchForm)
						return true
					},
					onSearch: function () {
						let sortColumn = $('#sort-column').val(),
							sortOrder = $('#sort-order').val();

						$("#jobStatusTable").jqGrid("setGridParam", {
							sortname: sortColumn,
							sortorder: sortOrder
						});
					}
				},
			);

			var clientEditState = {
				includeNonProd: false,
				checkbox: null,
			};

			function FormatClientEditFields(value, options) {
				var checkedAttribute = '';
				var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
					'" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>' +
					' <a class="fm-button ui-state-default ui-corner-all clientIdSelect">Select</a>' +
					' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
					' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
					'</span>';
				var elem = $(elemStr)[0];
				clientEditState.checkbox = $(elem).find('.clientIdCheckbox');
				clientEditState.checkbox.change(function () {
					clientEditState.includeNonProd = clientEditState.checkbox.is(":checked");
				});

				$(elem).children('.clientIdSelect').click(function () {
					DisplaySelectClientDialog($(elem).children('.clientIdValue'), clientEditState.includeNonProd);
				});
				return elem;
			}

			var clientSystemEditState = {
				includeNonProd: false,
				checkbox: null,
			};

			function FormatClientSystemEditFields(value, options) {
				var checkedAttribute = clientSystemEditState.includeNonProd ? 'checked' : '';
				var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
					'" class="FormElement ui-widget-content ui-corner-all clientSystemIdValue" role="textbox" type="text" readonly="true"/>' +
					' <a class="fm-button ui-state-default ui-corner-all clientSystemIdSelect">Select</a>' +
					' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
					' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
					'</span>';
				var elem = $(elemStr)[0];
				$(elem).children('.clientSystemIdSelect').click(function () {
					DisplaySelectClientSystemDialog($(elem).children('.clientSystemIdValue'), clientSystemEditState.includeNonProd);
				});
				$(elem).find('.clientIdCheckbox').change(function () {
					clientSystemEditState.includeNonProd = $(this).is(':checked');
				});
				return elem;
			}

			function DisplaySelectClientDialog(elem, includeNonProd) {
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
						Cancel: function () {
							$(this).dialog("close");
						}
					}
				});
				includeNonProd = clientEditState.includeNonProd;
				$("#clientsTable").jqGrid("setGridParam", {
					url: '<%: Url.Action("Clients") %>' + '?includeNonProd=' + includeNonProd,
				}).trigger("reloadGrid");
			}

			function DisplaySelectClientSystemDialog(elem, includeNonProd) {
				$("#clientSystemTable").jqGrid({
					datatype: 'json',
					url: '<%: Url.Action("ClientSystem") %>' + '?includeNonProd=' + includeNonProd,
					jsonReader: { root: 'eHubClientSystem', id: 'EH_ID', repeatitems: false },
					colNames: ['Client System ID', 'Last Update UTC'],
					colModel: [
						{ name: 'EH_ID', width: 100, classes: "wrapped", sortable: true, search: true },
						{ name: 'EH_LastUpdateUTC', width: 100, classes: "wrapped", formatter: 'date', formatoptions: { srcformat: 'Y-m-d H:i:s', newformat: 'Y-m-d H:i:s' }, sortable: true, search: false }
					],
					onSelectRow: function (id) {
						$('#selectClientSystemDialog #clientSystemID').val(id)
					},
					height: '400',
					width: '400',
					rowNum: 15,
					rowList: [15, 30, 50],
					sortname: 'EH_ID',
					sortorder: 'asc',
					pager: '#clientSystemTablePager'
				});
				$("#clientSystemTable").jqGrid('navGrid', '#clientSystemTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
				$("#clientSystemTable").jqGrid('filterToolbar', { searchOnEnter: false });
				$("#selectClientSystemDialog").dialog({
					title: 'Select Client System',
					width: "auto",
					height: "auto",
					resizable: false,
					modal: true,
					buttons: {
						"Select": function () {
							var selectID = $('#selectClientSystemDialog #clientSystemID').val();
							if (selectID == "") {
								DisplayError("Please select a client System.");
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
				includeNonProd = clientSystemEditState.includeNonProd;
				$("#clientSystemTable").jqGrid("setGridParam", {
					url: '<%: Url.Action("ClientSystem") %>' + '?includeNonProd=' + includeNonProd,
				}).trigger("reloadGrid");
			}

			function DisplayError(errorMsg) {
				$('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
			}

			function validation_check(value) {
				if (value == '') {
					return [false, "Value cannot be null"];
				}

				return [true, ""];
			}

			function ModifySearchForm(form) {
				let sortTable = $('<table/>')
				let headerRow = $('<tr/>')
				headerRow.append('<th>Order By</th>')
				let addCell = $('<td/>')
				let sortColumns = $('td.columns').first().find('select').clone().attr('id', 'sort-column')
				sortColumns.change(function () {
					UpdateQuery()
				})
				let sortOrder = $('<select>'
					+ '<option value="asc">Ascending</option >'
					+ '<option value="desc">Decending</option >'
					+ '</select >').attr('id', 'sort-order')
				sortOrder.change(function () {
					UpdateQuery()
				})
				headerRow.append(addCell)
				sortTable.append(headerRow)
				sortTable.append($('<tr/>').append($('<td/>').append(sortColumns)).append($('<td/>').append(sortOrder)))

				form.append(sortTable)
			}

            function ResetSortOrder() {
                $('#sort-order').val(DEFAULT_SORT_ORDER);
                $('#sort-column').val(DEFAULT_SORT_COLUMN);
                $('td.query-sort').html('');
                $("#jobStatusTable").jqGrid("setGridParam", {
                    sortname: DEFAULT_SORT_COLUMN,
                    sortorder: DEFAULT_SORT_ORDER
                });
            }

			function UpdateQuery() {
				let sortColumn = $('#sort-column').val()
				let sortOrder = $('#sort-order').val()

				let sortQueryCell = ($('td.query-sort').length) ? $('td.query-sort') : $('<td class="query-sort"/>')
				sortQueryCell.html(' ORDER By ' + sortColumn + ' ' + sortOrder)
				$(sortQueryCell).insertAfter($('td.query').first())
			}
		});
	</script>
</asp:Content>
