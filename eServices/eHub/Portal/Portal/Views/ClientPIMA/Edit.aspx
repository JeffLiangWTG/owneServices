<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.View.ClientPIMAView>" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>
<%@ Import Namespace="CargoWise.eHub.Portal.Helpers" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>           
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>    
        <li><%: Html.ActionLink("Airline Messaging", "Index", "Service")%></li>
        <li><%: Html.ActionLink(Model.Client.CC_FriendlyName, "Details", "Service", new { id = Model.Client.CC_PK }, null)%></li>
        <li><%:Html.ActionLink("Manage PIMA", "Index", new { id = Model.Client.CC_PK })%></li>
        <li class="active">Edit</li>



    </ol>
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<div id="tabs">
    <ul>
        <li><a href="#prod">Production</a></li>
        <li><a href="#test">Test</a></li>
    </ul>
    <div id="prod">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "editForm" })) {%>
            <%:Html.ValidationSummary(false, "Please correct the errors and try again.")%>    

            <%=Html.Grid(Model.ServiceProviderPIMAView)
                       	        .Columns(column =>
                       	         	        {
                       	         		        column.For(a => a.ServiceProviderName).InsertAt(0).Named("Provider").Encode(true);
                                                column.For(a => a.IATA).EditAction(
                                                    item => Html.TextBox("IATA", item.IATA, item.IATAHtmlAttributes).ToString()).
                                                    InsertAt(1).Named("IATA").Encode(true);
                       	         		        column.For(a => a.PIMA).EditAction(
                       	         			        item => Html.TextBox("PIMA", item.PIMA).ToString()).
                       	         			        InsertAt(2).Named("PIMA").Encode(true);
                       	         		        column.For(a => a.Password).EditAction(
                       	         			        item => Html.TextBox("password", item.Password).ToString()).
                                                    InsertAt(3).Named("Password").Encode(true);
                                                column.For(a => a.Button).
                                                    InsertAt(4).Named("").Encode(false).Attributes(@class => "last");
                                                column.For(a => a.ServiceProviderId).InsertAt(5).Named("").Encode(true).Attributes(@class => "invisible");
                       	         	        })
                       	        .Attributes(@class => "pima-table", @id => "pimatable")
	        %>

            <div id="actionBlock">
                <div class="leftActionButton"><input type="submit" value="Save" class="actionButton" id="saveButton"/></div>
                <div class="rightActionButton">
                        <%:Html.ActionLink("Cancel", "Index", new {id = Model.Client.CC_PK, env = "prod"})%>
                </div>
                <div class="clear"></div>
            </div>
        <% } %>
    </div>
    <div id="test">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "editTestForm" })) {%>
            <%:Html.ValidationSummary(false, "Please correct the errors and try again.")%>    

            <%=Html.Grid(Model.ServiceProviderPIMAViewTest)
                       	        .Columns(column =>
                       	         	        {
                       	         		        column.For(a => a.ServiceProviderName).InsertAt(0).Named("Provider").Encode(true);
                                                column.For(a => a.IATA).EditAction(
                                                    item => Html.TextBox("IATA", item.IATA, item.IATAHtmlAttributes).ToString()).
                                                    InsertAt(1).Named("IATA").Encode(true);
                       	         		        column.For(a => a.PIMA).EditAction(
                       	         			        item => Html.TextBox("PIMA", item.PIMA).ToString()).
                       	         			        InsertAt(2).Named("PIMA").Encode(true);
                       	         		        column.For(a => a.Password).EditAction(
                       	         			        item => Html.TextBox("password", item.Password).ToString()).
                                                    InsertAt(3).Named("Password").Encode(true);
                                                column.For(a => a.Button).
                                                    InsertAt(4).Named("").Encode(false).Attributes(@class => "last");
                                                column.For(a => a.ServiceProviderId).InsertAt(5).Named("").Encode(true).Attributes(@class => "invisible");
                       	         	        })
                       	        .Attributes(@class => "pima-table", @id => "pimatest")
	        %>

            <div id="Div1">
                <div class="leftActionButton"><input type="submit" value="Save" class="actionButton" id="Submit1"/></div>
                <div class="rightActionButton">
                        <%:Html.ActionLink("Cancel", "Index", new {id = Model.Client.CC_PK, env = "test"})%>
                </div>
                <div class="clear"></div>
            </div>
        <% } %>
    </div>
