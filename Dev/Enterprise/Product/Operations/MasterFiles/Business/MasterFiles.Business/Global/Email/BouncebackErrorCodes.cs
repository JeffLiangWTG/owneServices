using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BouncebackErrorCodes : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string E211 = "211";
			public const string E214 = "214";
			public const string E220 = "220";
			public const string E221 = "221";
			public const string E250 = "250";
			public const string E251 = "251";
			public const string E252 = "252";
			public const string E253 = "253";
			public const string E354 = "354";
			public const string E355 = "355";
			public const string E421 = "421";
			public const string E432 = "432";
			public const string E450 = "450";
			public const string E451 = "451";
			public const string E452 = "452";
			public const string E453 = "453";
			public const string E454 = "454";
			public const string E458 = "458";
			public const string E459 = "459";
			public const string E500 = "500";
			public const string E501 = "501";
			public const string E502 = "502";
			public const string E503 = "503";
			public const string E504 = "504";
			public const string E521 = "521";
			public const string E530 = "530";
			public const string E534 = "534";
			public const string E538 = "538";
			public const string E550 = "550";
			public const string E551 = "551";
			public const string E552 = "552";
			public const string E553 = "553";
			public const string E554 = "554";
			public const string EUNK = "UNV";
		}

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string E211 = "System status, or system help reply";
			public const string E214 = "Help message";
			public const string E220 = "Domain service ready. Ready to start TLS";
			public const string E221 = "Domain service closing transmission channel.";
			public const string E250 = "OK, queuing for node started. Requested mail action okay, completed.";
			public const string E251 = "OK, no messages waiting for node. User not local, will forward to forward path.";
			public const string E252 = "OK, pending messages for node started. Cannot VRFY user (e.g., info is not local), but will take message for this user and attempt delivery.";
			public const string E253 = "OK, messages pending messages for node started.";
			public const string E354 = "Start mail input; end with CRLF";
			public const string E355 = "Octet-offset is the transaction offset.";
			public const string E421 = "Domain service not available, closing transmission channel.";
			public const string E432 = "A password transition is needed.";
			public const string E450 = "Requested mail action not taken: mailbox unavailable. ATRN request refused.";
			public const string E451 = "Requested action aborted: local error in processing. Unable to process ATRN request now";
			public const string E452 = "Requested action not taken: insufficient system storage.";
			public const string E453 = "You have no mail.";
			public const string E454 = "TLS not available due to temporary reason. Encryption required for requested authentication mechanism.";
			public const string E458 = "Unable to queue messages for node.";
			public const string E459 = "Node not allowed: reason.";
			public const string E500 = "Command not recognized: command. Syntax error.";
			public const string E501 = "Syntax error, no parameters allowed.";
			public const string E502 = "Command not implemented.";
			public const string E503 = "Bad sequence of commands.";
			public const string E504 = "Command parameter not implemented.";
			public const string E521 = "Machine does not accept mail.";
			public const string E530 = "Must issue a STARTTLS command first. Encryption required for requested authentication mechanism.";
			public const string E534 = "Authentication mechanism is too weak.";
			public const string E538 = "Encryption required for requested authentication mechanism.";
			public const string E550 = "Requested action not taken: mailbox unavailable.";
			public const string E551 = "User not local; please try forward path.";
			public const string E552 = "Requested mail action aborted: exceeded storage allocation.";
			public const string E553 = "Requested action not taken: mailbox name not allowed.";
			public const string E554 = "Transaction failed.";
			public const string EUNK = "Unknown delivery failure";

			#endregion
		}

		public BouncebackErrorCodes()
		{
			AddPair(Codes.E211, Descriptions.E211);
			AddPair(Codes.E214, Descriptions.E214);
			AddPair(Codes.E220, Descriptions.E220);
			AddPair(Codes.E221, Descriptions.E221);
			AddPair(Codes.E250, Descriptions.E250);
			AddPair(Codes.E251, Descriptions.E251);
			AddPair(Codes.E252, Descriptions.E252);
			AddPair(Codes.E253, Descriptions.E253);
			AddPair(Codes.E354, Descriptions.E354);
			AddPair(Codes.E355, Descriptions.E355);
			AddPair(Codes.E421, Descriptions.E421);
			AddPair(Codes.E432, Descriptions.E432);
			AddPair(Codes.E450, Descriptions.E450);
			AddPair(Codes.E451, Descriptions.E451);
			AddPair(Codes.E452, Descriptions.E452);
			AddPair(Codes.E453, Descriptions.E453);
			AddPair(Codes.E454, Descriptions.E454);
			AddPair(Codes.E458, Descriptions.E458);
			AddPair(Codes.E459, Descriptions.E459);
			AddPair(Codes.E500, Descriptions.E500);
			AddPair(Codes.E501, Descriptions.E501);
			AddPair(Codes.E502, Descriptions.E502);
			AddPair(Codes.E503, Descriptions.E503);
			AddPair(Codes.E504, Descriptions.E504);
			AddPair(Codes.E521, Descriptions.E521);
			AddPair(Codes.E530, Descriptions.E530);
			AddPair(Codes.E534, Descriptions.E534);
			AddPair(Codes.E538, Descriptions.E538);
			AddPair(Codes.E550, Descriptions.E550);
			AddPair(Codes.E551, Descriptions.E551);
			AddPair(Codes.E552, Descriptions.E552);
			AddPair(Codes.E553, Descriptions.E553);
			AddPair(Codes.E554, Descriptions.E554);
			AddPair(Codes.EUNK, Descriptions.EUNK);
		}
	}
}
