using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocDeliveryContactValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOrgAddressPK()
		{
			OrgHeader organisation = OrgHeader.New(Factory);
			organisation.OH_FullName = "Hello THere How are You";
			organisation.MainAddress.OA_Address1 = "18 Henricks Street";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address = organisation.Addresses.AddNew();
			address.OA_Address1 = "OA_Address1";
			Factory.Save();

			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
			AssertNoErrors(deliveryContact.OrgAddressPKInfo);

			deliveryContact.OrgAddressPK = ZGuid.Invalid;
			AssertHasErrors(deliveryContact.OrgAddressPKInfo);

			using (deliveryContact.SuspendValidationTesting())
			{
				deliveryContact.OrgAddressPKInfo.ClearAllNotifications();
			}
			AssertNoErrors(deliveryContact.OrgAddressPKInfo);
			deliveryContact.Validation.ValidateAll();
			AssertHasErrors(deliveryContact.OrgAddressPKInfo);

			deliveryContact.OrgAddressPK = address.PK;
			AssertNoErrors(deliveryContact.OrgAddressPKInfo);
		}

		public void TestValidateAll()
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.Validation.ValidateAll();
			AssertHasErrors(contact.DeliveryMethodInfo);
		}

		public void TestValidateDeliveryAddress()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact orgContact = organisation.Contacts.AddNew();
			orgContact.OC_ContactName = "goo";
			orgContact.OC_Email = "googoo@gaga.com";
			orgContact.OC_Fax = "+61290251155";

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);

			DocDeliveryContact contact = new DocDeliveryContact(Factory);

			contact.Validation.ValidateDeliveryAddress();
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			contact.Validation.ValidateDeliveryAddress();
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.DeliveryAddress = "hello";
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.Validation.ValidateDeliveryAddress();
			AssertHasError(contact.DeliveryAddressInfo, "Please enter an Email Address.");

			contact.DeliveryAddress = "zubs";
			AssertHasError(contact.DeliveryAddressInfo, @"The email address ""zubs"" is invalid.");

			contact.DeliveryAddress = "zubs@zubs.com";
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.DeliveryAddress = "zubs@zubs.com, what@ever.com";
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.OrgHeaderPK = organisation.PK;
			contact.Name = "goo";
			contact.Validation.ValidateDeliveryAddress();
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.DeliveryAddress = "zubs@zubs.com";
			AssertNoErrors(contact.DeliveryAddressInfo);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact.DeliveryAddress = "";
			contact.Validation.ValidateDeliveryAddress();
			AssertHasError(contact.DeliveryAddressInfo, "Please enter a Fax Number.");

			contact.DeliveryAddress = "@@@(*(*@$(*!_)(^";
			AssertHasErrors(contact.DeliveryAddressInfo);

			contact.DeliveryAddress = "+61290251199";
			AssertNoErrors(contact.DeliveryAddressInfo);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);

			contact.DeliveryAddress = "@@@(*(*@$(*!_)(^";
			AssertHasErrors(contact.DeliveryAddressInfo);

			contact.DeliveryAddress = "+61290251199";
			AssertNoErrors(contact.DeliveryAddressInfo);

			var ePrinterAddress = "email@printer.com";
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			contact.Validation.ValidateDeliveryAddress();
			AssertHasError(contact.DeliveryAddressInfo, "ePrint email address is not defined. This can be set in the registry setting Documents > ePrint Email Address.");

			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress);
			contact.Validation.ValidateDeliveryAddress();
			AssertNoErrors(contact.DeliveryAddressInfo);
		}

		[TestDate(2016, 2, 10)]
		public void TestValidateDeliveryAddressWithIsNDR()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact orgContact = organisation.Contacts.AddNew();
			orgContact.OC_ContactName = "goo";
			orgContact.OC_Email = "googoo@gaga.com";
			orgContact.OC_Fax = "+61290251155";

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.OrgHeaderPK = organisation.PK;
			contact.Name = "goo";

			contact.Validation.ValidateDeliveryAddress();
			AssertNoWarnings(contact.DeliveryAddressInfo);

			orgContact.IsNDR = true;

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.Validation.ValidateDeliveryAddress();
			AssertHasWarning(contact.DeliveryAddressInfo, @"The last email sent to ""googoo@gaga.com"" received a Non-Delivery Receipt (at 10-Feb-16 00:00:00).");

			orgContact.IsNDR = false;
			contact.Validation.ValidateDeliveryAddress();
			AssertNoWarnings(contact.DeliveryAddressInfo);
		}

		public void TestValidateEmailCarbonCopyRecipientsAsStringWithIsNDR()
		{
			var ndrEmail1 = Factory.New<GlbEmailAddress>();
			ndrEmail1.GI_EmailAddress = "1@ndr.com";
			ndrEmail1.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			ndrEmail1.GI_DeliveryReportTimeUtc = new ZDateTime(2016, 2, 10);

			var ndrEmail2 = Factory.New<GlbEmailAddress>();
			ndrEmail2.GI_EmailAddress = "2@ndr.com";
			ndrEmail2.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			ndrEmail2.GI_DeliveryReportTimeUtc = new ZDateTime(2016, 2, 1);

			var contact = new DocDeliveryContact(Factory);

			contact.EmailCarbonCopyRecipientsAsString = "andrew.luong@wisetechglobal.com";
			AssertNoWarnings(contact.EmailCarbonCopyRecipientsAsStringInfo);

			contact.EmailCarbonCopyRecipientsAsString = "1@ndr.com";
			AssertHasWarning(contact.EmailCarbonCopyRecipientsAsStringInfo, $@"The last email sent to ""1@ndr.com"" received a Non-Delivery Receipt (at {ndrEmail1.GI_DeliveryReportTimeUtc}).");

			contact.EmailCarbonCopyRecipientsAsString = "1@ndr.com, 2@ndr.com, andrew.luong@wisetechglobal.com";
			AssertHasWarning(contact.EmailCarbonCopyRecipientsAsStringInfo, $@"The last email sent to ""1@ndr.com"" received a Non-Delivery Receipt (at {ndrEmail1.GI_DeliveryReportTimeUtc}).");
			AssertHasWarning(contact.EmailCarbonCopyRecipientsAsStringInfo, $@"The last email sent to ""2@ndr.com"" received a Non-Delivery Receipt (at {ndrEmail2.GI_DeliveryReportTimeUtc}).");
		}

		public void TestValidateEmailBlindCarbonCopyRecipientsAsStringWithIsNDR()
		{
			var ndrEmail1 = Factory.New<GlbEmailAddress>();
			ndrEmail1.GI_EmailAddress = "1@ndr.com";
			ndrEmail1.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			ndrEmail1.GI_DeliveryReportTimeUtc = new ZDateTime(2016, 2, 10);

			var ndrEmail2 = Factory.New<GlbEmailAddress>();
			ndrEmail2.GI_EmailAddress = "2@ndr.com";
			ndrEmail2.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			ndrEmail2.GI_DeliveryReportTimeUtc = new ZDateTime(2016, 2, 1);

			var contact = new DocDeliveryContact(Factory);

			contact.EmailBlindCarbonCopyRecipientsAsString = "andrew.luong@wisetechglobal.com";
			AssertNoWarnings(contact.EmailBlindCarbonCopyRecipientsAsStringInfo);

			contact.EmailBlindCarbonCopyRecipientsAsString = "1@ndr.com";
			AssertHasWarning(contact.EmailBlindCarbonCopyRecipientsAsStringInfo, $@"The last email sent to ""1@ndr.com"" received a Non-Delivery Receipt (at {ndrEmail1.GI_DeliveryReportTimeUtc}).");

			contact.EmailBlindCarbonCopyRecipientsAsString = "1@ndr.com, 2@ndr.com, andrew.luong@wisetechglobal.com";
			AssertHasWarning(contact.EmailBlindCarbonCopyRecipientsAsStringInfo, $@"The last email sent to ""1@ndr.com"" received a Non-Delivery Receipt (at {ndrEmail1.GI_DeliveryReportTimeUtc}).");
			AssertHasWarning(contact.EmailBlindCarbonCopyRecipientsAsStringInfo, $@"The last email sent to ""2@ndr.com"" received a Non-Delivery Receipt (at {ndrEmail2.GI_DeliveryReportTimeUtc}).");
		}

		public void TestValidateDeliveryMethod()
		{
			var contact = new DocDeliveryContact(Factory);

			contact.Validation.ValidateDeliveryMethod();
			AssertHasError(contact.DeliveryMethodInfo, "Please enter a value.");

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertNoErrors(contact.DeliveryMethodInfo);

			contact.DeliveryMethod = "Zub";
			AssertHasError(contact.DeliveryMethodInfo, "Enter a valid selection.");

			AssertValidateDeliveryMethodAndDescription(contact, contact.DeliveryMethodInfo, 1);
		}

		public void TestValidateDeliveryMethodForEmailAttachment()
		{
			var contact = new DocDeliveryContact(Factory);
			var collection = new DocDeliveryContactCollection(Factory) { contact };

			var attachment1 = new MockDeliverable
			{
				FileName = "Attachment1.XML",
				FileSizeInBytes = 1 * 1024 * 1024,
				ShouldBeAttached = true
			};
			var attachment2 = new MockDeliverable
			{
				FileName = "Attachment2.XML",
				FileSizeInBytes = 2 * 1024 * 1024,
				ShouldBeAttached = false
			};
			var attachment3 = new MockDeliverable
			{
				FileName = "Attachment3.XML",
				FileSizeInBytes = 3 * 1024 * 1024,
				ShouldBeAttached = true
			};
			var attachment4 = new MockDeliverable
			{
				FileName = "Attachment4.XML",
				FileSizeInBytes = 4 * 1024 * 1024,
				ShouldBeAttached = false
			};
			var attachment5 = new MockDeliverable
			{
				FileName = "Attachment5.XML",
				FileSizeInBytes = 5 * 1024 * 1024,
				ShouldBeAttached = true
			};
			var attachment6 = new MockDeliverable
			{
				FileName = "Attachment6.XML",
				FileSizeInBytes = 1024 * 1024 * 1024,
				ShouldBeAttached = true
			};
			var attachment7 = new MockDeliverable
			{
				FileName = "Attachment7.XML",
				FileSizeInBytes = 1_000_000_000L * 1024 * 1024,
				ShouldBeAttached = true
			};

			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var deliverables1 = new MockDeliverableCollection { attachment1, attachment2 };
				collection.Deliverables = deliverables1;

				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				AssertNoErrors(contact.DeliveryMethodInfo);

				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				AssertNoErrors(contact.DeliveryMethodInfo);

				var deliverables2 = new MockDeliverableCollection { attachment1, attachment2, attachment3, attachment4, attachment5 };
				collection.Deliverables = deliverables2;

				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				AssertNoErrors(contact.DeliveryMethodInfo);

				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				AssertHasError(contact.DeliveryMethodInfo,
					$"One or more eDoc files exceeds the 2MB attachment limit and cannot be sent: Attachment3.XML, Attachment5.XML. The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.");

				var deliverables3 = new MockDeliverableCollection { attachment1, attachment4 };
				collection.Deliverables = deliverables3;
				contact.Validation.ValidateDeliveryMethod();
				AssertNoErrors(contact.DeliveryMethodInfo);
			}

			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				var deliverables1 = new MockDeliverableCollection { attachment6 };
				collection.Deliverables = deliverables1;
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				AssertNoErrors(contact.DeliveryMethodInfo);

				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				AssertNoErrors(contact.DeliveryMethodInfo);

				var deliverables2 = new MockDeliverableCollection { attachment7 };
				collection.Deliverables = deliverables2;
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				AssertHasError(contact.DeliveryMethodInfo,
					$"One or more eDoc files exceeds the 999999999MB attachment limit and cannot be sent: Attachment7.XML. The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.");

				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				AssertNoErrors(contact.DeliveryMethodInfo);
			}
		}

		class MockDeliverable : NonPersistentBusinessObject, IDocument, IDeliveryEmailAttachment
		{
			public MockDeliverable()
			{
				CanIncludeInPrint = true;
			}

			public string DocumentDeliveryMethod { get; set; }
			public string DocumentName { get; set; }
			public bool IncludeInPrint { get; set; }
			public bool CanIncludeInPrint { get; set; }

			public string FileName { get; set; }
			public long FileSizeInBytes { get; set; }
			public bool ShouldBeAttached { get; set; }

			public IEnumerable<string> GetSupportedDeliveryMethods()
			{
				var deliveryMode = DocumentDeliveryMethod?.ToUpper();

				if (deliveryMode == nameof(PrintCopyType.ALL))
				{
					return Core.Constants.ContactNotifyModes.All;
				}
				else if (deliveryMode == nameof(PrintCopyType.PRN))
				{
					return new string[] { Core.Constants.ContactNotifyModes.Print, Core.Constants.ContactNotifyModes.EPrint };
				}
				else if (deliveryMode == nameof(PrintCopyType.EML))
				{
					return new string[] { Core.Constants.ContactNotifyModes.Email };
				}
				else if (deliveryMode == nameof(PrintCopyType.FAX))
				{
					return new string[] { Core.Constants.ContactNotifyModes.Fax };
				}

				return Array.Empty<string>();
			}

			public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType()
			{
				return GetSupportedDeliveryMethods();
			}

			public bool SupportsDeliveryMethod(string deliveryMethod)
			{
				return GetSupportedDeliveryMethods().Contains(deliveryMethod?.ToUpper());
			}
		}

		class MockDeliverableCollection : NonPersistentBusinessObjectCollection<MockDeliverable>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new MockDeliverable();
			}
		}

		public void TestValidateDeliveryMethodDescription()
		{
			var document = new Mock<IStmMenuItem>();
			document.SetupGet(x => x.SU_BusinessContext).Returns("Shipment");

			var contact = new DocDeliveryContact(Factory);
			contact.Initialise(document.Object, null);

			contact.Validation.ValidateDeliveryMethodDescription();
			AssertHasError(contact.DeliveryMethodDescriptionInfo, "Please enter a value.");

			contact.DeliveryMethodDescription = "Fax";
			AssertNoErrors(contact.DeliveryMethodDescriptionInfo);

			contact.DeliveryMethodDescription = "Zub";
			AssertHasError(contact.DeliveryMethodDescriptionInfo, "Please enter a value.");

			AssertValidateDeliveryMethodAndDescription(contact, contact.DeliveryMethodDescriptionInfo, 2);
		}

		public void AssertValidateDeliveryMethodAndDescription(DocDeliveryContact contact, ZPropertyInfo info, int type)
		{
			var collection = new DocDeliveryContactCollection(Factory);
			collection.Add(contact);

			var document1 = new MockDeliverable
			{
				DocumentName = "File 1",
				DocumentDeliveryMethod = nameof(PrintCopyType.EML),
				IncludeInPrint = true,
			};
			var document2 = new MockDeliverable
			{
				DocumentName = "File 2",
				DocumentDeliveryMethod = nameof(PrintCopyType.EML),
				IncludeInPrint = false,
			};
			var document3 = new MockDeliverable
			{
				DocumentName = "File 3",
				DocumentDeliveryMethod = nameof(PrintCopyType.FAX),
				IncludeInPrint = false,
			};
			var document4 = new MockDeliverable
			{
				DocumentName = "File 4",
				DocumentDeliveryMethod = nameof(PrintCopyType.FAX),
				IncludeInPrint = false,
				CanIncludeInPrint = false,
			};
			var document5 = new MockDeliverable
			{
				DocumentName = "File 5",
				DocumentDeliveryMethod = nameof(PrintCopyType.PRN),
				IncludeInPrint = true,
			};
			var document6 = new MockDeliverable
			{
				DocumentName = "File 6",
				DocumentDeliveryMethod = nameof(PrintCopyType.PRN),
				IncludeInPrint = false,
			};

			var deliverables = new MockDeliverableCollection { document1, document2, document3, document4, document5, document6 };

			deliverables.Add(Factory.New<DummyBaseBusinessObject>());
			collection.Deliverables = deliverables;

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			contact.DeliveryMethodDescription = "Print";
			AssertHasWarning(info, @"'File 1' cannot be delivered because it has been restricted for 'E-Mail' delivery only.");

			document4.CanIncludeInPrint = true;
			document5.IncludeInPrint = false;
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.DeliveryMethodDescription = "E-Mail";
			AssertNoNotifications(info);

			document4.CanIncludeInPrint = false;
			document2.IncludeInPrint = document3.IncludeInPrint = true;
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			contact.DeliveryMethodDescription = "Print";
			AssertHasWarning(info, @"'File 1' cannot be delivered because it has been restricted for 'E-Mail' delivery only.
'File 2' cannot be delivered because it has been restricted for 'E-Mail' delivery only.
'File 3' cannot be delivered because it has been restricted for 'Fax' delivery only.");

			document1.IncludeInPrint = document2.IncludeInPrint = false;
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			contact.DeliveryMethodDescription = "Fax";
			AssertNoNotifications(info);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;
			contact.DeliveryMethodDescription = "eDoc";
			AssertNoNotifications(info);

			document1.IncludeInPrint = false;
			document2.IncludeInPrint = false;
			document3.IncludeInPrint = false;
			document4.IncludeInPrint = false;
			document4.CanIncludeInPrint = true;
			document5.IncludeInPrint = false;
			document6.IncludeInPrint = true;

			GlbStaff.CurrentUser.GS_EmailAddress = "";
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			contact.DeliveryMethodDescription = "ePrint";
			AssertHasError(info, @"ePrint can only be used when the currently logged in user has a valid email address. Please set one on your staff profile.");

			GlbStaff.CurrentUser.GS_EmailAddress = "kelvin@bigboss.com";
			if (type == 1)
			{
				contact.Validation.ValidateDeliveryMethod();
			}
			else
			{
				contact.Validation.ValidateDeliveryMethodDescription();
			}
			AssertNoNotifications(info);
		}

		public void TestEDocsNotCheckedToSendDontCauseWarnings()
		{
			var contact = new DocDeliveryContact(Factory);

			contact.Validation.ValidateDeliveryMethod();

			var collection = new DocDeliveryContactCollection(Factory);
			collection.Add(contact);

			var document1 = new MockDeliverable
			{
				DocumentName = "File 1",
				DocumentDeliveryMethod = nameof(PrintCopyType.EML),
				IncludeInPrint = true,
			};

			var deliverables = new MockDeliverableCollection { document1 };

			deliverables.Add(Factory.New<DummyBaseBusinessObject>());
			collection.Deliverables = deliverables;

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			contact.DeliveryMethodDescription = "Print";
			AssertHasWarning(contact.DeliveryMethodInfo, "'File 1' cannot be delivered because it has been restricted for 'E-Mail' delivery only.");

			document1 = new MockDeliverable
			{
				DocumentName = "File 1",
				DocumentDeliveryMethod = nameof(PrintCopyType.EML),
				IncludeInPrint = false,
				CanIncludeInPrint = false,
			};
			deliverables = new MockDeliverableCollection { document1 };

			deliverables.Add(Factory.New<DummyBaseBusinessObject>());
			collection.Deliverables = deliverables;
			contact.Validation.ValidateDeliveryMethod();
			AssertNoWarning(contact.DeliveryMethodInfo, "'File 1' cannot be delivered because it has been restricted for 'E-Mail' delivery only.");
		}

		public void TestValidateAttachmentType()
		{
			AssertValidateAttachmentType(Core.Constants.ContactNotifyModes.Email);
			AssertValidateAttachmentType(Core.Constants.ContactNotifyModes.EPrint);
		}

		void AssertValidateAttachmentType(string deliveryMethod)
		{
			var contact = new DocDeliveryContact(Factory);

			contact.Validation.ValidateDeliveryMethod();
			AssertNoErrors(contact.AttachmentTypeInfo);

			contact.DeliveryMethod = deliveryMethod;
			contact.AttachmentType = "ZUB";
			AssertHasErrors(contact.AttachmentTypeInfo);

			contact.AttachmentType = "";
			AssertHasErrors(contact.AttachmentTypeInfo);

			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			AssertNoErrors(contact.AttachmentTypeInfo);

			contact.AttachmentType = OrgConstants.AttachmentType.HTML;
			if (deliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				AssertNoErrors(contact.AttachmentTypeInfo);
			}
			else
			{
				AssertHasErrors(contact.AttachmentTypeInfo);
			}

			contact.AttachmentType = OrgConstants.AttachmentType.HTMF;
			if (deliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				AssertNoErrors(contact.AttachmentTypeInfo);
			}
			else
			{
				AssertHasErrors(contact.AttachmentTypeInfo);
			}

			contact.AttachmentType = OrgConstants.AttachmentType.PDFC;
			if (deliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				AssertNoErrors(contact.AttachmentTypeInfo);
			}
			else
			{
				AssertHasErrors(contact.AttachmentTypeInfo);
			}
		}

		public void TestValidateOrgHeaderPK()
		{
			OrgHeader testOrg = OrgHeader.New(Factory);
			testOrg.OH_FullName = "Hello THere How are You";
			testOrg.MainAddress.OA_Address1 = "18 Henricks Street";
			testOrg.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			AssertNoErrors(contact.OrgHeaderPKInfo);

			contact.OrgHeaderPK = ZGuid.NewZGuid();
			AssertHasErrors(contact.OrgHeaderPKInfo);

			contact.OrgHeaderPK = testOrg.PK;
			AssertNoErrors(contact.OrgHeaderPKInfo);
		}

		public void TestValidateContactName()
		{
			OrgHeader testOrg = OrgHeader.New(Factory);
			testOrg.OH_FullName = "Hello THere How are You";
			testOrg.MainAddress.OA_Address1 = "18 Henricks Street";
			testOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgContact orgContact = testOrg.Contacts.AddNew();
			orgContact.OC_ContactName = "Hello Zubin";
			Factory.Save();

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			AssertNoErrors(contact.NameInfo);

			contact.OrgHeaderPK = testOrg.PK;
			contact.Name = "";
			AssertNoErrors(contact.NameInfo);

			contact.Name = "Hello Zubin";
			AssertNoErrors(contact.NameInfo);

			contact.Name = "Mary Mary Hello";
			AssertNoErrors(contact.NameInfo);
		}

		public void TestValidateEmailFromAddress()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "main@test.com";
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			contact.EmailFromAddressWithType = "XXX";
			AssertHasError(contact.EmailFromAddressWithTypeInfo, "Please select a valid Email Address.");

			contact.EmailFromAddress = "XXX";
			AssertHasError(contact.EmailFromAddressInfo, "Please select a valid Email Address.");

			contact.EmailFromAddressWithType = "Main - main@test.com";
			AssertNoErrors(contact.EmailFromAddressWithTypeInfo);

			contact.EmailFromAddress = "main@test.com";
			AssertNoErrors(contact.EmailFromAddressInfo);
		}

		public void TestValidateDeliveryRecipientType()
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryRecipientType = "Staff";
			contact.Validation.ValidateDeliveryRecipientType();
			AssertNoErrors(contact.DeliveryRecipientTypeInfo);

			contact.DeliveryRecipientType = "c";
			contact.Validation.ValidateDeliveryRecipientType();
			AssertHasError(contact.DeliveryRecipientTypeInfo, "Enter a valid selection.");
		}

		public void TestValidateStaffCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SSS";
			staff.GS_LoginName = "tester";
			staff.GS_EmailAddress = "test@test.com";

			var contact = new DocDeliveryContact(Factory);
			contact.StaffCode = "SSS";
			contact.Validation.ValidateStaffCode();
			AssertNoErrors(contact.StaffCodeInfo);

			contact.StaffCode = "-";
			contact.Validation.ValidateStaffCode();
			AssertHasError(contact.StaffCodeInfo, "Enter a valid Staff.");
		}
	}
}
