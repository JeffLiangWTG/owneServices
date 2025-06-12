<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<CargoWise.eHub.Portal.Models.TransformationMappingView>>" %>

<div id="transformationList">
    <ul count="<%:Model.Count()%>">
    <% 
        foreach (var item in Model) 
        {%>
            <%Html.RenderPartial("TransformationTypeAutoCompleteTextBox", item); %>
        <%}
    %>  
    </ul>
    
    <p id="addTransformation">Add Transformation</p>
    <div class="error" id="addTransformationStatus"></div>
</div>

<script type="text/javascript">
    $("#addTransformation").click(function () {

        if ($("#TId").val() == "00000000-0000-0000-0000-000000000000") {
            $("#addTransformationStatus").html("Select Source first.");
            return;
        }
        else { $("#addTransformationStatus").html(""); }

        var orderId = parseInt($("#transformationList ul").attr("count"));
        $.ajax({ url: '<%:Url.Action("Add", "TransformationType")%>/' + orderId, type: "POST", dataType: "html",

            success: function (html) {
                $("#transformationList ul").append(html);
            }

        });
        $("#transformationList ul").attr("count", orderId + 1);

        PopulateSelect(orderId);
    });

    function deleteTransformation(id)
    {
        $("#" + id).remove();
    }

    function PopulateSelect(orderId) {
        var id = "";
        for (var i = orderId - 1; i >= 0; i--) {
            if ($("#MId" + i) != null && $("#MId" + i).val() != null && $("#MId" + i).val() != "00000000-0000-0000-0000-000000000000") {
                id = "#MId" + i;
                break;
            }
        }

        if (id == "") id = "#TId";
        var text = $(id).val();
        if (text == null) text = "00000000-0000-0000-0000-000000000000";

        if ($("#MId" + orderId).attr("searchText") == text) return;

        $.ajax({ url: '<%:Url.Action("Find", "TransformationType")%>/' + text, type: "POST", dataType: "json",
            success: function (data) {
                var dropDown = $("#MId" + orderId);
                dropDown.children().remove();
                $.each(data, function (i, item) {
                    dropDown.append("<option value='" + item.Id + "'>" + item.Name + "</option>");
                });

                $("#MId" + orderId).attr("searchText", text);
            }
        })
    };

</script>