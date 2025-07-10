<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="True" Codebehind="OrgSupplierPartUserControl.ascx.cs" Inherits="Enterprise.Tracking.Web.OrgSupplierPartUserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<TABLE class="ResultsTable" id="SearchTable">
	<TBODY>
		<TR>
			<TD noWrap><edi:ZTextLabel id="ProductCodeLabel" runat="server">Product Code:</edi:ZTextLabel></TD>
			<TD noWrap style="width: 168px"><edi:ZTextBox id="ProductCode" runat="server" BindTo="OP_PartNum"></edi:ZTextBox></TD>
		</TR>
		<TR>
			<TD noWrap><edi:ZTextLabel id="ProductDescriptionLabel" runat="server">Product Description:</edi:ZTextLabel></TD>
			<TD noWrap style="width: 168px"><edi:ZTextBox id="ProductDescription" runat="server" BindTo="OP_Desc"></edi:ZTextBox></TD>
		</TR>
		<TR>
			<TD>&nbsp;</TD>
			<TD noWrap style="width: 168px">
				<edi:zradiobutton id="StartsWith" runat="server" Text="Starts With" GroupName="StartsContains" BindTo="DescStartsWith"
					Checked="True"></edi:zradiobutton>
				<edi:zradiobutton id="Contains" runat="server" Text="Contains" GroupName="StartsContains" BindTo="DescContains"></edi:zradiobutton>
			</TD>
		</TR>
        <tr>
            <td style="height: 21px">
                <edi:ZTextLabel ID="StatusLabel" runat="server">Product Status</edi:ZTextLabel></td>
            <td nowrap="nowrap" style="width: 186px; height: 21px">
                <edi:ZRadioButton ID="Active" runat="server" BindTo="Active" GroupName="Status"
                    Text="Active" />
                <edi:ZRadioButton ID="Inactive" runat="server" BindTo="Inactive" GroupName="Status"
                    Text="Inactive" />
                <edi:ZRadioButton ID="Both" runat="server" BindTo="Both" Checked="True"
                    GroupName="Status" Text="Both" /></td>
        </tr>
	</TBODY>
</TABLE>
