<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StatusControl.ascx.cs"
	Inherits="Enterprise.Tracking.Web.Declaration.CA.IMP.StatusControl" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls"
	Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls"
	Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<table class="ResultsTable DeclarationStatusDetailsSection">
	<tr>
		<th colspan="3">
			<asp:Label ID="StatusHeaderLabel" runat="server" CssClass="SectionTitle">Release Status Summary</asp:Label>
		</th>
	</tr>
	<tr>
		<td colspan="3">
			<asp:Label ID="CustomsReleaseStatusLabel" CssClass="DetailsItem" runat="server">Customs Release Status: </asp:Label><edi:ZTextLabel
				runat="server" ID="CustomsReleaseStatusText" BindTo="ReleaseStatusWrapper.ProcessingIndicatorDescription" />
		</td>
	</tr>
	<tr>
		<td colspan="3">
			<asp:Label ID="CCNLabel" CssClass="DetailsItem" runat="server">CCN: </asp:Label><edi:ZTextLabel
				runat="server" ID="CCNText" BindTo="ReleaseStatusWrapper.CCN" HideIfBlank="true" /><edi:ZTextLabel
				runat="server" ID="EffectiveCCNText" BindTo="EffectiveCCN" />
		</td>
	</tr>
	<tr>
		<td class="TextColumn">
			<asp:Label ID="ReleaseDateLabel" CssClass="DetailsItem" runat="server">Release Date: </asp:Label><edi:ZTextLabel
				runat="server" ID="ReleaseDateText" BindTo="ReleaseStatusWrapper.ReleaseDate" />
		</td>
		<td style="width: 50px;">
		</td>
		<td class="TextColumn">
			<asp:Label ID="ReleaseOfficeLabel" CssClass="DetailsItem" runat="server">Release Office: </asp:Label>
			<edi:ZTextLabel runat="server" ID="WrapperReleaseOfficeText" BindTo="ReleaseStatusWrapper.ReleaseOffice" />
			<edi:ZCodeFindBoxLabel ID="ReleaseOfficeText" runat="server" BindTo="JE_CustomsOffice" BindToList="Lookups.CBSAOffices" DisplayStyle="CodeAndDescription" />
		</td>
	</tr>
	<tr>
		<td>
			<asp:Label ID="WarehouseLabel" CssClass="DetailsItem" runat="server">Warehouse: </asp:Label><edi:ZTextLabel
				runat="server" ID="WarehouseText" BindTo="ReleaseStatusWrapper.Warehouse" />
		</td>
		<td style="width: 50px;">
		</td>
		<td>
			<asp:Label ID="ContainersLabel" CssClass="DetailsItem" runat="server">Containers: </asp:Label><edi:ZTextLabel
				runat="server" ID="ContainersText" BindTo="ReleaseStatusWrapper.Containers" />
		</td>
	</tr>
	<tr>
		<td colspan="3">
			<asp:Label ID="DeliveryInstructionsLabel" CssClass="DetailsItem" runat="server">Delivery Instructions: </asp:Label><edi:ZTextLabel
				runat="server" ID="DeliveryInstructions1Text" BindTo="ReleaseStatusWrapper.DeliveryInstructions1" /><asp:Label
					ID="DeliveryInstructionSeparator" runat="server" Text=", " />
			<edi:ZTextLabel runat="server" ID="DeliveryInstructions2Text" BindTo="ReleaseStatusWrapper.DeliveryInstructions2" />
		</td>
	</tr>
	<tr ID="DeclarationDatesRow" runat="server">
		<td>
			<asp:Label ID="AcceptedDateLabel" CssClass="DetailsItem" runat="server">Accepted Date: </asp:Label><edi:ZDateTimeLabel
				runat="server" ID="AcceptedDateText" BindTo="B3AcceptedDate" />
		</td>
		<td style="width: 50px;">
		</td>
		<td>
			<asp:Label ID="AccountingDateLabel" CssClass="DetailsItem" runat="server">Accounting Date: </asp:Label><edi:ZDateTimeLabel
				runat="server" ID="AccountingDateText" BindTo="CA_K84AccountingDate" />
		</td>
	</tr>
</table>
