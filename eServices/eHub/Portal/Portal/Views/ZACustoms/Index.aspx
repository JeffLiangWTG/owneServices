<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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
	
	<div id="errorMessage" class="error" style="display: none"></div>
	
	<div><label for="registrationTypesList">Client Registrations</label></div>
	<div>
		<div id="registrationsFields">
			<table id="registrationsTable"></table>
			<div id="registrationsTablePager"></div>
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
			const DEFAULT_SORT_COLUMN = 'CX_CC_ID';
			const DEFAULT_SORT_ORDER = 'asc';

	        $('#errorMessage').ajaxError(function (event, request, settings, exception) {
	            $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
	            $(this).show();
	        }).ajaxComplete(function () {
	            $(this).hide()
	        });

	        function ReloadRegistrations() {
	            $('#registrationsTable').trigger("reloadGrid");
	        }

	        $("#registrationsTable").jqGrid({
	            datatype: 'json',
	            url: '<%: Url.Action("Registrations") %>',
	            editurl: '<%: Url.Action("RegistrationEdit") %>',
	            jsonReader: { root: 'eHubClientRegistrations', id: 'CX_PK', repeatitems: false },
	            colNames: ['CX_PK', 'Client ID', 'Agent ID', 'Sender ID', 'Sender Sub-ID', 'Trading Partner ID', 'AACID'],
	            colModel: [
					{ name: 'CX_PK', hidden: true },
					{ name: 'CX_CC_ID', index: 'CX_CC_ID', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true,
					    edittype: 'custom',
					    editoptions: {
					        custom_element: FormatClientEditFields,
					        custom_value: function (elem) {
					            return $(elem).children('.clientIdValue').val();
					        }
					    }
					},
					{ name: 'CX_Code', index: 'CX_Code', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true, editoptions: { Size: 40} },
                    { name: 'SenderID', index: 'SenderID', classes: "wrapped", hidden: true, search: false, editable: true, editoptions: { Size: 40 }, editrules: { edithidden: true} },
                    { name: 'SenderSubID', index: 'SenderSubID', classes: "wrapped", hidden: true, search: false, editable: true, editoptions: { Size: 40 }, editrules: { edithidden: true} },
                    { name: 'TradingPartnerID', index: 'TradingPartnerID', classes: "wrapped", hidden: true, search: false, editable: true, editoptions: { Size: 40 }, editrules: { edithidden: true} },
                    { name: 'AACID', index: 'AACID', classes: "wrapped", hidden: true, search: false, editable: true, editoptions: { Size: 40 }, editrules: { edithidden: true} },
				],
	            height: '500',
	            width: '400',
	            rowNum: 20,
	            rowList: [20, 50, 100],
	            caption: "Registrations",
	            sortable: true,
	            sortname: DEFAULT_SORT_COLUMN,
	            sortorder: DEFAULT_SORT_ORDER,
	            pager: '#registrationsTablePager'
	        });
	        $("#registrationsTable").jqGrid('navGrid', '#registrationsTablePager', {},
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
				        ReloadRegistrations();
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

						$("#registrationsTable").jqGrid("setGridParam", {
							sortname: sortColumn,
							sortorder: sortOrder
						});
					}
				}
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
	                    "Cancel": function () {
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
                $("#registrationsTable").jqGrid("setGridParam", {
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
