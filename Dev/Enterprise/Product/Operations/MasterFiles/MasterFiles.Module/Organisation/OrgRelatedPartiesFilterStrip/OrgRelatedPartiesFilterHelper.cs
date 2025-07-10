using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public static class OrgRelatedPartiesFilterHelper
	{
		public static ZQuery GetDocAddressFromOrgAddressFilter(ZQuery orgAddressFilter, bool orgAddressNotIn)
		{
			ZDBOnlySubQuery orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address, orgAddressNotIn);
			orgAddress.AddToFilter(orgAddressFilter);

			ZDBOnlyQuery docAddress = new ZDBOnlyQuery(typeof(JobDocAddress));
			docAddress.AddSubQuery(orgAddress, JoinCondition.And);

			return docAddress;
		}

		public static ZQuery GetDocAddressFromOrgHeaderFilter(ZQuery orgHeaderFilter, ZQuery orgAddressFilter, bool orgHeaderNotIn, bool orgAddressNotIn)
		{
			ZQuery orgAddress = GetOrgAddressFromOrgHeaderFilter(orgHeaderFilter, orgHeaderNotIn);
			orgAddress.AddToFilter(orgAddressFilter);
			return GetDocAddressFromOrgAddressFilter(orgAddress, orgAddressNotIn);
		}

		public static ZQuery GetJobHeaderFromOrgAddressFilter(ZQuery orgAddressFilter, bool orgAddressNotIn)
		{
			ZDBOnlySubQuery orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr, orgAddressNotIn);
			orgAddress.AddToFilter(orgAddressFilter);

			ZDBOnlyQuery jobHeader = new ZDBOnlyQuery(typeof(JobHeader));
			jobHeader.AddSubQuery(orgAddress, JoinCondition.And);

			return jobHeader;
		}

		public static ZQuery GetJobHeaderFromOrgHeaderFilter(ZQuery orgHeaderFilter, ZQuery orgAddressFilter, bool orgHeaderNotIn, bool orgAddressNotIn)
		{
			ZQuery orgAddress = GetOrgAddressFromOrgHeaderFilter(orgHeaderFilter, orgHeaderNotIn);
			orgAddress.AddToFilter(orgAddressFilter);
			return GetJobHeaderFromOrgAddressFilter(orgAddress, orgAddressNotIn);
		}

		public static ZQuery GetOrgAddressFromOrgHeaderFilter(ZQuery orgHeaderFilter, bool orgHeaderNotIn)
		{
			ZDBOnlySubQuery orgHeader = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH, orgHeaderNotIn);
			orgHeader.AddToFilter(orgHeaderFilter);

			ZDBOnlyQuery orgAddress = new ZDBOnlyQuery(typeof(OrgAddress));
			orgAddress.AddSubQuery(orgHeader, JoinCondition.And);

			return orgAddress;
		}

		#region Filter Control Builders

		public static void AddAllFilterControlBuilders(IFilterControl filterControl)
		{
			Argument.NotNull(filterControl, "filterControl");

			filterControl.FilterStripAdding += FilterControl_FilterStripAdding;
		}

		static void FilterControl_FilterStripAdding(object sender, ZFilterStripEventArgs e)
		{
			e.Strip.AddCustomFilterControlsBuilder(new OrgRelatedPartiesModuleFilterControlBuilder());
		}

		public class OrgRelatedPartiesModuleFilterControlBuilder : ZFilterStrip.CustomFilterControlsBuilder
		{
			public override bool Handles(ZArchitecture.Business.ModuleFilter moduleFilter)
			{
				return moduleFilter is OrgRelatedPartiesModuleFilter;
			}

			public override Control[] GetFilterControls(ZFilterStrip parentStrip, ZArchitecture.Business.ModuleFilter moduleFilter, ZBindingSource bindingSource)
			{
				OrgRelatedPartiesFilterControl control = new OrgRelatedPartiesFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				parentStrip.SetHeight(control.Height);
				return new Control[] { control };
			}
		}

		#endregion
	}
}
