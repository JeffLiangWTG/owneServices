<%@ Control Language="c#" AutoEventWireup="True" Codebehind="SailingSearchUserControl.ascx.cs" Inherits="Enterprise.WebCFS.Web.SailingSearchUserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<table class="ResultsTable" id="Table2">
	<TBODY>
		<tr>
			<td noWrap><edi:ZTextLabel id="Zlabel1" runat="server">Date:</edi:ZTextLabel></td>
			<td noWrap><edi:zdropdownlist id="Zdropdownlist1" runat="server" BindTo="JX_DateFilterType" BindToList="JX_DateFilterType_List"
					ShowDescriptionInDropDown="True" width="100%"></edi:zdropdownlist></td>
			<td noWrap colspan="3">&nbsp;</td>
		</tr>
		<tr>
			<td noWrap><edi:ZTextLabel id="Zlabel7" runat="server">From:</edi:ZTextLabel></td>
			<td noWrap><edi:zdateedit id="AvailabilityDate" runat="server" BindTo="JX_FromDate" width="100%"/></td>
			<td noWrap>&nbsp;</td>
			<TD noWrap><edi:ZTextLabel id="Zlabel4" runat="server">To:</edi:ZTextLabel></TD>
			<td nowrap><edi:zdateedit id="Ztextbox2" runat="server" BindTo="JX_ToDate"  width="100%"/></td>
		</tr>
		<TR>
			<TD noWrap><edi:ZTextLabel id="Zlabel3" runat="server">Vessel:</edi:ZTextLabel></TD>
			<TD noWrap><edi:ztextbox id="Ztextbox1" runat="server" BindTo="JX_JV_NKVessel" width="100%"/></TD>
			<td noWrap>&nbsp;</td>
			<TD noWrap><edi:ZTextLabel id="Zlabel9" runat="server">Voyage:</edi:ZTextLabel></TD>
			<TD noWrap><edi:ztextbox id="Ztextbox3" runat="server" BindTo="JX_JV_VoyageFlight"  width="100%"/></TD>
		<TR>
			<TD noWrap><edi:ZTextLabel id="Zlabel5" runat="server">Load Port:</edi:ZTextLabel></TD>
			<td>
				<edi:ZFindBox id="ZFindPortCode2" runat="server" BindTo="JX_RL_NKPort1" ModuleID="RefUNLOCOWeb"
					ToolTip="Pick the country or port from a list"></edi:ZFindBox>
			</td>
			<td noWrap>&nbsp;</td>
			<td nowrap>Discharge Port:</td>
			<TD noWrap>
				<edi:ZFindBox id="ZFindPortCode1" runat="server" BindTo="JX_RL_NKPort2" ModuleID="RefUNLOCOWeb"
					ToolTip="Pick the country or port from a list"></edi:ZFindBox>
			</TD>
		</TR>
		<tr>
			<TD noWrap><edi:ZTextLabel id="Zlabel2" runat="server">Status:</edi:ZTextLabel></TD>
			<td noWrap><edi:zdropdownlist id="DateFilterList" runat="server" BindTo="ShipStatus" BindToList="JA_E_DEP_List"
					ShowDescriptionInDropDown="True" width="100%"></edi:zdropdownlist></td>
			<td noWrap colspan="3">&nbsp;</td>
		</tr>
	</TBODY>
</table>
