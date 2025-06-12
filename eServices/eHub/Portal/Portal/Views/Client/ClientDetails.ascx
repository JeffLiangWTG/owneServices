<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CargoWise.eHub.Portal.Models.View.ClientView>" %>

<div id="details"> 
<fieldset>
    <div class="inline-lable">eHub ID</div>
    <div class="inline-field"><%:Model.Client.CC_ID%></div>
    <div class="clear"></div>

    <div class="inline-lable">Name</div>
    <div class="inline-field"><%:Model.Client.CC_FriendlyName%></div>
    <div class="clear"></div>

    <div class="inline-lable">Enterprise ID</div>
    <div class="inline-field"><%:Model.Client.CC_Odyssey_OH%></div>
    <div class="clear"></div>

    <div class="inline-lable">Distribution Zone</div>
    <div class="inline-field"><%:Model.DistributionZoneName%></div>
    <div class="clear"></div>

    <div class="inline-lable">Email</div>
    <div class="inline-field"><%:Model.Client.CC_EmailAddress%></div>
    <div class="clear"></div>

    <div class="inline-lable">Password</div>
    <div class="inline-field"><%:Model.Client.CC_Password%></div>
    <div class="clear"></div>

    <div class="inline-lable">IsAirServiceProvider</div>
    <div class="inline-field"><% if (Model.Client.CC_IsAirServiceProvider != null && Model.Client.CC_IsAirServiceProvider.Value) {%> <img src="<%:Url.Content("~/Content/Images/Checked.png")%>" alt="True"/>  <%}%></div>
    <div class="clear"></div>

    <div class="inline-lable">AirlineCode</div>
    <div class="inline-field"><%:Model.Client.CC_AirlineCode%></div>
    <div class="clear"></div>
    
    <div class="inline-lable">AirlinePrefix</div>
    <div class="inline-field"><%:Model.Client.CC_AirlinePrefix%></div>
    <div class="clear"></div>

    <div class="inline-lable">AirlineServiceProvider</div>
    <div class="inline-field"><%:Model.AirServiceProviderName%></div>
    <div class="clear"></div>

    <div class="inline-lable">AS2 Code</div>
    <div class="inline-field"><%:Model.Client.CC_AS2_Code%></div>
    <div class="clear"></div>
    

</fieldset>
</div>
        

