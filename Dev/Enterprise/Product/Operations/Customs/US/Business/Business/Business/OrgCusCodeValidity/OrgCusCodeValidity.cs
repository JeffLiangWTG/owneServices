using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class OrgCusCodeValidity : MasterFiles.Business.OrgCusCodeValidity, Integration.Customs.US.IOrgCusCodeValidity
	{
		public OrgCusCodeValidity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		public override ZString VerificationAuthority
		{
			get
			{
				var result = ZString.Empty;

				if (OCV_Verified)
				{
					if (OCV_VerificationAuthority.IsEmpty)
					{
						result = "USER ENTRY";
					}
					else
					{
						result = OCV_VerificationAuthority;
					}
				}

				return result;
			}
		}

		public override ZString SnapshotContextForDisplay
		{
			get
			{
				if (!fSnap.HasValue)
				{
					var result = OCV_SnapShotOfWhatIsVerified;

					if (!OCV_SnapShotOfWhatIsVerified.IsEmpty && RegistrationNumber?.Header is OrgHeader header)
					{
						var emailBuilder = new ZStringBuilder();
						try
						{
							using (var stringReader = new StringReader(OCV_SnapShotOfWhatIsVerified))
							{
								var messageBlocks = new List<MessageBlock>();
								var reader = new BlockControlReader(stringReader, new string[] { EDIMessage.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.GBIReferenceStatusUpdate);
								var enumerator = reader.GetOutputMessageEnumerator();
								while (enumerator.MoveNext())
								{
									messageBlocks.Add(enumerator.Current);
								}

								GlobalBusinessIdentifierStatusNotificationProcessor.GetGlobalBusinessIdentifiersAndEmailBodyAndBranchAndOverallStatus(Factory, messageBlocks, emailBuilder, goMessage: null, orgHeader: header);
							}
						}
						finally
						{
							result = emailBuilder.ToString();
						}
					}

					fSnap = result;
				}

				return fSnap.Value;
			}
		}
		ZString? fSnap;

		#endregion
	}
}
