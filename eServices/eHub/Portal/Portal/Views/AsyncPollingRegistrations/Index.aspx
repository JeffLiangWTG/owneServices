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
	
	<div><label for="registrationTypesList">Async Polling Registrations</label></div>
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
		<div id="clientSystemsFields">
			<table id="clientSystemsTable"></table>
			<div id="clientSystemsTablePager"></div>
		</div>
		<form action="">
			<input type="hidden" name="clientSystemID" id="clientSystemID" />
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
    
    <div id="viewConfigXmlDialog" title="Configuration Xml" style="display: none">
        <p id="configXmlText" style="white-space: pre-wrap; font-size: 13px; word-wrap: break-word;
">
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

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

		$(document).ready(function () {

			var registrationTypePK = "";

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
				$("#registrationsTable").jqGrid('setGridParam', { search: false, postData: { "searchField": "", "searchString": "", "searchOper": ""} });
                ResetHeaders();
				if (registrationTypePK != "") {
					ReloadRegistrations();
					$("#viewRegistrationTypeButton").show();
				} else {
					$('#registrationsTable').clearGridData();
					$("#viewRegistrationTypeButton").hide();
				}
				return;
			}
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
                $('#registrationsTable').jqGrid('setGridWidth', Math.max(totalWidth, 500));
            }

			function ReloadRegistrations() {
                $('#registrationsTable').setGridParam({
					datatype: 'json',
					url: '<%: Url.Action("AsyncPollingRegistrations") %>' + '?regType=' + registrationTypePK,
					editurl: '<%: Url.Action("AsyncPollingRegistrationsEdit") %>' + '?regType=' + registrationTypePK
				}).trigger("reloadGrid");
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
                if (registrationTypePK == '') return jQuery.ajax({
                    url: encodeURI('<%: Url.Action("GetCustomisedHeaderForID") %>' + '?regTypeID=Default&columnName=' + columnName),
					async: false
				}).responseText;
				return jQuery.ajax({
					url: encodeURI('<%: Url.Action("GetCustomisedHeader") %>' + '?regType=' + registrationTypePK + '&columnName=' + columnName),
					async: false
				}).responseText;
            }

		    $("#registrationsTable").jqGrid({
		        datatype: 'local',
                jsonReader: { root: 'AsyncPollingRegistrations', id: 'PR_PK', repeatitems: false },
		        colModel: [
		            { name: 'PR_PK', hidden: true },
                    {
                        name: 'PR_CC_ID', index: 'PR_CC_ID',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'], searchhidden: false },
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientIdValue').val();
                            }
                        },
                        label: GetCustomisedHeader('PR_CC_ID')
                    },
                    {
		                name: 'PR_EH_ID',
		                index: 'PR_EH_ID',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientSystemEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientSystemIDValue').val();
                            }
                        },
		                label: GetCustomisedHeader('PR_EH_ID')
                    },
                    {
		                name: 'PR_Text',
                        index: 'PR_Text',
		                classes: "wrapped",
		                search: true,
		                searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
		                editable: false,
		                editoptions: { Size: 40 },
                        label: GetCustomisedHeader('PR_Text')
		            },
                    {
                        name: 'PR_CreatedUTC',
                        index: 'PR_CreatedUTC',
		                classes: "wrapped",
		                search: false,
		                sortable: false,
		                editable: false,
                        label: GetCustomisedHeader('PR_CreatedUTC')
                    },
                    {
                        name: 'PR_XML',
                        index: 'PR_XML',
                        hidden: false,
                        search: false,
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatDownloadAndUpload,
                            custom_value: function (elem) {
                                return $(elem).find('#pr_inputFile').val();
                            }
                        },
                        width: 300,
                        label: GetCustomisedHeader('PR_XML'),
                        formatter: ConfigXmlOnLoading
                    }
                ],
				height: '500',
				width: '800',
				rowNum: 20,
				rowList: [20, 50, 100],
				caption: "Registrations",
				sortable: true,
				sortname: 'PR_EH_ID',
				sortorder: 'asc',
                pager: '#registrationsTablePager',
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
            $("#registrationsTable").jqGrid('navGrid', '#registrationsTablePager', { view: true},
				{//Edit
					width: 400,
					closeAfterEdit: true,
					closeOnEscape: true,
					recreateForm: true,
					viewPagerButtons: true,
                    resize: true,
                    beforeShowForm: function () {
                        $('#pr_upload').hide();
                        $('#inputFile1').hide();
                    },
					afterSubmit: function (response, postdata) {
						var data = eval('(' + response.responseText + ')');
						return [data.success, data.message, data.id];
					}
				},
				{//Add
					width: 400,
					closeAfterAdd: false,
					closeOnEscape: true,
					recreateForm: true,
					resize: true,
					beforeInitData: function (formid) {
						return (registrationTypePK != "");
                    },
                    beforeShowForm: function () {
                        $('#pr_download').hide();
                        $('#pr_upload').show();
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
                    resize: true
                }
            );



            $("#uploadconfigxmlFile").change(UploadconfigurationChanged);
            $('#upload_configxmltarget').load(UploadconfigurationChanged);

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

			var clientSystemEditState = {
				includeNonProd: false,
				checkbox: null,
			};

            function FormatClientSystemEditFields(value, options) {
				var checkedAttribute = clientSystemEditState.includeNonProd ? 'checked' : '';
				var elemStr = '<span><input id="' + options.id + '_val" value="' + value +
					'" class="FormElement ui-widget-content ui-corner-all clientSystemIDValue" role="textbox" type="text" readonly="true"/>' +
					' <a class="fm-button ui-state-default ui-corner-all clientSystemIDSelect">Select</a>' +
					' <label for="' + options.id + '_val" class "clientIdLabel">Include Non-Prod CW1 Systems</label>' +
					' <input type="checkbox" class="clientIdCheckbox" id="' + options.id + '_checkbox" ' + checkedAttribute + '/>' +
					'</span>';
				var elem = $(elemStr)[0];
				$(elem).children('.clientSystemIDSelect').click(function () {
					DisplaySelectClientSystemDialog($(elem).children('.clientSystemIDValue'), clientSystemEditState.includeNonProd);
				});

				$(elem).find('.clientIdCheckbox').change(function () {
					clientSystemEditState.includeNonProd = $(this).is(':checked');
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
                        $('#selectClientSystemDialog #clientSystemID').val(id)
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
				includeNonProd = clientSystemEditState.includeNonProd;
				$("#clientSystemsTable").jqGrid("setGridParam", {
					url: '<%: Url.Action("ClientSystems") %>' + '?includeNonProd=' + includeNonProd,
				}).trigger("reloadGrid");
            }

			function DisplayError(errorMsg) {
				$('<div><p class="ui-state-error">' + errorMsg + '<\/p><\/div>').dialog({ title: 'Error', modal: true, width: "auto" });
            }

            function FormatDownloadAndUpload(value, options) {
                var elemStr = '<span style="overflow: hidden;white-space: nowrap;">' + GetDownloadButtonForAddandEdit() + '<a id="pr_upload" class="fileUpload fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;height:25px;"><p>Upload</p><input id="pr_inputFile" name="pr_inputFile" type="file" style="position:absolute;top:0;right:0;margin:0;padding:0;cursor:pointer;opacity:0;height:25px;"/></a><label id="inputFile1">No file selected.</label></span>';
                var elem = $(elemStr)[0];
                $(elem).find('#pr_inputFile').change(function () {
                    var o = this.value || 'No file selected.';
                    var res = o.split("\\");
                    var filename = res[res.length - 1];
                    $('#inputFile1').text(filename);
                });
                return elem;
            }

            function ModifyJQGridPOSTRequest(oper) {
                $("#FrmGrid_registrationsTable").attr("id", "newFrmGrid_registrationsTable").removeAttr('onsubmit').append('<input type="hidden" name="oper" value="' + oper + '">')
                    .attr('action', '<%: Url.Action("AsyncPollingRegistrationsEdit") %>' + "?regType=" + registrationTypePK).attr('method', 'post').attr('enctype', 'multipart/form-data').attr('target', 'upload_target1')
                    .append('<iframe id="upload_target1" name="upload_target1" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>');
                $("#sData").attr("id", "newsData");
                $("#PR_CC_ID_val").attr("name", "PR_CC_ID");
                $("#PR_EH_ID_val").attr("name", "PR_EH_ID");
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
                var pr_pk = $('#registrationsTable').getRowData(selected_row)["PR_PK"];
                return '<a href="<%: Url.Action("DownloadConfiguration") %>?PR_PK=' + pr_pk + '" id="pr_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;height:25px;" target="_blank">Download</a>';
            }

            function ConfigXmlOnLoading(PR_PK, options, rowObject) {
                return '<a href="<%: Url.Action("DownloadConfiguration") %>?PR_PK=' + PR_PK + '" id="pr_configxml_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;" target="_blank">Download</a>' +
                    '<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayViewConfigXmlDialog(\'' + PR_PK + '\')"\>View</a>' +
                    '<a class="fm-button ui-state-default ui-corner-all" style="position:relative;" onclick="displayUploadConfigXmlDialog(\'' + PR_PK + '\', \'' + registrationTypePK + '\')">Upload</a>';

            }

            function UploadconfigurationChanged() {
                if ($(this).val() == "") {
                    $("#uploadConfiguration").attr("disabled", "disabled");


                } else {
                    $("#uploadConfiguration").removeAttr("disabled");
                }
            };
        });

        function displayViewConfigXmlDialog(PR_PK) {
            var prConfigXml = jQuery.ajax({
                url: encodeURI('<%: Url.Action("DownloadConfiguration") %>' + '?PR_PK=' + PR_PK),
                async: false
            }).responseText;
            $('#viewConfigXmlDialog #configXmlText').text(prConfigXml);

            $("#viewConfigXmlDialog").dialog({
                title: 'View Configuration Xml',
                width: 500,
                height: 700,
                resizable: true,
                modal: true
            });
        }

        function displayUploadConfigXmlDialog(PR_PK, registrationTypePK) {
            $('#uploadConfigXmlDialog #registrationPK').val(PR_PK);
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
