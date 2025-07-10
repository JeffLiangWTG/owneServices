//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobServiceLookups
//
//    This class should be used for overriding collections in AutoJobServiceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceLookups : AutoJobServiceLookups
	{
		public JobServiceLookups(AutoJobService parent)
			: base(parent)
		{
		}

		new JobService Parent
		{
			get { return (JobService)base.Parent; }
		}

		#region ServiceContractor_List

		public OrgHeaderCollection ServiceContractor_List
		{
			get
			{
				switch (Parent.ES_ServiceCode)
				{
					case Core.Constants.FreightServiceType.Codes.Fumigation:
						return FumigationContractor_List;
					default:
						return OrgHeader_List;
				}
			}
		}

		#endregion

		#region OrgHeader_List

		public OrgHeaderCollection OrgHeader_List
		{
			get { return orgHeader_List ?? (orgHeader_List = new OrgHeaderCollection(Factory)); }
		}

		OrgHeaderCollection orgHeader_List;

		#endregion

		#region ServiceProvider

		public OrgHeaderCollection ServiceProvider
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region FumigationContractor_List

		public FumigationContractorCollection FumigationContractor_List
		{
			get { return fumigationContractor_List ?? (fumigationContractor_List = new FumigationContractorCollection(Factory)); }
		}

		FumigationContractorCollection fumigationContractor_List;

		#endregion

		#region LocationAddress_List

		public CodeDescriptionPairList LocationAddress_List
		{
			get
			{
				string key = Parent?.ServiceProviderPK.ToStringKey() ?? "EmptyList";

				return Factory.GetCachedValue("Enterprise.MasterFiles.Business.JobServiceLookups.LocationAddress_List_" + key, () =>
					{
						var result = new CodeDescriptionPairList();

						if (Parent != null && Parent.ServiceProvider != null)
						{
							foreach (OrgAddress address in Parent.ServiceProvider.Addresses)
							{
								result.AddPair(address.PK, address.OA_Code, address.OA_Address1);
							}
						}

						return result;
					});
			}
		}

		#endregion

		#region JobServiceType_List

		public CodeDescriptionPairList JobServiceType_List
		{
			get { return Factory.GetCachedValue("Enterprise.MasterFiles.Business.JobServiceLookups.JobServiceType_List", () => GetNewJobServiceType_List()); }
		}

		protected virtual CodeDescriptionPairList GetNewJobServiceType_List()
		{
			var result = new FreightServiceTypes();
			foreach (ICodeDescription codeDesc in CountrySpecificJobServiceTypeList)
			{
				result.AddPairIfNotExist(codeDesc.Code, codeDesc.Description);
			}
			return result;
		}

		public ICodeDescriptionPairList CountrySpecificJobServiceTypeList
		{
			get
			{
				var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue("Enterprise.MasterFiles.Business.JobServiceLookups.JobServiceTypeList_" + countryCode, () =>
				{
					return Parent.GetJobServiceTypeProvider(countryCode)?.GetJobServiceTypes() ?? new CodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region ServiceContext_List

		public CodeDescriptionPairList ServiceContext_List
		{
			get { return (Parent?.Parent as IHaveServicesWithContext)?.ServiceCurrentContextList; }
		}

		#endregion

		#region Measurement Basis List

		public CodeDescriptionPairList MeasurementBasisList
		{
			get { return Factory.GetCachedValue("JobServiceLookups.MeasurementsBasisList", GetMeasurementBasisList); }
		}

		CodeDescriptionPairList GetMeasurementBasisList()
		{
			var result = new CodeDescriptionPairList
			{
				new CodeDescriptionPair(JobServiceInfo.Constants.Codes.ServiceOccurrence, Res.GetString("6d4fde70-23c5-46a9-91a2-624e12560f3c", "Service Count")),
				new CodeDescriptionPair(JobServiceInfo.Constants.Codes.Hour, Res.GetString("a01fb22f-9884-4aa2-b8a2-49929d9a5178", "Service Duration")),
				new CodeDescriptionPair(JobServiceInfo.Constants.Codes.FlatRate, Res.GetString("b6fb3b88-6d85-4c8f-9520-1f48dcc60368", "Flat Rate")),
			};

			switch (Parent.ES_ParentTableCode)
			{
				case JobDocsAndCartageSchema.Constants.Prefix:
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.Chargeable, Res.GetString("3ef1590b-fc77-4999-bf8e-d101e0aa4f9f", "Chargeable")));
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.Container, Res.GetString("7a5d9147-64f9-49cb-8b6e-b645aa29d627", "Container Count")));
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.PickUpDistance, Res.GetString("90d111ff-dc07-4cf4-93ae-1ba6e5503eb6", "Pick-up Distance")));
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.DeliveryDistance, Res.GetString("4795b605-bcf3-4eb9-9938-26fee62352a7", "Delivery Distance")));
					break;

				case WhsDocketSchema.Constants.Prefix:
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.Chargeable, Res.GetString("3ef1590b-fc77-4999-bf8e-d101e0aa4f9f", "Chargeable")));
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.Container, Res.GetString("7a5d9147-64f9-49cb-8b6e-b645aa29d627", "Container Count")));
					break;

				case JobContainerSchema.Constants.Prefix:
					result.Add(new CodeDescriptionPair(JobServiceInfo.Constants.Codes.Container, Res.GetString("7a5d9147-64f9-49cb-8b6e-b645aa29d627", "Container Count")));
					break;
			}

			return result;
		}

		#endregion
	}
}
