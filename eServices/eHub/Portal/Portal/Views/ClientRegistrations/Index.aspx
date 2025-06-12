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
	
	<div><label for="registrationTypesList">Client Registrations</label></div>
	<div>
		<div id="typesFields">
			<input type="radio" id="sortById" name="sortingMethod" value="SORTBYID" checked>
			<label for="sortById" style="font-weight: normal;">Sort by ID</label>
			<input type="radio" id="sortByDescription" name="sortingMethod" value="SORTBYDESCRIPTION">
			<label for="sortByDescription" style="font-weight: normal;">Sort by Description</label>
			<br>
			<select id="registrationTypesList" class="ui-widget ui-state-default ui-corner-all">
			</select>
			<img id="registrationTypesListThrobber" src='<%: Url.Content("~/Content/images/throbber.gif") %>'
				alt='Loading ...' style="display: none" class="throbber" />
			<button id="viewRegistrationTypeButton" style="display: none">Details</button>
			<button id="newRegistrationTypeButton" >Add New</button>
		</div>
		<br />
		<div id="registrationsFields">
			<table id="registrationsTable"></table>
			<div id="registrationsTablePager"></div>
		</div>
		<div id="registrationsCsvFields" style="display: none">
			<br />
			<table>
				<tr>
					<td valign="top" style="border-right:1px solid #999; padding-right:5px">
						<form id="registrationsExportCsvForm" action="<%: Url.Action("RegistrationsExportCsv") %>">
							<input type="hidden" id="exportregistrationTypePK" name="registrationTypePK" value="" />
							<input type="hidden" id="gridColumns" name="gridColumns" value="" />
							<input type="hidden" id="sortDataExportCsv" name="sortDataExportCsv" value="" />
							<input type="submit" id="registrationsExportCsv" value="Export to CSV" />
						</form>
						<form id="registrationsExportCsvFormWithFilters" style="margin-top: 8px;" action="<%: Url.Action("RegistrationsExportCsv") %>">
							<input type="hidden" id="exportregistrationTypePK" name="registrationTypePK" value="" />
							<input type="hidden" id="filterForExportCsv" name="filterForExportCsv" value="" />
							<input type="hidden" id="sortDataExportCsvWithFilters" name="sortDataExportCsv" value="" />
							<input type="hidden" id="gridColumnsWithFilters" name="gridColumns" value="" />
							<input type="submit" id="registrationsExportCsvWithFilters" value="Export Filtered Data to CSV" />
						</form>
					</td>
					<td style="padding-left:5px">
						<form id="registrationsImportCsvForm" action="<%: Url.Action("RegistrationsImportCsv") %>" method="post" enctype="multipart/form-data" target="upload_target" >
							<input type="hidden" id="inputregistrationTypePK" name="registrationTypePK" value="" />
							<input type="submit" name="registrationsImportCsv" id="registrationsImportCsv" value="Import from CSV" disabled="disabled" />
							<select name="option" id="registrationsInportOption" class="ui-state-disabled ui-corner-all ui-widget" disabled="disabled">
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
		<div id="registrationsCsvFieldWithoutRegistrationType" style="display: none">
			<br />
			<table>
				<tr>
					<td valign="top" style="padding-right:5px">
						<form id="registrationsExportCsvFormWithoutRegType" action="<%: Url.Action("RegistrationsExportCsvForFilteredDataWithoutRegistrationType") %>">
							<input type="hidden" id="exportregistrationTypePKWithoutRegType" name="registrationTypePK" value="" />
							<input type="hidden" id="filterForExportCsvWithoutRegType" name="filterForExportCsv" value="" />
							<input type="hidden" id="sortDataExportCsvWithoutRegType" name="sortDataExportCsv" value="" />
							<input type="submit" id="registrationsExportCsvWithFiltersWithoutRegType" class="ui-button ui-widget ui-state-default ui-corner-all" value="Export to CSV" />
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
    
    <div id="viewConfigXmlDialog" title="Configuration Xml" style="display: none">
        <p id="configXmlText" style="white-space: pre-wrap; font-size: 13px; word-wrap: break-word;">
        </p>
        <form action="">
            <input type="hidden" name="configXml" id="configXml" />
        </form>
    </div>
    
    <div id="uploadConfigXmlDialog" title="Configuration Xml" style="display: none">
        <form id="uploadConfigXml" action="<%: Url.Action("UploadConfiguration") %>" method="post" enctype="multipart/form-data" target="upload_configxmltarget" >
            <input type="file" name="uploadconfigxmlFile" id="uploadconfigxmlFile" class="ui-widget ui-state-default ui-corner-all ui-button-text" size="75" />
            <iframe id="upload_configxmltarget" name="upload_configxmltarget" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>
            <br />
            <br />
            <input type="hidden" id="registrationPK" name="registrationPK" value="" />
            <input type="hidden" id="registrationTypePK" name="registrationTypePK" value="" />
            <input type="submit" name="uploadConfiguration" id="uploadConfiguration" value="Select" disabled="disabled" />
        </form>
    </div>

	<div id="registrationTypeDetailsForm" title="RegistrationType Details" style="display: none">
		<p id="errorMsg" class="ui-state-error" style="display: none"></p>
		<form action="">
			<fieldset>
				<table>
					<tbody>
						<tr>
							<td>ID</td>
							<td>
								<input type="text" name="RT_ID" id="RT_ID" class="ui-widget ui-widget-content" size="50"/>
							</td>
						</tr>
						<tr>
							<td>Description</td>
							<td>
								<input type="text" name="RT_Description" id="RT_Description" class="ui-widget ui-widget-content" size="50"/>
							</td>
						</tr>
					</tbody>
				</table>
				<input type="hidden" name="RT_PK" id="RT_PK"/>
			</fieldset>
		</form>
	</div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

		$(document).ready(function () {

			var registrationTypePK = "";
			var attr1List = "";
			if (!sessionStorage.getItem("sortData")) {
				sessionStorage.setItem("sortData", JSON.stringify({ sidx: 'CX_CC_ID', sord: 'asc' }));
			}


			$.jgrid.extend({
				setColWidth: function (iCol, newWidth, adjustGridWidth) {
					return this.each(function () {
						var $self = $(this), grid = this.grid, p = this.p, colName, colModel = p.colModel, i, nCol;
						if (typeof iCol === "string") {
							colName = iCol;
							for (i = 0, nCol = colModel.length; i < nCol; i++) {
								if (colModel[i].name === colName) {
									iCol = i;
									break;
								}
							}
							if (i >= nCol) {
								return;
							}
						} else if (typeof iCol !== "number") {
							return;
						}
						grid.resizing = { idx: iCol };
						grid.headers[iCol].newWidth = newWidth;
						grid.newWidth = p.tblwidth + newWidth - grid.headers[iCol].width;
						grid.dragEnd();
						if (adjustGridWidth !== false) {
							$self.jqGrid("setGridWidth", Math.max(grid.newWidth, 1300), false);
						}
					});
				}
			});
			let currentGridWidth = 0;

			$('#errorMessage').ajaxError(function (event, request, settings, exception) {
				$(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
				$(this).show();
			}).ajaxComplete(function () {
				$(this).hide()
			});

			LoadRegistrationTypesList();

			function LoadRegistrationTypesList() {
				registrationTypePK = "";
				$('#registrationsTable').clearGridData();
				$("#viewRegistrationTypeButton").hide();
				$("#registrationTypesList").attr("disabled", "disabled");
				$("#registrationTypesList").html("<option value=''>Loading ...<\/option>");
				$("#registrationTypesListThrobber").show();
				var url = '<%: Url.Action("RegistrationTypes") %>' + '?sortByDescription=' + document.getElementById('sortByDescription').checked.toString();
				$.getJSON(url, null, function (data) {
					$("#registrationTypesList").html("<option value=''>-- Select Type --<\/option>");
					$.each(data.eHubRegistrationTypes, function (index, optionData) {
						if (document.getElementById('sortByDescription').checked) {
							$("#registrationTypesList").append("<option value='" + optionData.RT_PK + "'>" + optionData.RT_Description + ' (' + optionData.RT_ID + ')' + "<\/option>");
						} else {
							$("#registrationTypesList").append("<option value='" + optionData.RT_PK + "'>" + optionData.RT_ID + ' - ' + optionData.RT_Description + "<\/option>");
						}
					});
					$("#registrationTypesList").removeAttr("disabled");
					$("#registrationTypesList").focus();
					$("#registrationTypesListThrobber").hide();
				});
			}

			$('#sortById').bind('click', LoadRegistrationTypesList);
			$('#sortByDescription').bind('click', LoadRegistrationTypesList);

			var RegistrationTypeChangeHandler = function () {
				var currentPK = $("#registrationTypesList > option:selected").attr("value");
				if (registrationTypePK == currentPK)
					return;
				registrationTypePK = currentPK;
				$("#registrationsTable").GridUnload();
				initGrid();
				$("#registrationsCsvFields input[name*='registrationTypePK']").each(function () { $(this).val(registrationTypePK); });
				$("#registrationsImportCsvForm")[0].reset();
				UploadFileChanged();
				if (registrationTypePK != "") {
					attr1List = GetAttr1List();
					ReloadRegistrations();
					$("#viewRegistrationTypeButton").show();
					$("#registrationsCsvFields").show();
					sessionStorage.setItem("sortData", JSON.stringify({ sidx: 'CX_CC_ID', sord: 'asc' }));
				} else {
					$("#viewRegistrationTypeButton").hide();
					$("#registrationsCsvFields").hide();
				}
				return;
			}
			$("#registrationTypesList").change(RegistrationTypeChangeHandler).keypress(RegistrationTypeChangeHandler);

			initGrid();

			function initGrid() {
				SetUpRegistrationTableGridBody();
				SetUpRegistrationGridNav();
				ResetHeaders();
				registrationTypePK == "" ? $("#registrationsCsvFieldWithoutRegistrationType").show() : $("#registrationsCsvFieldWithoutRegistrationType").hide();

				ShowEditableBtns(registrationTypePK != "");
				RegistrateItemUrl();
			}

			function isFilterStringEmpty(filterString) {
				const conditions = filterString.split(/(?:AND|OR)/i).map(cond => cond.trim());
				for (let condition of conditions) {
					const match = condition.match(/LIKE\s*"([^"]*)"/i);
					if (match) {
						const value = match[1];
						if (value !== "%") {
							return false;
						}
					}
				}
				return true;
			}

			function toggleRegistrationExportCsvWithFilterButtonVisibility() {
				if (registrationTypePK != "") {
					var filtersInputElement = document.querySelector('#fbox_registrationsTable .query');
					var filtersString = filtersInputElement ? filtersInputElement.textContent.trim() : '';

					if (!isFilterStringEmpty(filtersString)) {
						$("#registrationsExportCsvWithFilters").button("enable");
					} else {
						$("#registrationsExportCsvWithFilters").button("disable");
					}
				}
			}

			function ResetHeaders() {
				$($('#registrationsTable').jqGrid('getGridParam', 'colModel')).each(function (index, colModel) {
					var colName = colModel.name;
					if (colName == 'DecodedPassword') {
						$('#registrationsTable').jqGrid('setLabel', colName, GetCustomisedHeader('CX_Password1'));
					}
					else {
						var header = GetCustomisedHeader(colName);
						if (header === '') {
							$('#registrationsTable').jqGrid('hideCol', colName);
						}
						else {
							$('#registrationsTable').jqGrid('showCol', colName);
							$('#registrationsTable').jqGrid('setLabel', colName, header);
						}
					}
				});
			}

			function gridContainsPasswordField() {
				var colModels = $('#registrationsTable').jqGrid('getGridParam', 'colModel');
				for (var i = 0; i < colModels.length; i++) {
					var colModel = colModels[i];
					var colName = colModel.name;
					if (colName === "CX_Password1") {
						var header = GetCustomisedHeader(colName);
						if (header != '') {
							return true;
						}
						else {
							return false;
						}
					}
				}

				return false;
			}

			function GetFlag1DescriptionList() {
				return jQuery.ajax({
					url: '<%: Url.Action("GetFlag1DescriptionList") %>' + '?regType=' + registrationTypePK,
					async: false
				}).responseText;
			}

			function GetFlag2DescriptionList() {
				return jQuery.ajax({
					url: '<%: Url.Action("GetFlag2DescriptionList") %>' + '?regType=' + registrationTypePK,
					async: false
				}).responseText;
			}

			function GetAttr1List() {
				return jQuery.ajax({
					url: '<%: Url.Action("GetAttr1List") %>' + '?regType=' + registrationTypePK,
					async: false
				}).responseText;
			}

			function ReloadRegistrations() {
				if (registrationTypePK !== "") {
					$('#registrationsTable').setColProp('CX_Flag1', { editoptions: { value: GetFlag1DescriptionList() } });
					$('#registrationsTable').setColProp('CX_Flag2', { editoptions: { value: GetFlag2DescriptionList() } });
				}
				$('#registrationsTable').trigger("reloadGrid");
			}

			function RegistrateItemUrl() {
				$('#registrationsTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("Registrations") %>' + '?regType=' + registrationTypePK,
					editurl: '<%: Url.Action("RegistrationEdit") %>' + '?regType=' + registrationTypePK
				});
			}

			$("#viewRegistrationTypeButton").button().click(function () {
				$.getJSON('<%: Url.Action("RegistrationTypeInfo") %>' + "?regType=" + registrationTypePK, function (data) {
					$("#RT_PK").val(data.eHubRegistrationType.RT_PK);
					$("#RT_ID").val(data.eHubRegistrationType.RT_ID);
					$("#RT_Description").val(data.eHubRegistrationType.RT_Description);
				});
				$('#registrationTypeDetailsForm #errorMsg').hide();
				$("#registrationTypeDetailsForm").dialog({
					autoOpen: true,
					height: "auto",
					width: "auto",
					modal: true,
					buttons: {
						"Save": function () {
							$.ajax({
								type: "POST",
								async: false,
								url: '<%: Url.Action("RegistrationTypeInfoEdit") %>',
								data: {
									oper: "edit",
									RT_PK: registrationTypePK,
									RT_ID: $("#RT_ID").val(),
									RT_Description: $("#RT_Description").val(),
								},
								success: function (data) {
									$("#registrationTypeDetailsForm").dialog("close");
									LoadRegistrationTypesList();
								},
								error: function (data) {
									$('#registrationTypeDetailsForm #errorMsg').text($(data.responseText)[1].innerText);
									$('#registrationTypeDetailsForm #errorMsg').show();
								}
							});
						},
						"Delete": function () {
							$('<div><p class="ui-state-error">Are you sure you want to delete this registration type?<\/p><\/div>').dialog({
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
														url: '<%: Url.Action("RegistrationTypeInfoEdit") %>',
														data: {
															oper: "del",
															RT_PK: registrationTypePK
														},
														success: function (data) {
															LoadRegistrationTypesList();
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

			$("#newRegistrationTypeButton").button().click(function () {
				$('#registrationTypeDetailsForm #errorMsg').hide();
				$('#registrationTypeDetailsForm input').val("");
				$("#registrationTypeDetailsForm").dialog({
					autoOpen: true,
					height: "auto",
					width: "auto",
					modal: true,
					buttons: {
						"Create": function () {
							$.ajax({
								type: "POST",
								async: false,
								url: '<%: Url.Action("RegistrationTypeInfoEdit") %>' + "?type=",
								data: {
									oper: "add",
									RT_ID: $("#RT_ID").val(),
									RT_Description: $("#RT_Description").val(),
								},
								success: function (data) {
									$("#registrationTypeDetailsForm").dialog("close");
									LoadRegistrationTypesList();
								},
								error: function (data) {
									$('#registrationTypeDetailsForm #errorMsg').text($(data.responseText)[1].innerText);
									$('#registrationTypeDetailsForm #errorMsg').show();
								}
							});
						},
						Cancel: function () {
							$(this).dialog("close");
						}
					}
				});
			});

			function GetCustomisedHeader(columnName) {
				if (registrationTypePK == "") return jQuery.ajax({
					url: encodeURI('<%: Url.Action("GetCustomisedHeaderForID") %>' + '?regTypeID=Default&columnName=' + columnName),
					async: false
				}).responseText;
				return  jQuery.ajax({
					url: encodeURI('<%: Url.Action("GetCustomisedHeader") %>' + '?regType=' + registrationTypePK + '&columnName=' + columnName),
					async: false
				}).responseText;
			}

			function SetUpRegistrationTableGridBody() {
				$("#registrationsTable").jqGrid({
					datatype: 'local',
					jsonReader: { root: 'eHubClientRegistrations', id: 'CX_PK', repeatitems: false },
					colModel: [
						{ name: 'CX_PK', hidden: true },
						{
							name: 'RegType',
							classes: "wrapped",
							search: true,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: false,
							sortable: true,
							label: '',
						},
						{ name: 'CX_CC_ID', index: 'CX_CC_ID', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false }, editable: true,
							edittype: 'custom',
							editoptions: {
								custom_element: FormatClientEditFields,
								custom_value: function (elem) {
									return $(elem).children('.clientIdValue').val();
								}
							},
							label: '',
						},
						{ name: 'CX_Qualifier', index: 'CX_Qualifier', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false }, editable: true, editoptions: { Size: 40 }, sortable: false, label: GetCustomisedHeader('CX_Qualifier') },
						{ name: 'CX_Code', index: 'CX_Code', classes: "wrapped", search: true, searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false }, editable: true, editoptions: { Size: 40 }, label: GetCustomisedHeader('CX_Code') },
						{
							name: 'CX_Attr1',
							index: 'CX_Attr1',
							classes: "wrapped",
							search: true,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: true,
                        editoptions: { Size: 40},
							label: '',
						},
						{
							name: 'CX_Password1',
							index: 'CX_Password1',
							classes: "wrapped",
							search: false,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: false,
							sortable: false,
							editoptions: { Size: 40 },
							label: '',
						},
						{
							name: 'DecodedPassword',
							index: 'DecodedPassword',
							hidden: true,
							editable: true,
							editrules: { edithidden: true },
							hidedlg: true,
							viewable: false,
							editoptions: { Size: 40 },
							label: '',
						},
						{
							name: 'CX_Flag1',
							index: 'CX_Flag1',
							classes: "wrapped",
							search: false,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: true,
							edittype: "select",
							formatter: "select",
							editoptions: { readonly: true, Size: 226 },
						beforeShowForm: function() { $("#tr_CX_Flag1").disable(); },
							sortable: false,
							label: '',
						},
						{
							name: 'CX_Flag2',
							index: 'CX_Flag2',
							classes: "wrapped",
							search: false,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: true,
							edittype: "select",
							formatter: "select",
							editoptions: { readonly: true, Size: 226 },
						beforeShowForm: function() { $("#tr_CX_Flag2").disable(); },
							sortable: false,
							label: '',
						},
						{
							name: 'CustomValue1',
							index: 'CustomValue1',
							classes: "wrapped",
							search: false,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: true,
							sortable: false,
							editoptions: { Size: 40},
							label: '',
						},
						{
							name: 'CustomValue2',
							index: 'CustomValue2',
							classes: "wrapped",
							search: false,
							searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
							editable: true,
							sortable: false,
                        editoptions: { Size: 40},
							label: '',
						},
						{
							name: 'CX_ConfigXml',
							index: 'CX_ConfigXml',
							hidden: false,
							edittype: 'custom',
							search: false,
							editable: true,
							editoptions: {
								custom_element: editConfigXML,
							},
							label: '',
							formatter: ConfigXmlOnLoading
						},
						{
							name: 'CX_IssuedUTC',
							index: 'CX_IssuedUTC',
							search: false,
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
							label: '',
						},
						{
							name: 'CX_ExpiryUTC',
							index: 'CX_ExpiryUTC',
							search: false,
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
							label: '',
						}
					],
					height: '500',
					width: '700',
					rowNum: 20,
					rowList: [20, 50, 100],
					caption: "Registrations",
					sortable: true,
					sortname: 'CX_CC_ID',
					sortorder: 'asc',
					pager: '#registrationsTablePager',
					shrinkToFit: "false",
					loadComplete: function () {
						let grid = $("#registrationsTable");
						var $this = $(this), iCol, iRow, rows, row, cm, colWidth,
							$cells = $this.find(">tbody>tr>td"),
							$colHeaders = $(this.grid.hDiv).find(">.ui-jqgrid-hbox>.ui-jqgrid-htable>thead>.ui-jqgrid-labels>.ui-th-column>div"),
							colModel = $this.jqGrid("getGridParam", "colModel"),
							n = $.isArray(colModel) ? colModel.length : 0,
							idColHeadPrexif = "jqgh_" + this.id + "_";
						totalWidth = 0;
						$cells.wrapInner("<span class='width-wrapper'></span>");
						$colHeaders.wrapInner("<span class='width-wrapper'></span>");
						for (iCol = 0; iCol < n; iCol++) {
							cm = colModel[iCol];
							colName = cm.name;

							if (colName !== 'CX_ConfigXml') {
								colWidth = $("#" + idColHeadPrexif + $.jgrid.jqID(cm.name) + ">.width-wrapper").outerWidth();
								for (iRow = 0, rows = this.rows; iRow < rows.length; iRow++) {
									row = rows[iRow];
									if ($(row).hasClass("jqgrow")) {
										colWidth = Math.max(colWidth, $(row.cells[iCol]).find(".width-wrapper").outerWidth());
									}
								}
							}
							if (colName === 'CX_ConfigXml') colWidth = 150;
							colWidth = colWidth + 10;
							totalWidth += colWidth;
							$this.jqGrid("setColWidth", iCol, colWidth);
							currentGridWidth = Math.min(1300, totalWidth + 50);
							grid.jqGrid('setGridWidth', currentGridWidth, false);

						}
						$cells.find(".width-wrapper").contents().unwrap();
						$colHeaders.find(".width-wrapper").contents().unwrap();
					},
					onSortCol: function (index, iCol, sortorder) {
						sessionStorage.setItem("sortData", JSON.stringify({ sidx: index, sord: sortorder }));
					},
					ondblClickRow: function (rowid) {
						jQuery(this).jqGrid('viewGridRow', rowid, {   // view option
							width: 400,
							closeAfterEdit: true,
							closeOnEscape: true,
							recreateForm: true,
							viewPagerButtons: false,
							resize: true
						});
					}
				});

			}

			function SetUpRegistrationGridNav() {
				$("#registrationsTable").jqGrid('navGrid', '#registrationsTablePager', { view: true },
					{//edit option
						id: "registrationsGridNavEdit",
						width: 400,
						closeAfterEdit: true,
						closeOnEscape: true,
						recreateForm: true,
						viewPagerButtons: true,
						resize: true,
						beforeInitData: function (formid) {
							var containsPasswordField = gridContainsPasswordField();
							if (!containsPasswordField) {
								$('#registrationsTable').setColProp('DecodedPassword', { editable: false });
							} else {
								$('#registrationsTable').setColProp('DecodedPassword', { editable: true });
							}

							$('#registrationsTable').setColProp('CustomValue1', { editable: true });
							$('#registrationsTable').setColProp('CustomValue2', { editable: true });

							if (attr1List.length > 0) {
								$('#registrationsTable').setColProp('CX_Attr1', { edittype: 'select', editoptions: { value: attr1List } });
							} else {
								$('#registrationsTable').setColProp('CX_Attr1', { edittype: 'text', editoptions: { value: $('#registrationsTable').getRowData($('#registrationsTable').jqGrid('getGridParam', 'selrow'))["CX_Attr1"] } });
							}
						},
						beforeShowForm: function () {
							$('#cx_upload').hide();
							$('#inputFile1').hide();
						},
						afterSubmit: function (response, postdata) {
							var data = eval('(' + response.responseText + ')');
							return [data.success, data.message, data.id];
						}
					},
					{ //add option
						id: "registrationsGridNavAdd",
						width: 400,
						closeAfterAdd: false,
						closeOnEscape: true,
						recreateForm: true,
						resize: true,
						beforeInitData: function (formid) {
							var containsPasswordField = gridContainsPasswordField();
							if (!containsPasswordField) {
								$('#registrationsTable').setColProp('DecodedPassword', { editable: false });
							} else {
								$('#registrationsTable').setColProp('DecodedPassword', { editable: true });
							}

							$('#registrationsTable').setColProp('CustomValue1', { editable: false });
							$('#registrationsTable').setColProp('CustomValue2', { editable: false });

							if (attr1List.length > 0) {
								$('#registrationsTable').setColProp('CX_Attr1', { edittype: 'select', editoptions: { value: attr1List } });
							} else {
								$('#registrationsTable').setColProp('CX_Attr1', { edittype: 'text', editoptions: { value: ''} });
							}
							return (registrationTypePK != "");
						},
						beforeShowForm: function () {
							$('#cx_download').hide();
							$('#cx_upload').show();
							ModifyJQGridPOSTRequest("add");
						},
						afterSubmit: function (response, postdata) {
							ReloadRegistrations();
							var data = eval('(' + response.responseText + ')');
							return [data.success, data.message, data.id];
						}
					},
					{ // del option
						id: "registrationsGridNavDel",
						width: 400,
						closeAfterEdit: true,
						closeOnEscape: true,
						recreateForm: true,
						viewPagerButtons: false,
						resize: true
					},
					{
						multipleSearch: true,
						showQuery: true,
						onReset: function () {
							$("#registrationsExportCsvWithFilters").button("disable");
						},
						onSearch: function () {
							toggleRegistrationExportCsvWithFilterButtonVisibility();
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
						recreateFilter: true,
						beforeShowSearch: function (searchForm) {
						searchForm.find('.columns').find('option').each( function (index, option) {
								option.text = GetCustomisedHeader(option.value);
							});
							return true;
						}
					});
			}

			function ShowEditableBtns(isShow) {
				if (isShow) {
					$("#registrationsGridNavAdd").show();
					$("#registrationsGridNavEdit").show();
					$("#registrationsGridNavDel").show();
				}
				else {
					$("#registrationsGridNavAdd").hide();
					$("#registrationsGridNavEdit").hide();
					$("#registrationsGridNavDel").hide();
				}
			}

			$("#uploadFile").change(UploadFileChanged);
			$('#upload_configxmltarget').load(UploadconfigurationChanged);
			$("#uploadconfigxmlFile").change(UploadconfigurationChanged);
			$("#registrationsExportCsv").button();
			$("#registrationsImportCsv").button().button("disable");
			$("#registrationsExportCsvWithFilters").button().button("disable");

			$("#registrationsExportCsv").click(function (event) {
				var columns = $('#registrationsTable').jqGrid('getGridParam', 'colModel').filter(x => !x.hidden).map(x => x.name).join(",");
				document.getElementById('gridColumns').value = columns;
				document.getElementById('sortDataExportCsv').value = sessionStorage.getItem("sortData");
			});

			$('#registrationsExportCsvWithFilters').click(function (event) {
				var filtersElement = document.querySelector('#fbox_registrationsTable .query');
				var filters = filtersElement ? filtersElement.textContent.trim() : '';
				document.getElementById('filterForExportCsv').value = filters;
				var columns = $('#registrationsTable').jqGrid('getGridParam', 'colModel').filter(x => !x.hidden).map(x => x.name).join(",");
				document.getElementById('gridColumnsWithFilters').value = columns;
				document.getElementById('sortDataExportCsvWithFilters').value = sessionStorage.getItem("sortData");
			});

			$('#registrationsExportCsvWithFiltersWithoutRegType').click(function (event) {
				var filtersElement = document.querySelector('#fbox_registrationsTable .query');
				var filters = filtersElement ? filtersElement.textContent.trim() : '';
				document.getElementById('filterForExportCsvWithoutRegType').value = filters;
				document.getElementById('sortDataExportCsvWithoutRegType').value = sessionStorage.getItem("sortData");
			});

			function UploadFileChanged() {
				if ($(this).val() == "") {
					$("#registrationsImportCsv").button("disable");
					$("#registrationsInportOption").attr("disabled", "disabled");
					$("#registrationsInportOption").addClass("ui-state-disabled");
				} else {
					$("#registrationsImportCsv").button("enable");
					$("#registrationsInportOption").removeAttr("disabled");
					$("#registrationsInportOption").removeClass("ui-state-disabled");
				}
			};

			function UploadconfigurationChanged() {
				if ($(this).val() == "") {
					$("#uploadConfiguration").attr("disabled", "disabled");
				} else {
					$("#uploadConfiguration").removeAttr("disabled");
				}
			};

			$('#upload_target').load(function () {
				var responseBody = $('#upload_target').contents().find('body').text() || "";
				try {
					var data = JSON.parse(responseBody);
					if (!data.success) {
						DisplayError(data.message);
					} else {
						$("#registrationsTable").trigger("reloadGrid");
					}
				} catch (e) {
					if (responseBody.trim().length > 0) {
						DisplayError(responseBody.trim());
					} else {
						$("#registrationsTable").trigger("reloadGrid");
					}
				}
			});

			function FormatDownloadAndUpload(value, options) {
				var elemStr = '<span style="overflow: hidden;white-space: nowrap;">' + GetDownloadButtonForAddandEdit() + '<a id="cx_upload" class="fileUpload fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;height:25px;"><p>Upload</p><input id="cx_inputFile" name="cx_inputFile" type="file" style="position:absolute;top:0;right:0;margin:0;padding:0;cursor:pointer;opacity:0;height:25px;"/></a><label id="inputFile1">No file selected.</label></span>';
				var elem = $(elemStr)[0];
				$(elem).find('#cx_inputFile').change(function () {
					var o = this.value || 'No file selected.';
					var res = o.split("\\");
					var filename = res[res.length - 1];
					$('#inputFile1').text(filename);
				});
				return elem;
			}

			function GetDownloadButtonForAddandEdit() {
				var selected_row = $('#registrationsTable').jqGrid('getGridParam', 'selrow');
				var cx_pk = $('#registrationsTable').getRowData(selected_row)["CX_PK"];
				return '<a href="<%: Url.Action("DownloadConfiguration") %>?CX_PK=' + cx_pk + '" id="cx_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;height:25px;" target="_blank">Download</a>';
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

			function editConfigXML() {
				var selected_row = $('#registrationsTable').jqGrid('getGridParam', 'selrow');
				var CX_PK = $('#registrationsTable').getRowData(selected_row)["CX_PK"];
				return '<a href="<%: Url.Action("DownloadConfiguration") %>?CX_PK=' + CX_PK + '" id="cx_configxml_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;" target="_blank">Download</a>' +
					'<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayViewConfigXmlDialog(\'' + CX_PK + '\')"\>View</a>' +
					'<a class="fm-button ui-state-default ui-corner-all" style="position:absolute;" onclick="displayUploadConfigXmlDialog(\'' + CX_PK + '\', \'' + registrationTypePK + '\')">Upload</a>';

			}

			function ModifyJQGridPOSTRequest(oper) {
				$("#FrmGrid_registrationsTable").attr("id", "newFrmGrid_registrationsTable").removeAttr('onsubmit').append('<input type="hidden" name="oper" value="' + oper + '">')
					.attr('action', '<%: Url.Action("RegistrationEdit") %>' + "?regType=" + registrationTypePK).attr('method', 'post').attr('enctype', 'multipart/form-data').attr('target', 'upload_target1')
					.append('<iframe id="upload_target1" name="upload_target1" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>');
				$("#sData").attr("id", "newsData");
				$("#CX_CC_ID_val").attr("name", "CX_CC_ID");
				$("#TblGrid_registrationsTable_2").insertAfter($("#TblGrid_registrationsTable"));

				$('#newsData').append('<input type="submit" value="submit" size="200" style="position:absolute;top:0;right:0;margin:0;padding:0;cursor:pointer;opacity:0;border:0;height:22.8px;width:62.7px"/>');

				$('#upload_target1').load(function () { // the first time, it loads, is when the form appear.
					$('#upload_target1').load(function () {
						var bodyData = $('#upload_target1').contents().find("body").find("pre").html();
						if (!bodyData) return;
						var result = eval("(" + bodyData + ")");
						if (result.success) {
							$('#cData').click();
							$("#registrationsTable").trigger("reloadGrid");
						}
						else
							DisplayError(result.message);
					});
				});
			}

			function ConfigXmlOnLoading(CX_PK, options, rowObject) {
				return '<a href="<%: Url.Action("DownloadConfiguration") %>?CX_PK=' + CX_PK + '" id="cx_configxml_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;" target="_blank">Download</a>' +
					'<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayViewConfigXmlDialog(\'' + CX_PK + '\')"\>View</a>' +
					'<a class="fm-button ui-state-default ui-corner-all" style="position:absolute;" onclick="displayUploadConfigXmlDialog(\'' + CX_PK + '\', \'' + registrationTypePK + '\')">Upload</a>';

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

			function DisplayError(errorMsg) {
				$('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
			}
		});

		function displayViewConfigXmlDialog(CX_PK) {
			var cxConfigXml = jQuery.ajax({
				url: encodeURI('<%: Url.Action("DownloadConfiguration") %>' + '?CX_PK=' + CX_PK),
				async: false
			}).responseText;
			$('#viewConfigXmlDialog #configXmlText').text(cxConfigXml);

			$("#viewConfigXmlDialog").dialog({
				title: 'View Configuration Xml',
				width: 500,
				height: 700,
				resizable: true,
				modal: true
			});

		}

		function displayUploadConfigXmlDialog(CX_PK, registrationTypePK) {
			$('#uploadConfigXmlDialog #registrationPK').val(CX_PK);
			$('#uploadConfigXmlDialog #registrationTypePK').val(registrationTypePK);

			$("#uploadConfigXmlDialog").dialog({
				title: 'Upload Configuration Xml',
				width: "auto",
				height: "auto",
				resizable: true,
				modal: true
			});

		}
	</script>


</asp:Content>
