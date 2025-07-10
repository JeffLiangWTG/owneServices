using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class DateAndReferenceCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Overrides

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DateAndReferenceCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DateAndReference();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var dateAndReference = (DateAndReference)child;

			using (dateAndReference.GetValidationSuspender())
			{
				dateAndReference.BookingDirection = DatesAndReference.Any;
				dateAndReference.InstructionType = DatesAndReference.Any;
				dateAndReference.OrganisationType = DatesAndReference.Any;
				dateAndReference.ContainerMode = DatesAndReference.Any;
			}
		}

		#endregion

		#region Index

		public new DateAndReference this[int i]
		{
			get { return (DateAndReference)Elements[i]; }
		}

		public new DateAndReference AddNew()
		{
			return (DateAndReference)base.AddNew();
		}

		#endregion

		#region Find Confirmations

		#region FindConfirmation

		public DateAndReference FindConfirmation(ZString code, ZString bookingDirection, ZString instructionType, ZString organisationType, ZString containerMode)
		{
			var highestScore = 0;
			DateAndReference result = null;

			foreach (var dateAndReference in this.Cast<DateAndReference>().Where(d => d.Code == code))
			{
				var isBookingDirection = dateAndReference.BookingDirection == bookingDirection;
				var isInstructionType = dateAndReference.InstructionType == instructionType;
				var isOrganisationType = dateAndReference.OrganisationType == organisationType;
				var isContainerMode = dateAndReference.ContainerMode == containerMode;

				if (IsMatchingDateAndReference(dateAndReference, isBookingDirection, isInstructionType, isOrganisationType, isContainerMode))
				{
					var score = isBookingDirection ? 2000 : 1000;
					score += isInstructionType ? 200 : 100;
					score += isOrganisationType ? 20 : 10;
					score += isContainerMode ? 2 : 1;

					if (score > highestScore)
					{
						highestScore = score;
						result = dateAndReference;
					}
				}
			}

			return result;
		}

		#endregion

		#region FindDefaultConfirmations

		public IEnumerable<DateAndReference> FindDefaultConfirmations(ZString bookingDirection, ZString instructionType, ZString organisationType, ZString containerMode)
		{
			var datesAndReferences = new List<DateAndReference>();

			foreach (var dateAndReference in this.Cast<DateAndReference>().Where(d => d.AutoAdd))
			{
				var isBookingDirection = dateAndReference.BookingDirection == bookingDirection;
				var isInstructionType = dateAndReference.InstructionType == instructionType;
				var isOrganisationType = dateAndReference.OrganisationType == organisationType;
				var isContainerMode = containerMode.IsEmpty || containerMode == PackageCategories.Codes.Both || dateAndReference.ContainerMode == containerMode;

				if (IsMatchingDateAndReference(dateAndReference, isBookingDirection, isInstructionType, isOrganisationType, isContainerMode))
				{
					if (!datesAndReferences.Any(d => d.Code == dateAndReference.Code))
					{
						datesAndReferences.Add(dateAndReference);
					}
				}
			}

			return datesAndReferences;
		}

		bool IsMatchingDateAndReference(DateAndReference dateAndReference, ZBool isBookingDirection, ZBool isInstructionType, ZBool isOrganisationType, ZBool isContainerMode)
		{
			return (dateAndReference.BookingDirection == DatesAndReference.Any || isBookingDirection) &&
				   (dateAndReference.InstructionType == DatesAndReference.Any || isInstructionType) &&
				(dateAndReference.OrganisationType == DatesAndReference.Any || isOrganisationType) &&
					(dateAndReference.ContainerMode == DatesAndReference.Any || isContainerMode);
		}

		#endregion

		#endregion

		#region GetDateAndReferenceByOrganisationType

		public DateAndReference[] GetDateAndReferenceByOrganisationType(ZString organisationType)
		{
			var result = new Dictionary<ZString, DateAndReference>();

			foreach (DateAndReference dateAndReference in this)
			{
				if (dateAndReference.OrganisationType == organisationType && !result.ContainsKey(dateAndReference.Description))
				{
					result.Add(dateAndReference.Description, dateAndReference);
				}
			}

			foreach (DateAndReference dateAndReference in this)
			{
				if (dateAndReference.OrganisationType == DatesAndReference.Any && !result.ContainsKey(dateAndReference.Description))
				{
					result.Add(dateAndReference.Description, dateAndReference);
				}
			}

			return result.Values.ToArray();
		}

		#endregion

		#region Defaults

		public static DateAndReferenceCollection GetDefault()
		{
			var result = new DateAndReferenceCollection();

			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CTO", DatesAndReference.Any, true, true, true, true, true, RequiredConstants.FCLAvailable, true, RequiredConstants.FCLStorage, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CFS", Constants.ContainerModes.Loose, true, true, true, false, true, RequiredConstants.LCLAvailable, true, RequiredConstants.LCLStorage, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, DatesAndReference.Any, DatesAndReference.Any, true, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, Constants.CartageDirection.Destination, InstructionTypes.Codes.Multi, "CFS", "LSE", false, true, true, false, true, RequiredConstants.LCLAvailable, true, RequiredConstants.LCLStorage, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, false, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);

			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CTO", DatesAndReference.Any, true, true, true, true, true, RequiredConstants.FCLReceive, true, RequiredConstants.FCLCutOff, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CFS", Constants.ContainerModes.Loose, true, true, true, false, true, RequiredConstants.LCLReceive, true, RequiredConstants.LCLCutOff, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CYD", "CNT", true, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.EmptyReturnBy, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, DatesAndReference.Any, DatesAndReference.Any, true, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, Constants.CartageDirection.Origin, InstructionTypes.Codes.Multi, "CFS", Constants.ContainerModes.Loose, false, true, true, false, true, RequiredConstants.LCLReceive, true, RequiredConstants.LCLCutOff, true, true);
			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, false, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);

			SetupDefaultFields(result.AddNew(), ConfirmationTypes.Codes.ConNoteNo, ConfirmationTypes.Descriptions.ConnoteNo, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, false, false, false, false, false, (NoResString)"", false, (NoResString)"", true, false);

			return result;
		}

		static void SetupDefaultFields(DateAndReference dateAndReference,
			ZString code,
			MultilingualString description,
			ZString bookingDirection,
			ZString instructionType,
			ZString organisationType,
			ZString containerMode,
			ZBool autoAdd,
			ZBool allowActualDate,
			ZBool allowEstimatedDate,
			ZBool allowSlotDate,
			ZBool allowRequiredFromDate,
			MultilingualString allowRequiredFromLabel,
			ZBool allowRequiredToDate,
			MultilingualString allowRequiredToLabel,
			ZBool allowReference,
			ZBool allowReceivedBy)
		{
			using (dateAndReference.GetValidationSuspender())
			{
				dateAndReference.Code = code;
				dateAndReference.Description = description;
				dateAndReference.BookingDirection = bookingDirection;
				dateAndReference.InstructionType = instructionType;
				dateAndReference.OrganisationType = organisationType;
				dateAndReference.ContainerMode = containerMode;
				dateAndReference.AllowActualDate = allowActualDate;
				dateAndReference.AllowEstimatedDate = allowEstimatedDate;
				dateAndReference.AllowSlotDate = allowSlotDate;
				dateAndReference.AllowRequiredFromDate = allowRequiredFromDate;
				dateAndReference.AllowRequiredFromLabel = allowRequiredFromLabel;
				dateAndReference.AllowRequiredToDate = allowRequiredToDate;
				dateAndReference.AllowRequiredToLabel = allowRequiredToLabel;
				dateAndReference.AllowReference = allowReference;
				dateAndReference.AllowReceivedBy = allowReceivedBy;
				dateAndReference.AutoAdd = autoAdd;
				dateAndReference.IsSystemDefined = true;
			}
		}

		#endregion
	}
}
