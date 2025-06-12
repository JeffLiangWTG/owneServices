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

		.query-sort {
			padding-left: 2px;
		}
	</style>
	<script src="<%: Url.Content("~/PlugIns/jquery-ui-timepicker-addon/1.6.3/jquery-ui-timepicker-addon.min.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jquery-ui-timepicker-addon/1.6.3/i18n/jquery-ui-timepicker-addon-i18n.min.js")%>" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb"><%=Html.MvcSiteMap().SiteMapPath()%></ol>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	
	<div id="errorMessage" class="error" style="display: none"></div>
	
	<div><label for="registrationTypesList">Client System Registrations</label></div>
	<div>
		<div id="typesFields">
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
	</div>

	<div id="selectClientSystemDialog" title="Select Client System" style="display: none">
		<div id="clientSystemsFields">
			<table id="clientSystemsTable"></table>
			<div id="clientSystemsTablePager"></div>
		</div>
		<form action="">
			<input type="hidden" name="clientSystemID" id="clientSystemID" />
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
			const DEFAULT_SORT_COLUMN = 'CD_EH_ID';
			const DEFAULT_SORT_ORDER = 'asc';

			var registrationTypePK = "";

			$('#errorMessage').ajaxError(function (event, request, settings, exception) {
				$(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
				$(this).show();
			}).ajaxComplete(function () {
				$(this).hide();
			});

			LoadRegistrationTypesList();

			function LoadRegistrationTypesList() {
				registrationTypePK = "";
				$('#registrationsTable').clearGridData();
				$("#viewRegistrationTypeButton").hide();
				$("#registrationTypesList").attr("disabled", "disabled");
				$("#registrationTypesList").html("<option value=''>Loading ...<\/option>");
				$("#registrationTypesListThrobber").show();
				var url = '<%: Url.Action("RegistrationTypes") %>';
				$.getJSON(url, null, function (data) {
					$("#registrationTypesList").html("<option value=''>-- Select Type --<\/option>");
					$.each(data.eHubRegistrationTypes, function (index, optionData) {
						$("#registrationTypesList").append("<option value='" + optionData.RT_PK + "'>" + optionData.RT_ID + ' - ' + optionData.RT_Description + "<\/option>");
					});
					$("#registrationTypesList").removeAttr("disabled");
					$("#registrationTypesList").focus();
					$("#registrationTypesListThrobber").hide();
				});
			}

			var RegistrationTypeChangeHandler = function () {
				var currentPK = $("#registrationTypesList > option:selected").attr("value");
				if (registrationTypePK == currentPK)
					return;
				registrationTypePK = currentPK;
				$("#registrationsTable").jqGrid('setGridParam', {search: false, postData: { "searchField": "", "searchString": "", "searchOper": ""} });
				ResetHeaders();
				if (registrationTypePK != "") {
					ReloadRegistrations();
					$("#viewRegistrationTypeButton").show();
				} else {
					$('#registrationsTable').clearGridData();
					$("#viewRegistrationTypeButton").hide();
				}
				return;
			};
			$("#registrationTypesList").change(RegistrationTypeChangeHandler).keypress(RegistrationTypeChangeHandler);

			function ResetHeaders() {
				var totalWidth = 0;
				$($('#registrationsTable').jqGrid('getGridParam', 'colModel')).each(function (index, colModel) {
					var header = GetCustomisedHeader(colModel.name);
                    if (header === '')
                    {
						$('#registrationsTable').jqGrid('hideCol', colModel.name);
					}
                    else
                    {
						totalWidth += 100;
						$('#registrationsTable').jqGrid('showCol', colModel.name);
						$('#registrationsTable').jqGrid('setLabel', colModel.name, header);
					}
				});
			}

			function ReloadRegistrations() {
				$('#registrationsTable').setColProp('CD_Flag1', { editoptions: { value: GetStatusDescriptionList() } });

				$('#registrationsTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("ClientSystemRegistrations") %>' + '?regType=' + registrationTypePK,
					editurl: '<%: Url.Action("ClientSystemRegistrationsEdit") %>' + '?regType=' + registrationTypePK
				}).trigger("reloadGrid");
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

			function GetStatusDescriptionList() {
				return jQuery.ajax({
					url: '<%: Url.Action("GetStatusDescriptionList") %>' + '?regType=' + registrationTypePK,
					async: false
				}).responseText;
			}

			function GetCustomisedHeader(columnName) {
				if (registrationTypePK == '') return jQuery.ajax({
					url: encodeURI('<%: Url.Action("GetCustomisedHeaderForID") %>' + '?regTypeID=Default&columnName=' + columnName),
					async: false
				}).responseText;
				return jQuery.ajax({
					url: encodeURI('<%: Url.Action("GetCustomisedHeader") %>' + '?regType=' + registrationTypePK + '&columnName=' + columnName),
					async: false
				}).responseText;
			}

			let currentGridWidth = 0;

			$("#registrationsTable").jqGrid({
				datatype: 'local',
				jsonReader: { root: 'eHubClientSystemRegistrations', id: 'CD_PK', repeatitems: false },
				colModel: [
					{ name: 'CD_PK', hidden: true },
					{
						name: 'CD_RT',
						index: 'CD_RT',
						classes: "wrapped",
						search: false,
						editable: false,
						edittype: 'custom',
						editoptions: { Size: 40 },
						label: GetCustomisedHeader('CD_RT'),
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'CD_EH_ID',
						index: 'CD_EH_ID',
						classes: "wrapped",
						search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: FormatClientEditFields,
							custom_value: function (elem) {
								return $(elem).children('.clientSystemIDValue').val();
							}
						},
						label: GetCustomisedHeader('CD_EH_ID'),
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'CD_Qualifier',
						index: 'CD_Qualifier',
						classes: "wrapped",
						search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						editable: true,
						editoptions: { Size: 40 },
						label: GetCustomisedHeader('CD_Qualifier'),
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'CD_Code',
						index: 'CD_Code',
						classes: "wrapped",
						search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						editable: true,
						editoptions: { Size: 40 },
						label: GetCustomisedHeader('CD_Code'),
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'CD_Attr1',
						index: 'CD_Attr1',
						classes: "wrapped",
						search: true,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						editable: true,
						editoptions: { Size: 40 },
						label: GetCustomisedHeader('CD_Attr1'),
						searchrules: { custom: true, custom_func: validation_check }
					},
					{
						name: 'CD_Attr2',
						index: 'CD_Attr2',
						classes: "wrapped",
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
						editable: true,
						editoptions: { Size: 40 },
						sortable: false,
						search: false,
						label: GetCustomisedHeader('CD_Attr2')
					},
					{
						name: 'CD_Flag1',
						index: 'CD_Flag1',
						resizable: true,
						search: false,
						editable: true,
						edittype: "select",
						formatter: "select",
						editoptions: { readonly: true, style: "width:226px" },
                        beforeShowForm: function() { $("#tr_CD_Flag1").disable(); },
						sortable: true,
						label: GetCustomisedHeader('CD_Flag1')
					},
					{
						name: 'CustomValue1',
						index: 'CustomValue1',
						classes: "wrapped",
						search: false,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
						editable: true,
						sortable: false,
						editoptions: { Size: 40 },
						label: GetCustomisedHeader('CustomValue1'),
					},
					{
						name: 'CustomValue2',
						index: 'CustomValue2',
						classes: "wrapped",
						search: false,
						searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
						editable: true,
						sortable: false,
						editoptions: { Size: 40 },
						label: GetCustomisedHeader('CustomValue2')
					},
					{
						name: 'CD_ConfigXml',
						index: 'CD_ConfigXml',
						hidden: false,
						search: false,
						editable: true,
						edittype: 'custom',
						editoptions: {
							custom_element: editConfigXML,
						},
						label: GetCustomisedHeader('CD_ConfigXml'),
						formatter: ConfigXmlOnLoading
					},
					{
						name: 'CD_IssuedUTC',
						index: 'CD_IssuedUTC',
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
						label: GetCustomisedHeader('CD_IssuedUTC')
					},
					{
						name: 'CD_ExpiryUTC',
						index: 'CD_ExpiryUTC',
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
						label: GetCustomisedHeader('CD_ExpiryUTC'),
					}

				],
				height: '500',
				width: '700',
				rowNum: 20,
				rowList: [20, 50, 100],
				caption: "Registrations",
				sortable: true,
				sortname: DEFAULT_SORT_COLUMN,
				sortorder: DEFAULT_SORT_ORDER,
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

					$cells.wrapInner("<span class='mywrapping'></span>");
					$colHeaders.wrapInner("<span class='mywrapping'></span>");

					for (iCol = 0; iCol < n; iCol++) {
						cm = colModel[iCol];
						colWidth = $("#" + idColHeadPrexif + $.jgrid.jqID(cm.name) + ">.mywrapping").outerWidth();
						for (iRow = 0, rows = this.rows; iRow < rows.length; iRow++) {
							row = rows[iRow];
							if ($(row).hasClass("jqgrow")) {
								colWidth = Math.max(colWidth, $(row.cells[iCol]).find(".mywrapping").outerWidth());
							}
						}
						colWidth = Math.min(colWidth + 10, 300);
						totalWidth += colWidth;
						$this.jqGrid("setColWidth", iCol, colWidth + 10);
						currentGridWidth = Math.min(1300, totalWidth + 100);
						grid.jqGrid('setGridWidth', currentGridWidth, false);
					}

					$cells.find(".mywrapping").contents().unwrap();
					$colHeaders.find(".mywrapping").contents().unwrap();
				},
				ondblClickRow: function (rowid) {
					jQuery(this).jqGrid('viewGridRow',
						rowid,
						{ // view option
							width: 400,
							closeAfterEdit: true,
							closeOnEscape: true,
							recreateForm: true,
							viewPagerButtons: false,
							resize: true,
						});
				}
			});

			$("#registrationsTable").jqGrid('navGrid', '#registrationsTablePager', { view : true},
				{// edit option
					width: 400,
					closeAfterEdit: true,
					closeOnEscape: true,
					recreateForm: true,
					viewPagerButtons: true,
					resize: true,
					beforeInitData: function (formid) {
						$('#registrationsTable').setColProp('CustomValue1', { editable: true });
						$('#registrationsTable').setColProp('CustomValue2', { editable: true });
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
				{//add option
					width: 400,
					closeAfterAdd: false,
					closeOnEscape: true,
					recreateForm: true,
					resize: true,
					beforeInitData: function (formid) {
						$('#registrationsTable').setColProp('CustomValue1', { editable: false });
						$('#registrationsTable').setColProp('CustomValue2', { editable: false });
						return (registrationTypePK != "");
					},
					beforeShowForm: function () {
						$('#cd_download').hide();
						$('#cd_upload').show();
						ModifyJQGridPOSTRequest("add");
					},
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
				},
				{
					recreateFilter: true,
					beforeShowSearch: function (searchForm) {
						searchForm.find('.columns').find('option').each( function (index, option) {
							option.text = GetCustomisedHeader(option.value);
						});
						return true;
					}
				},
				{ // view option
					width: 400,
					closeAfterEdit: true,
					closeOnEscape: true,
					recreateForm: true,
					viewPagerButtons: false,
					resize: true,
				}

			);

			$('#upload_configxmltarget').load(UploadconfigurationChanged);
			$("#uploadconfigxmlFile").change(UploadconfigurationChanged);

			function UploadconfigurationChanged() {
				if ($(this).val() == "") {
					$("#uploadConfiguration").attr("disabled", "disabled");


				} else {
					$("#uploadConfiguration").removeAttr("disabled");
				}
			};

			function FormatDownloadAndUpload(value, options) {
				var elemStr = '<span style="overflow: hidden;white-space: nowrap;">' + GetDownloadButtonForAddandEdit() + '<a id="cd_upload" class="fileUpload fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;height:25px;"><p>Upload</p><input id="cd_inputFile" name="cd_inputFile" type="file" style="position:absolute;top:0;right:0;margin:0;padding:0;cursor:pointer;opacity:0;height:25px;"/></a><label id="inputFile1">No file selected.</label></span>';
				var elem = $(elemStr)[0];
				$(elem).find('#cd_inputFile').change(function () {
					var o = this.value || 'No file selected.';
					var res = o.split("\\");
					var filename = res[res.length - 1];
					$('#inputFile1').text(filename);
				});
				return elem;
			}

			function ModifyJQGridPOSTRequest(oper) {
				$("#FrmGrid_registrationsTable").attr("id", "newFrmGrid_registrationsTable").removeAttr('onsubmit').append('<input type="hidden" name="oper" value="' + oper + '">')
					.attr('action', '<%: Url.Action("ClientSystemRegistrationsEdit") %>' + "?regType=" + registrationTypePK).attr('method', 'post').attr('enctype', 'multipart/form-data').attr('target', 'upload_target1')
					.append('<iframe id="upload_target1" name="upload_target1" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>');
				$("#sData").attr("id", "newsData");
				$("#CD_EH_ID_val").attr("name", "CD_EH_ID");
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

			function GetDownloadButtonForAddandEdit() {
				var selected_row = $('#registrationsTable').jqGrid('getGridParam', 'selrow');
				var cd_pk = $('#registrationsTable').getRowData(selected_row)["CD_PK"];
				return '<a href="<%: Url.Action("DownloadConfiguration") %>?CD_PK=' + cd_pk + '" id="cd_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;height:25px;" target="_blank">Download</a>';
			}

			function ConfigXmlOnLoading(CD_PK, options, rowObject) {
				return '<a href="<%: Url.Action("DownloadConfiguration") %>?CD_PK=' + CD_PK + '" id="cd_configxml_download" class="fm-button ui-state-default ui-corner-all" style="position:left;" target="_blank">Download</a>' +
					'<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayViewConfigXmlDialog(\'' + CD_PK + '\')"\>View</a>' +
					'<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayUploadConfigXmlDialog(\'' + CD_PK + '\', \'' + registrationTypePK + '\')">Upload</a>';
			}

			var clientEditState = {
				includeNonProd: false,
				checkbox: null,
			};

			function FormatClientEditFields(value, options) {
				var checkedAttribute = clientEditState.includeNonProd ? 'checked' : '';
				var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
					'" class="FormElement ui-widget-content ui-corner-all clientSystemIDValue" role="textbox" type="text" readonly="true"/>' +
					' <a class="fm-button ui-state-default ui-corner-all clientSystemIDSelect">Select</a>' +
					' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
					' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
					'</span>';
				var elem = $(elemStr)[0];
				$(elem).children('.clientSystemIDSelect').click(function () {
					DisplaySelectClientSystemDialog($(elem).children('.clientSystemIDValue'), clientEditState.includeNonProd);
				});

				$(elem).find('.clientIdCheckbox').change(function () {
					clientEditState.includeNonProd = $(this).is(':checked');
				});

				return elem;
			}


			function DisplaySelectClientSystemDialog(elem, includeNonProd) {
				$("#clientSystemsTable").jqGrid({
					datatype: 'json',
					url: '<%: Url.Action("ClientSystems") %>' + '?includeNonProd=' + includeNonProd,
					jsonReader: { root: 'eHubClientSystems', id: 'EH_ID', repeatitems: false },
					colNames: ['Client System ID', 'URL'],
					colModel: [
						{ name: 'EH_ID', width: 100, classes: "wrapped", sortable: true, search: true },
						{ name: 'EH_URL', width: 300, classes: "wrapped", sortable: true, search: true }
					],
					onSelectRow: function (id) {
						$('#selectClientSystemDialog #clientSystemID').val(id);
					},
					height: '400',
					width: '400',
					rowNum: 15,
					rowList: [15, 30, 50],
					sortname: 'EH_ID',
					sortorder: 'asc',
					pager: '#clientSystemsTablePager'
				});
				$("#clientSystemsTable").jqGrid('navGrid', '#clientSystemsTablePager', { search: false, refresh: false, add: false, edit: false, del: false });
				$("#clientSystemsTable").jqGrid('filterToolbar', { searchOnEnter: false });
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
				$("#clientSystemsTable").jqGrid("setGridParam", {
					url: '<%: Url.Action("ClientSystems") %>' + '?includeNonProd=' + includeNonProd,
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

		function displayViewConfigXmlDialog(CD_PK) {
			var cdConfigXml = jQuery.ajax({
				url: encodeURI('<%: Url.Action("DownloadConfiguration") %>' + '?CD_PK=' + CD_PK),
				async: false
			}).responseText;
			$('#viewConfigXmlDialog #configXmlText').text(cdConfigXml);

			$("#viewConfigXmlDialog").dialog({
				title: 'View Configuration Xml',
				width: 500,
				height: 700,
				resizable: true,
				modal: true
			});
		}

		function displayUploadConfigXmlDialog(CD_PK, registrationTypePK) {
			$('#uploadConfigXmlDialog #registrationPK').val(CD_PK);
			$('#uploadConfigXmlDialog #registrationTypePK').val(registrationTypePK);

			$("#uploadConfigXmlDialog").dialog({
				title: 'Upload Configuration Xml',
				width: "auto",
				height: "auto",
				resizable: true,
				modal: true
			});
		}

		function editConfigXML() {
			var selected_row = $('#registrationsTable').jqGrid('getGridParam', 'selrow');
			var CD_PK = $('#registrationsTable').getRowData(selected_row)["CD_PK"];
			return '<a href="<%: Url.Action("DownloadConfiguration") %>?CD_PK=' + CD_PK + '" id="cd_configxml_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;" target="_blank">Download</a>' +
				'<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayViewConfigXmlDialog(\'' + CD_PK + '\')"\>View</a>' +
				'<a class="fm-button ui-state-default ui-corner-all" style="position:absolute;" onclick="displayUploadConfigXmlDialog(\'' + CD_PK + '\', \'' + registrationTypePK + '\')">Upload</a>';

		}

		function validation_check(value, colname) {
			const searchOperator = $('.operators select').val();
			if (value == '') {
				return [false, "Value cannot be null"];
			}

			if (colname == 'Client System') {
				if (searchOperator == 'eq' && (value.length < 3 || value.length > 13)) {
					return [false, "Client System must be between 3 and 13"];
				}
			}

			if (colname == 'Qualifer') {
				if (value.length > 20) {
					return [false, "Qualifer must be less than 20 characters long"];
				}
			}
			// more conditions can be added later on
			return [true, ""];
		}

	</script>


</asp:Content>
