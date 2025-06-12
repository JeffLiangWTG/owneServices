<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.View.ClientView>" %>

<div id="details"> 
    <fieldset>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_ID, "eHub ID") %>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_ID)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_ID)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_FriendlyName, "Name")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_FriendlyName)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_FriendlyName)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_Odyssey_OH, "Enterprise ID")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_Odyssey_OH)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_Odyssey_OH)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_DistributionZone, "Distribution Zone")%>
        </div>
        <div class="editor-field">
            <%: Html.DropDownListFor(model => model.Client.CC_DistributionZone, new SelectList(Model.DistributionZone, "ZZ_PK", "ZZ_ID"), "None")%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_DistributionZone)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_EmailAddress, "Email")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_EmailAddress)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_EmailAddress)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_Password, "Password")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_Password)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_Password)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_IsAirServiceProvider, "IsAirServiceProvider")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_IsAirServiceProvider)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_IsAirServiceProvider)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_AirlineCode, "AirlineCode")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_AirlineCode)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_AirlineCode)%>
        </div>
        <div class="clear"></div>
        
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_AirlinePrefix, "AirlinePrefix")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.Client.CC_AirlinePrefix)%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_AirlinePrefix)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_AirServiceProvider, "AirServiceProvider")%>
        </div>
        <div class="editor-field">
            <%: Html.DropDownListFor(model => model.Client.CC_AirServiceProvider, new SelectList(Model.AirServiceProvider, "CC_PK", "CC_ID"), "None")%>
            <%: Html.ValidationMessageFor(model => model.Client.CC_AirServiceProvider)%>
        </div>
        <div class="clear"></div>

        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Client.CC_AS2_Code, "AS2 Code") %>
        </div>
        <div>
            <%: Html.EditorFor(model => model.Client.CC_AS2_Code) %>
            <%: Html.ValidationMessageFor(model => model.Client.CC_AS2_Code) %>
        </div>
    </fieldset>
</div>
        
  