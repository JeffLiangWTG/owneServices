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
	
	<div id="certificatesFields">
		<table id="certificatesTable"></table>
		<div id="certificatesTablePager"></div>
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
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">

	<script type="text/javascript">

	    $(document).ready(function () {

	        $('#errorMessage').ajaxError(function (event, request, settings, exception) {
	            $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
	            $(this).show();
	        }).ajaxComplete(function () {
	            $(this).hide()
	        });

	        $("#certificatesTable").jqGrid({
	            datatype: 'json',
	            url: '<%: Url.Action("Certificates") %>',
	            editurl: '<%: Url.Action("CertificateEditJQGrid") %>',
	            jsonReader: { root: 'eHubClientCertificates', id: 'CE_PK', repeatitems: false },
	            colNames: ['CE_PK', 'Category', 'ID', 'Owner', 'Owner System', 'Valid From UTC', 'Valid To UTC', 'Active From UTC', 'Added UTC', 'Container Type', 'File', 'Password', 'Thumbprint', 'Issuer', 'Serial Number', 'Subject Key Identifier'],
	            colModel: [
					{
					    name: 'CE_PK',
					    hidden: true
					},
                    {
                        name: 'CE_Category',
						index: 'CE_Category',
						width: 250,
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        editoptions: { Size: 40 },
                        sortable: false
                    },
                    {
                        name: 'CE_ID',
						index: 'CE_ID',
						width: 600,
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        editoptions: { Size: 40 },
                        sortable: false
                    },
                    {
                        name: 'CE_CC_ID',
                        index: 'CE_CC_ID',
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientIdValue').val();
                            }
                        }
                    },
                    {
                        name: 'CE_EH_ID',
                        index: 'CE_EH_ID',
                        width: 200,
                        classes: "wrapped",
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientSystemEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientSystemIdValue').val();
                            }
                        }
                    },
					{
					    name: 'CE_ValidFromUTC',
					    index: 'CE_ValidFromUTC',
					    width: 200,
					    classes: "wrapped",
					    search: true,
					    searchoptions: { sopt: ['gt', 'ge', 'lt', 'le'] },
					    editable: true,
					    editoptions: { readonly: "readonly" },
					    sortable: true,
					    formatter: "date",
					    formatoptions: { srcformat: "ISO8601Long", newformat: "d/m/Y h:i A" }
					},
					{
					    name: 'CE_ValidToUTC',
					    index: 'CE_ValidToUTC',
					    width: 200,
					    classes: "wrapped",
					    search: true,
					    searchoptions: { sopt: ['gt', 'ge', 'lt', 'le'] },
					    editable: true,
					    editoptions: { readonly: "readonly" },
					    sortable: true,
					    formatter: "date",
					    formatoptions: { srcformat: "ISO8601Long", newformat: "d/m/Y h:i A" }
					},
					{
					    name: 'CE_ActiveFromUTC',
					    index: 'CE_ActiveFromUTC',
					    width: 200,
					    classes: "wrapped",
					    search: true,
					    searchoptions: { sopt: ['gt', 'ge', 'lt', 'le'] },
					    editable: true,
					    sortable: true,
					    formatter: "date",
					    formatoptions: { srcformat: "ISO8601Long", newformat: "d/m/Y h:i A" }
					},
					{
					    name: 'CE_AddedUTC',
					    index: 'CE_AddedUTC',
					    width: 200,
					    classes: "wrapped",
					    search: true,
					    searchoptions: { sopt: ['gt', 'ge', 'lt', 'le'] },
					    editable: true,
					    editoptions: { readonly: "readonly" },
					    sortable: true,
					    formatter: "date",
					    formatoptions: { srcformat: "ISO8601Long", newformat: "d/m/Y h:i A" }
					},
                    {
                        name: 'CE_ContainerType',
                        index: 'CE_ContainerType',
                        resizable: true,
                        width: 200,
                        search: false,
                        hidden: true,
                        editable: true,
                        edittype: "select",
                        editoptions: { value: getContainerTypes(), style: "width:226px" },
                        editrules: { required: true, edithidden: true },
                        sortable: false
                    },
                    {
                        name: 'CE_File',
                        hidden: true,
                        classes: "wrapped",
                        search: false,
                        edittype: 'custom',
                        viewable: true,
                        editable: true,
                        editrules: { edithidden: true },
                        editoptions: {
                            custom_element: FormatCertificateDownloadAndUpload,
                            custom_value: function (elem) {
                                return $(elem).find('#certificate_inputFile').val();
                            }
                        }
                    },
                    {
                        name: 'CE_Password',
                        hidden: true,
                        classes: "wrapped",
                        search: false,
                        editable: true,
                        editoptions: { Size: 40 },
                        editrules: { edithidden: true },
                        sortable: false
                    },
					{
					    name: 'CE_Thumbprint',
					    index: 'CE_Thumbprint',
					    classes: "wrapped",
					    hidden: true,
					    search: true,
					    searchoptions: { sopt: ['eq', 'cn'], searchhidden: true },
					    editable: true,
					    editrules: { edithidden: true },
					    editoptions: { Size: 40, readonly: "readonly" },
					    sortable: false
					},
					{
					    name: 'CE_Issuer',
					    index: 'CE_Issuer',
					    classes: "wrapped",
					    hidden: true,
					    search: true,
					    searchoptions: { sopt: ['eq', 'cn'], searchhidden: true },
					    editable: true,
					    editrules: { edithidden: true },
					    editoptions: { Size: 40, readonly: "readonly" },
					    sortable: false
					},
					{
					    name: 'CE_SerialNumber',
					    index: 'CE_SerialNumber',
					    classes: "wrapped",
					    hidden: true,
					    search: true,
					    searchoptions: { sopt: ['eq', 'cn'], searchhidden: true },
					    editable: true,
					    editrules: { edithidden: true },
					    editoptions: { Size: 40, readonly: "readonly" },
					    sortable: false
					},
					{
					    name: 'CE_SubjectKeyIdentifier',
					    index: 'CE_SubjectKeyIdentifier',
					    classes: "wrapped",
					    hidden: true,
					    search: true,
					    searchoptions: { sopt: ['eq', 'cn'], searchhidden: true },
					    editable: true,
					    editrules: { edithidden: true },
					    editoptions: { Size: 40, readonly: "readonly" },
					    sortable: false
					}
				],
	            height: '450',
	            width: '1200',
	            rowNum: 20,
	            rowList: [20, 50, 100],
	            caption: "Certificates",
	            sortable: true,
                ondblClickRow: function(rowid) {
                    jQuery(this).jqGrid('viewGridRow', rowid, {   // view option
                        width: 400,
                        closeAfterEdit: true,
                        closeOnEscape: true,
                        recreateForm: true,
                        viewPagerButtons: false,
                        resize: true,
                        beforeShowForm: function () {
                            $('#v_CE_File span').html(GetButtonDownload());
                        }
                    });
                },
	            sortname: 'CE_CC_ID',
	            sortorder: 'asc',
	            pager: '#certificatesTablePager'
			});

	        $("#certificatesTable").jqGrid('navGrid', '#certificatesTablePager', { view: true },
				{ // edit option
				    width: 400,
				    closeAfterEdit: true,
				    closeOnEscape: true,
				    recreateForm: true,
				    viewPagerButtons: false,
				    resize: true,
				    beforeShowForm: function () {
				        $('#ce_certificate_download').show();
				        ModifyJQGridPOSTRequest("edit")
				    }
				},
				{ // add option
				    width: 400,
				    closeAfterAdd: false,
				    closeOnEscape: true,
				    recreateForm: true,
				    resize: true,
				    beforeShowForm: function () {
				        $('#ce_certificate_download').hide();
				        $('#tr_CE_AddedUTC').remove();
				        ModifyJQGridPOSTRequest("add");
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
					showQuery: true
				},
                {   // view option
                    width: 400,
                    closeAfterEdit: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    viewPagerButtons: false,
                    resize: true,
                    beforeShowForm: function () {
                        $('#v_CE_File span').html(GetButtonDownload());
                    }
                }
			);

	        function ModifyJQGridPOSTRequest(oper) {
	            $("#FrmGrid_certificatesTable").attr("id", "newFrmGrid_certificatesTable").removeAttr('onsubmit').append('<input type="hidden" name="oper" value="' + oper + '">')
                            .attr('action', '<%: Url.Action("CertificateEdit") %>').attr('method', 'post').attr('enctype', 'multipart/form-data').attr('target', 'certificate_target')
                            .append('<iframe id="certificate_target" name="certificate_target" src="" style="width:0;height:0;border:0px solid #fff;"></iframe>');
	            $("#sData").attr("id", "newsData");
	            $("#CE_CC_ID_val").attr("name", "CE_CC_ID");
	            $("#CE_EH_ID_val").attr("name", "CE_EH_ID");
	            $("#TblGrid_certificatesTable_2").insertAfter($("#TblGrid_certificatesTable"));

	            $('#newsData').append('<input type="submit" value="submit" size="200" style="position:absolute;top:0;right:0;margin:0;padding:0;cursor:pointer;opacity:0;border:0;height:22.8px;width:62.7px"/>');

	            $('#certificate_target').load(function () { // the first time, it loads, is when the form appear.
	                $('#certificate_target').load(function () {
						var bodyData = $('#certificate_target').contents().find("body").find("pre").html();
                        if (!bodyData) bodyData = $('#certificate_target').contents().find("body").html();
	                    if (!bodyData) return;
	                    var result = eval("(" + bodyData + ")");
	                    if (result.success) {
	                        $('#cData').click();
	                        $("#certificatesTable").trigger("reloadGrid");
	                    }
	                    else
	                        DisplayError(result.message);
	                });
	            });
	        }

	        function getContainerTypes() {
	            var containerTypes = jQuery.ajax({
	                url: '<%: Url.Action("ContainerTypes") %>',
	                async: false
	            }).responseText;
	            return containerTypes;
	        }

	        function FormatCertificateDownloadAndUpload(value, options) {
	            var elemStr = '<span style="overflow: hidden;white-space: nowrap;">' + GetButtonDownload() + '<a id="ce_certificate_upload" class="fileUpload fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;"><p>Upload</p><input id="certificate_inputFile" name="certificate_inputFile" type="file" style="position:absolute;top:0;right:0;margin:0;padding:0;cursor:pointer;opacity:0;height:25px;"/></a><label id="inputFile1">No file selected.</label></span>';
	            var elem = $(elemStr)[0];
	            $(elem).find('#certificate_inputFile').change(function () {
	                var o = this.value || 'No file selected.';
	                var res = o.split("\\");
	                var filename = res[res.length - 1];
	                $('#inputFile1').text(filename);
	            });
	            return elem;
	        }

	        function GetButtonDownload() {
	            var selected_row = $('#certificatesTable').jqGrid('getGridParam', 'selrow');
	            var ce_pk = $('#certificatesTable').getRowData(selected_row)["CE_PK"];
	            return '<a href="<%: Url.Action("DownloadCertificate") %>?CE_PK=' + ce_pk + '" id="ce_certificate_download" class="fm-button ui-state-default ui-corner-all" style="position:relative;overflow:hidden;" target="_blank">Download</a>';
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
	            $("#clientSystemsTable").jqGrid({
	                datatype: 'json',
					url: '<%: Url.Action("ClientSystems") %>' + '?includeNonProd=' + includeNonProd,
	                jsonReader: { root: 'eHubClientSystems', id: 'EH_ID', repeatitems: false },
	                colNames: ['Client System ID'],
	                colModel: [
						{ name: 'EH_ID', width: 100, classes: "wrapped", sortable: true, search: true }
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
	                            DisplayError("Please select a client system.");
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
		});

       
	</script>


</asp:Content>
