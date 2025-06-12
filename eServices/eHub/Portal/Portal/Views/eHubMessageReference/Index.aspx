<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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
    
    <asp:Content ID="SideBarContent" ContentPlaceHolderID="SideBarContent" runat="server">
        <%if (Html.MvcSiteMap().SiteMap.CurrentNode != null && Html.MvcSiteMap().SiteMap.CurrentNode.ParentNode != null)
            {%><%=Html.MvcSiteMap().Menu(Html.MvcSiteMap().SiteMap.CurrentNode.ParentNode, true, false, 2)%>
        <%}%>
    </asp:Content>
    
    <asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
        <ol class="breadcrumb"><%:Html.MvcSiteMap().SiteMapPath()%></ol>
    </asp:Content>
    
    <asp:Content ID="Content4" ContentPlaceHolderID="PageNameContent" runat="server">
    <div id="page-title-container"><h2>eHub Message Reference</h2></div>
    <div id="top-right-container">
        <h3>Application Code: </h3>
        <select class="ui-widget" name="ApplicationCode" id="appCodeSelector">
        <% if (!Model.CodeList.Contains(Model.Selected)) { %><option value="<%: Model.Selected %>"><%: Model.Selected%> [NEW]</option><% } %>
        <% foreach (var code in Model.CodeList) { %>
            <option value="<%: code %>" <% if (code == Model.Selected) { %> selected="selected" <% } %> ><%: code %></option>
        <% } %>
            <option id="_NEW" value="_NEW">New</option>
        </select>
        <span id="newCodeSection" <% if (!string.IsNullOrEmpty(Model.Selected)) { %>style="display:none"<% } %>>
            <input id="newCode" type="text" size="3" />
            <button id="goButton">Go</button>
        </span>
    </div>
    </asp:Content>
    
    <asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        
        <div id="errorMessage" class="error" style="display: block"></div>
        
        <div id="registryFields">
            <table id="registryTable"></table>
            <div id="registryTablePager"></div>
        </div>
        <div id="data-transfer-forms-container">
            <h3 class="important">Import & Export:</h3>
            <ul>
                <li>
                    Exporting outputs a CSV file with the format:
    <pre class="code">eHub Client ID, Name, Application Code, Message Reference, Password
    &lt;Client ID&gt;,&lt;Client Name&gt;,&lt;Application Code&gt;,&lt;Message Reference&gt;,[Password]</pre>
                    Take note that <span class="important uppercase">Password</span> is optional and may not exist but will still output the last coma.
                </li>
                <li>Values that have coma will be wrapped in double quotes. <pre class="code">e.g. ADSDFWDFW,"Apex Diversified Solutions, Inc.",AQW,REF235A,P@sSw0Rd</pre></li>
                <li>Exporting will <span class="important uppercase">only</span> export message references that are of the <span class="important uppercase">selected application code</span>.</li>
                <li>Importing will require the <span class="important uppercase">same format as the output CSV</span> taking note of the fomatting constraints (Required comas even if Password does not exist & Values with coma should be wrapped with double quotes).</li>
                <li>Importing will <span class="important uppercase">not</span> merge but will <span class="important uppercase">replace</span> the current data of the <span class="important uppercase">selected application code</span> with the items in the specified CSV file.</li>
                <li>Importing will import items on the <span class="important uppercase">selected application code</span> and will ignore whatever is specified in the CSV file.</li>
                <li>Importing will also ignore the <span class="important uppercase">Client Name</span> so any changes to it will not update the client's details.</li>
            </ul>
            <h3 class="important">Use Case:</h3>
            <ol>
                <li>Select Application Code.</li>
                <li>Export & open CSV file. MS Excel or any text editor can be used.</li>
                <li>Edit/Add data. Take note of the required format.</li>
                <li>Make sure the correct Application code is selected. Import modified CSV.</li>
            </ol>
            <form action="<%: Url.Action("ExportCSV") %>">
                <fieldset class="export data-transfer-container">
                    <legend>Export</legend>
                    <input type="submit" value="Export to CSV" class="ui-widget" />
                </fieldset>
            </form>
            <form action="<%: Url.Action("ImportCSV") %>" method="post" enctype="multipart/form-data" >
                <fieldset class="import data-transfer-container">
                    <legend>Import</legend>
                    <div>
                    <input type="file" name="file" class="ui-widget" />
                    <input type="submit" value="Import CSV" class="ui-widget" />
                    <div id="importError"><p class="error"><%: TempData["errorMessage"] ?? ""%></p></div>
                    </div>
                </fieldset>
            </form>
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
            function isNullOrEmpty(string) {
                return string === null || string === undefined || string === "";
            }
    
            function isNewCode(selected) {
                return selected.id === '_NEW' && $(selected).val() === '_NEW'
            }
    
            function goToCode(appCode) {
                if (appCode != null && appCode != undefined && appCode != '') {
                    var index = '<%: Url.Action("Index") %>';
                    var selected = '<%: Model.Selected %>';
                    if (!isNullOrEmpty(selected) && appCode) {
                        top.location = index.replace(new RegExp(selected + '$'), appCode);
                    }
                }
            }
    
            $('#goButton').click(function (event) {
                goToCode($('#newCode').val());
            });
    
            $('#appCodeSelector').change(function (event) {
                var selected = $('#appCodeSelector').children(':selected');
                if (selected.length === 1) {
                    if (!isNewCode(selected[0])) {
                        goToCode($('#appCodeSelector').val());
                    } else {
                        $('#newCodeSection').show();
                    }
                }
            });
    
            $(document).ready(function () {
                var inEdit = false;
                $('#errorMessage').ajaxError(function (event, request, settings, exception) {
                    $(this).html("<p>Error retrieving data from " + settings.url + "<\/p>");
                    $(this).show();
                }).ajaxComplete(function () {
                    $(this).hide()
                });
    
                $("#registryTable").jqGrid({
                    datatype: 'json',
                    url: '<%: Url.Action("List") %>',
                    editurl: '<%: Url.Action("Edit") %>',
                    jsonReader: {
                        root: 'eHubMessageReferenceRegistry',
                        id: 'CR_PK',
                        repeatitems: false
                    },
                    colNames: ['eHub Client ID', 'Name', 'Message Reference', 'Password'],
                    colModel: [{
                        name: 'CC_ID',
                        classes: "wrapped",
                        width: 250,
                        sortable: true,
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        edittype: 'custom',
                        editoptions: {
                            custom_element: FormatClientEditFields,
                            custom_value: function (elem) {
                                return $(elem).children('.clientIdValue').val();
                            }
                        },
                        searchrules: { custom: true, custom_func: validation_check }
                    }, {
                        name: 'CC_FriendlyName',
                        width: 600,
                        sortable: true,
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        searchrules: { custom: true, custom_func: validation_check }
                    }, {
                        name: 'CR_MessageReference',
                        width: 250,
                        sortable: true,
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        searchrules: { custom: true, custom_func: validation_check }
                    }, {
                        name: 'CR_Password',
                        width: 300,
                        sortable: true,
                        search: true,
                        searchoptions: { sopt: ['bw', 'ew', 'eq', 'cn'] },
                        editable: true,
                        searchrules: { custom: true, custom_func: validation_check }
                    }],
                    height: '100%',
                    width: '700',
                    rowNum: 10,
                    rowList: [10, 20, 50],
                    caption: "eHub Message Reference Registry",
                    sortable: true,
                    sortname: 'CC_ID',
                    sortorder: 'asc',
                    pager: '#registryTablePager'
                });
    
                $("#registryTable").jqGrid('navGrid', '#registryTablePager', {}, {
                    width: 400,
                    closeAfterEdit: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    viewPagerButtons: false,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    },
                    beforeInitData: function () { inEdit = true; }
                }, {
                    width: 400,
                    closeAfterAdd: true,
                    closeOnEscape: true,
                    recreateForm: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    },
                    beforeInitData: function () { inEdit = false; }
                }, {
                    closeOnEscape: true,
                    afterSubmit: function (response, postdata) {
                        var data = eval('(' + response.responseText + ')');
                        return [data.success, data.message, data.id];
                    }
                },
                    {
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
                });
    
                var clientEditState = {
                    includeNonProd: false,
                    checkbox: null,
                };
    
                function FormatClientEditFields(value, options) {
                    var checkedAttribute = ''; 
                    var elemStr = '<span><input id="'+ options.id + '_val" value="' + value +
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
    
                    $(elem).children('.clientIdSelect').click(function(){
                        DisplaySelectClientDialog($(elem).children('.clientIdValue'), clientEditState.includeNonProd);
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
                    includeNonProd = clientEditState.includeNonProd;
                    $("#clientsTable").jqGrid("setGridParam", {
                        url: '<%: Url.Action("ClientList") %>' + '?includeNonProd=' + includeNonProd,
                    }).trigger("reloadGrid");
                    $("#clientsTable").jqGrid({
                        datatype: 'json',
                        url: '<%: Url.Action("ClientList") %>' + '?includeNonProd=' + includeNonProd,
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

