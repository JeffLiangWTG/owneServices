<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.SelectValueView>" %>

<%
    var valueName = String.Format("CId{0}", Model.Type);
    var labelName = String.Format("CName{0}", Model.Type);
    var statusName = String.Format("CStatus{0}", Model.Type);
%>

<div class="atocompleteTextBox"> 
<%=Html.Hidden(valueName, Model.Id)%> 
<%=Html.TextBox(labelName, Model.DisplayName)%> 
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
                 else{
                     $.ajax({ url: '<%:Url.Action("Find", "Client")%>/' + $("#<%:labelName%>").val(),
                         type: "POST", dataType: "json",
                         success: function (data) {
                             var str = "";
                             if (data.length == 0) { str = "No results was found. <a class='createClient' href='" + '<%:Url.Action("CreateModal", "Client")%>/' + request.term + "'>Create a new one?</a> " }
                             else { str = data.length + " results was found. Select one. "}
                             $("#<%:statusName%>").html(str);
                             $(".createClient").colorbox({ iframe: true, innerWidth: 600, innerHeight: 500, onClosed:function(){ if( $("#cboxClose").attr("itemId") != null && $("#cboxClose").attr("itemId") != ""){ $("#<%:valueName%>").val($("#cboxClose").attr("itemId")); $("#<%:labelName%>").val($("#cboxClose").attr("itemName")); $("#<%:statusName%>").html(""); } }
                             });
                             response($.map(data, function (item) { return { label: item.Name + " (" + item.Code + ")", value: item.Id }; }))
                         }
                     })
                 }
  
             },
             minLength: 1,
             select: function (event, ui) {
                 $("#<%:labelName%>").val(ui.item.label);
                 $("#<%:valueName%>").val(ui.item.value);
                 $("#<%:statusName%>").html("");
                 return false;
             }
         });
    </script> 
</div>