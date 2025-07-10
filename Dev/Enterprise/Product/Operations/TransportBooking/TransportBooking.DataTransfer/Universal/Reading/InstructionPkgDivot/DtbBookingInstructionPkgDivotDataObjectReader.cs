using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	abstract class DtbBookingInstructionPkgDivotDataObjectReader<T> : DataObjectReader<T, DtbBookingInstructionPkgDivot>
		where T : IConfirmationParentDivot
	{
		protected DtbBookingInstructionPkgDivotDataObjectReader(T confirmationParentDivotDataObject, IXmlImportLogger logger,
			UniversalObjectFactory factory, Dictionary<ZInt, PkgPackage> packageLinks, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
			: base(confirmationParentDivotDataObject, logger, factory)
		{
			PackageLinks = Argument.NotNull(packageLinks, "Dictionary<ZInt, PkgPackage> packageLinks");
			ConfirmationDataObjectsToExclude = confirmationDataObjectsToExclude;
		}

		readonly Dictionary<ZInt, PkgPackage> PackageLinks;
		readonly IEnumerable<Confirmation> ConfirmationDataObjectsToExclude;

		protected override DtbBookingInstructionPkgDivot GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(DtbBookingInstructionPkgDivot instructionPkgDivot)
		{
			PopulateData(instructionPkgDivot);
			PopulateRelatedEntities(instructionPkgDivot);
		}

		void PopulateData(DtbBookingInstructionPkgDivot instructionPkgDivot)
		{
			var packageLink = GetPackageLinkFromDataObject();

			PkgPackage package;
			if (PackageLinks.TryGetValue(packageLink, out package))
			{
				SetValue(instructionPkgDivot, DtbBookingInstructionPkgDivotSchema.KD_KP_Package, package.PK);
				SetValue(instructionPkgDivot, DtbBookingInstructionPkgDivotSchema.KD_Quantity, dataObject.Quantity);
			}
			else
			{
				throw new InvalidOperationException("Should never try to read in a Divot with no Package Link.");
			}
		}

		protected abstract ZInt GetPackageLinkFromDataObject();

		void PopulateRelatedEntities(DtbBookingInstructionPkgDivot instructionPkgDivot)
		{
			if (dataObject.ConfirmationCollection != null)
			{
				var confirmationCollectionReader = new ConfirmationDataObjectCollectionReader(this, instructionPkgDivot,
					dataObject.ConfirmationCollection.ToArray(), ConfirmationDataObjectsToExclude);
				confirmationCollectionReader.ReadIntoCollection();
			}
		}

		class ConfirmationDataObjectCollectionReader : DataObjectCollectionReader<Confirmation, DtbBookingConfirmation>
		{
			public ConfirmationDataObjectCollectionReader(DtbBookingInstructionPkgDivotDataObjectReader<T> reader,
				DtbBookingInstructionPkgDivot divot, Confirmation[] confirmationDataObjects, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
				: base(confirmationDataObjects)
			{
				Reader = reader;
				Divot = divot;
				ConfirmationDataObjectsToExclude = confirmationDataObjectsToExclude;
			}
			readonly DtbBookingInstructionPkgDivotDataObjectReader<T> Reader;
			readonly DtbBookingInstructionPkgDivot Divot;
			readonly IEnumerable<Confirmation> ConfirmationDataObjectsToExclude;

			protected override void AddToCollection(DtbBookingConfirmation confirmation)
			{
				Divot.ConfirmationsDivotOnly.Add(confirmation);
			}

			protected override DtbBookingConfirmation[] BusinessObjects
			{
				get { return Divot.ConfirmationsDivotOnly.ToArray(); }
			}

			protected override DtbBookingConfirmation FindMatchingBusinessObject(Confirmation dataObject)
			{
				return null;
			}

			protected override DtbBookingConfirmation ReadIntoBusinessObject(Confirmation confirmationDataObject, DtbBookingConfirmation confirmation)
			{
				return ConfirmationDataObjectsToExclude == null || !ConfirmationDataObjectsToExclude.Contains(confirmationDataObject, new ConfirmationComparer())
					? new DtbBookingConfirmationDataObjectReader(confirmationDataObject, Reader.logger, Reader.factory, GetColumnIndexerFromRow(Divot)).ReadIntoBusinessObject()
					: null;
			}

			protected override void RemoveFromCollection(DtbBookingConfirmation confirmation)
			{
				Divot.ConfirmationsDivotOnly.Delete(confirmation);
			}
		}
	}
}
