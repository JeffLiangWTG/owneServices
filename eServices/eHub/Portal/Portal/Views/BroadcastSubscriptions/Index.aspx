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

	<style type="text/css">
		.query-stacked {
			display: block;
			width: 100%;
		}
	</style>
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	
	<div id="errorMessage" class="error" style="display: none"></div>
	
	<div><label for="broadcastersList">Broadcaster</label></div>
	<div>
		<div id="typesFields">
			<select id="broadcastersList" class="ui-widget ui-state-default ui-corner-all">
			</select>
			<img id="broadcastersListThrobber" src='<%: Url.Content("~/Content/images/throbber.gif") %>'
				alt='Loading ...' style="display: none" class="throbber" />
			<button id="viewBroadcasterButton" style="display: none">Details</button>
			<button id="newBroadcasterButton" >Add New</button>
		</div>
		<br />
		<div id="subscribersFields">
			<table id="subscribersTable"></table>
			<div id="subscribersTablePager"></div>
		</div>
		<div id="subscribersCsvFields" style="display: none">
			<br />
			<table>
				<tr>
					<td valign="top" style="border-right:1px solid #999; padding-right:5px">
						<form id="subscribersExportCsvForm" action="<%: Url.Action("SubscribersExportCsv") %>">
							<input type="hidden" id="exportbroadcasterPK" name="broadcasterPK" value="" />
							<input type="submit" id="subscribersExportCsv" value="Export to CSV" />
						</form>
					</td>
					<td style="padding-left:5px">
						<form id="subscribersImportCsvForm" action="<%: Url.Action("SubscribersImportCsv") %>" method="post" enctype="multipart/form-data" target="upload_target" >
							<input type="hidden" id="inputbroadcasterPK" name="broadcasterPK" value="" />
							<input type="submit" name="subscribersImportCsv" id="subscribersImportCsv" value="Import from CSV" disabled="disabled" />
							<select name="option" id="subscribersInportOption" class="ui-state-disabled ui-corner-all ui-widget" disabled="disabled">
								<option value="merge">Merge</option>
								<option value="replace">Replace</option>
							</select>
							<br />
							<input type="file" name="uploadFile" id="uploadFile" class="ui-widget ui-state-default ui-corner-all ui-button-text" size="75" />
							<iframe id="upload_target" name="upload_target" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>
                            <p id="importCsvMessage" style="display: none;"></p>
						</form>
					</td>
				</tr>
			</table>
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

	<div id="broadcasterDetailsForm" title="Broadcaster Details" style="display: none">
		<p id="errorMsg" class="ui-state-error" style="display: none"></p>
		<form action="">
			<fieldset>
				<table>
					<tbody>
						<tr>
							<td>ID</td>
							<td>
								<input type="text" name="ST_ID" id="ST_ID" class="ui-widget ui-widget-content" size="50"/>
							</td>
						</tr>
						<tr>
							<td>Name</td>
							<td>
								<input type="text" name="ST_Name" id="ST_Name" class="ui-widget ui-widget-content" size="50"/>
							</td>
						</tr>
						<tr>
							<td>Broadcast Sender ID</td>
							<td>
								<input id="CC_ID_Sender" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>
								<button id="selectClientIdSenderButton" >Select</button>
							</td>
						</tr>
                    <tr id="selectClientIdRecipientRow">
						<td>Broadcast Recipient ID</td>
						<td>
							<input id="CC_ID_Recipient" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>							
						</td>
					</tr>
					<tr id="SB_SubscriberSelectSQLRow">
						<td>Subscriber Select SQL</td>
						<td>
                            <textarea name="SB_SubscriberSelectSql" id="SB_SubscriberSelectSql" class="ui-widget ui-widget-content" rows="4" cols="50" ></textarea>
						</td>
					</tr>
					</tbody>
				</table>
				<input type="hidden" name="SB_PK" id="SB_PK"/>
			</fieldset>
		</form>
	</div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

		$(document).ready(function () {
			const DEFAULT_SORT_COLUMN = 'eHubClient_Subscriber.CC_ID';
			const DEFAULT_SORT_ORDER = 'asc';
			var broadcasterPK = "";

			$('#errorMessage').ajaxError(function (event, request, settings, exception) {
				$(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
				$(this).show();
			}).ajaxComplete(function () {
				$(this).hide()
			});

			loadbroadcastersList();

			function loadbroadcastersList() {
				broadcasterPK = "";
				$('#subscribersTable').clearGridData();
				$("#viewBroadcasterButton").hide();
				$("#broadcastersList").attr("disabled", "disabled");
				$("#broadcastersList").html("<option value=''>Loading ...<\/option>");
				$("#broadcastersListThrobber").show();
				var url = '<%: Url.Action("Broadcasters") %>';
				$.getJSON(url, null, function (data) {
					$("#broadcastersList").html("<option value=''>-- Select Type --<\/option>");
					$.each(data.eHubSubscriptionBroadcasters, function (index, optionData) {
                        $("#broadcastersList").append("<option value='" + optionData.SB_PK + "'>" + optionData.ST_ID + ' - ' + optionData.ST_Name +
                            "  [" + optionData.CC_ID_Sender + (optionData.CC_ID_Recipient == null ? "" : "-" + optionData.CC_ID_Recipient ) + "]<\/option>");
					});
					$("#broadcastersList").removeAttr("disabled");
					$("#broadcastersList").focus();
					$("#broadcastersListThrobber").hide();
				});
			}

			var broadcasterChangeHandler = function () {
				var currentPK = $("#broadcastersList > option:selected").attr("value");
				if (broadcasterPK == currentPK)
					return;
				broadcasterPK = currentPK;
				$("#subscribersTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": ""} });
				$("#subscribersCsvFields input[name*='broadcasterPK']").each(function () { $(this).val(broadcasterPK); });
				$("#subscribersImportCsvForm")[0].reset();
				UploadFileChanged();
				if (broadcasterPK != "") {
					ReloadSubscribers();
					$("#viewBroadcasterButton").show();
					$("#subscribersCsvFields").show();
				} else {
					$('#subscribersTable').clearGridData();
					$("#viewBroadcasterButton").hide();
					$("#subscribersCsvFields").hide();
				}
				return;
			}
            $("#broadcastersList").change(broadcasterChangeHandler).keypress(broadcasterChangeHandler);

			function ReloadSubscribers() {
				$('#subscribersTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("Subscribers") %>' + '?broadcaster=' + broadcasterPK,
					editurl: '<%: Url.Action("SubscriberEdit") %>' + '?broadcaster=' + broadcasterPK
				}).trigger("reloadGrid");
			}

			$("#viewBroadcasterButton").button().click(function () {
				$.getJSON('<%: Url.Action("BroadcasterInfo") %>' + "?broadcaster=" + broadcasterPK, function (data) {
				    $("#SB_PK").val(data.eHubSubscriptionBroadcaster.SB_PK);
				    $("#ST_ID").val(data.eHubSubscriptionBroadcaster.ST_ID);
				    $("#ST_Name").val(data.eHubSubscriptionBroadcaster.ST_Name);
                    $("#CC_ID_Sender").val(data.eHubSubscriptionBroadcaster.CC_ID_Sender);
                    $("#CC_ID_Recipient").val(data.eHubSubscriptionBroadcaster.CC_ID_Recipient);
					$("#SB_SubscriberSelectSql").val(data.eHubSubscriptionBroadcaster.SB_SubscriberSelectSql);
				});
				$("#SB_SubscriberSelectSQLRow").attr("hidden", false);
				$("#selectClientIdRecipientRow").attr("hidden", false);
				$("#SB_SubscriberSelectSql").attr("readonly", true);
				$('#broadcasterDetailsForm #errorMsg').hide();
				$("#broadcasterDetailsForm").dialog({
					autoOpen: true,
					height: "auto",
					width: "auto",
					modal: true,
					buttons: {
						"Save": function () {
							$.ajax({
								type: "POST",
								async: false,
								url: '<%: Url.Action("BroadcasterInfoEdit") %>',
								data: {
									oper: "edit",
									SB_PK: broadcasterPK,
									ST_ID: $("#ST_ID").val(),
									ST_Name: $("#ST_Name").val(),
                                    CC_ID_Sender: $("#CC_ID_Sender").val(),
                                    CC_ID_Recipient: $("#CC_ID_Recipient").val(),
                                    SB_SubscriberSelectSql: $("#SB_SubscriberSelectSql").val()
								},
								success: function (data) {
									$("#broadcasterDetailsForm").dialog("close");
									loadbroadcastersList();
								},
								error: function (data) {
									$('#broadcasterDetailsForm #errorMsg').text($(data.responseText)[1].innerText);
									$('#broadcasterDetailsForm #errorMsg').show();
								}
							});
						},
						"Delete": function () {
							$('<div><p class="ui-state-error">Are you sure you want to delete this subscription type?<\/p><\/div>').dialog({
								title: 'Warning',
								modal: true,
								resizable: false,
								buttons: {
									"Yes I'm Sure": function () {
										$(this).dialog("close");
										$('<div><p class="ui-state-error">Are you absolutely certain? You know you can&apos;t take this back.<\/p><\/div>').dialog({
											title: 'Warning',
											modal: true,
											resizable: false,
											buttons: {
												"Just do it already": function () {
													$.ajax({
														type: "POST",
														async: false,
														url: '<%: Url.Action("BroadcasterInfoEdit") %>',
														data: {
															oper: "del",
															SB_PK: broadcasterPK
														},
														success: function (data) {
															loadbroadcastersList();
														},
														error: function (data) {
															DisplayError($(data.responseText)[1].innerText);
														}
													});
													$(this).dialog("close");
												},
												Cancel: function () {
													$(this).dialog("close");
												}
											}
										});
									},
									Cancel: function () {
										$(this).dialog("close");
									}
								}
							});
							$(this).dialog("close");
						},
						Cancel: function () {
							$(this).dialog("close");
						}
					}
				});
			});

			$("#newBroadcasterButton").button().click(function () {
				$('#broadcasterDetailsForm #errorMsg').hide();
				$("#SB_SubscriberSelectSQLRow").attr("hidden", true);
				$("#selectClientIdRecipientRow").attr("hidden", true);
				
				$('#broadcasterDetailsForm input').val("");
				$("#broadcasterDetailsForm").dialog({
					autoOpen: true,
					height: "auto",
					width: "auto",
					modal: true,
					buttons: {
						"Create": function () {
							$.ajax({
								type: "POST",
								async: false,
								url: '<%: Url.Action("BroadcasterInfoEdit") %>' + "?type=",
								data: {
									oper: "add",
									ST_ID: $("#ST_ID").val(),
									ST_Name: $("#ST_Name").val(),
                                    CC_ID_Sender: $("#CC_ID_Sender").val()
								},
								success: function (data) {
									$("#broadcasterDetailsForm").dialog("close");
									loadbroadcastersList();
								},
								error: function (data) {
									$('#broadcasterDetailsForm #errorMsg').text($(data.responseText)[1].innerText);
									$('#broadcasterDetailsForm #errorMsg').show();
								}
							});
						},
						Cancel: function () {
							$(this).dialog("close");
						}
					}
				});
			});

            $("#selectClientIdSenderButton").button().click(function () { DisplaySelectClientDialog($("#CC_ID_Sender")) });
            $("#selectClientIdRecipientButton").button().click(function () { DisplaySelectClientDialog($("#CC_ID_Recipient")) });
            $("#clearClientIdRecipientButton").button().click(function () { $("#CC_ID_Recipient, #SB_SubscriberSelectSql").val('') });

			$("#subscribersTable").jqGrid({
				datatype: 'local',
				jsonReader: { root: 'eHubSubscriptionValues', id: 'SV_PK', repeatitems: false },
				colNames: ['SV_PK', 'Provider', 'Subscriber', 'Name', 'Subscribed', 'Archive Outbox'],
				colModel: [
					{ name: 'SV_PK', hidden: true },
					{ name: 'SV_CC_Provider', hidden: true },
					{ name: 'SV_CC_SubscriberID', index: 'eHubClient_Subscriber.CC_ID', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatClientEditFields,
							custom_value: function (elem) {
								return $(elem).children('.clientIdValue').val();
							}
						}
					},
					{ name: 'SV_CC_SubscriberName', index: 'eHubClient_Subscriber.CC_FriendlyName', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: false, editoptions: { Size: 100} },
                    { name: 'SV_SubscribedUTC', search: false, editable: false, editoptions: { Size: 50 } },
                    { name: 'SV_Value', index: 'SV_Value', classes: "wrapped", editable: true, edittype: 'select', editoptions: { value: "FALSE:FALSE;TRUE:TRUE" }, editrules: { required: true, edithidden: true }, search: false}
				],
				height: '500',
				width: '700',
				rowNum: 20,
				rowList: [20, 50, 100],
				caption: "Subscribers",
				sortable: true,
				sortname: DEFAULT_SORT_COLUMN,
				sortorder: DEFAULT_SORT_ORDER,
				pager: '#subscribersTablePager'
			});
			$("#subscribersTable").jqGrid('navGrid', '#subscribersTablePager', {},
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
					closeAfterAdd: true,
					closeOnEscape: true,
					recreateForm: true,
					resize: true,
					beforeInitData: function (formid) {
						return (broadcasterPK != "");
					},
					afterSubmit: function (response, postdata) {
						ReloadSubscribers();
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
					onReset: resetSortOrder,
					multipleSearch: true,
					showQuery: true,
					beforeShowSearch: modifySearchForm,
					onSearch: function () {
						let sortColumn = $('#sort-column').val(),
							sortOrder = $('#sort-order').val();

						$("#subscribersTable").jqGrid("setGridParam", {
							sortname: sortColumn,
							sortorder: sortOrder
						});
					}
				}
			);

			$("#uploadFile").change(UploadFileChanged);
			$("#subscribersExportCsv").button();
			$("#subscribersImportCsv").button().button("disable");

			function UploadFileChanged() {
				if ($(this).val() == "") {
					$("#subscribersImportCsv").button("disable");
					$("#subscribersInportOption").attr("disabled", "disabled");
					$("#subscribersInportOption").addClass("ui-state-disabled");
				} else {
					$("#subscribersImportCsv").button("enable");
					$("#subscribersInportOption").removeAttr("disabled");
					$("#subscribersInportOption").removeClass("ui-state-disabled");
				}
			};

			$('#upload_target').load(function () {
                var responseText = $('#upload_target')[0].contentDocument.body.innerText;
                var data = JSON.parse(responseText);
                if (data.success) {
                    $("#subscribersTable").trigger("reloadGrid");
                    if (data.archivedWarning) {
                        $('#importCsvMessage').css('color', 'orange');
                    } else {
                        $('#importCsvMessage').css('color', 'green');
                    }
                } else {
                    $('#importCsvMessage').css('color', 'red');
                }
                $('#importCsvMessage').hide();
                var message = data.message.split(".").join("<br>");
                $('#importCsvMessage').html(message);
                $('#importCsvMessage').show();
                $('#uploadFile').val('');
                $('#uploadFile').trigger('change');

            });

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
					url: '<%: Url.Action("Clients") %>' + '?includeNonProd=' + includeNonProd + '&broadcaster=' + broadcasterPK,
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
                                $('#selectClientDialog #clientID').val("");
                            }
						},
						Cancel: function () {
							$(this).dialog("close");
						}
					}
				});
				includeNonProd = clientEditState.includeNonProd;
				$("#clientsTable").jqGrid("setGridParam", {
					url: '<%: Url.Action("Clients") %>' + '?includeNonProd=' + includeNonProd + '&broadcaster=' + broadcasterPK,
				}).trigger("reloadGrid");
			}

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
                $('#query-sort').html('');
                $("#subscribersTable").jqGrid("setGridParam", {
                    sortname: DEFAULT_SORT_COLUMN,
                    sortorder: DEFAULT_SORT_ORDER
                });
            }

			function updateSortOrderInQuery() {
				let sortColumn = $('#sort-column').val();
				let sortOrder = $('#sort-order').val();
				let sortQueryCell = ($('#query-sort').length) ? $('#query-sort') : $('<td id="query-sort" class="query-stacked"/>');
				sortQueryCell.html(`ORDER BY ${sortColumn} ${sortOrder.toUpperCase()}`);
				let insertionPoint = $('td.query').first();
				insertionPoint.addClass('query-stacked');
				insertionPoint.after(sortQueryCell);
			}

			function DisplayError(errorMsg) {
				$('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
			}
		});

	</script>


</asp:Content>