</div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%: Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <!--[if lt IE 8]>
        <script src="http://www.json.org/json2.js"></script>
    <![endif]-->

    <script type="text/javascript">
    <!--
        var env = '<%: Model.Env %>';
        var defaultRows = new Object();


        function createTextBox(fieldName) {
            var textBox = document.createElement('input');
            textBox.type = 'text';
            textBox.name = fieldName;
            return textBox;
        }

        function removeRow() {
            $(this).closest("tr").remove();
        }

        function createServiceObject(cells, i, id) {
            return {
                ServiceProviderId: cells[5].innerHTML,
                ServiceProviderName: cells[0].innerHTML,
                IATA: $(cells[1]).children('input').val(),
                PIMA: $(cells[2]).children('input').val(),
                Password: $(cells[3]).children('input').val(),
                rowNumber: i, tableId: id,
                IsDefault: $(cells[1]).children('input').val().toUpperCase() == 'DEFAULT'
            };
        }

        function markRowForErrors(rowNum, message, tableId) {
            var table = $("#" + tableId)[0];
            var tab = $(table).closest('div.ui-tabs-panel')[0].id;

            $('#tabs a[href=#' + tab + ']').addClass('tabError');
            $('#tabs a[href=#' + tab + ']').attr("title", "Review tab for error/s.");

            $(table.rows[rowNum]).children('td').not('td:nth-child(5)').css('background-color', 'red');

            if (message != null && message != undefined && message != "") {
                $(table.rows[rowNum]).attr("title", message);
            }
        }

        function markRowsForErrors(rowArray, message, tableId) {
            var table = $("#" + tableId)[0];
            for (var i = 0; i < rowArray.length; i++) {
                markRowForErrors(rowArray[i], message, tableId);
            }
        }

        function clearMarkedErrors() {
            $('#tabs ul li a').removeClass('tabError');
            $('.pima-table tr').removeAttr('title');
            $('.pima-table td').css('background-color', '');
        }

        function validateForDefaultValue(providerArray, errors) {
            var defaultCounter = 0;
            var defaultRowNumbers = new Array();

            for (var i = 0; i < providerArray.length; i++) {
                if (providerArray[i].IsDefault) {
                    defaultCounter++;
                    defaultRowNumbers.push(providerArray[i].rowNumber);
                }
            }

            if (defaultCounter < 1) {
                errors.hasError = true;
                var errorMessage = "Should have at least one default."
                errors.messages.push(errorMessage);
                markRowForErrors(defaultRows[providerArray[0].ServiceProviderId], errorMessage, providerArray[0].tableId);
            } else if (defaultCounter > 1) {
                errors.hasError = true;
                var errorMessage = "Should have only one default."
                errors.messages.push(errorMessage);
                markRowsForErrors(defaultRowNumbers, errorMessage, providerArray[0].tableId);
            }
        }

        function validateUniqueIATAPerClient(providerArray, errors) {
            var services = new Object();
            for (var i = 0; i < providerArray.length; i++) {
                var record = providerArray[i];
                if (!(record.IATA in services)) {
                    services[record.IATA] = record;
                } else {
                    var errorMessage = "IATA should be unique per Provider.";
                    errors.hasError = true;
                    errors.messages.push(errorMessage);
                    markRowForErrors(record.rowNumber, errorMessage, record.tableId);
                    markRowForErrors(services[record.IATA].rowNumber, errorMessage, services[record.IATA].tableId);
                }
            }
        }

        function validatePIMA(clientPIMA) {
            clearMarkedErrors();

            var errors = {
                hasError: false,
                messages: new Array()
            };

            for (var provider in clientPIMA) {
                var providerArray = clientPIMA[provider];

                validateForDefaultValue(providerArray, errors);
                validateUniqueIATAPerClient(providerArray, errors);

                for (var i = 0; i < providerArray.length; i++) {
                    var service = providerArray[i];

                    if (service.IATA == "" || service.IATA == null) {
                        errors.hasError = true;
                        var errorMessage = "IATA should not be blank";
                        errors.messages.push(errorMessage);
                        markRowForErrors(service.rowNumber, errorMessage, service.tableId);
                    } else if ((service.IATA.toUpperCase() != "DEFAULT") && (service.PIMA == "" || service.PIMA == null)) {
                        var table = $('#pimatable')[0];
                        errors.hasError = true;
                        var errorMessage = "PIMA should not be blank"
                        errors.messages.push(errorMessage);
                        markRowForErrors(service.rowNumber, errorMessage, service.tableId);
                    }

                    if ((service.IATA.toUpperCase() != "DEFAULT") && (service.Password != "" && service.Password != null) && (service.PIMA == "" || service.PIMA == null)) {
                        errors.hasError = true;
                        var errorMessage = "PIMA should not be blank";
                        errors.messages.push(errorMessage);
                        markRowForErrors(service.rowNumber, errorMessage, service.tableId);
                    }
                }
            }

            return errors;
        }


        function convertToDictionary(clientPIMA) {
            var dictionary = new Array();
            for (var id in clientPIMA) {
                var providerArray = clientPIMA[id];
                dictionary.push({ 'key': id, 'value': providerArray });
            }
            return dictionary;
        }

        function getActiveTable() {
            var active = $("#tabs").tabs("option", "selected");
            return (active == 0) ? $('#pimatable')[0] : $('#pimatest')[0];
        }

        function update(pimaDictionary) {
            $.ajax({
                type: 'POST',
                url: '<%: Url.Action("Update", new {id = Model.Client.CC_PK})  %>',
                data: JSON.stringify(pimaDictionary),
                success: function (response) {
                    if (response.success) {
                        window.location = '<%: Url.Action("Index", new {id = Model.Client.CC_PK}) %>/' + ($("#tabs").tabs("option", "selected") == 0 ? 'prod' : 'test');
                    } else {
                        alert("An error ocurred while processing your request: \n\t - " + response.message);
                    }
                },
                error: function (xhr) {
                    alert("An error ocurred while processing your request: \n\t - " + xhr.status + ": " + xhr.statusText);
                },
                contentType: 'application/json; charset=UTF-8',
                dataType: 'json'
            });
        }

        function createClientPimaData(tables) {
            var clientPIMA = {};

            for (var j = 0; j < tables.length; j++) {
                var table = tables[j];
                var rows = table.rows;
                // 1st row is the header so start from the second row.
                for (var i = 1; i < rows.length; i++) {
                    var cells = rows[i].cells;

                    var service = createServiceObject(cells, i, table.id);

                    if (service.IATA.toUpperCase() == "DEFAULT") {
                        if (defaultRows[service.ServiceProviderId] == null && defaultRows[service.ServiceProviderId] == undefined) {
                            defaultRows[service.ServiceProviderId] = i;
                        }

                        if ((service.PIMA != "" && service.PIMA != null) || (service.Password != "" && service.Password != null)) {
                            if (clientPIMA[cells[5].innerHTML] == undefined) {
                                clientPIMA[cells[5].innerHTML] = new Array();
                            }

                            if (service.PIMA == "" || service.PIMA == null) {
                                $(cells[2]).children('input').val("Default")
                                service.PIMA = "Default"
                            }

                            service.IsDefault = true;

                            clientPIMA[cells[5].innerHTML].push(service);
                        }
                    } else {
                        if (clientPIMA[cells[5].innerHTML] == undefined) {
                            clientPIMA[cells[5].innerHTML] = new Array();
                        }
                        clientPIMA[cells[5].innerHTML].push(service);
                    }
                }
            }

            return clientPIMA;
        }

        function ajaxSubmit() {
            try {
                var clientPIMA = createClientPimaData($('.pima-table'));

                var errors = validatePIMA(clientPIMA);
                var clientPIMADictionary;

                if (!errors.hasError) {
                    clientPIMADictionary = convertToDictionary(clientPIMA);
                    update(clientPIMADictionary);
                }
            } catch (e) {
                alert(e.message);
            }

            return false;
        }

        $("#tabs").tabs({ selected: env == 'prod' ? 0 : 1 });

        $('.addButton').click(function () {
            var table = getActiveTable();

            var row = $(this).closest("tr")[0];

            var newRow = table.insertRow(row.rowIndex + 1);
            var firstColumn = newRow.insertCell();
            firstColumn.innerHTML = row.children[0].innerHTML;

            var secondColumn = newRow.insertCell(1);
            var iataTextField = createTextBox('IATA');
            secondColumn.appendChild(iataTextField);

            var thirdColumn = newRow.insertCell(2);
            var pimaTextField = createTextBox('PIMA');
            thirdColumn.appendChild(pimaTextField);

            var fourthColumn = newRow.insertCell(3);
            var pimaTextField = createTextBox('PASSWORD');
            fourthColumn.appendChild(pimaTextField);

            var fifthColumn = newRow.insertCell(4);
            var deleteButton = document.createElement("div");
            deleteButton.className = "deleteButton";
            $(deleteButton).click(removeRow);
            fifthColumn.appendChild(deleteButton);

            var sixthColumn = newRow.insertCell(5);
            sixthColumn.className = "invisible";
            sixthColumn.innerHTML = this.id;

            // Alternate CSS Row Style
            var isGridRow = (row.className == "gridrow");
            for (var i = newRow.rowIndex - 1; i < table.rows.length; i++) {
                table.rows[i].className = isGridRow ? "gridrow" : "gridrow_alternate";
                isGridRow = !isGridRow;
            }
        });

        $('.deleteButton').click(removeRow);

        $("#editTestForm").submit(ajaxSubmit);

        $('#editForm').submit(ajaxSubmit);
    -->
    </script>

</asp:Content>