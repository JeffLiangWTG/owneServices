<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
	<style type="text/css">
		table.EditTable td {
			vertical-align: middle !important
		}
	</style>
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
	<div id="errorMessage" class="error" style="display: none">
	</div>

	<div id="registryFields">
		<table id="registryTable">
		</table>
		<div id="registryTablePager">
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
			const DEFAULT_SORT_COLUMN = 'CC_ID';
			const DEFAULT_SORT_ORDER = 'asc';

			$('#errorMessage').ajaxError(function (event, request, settings, exception) {
				$(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
				$(this).show();
			}).ajaxComplete(function () {
				$(this).hide()
			});

			$("#registryTable").jqGrid({
				datatype: 'json',
				url: '<%: Url.Action("Registry") %>',
				editurl: '<%: Url.Action("RegistryEdit") %>',
				jsonReader: {
					root: 'eHubUSCustomsRegistry',
					id: 'id',
					repeatitems: false
				},
				colNames: ['id', 'eHubID', 'Name', 'Recipient', 'AES', 'Prod', 'ABI', 'Prod', 'ISF', 'Prod', 'AMS (Sea)', 'Prod', 'AMS (Air)', 'Prod', 'AMS (Truck)', 'Prod', 'UEM', 'Prod'],
				colModel: [
					{ name: 'id', index: 'id', hidden: true },
					{
						name: 'CC_ID',
						index: 'CC_ID',
						width: 200,
						editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatClientEditFields,
							custom_value: function (elem) {
								return $(elem).children('.clientIdValue').val();
							}
						},
						sortable: true,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false
						}
					},
					{
						name: 'CC_FriendlyName',
						index: 'CC_FriendlyName',
						width: 800,
						sortable: true,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						}
					},
					{
						name: 'CC_USCustomsRecipient',
						formatter: 'checkbox',
						width: 150,
						edittype: 'checkbox',
						align: 'center',
						editable: true,
						editoptions: { value: 'true:false', defaultValue: 'true' },
						sortable: false,
						search: false
					},
					{
						name: 'USE',
						classes: 'odd',
						width: 150,
						editable: true,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.USE').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'USE_IsProduction',
						classes: 'odd',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'center',
						editable: true,
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						sortable: false,
						search: false
					},
					{
						name: 'USI',
						editable: true,
						width: 100,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.USI').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'USI_IsProduction',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'center',
						editable: true,
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						sortable: false,
						search: false
					},
					{
						name: 'ISF',
						classes: 'odd',
						editable: true,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.ISF').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'ISF_IsProduction',
						classes: 'odd',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'center',
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						editable: true,
						sortable: false,
						search: false
					},
					{
						name: 'AMS',
						width: 200,
						editable: true,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.AMS').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'AMS_IsProduction',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'right',
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						editable: true,
						sortable: false,
						search: false
					},
					{
						name: 'AMA',
						width: 150,
						editable: true,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.AMA').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'AMA_IsProduction',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'right',
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						editable: true,
						sortable: false,
						search: false
					},
					{
						name: 'MAN',
						width: 200,
						classes: 'odd',
						editable: true,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.MAN').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'MAN_IsProduction',
						classes: 'odd',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'center',
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						editable: true,
						sortable: false,
						search: false
					},
					{
						name: 'UEM',
						width: 150,
						classes: 'odd',
						editable: true,
						sortable: false,
						search: true,
						searchoptions: {
							sopt: ['bw', 'ew', 'eq', 'cn']
						},
						edittype: 'custom', editoptions: {
							custom_element: FormatSCACFields,
							custom_value: function (elem) {
								return $(elem).find('input.UEM').map(function () {
									return $(this).val();
								}).get().join(",");
							}
						}
					},
					{
						name: 'UEM_IsProduction',
						classes: 'odd',
						width: 100,
						formatter: 'checkbox',
						edittype: 'checkbox',
						align: 'center',
						editoptions: { value: 'true:false', defaultValue: 'false', disabled: true },
						editable: true,
						sortable: false,
						search: false
					}
				],
				height: '250',
				width: '1300',
				rowNum: 10,
				rowList: [10, 20, 50],
				caption: "US Customs Registry",
				sortable: true,
				sortname: DEFAULT_SORT_COLUMN,
				sortorder: DEFAULT_SORT_ORDER,
				pager: '#registryTablePager'
			});

			$("#registryTable").jqGrid('navGrid', '#registryTablePager',
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
						$('#tr_USE').append($('#tr_USE_IsProduction > td'));
						$('#tr_USI').append($('#tr_USI_IsProduction > td'));
						$('#tr_ISF').append($('#tr_ISF_IsProduction > td'));
						$('#tr_AMS').append($('#tr_AMS_IsProduction > td'));
						$('#tr_AMA').append($('#tr_AMA_IsProduction > td'));
						$('#tr_MAN').append($('#tr_MAN_IsProduction > td'));
						$('#tr_UEM').append($('#tr_UEM_IsProduction > td'));

						$('#USE_IsProduction').css("margin-top", "10px");
						$('#USI_IsProduction').css("margin-top", "10px");
						$('#ISF_IsProduction').css("margin-top", "10px");
						$('#AMS_IsProduction').css("margin-top", "10px");
						$('#AMA_IsProduction').css("margin-top", "10px");
						$('#MAN_IsProduction').css("margin-top", "10px");
						$('#UEM_IsProduction').css("margin-top", "10px");
					}
				},
				{
					width: 700,
					closeAfterAdd: true,
					closeOnEscape: true,
					recreateForm: true,
					afterSubmit: function (response, postdata) {
						var data = eval('(' + response.responseText + ')');
						return [data.success, data.message, data.id];
					},
					beforeShowForm: function () {
						$('#tr_USE').append($('#tr_USE_IsProduction > td'));
						$('#tr_USI').append($('#tr_USI_IsProduction > td'));
						$('#tr_ISF').append($('#tr_ISF_IsProduction > td'));
						$('#tr_AMS').append($('#tr_AMS_IsProduction > td'));
						$('#tr_AMA').append($('#tr_AMA_IsProduction > td'));
						$('#tr_MAN').append($('#tr_MAN_IsProduction > td'));
						$('#tr_UEM').append($('#tr_UEM_IsProduction > td'));

						$('#USE_IsProduction').css("margin-top", "10px");
						$('#USI_IsProduction').css("margin-top", "10px");
						$('#ISF_IsProduction').css("margin-top", "10px");
						$('#AMS_IsProduction').css("margin-top", "10px");
						$('#AMA_IsProduction').css("margin-top", "10px");
						$('#MAN_IsProduction').css("margin-top", "10px");
						$('#UEM_IsProduction').css("margin-top", "10px");
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
					onReset: ResetSortOrder,
					multipleSearch: true,
					showQuery: true,
					beforeShowSearch: function (searchForm) {
						ModifySearchForm(searchForm)
						return true
					},
					onSearch: function () {
						let sortColumn = $('#sort-column').val(),
							sortOrder = $('#sort-order').val();

						$("#registryTable").jqGrid("setGridParam", {
							sortname: sortColumn,
							sortorder: sortOrder
						});
					}
				},
			);

			function DoesClientHavePrdLicence(ehubId) {
				return (!(ehubId === ""))
					? $.ajax({
						type: 'GET',
						url:  '<%: Url.Action("DoesClientHavePrdLicence") %>',
						data: { ehubId: ehubId },
						dataType: "html",
						global: false,
						async: false,
						success: (data) => { return data; }
					}).responseText === "True"
					: false;
			}

			function FormatSCACFields(value, options) {
				var values = value.split(",");
				var elemStr = '<div id="' + options.id + '_values" style="max-width:500px; display: inline-block; margin-right:5px; vertical-align: top;">';
				for (index = 0; index < values.length; ++index) {
					elemStr += '<div style="width:80px; display: inline-block; margin-right:5px; vertical-align: top; padding-top:10px;">'
						+ '<input class="FormElement ui-widget-content ui-corner-all ' + options.id + '" role="textbox" type="text" style="width:80px; padding-top:10px; height: 5px; padding-bottom:6px;"';
					if (values[index]) elemStr += ' value="' + values[index] + '"';
					elemStr += index > 0 ? '/><center><a style="margin-top:10px; margin-bottom:-5px; margin-top:-1px;" class="ui-icon ui-icon-circle-minus removeInput clickable" align="center"></a></center></div>'
						: '/></div>';
				}
				elemStr += '<a style="display: inline-block; margin-top:10px; margin-left:5px" class="ui-icon ui-icon-circle-plus add clickable"></a></div>';

				var elem = $(elemStr)[0];
				$(elem).children('a.add').click(function (event) {
					var beforeDiv = $('<div style="width:80px; display: inline-block; margin-right:5px; vertical-align: top; padding-top:10px;">'
						+ '<input class="FormElement ui-widget-content ui-corner-all ' + options.id + '" role="textbox" type="text" style="padding-top:10px; height: 5px; width:100%; padding-bottom:6px;"/>'
						+ '<center><a style="margin-top:10px; margin-bottom:-5px; margin-top:-1px;" class="ui-icon ui-icon-circle-minus removeInput clickable" align="center"></a></center></div>')[0];
					$(this).before(beforeDiv);
					$(beforeDiv).children('center').children('a.removeInput').click(function (event) {
						$(this).closest('div').remove();
					});
				});
				$(elem).children('div').children('center').children('a.removeInput').click(function (event) {
					$(this).closest('div').remove();
				});
				return elem;
			}

			var clientEditState = {
				includeNonProd: false,
				checkbox: null,
			};

			function FormatClientEditFields(value, options) {
				var checkedAttribute = '';

				var elemStr = '<span>' +
					'<input id="' + options.id + '_val" value="' + value + '" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>' +
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
								UpdateIsProductionCheckboxes(selectID);
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

			function DisplayError(errorMsg) {
				$('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
			}

			function UpdateIsProductionCheckboxes(ehubClientID) {
				var isProduction = DoesClientHavePrdLicence(ehubClientID);
				document.getElementById("USE_IsProduction").checked = isProduction;
				document.getElementById("USI_IsProduction").checked = isProduction;
				document.getElementById("ISF_IsProduction").checked = isProduction;
				document.getElementById("AMS_IsProduction").checked = isProduction;
				document.getElementById("AMA_IsProduction").checked = isProduction;
				document.getElementById("MAN_IsProduction").checked = isProduction;
				document.getElementById("UEM_IsProduction").checked = isProduction;
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
                $("#registryTable").jqGrid("setGridParam", {
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
