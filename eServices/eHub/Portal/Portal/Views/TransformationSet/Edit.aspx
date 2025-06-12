<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.TransformationSetView>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
    <ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li> 
        <li><%: Html.ActionLink("Transformation Sets", "Index", "TransformationSet")%></li>    
        <li><%: Html.ActionLink(Model.TransformationSet.TS_Name, "Details", new { id = Model.TransformationSet.TS_PK})%></li>
        <li class="active">Edit</li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm()) {%>
    <%: Html.ValidationSummary(false, "Please correct the errors and try again.") %>    
    <div id="details"> 
    <fieldset>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_Name, "Name") %>
        </div>  
        <div class="editor-field">
            <%: Html.EditorFor(model => model.TransformationSet.TS_Name)%>
            <%: Html.ValidationMessage("TS_Name")%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Sender)%>
        </div>
        <div class="editor-field">
            <%Html.RenderPartial("ClientAutoCompleteTextBox", Model.Sender); %>
            <%: Html.ValidationMessage("TS_CC_Sender")%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.Recipient)%>   
        </div>  
        <div class="editor-field">
            <%Html.RenderPartial("ClientAutoCompleteTextBox", Model.Recipient); %>
            <%: Html.ValidationMessage("TS_CC_Recipient")%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_DT_Source, "Source")%>
        </div>
        <div class="editor-field">
            <%Html.RenderPartial("MessageTypeAutoCompleteTextBox", Model.Source); %>
            <%: Html.ValidationMessage("TS_DT_Source")%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_XPathPredicate, "XPathPredicate")%>
        </div>  
        <div class="editor-field">
            <%: Html.EditorFor(model => model.TransformationSet.TS_XPathPredicate)%>
            <%: Html.ValidationMessageFor(model => model.TransformationSet.TS_XPathPredicate)%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_BillSender, "BillSender")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.TransformationSet.TS_BillSender)%>
            <%: Html.ValidationMessageFor(model => model.TransformationSet.TS_BillSender)%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_BillRecipient, "BillRecpient")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.TransformationSet.TS_BillRecipient)%>
            <%: Html.ValidationMessageFor(model => model.TransformationSet.TS_BillRecipient)%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.BillOther)%>
        </div>
        <div class="editor-field">
            <%Html.RenderPartial("ClientAutoCompleteTextBox", Model.BillOther); %>
            <%: Html.ValidationMessage("TS_CC_BillOther")%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_BillingNumMessagesIncluded, "BillingMsgsIncluded")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.TransformationSet.TS_BillingNumMessagesIncluded)%>
            <%: Html.ValidationMessageFor(model => model.TransformationSet.TS_BillingNumMessagesIncluded)%>
        </div>
        <div class="clear"></div>
        <div class="inline-lable">
            <%: Html.LabelFor(model => model.TransformationSet.TS_BillingFee, "BillingFee")%>
        </div>
        <div class="editor-field">
            <%: Html.EditorFor(model => model.TransformationSet.TS_BillingFee)%>
            <%: Html.ValidationMessageFor(model => model.TransformationSet.TS_BillingFee)%>
        </div>
        <div class="clear"></div>

        <div class="line-lable">Transformations</div>
        <%Html.RenderPartial("TransformationEditList", Model.MappingList); %>

    </fieldset>
        <div id="actionBlock">
            <div class="leftActionButton"><input type="submit" value="Save" class="actionButton" /></div>
            <div class="rightActionButton">

                    <% if (ViewContext.RouteData.GetRequiredString("action") == "Edit")
                       {%>
                    <%: Html.ActionLink("Cancel", "Details", new { id = Model.TransformationSet.TS_PK })%>
                    <%} %>

                    <% if (ViewContext.RouteData.GetRequiredString("action") == "Create")
                       {%>
                    <%: Html.ActionLink("Back to List", "Index")%>
                    <%} %>
                    </div>
            <div class="clear"></div>
        </div>
    </div>    

<% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="<%:Url.Content("~/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css")%>" rel="Stylesheet" type="text/css" media="screen" />
    <link href="<%:Url.Content("~/PlugIns/ColorBox/colorbox.css")%>" rel="stylesheet" type="text/css" />
    <script src="<%:Url.Content("~/PlugIns/ColorBox/jquery.colorbox-min.js")%>" type="text/javascript"></script>
    
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script src="//ajax.microsoft.com/ajax/jQuery.Validate/1.7/jQuery.Validate.min.js" type="text/javascript"></script>
    <script src="//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.validate.unobtrusive.min.js" type="text/javascript"></script>
</asp:Content>
