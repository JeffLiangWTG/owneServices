<%@ Control Language="c#" AutoEventWireup="True" Codebehind="FumigationSearchUserControl.ascx.cs" Inherits="Enterprise.WebCFS.Web.FumigationSearchUserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<table class="ResultsTable" id="Table2">
	<tbody>
		<tr>
			<td noWrap><edi:ZTextLabel id="ZLabel1" runat="server">Container Number:</edi:ZTextLabel></td>
			<td noWrap colspan="3"><edi:ztextbox id="ContainerNumber" runat="server" BindTo="LFV_ContainerNum" Width="100%"/></td>
		</tr>
		<tr>
			<td noWrap><edi:ZTextLabel id="Zlabel2" runat="server">Date:</edi:ZTextLabel></td>
			<td noWrap colspan="3"><edi:zdropdownlist id="DateFilterList" runat="server" BindTo="ContainerDateType" BindToList="DateList"
					ShowDescriptionInDropDown="True" width="100%"/></td>
		</tr>
		<tr>
			<td noWrap><edi:ZTextLabel id="Zlabel7" runat="server">From:</edi:ZTextLabel></td>
			<td noWrap><edi:zdateedit id="AvailabilityDate" runat="server" BindTo="FromDate" /></td>
			<td noWrap><edi:ZTextLabel id="Zlabel4" runat="server">To:</edi:ZTextLabel></td>
			<td nowrap><edi:zdateedit id="Ztextbox2" runat="server" BindTo="ToDate" /></td>
		</tr>
		<tr>
			<td noWrap><edi:ZTextLabel id="Zlabel5" runat="server">Vessel:</edi:ZTextLabel></td>
			<td noWrap colspan="3"><edi:ztextbox id="Vessel" runat="server" BindTo="LFV_Vessel" Width="100%" /></td>
		</tr>
		<tr>
			<td noWrap><edi:ZTextLabel id="Zlabel6" runat="server">Voyage:</edi:ZTextLabel></td>
			<td noWrap colspan="3"><edi:ztextbox id="Voyage" runat="server" BindTo="LFV_VoyageFlight" Width="100%" />
			</td>
		</tr>
	</tbody>
</table>
