using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingBookingContainerColumnProvider))]
	sealed class ForwardingBookingContainerColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Container#", JobContainerSchema.JC_ContainerNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo(((MasterFiles.Business.RefContainerCollection)(((Freight.Forwarding.Business.ForwardingContainer)(null)).RefContainer_List)));
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			AddDefaultsColumn(new ZFindBoxColumn("Type", JobContainerSchema.JC_RC.Name, "RefContainer_List")
			{
				ColumnKey = WebTracker.Grids.TrackingContainers.Type,
				ValueFieldName = MasterFiles.Business.RefContainer.Schema.RC_Code,
				TextFieldName = MasterFiles.Business.RefContainer.Schema.RC_Description
			});
			AddDefaultsColumn(new ZCalcEditColumn("Count", JobContainerSchema.JC_ContainerCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingContainers.Type
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ForwardingBookingContainerColumnProvider();
		}
	}
}
