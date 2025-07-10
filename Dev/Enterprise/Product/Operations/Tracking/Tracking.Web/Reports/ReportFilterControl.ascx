<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReportFilterControl.ascx.cs" Inherits="Enterprise.Tracking.Web.ReportFilterControl" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<div id="ColumnConfigDIV" runat="server">
    <br />
    <edi:ztextlabel id="ColumnConfigLabel" runat="server"></edi:ztextlabel><br />
    <edi:ZDropDownList ID="ColumnConfigDropDownList" runat="server" AutoPostBack="true" />
</div>
<div>
    <table id="FilterTable" runat="server" cellpadding="0" cellspacing="0">
    </table>
</div>
<table id="SortGroupByTemplateTable" runat="server" cellpadding="0" cellspacing="0">
    <tr>
        <td>
            <div id="SortDIV" runat="server">
                <br />
                <edi:ztextlabel id="SortLabel" runat="server"></edi:ztextlabel><br />      
                <edi:ZRadioButtonList ID="SortOrderList" BindTo = "SortOrderCollection" runat="server"/>
            </div>
        </td>            
    </tr>    
    <tr>
        <td>
            <div id="GroupByDIV" runat="server">
                <br />
                <edi:ztextlabel id="GroupByLabel" runat="server"></edi:ztextlabel><br />
                <edi:ZRadioButtonList ID="GroupBysList" BindTo = "GroupByCollection" runat="server"/><br />
                <edi:ZCheckBox ID="PageBreakOnNewGroup" runat="server"/>
            </div>
        </td>
    </tr>
    <tr>
        <td>
            <div id="OptionalTemplateDIV" runat="server">
                <br />
                <edi:ztextlabel id="OptionalTemplateLabel" runat="server"></edi:ztextlabel><br />   
                <edi:ZCheckBoxList ID="OptionalTemplateList" BindTo = "OptionalTemplateSheetList" runat="server"/>
            </div>
        </td>
    </tr>
    <tr>
		<td>
            <br />
			<edi:ztextlabel id="FormatTypeLabel" runat="server"></edi:ztextlabel> &nbsp; 
			<edi:ZDropDownList ID="FormatTypeDropDownList" runat="server"/>
		</td>
	</tr>
</table>
<div>
    <br />
    <edi:ztextlabel id="ReportLanguageLabel" runat="server"></edi:ztextlabel> &nbsp; 
    <edi:ZDropDownList id="LanguageDropDownList" runat="server"/>
    <br />
    <br />
    <edi:ZButton ID="RunReportButton" runat="server" OnClick="RunReportButton_Click" />
</div>
