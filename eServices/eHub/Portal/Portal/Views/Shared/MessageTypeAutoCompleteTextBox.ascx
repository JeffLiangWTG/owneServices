<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.MessageTypeView>" %>

<%
    var valueName = String.Format("TId{0}", Model.Type);
    var labelName = String.Format("TName{0}", Model.Type);
    var statusName = String.Format("TStatus{0}", Model.Type);
%>

<div class="atocompleteTextBox"> 
<%=Html.Hidden(valueName, Model.Id)%> 
<%=Html.TextBox(labelName, Model.Name)%> 
<div id="<%:statusName%>" class="autocompleteHint">Please type <%:Model.MinLetters %> letter .</div>

 <!-- autocomplete block -->
    <script type="text/javascript">

        $("#<%:labelName%>").autocomplete
         ({
             source: function (request, response) {
                 $("#<%:valueName%>").val("00000000-0000-0000-0000-000000000000");
                 if (request.term.length < <%:Model.MinLetters %>) {
                     $("#<%:statusName%>").html("Please type <%:Model.MinLetters %> letter. ");
                 }
                 else {
                     $.ajax({ url: '<%:Url.Action("Find", "MessageType")%>/' + $("#<%:labelName%>").val(),
                         type: "POST", dataType: "json",
                         success: function (data) {
                             var str = "";
                             if (data.length == 0) { str = "No results was found. " }
                             else { str = data.length + " results was found. Select one. " }
                             $("#<%:statusName%>").html(str);
                             response($.map(data, function (item) { return { label: item.Name, value: item.Id }; }))
                         }
                     })
                 }

             },
             minLength: 1,
             select: function (event, ui) {
                 $("#<%:labelName%>").val(ui.item.label);
                 $("#<%:valueName%>").val(ui.item.value);
                 $("#<%:statusName%>").html("<a class='detailsMessageType' href='" + '<%:Url.Action("DetailsModal", "MessageType")%>/' + ui.item.value + "'>View Details?</a>");
                 $(".detailsMessageType").colorbox({ iframe: true, innerWidth: 600, innerHeight: 500 });
                 return false;
             }
         });

    </script>
</div>