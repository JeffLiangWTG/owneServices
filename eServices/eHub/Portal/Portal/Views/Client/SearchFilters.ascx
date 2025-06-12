<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.ClientFilterViewModel>" %>
<%
    var htmlAttributes = new Dictionary<string, object> { { "data-autopostback", "true" } }; 
%>
<%
    using (Html.BeginForm("Index", "Client", FormMethod.Get))
    { %>
    <div id="searchFilter">
        <div class="label-column">Client ID</div>
        <div class="input-column"><%:Html.TextBox("Id", Model.ID, new { size = 40, maxlength = "36" })%></div>
        <div class="clear"></div>
        <div class="label-column">Client Name</div>
        <div class="input-column"><%:Html.TextBox("Name", Model.Name, new { size = 40, maxlength = "128" })%></div>
        <div class="clear"></div>
        <div class="label-column">AS2 Code</div>
        <div class="input-column"><%:Html.TextBox("AS2Code", Model.AS2Code, new { size = 40, maxlength = "50" })%></div>
        <div class="clear"></div>
        <div class="label-column">Is Airline</div>
        <div class="input-column"><%:Html.CheckBox("IsAirline", Model.IsAirline)%></div>
        <div class="clear"></div>
        <div class="label-column">Airline Code</div>
        <div class="input-column"><%:Html.TextBox("AirlineCode", Model.AirlineCode, new { size = 2, maxlength = "2" })%></div>
        <div class="clear"></div>
        <div class="label-column">Airline Prefix</div>
        <div class="input-column"><%:Html.TextBox("AirlinePrefix", Model.AirlinePrefix, new { size = 3, maxlength = "3" })%></div>
        <div class="clear"></div>
        <div class="cancel"><%: Html.ActionLink("Clear", "Index")%></div>
        <div class="search"><input type="submit" value="Search" class="btnNeutral" /></div>
        <div class="clear"></div>
    </div>
<%} %>