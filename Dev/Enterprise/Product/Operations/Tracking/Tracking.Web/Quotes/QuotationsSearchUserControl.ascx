<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuotationsSearchUserControl.ascx.cs" Inherits="Enterprise.Tracking.Web.Quotes.QuotationsSearchUserControl" %>
<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls"
	TagPrefix="edi" %>
<table id="Table2" class="ResultsTable">
	<tbody>
		<tr>
			<td nowrap="nowrap" style="height: 26px">
				<edi:ZTextLabel ID="NumbersTextLabel" runat="server">Quote No.:</edi:ZTextLabel></td>
			<td nowrap="nowrap" style="height: 26px">
			<edi:ZTextBox ID="QuoteNumberTextBox" runat="server" BindTo="QuoteNumber"></edi:ZTextBox></td>
			<td style="height: 26px">
				&nbsp;</td>
			<td nowrap="nowrap" style="height: 26px">
			</td>
			<td colspan="4" nowrap="nowrap" style="height: 26px">
			</td>
		</tr>
		<tr>
			<td nowrap="nowrap" style="height: 26px">
			<edi:ztextlabel id="StatusTextLabel" runat="server">Status:</edi:ztextlabel></td>
			<td nowrap="nowrap" style="height: 26px">
			<edi:zdropdownlist id="StatusDropDownList" runat="server" BindTo="QuoteStatus" BindToList="StatusList"></edi:zdropdownlist></td>
			<td style="height: 26px">
			</td>
			<td nowrap="nowrap" style="height: 26px">
			<edi:ztextlabel id="TransportTextLabe" runat="server">Transport:</edi:ztextlabel></td>
			<td colspan="4" style="height: 26px">
			<edi:zdropdownlist id="TransportModeDropDownList" runat="server" BindTo="FilterTransportMode" BindToList="TransportModes"></edi:zdropdownlist></td>
		</tr>
		<tr>
			<td nowrap="nowrap" style="height: 26px">
			<edi:ztextlabel id="DatesTextLabel" runat="server">Dates:</edi:ztextlabel></td>
			<td nowrap="nowrap" style="height: 26px">
			<edi:zdropdownlist id="DatesDropDownList" runat="server" BindTo="Dates" BindToList="DatesList"></edi:zdropdownlist></td>
			<td style="height: 26px">
			</td>
			<td nowrap="nowrap" style="height: 26px">
				<edi:ZTextLabel ID="FromDateTextLabel" runat="server">From:</edi:ZTextLabel></td>
			<td style="height: 26px">
				<edi:zdateedit id="FromDateEdit" runat="server" width="121px" BindTo="FilterDateFrom"></edi:zdateedit>
			</td>
			<td style="height: 26px">
			</td>
			<td nowrap="nowrap" style="height: 26px">
				<edi:ZTextLabel ID="ToDateTextLabel" runat="server">To:</edi:ZTextLabel></td>
			<td style="width: 3px; height: 26px">
				<edi:zdateedit id="ToDateEdit" runat="server" width="121px" BindTo="FilterDateTo"></edi:zdateedit>
			</td>
		</tr>
		<tr>
			<td nowrap="nowrap" style="height: 26px">
				<edi:ZTextLabel ID="Ztextlabel1" runat="server">Ports:</edi:ZTextLabel></td>
			<td nowrap="nowrap" style="height: 26px">
			</td>
			<td style="height: 26px">
				&nbsp;</td>
			<td style="height: 26px">
				<edi:ZTextLabel ID="PortsTextLabel" runat="server">Origin:</edi:ZTextLabel></td>
			<td nowrap="nowrap" style="height: 26px">
			<edi:ZFindBox ID="OriginPort" runat="server" BindTo="FilterPort1" ModuleID="RefUNLOCOWeb">
			</edi:ZFindBox>
			</td>
			<td style="height: 26px">
				&nbsp;</td>
			<td style="height: 26px">
			<edi:ZTextLabel ID="Ztextlabel2" runat="server">Destination:</edi:ZTextLabel></td>
			<td nowrap="nowrap" style="width: 3px; height: 26px">
			<edi:ZFindBox ID="DestinationPort" runat="server" BindTo="FilterPort2" ModuleID="RefUNLOCOWeb">
			</edi:ZFindBox>
			</td>
		</tr>
	</tbody>
</table>
