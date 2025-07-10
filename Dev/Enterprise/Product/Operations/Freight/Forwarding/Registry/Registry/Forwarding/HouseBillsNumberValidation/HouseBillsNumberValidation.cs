using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class HouseBillsNumberValidation : AutoHouseBillsNumberValidation
	{
		public HouseBillsNumberValidation() { }

		public HouseBillsNumberValidation(BusinessObjectFactory factory)
			: base(factory) { }

		#region Properties

		[List("TransportModeList")]
		public override ZString TransportMode
		{
			get { return base.TransportMode; }
			set { base.TransportMode = value; }
		}

		[List("Locations")]
		public override ZString Origin
		{
			get { return base.Origin; }
			set { base.Origin = value; }
		}

		[List("Locations")]
		public override ZString Destination
		{
			get { return base.Destination; }
			set { base.Destination = value; }
		}

		[List("CheckDigitAlgorithmList")]
		public override ZString CheckDigitAlgorithm
		{
			get { return base.CheckDigitAlgorithm; }
			set { base.CheckDigitAlgorithm = value; }
		}

		public override ZString HBLPrefix
		{
			get { return base.HBLPrefix.ToUpper(); }
			set { base.HBLPrefix = value.ToUpper(); }
		}

		public override ZString HBLSuffix
		{
			get { return base.HBLSuffix.ToUpper(); }
			set { base.HBLSuffix = value.ToUpper(); }
		}

		[BusinessObjectTestExclude] // This is because basher test expects to see List attribute here
		public override ZString UserDefinedCondition
		{
			get { return base.UserDefinedCondition; }
			set { base.UserDefinedCondition = value; }
		}

		public override ZString HBLLengthFormatted
		{
			get { return base.HBLLengthFormatted; }
			set
			{
				if (!value.IsEmpty && value.IsNumbersOnlyOrEmpty && Convert.ToInt32(value) == 0)
				{
					base.HBLLengthFormatted = ZString.Empty;
				}
				else
				{
					base.HBLLengthFormatted = value;
				}
			}
		}

		public ZInt HBLLength
		{
			get { return (!HBLLengthFormatted.IsEmpty && HBLLengthFormatted.IsNumbersOnlyOrEmpty) ? Convert.ToInt32(HBLLengthFormatted) : 0; }
		}

		#endregion

		#region Validation

		public override void ValidateTransportMode()
		{
			base.ValidateTransportMode();
			MandatoryValidation.CheckEntered(TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(TransportModeInfo);
			ValidateDuplicateEntries();
		}

		public override void ValidateOrigin()
		{
			base.ValidateOrigin();
			ListValidation.ErrorIfInvalidCode(OriginInfo);
			ValidateDuplicateEntries();
		}

		public override void ValidateDestination()
		{
			base.ValidateDestination();
			ListValidation.ErrorIfInvalidCode(DestinationInfo);
			ValidateDuplicateEntries();
		}

		public override void ValidateHBLPrefix()
		{
			base.ValidateHBLPrefix();
			ValidateDuplicateEntries();
		}

		public override void ValidateHBLSuffix()
		{
			base.ValidateHBLSuffix();

			if (!HBLSuffix.IsEmpty && "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(HBLSuffix.Left(1)))
			{
				HBLSuffixInfo.AddError(Res.GetString("a5475aa2-abdb-40cc-a538-2ea1b20918d4", "The HBL Suffix must start with a symbol."));
			}

			ValidateDuplicateEntries();
		}

		public override void ValidateHBLLengthFormatted()
		{
			base.ValidateHBLLengthFormatted();

			if (!HBLLengthFormatted.IsEmpty && !HBLLengthFormatted.IsNumbersOnlyOrEmpty)
			{
				HBLLengthFormattedInfo.AddError(Res.GetString("1f933d1c-f4d0-4c19-85c1-82184625e2c8", "HBL Length must be numeric."));
			}
			else if (HBLLength > JobShipmentSchema.JS_HouseBill.MaxLength)
			{
				HBLLengthFormattedInfo.AddError(Res.GetString("482526c2-9ffe-43dd-8cb6-31ef41d130fd", "The maximum length for a House Bill is {0} characters.", JobShipmentSchema.JS_HouseBill.MaxLength));
			}
		}

		public override void ValidateUserDefinedCondition()
		{
			base.ValidateUserDefinedCondition();
			ValidateDuplicateEntries();
		}

		public override void ValidateCheckDigitAlgorithm()
		{
			base.ValidateCheckDigitAlgorithm();
			MandatoryValidation.CheckEntered(CheckDigitAlgorithmInfo);
			ListValidation.ErrorIfInvalidCode(CheckDigitAlgorithmInfo);
		}

		void ValidateDuplicateEntries()
		{
			ClearRowNotifications();

			if (ParentCollection != null && ParentCollection.Cast<HouseBillsNumberValidation>().Count(x => x.TransportMode == TransportMode
						&& x.Origin == Origin
						&& x.Destination == Destination
						&& x.HBLPrefix == HBLPrefix
						&& x.HBLSuffix == HBLSuffix
						&& x.UserDefinedCondition == UserDefinedCondition) > 1)
			{
				AddRowError(Res.GetString("2838bd03-a574-4946-8772-043c36218902", "Duplicate entries are not allowed. Each entry must be unique across {0}, {1}, {2}, {3}, {4} and {5}.",
						TransportModeInfo.HumanReadableName, OriginInfo.HumanReadableName, DestinationInfo.HumanReadableName, HBLPrefixInfo.HumanReadableName, HBLSuffixInfo.HumanReadableName, UserDefinedConditionInfo.HumanReadableName));
			}
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList CheckDigitAlgorithmList
		{
			get
			{
				if (checkDigitAlgorithmList == null)
				{
					checkDigitAlgorithmList = new CheckDigitAlgorithmList();
				}

				return checkDigitAlgorithmList;
			}
		}
		CodeDescriptionPairList checkDigitAlgorithmList;

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
					transportModeList.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
					transportModeList.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
					transportModeList.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
					transportModeList.AddPair(Constants.TransportModes.All, Constants.TransportModeDescriptions.All);
				}

				return transportModeList;
			}
		}
		CodeDescriptionPairList transportModeList;

		public IBusinessObjectCollection Locations
		{
			get
			{
				if (locations == null)
				{
					locations = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.ILocationCollection>(), new object[] { CurrentFactory });
				}

				return locations;
			}
		}
		IBusinessObjectCollection locations;

		#endregion

		#region Implementation

		HouseBillsNumberValidationCollection ParentCollection
		{
			get { return (HouseBillsNumberValidationCollection)GetParentCollection(this, typeof(HouseBillsNumberValidationCollection)); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HouseBillsNumberValidation(factory);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("cb5242ce-1de3-4172-bc62-a9147ad472a9", "House Bills Number Validation"); }
		}

		#endregion
	}
}
