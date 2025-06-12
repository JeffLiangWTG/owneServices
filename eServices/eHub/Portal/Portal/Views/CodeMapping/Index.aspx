<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">

	<link href="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.css")%>" rel="Stylesheet" type="text/css" />
	<link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
	<link href="<%: Url.Content("~/PlugIns/jqGrid/ui.jqgrid.css")%>" rel="Stylesheet" type="text/css" media="screen" />

	<script src="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.min.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jqGrid/grid.locale-en.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/jqGrid/jquery.jqGrid.min.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/JeeGooContext/jquery.jeegoocontext.js")%>" type="text/javascript"></script>
	<script src="<%: Url.Content("~/PlugIns/JSON/json2.js")%>"type="text/javascript"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	
	<div id="errorMessage" class="error" style="display: none"></div>
	
	<fieldset id="filterFields" class="ui-widget-header">
		<ul>
			<li>
				<div><label for="sendersList">Sender</label></div>
				<div>
					<select id="sendersList" class="ui-widget"></select>
					<img id="sendersListThrobber" src='<%: Url.Content("~/Content/images/throbber.gif") %>' alt='Loading ...' style="display: none" class="throbber"/>
				</div>
			</li>
			<li>
				<div><label for="recipientsList">Recipient</label></div>
				<div>
					<select id="recipientsList" class="ui-widget"></select>
					<img id="recipientsListThrobber" src='<%: Url.Content("~/Content/images/throbber.gif") %>' alt='Loading ...' style="display: none" class="throbber"/>
				</div>
			</li>
			<li>
				<div><label for="transformationSetsList">Interface</label></div>
				<div>
					<select id="transformationSetsList" class="ui-widget"></select>
					<img id="transformationSetsListThrobber" src='<%: Url.Content("~/Content/images/throbber.gif") %>' alt='Loading ...' style="display: none" class="throbber"/>
				</div>
			</li>
		</ul>
	</fieldset>
	<br />
	<div id="dataAreaHolder">
		<div id="codeSetsFields" style="display: none">
			<table id="codeSetsTable"></table>
			<div id="codeSetsTablePager"></div>
		</div>
		<div id="codeMapsFields" style="display: none">
			<div id="codeMapsLoader" class="loader" style="display: none" >
				<div class="loader ui-state-default ui-state-active">Loading...</div>
			</div>
			<div id="codeMapsTableArea">
				<table id="codeMapsTable"></table>
				<div id="codeMapsTablePager"></div>
			</div>
			<div id="codeMapsButtons">
				<div id="codeMapsUpDownBtns">
					<button id="codeMapsUpBtn" disabled="disabled" class="ui-widget">Move Up</button>
					<button id="codeMapsDownBtn" disabled="disabled" class="ui-widget">Move Down</button>
				</div>
				<div id="codeMapsSaveCancelBtns">
					<button id="codeMapsSaveBtn" disabled="disabled" class="ui-widget">Save</button>
					<button id="codeMapsCancelBtn" disabled="disabled" class="ui-widget">Cancel</button>
				</div>
				<form id="importCsvForm" action="<%: Url.Action("ImportCsvFile") %>" enctype="multipart/form-data" method="post" target="upload_target" >
					<br />
					<input type="hidden" id="codeset" name="codeset" value="" />
					<input type="hidden" id="codeMapsData" name="codeMapsData" value="" />
					<input type="submit" name="importCsv" id="importCsv" value="Import from CSV" disabled="disabled" class="ui-widget"/>
					<select name="merge" id="merge" class="ui-widget">
						<option value="merge">Merge</option>
						<option value="override">Override</option>
					</select>
					<input type="file" name="uploadFile" id="uploadFile" class="ui-widget" />
					<iframe id="upload_target" name="upload_target" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>
				</form>
				<button id="exportCsv" class="ui-widget">Export to CSV</button>
				<div class="textBlockGroup"><div class="textBlock">Special Code Map Input Values:&nbsp;</div><div class="textBlock"><b>#BLANK#,<br />*</b> (wildcard)</div></div>
				<div class="textBlockGroup"><div class="textBlock">Special Code Map Output Values:&nbsp;</div><div class="textBlock"><b>#BLANK#,<br />#INPUTFIELD</b><i>[1-5]</i><b>#</b><br />(for mapping from input column)</div></div>
			</div>
		</div>
		<div id="codeSetsEditFields" style="display: none">
			<div id="codeSetProperties">
				<br />
				<span class="ui-widget">Code Set Name: </span>
				<input id="codeSetName" value="" class="ui-widget" />
				<br /><br />
			</div>
			<div id="codeSetsKeysTableArea">
				<table id="codeSetsKeysTable"></table>
				<div id="codeSetsKeysTablePager"></div>
				<div id="codeSetsKeysUpDownButtons">
					<button id="codeSetsKeysUpBtn" disabled="disabled" class="ui-widget">Move Up</button>
					<button id="codeSetsKeysDownBtn" disabled="disabled" class="ui-widget">Move Down</button>
				</div>
			</div>
			<div id="codeSetsResultsTableArea">
				<table id="codeSetsResultsTable"></table>
				<div id="codeSetsResultsTablePager"></div>
				<div id="codeSetsResultsUpDownButtons">
					<button id="codeSetsResultsUpBtn" disabled="disabled" class="ui-widget">Move Up</button>
					<button id="codeSetsResultsDownBtn" disabled="disabled" class="ui-widget">Move Down</button>
				</div>
			</div>
			<div id="codeSetsEditButtons">
				<input type="submit" id="codeSetsSaveBtn" disabled="disabled" value="Save" class="ui-widget"/>
				<input type="submit" id="codeSetsCancelBtn" value="Cancel" class="ui-widget"/>
			</div>
		</div>
	</div>

	<ul id="codeSetsContextMenu" class="jeegoocontext cm_default" style="display: none">
		<li class="menugroup">
			<a>Assign to Interface</a>
			<ul id="codeSetsMenuAssign" ></ul>
		</li>
		<li class="menugroup">
			<a>Copy to Interface</a>
			<ul id="codeSetsMenuCopy" ></ul>
		</li>
	</ul>

	<div id="copyAssignCodeSetDlg" title="Copy Code Set" style="display: none">
		<form action="">
			<p id="errorMsg" class="ui-state-error"></p>
			<label for='name'>Please specify a new name:</label>
			<input type="text" name="name" id="name" class="text ui-widget-content ui-corner-all" />
		</form>
	</div>

	<div id="codeMapsDirtyWarning" title="Warning" style="display: none;">
		<p class="ui-state-error">Details have not been saved.<br/><br/>Do you wish to continue and lose your changes? Otherwise press cancel and then save.</p>
	</div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

	    $(document).ready(function () {

            var isDirty = false;
            window.onload = function() {
                window.addEventListener("beforeunload", function(e) {
                    if (!isDirty) {
                        return undefined;
                    }

                    (e || window.event).returnValue = "";

                    return "";
                });
            };

	        var senderPK = "";
	        var recipientPK = "";
	        var transformationPK = "";
	        var codesetPK = "";
	        var codesetAddEditInd = "";

	        var codeBlank = '#BLANK#';

	        loadSendersList();

	        $('#errorMessage').ajaxError(function (event, request, settings, exception) {
	            $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
	            $(this).show();
	        }).ajaxComplete(function () {
	            $(this).hide()
	        });

	        function positionOf(arr, val) {
	            for (var i = 0; i < arr.length; i++) {
	                if (arr[i] == val) {
	                    return i;
	                }
	            }
	            return -1;
	        }

	        /**************************************
	        *  Senders
	        **************************************/

	        function loadSendersList() {
	            senderPK = "";
	            resetRecipientsList();
	            $("#sendersList").attr("disabled", "disabled");
	            $("#sendersList").html("<option value=''>Loading ...<\/option>");
	            $("#sendersListThrobber").show();
	            var url = '<%: Url.Action("Senders") %>';
	            $.getJSON(url, null, function (data) {
	                $("#sendersList").html("<option value=''>-- Select Sender --<\/option>");
	                $.each(data.eHubClients, function (index, optionData) {
	                    $("#sendersList").append("<option value='" + optionData.CC_PK + "'>" + optionData.CC_ID + " - " + optionData.CC_FriendlyName + "<\/option>");
	                });
	                $("#sendersList").removeAttr("disabled");
	                $("#sendersList").focus();
	                $("#sendersListThrobber").hide();
	            });
	        }

	        var senderChangeHandler = function () {
                var currentPK = $("#sendersList > option:selected").attr("value");
	            if (senderPK == currentPK)
                    return;
                if ($('#codeMapsSaveBtn').attr('disabled') && $('#codeSetsSaveBtn').attr('disabled')) {
	                senderPK = currentPK;
	                resetRecipientsList();
	                if (senderPK != "") {
	                    loadRecipientsList();
	                }
	                return;
                }
                $('#codeMapsDirtyWarning').dialog({
	                modal: true,
	                resizable: false,
	                buttons: {
                        'Continue (lose changes)': function () {
	                        $(this).dialog('close');
	                        senderPK = currentPK;
	                        resetRecipientsList();
	                        if (senderPK != "") {
                                loadRecipientsList();
                            }
                        },
	                    Cancel: function () {
	                        $(this).dialog('close');
	                        $("#sendersList").val(senderPK);
	                    }
	                }
                });
                $('#codeMapsDirtyWarning').dialog('open');
            }
            $("#sendersList").change(senderChangeHandler).keyup(senderChangeHandler);


	        /**************************************
	        *  Recipients
	        **************************************/

	        function resetRecipientsList() {
	            recipientPK = "";
	            resetTransformationSetsList();
	            $("#recipientsList").attr("disabled", "disabled");
	            $("#recipientsList").empty();
	        }

	        function loadRecipientsList() {
	            $("#recipientsList").html("<option value=''>Loading ...<\/option>");
	            $("#recipientsListThrobber").show();
	            var url = '<%: Url.Action("Recipients") %>' + '?sender=' + senderPK;
	            $.getJSON(url, null, function (data) {
	                $("#recipientsList").html("<option value=''>-- Select Recipient --<\/option>");
	                $.each(data.eHubClients, function (index, optionData) {
	                    $("#recipientsList").append("<option value='" + optionData.CC_PK + "'>" + optionData.CC_ID + " - " + optionData.CC_FriendlyName + "<\/option>");
	                });
	                $("#recipientsList").removeAttr("disabled");
	                $("#recipientsListThrobber").hide();
	            });
	        }

	        var recipientChangeHandler = function () {
                var currentPK = $("#recipientsList > option:selected").attr("value");
	            if (recipientPK == currentPK) {
	                return;
                }
                if ($('#codeMapsSaveBtn').attr('disabled') && $('#codeSetsSaveBtn').attr('disabled')) {
	                recipientPK = currentPK;
	                resetTransformationSetsList();
	                if (recipientPK != "") {
	                    loadTransformationSetsList();
	                }
	                return;
                }
                $('#codeMapsDirtyWarning').dialog({
	                modal: true,
	                resizable: false,
	                buttons: {
                        'Continue (lose changes)': function () {
                            $(this).dialog('close');
	                        recipientPK = currentPK;
	                        resetTransformationSetsList();
	                        if (recipientPK != "") {
	                            loadTransformationSetsList();
	                        }
	                    },
	                    Cancel: function () {
	                        $(this).dialog('close');
	                        $("#recipientsList").val(recipientPK);
	                    }
	                }
	            });
                $('#codeMapsDirtyWarning').dialog('open');
            }
	        $("#recipientsList").change(recipientChangeHandler).keyup(recipientChangeHandler);


	        /**************************************
	        *  Transformation Sets
	        **************************************/

	        function resetTransformationSetsList() {
	            $("#transformationSetsList").attr("disabled", "disabled");
	            $("#transformationSetsList").empty();
	            $("#codeSetsFields").hide();
	            $("#codeSetsEditFields").hide();
	            $("#codeMapsFields").hide();
	            $("#codeMapsSaveBtn, #codeMapsCancelBtn, #codeSetsSaveBtn").attr("disabled", "disabled");
	            transformationPK = null;
	        }

            function loadTransformationSetsList() {
	            $("#transformationSetsList").html("<option value=''>Loading ...<\/option>");
	            $("#transformationSetsListThrobber").show();
	            var url = '<%: Url.Action("TransformationSets") %>' + '?sender=' + senderPK + '&recipient=' + recipientPK;
	            $.getJSON(url, null, function (data) {
	                $("#transformationSetsList").html("<option value=''>(unassigned)<\/option>");
	                $.each(data.eHubTransformationSets, function (index, optionData) {
	                    $("#transformationSetsList").append("<option value='" + optionData.TS_PK + "'>" + optionData.TS_Name + "<\/option>");
	                });
	                $("#transformationSetsList").removeAttr("disabled");
	                $("#transformationSetsListThrobber").hide();
	                $("#codeSetsFields").show();
	                LoadCodeSetsTable();
	            });
	        }

            var transformationChangeHandler = function () {
                var currentPK = $("#transformationSetsList > option:selected").attr("value");
                if (transformationPK == currentPK) 
                    return;
                if ($('#codeMapsSaveBtn').attr('disabled') && $('#codeSetsSaveBtn').attr('disabled')) {
                    transformationPK = currentPK;
                    LoadCodeSetsTable();
                    return;
                }
                $('#codeMapsDirtyWarning').dialog({
	                modal: true,
	                resizable: false,
	                buttons: {
                        'Continue (lose changes)': function () {
                            $(this).dialog('close');
	                        transformationPK = currentPK;
	                        LoadCodeSetsTable();
	                    },
	                    Cancel: function () {
	                        $(this).dialog('close');
	                        $("#transformationSetsList").val(transformationPK);
	                    }
	                }
	            });
                $('#codeMapsDirtyWarning').dialog('open');
            }
            $("#transformationSetsList").change(transformationChangeHandler).keyup(transformationChangeHandler);


	        /**************************************
	        *  Code Sets Table
	        **************************************/

            function LoadCodeSetsTable(selectCodeset) {
	            codesetPK = selectCodeset;
	            $("#codeSetsMenuAssign").empty();
	            $("#codeSetsMenuCopy").empty();
	            $('#transformationSetsList > option').each(function () {
	                if (!$(this).attr('selected')) {
	                    $("#codeSetsMenuAssign").append("<li class='codeSetAssign'>" + this.innerText + "<\/li>");
	                }
	                $("#codeSetsMenuCopy").append("<li class='codeSetCopy'>" + this.innerText + "<\/li>");
	            });
	            $('#codeSetsTable').clearGridData();
	            $('#codeSetsFields').show();
	            $('#codeSetsTable').setGridParam({
	                datatype: 'json',
	                url: '<%: Url.Action("CodeSets") %>',
	                postData: {
	                    sender: senderPK,
	                    recipient: recipientPK,
	                    transformation: transformationPK
	                }
	            }).trigger("reloadGrid");
	        }

	        $("#codeSetsTable").jqGrid({
	            datatype: 'local',
	            mtype: 'GET',
	            jsonReader: {
	                root: 'eHubCodeSets',
	                id: 'CS_PK',
	                repeatitems: false
	            },
	            colNames: ['Code Set'],
	            colModel: [
					{
					    name: 'CS_Name',
					    classes: 'CS_Name',
					    sortable: false,
					    width: '200',
					    editable: true,
					    editoptions: {
					        maxlength: '25'
					    }
					},
				],
	            height: '300',
	            scroll: true,
	            scrollrows: true,
	            rowNum: 999999,
	            pager: '#codeSetsTablePager',
	            loadComplete: function (data) {
	                if (data.eHubCodeSets != null && data.eHubCodeSets.length > 0) {
	                    if (codesetPK != null) {
	                        $("#codeSetsTable").setSelection(codesetPK, false);
	                        LoadCodeMapsTable();
	                    } else {
	                        $("#codeSetsTable").setSelection(data.eHubCodeSets[0].CS_PK, true);
	                    }
	                } else {
	                    $("#codeMapsFields").hide();
	                }
	            },
	            gridComplete: function () {
	                $("#codeSetsTable .CS_Name").jeegoocontext('codeSetsContextMenu', {
	                    fadeIn: 0,
	                    onShow: function (e, context) {
	                        if ($(context).parent().attr('id') != codesetPK) {
	                            $("#codeSetsTable").setSelection($(context).parent().attr('id'), true);
	                        }
	                    },
	                    onSelect: function (e, context) {
	                        if ($(this).hasClass("menugroup")) { return false; }
	                        $('#copyAssignCodeSetDlg #name').val($(context).text());
	                        if ($(this).hasClass("codeSetAssign")) {
	                            MoveOrCopyCodeSet($(this).text(), 'Assign', '');
	                        } else if ($(this).hasClass("codeSetCopy")) {
	                            if ($("#transformationSetsList > option:selected").text() == $(this).text()) {
	                                DisplayCopyAssignCodeSetDialog($(this).text(), 'Copy');
	                            } else {
	                                MoveOrCopyCodeSet($(this).text(), 'Copy', '');
	                            }
	                        }
	                    }
	                });
	            },
	            onSelectRow: function (rowid, status) {
	                if (rowid != codesetPK) {
	                    codesetPK = rowid;
	                    LoadCodeMapsTable();
	                }
	            },
                beforeSelectRow: function (rowid, e) {
                    if (rowid == codesetPK) {
	                    return false;
	                }
	                if ($('#codeMapsSaveBtn').attr('disabled') && $('#codeSetsSaveBtn').attr('disabled')) {
	                    return true;
                    }
                    $('#codeMapsDirtyWarning').dialog({
	                    modal: true,
	                    resizable: false,
	                    buttons: {
                            'Continue (lose changes)': function () {
                                $(this).dialog('close');
	                            $('#codeSetsTable').setSelection(rowid, false);
	                            codesetPK = rowid;
	                            LoadCodeMapsTable();
	                            return false;
	                        },
	                        Cancel: function () {
	                            $(this).dialog('close');
	                            return false;
	                        }
	                    }
	                });
                    $('#codeMapsDirtyWarning').dialog('open');
                }
	        })
	        $('#codeMapsDirtyWarning').dialog({ autoOpen: false });

	        $('#codeSetsTable').jqGrid('navGrid', '#codeSetsTablePager',
				{
				    refresh: false,
				    search: false,
				    editfunc: function (id) {
				        $('<div><p class="ui-state-error">Changes to code set definitions MUST be co-ordinated with changes to transformations.<br/><br/>Do you wish to proceed?<\/p><\/div>').dialog({
				            title: 'Warning',
				            modal: true,
				            resizable: false,
				            buttons: {
                                OK: function () {
                                    isDirty = true;
				                    $(this).dialog("close");
				                    codesetAddEditInd = 'edit';
				                    $('#codeMapsFields').hide();
				                    $('#codeSetsKeysTable').clearGridData();
				                    $('#codeSetsResultsTable').clearGridData();  
				                    ToggleKeyUpDownButtons('-1');
				                    ToggleResultUpDownButtons('-1');
				                    $('#codeSetsSaveBtn').attr('disabled', 'disabled');
				                    $('#codeSetName').val($('#codeSetsTable').jqGrid('getCell', id, 0));
				                    $('#codeSetsEditFields').show();
				                    $.getJSON('<%: Url.Action("CodeSetDetails") %>' + '?codeset=' + id, function (data) {
				                        $(data.codesetKeys).each(function (i, key) {
				                            $('#codeSetsKeysTable').jqGrid('addRowData', i + 1, key, 'last');
				                        });
				                        $(data.codesetResults).each(function (i, result) {
				                            $('#codeSetsResultsTable').jqGrid('addRowData', i + 1, result, 'last');
				                        });
				                    });
				                },
				                Cancel: function () {
				                    $(this).dialog("close");
				                }
				            }
				        });
				    },
                    addfunc: function () {
				        codesetAddEditInd = 'add';
				        $('#codeMapsFields').hide();
				        $('#codeSetsKeysTable').clearGridData();
				        $('#codeSetsResultsTable').clearGridData();
				        ToggleKeyUpDownButtons('-1');
				        ToggleResultUpDownButtons('-1');
				        $('#codeSetsSaveBtn').removeAttr('disabled');
				        $('#codeSetName').val('New Code Set');
                        $('#codeSetsEditFields').show();
                        isDirty = true;
                    }
				},
				{},
				{},
				{
				    resize: false,
				    closeOnEscape: true,
				    beforeShowForm: function (formid) {
				        $('<div><p class="ui-state-error">Changes to code set definitions MUST be co-ordinated with changes to transformations.<br/><br/>Do you wish to proceed?<\/p><\/div>').dialog({
				            title: 'Warning',
				            modal: true,
				            resizable: false,
				            buttons: {
				                OK: function () {
				                    $(this).dialog("close");
				                },
				                Cancel: function () {
				                    $('#editmodcodeSetsKeysTable').remove();
				                    $(this).dialog("close");
				                }
				            }
				        });
				    },
                    onclickSubmit: function (params, postdata) {
                        isDirty = false;
                        $('#codeSetsTable').setGridParam({
				            editurl: '<%: Url.Action("DeleteCodeSet") %>' + '?codeset=' + codesetPK
				        });
				    },
				    afterSubmit: function (response, postdata) {
				        codesetPK = $('#codeSetsTable tbody tr').not('.jqgfirstrow').not('#' + codesetPK)[0].id;
				        return [true, ''];
				    },
				    errorTextFormat: FormatAjaxError
				}
			);

	        function ValidateCodeSetData(postdata, formid) {
	            var rowsToCheck;
	            if (postdata.codeMapsTable_id == '_empty') {
	                rowsToCheck = $('#codeSetsTable tbody tr');
	            } else {
	                rowsToCheck = $('#codeSetsTable tbody tr').not('#' + postdata.codeSetsTable_id);
	            }

	            if (rowsToCheck.children('.CS_Name').filter(function () {
	                return this.innerText.toUpperCase() == postdata.CS_Name.toUpperCase();
	            }).length > 0)
	                return [false, 'Duplicate Code Set Name'];

	            return [true, ''];
	        };

	        function FormatAjaxError(response) {
	            if (response.status == 500)
	                return 'A system error has occured. Please contact support.';
	        }

	        function AppendCodeSetParameters() {
	            return {
	                sender: senderPK,
	                recipient: recipientPK,
	                transformation: transformationPK
	            }
	        }

	        $('#copyAssignCodeSetDlg').dialog({ autoOpen: false });
	        $('#copyAssignCodeSetDlg #name').focus(function () {
	            this.value = this.value;
	        });

	        function DisplayCopyAssignCodeSetDialog(transformationName, action, error) {
	            $('#copyAssignCodeSetDlg').dialog({
	                autoOpen: false,
	                modal: true,
	                buttons: {
	                    Submit: function () {
	                        $(this).dialog('close');
	                        MoveOrCopyCodeSet(transformationName, action, $('#copyAssignCodeSetDlg #name').val());
	                    },
	                    Cancel: function () {
	                        $(this).dialog('close');
	                    }
	                }
	            });
	            if (action == 'Copy') {
	                $('#copyAssignCodeSetDlg').dialog('option', 'title', 'Copy Code Set');
	            } else {
	                $('#copyAssignCodeSetDlg').dialog('option', 'title', 'Assign Code Set');
	            }
	            if (error) {
	                $('#copyAssignCodeSetDlg #errorMsg').show();
	                $('#copyAssignCodeSetDlg #errorMsg').text(error);
	            } else {
	                $('#copyAssignCodeSetDlg #errorMsg').hide();
	            }
	            $('#copyAssignCodeSetDlg #name').focus();
	            $('#copyAssignCodeSetDlg').dialog('open');
	        }

	        function MoveOrCopyCodeSet(transformationName, action, name) {
	            var toTransformationPK = $("#transformationSetsList > option").filter(function () {
	                return $.trim($(this).text()) == transformationName
	            }).val();
	            var url;
	            if (action == 'Copy') {
	                url = '<%: Url.Action("CopyCodeSet") %>';
	            } else {
	                url = '<%: Url.Action("AssignCodeSet") %>';
	            }
	            url += "?codeset=" + codesetPK + "&transformation=" + toTransformationPK + "&name=";
	            $.ajax({
	                type: "POST",
	                async: false,
	                url: url + name,
	                success: function (data) {
	                    if (data.success) {
	                        if (toTransformationPK == transformationPK) {
	                            $('#codeSetsTable').trigger('reloadGrid');
	                        } else {
	                            $('#transformationSetsList').val(toTransformationPK);
	                            $('#transformationSetsList').change();
	                        }
	                    } else {
	                        if (data.duplicate) {
	                            DisplayCopyAssignCodeSetDialog(transformationName, action, 'Name conflicts with existing map.');
	                        }
	                        else {
	                            DisplayError(data.error);
	                        }
	                    }
	                },
	                error: function (data) {
	                    DisplayError(data.responseText);
	                }
	            });
	        }

	        $('#codeSetName').change(function () {
	            $("#codeSetsSaveBtn").removeAttr("disabled");
	        });

	        $('#codeSetsKeysTable').jqGrid({
	            datatype: 'local',
	            colNames: ['Pos', 'Key Fields (max 5)'],
	            colModel: [
					{
					    name: 'pos',
					    sortable: false,
					    hidden: true,
					    editable: false
					},
					{
					    name: 'keyName',
					    sortable: false,
					    width: '200',
					    editable: true,
					    editoptions: {
					        maxlength: '250'
					    }
					},
				],
	            height: '150',
	            gridview: true,
	            scroll: true,
	            sortable: false,
	            pager: '#codeSetsKeysTablePager',
	            editurl: '<%: Url.Content("~/.") %>',
	            beforeSelectRow: function (rowid, e) {
	                if (rowid == $(this).getGridParam('selrow')) {
	                    ToggleKeyUpDownButtons('-1');
	                }
	                return true;
	            },
	            onSelectRow: ToggleKeyUpDownButtons
	        });

	        $('#codeSetsKeysTable').jqGrid('navGrid', '#codeSetsKeysTablePager',
				{
				    refresh: false,
				    search: false
				},
				{
				    width: 320,
				    resize: false,
				    closeOnEscape: true,
				    closeAfterEdit: true,
				    reloadAfterSubmit: false,
				    recreateForm: true,
				    viewPagerButtons: false,
				    beforeSubmit: function (postdata, formid) {
				        if ($('#codeSetsKeysTable tbody tr').not('.jqgfirstrow').not('#' + postdata.codeSetsKeysTable_id).find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.keyName.toUpperCase(); }).length > 0
							|| $('#codeSetsResultsTable tbody tr').not('.jqgfirstrow').find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.keyName.toUpperCase(); }).length > 0) {
				            return [false, 'Duplicate Field Name.'];
				        } else {
				            return [true, ''];
				        }
				    },
				    afterComplete: function (response, postdata, formid) {
				        $("#codeSetsSaveBtn").removeAttr("disabled");
				    }
				},
				{
				    width: 320,
				    resize: false,
				    closeOnEscape: true,
				    reloadAfterSubmit: false,
				    closeAfterAdd: true,
				    recreateForm: true,
				    addedrow: 'last',
				    beforeShowForm: function (formid) {
				        cnt = $('#codeSetsKeysTable').getGridParam('records');
				        if (cnt >= 5) {
				            $('#editmodcodeSetsKeysTable').remove();
				        }
				        if (codesetAddEditInd == 'edit' && cnt == 0) {
				            $('#editmodcodeSetsKeysTable').remove();
				            DisplayError('Cannot convert an unkeyed code set to a keyed one.');
				        }
				    },
				    beforeSubmit: function (postdata, formid) {
				        if ($('#codeSetsKeysTable tbody tr').not('.jqgfirstrow').find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.keyName.toUpperCase(); }).length > 0
							|| $('#codeSetsResultsTable tbody tr').not('.jqgfirstrow').find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.keyName.toUpperCase(); }).length > 0) {
				            return [false, 'Duplicate Field Name.'];
				        } else {
				            return [true, ''];
				        }
				    },
				    afterComplete: function (response, postdata, formid) {
				        $("#codeSetsSaveBtn").removeAttr("disabled");
				    }
				},
				{
				    resize: false,
				    reloadAfterSubmit: false,
				    beforeShowForm: function (formid) {
				        if (codesetAddEditInd == 'edit' && $('#codeSetsKeysTable').getGridParam('records') <= 1) {
				            $('#delmodcodeSetsKeysTable').remove();
				            DisplayError('Cannot convert a keyed code set to an unkeyed one.');
				        }
				    },
				    afterComplete: function (response, postdata, formid) {
				        ToggleKeyUpDownButtons('-1');
				        $("#codeSetsSaveBtn").removeAttr("disabled");
				    }
				}
			);

	        $('#codeSetsResultsTable').jqGrid({
	            datatype: 'local',
	            colNames: ['Key', 'Result Fields'],
	            colModel: [
					{
					    name: 'key',
					    sortable: false,
					    hidden: true,
					    editable: false
					},
					{
					    name: 'resultName',
					    sortable: false,
					    width: '200',
					    editable: true,
					    editoptions: {
					        maxlength: '250'
					    }
					},
				],
	            height: '150',
	            gridview: true,
	            scroll: true,
	            sortable: false,
	            pager: '#codeSetsResultsTablePager',
	            editurl: '<%: Url.Content("~/.") %>',
	            beforeSelectRow: function (rowid, e) {
	                if (rowid == $(this).getGridParam('selrow')) {
	                    ToggleResultUpDownButtons('-1');
	                }
	                return true;
	            },
	            onSelectRow: ToggleResultUpDownButtons
	        });

	        $('#codeSetsResultsTable').jqGrid('navGrid', '#codeSetsResultsTablePager',
				{
				    refresh: false,
				    search: false
				},
				{
				    width: 320,
				    resize: false,
				    closeOnEscape: true,
				    closeAfterEdit: true,
				    reloadAfterSubmit: false,
				    recreateForm: true,
				    viewPagerButtons: false,
				    beforeSubmit: function (postdata, formid) {
				        if ($('#codeSetsResultsTable tbody tr').not('.jqgfirstrow').not('#' + postdata.codeSetsResultsTable_id).find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.resultName.toUpperCase(); }).length > 0
							|| $('#codeSetsKeysTable tbody tr').not('.jqgfirstrow').find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.resultName.toUpperCase(); }).length > 0) {
				            return [false, 'Duplicate Field Name.'];
				        } else {
				            return [true, ''];
				        }
				    },
				    afterComplete: function (response, postdata, formid) {
				        $("#codeSetsSaveBtn").removeAttr("disabled");
				    }
				},
				{
				    width: 320,
				    resize: false,
				    closeOnEscape: true,
				    reloadAfterSubmit: false,
				    closeAfterAdd: true,
				    recreateForm: true,
				    addedrow: 'last',
				    beforeSubmit: function (postdata, formid) {
				        if ($('#codeSetsResultsTable tbody tr').not('.jqgfirstrow').find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.resultName.toUpperCase(); }).length > 0
							|| $('#codeSetsKeysTable tbody tr').not('.jqgfirstrow').find('td:visible').filter(function () { return this.innerText.toUpperCase() == postdata.resultName.toUpperCase(); }).length > 0) {
				            return [false, 'Duplicate Field Name.'];
				        } else {
				            return [true, ''];
				        }
				    },
				    afterComplete: function (response, postdata, formid) {
				        $("#codeSetsSaveBtn").removeAttr("disabled");
				    }
				},
				{
				    resize: false,
				    reloadAfterSubmit: false,
				    afterComplete: function (response, postdata, formid) {
				        ToggleResultUpDownButtons('-1');
				        $("#codeSetsSaveBtn").removeAttr("disabled");
				    }
				}
			);

	        $("#codeSetsKeysUpBtn").click(function () {
	            MoveKey('up');
	        });

	        $("#codeSetsKeysDownBtn").click(function () {
	            MoveKey('down');
	        });

	        function MoveKey(dir) {
	            var sel = $("#codeSetsKeysTable").getGridParam('selrow');
	            var ids = $("#codeSetsKeysTable").getDataIDs();
	            var pos = positionOf(ids, sel);
	            var rel = dir == 'up' ? pos - 1 : pos + 1;
	            var place = dir == 'up' ? 'before' : 'after';
	            var rowData = $('#codeSetsKeysTable').jqGrid('getRowData', sel);
	            $('#codeSetsKeysTable').jqGrid('delRowData', sel);
	            $('#codeSetsKeysTable').jqGrid('addRowData', sel, rowData, place, ids[rel]);
	            $("#codeSetsKeysTable").setSelection(sel);
	            ToggleKeyUpDownButtons(sel);
	            $("#codeSetsSaveBtn").removeAttr("disabled");
	        };

	        function ToggleKeyUpDownButtons(rowid) {
	            var ids = $('#codeSetsKeysTable').getDataIDs();
	            if (rowid >= 0 && ids.length > 1) {
	                switch (positionOf(ids, rowid)) {
	                    case 0:
	                        $("#codeSetsKeysUpBtn").attr('disabled', 'disabled');
	                        $("#codeSetsKeysDownBtn").removeAttr('disabled');
	                        break;
	                    case ids.length - 1:
	                        $("#codeSetsKeysUpBtn").removeAttr('disabled');
	                        $("#codeSetsKeysDownBtn").attr('disabled', 'disabled');
	                        break;
	                    default:
	                        $("#codeSetsKeysUpBtn").removeAttr('disabled');
	                        $("#codeSetsKeysDownBtn").removeAttr('disabled');
	                        break;
	                }
	            } else {
	                $("#codeSetsKeysUpBtn").attr('disabled', 'disabled');
	                $("#codeSetsKeysDownBtn").attr('disabled', 'disabled');
	            }
	        }

	        $("#codeSetsResultsUpBtn").click(function () {
	            MoveResult('up');
	        });

	        $("#codeSetsResultsDownBtn").click(function () {
	            MoveResult('down');
	        });

	        function MoveResult(dir) {
	            var sel = $("#codeSetsResultsTable").getGridParam('selrow');
	            var ids = $("#codeSetsResultsTable").getDataIDs();
	            var pos = positionOf(ids, sel);
	            var rel = dir == 'up' ? pos - 1 : pos + 1;
	            var place = dir == 'up' ? 'before' : 'after';
	            var rowData = $('#codeSetsResultsTable').jqGrid('getRowData', sel);
	            $('#codeSetsResultsTable').jqGrid('delRowData', sel);
	            $('#codeSetsResultsTable').jqGrid('addRowData', sel, rowData, place, ids[rel]);
	            $("#codeSetsResultsTable").setSelection(sel);
	            ToggleResultUpDownButtons(sel);
	            $("#codeSetsSaveBtn").removeAttr("disabled");
	        };

	        function ToggleResultUpDownButtons(rowid) {
	            var ids = $('#codeSetsResultsTable').getDataIDs();
	            if (rowid >= 0 && ids.length > 1) {
	                switch (positionOf(ids, rowid)) {
	                    case 0:
	                        $("#codeSetsResultsUpBtn").attr('disabled', 'disabled');
	                        $("#codeSetsResultsDownBtn").removeAttr('disabled');
	                        break;
	                    case ids.length - 1:
	                        $("#codeSetsResultsUpBtn").removeAttr('disabled');
	                        $("#codeSetsResultsDownBtn").attr('disabled', 'disabled');
	                        break;
	                    default:
	                        $("#codeSetsResultsUpBtn").removeAttr('disabled');
	                        $("#codeSetsResultsDownBtn").removeAttr('disabled');
	                        break;
	                }
	            } else {
	                $("#codeSetsResultsUpBtn").attr('disabled', 'disabled');
	                $("#codeSetsResultsDownBtn").attr('disabled', 'disabled');
	            }
	        }

            $('#codeSetsCancelBtn').click(function () {
                isDirty = false;
                LoadCodeMapsTable();
            });

	        $('#codeSetsSaveBtn').click(function () {
	            if ($('#codeSetsResultsTable').getGridParam('records') == 0) {
	                DisplayError('At least one result field is required.');
	                return;
	            };
	            var checkName = $('#codeSetName').val().toUpperCase();
	            var checkValues = $('#codeSetsTable tbody tr').not('.jqgfirstrow');
	            if (codesetAddEditInd == 'edit') {
	                checkValues = $(checkValues).not('#' + codesetPK);
	            }
	            if ($(checkValues).find('td:visible[innerText=' + $('#codeSetName').val() + ']').filter(function () {
	                return this.innerText.toUpperCase() == checkName;
	            }).length > 0) {
	                DisplayError('Duplicate Code Set Name.');
	                return;
	            }
	            $.ajax({
	                type: "POST",
	                async: false,
	                url: '<%: Url.Action("SaveCodeSet") %>',
	                data: {
	                    action: codesetAddEditInd,
	                    sender: senderPK,
	                    recipient: recipientPK,
	                    transformation: transformationPK,
	                    codeset: codesetPK,
	                    codesetName: $('#codeSetName').val(),
	                    codesetKeys: GetCodeSetsKeysData(),
	                    codesetResults: GetCodeSetsResultsData()
	                },
	                success: function (data) {
                        isDirty = false;
	                    if (codesetAddEditInd == 'add') {
	                        LoadCodeSetsTable(data.codeset);
	                    } else if (data.exception) {
	                        DisplayLargeError(data.exception);
	                    } else {
	                        $("#codeSetsTable").jqGrid('setCell', codesetPK, '0', $('#codeSetName').val());
	                        LoadCodeMapsTable();
	                    }
	                },
	                error: function (data) {
	                    DisplayError(data.responseText);
	                }
	            });
	        });

	        function GetCodeSetsKeysData() {
	            var tableData = [];
	            $("#codeSetsKeysTable tbody tr").not('.jqgfirstrow').each(function (index, e) {
	                tableData.push({
	                    pos: $(this).find('td')[0].innerText,
	                    keyName: $(this).find('td')[1].innerText
	                });
	            });
	            return JSON.stringify({
	                codesetKeys: tableData
	            });
	        }

	        function GetCodeSetsResultsData() {
	            var tableData = [];
	            $("#codeSetsResultsTable tbody tr").not('.jqgfirstrow').each(function (index, e) {
	                tableData.push({
	                    key: $(this).find('td')[0].innerText,
	                    resultName: $(this).find('td')[1].innerText
	                });
	            });
	            return JSON.stringify({
	                codesetResults: tableData
	            });
	        }


	        /**************************************
	        *  Code Maps Table
	        **************************************/

	        var defaultId = -1;

            function LoadCodeMapsTable() {
                defaultId = -1;
              
                $("#codeMapsSaveBtn, #codeMapsCancelBtn, #codeSetsSaveBtn").attr("disabled", "disabled");
	            $("#exportCsv").removeAttr('disabled');
	            $('#codeSetsEditFields').hide();
	            $('#codeMapsFields').show();
	            $('#codeMapsLoader').show();
                $('#codeMapsTable').GridUnload('codeMapsTable');
                $.getJSON('<%: Url.Action("CodeMaps") %>' + '?codeset=' + codesetPK, LoadCodeMapsData);
            }

	        function DisplayDuplicatesError(duplicates) {
	            if (duplicates.length > 0) {
	                var message = "There are currently duplicates rows present in this mapping:\n";
	                $(duplicates).each(function () { message += this.value.join() + " x " + this.count + "\n"; });
	                message += "Please remove the above repetitions before making further changes.";
	                DisplayLargeError(message);
	            }
	        }

            function LoadCodeMapsData(data) {
                if (data.hasDefaultRow) {
                    defaultId = 0;
                }
				var colModel = new Array();
				var totalGridWidth = 0;

				function getTextWidth(text) {
					var span = $('<span>').text(text).css({});
					$('body').append(span);
					var width = span.width();
					span.remove();
					return width;
				}

				$(data.keys).each(function (i, key) {
					var maxContentWidth = Math.max(getTextWidth(key), 75);
					$(data.rows).each(function (j, row) {
						maxContentWidth = Math.max(maxContentWidth, getTextWidth(row[key]));
					});
					totalGridWidth += maxContentWidth;
                    colModel.push({
                        name: key,
                        sortable: false,
                        editable: true,
						edittype: 'text',
						width: maxContentWidth,
                        editoptions: {
                            maxlength: '250'
                        },
                        classes: 'codeMapsKey'
                    });
                });

				$(data.results).each(function (i, result) {
					var maxContentWidth = Math.max(getTextWidth(result), 75);
					$(data.rows).each(function (j, row) {
						maxContentWidth = Math.max(maxContentWidth, getTextWidth(row[result]));
					});
					totalGridWidth += maxContentWidth;
                    colModel.push({
                        name: result,
                        sortable: false,
                        editable: true,
						edittype: 'text',
						width: maxContentWidth,
                        editoptions: {
                            maxlength: '250'
                        },
                        classes: 'codeMapsResult'
                    });
                });

                $('#codeMapsTable').jqGrid({
                    datatype: 'local',
                    gridview: true,
                    rowNum: data.rows.length,
                    colNames: data.keys.concat(data.results),
                    colModel: colModel,
                    jsonReader: {
                        repeatitems: false
					},
					autowidth: true,
					shrinkToFit: true,
                    height: '300',
                    scroll: true,
                    sortable: false,
                    pager: '#codeMapsTablePager',
                    editurl: '<%: Url.Content("~/.") %>',
                    beforeSelectRow: function(rowid, e) {
                        if (rowid == $(this).getGridParam('selrow')) {
                            return false;
                        }
                        ToggleUpDownButtons(rowid);
                        return true;
                    }
                });

                $('#codeMapsTable').jqGrid('navGrid',
                    '#codeMapsTablePager',
                    {
                        refresh: false,
                        search: false,
                        add: (data.keys.length > 0),
                        del: (data.keys.length > 0)
                    },
                    {
                        width: 320,
                        resize: false,
                        closeOnEscape: true,
                        closeAfterEdit: true,
                        reloadAfterSubmit: false,
                        recreateForm: true,
                        viewPagerButtons: false,
                        beforeShowForm: PrepareMapEditForm,
                        beforeCheckValues: ConvertCodeMapDataInput,
                        beforeSubmit: ValidateCodeMapData,
                        afterComplete: CompletedCodeMapChanges
                    },
                    {
                        width: 320,
                        resize: false,
                        closeOnEscape: true,
                        reloadAfterSubmit: false,
                        recreateForm: true,
                        beforeCheckValues: ConvertCodeMapDataInput,
                        beforeSubmit: ValidateCodeMapData,
                        afterComplete: CompletedCodeMapChanges
                    },
                    {
                        resize: false,
                        reloadAfterSubmit: false,
                        beforeShowForm: function(formid) {
                            var ids = $('#codeMapsTable').getDataIDs();
                            var pos = positionOf(ids, $("#codeMapsTable").getGridParam('selrow'));
                        },
                        afterComplete: CompletedCodeMapChanges
                    }
                );

                $(data.keys).each(function(i, key) {
                    $('[id="codeMapsTable_' + key + '"]').addClass('codeMapsKeyHead');
                });

                $(data.results).each(function(i, res) {
                    $('[id="codeMapsTable_' + res + '"]').addClass('codeMapsResultHead');
                });

				$('#codeMapsTable').jqGrid('setGridWidth', totalGridWidth > 1050 ? 1050 : totalGridWidth + 10);
                $('#codeMapsTable')[0].addJSONData(eval('data'));

                if (data.hasDefaultRow) {
                    defaultId = $('#codeMapsTable').jqGrid('getDataIDs')[data.rows.length - 1];
                }
                $("#codeMapsTable").setSelection('0');
                ToggleUpDownButtons('0');

	            $('#codeMapsLoader').hide();
	        };

	        function PrepareMapEditForm(id) {
	            if (id.find('[name="codeMapsTable_id"]').val() == defaultId) {
	                $.each($('th.codeMapsKeyHead div'), function () {
	                    id.find('[id="' + this.innerText + '"]').attr("disabled", "disabled");
	                });
	            }
	        };

	        function ConvertCodeMapDataInput(postdata, formid, mode) {
	            $.each(postdata, function (i, e) {
	                if (this == null
						|| this == ''
						|| this.toUpperCase() == codeBlank) {
	                    postdata[i] = codeBlank;
	                }
	            });
	            return postdata;
	        };

	        function ValidateCodeMapData(postdata, formid) {
	            var rowsToCheck;
	            if (postdata.codeMapsTable_id == '_empty') {
	                // Insert
	                rowsToCheck = $('#codeMapsTable tbody tr').not('.jqgfirstrow');
	            } else {
	                // Update
	                rowsToCheck = $('#codeMapsTable tbody tr').not('.jqgfirstrow').not('#' + postdata.codeMapsTable_id);
	            }

	            // Check for duplicates.
	            var keys = $('th.codeMapsKeyHead div').add($('th.codeMapsResultHead div'));
	            if (rowsToCheck.filter(function () {
	                var vals = $(this).children('.codeMapsKey').add($(this).children('.codeMapsResult'));
	                for (var i = 0; i < vals.length; i++) {
	                    if (vals[i].innerText.toUpperCase() != postdata[keys[i].innerText].toUpperCase()) {
	                        return false;
	                    }
	                }
	                return true;
	            }).length > 0) {
	                return [false, 'Duplicate Row'];
				}

                // Validation for ITCustoms Response Message Job Status
				if (IsITCustomsJobStatus()) {
                    if (!ValidateITCustomsJobStatusCodeMapData(postdata)) {
                        return [false, ITCustomsJobStatusValidateErrorStr];
                    }
                }

	            // Valid
	            return [true, ''];
	        };

            function CompletedCodeMapChanges(response, postdata, formid) {
                isDirty = true;
	            if (postdata.oper == 'add') {
	                var rowData = $('#codeMapsTable').jqGrid('getRowData', postdata.id);
	                $('#codeMapsTable').jqGrid('delRowData', postdata.id);
	                $('#codeMapsTable').jqGrid('addRowData', postdata.id, rowData, 'before', $("#codeMapsTable").getGridParam('selrow'));
	            }
	            $("#codeMapsSaveBtn, #codeMapsCancelBtn").removeAttr("disabled");
	            $("#exportCsv").attr('disabled', 'disabled');
	        }

	        function ToggleUpDownButtons(rowid) {
	            var ids = $('#codeMapsTable').getDataIDs();
	            switch (positionOf(ids, rowid)) {
                    case ids.length - 1:
                        if (defaultId != -1) {
                            $("#codeMapsUpBtn").attr('disabled', 'disabled');
                            $("#codeMapsDownBtn").attr('disabled', 'disabled');
                        } else {
                            $("#codeMapsUpBtn").removeAttr('disabled');
                            $("#codeMapsDownBtn").attr('disabled', 'disabled');
                        }
	                    break;
	                case ids.length - 2:
	                    if (ids.length == 2) {
	                        $("#codeMapsUpBtn").attr('disabled', 'disabled');
	                    } else {
	                        $("#codeMapsUpBtn").removeAttr('disabled');
                        }
                        if (defaultId != -1) {
                            $("#codeMapsDownBtn").attr('disabled', 'disabled');
                        } else{
                            $("#codeMapsDownBtn").removeAttr('disabled');
                        }
                        break;
	                case 0:
	                    $("#codeMapsUpBtn").attr('disabled', 'disabled');
	                    $("#codeMapsDownBtn").removeAttr('disabled');
	                    break;
	                default:
	                    $("#codeMapsUpBtn").removeAttr('disabled');
	                    $("#codeMapsDownBtn").removeAttr('disabled');
	                    break;
	            }
			}

            // Validation for ITCustoms Response Message Job Status
            var ITCustomsJobStatusDataPrompt =
                `The "Status" should be one character letter except the last status "MIA".<br/>
                The "Poll Interval" is numeric.<br/>
                The "Repeat Times" is numeric and should between 1 to 100.<br/><br/>`;
            var ITCustomsJobStatusDataExample =
                `A example data for polling the Customs response updates till the final status (Clearance) with these frequencies:<br/>
                1 minute for the first hour<br/>
                10 minutes for the next 6 hours<br/>
                1 hour for the next 4 days<br/>
				as the following:
				<br/><br/>
                <table style="width:100%;">
					<tr>
						<th>Status</th>
						<th>Poll Interval</th>
						<th>Repeat Times</th>
					</tr>
					<tr>
						<td>A</td>
						<td>1</td>
						<td>60</td>
					</tr>
					<tr>
						<td>B</td>
						<td>10</td>
						<td>36</td>
					</tr>
					<tr>
						<td>C</td>
						<td>60</td>
						<td>96</td>
					</tr>
					<tr>
						<td>MIA</td>
						<td>2592000</td>
						<td>1</td>
					</tr>
				</table>`;
            var ITCustomsJobStatusValidateErrorStr = 'The data is invalid.<br/><br/>'
                + ITCustomsJobStatusDataPrompt
                + ITCustomsJobStatusDataExample;

            function IsITCustomsJobStatus() {
                return $('#recipientsList option:selected').text() == "ITCustoms - IT Customs Production"
                    && $('#transformationSetsList option:selected').text() == "ITCustoms Response Message Job Status"
                    && $('#codeSetsTable td:contains("JobStatus")').length > 0
                    && $('#codeSetsTable td:contains("JobStatus")').parent().attr('aria-selected') == 'true';
			}

            function isInteger(a) {
                return !isNaN(a - parseInt(a));
            }

			function ValidateITCustomsJobStatusCodeMapData(postdata) {
				var letterRegex = /^[A-Za-z]$/;

				return ((postdata['Status'].length == 1 && letterRegex.test(postdata['Status'])) || postdata['Status'] == 'MIA')
					&& isInteger(postdata["Poll Interval"]) && isInteger(postdata["Repeat Times"])
					&& postdata["Repeat Times"] > 0 && postdata["Repeat Times"] <= 100;
             }

			function ValidateImportCsvITCustomsJobStatusCodeMapData(data) {
				for (const item of data['rows']) {
					if (!ValidateITCustomsJobStatusCodeMapData(item)) {
						return false;
					}
				}

                return true;
            }

	        /**************************************
	        *  Code Maps Buttons
	        **************************************/

	        $("#codeMapsUpBtn").click(function () {
	            MoveRow('up');
	        });

	        $("#codeMapsDownBtn").click(function () {
	            MoveRow('down');
	        });

	        function MoveRow(dir) {
	            var sel = $("#codeMapsTable").getGridParam('selrow');
	            var ids = $("#codeMapsTable").getDataIDs();
	            var pos = positionOf(ids, sel);
	            var rel = dir == 'up' ? pos - 1 : pos + 1;
	            var place = dir == 'up' ? 'before' : 'after';
	            var rowData = $('#codeMapsTable').jqGrid('getRowData', sel);
	            $('#codeMapsTable').jqGrid('delRowData', sel);
	            $('#codeMapsTable').jqGrid('addRowData', sel, rowData, place, ids[rel]);
	            $("#codeMapsTable").setSelection(sel);
	            ToggleUpDownButtons(sel);
	            $("#codeMapsSaveBtn, #codeMapsCancelBtn").removeAttr("disabled");
	        };

            $("#importCsv").click(function () {
                $('#load_codeMapsTable').show();
	            $("#codeset").val(codesetPK);
	            $("#codeMapsData").val(GetCodeMapsData());
                $("#exportCsv").attr('disabled', 'disabled');
	        });

	        $('#upload_target').load(function () {
	            var responseText = $('#upload_target')[0].contentDocument.body.innerText;
	            if (responseText != "") {
	                try {
						var data = JSON.parse(responseText);

						// Validation for ITCustoms Response Message Job Status
                        if (IsITCustomsJobStatus()) {
                            if (!ValidateImportCsvITCustomsJobStatusCodeMapData(data)) {
                                DisplayError(ITCustomsJobStatusValidateErrorStr + '<br/>The responseText is:<br/>' + responseText);
                                $('#load_codeMapsTable').hide();
								return;
                            }
                        }

						$('#codeMapsTable').GridUnload('codeMapsTable');
						if (data["warning"] && !confirm(data["warning"])) {
							LoadCodeMapsTable();
						} else {
							LoadCodeMapsData(data);
							$("#importCsv").attr("disabled", "disabled");
							$("#codeMapsSaveBtn, #codeMapsCancelBtn").removeAttr("disabled");
							$("#uploadFile").replaceWith('<input type="file" name="uploadFile" id="uploadFile" />');
							$("#uploadFile").change(UploadChangeHandler);
						}
	                } catch (e) {
	                    DisplayLargeError(responseText);
	                    $('#load_codeMapsTable').hide();
	                }
	            }
	        });

	        function UploadChangeHandler() {
                if ($(this).val() == "") {
                    $("#importCsv").attr("disabled", "disabled");
                }
                else {
                    isDirty = true;
                    $("#importCsv").removeAttr("disabled");
                }
	        }
	        $("#uploadFile").change(UploadChangeHandler);

	        $("#exportCsv").click(function (e) {
	            e.preventDefault();
	            window.location.href = '<%: Url.Action("DownloadCsv") %>' + "?codeset=" + codesetPK;
	        });

	        $("#codeMapsSaveBtn").click(function () {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: '<%: Url.Action("SaveCodeMaps") %>',
                    data: { codeset: codesetPK, codeMapsData: GetCodeMapsData() },
                    success: function(data) {
                        isDirty = false;
                        if (data.exception) {
                            DisplayError(data.exception);
                        } else if (data.duplicates) {
                            DisplayDuplicatesError(data.duplicates);
                        }
                        LoadCodeMapsTable();
                    }
                });
	            $("#codeMapsSaveBtn, #codeMapsCancelBtn").attr("disabled", "disabled");
	            $("#exportCsv").removeAttr('disabled');
	        });

            $("#codeMapsCancelBtn").click(function () {
                isDirty = false;
	            $("#codeMapsSaveBtn, #codeMapsCancelBtn").attr("disabled", "disabled");
                $("#exportCsv").removeAttr('disabled');
	            LoadCodeMapsTable();
	        });

	        function GetCodeMapsData    () {
	            var tableData = [];
	            $("#codeMapsTable tbody tr").not('.jqgfirstrow').each(function (index, e) {
	                var rowData = [];
	                $(this).find("td").each(function () {
	                    rowData.push($(this).html());
	                });
	                tableData.push(rowData);
	            });
	            return JSON.stringify(tableData);
	        }

	        function DisplayLargeError(errorMsg) {
	            $('<div><pre class="ui-state-error">' + errorMsg + '<\/pre><\/div>').dialog({
	                title: 'Error',
	                modal: true,
	                resizable: true,
	                width: 1000,
	                height: 1000,
	                buttons: {
	                    OK: function () {
	                        $(this).dialog("close");
	                    }
	                }
	            });
	        }

	        function DisplayError(errorMsg) {
	            $('<div><div class="ui-state-error">' + errorMsg + '<\/div><\/div>').dialog({
	                title: 'Error',
	                modal: true,
	                resizable: false,
	                buttons: {
	                    OK: function () {
	                        $(this).dialog("close");
	                    }
	                }
	            });
	        }
	    });

	</script>

</asp:Content>
