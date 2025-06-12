<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/SiteBase.Master" Inherits="System.Web.Mvc.ViewPage<CargoWise.eHub.Portal.Models.TransformationSetView>" %>

<asp:Content ID="BreadcrumbContent" ContentPlaceHolderID="BreadcrumbContent" runat="server">
<ol class="breadcrumb">
        <li><a href="/">
            <i class="glyphicon glyphicon-home"></i>
        </a></li>
        <li><%: Html.ActionLink("Setup", "Index", "TransformationSet")%></li>  
        <li><%: Html.ActionLink("Transformation Sets", "Index", "TransformationSet")%></li>   
        <li class="active"><%: Model.TransformationSet.TS_Name %></li>
    </ol>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<div id="details"> 
    <fieldset>
        <div class="inline-lable">Name</div>
        <div class="inline-field"><%:Model.TransformationSet.TS_Name%></div>
        <div class="clear"></div>

        <div class="inline-lable">Sender</div>
        <div class="inline-field"><%:Model.Sender.DisplayName%></div>
        <div class="clear"></div>

        <div class="inline-lable">Recipient</div>
        <div class="inline-field"><%:Model.Recipient.DisplayName%></div>
        <div class="clear"></div>
            
        <div class="inline-lable">Source</div>
        <div class="inline-field"><%:Model.Source.Name%></div>
        <div class="clear"></div>

        <div class="inline-lable">XPathPredicate</div>
        <div class="inline-field"><%:Model.TransformationSet.TS_XPathPredicate%></div>
        <div class="clear"></div>

        <div class="inline-lable">Bill Sender</div>
        <div class="inline-field"><% if (Model.TransformationSet.TS_BillSender ?? false) {%> <img src="<%:Url.Content("~/Content/Images/Checked.png")%>" alt="True"/>  <%}%></div>
        <div class="clear"></div>

        <div class="inline-lable">Bill Recipient</div>
        <div class="inline-field"><% if (Model.TransformationSet.TS_BillRecipient ?? false) {%> <img src="<%:Url.Content("~/Content/Images/Checked.png")%>" alt="True"/>  <%}%></div>
        <div class="clear"></div>

        <div class="inline-lable">Bill Other</div>
        <div class="inline-field"><%:Model.BillOther.DisplayName%></div>
        <div class="clear"></div>

        <div class="inline-lable">Billing Number of Messages Included</div>
        <div class="inline-field"><%:Model.TransformationSet.TS_BillingNumMessagesIncluded%></div>
        <div class="clear"></div>

        <div class="inline-lable">Billing Fee</div>
        <div class="inline-field"><%:Model.TransformationSet.TS_BillingFee%></div>
        <div class="clear"></div>

        <div class="line-lable">Transformations</div>
        <div class="list">
            <ul>
            <% 
                for (int i = 0; i < Model.MappingList.Count; i++)
                {%>
                    <li>
                    <%:Model.MappingList[i].TransformationTypeName%>
                    </li>
                <%}
            %>  
            </ul>          
        </div>

    </fieldset>
    <div id="actionBlock">
        <% if (Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableMappingEdits"] ?? "false")) { %>
            <div class="leftActionButton">
                <%: Html.ActionLink("Edit", "Edit", new {id=Model.TransformationSet.TS_PK}) %>
                <%: Html.ActionLink("Delete", "Delete", new {id=Model.TransformationSet.TS_PK}) %>
            </div>
        <% } %>
        <div class="rightActionButton">
        <%: Html.ActionLink("Back to List", "Index", new { sender = Model.TransformationSet.TS_CC_Sender, recipient = Model.TransformationSet.TS_CC_Recipient})%>
        </div>
        <div class="clear"></div>
    </div>
</div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
