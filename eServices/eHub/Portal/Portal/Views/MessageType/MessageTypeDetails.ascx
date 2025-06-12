<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.eHubTransactions.eHubMessageType>" %>

<div id="details"> 
<fieldset>
        
    <div class="inline-lable">Name</div>
    <div class="inline-field"><%:Model.DT_Code%></div>
    <div class="clear"></div>

    <div class="inline-lable">IsFlatFile</div>
    <div class="inline-field"><% if (Model.DT_IsFlatFile) {%> <img src="<%:Url.Content("~/Content/Images/Checked.png")%>" alt="True"/>  <%}%></div>
    <div class="clear"></div>

    <div class="inline-lable">IsEDI</div>
    <div class="inline-field"><% if (Model.DT_IsEDI) {%> <img src="<%:Url.Content("~/Content/Images/Checked.png")%>" alt="True"/>  <%}%></div>
    <div class="clear"></div>

    </fieldset>
</div>


