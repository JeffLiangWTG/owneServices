<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="True" Codebehind="AccountsSearchUserControl.ascx.cs" Inherits="Enterprise.Tracking.Web.Accounts.AccountsSearchUserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<table class="ResultsTable" id="Table2">
	<TBODY>
		<TR>
			<TD noWrap><edi:ztextlabel id="NumbersLabel" runat="server">Numbers:</edi:ztextlabel></TD>
			<TD noWrap><edi:zdropdownlist id="NumberFilterList" runat="server" BindToList="AH_NumberFilter_List" BindTo="AH_NumberFilter"></edi:zdropdownlist></TD>
			<TD noWrap colSpan="2"><edi:ztextbox id="SearchNumber" runat="server" BindTo="AH_Number"></edi:ztextbox></TD>
			<TD><edi:ztextlabel id="TransactionTypeLabel" runat="server">Tran. Type:</edi:ztextlabel></TD>
			<TD><edi:zdropdownlist id="TransactionTypeList" runat="server" BindToList="TransactionTypeList" BindTo="AH_TransactionType"></edi:zdropdownlist></TD>
		</TR>
		<TR>
			<TD noWrap><edi:ztextlabel id="DatesLabel" runat="server">Dates:</edi:ztextlabel></TD>
			<TD noWrap><edi:zdropdownlist id="DateFilterList" runat="server" BindToList="AH_DateFilter_List" BindTo="AH_DateFilter"></edi:zdropdownlist></TD>
			<TD noWrap><edi:ztextlabel id="FromLabel" runat="server">From:</edi:ztextlabel></TD>
			<TD><edi:zdateedit id="Date1" runat="server" BindTo="AH_FromDate" ToolTip="Select a date"></edi:zdateedit></TD>
			<TD noWrap><edi:ztextlabel id="ToLabel" runat="server">To:</edi:ztextlabel></TD>
			<TD><edi:zdateedit id="Date2" runat="server" BindTo="AH_ToDate" ToolTip="Select a date"></edi:zdateedit></TD>
		</TR>
		<TR>
			<TD noWrap><edi:ztextlabel id="Ztextlabel2" runat="server">Issued by:</edi:ztextlabel></TD>
			<TD noWrap colspan="5"><edi:zguiddropdownlist id="Zdropdownlist2" runat="server" BindToList="Companies" BindTo="FilterByCompanyPK"
					DataTextField="GC_Name" DataValueField="PK"></edi:zguiddropdownlist></TD>
		</TR>
		<TR>
			<TD noWrap><edi:ztextlabel id="PaymentStatusLabel" runat="server">Payment Status:</edi:ztextlabel></TD>
			<TD noWrap><edi:zdropdownlist id="PaymentStatus" runat="server" BindToList="PaymentStatusList" BindTo="PaymentStatus"></edi:zdropdownlist></TD>
			<TD noWrap align="right" colSpan="4"></TD>
		</TR>
	</TBODY>
</table>
