using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class AutoLoggedTablesDefaultConfigValues
	{
		public static AutoLoggedTablesDefaultConfigValues Instance
		{
			get
			{
				var type = TypeDecider.GetTypeForBinding(typeof(AutoLoggedTablesDefaultConfigValues));
				return (AutoLoggedTablesDefaultConfigValues)Activator.CreateInstance(type);
			}
		}

		readonly IDictionary<string, (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)> configurationValues = new Dictionary<string, (bool, bool, bool)>()
		{
			[ProcessTasksSchema.Constants.TableName] = (false, false, false),
			[WorkItemSchema.Constants.TableName] = (false, false, false),
			[AccComplianceSequenceSchema.Constants.TableName] = (false, false, false),
			[WorkItemSchema.Constants.TableName] = (false, false, false),
			[HVLVConsignmentSchema.Constants.TableName] = (false, false, false),
			[HVLVBookingHeaderSchema.Constants.TableName] = (false, false, false),
			[HVLVOuterPackageSchema.Constants.TableName] = (false, false, false),
			[HVLVOriginLoadListSchema.Constants.TableName] = (false, false, false),
		};

		public virtual IDictionary<string, (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)> GetDefaultConfigurationValues()
		{
			return configurationValues;
		}
	}
}
