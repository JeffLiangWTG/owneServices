<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="WebScheduleChooserControl.ascx.cs" Inherits="Enterprise.Tracking.Web.Bookings.WebScheduleChooserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5%" %>
<edi:ZTextLabel id = "HeaderLabel" CssClass="SectionTitle" runat = "server"/>
<div id= "SailingDiv" runat = "server">
    <table class="ResultsTable">
        <tr>
            <td>
                <edi:ZTextLabel id = "VoyageNumberLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZTextBox id = "VoyageNumber" runat = "server"/>
            </td>
            <td style="width:10px"/>
            <td>
                <edi:ZTextLabel id = "JourneyLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZTextBox id = "Journey" runat = "server"/>
            </td>
        </tr>
        <tr>
            <td>
                <edi:ZTextLabel id = "LCLCutOffLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZDateTimeLabel id = "LCLCutOff" runat = "server"/>
            </td>
            <td style="width:10px"/>
            <td>
                <edi:ZTextLabel id = "FCLCutOffLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZDateEdit id = "FCLCutOff" runat = "server"/>
            </td>
        </tr>
        <tr>
            <td>
                <edi:ZTextLabel id = "ETDLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZDateEdit id = "ETD" runat = "server"/>
            </td>
            <td style="width:10px"/>
            <td>
                <edi:ZTextLabel id = "ETALabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZDateEdit id = "ETA" runat = "server"/>
            </td>
        </tr>
        <tr>
            <td>
                <edi:ZTextLabel id = "TotalWeightLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZNumericTextBox id = "TotalWeight" runat = "server"/>
                <edi:zdropdownlist id="UnitsOfWeight" runat="server"/>
            </td>
            <td style="width:10px"/>
            <td>
                <edi:ZTextLabel id = "TotalVolumeLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZNumericTextBox id = "TotalVolume" runat = "server"/>
                <edi:zdropdownlist id="UnitsOfVolume" runat="server"/>
            </td>
        </tr>
    </table>
</div>
<div id= "VisibleDiv" runat = "server">
    <table class="ResultsTable">
        <tr>
            <td>
                <edi:ZTextLabel id = "CarrierLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZGuidFindBox id = "Carrier" runat = "server" ModuleID="OrganisationWebTracking"/>
            </td>
            <td style="width:10px"/>
            <td>
                <edi:ZTextLabel id = "CFSRefLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZTextBox id = "CFSRef" runat = "server"/>
            </td>
        </tr>
        <tr>
            <td colspan = 5>                
                <edi:ZCheckBox id = "Direct" runat = "server" AutoPostBack = "True" CssClass="DetailsItem"/>                
            </td>            
        </tr>
        <tr>
            <td>
                <edi:ZTextLabel id = "ServiceLevelLabel" runat = "server" CssClass="DetailsItem"/>
            </td>
            <td>
                <edi:ZDropDownList id = "ServiceLevel" runat = "server"/>
            </td>
            <td style="width:10px"/>
            <td>
                <edi:ZTextLabel id = "MAWBLabel" runat = "server" CssClass="DetailsItem"/>
            </td>            
            <td>             
                <edi:ZTextBox id = "MAWBSeaNumberTextBox" runat = "server"/><edi:ZTextBox id = "MAWBPrefixTextBox" runat = "server"/><edi:ZTextLabel id = "MAWBHyphenLabel" runat = "server"/><edi:ZTextBox id = "MAWBNumberTextBox" runat = "server"/>
            </td>
        </tr>
        <tr>
            <td colspan = 5>
                <edi:ZCheckBox id = "IsNeutral" runat = "Server" CssClass="DetailsItem"/>
            </td>        
        </tr>        
    </table>    
</div>    
<edi:ZGuidFindBox id = "SelectScheduleBtn" runat = "server" HideTextBox = "True" AutoPostBack = "True" Visible = "False"/>