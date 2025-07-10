using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobTypeDirectionAndTransportInfoProvider
	{
		public JobTypeDirectionAndTransportInfoProvider(Func<ZPropertyInfo> jobTypePropertyInfoGetter, Func<ZPropertyInfo> directionPropertyInfoGetter, Func<ZPropertyInfo> transportModePropertyInfoGetter)
		{
			this.jobTypeGetter = jobTypePropertyInfoGetter;
			this.directionGetter = directionPropertyInfoGetter;
			this.transportModeGetter = transportModePropertyInfoGetter;
		}
		readonly Func<ZPropertyInfo> jobTypeGetter;
		readonly Func<ZPropertyInfo> directionGetter;
		readonly Func<ZPropertyInfo> transportModeGetter;

		#region LookUp

		#region JobType

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				if (jobTypeList == null)
				{
					jobTypeList = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
					jobTypeList.Insert(0, new AllJobsConsumerType());
				}
				return jobTypeList;
			}
		}
		CodeDescriptionPairList jobTypeList;

		public JobInvoicingConsumerType GetJobTypeDetail()
		{
			JobInvoicingConsumerType resultJobType = null;
			var jobTypePropInfo = jobTypeGetter();
			if (jobTypePropInfo != null)
			{
				var jobTypeCode = GetValue<ZString>(jobTypePropInfo);
				resultJobType = JobTypeList[jobTypeCode, StringComparison.CurrentCultureIgnoreCase] as JobInvoicingConsumerType;
			}
			return resultJobType;
		}

		#endregion

		#region TransportMode

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				var jobTypePropInfo = jobTypeGetter();
				var jobTypeCode = jobTypePropInfo != null ? GetValue<ZString>(jobTypePropInfo) : ZString.Empty;

				var result = jobTypeCode == JobInvoicingConsumerTypes.BrokerageCode
					? GetBrokerageTransportModeList()
					: new CodeDescriptionPairList(OLookUpEditType.TransportType);
				result.Insert(0, new CodeDescriptionPair(Constants.TransportModes.All, Constants.TransportModeDescriptions.All));

				return result;
			}
		}

		CodeDescriptionPairList GetBrokerageTransportModeList()
		{
			var result = new CodeDescriptionPairList();

			// The TransportTypeGenericList contains all codes used by Customs. See it here: https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FProduct%2FOperations%2FCustoms%2FShared%2FBusiness%2FBusiness%2FCodeDescriptionPairLists%2FTransportTypeGenericList.cs&_a=contents&version=GBmaster
			// But the ObjectFactory.GetType<IDeclarationTransportModeCodeDescriptionPairProvider>() used by Lookups gives a country specific subset.
			// That is why we have to explicitly create CodeDescriptionPairList with all the codes, except the empty one named as Unknown and Other.
			result.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
			result.AddPair(Constants.TransportModes.FixedTransportInstallations, Constants.TransportModeDescriptions.FixedTransportInstallations);
			result.AddPair(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportModeDescriptions.InlandWaterwayTransport);
			result.AddPair(Constants.TransportModes.OwnPropulsion, Constants.TransportModeDescriptions.OwnPropulsion);
			result.AddPair(Constants.TransportModes.Mail, Constants.TransportModeDescriptions.Mail);
			result.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
			result.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
			result.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);

			return result;
		}

		#endregion

		#region Direction
		public CodeDescriptionPairList DirectionList
		{
			get
			{
				if (directionList == null)
				{
					directionList = new CodeDescriptionPairList();
					directionList.AddPair(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All);
					directionList.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
					directionList.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
					directionList.AddPair(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic);
					directionList.AddPair(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other);
				}

				return directionList;
			}
		}
		CodeDescriptionPairList directionList;
		#endregion

		#region InvoiceTypeList
		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				var resultInvoiceTypes = new CodeDescriptionPairList();
				var jobType = GetJobTypeDetail();
				if (jobType != null)
				{
					resultInvoiceTypes.AddRange(jobType.InvoiceTypeList);
				}
				resultInvoiceTypes.RemoveCode(InvoiceTypesList.Codes.DoNotPost);
				return resultInvoiceTypes;
			}
		}

		#endregion

		#endregion

		#region Validation

		public void ValidateJobType(bool checkMandatory = true)
		{
			var jobTypePropertyInfo = jobTypeGetter();
			if (jobTypePropertyInfo != null)
			{
				if (checkMandatory)
				{
					MandatoryValidation.CheckEntered(jobTypePropertyInfo);
				}
				ListValidation.ErrorIfInvalidCode(jobTypePropertyInfo);
			}
		}

		public void ValidateDirection(bool checkMandatory = true)
		{
			var directionPropertyInfo = directionGetter();
			if (directionPropertyInfo != null)
			{
				var jobType = GetJobTypeDetail();
				if (checkMandatory && jobType != null && jobType.IsDirectionSupported)
				{
					MandatoryValidation.CheckEntered(directionPropertyInfo);
				}
				ListValidation.ErrorIfInvalidCode(directionPropertyInfo);
			}
		}

		public void ValidateTransportMode(bool checkMandatory = true)
		{
			var transportModePropertyInfo = transportModeGetter();
			if (transportModePropertyInfo != null)
			{
				var jobType = GetJobTypeDetail();
				if (checkMandatory && jobType != null && jobType.IsTransportModeSupported)
				{
					MandatoryValidation.CheckEntered(transportModePropertyInfo);
				}
				ListValidation.ErrorIfInvalidCode(transportModePropertyInfo);
			}
		}

		#endregion

		#region ReadOnlyHelper

		public bool IsDirectionReadonly
		{
			get
			{
				var jobType = GetJobTypeDetail();
				return jobType != null && !jobType.IsDirectionSupported;
			}
		}

		public bool IsTransportModeReadonly
		{
			get
			{
				var jobType = GetJobTypeDetail();
				return jobType != null && !jobType.IsTransportModeSupported;
			}
		}

		#endregion

		T GetValue<T>(ZPropertyInfo info)
				where T : IZType
		{
			return (T)info.Value;
		}

		public const string All = "ALL";
	}
}
