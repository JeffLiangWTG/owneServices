using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class OrgRatingDocRollupOrGroupHelper
	{
		public OrgRatingDocRollupOrGroupHelper(IRatingDocRollupOrSort parent)
		{
			this.parent = parent;
		}

		protected readonly IRatingDocRollupOrSort parent;

		#region validations

		public void ValidateModule()
		{
			MandatoryValidation.CheckEntered(parent.ModuleInfo);
			ListValidation.ErrorIfInvalidCode(parent.ModuleInfo, ModuleList);
			CheckDuplicatedLines(parent.ModuleInfo);
		}

		public void ValidateJobType()
		{
			MandatoryValidation.CheckEntered(parent.JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(parent.JobTypeInfo, JobTypeList);
			CheckDuplicatedLines(parent.JobTypeInfo);
		}

		public void ValidateTransportMode()
		{
			MandatoryValidation.CheckEntered(parent.TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(parent.TransportModeInfo, TransportModeList);
			CheckDuplicatedLines(parent.TransportModeInfo);
		}

		public void ValidateDisplay()
		{
			MandatoryValidation.CheckEntered(parent.DisplayInfo);
			ListValidation.ErrorIfInvalidCode(parent.DisplayInfo, DisplayList);
		}

		public void ValidateStyle()
		{
			MandatoryValidation.CheckEntered(parent.StyleInfo);
			ListValidation.ErrorIfInvalidCode(parent.StyleInfo, StyleList);
		}

		#endregion

		#region lookups

		public CodeDescriptionPairList ModuleList => new DocRollupOrSortModuleList();

		public CodeDescriptionPairList JobTypeList => new DocRollupOrSortJobTypeList();

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(DocRollupOrSortTransportModeList.Codes.All, DocRollupOrSortTransportModeList.Descriptions.All);

				var specificList = new CodeDescriptionPairList();
				switch (parent.JobType.ToString())
				{
					case DocRollupOrSortJobTypeList.Codes.Forwarding:
					case DocRollupOrSortJobTypeList.Codes.Customs:
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.AirFreight, DocRollupOrSortTransportModeList.Descriptions.AirFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.SeaFreight, DocRollupOrSortTransportModeList.Descriptions.SeaFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RoadFreight, DocRollupOrSortTransportModeList.Descriptions.RoadFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RailFreight, DocRollupOrSortTransportModeList.Descriptions.RailFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.BreakBulk, DocRollupOrSortTransportModeList.Descriptions.BreakBulk);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.Bulk, DocRollupOrSortTransportModeList.Descriptions.Bulk);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RollOnRollOff, DocRollupOrSortTransportModeList.Descriptions.RollOnRollOff);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.BuyersConsol, DocRollupOrSortTransportModeList.Descriptions.BuyersConsol);
						specificList.Sort();
						break;

					case DocRollupOrSortJobTypeList.Codes.LinerAndAgency:
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.Containerised, DocRollupOrSortTransportModeList.Descriptions.Containerised);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.NonContainerised, DocRollupOrSortTransportModeList.Descriptions.NonContainerised);
						break;

					case DocRollupOrSortJobTypeList.Codes.CFS:
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.AirFreight, DocRollupOrSortTransportModeList.Descriptions.AirFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.SeaFreight, DocRollupOrSortTransportModeList.Descriptions.SeaFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RoadFreight, DocRollupOrSortTransportModeList.Descriptions.RoadFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RailFreight, DocRollupOrSortTransportModeList.Descriptions.RailFreight);
						break;

					case DocRollupOrSortJobTypeList.Codes.Warehouse:
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.AirFreight, DocRollupOrSortTransportModeList.Descriptions.AirFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.SeaFreight, DocRollupOrSortTransportModeList.Descriptions.SeaFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RoadFreight, DocRollupOrSortTransportModeList.Descriptions.RoadFreight);
						break;

					case DocRollupOrSortJobTypeList.Codes.Transport:
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.AirFreight, DocRollupOrSortTransportModeList.Descriptions.AirFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RoadFreight, DocRollupOrSortTransportModeList.Descriptions.RoadFreight);
						specificList.AddPair(DocRollupOrSortTransportModeList.Codes.RailFreight, DocRollupOrSortTransportModeList.Descriptions.RailFreight);
						break;

					default:
						break;
				}
				specificList.Sort();
				result.AddRange(specificList);

				return result;
			}
		}

		public CodeDescriptionPairList DisplayList => parent.IsRegistry
			? DisplaysExcludingDefault
			: new DocRollupOrSortDisplayList();

		static CodeDescriptionPairList DisplaysExcludingDefault
		{
			get
			{
				var list = new DocRollupOrSortDisplayList();
				list.RemoveCode(DocRollupOrSortDisplayList.Codes.Default);
				return list;
			}
		}

		public CodeDescriptionPairList StyleList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				switch (parent.Display)
				{
					case DocRollupOrSortDisplayList.Codes.Alphabetical:
					case DocRollupOrSortDisplayList.Codes.Sequence:
						list.AddPair(DocRollupOrSortStyleList.Codes.NoGrouping, DocRollupOrSortStyleList.Descriptions.NoGrouping);
						break;

					case DocRollupOrSortDisplayList.Codes.Default:
					case DocRollupOrSortDisplayList.Codes.RollUpCharges:
					case DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence:
					case DocRollupOrSortDisplayList.Codes.SubTotalCharges:
					case DocRollupOrSortDisplayList.Codes.SubTotalChargesAndSequence:
						if (!parent.IsRegistry)
						{
							list.AddPair(DocRollupOrSortDisplayList.Codes.Default, DocRollupOrSortDisplayList.Descriptions.Default);
						}
						list.AddRange(StylesExcludingDefault);
						break;
					default:
						break;
				}
				return list;
			}
		}

		CodeDescriptionPairList StylesExcludingDefault
		{
			get
			{
				var list = new DocRollupOrSortStyleList();
				list.RemoveCode(DocRollupOrSortStyleList.Codes.Default);
				return list;
			}
		}

		#endregion

		public void CheckDuplicatedLines(ZPropertyInfo propertyInfo)
		{
			if (parent.ParentCollection != null && !parent.Module.IsEmpty && !parent.JobType.IsEmpty && !parent.TransportMode.IsEmpty)
			{
				foreach (IRatingDocRollupOrSort item in parent.ParentCollection)
				{
					if (parent != item && parent.Module == item.Module && parent.JobType == item.JobType && parent.TransportMode == item.TransportMode)
					{
						if (parent.IsRegistry)
						{
							propertyInfo.AddError(Res.GetString("3f7a5a13-9655-4fad-b4f6-4d182e010632", "Can NOT have more than one Line with the same Module, Job Type, and Mode."));
						}
						else
						{
							propertyInfo.AddError(Res.GetString("25f0fbe8-81e0-4306-9ea1-09c6b9be9313", "Duplicate configurations with the same Module, Job Type and Mode is NOT allowed."));
						}
						break;
					}
				}
			}
		}
	}
}
