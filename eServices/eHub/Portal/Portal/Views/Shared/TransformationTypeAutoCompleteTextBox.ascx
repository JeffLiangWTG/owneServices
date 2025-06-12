<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.TransformationMappingView>" %>

<%
    var valueName = String.Format("MId{0}", Model.Mapping.TM_Order);
%>

<li id="li<%:Model.Mapping.TM_Order%>">
    <select id="<%:valueName%>" name="<%:valueName%>" onclick="PopulateSelect('<%:Model.Mapping.TM_Order%>')">
        <option value="<%:Model.Mapping.TM_TT_PK %>"><%:Model.TransformationTypeName%></option>
    </select>
    <a onclick="deleteTransformation('li<%:Model.Mapping.TM_Order%>'); return false;"><img alt="delete" src='<%:Url.Content("~/Content/Images/delete_button.gif")%>'/></a>
    <div class="clear"><%=Html.ValidationMessage(valueName)%></div>
</li>
   


