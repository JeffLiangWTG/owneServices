<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%
    using (Html.BeginForm("Index", "TransformationSet", FormMethod.Get))
    { %>
        <div id="selector">
           <select name="searchType" id="searchType">
            <option value="sender">Sender</option>
            <option value="recipient">Recipient</option>
            <option value="messageType">MessageType</option>
           </select>
           <%=Html.TextBox("client")%> 
       </div>
       <div id="listbox">
            <div id="opositLabel">Recipient</div>   
            <select name="opositClient" id="opositClient" size="5"></select>
       </div>
<%} %>

    <!-- client type changed block -->
    <script type="text/javascript">
        $('#searchType').change(function () {
            if ($("#searchType").val() == "messageType") {
                $("#listbox").hide();
            }
            else {
                $("#opositLabel").text($("#searchType").val() == "sender" ? "Recipients" : "Senders");
                $("#listbox").show();

            }
            $("#client").attr("value", "");
            $("#opositClient").children().remove();
            PopulateGrid("");
        });
    </script> 

    <!-- autocomplete block -->
    <script type="text/javascript">
        $("#client").autocomplete
         ({
             source: function (request, response) {
                 $.ajax({ url: '<%:Url.Action("Clients", "TransformationFilter")%>/' + $("#searchType").val() + "/" + $("#client").val(),
                     type: "POST", dataType: "json",
                     success: function (data) {
                         if ($("#searchType").val() == "messageType") {
                             response($.map(data, function (item) { return { label: item.Name, value: item.Id }; }))
                         }
                         else {
                             response($.map(data, function(item) { return { label: item.Name + " (" + item.Code + ")", value: item.Id }; }))
                         }
                     }
                 })
             },
             minLength: 1,
             select: function (event, ui) {
                 $("#client").val(ui.item.label);

                 if ($("#searchType").val() == "messageType") {
                     PopulateGrid(ui.item.value + "/00000000-0000-0000-0000-000000000000");
                 }
                 else {
                     $.ajax({ url: '<%:Url.Action("OpositClients", "TransformationFilter")%>/' + $("#searchType").val() + "/" + ui.item.value, type: "POST", dataType: "json",
                         success: function (data) {
                             var dropDown = $('#opositClient');
                             dropDown.children().remove();
                             $.each(data, function (i, item) {
                                 dropDown.append("<option value='" + item.SenderId + "/" + item.RecipientId + "'>" + item.Name + " (" + item.Code + ")" + "</option>");
                             });
                         }
                     })
                 }

                 return false;
             }
         });

    </script> 

    <!-- Populate WebGrid -->
    <script type="text/javascript">
        $("#opositClient").change(function () {
            var str = ""; $("#opositClient option:selected").each(function () { str = $(this).val(); });
            PopulateGrid(str);
        });
       
    </script>

    <!-- Watermark -->
   <script type="text/javascript">
       $(function () {
           $("#client").watermark("Type a letter");
           $("#clientFocus").click(
			function () {
			    $("#client")[0].focus();
			}
		);
       });
   </script>

