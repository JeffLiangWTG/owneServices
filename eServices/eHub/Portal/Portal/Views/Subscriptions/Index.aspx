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
	
	<div><label for="typesList">Subscription Type</label></div>
	<div>
		<div id="typesFields">
			<select id="typesList" class="ui-widget ui-state-default ui-corner-all">
			</select>
			<img id="typesListThrobber" src='<%: Url.Content("~/Content/images/throbber.gif") %>'
				alt='Loading ...' style="display: none" class="throbber" />
			<button id="viewTypeButton" style="display: none">Details</button>
			<button id="newTypeButton" >Add New</button>
			<a href="http://intranet/development/eServices/Development/HOWTO%20-%20Configure%20Subscriptions.docx" class="ui-widget" style="float:right;padding-top:15px">User Guide</a>
		</div>
		<br />
		<div id="autoSubscribesFields">
			<table id="autoSubscribesTable"></table>
			<div id="autoSubscribesTablePager"></div>
		</div>
		<br />
		<div id="lookupsFields">
			<table id="lookupsTable"></table>
			<div id="lookupsTablePager"></div>
		</div>
		<br />
		<div id="valuesFields">
			<table id="valuesTable"></table>
			<div id="valuesTablePager"></div>
		</div>
		<div id="valuesCsvFields" style="display: none">
			<br />
			<table>
				<tr>
					<td valign="top" style="border-right:1px solid #999; padding-right:5px">
						<form id="valuesExportCsvForm" action="<%: Url.Action("ValuesExportCsv") %>">
							<input type="hidden" id="exportTypePK" name="typePK" value="" />
							<input type="submit" id="valuesExportCsv" value="Export to CSV" />
						</form>
					</td>
					<td style="padding-left:5px">
						<form id="valuesImportCsvForm" action="<%: Url.Action("ValuesImportCsv") %>" method="post" enctype="multipart/form-data" target="upload_target" >
							<input type="hidden" id="inputTypePK" name="typePK" value="" />
							<input type="submit" name="valuesImportCsv" id="valuesImportCsv" value="Import from CSV" disabled="disabled" />
							<select name="option" id="valuesInportOption" class="ui-state-disabled ui-corner-all ui-widget" disabled="disabled">
								<option value="merge">Merge</option>
								<option value="replace">Replace</option>
							</select>
							<br />
							<input type="file" name="uploadFile" id="uploadFile" class="ui-widget ui-state-default ui-corner-all ui-button-text" size="75" />
							<iframe id="upload_target" name="upload_target" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>
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

	<div id="selectMessageTypesDialog" title="Select Message Type" style="display: none">
		<div id="messageTypesFields">
			<table id="messageTypesTable"></table>
			<div id="messageTypesTablePager"></div>
		</div>
		<form action="">
			<input type="hidden" name="messageType" id="messageType" />
		</form>
	</div>

	<div id="subscriptionTypeForm" title="Subscription Details" style="display: none">
		<p id="errorMsg" class="ui-state-error" style="display: none"></p>
		<form action="">
			<fieldset>
				<table>
					<tbody>
						<tr>
							<td>ID</td>
							<td>
								<input type="text" name="ST_ID" id="ST_ID" class="ui-widget ui-widget-content" size="10"/>
							</td>
						</tr>
						<tr>
							<td>Name</td>
							<td>
								<input type="text" name="ST_Name" id="ST_Name" class="ui-widget ui-widget-content" size="50"/>
							</td>
						</tr>
						<tr>
							<td>Expiry Days</td>
							<td>
								<input type="text" name="ST_ExpiryDays" id="ST_ExpiryDays" class="ui-widget ui-widget-content" size="10"/>
							</td>
						</tr>
					</tbody>
				</table>
				<input type="hidden" name="ST_PK" id="ST_PK"/>
			</fieldset>
		</form>
	</div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

		$(document).ready(function () {
			const DEFAULT_SORT_COLUMN = 'eHubClient_Subscribed.CC_ID';
			const DEFAULT_SORT_ORDER = 'desc';
			var typePK = "";

			$('#errorMessage').ajaxError(function (event, request, settings, exception) {
				$(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
				$(this).show();
			}).ajaxComplete(function () {
				$(this).hide()
			});

			loadTypesList();

			function loadTypesList() {
				typePK = "";
				$('#autoSubscribesTable').clearGridData();
				$('#lookupsTable').clearGridData();
				$('#valuesTable').clearGridData();
				$("#viewTypeButton").hide();
				$("#typesList").attr("disabled", "disabled");
				$("#typesList").html("<option value=''>Loading ...<\/option>");
				$("#typesListThrobber").show();
				var url = '<%: Url.Action("Types") %>';
				$.getJSON(url, null, function (data) {
					$("#typesList").html("<option value=''>-- Select Type --<\/option>");
					$.each(data.eHubSubscriptionTypes, function (index, optionData) {
						$("#typesList").append("<option value='" + optionData.ST_PK + "'>" + optionData.ST_ID + " - " + optionData.ST_Name + "<\/option>");
					});
					$("#typesList").removeAttr("disabled");
					$("#typesList").focus();
					$("#typesListThrobber").hide();
				});
			}

			var typeChangeHandler = function () {
				var currentPK = $("#typesList > option:selected").attr("value");
				if (typePK == currentPK)
					return;
				typePK = currentPK;
				$("#valuesTable").GridUnload();
				initGrid();
				$("#autoSubscribesTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": "" } });
				$("#lookupsTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": "" } });
				$("#valuesTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": "" } });
				$("#valuesCsvFields input[name*='typePK']").each(function () { $(this).val(typePK); });
				$("#valuesImportCsvForm")[0].reset();
				UploadFileChanged();
				ReloadAutoSubscribes();
				ReloadLookups();
				if (typePK !== "") {
					$('#valuesTable').jqGrid('hideCol', "SV_ST");
					$("#viewTypeButton").show();
					$("#valuesCsvFields").show();
				} else {
					$('#valuesTable').jqGrid('showCol', "SV_ST");
					$("#viewTypeButton").hide();
					$("#valuesCsvFields").hide();
				}
				ReloadValues();
				return;
			}
			$("#typesList").change(typeChangeHandler).keypress(typeChangeHandler);

			initGrid();

			function initGrid() {
				SetUpValuesTableGridBody();
				sessionStorage.setItem("hasUserInputChanged", "false");
				SetUpValuesGridNav();
			}

			function ReloadAutoSubscribes() {
				$('#autoSubscribesTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("AutoSubscribes") %>' + '?type=' + typePK,
					editurl: '<%: Url.Action("AutoSubscribesEdit") %>' + '?type=' + typePK
				}).trigger("reloadGrid");
			}
			function ReloadLookups() {
				$('#lookupsTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("Lookups") %>' + '?type=' + typePK,
					editurl: '<%: Url.Action("LookupsEdit") %>' + '?type=' + typePK
				}).trigger("reloadGrid");
			}
			function ReloadValues() {
				$('#valuesTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("Values") %>' + '?type=' + typePK,
					editurl: '<%: Url.Action("ValuesEdit") %>' + '?type=' + typePK
				}).trigger("reloadGrid");
			}

			$("#viewTypeButton").button().click(function() {
				$.getJSON('<%: Url.Action("TypeInfo") %>' + "?type=" + typePK, function (data) {
					$("#ST_ID").val(data.eHubSubscriptionType.ST_ID);
					$("#ST_Name").val(data.eHubSubscriptionType.ST_Name);
					$("#ST_ExpiryDays").val(data.eHubSubscriptionType.ST_ExpiryDays);
				});
				$('#subscriptionTypeForm #errorMsg').hide();
				$( "#subscriptionTypeForm" ).dialog({
					autoOpen: true,
					height: "auto",
					width: "auto",
					modal: true,
					buttons: {
						"Save": function() {
							$.ajax({
								type: "POST",
								async: false,
								url: '<%: Url.Action("TypeInfoEdit") %>',
								data: {
									oper: "edit",
									ST_PK: typePK,
									ST_ID: $("#ST_ID").val(),
									ST_Name: $("#ST_Name").val(),
									ST_ExpiryDays: $("#ST_ExpiryDays").val()
								},
								success: function (data) {
									$("#subscriptionTypeForm").dialog( "close" );
									loadTypesList();
								},
								error: function (data) {
									$('#subscriptionTypeForm #errorMsg').text($(data.responseText)[1].innerText);
									$('#subscriptionTypeForm #errorMsg').show();
								}
							});
						},
						"Delete": function() {
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
														url: '<%: Url.Action("TypeInfoEdit") %>',
														data: {
															oper: "del",
															ST_PK: typePK
														},
														success: function (data) {
															loadTypesList();
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
							$( this ).dialog( "close" );
						},
						Cancel: function() {
							$( this ).dialog( "close" );
						}
					}
				});
			});

			$("#newTypeButton").button().click(function() {
				$('#subscriptionTypeForm #errorMsg').hide();
				$('#subscriptionTypeForm input').val("");
				$("#subscriptionTypeForm").dialog({
					autoOpen: true,
					height: "auto",
					width: "auto",
					modal: true,
					buttons: {
						"Create": function() {
							$.ajax({
								type: "POST",
								async: false,
								url: '<%: Url.Action("TypeInfoEdit") %>' + "?type=",
								data: {
									oper: "add",
									ST_ID: $("#ST_ID").val(),
									ST_Name: $("#ST_Name").val(),
									ST_ExpiryDays: $("#ST_ExpiryDays").val()
								},
								success: function (data) {
									$("#subscriptionTypeForm").dialog( "close" );
									loadTypesList();
								},
								error: function (data) {
									$('#subscriptionTypeForm #errorMsg').text($(data.responseText)[1].innerText);
									$('#subscriptionTypeForm #errorMsg').show();
								}
							});
						},
						Cancel: function() {
							$( this ).dialog( "close" );
						}
					}
				});
			});

			$("#autoSubscribesTable").jqGrid({
				datatype: 'local',
				jsonReader: {
					root: 'eHubSubscriptionAutoSubscribes',
					id: 'SA_PK',
					repeatitems: false
				},
				colNames: ['SA_PK', 'Message Type', 'Recipient', 'Value XPath', 'Value Property', 'Ref XPath', 'Value Property'],
				colModel: [
					{ name: 'SA_PK', hidden: true, editable: true },
					{ name: 'DT_Code',  classes: "wrapped", width: 250, sortable: false, editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatMessageTypeEditFields,
							custom_value: function(elem) {
								return $(elem).children('.messageTypeValue').val();
							}
						}
					},
					{ name: 'CC_ID_Recipient', classes: "wrapped", width: 200, sortable: false, editable: true,
						hidden: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatClientEditFieldsSubscriber,
							custom_value: function(elem) {
								return $(elem).children('.clientIdValue').val();
							}
						}
					},
					{ name: 'SA_ValueXpath', classes: "wrapped", width: 250, sortable: false, editable: true, editoptions: { Size: 130 } },
					{ name: 'SA_ValueProperty', classes: "wrapped", width: 250, sortable: false, editable: true, editoptions: { Size: 130 } },
					{ name: 'SA_ReferenceXpath', classes: "wrapped", width: 250, sortable: false, editable: true, editoptions: { Size: 130 } },
					{ name: 'SA_ReferenceProperty', classes: "wrapped", width: 250, sortable: false, editable: true, editoptions: { Size: 130 } },
				],
				hiddengrid: true,
				height: '150',
				width: '900',
				scroll: true,
				rowNum: 999999,
				caption: "Auto Subscribes",
				pager: '#autoSubscribesTablePager'
			});
			$("#autoSubscribesTable").jqGrid('navGrid', '#autoSubscribesTablePager', { refresh: false, search: false },
				{
					width: 800,
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
					width: 800,
					closeAfterAdd: true,
					reloadAfterAdd: true,
					closeOnEscape: true,
					recreateForm: true,
					resize: true,
					beforeInitData: function (formid) {
						return (typePK != "");
					},
					afterSubmit: function (response, postdata) {
						ReloadAutoSubscribes();
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

			$("#lookupsTable").jqGrid({
				datatype: 'local',
				jsonReader: { root: 'eHubSubscriptionLookups', id: 'SL_PK', repeatitems: false },
				colNames: ['SL_PK', 'Message Type', 'Value XPath', 'Value Property'],
				colModel: [
					{ name: 'SL_PK', hidden: true },
					{ name: 'DT_Code', classes: "wrapped", width: 250, sortable: false, editable: true, edittype: 'custom', 
						editoptions: {
							custom_element: FormatMessageTypeEditFields,
							custom_value: function(elem) {
								return $(elem).children('.messageTypeValue').val();
							}
						}
					},
					{ name: 'SL_ValueXpath', classes: "wrapped", width: 250, sortable: false, editable: true, editoptions: { Size: 130 } },
					{ name: 'SL_ValueProperty', classes: "wrapped", width: 250, sortable: false, editable: true, editoptions: { Size: 130 }}
				],
				hiddengrid: true,
				height: '200',
				width: '900',
				scroll: true,
				rowNum: 999999,
				caption: "Lookups",
				pager: '#lookupsTablePager'
			});
			$("#lookupsTable").jqGrid('navGrid', '#lookupsTablePager', { refresh: false, search: false },
				{
					width: 800,
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
					width: 800,
					closeAfterAdd: false,
					closeOnEscape: true,
					recreateForm: true,
					resize: true,
					beforeInitData: function (formid) {
						return (typePK != "");
					},
					afterSubmit: function (response, postdata) {
						ReloadLookups();
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

			function SetUpValuesTableGridBody() {
				$("#valuesTable").jqGrid({
					datatype: 'local',
					jsonReader: { root: 'eHubSubscriptionValues', id: 'SV_PK', repeatitems: false },
					colNames: ['SV_PK', 'Subscription Type', 'Provider', 'Subscriber', 'Reference Type', 'Value', 'Reference', 'Subscribed', 'Expiry', '   '],
					colModel: [
						{ name: 'SV_PK', hidden: true },
						{ name: 'SV_ST', index: 'Subscription Type', search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: false, sortable: true },
						{ name: 'SV_CC_Provider', index: 'eHubClient_Provider.CC_ID', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true,
							edittype: 'custom',
							editoptions: {
								custom_element: FormatClientEditFieldsProvider,
								custom_value: function (elem) {
									return $(elem).children('.clientIdValue').val();
								}
							}
						},
						{ name: 'SV_CC_Subscriber', index: 'eHubClient_Subscriber.CC_ID', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true,
							edittype: 'custom',
							editoptions: {
								custom_element: FormatClientEditFieldsSubscriber,
								custom_value: function (elem) {
									return $(elem).children('.clientIdValue').val();
								}
							}
						},
						{ name: 'SV_ReferenceType', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true, editoptions: { Size: 50 } },
						{ name: 'SV_Value', width: 400, classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true, editoptions: { Size: 50 } },
						{ name: 'SV_Reference', width: 400, classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] }, editable: true, editoptions: { Size: 50 } },
						{
							name: 'SV_SubscribedUTC',
							index: 'eHubClient_Subscribed.CC_ID',
							sortable: true,
							search: true,
							searchoptions: { sopt: ['ge', 'gt', 'lt', 'le'] },
							editable: true,
							editoptions: { Size: 50 },
							searchrules: {
								custom: true,
								custom_func: validation_check
							}
						},
						{
							name: 'SV_ExpiryUTC',
							search: true,
							editable: true,
							searchoptions: { sopt: ['ge', 'gt', 'lt', 'le'] },
							editoptions: { Size: 50 },
							searchrules: {
								custom: true,
								custom_func: validation_check
							}
						},
						{ name: 'Link', index: 'Link', width: 50, align: 'left', sortable: false, search: false, label: '  ', formatter: NavigateToMessage },
					],
					height: '500',
					width: '1200',
					rowNum: 20,
					rowList: [20, 50, 100],
					caption: "Values",
					sortable: true,
					sortname: DEFAULT_SORT_COLUMN,
					sortorder: DEFAULT_SORT_ORDER,
					pager: '#valuesTablePager',
				});
			}

			function SetUpValuesGridNav() {
				$("#valuesTable").jqGrid('navGrid', '#valuesTablePager', {},
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
						beforeInitData: function (formid) {
							return (typePK != "");
						},
						afterSubmit: function (response, postdata) {
							ReloadValues();
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
							ModifySearchForm(searchForm);
							return true;
						},
						onSearch: function () {
							let sortColumn = $('#sort-column').val(),
								sortOrder = $('#sort-order').val();

							$("#valuesTable").jqGrid("setGridParam", {
								sortname: sortColumn,
								sortorder: sortOrder
							});
							
							hasUserInputChanged();
						}
					}
				);
			}

			$('#autoSubscribesTable').setGridParam({
				datatype: 'json',
				url: '<%: Url.Action("AutoSubscribes") %>' + '?type=',
				editurl: '<%: Url.Action("AutoSubscribesEdit") %>' + '?type='
			})
			$('#lookupsTable').setGridParam({
				datatype: 'json',
				url: '<%: Url.Action("Lookups") %>' + '?type=',
				editurl: '<%: Url.Action("LookupsEdit") %>' + '?type='
			})
			$('#valuesTable').setGridParam({
				datatype: 'json',
				url: '<%: Url.Action("Values") %>' + '?type=',
				editurl: '<%: Url.Action("ValuesEdit") %>' + '?type='
			})

			$("#uploadFile").change(UploadFileChanged);
			$("#valuesExportCsv").button();
			$("#valuesImportCsv").button().button("disable");

			function UploadFileChanged() {
				if ($(this).val() == "") {
					$("#valuesImportCsv").button("disable");
					$("#valuesInportOption").attr("disabled", "disabled");
					$("#valuesInportOption").addClass("ui-state-disabled");
				} else {
					$("#valuesImportCsv").button("enable");
					$("#valuesInportOption").removeAttr("disabled");
					$("#valuesInportOption").removeClass("ui-state-disabled");
				}
			};

			$('#upload_target').load(function () {
				$("#valuesTable").trigger("reloadGrid");
			});

			var clientEditStateProvider = {
				includeNonProd: false,
				checkbox: null,
			};

			var clientEditStateSubscriber = {
				includeNonProd: false,
				checkbox: null,
			};

			function FormatClientEditFieldsProvider(value, options) {
				var checkedAttribute = '';
				var elemStr = '<span><input id="'+ options.id + '_val" value="' + value +
								'" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>' +
					' <a class="fm-button ui-state-default ui-corner-all clientIdSelect">Select</a>' +
					' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
					' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
					'</span>';
				var elem = $(elemStr)[0];
				clientEditStateProvider.checkbox = $(elem).find('.clientIdCheckbox');
				clientEditStateProvider.checkbox.change(function () {
					clientEditStateProvider.includeNonProd = clientEditStateProvider.checkbox.is(":checked");
				});
				$(elem).children('.clientIdSelect').click(function(){
					DisplaySelectClientDialog($(elem).children('.clientIdValue'), clientEditStateProvider.includeNonProd);
				});
				return elem;
			}

			function FormatClientEditFieldsSubscriber(value, options) {
				var checkedAttribute = '';
				var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
					'" class="FormElement ui-widget-content ui-corner-all clientIdValue" role="textbox" type="text" readonly="true"/>' +
					' <a class="fm-button ui-state-default ui-corner-all clientIdSelect">Select</a>' +
					' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
					' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
					'</span>';
				var elem = $(elemStr)[0];
				clientEditStateSubscriber.checkbox = $(elem).find('.clientIdCheckbox');
				clientEditStateSubscriber.checkbox.change(function () {
					clientEditStateSubscriber.includeNonProd = clientEditStateSubscriber.checkbox.is(":checked");
				});
				$(elem).children('.clientIdSelect').click(function () {
					DisplaySelectClientDialog($(elem).children('.clientIdValue'), clientEditStateSubscriber.includeNonProd);
				});
				return elem;
			}
			function DisplaySelectClientDialog(elem, includeNonProd) {
				$("#selectClientDialog").dialog({
					title: 'Select Client',
					width: "auto",
					position: { my: "left top", at: "left+100 top-100", of: $(elem) },
					resizable: false,
					modal: true,
					buttons: {
						"Select": function() {
							var selectID = $('#selectClientDialog #clientID').val();
							if (selectID == "") {
								DisplayError("Please select a client.");
							}
							else {
								$(elem).val(selectID);
								$(this).dialog("close");
							}
						},
						Cancel: function() {
							$(this).dialog("close");
						}
					}
				});
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
					onSelectRow: function(id) {
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
			}

			function FormatMessageTypeEditFields(value, options) {
				var elemStr = '<span><input id="'+ options.id + '_val" value="' + value +
								'" class="FormElement ui-widget-content ui-corner-all messageTypeValue" role="textbox" type="text" readonly="true" size="120"/>' +
								' <a class="fm-button ui-state-default ui-corner-all messageTypeSelect">Select</a>';
				var elem = $(elemStr)[0];
				$(elem).children('.messageTypeSelect').click(function(){
					DisplaySelectMessageTypesDialog($(elem).children('.messageTypeValue'));
				});
				return elem;
			}
			function DisplaySelectMessageTypesDialog(elem) {
				$("#selectMessageTypesDialog").dialog({
					title: 'Select Message Type',
					width: "auto",
					position: { my: "left top", at: "left+100 top-100", of: $(elem) },
					resizable: false,
					modal: true,
					buttons: {
						"Select": function() {
							var selectType = $('#selectMessageTypesDialog #messageType').val();
							if (selectType == "") {
								DisplayError("Please select a messageType.");
							}
							else {
								$(elem).val(selectType);
								$(this).dialog("close");
							}
						},
						Cancel: function() {
							$(this).dialog("close");
						}
					}
				});
				$("#messageTypesTable").jqGrid({
					datatype: 'json',
					url: '<%: Url.Action("MessageTypes") %>',
					jsonReader: { root: 'eHubMessageTypes', id: 'DT_Code', repeatitems: false },
					colNames: ['Message Type'],
					colModel: [ { name: 'DT_Code', width: 400, classes: "wrapped", sortable: true, search: true }, ],
					onSelectRow: function(id) {
						$('#selectMessageTypesDialog #messageType').val(id)
					},
					height: '400',
					width: '400',
					rowNum: 15,
					rowList: [15, 30, 50],
					sortname: 'DT_Code',
					sortorder: 'asc',
					pager: '#messageTypesTablePager'
				});
				$("#messageTypesTable").jqGrid('navGrid', '#messageTypesTablePager', { add: false, edit: false, del: false, search: false, refresh: false } );
				$("#messageTypesTable").jqGrid('filterToolbar', { searchOnEnter: false });
			}

			function DisplayError(errorMsg) {
				$('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
			}

			function NavigateToMessage(cl) {
				return be = "<input id='il_navigate_" + cl + "' style='height:22px;width:20px;' type='button' class='ui-icon ui-icon-arrowthick-1-e inline-block' onclick= window.open('" + cl + "') target='_blank'; />";

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

				if (sessionStorage.getItem("hasUserInputChanged") !== 'true') {
        			ResetSortOrder();
    			}
			}

			function hasUserInputChanged() {
				const currentSelectColumn = $('td.columns').first().find('select').val();
				const currentSortOrder = $('#sort-order').val();
				const currentSortColumn = $('#sort-column').val();

				if (
					currentSelectColumn != 'SV_Value' ||
					currentSortOrder != DEFAULT_SORT_ORDER ||
					currentSortColumn != DEFAULT_SORT_COLUMN
				) {
					sessionStorage.setItem("hasUserInputChanged", "true");
				}
			}

			function ResetSortOrder() {
				$('td.columns').first().find('select').val('SV_Value').trigger('change')
                $('#sort-order').val(DEFAULT_SORT_ORDER);
                $('#sort-column').val(DEFAULT_SORT_COLUMN);
                $('td.query-sort').html('');
                $("#valuesTable").jqGrid("setGridParam", {
                    sortname: DEFAULT_SORT_COLUMN,
                    sortorder: DEFAULT_SORT_ORDER
                });

				sessionStorage.setItem("hasUserInputChanged", "false");
				UpdateQuery();
            }

			function UpdateQuery() {
				let sortColumn = $('#sort-column').val()
				let sortOrder = $('#sort-order').val()
				let sortQueryCell = ($('td.query-sort').length) ? $('td.query-sort') : $('<td class="query-sort"/>')
				sortQueryCell.html(' ORDER By ' + sortColumn + ' ' + sortOrder)
				$(sortQueryCell).insertAfter($('td.query').first())
			}

			function validation_check(value, colname) {
				if (colname == 'Subscribed' || colname == "Expiry") {
					const regex = /^(\d{4})-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])(T([01]\d|2[0-3]):([0-5]\d):([0-5]\d))?$/;
					if (!regex.test(value)) {
						return [false, "Invalid date format. Use 'YYYY-MM-DD' or 'YYYY-MM-DDTHH:MM:SS'."]
					}

					const [year, month, day] = value.split('T')[0].split('-').map(Number);
					const date = new Date(value);
					// JS auto-corrects the date if its invalid and is 0-based for months, hence we need to check if it was corrected below to check validity.
					if (date.getFullYear() != year || date.getMonth() + 1 != month || date.getDate() != day) {
						return [false, "Invalid date. Please enter a valid date."];
					}
				}

				// more conditions can be added later on
				return [true, ""];
			}
		});

	</script>


</asp:Content>
