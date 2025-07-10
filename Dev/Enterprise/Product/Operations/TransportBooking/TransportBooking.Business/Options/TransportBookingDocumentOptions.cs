using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.Business.Options
{
	public partial class TransportBookingDocumentOptions : AutoTransportBookingDocumentOptions
	{
		/// <summary>
		/// Create Transport Booking
		/// </summary>
		/// <param name="parentDataContext"></param>
		/// <param name="direction"></param>
		/// <param name="transportMode"></param>
		/// <param name="containerMode"></param>
		/// <param name="hasValidCFSAddress"></param>
		public TransportBookingDocumentOptions(IDtbBookingParent parent, BusinessObjectFactory factory, DataContextType parentDataContext, DtbBookingDirection direction, ZBool combineContainers, ZString transportMode, ZString containerMode, bool hasValidCFSAddress)
			: this(parent, factory, parentDataContext, direction, combineContainers, transportMode, containerMode, hasValidCFSAddress, template: string.Empty)
		{
		}

		public TransportBookingDocumentOptions(IDtbBookingParent parent, BusinessObjectFactory factory, DataContextType parentDataContext, DtbBookingDirection direction, ZBool combineContainers, ZString transportMode, ZString containerMode, bool hasValidCFSAddress, ZString template) : base(factory)
		{
			using (SuspendSettingHasChanges())
			{
				this.Parent = parent;
				this.Direction = direction;
				this.ParentDataContext = parentDataContext;
				this.ContainerMode = containerMode;
				this.ShowTemplateSelection = true;
				this.ShowAutoDelivery = true;
				this.CombineContainers = combineContainers;

				var parentType = ParentTypes.GetParentType(parentDataContext);
				var directionCode = Directions.GetDirectionCodeFromDtbBookingDirection(Direction);
				var containerModeForRegistry = IsFCL ? Constants.ContainerModes.Containerised : Constants.ContainerModes.Loose;
				Template = string.IsNullOrEmpty(template) ? TransportRegistry.Instance.JobTemplateDefault.Value.GetBookingTemplate(parentType.Code, directionCode, transportMode, containerModeForRegistry, hasValidCFSAddress) : template;
			}
			SetLastDocumentOptionsForTest();
		}

		/// <summary>
		/// View Transport Booking
		/// </summary>
		/// <param name="direction"></param>
		public TransportBookingDocumentOptions(BusinessObjectFactory factory, DtbBookingDirection direction)
			: base(factory)
		{
			using (SuspendSettingHasChanges())
			{
				this.Direction = direction;
				this.ShowTemplateSelection = false;
				this.ShowAutoDelivery = true;
			}
			SetLastDocumentOptionsForTest();
		}

		public readonly IDtbBookingParent Parent;
		public readonly DtbBookingDirection Direction;
		public readonly DataContextType ParentDataContext;
		public readonly ZBool CombineContainers;
		readonly ZString ContainerMode;

		public DtbDocumentContainerOptionCollection Containers
		{
			get { return containers ?? (containers = new DtbDocumentContainerOptionCollection()); }
		}

		DtbDocumentContainerOptionCollection containers;

		public IEnumerable<DtbDocumentContainerOption> ContainersToDeliver
		{
			get { return Containers.Cast<DtbDocumentContainerOption>().Where(c => c.DeliverContainer); }
		}

		[List("Lookups.BookingTemplates")]
		public override ZString Template
		{
			get { return base.Template; }
			set { base.Template = value; }
		}

		public ZBool IsFCL
		{
			get { return ContainerMode == Constants.ContainerModes.FCL || (ContainerMode == Constants.ContainerModes.BuyersConsol && Direction == DtbBookingDirection.DLV); }
		}

		public enum DeliveryOption
		{
			None,

			AutoDelivery,

			OpenStandardDesigner,
			OpenInstructionDesigner
		}

		public DeliveryOption DeliveryOptions
		{
			get { return deliveryOption; }
			set { deliveryOption = value; }
		}

		DeliveryOption deliveryOption = DeliveryOption.None;

		public ResourceStringData MessageLabelText
		{
			get
			{
				return ShowCommencedError ?
					Res.GetData("bc10f54b-daad-47d6-a3d1-49d6977bca02", @"The Transport company has commenced work on transport related to this job.  If any changes have been made to the job it is necessary to advise the transport company manually.
Click ‘Deliver’ again to deliver the cartage advice with original details or select Open Transport Booking to action further.") :
					Res.GetData("02e2f7c0-29e2-4831-bdad-189609a187d9", @"Changes may have been made on the job that affect the Cartage Advice.
				These changes will not be reflected on the Cartage Advice as the booking has been overridden and is now managed independently of the job.");
			}
		}

		public TransportBookingDocumentOptionsLookups Lookups
		{
			get { return lookups ?? (lookups = new TransportBookingDocumentOptionsLookups(this)); }
		}

		TransportBookingDocumentOptionsLookups lookups;

		partial void SetLastDocumentOptionsForTest();
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.Business.Options
{
	using Enterprise.TransportBookings.Shared.Testing;

	public partial class TransportBookingDocumentOptions : ITransportBookingDocumentOptions
	{
		partial void SetLastDocumentOptionsForTest()
		{
			if (lastDocumentOptionsForTestKey != null)
			{
				lastDocumentOptionsForTest = this;
			}
		}

		public static IDisposable LastDocumentOptionsForTestStartRecording()
		{
			return lastDocumentOptionsForTestKey = new DisposableAction(delegate
				{
					lastDocumentOptionsForTestKey = null;
					lastDocumentOptionsForTest = null;
				});
		}

		[ThreadStatic]
		static IDisposable lastDocumentOptionsForTestKey;

		[ThreadStatic]
		static TransportBookingDocumentOptions lastDocumentOptionsForTest;
		public static TransportBookingDocumentOptions LastDocumentOptionsForTest { get { return lastDocumentOptionsForTest; } }

		List<IDtbDocumentContainerOption> ITransportBookingDocumentOptions.Containers
		{
			get { return this.Containers.Cast<IDtbDocumentContainerOption>().ToList(); }
		}
	}

	public class TransportBookingDocumentOptionsProvider : ITransportBookingDocumentOptionsProvider
	{
		public IDisposable LastDocumentOptionsForTestStartRecording()
		{
			return TransportBookingDocumentOptions.LastDocumentOptionsForTestStartRecording();
		}

		public ITransportBookingDocumentOptions LastDocumentOptionsForTest()
		{
			return TransportBookingDocumentOptions.LastDocumentOptionsForTest;
		}
	}
}

#endif
