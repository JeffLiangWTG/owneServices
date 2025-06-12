<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.eHubTransactions.eHubMessageType>" %>
<b><%:Model.DT_Code%></b>
<div id="details"> 
    <fieldset>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.DT_IsFlatFile, "IsFlatFile")%>
        </div>
        <div class="editor-field">
            <%: Html.CheckBoxFor(model => model.DT_IsFlatFile)%>
            <%: Html.ValidationMessageFor(model => model.DT_IsFlatFile)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.DT_IsEDI, "IsEDI")%>
        </div>
        <div class="editor-field">
            <%: Html.CheckBoxFor(model => model.DT_IsEDI)%>
            <%: Html.ValidationMessageFor(model => model.DT_IsEDI)%>
        </div>
        <div class="clear"></div>
    </fieldset>
</div>


