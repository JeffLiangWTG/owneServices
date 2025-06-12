<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.MessageTypeFilterViewModel>" %>
<%
    var htmlAttributes = new Dictionary<string, object> { { "data-autopostback", "true" } }; 
%>
<%
    using (Html.BeginForm("Index", "MessageType", FormMethod.Get))
    { %>
    <div id="searchFilter">
        <div class="label-column">Code</div>
        <div class="input-column"><%:Html.TextBox("Code", Model.Code, new { size = 40, maxlength = "200" })%></div>
        <div class="clear"></div>
        <div class="label-column">Format</div>
        <div class="input-column"><%:Html.DropDownList("formatID", Model.Formats, "-- All --", htmlAttributes)%> </div>
        <div class="clear"></div>
        <div class="cancel"><%: Html.ActionLink("Clear", "Index")%></div>
        <div class="search"><input type="submit" value="Search" class="btnNeutral" /></div>
        <div class="clear"></div>
    </div>
<%} %>