#region Copyright © 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * Based on code submitted by Mitsugi Ogawa.
 * 
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */
#endregion

using Enterprise.ZArchitecture.Core;

namespace CodeFormatter
{
	/// <summary>
	/// Generates color-coded T-SQL source code.
	/// </summary>
	public class TsqlFormat : CodeFormat
	{
		/// <summary>
		/// Regular expression string to match single line 
		/// comments (--). 
		/// </summary>
		protected override string CommentRegEx
		{
			get
			{
				return (NoResString)@"(?:--\s).*?(?=\r|\n)"; // It is a regular expression
			}
		}

		/// <summary>
		/// Regular expression string to match string literals. 
		/// </summary>
		protected override string StringRegEx
		{
			get
			{
				return @"''|'.*?'|&#39;&#39;|&#39;.*&#39;";
			}
		}

		/// <summary>
		/// Returns <b>false</b>, since T-SQL is not case sensitive.
		/// </summary>
		public override bool CaseSensitive
		{
			get { return false; }
		}

		/// <summary>
		/// The list of T-SQL keywords.
		/// </summary>
		protected override string Keywords
		{
			get
			{
				return (NoResString)"ABSOLUTE ACTION ADA ADD ADMIN AFTER AGGREGATE "
					+ (NoResString)"ALIAS ALL ALLOCATE ALTER AND ANY ARE ARRAY AS ASC "
					+ (NoResString)"ASSERTION AT AUTHORIZATION AVG BACKUP BEFORE BEGIN "
					+ (NoResString)"BETWEEN BINARY BIT BIT_LENGTH BLOB BOOLEAN BOTH BREADTH "
					+ (NoResString)"BREAK BROWSE BULK BY CALL CASCADE CASCADED CASE CAST "
					+ (NoResString)"CATALOG CHAR CHAR_LENGTH CHARACTER CHARACTER_LENGTH "
					+ (NoResString)"CHECK CHECKPOINT CLASS CLOB CLOSE CLUSTERED COALESCE "
					+ (NoResString)"COLLATE COLLATION COLUMN COMMIT COMPLETION COMPUTE "
					+ (NoResString)"CONNECT CONNECTION CONSTRAINT CONSTRAINTS CONSTRUCTOR "
					+ (NoResString)"CONTAINS CONTAINSTABLE CONTINUE CONVERT CORRESPONDING "
					+ (NoResString)"COUNT CREATE CROSS CUBE CURRENT CURRENT_DATE CURRENT_PATH "
					+ (NoResString)"CURRENT_ROLE CURRENT_TIME CURRENT_TIMESTAMP CURRENT_USER "
					+ (NoResString)"CURSOR CYCLE DATA DATABASE DATE DAY DBCC DEALLOCATE DEC "
					+ (NoResString)"DECIMAL DECLARE DEFAULT DEFERRABLE DEFERRED DELETE DENY "
					+ (NoResString)"DEPTH DEREF DESC DESCRIBE DESCRIPTOR DESTROY DESTRUCTOR "
					+ (NoResString)"DETERMINISTIC DIAGNOSTICS DICTIONARY DISCONNECT DISK "
					+ (NoResString)"DISTINCT DISTRIBUTED DOMAIN DOUBLE DROP DUMMY DUMP "
					+ (NoResString)"DYNAMIC EACH ELSE END END-EXEC EQUALS ERRLVL ESCAPE "
					+ (NoResString)"EVERY EXCEPT EXCEPTION EXEC EXECUTE EXISTS EXIT EXTERNAL "
					+ (NoResString)"EXTRACT FALSE FETCH FILE FILLFACTOR FIRST FLOAT FOR "
					+ (NoResString)"FOREIGN FORTRAN FOUND FREE FREETEXT FREETEXTTABLE FROM "
					+ (NoResString)"FULL FUNCTION GENERAL GET GLOBAL GO GOTO GRANT GROUP "
					+ (NoResString)"GROUPING HAVING HOLDLOCK HOST HOUR IDENTITY IDENTITY_INSERT "
					+ (NoResString)"IDENTITYCOL IF IGNORE IMMEDIATE IN INCLUDE INDEX INDICATOR "
					+ (NoResString)"INITIALIZE INITIALLY INNER INOUT INPUT INSENSITIVE INSERT "
					+ (NoResString)"INT INTEGER INTERSECT INTERVAL INTO IS ISOLATION ITERATE "
					+ (NoResString)"JOIN KEY KILL LANGUAGE LARGE LAST LATERAL LEADING LEFT "
					+ (NoResString)"LESS LEVEL LIKE LIMIT LINENO LOAD LOCAL LOCALTIME LOCALTIMESTAMP "
					+ (NoResString)"LOCATOR LOWER MAP MATCH MAX MIN MINUTE MODIFIES MODIFY "
					+ (NoResString)"MODULE MONTH NAMES NATIONAL NATURAL NCHAR NCLOB NEW NEXT "
					+ (NoResString)"NO NOCHECK NONCLUSTERED NONE NOT NULL NULLIF NUMERIC OBJECT "
					+ (NoResString)"OCTET_LENGTH OF OFF OFFSETS OLD ON ONLY OPEN OPENDATASOURCE "
					+ (NoResString)"OPENQUERY OPENROWSET OPENXML OPERATION OPTION OR ORDER "
					+ (NoResString)"ORDINALITY OUT OUTER OUTPUT OVER OVERLAPS PAD PARAMETER "
					+ (NoResString)"PARAMETERS PARTIAL PASCAL PATH PERCENT PLAN POSITION "
					+ (NoResString)"POSTFIX PRECISION PREFIX PREORDER PREPARE PRESERVE "
					+ (NoResString)"PRIMARY PRINT PRIOR PRIVILEGES PROC PROCEDURE "
					+ (NoResString)"PUBLIC RAISERROR READ READS READTEXT REAL RECONFIGURE "
					+ (NoResString)"RECURSIVE REF REFERENCES REFERENCING RELATIVE REPLICATION "
					+ (NoResString)"RESTORE RESTRICT RESULT RETURN RETURNS REVOKE RIGHT ROLE "
					+ (NoResString)"ROLLBACK ROLLUP ROUTINE ROW ROWCOUNT ROWGUIDCOL ROWS RULE "
					+ (NoResString)"SAVE SAVEPOINT SCHEMA SCOPE SCROLL SEARCH SECOND SECTION "
					+ (NoResString)"SELECT SEQUENCE SESSION SESSION_USER SET SETS SETUSER "
					+ (NoResString)"SHUTDOWN SIZE SMALLINT SOME SPACE SPECIFIC SPECIFICTYPE "
					+ (NoResString)"SQL SQLCA SQLCODE SQLERROR SQLEXCEPTION SQLSTATE SQLWARNING "
					+ (NoResString)"START STATE STATEMENT STATIC STATISTICS STRUCTURE SUBSTRING "
					+ (NoResString)"SUM SYSTEM_USER TABLE TEMPORARY TERMINATE TEXTSIZE THAN THEN "
					+ (NoResString)"TIME TIMESTAMP TIMEZONE_HOUR TIMEZONE_MINUTE TO TOP TRAILING "
					+ (NoResString)"TRAN TRANSACTION TRANSLATE TRANSLATION TREAT TRIGGER TRIM "
					+ (NoResString)"TRUE TRUNCATE TSEQUAL UNDER UNION UNIQUE UNKNOWN UNNEST "
					+ (NoResString)"UPDATE UPDATETEXT UPPER USAGE USE USER USING VALUE VALUES "
					+ (NoResString)"VARCHAR VARIABLE VARYING VIEW WAITFOR WHEN WHENEVER WHERE "
					+ (NoResString)"WHILE WITH WITHOUT WORK WRITE WRITETEXT YEAR ZONE"; // These are SQL keywords in a string.
			}
		}

		/// <summary>
		/// Use the pre-processor color to mark keywords that start with @@.
		/// </summary>
		protected override string Preprocessors
		{
			get
			{
				return @"@@CONNECTIONS @@CPU_BUSY @@CURSOR_ROWS @@DATEFIRST "
					+ "@@DBTS @@ERROR @@FETCH_STATUS @@IDENTITY @@IDLE "
					+ "@@IO_BUSY @@LANGID @@LANGUAGE @@LOCK_TIMEOUT "
					+ "@@MAX_CONNECTIONS @@MAX_PRECISION @@NESTLEVEL @@OPTIONS "
					+ "@@PACK_RECEIVED @@PACK_SENT @@PACKET_ERRORS @@PROCID "
					+ "@@REMSERVER @@ROWCOUNT @@SERVERNAME @@SERVICENAME @@SPID "
					+ "@@TEXTSIZE @@TIMETICKS @@TOTAL_ERRORS @@TOTAL_READ "
					+ "@@TOTAL_WRITE @@TRANCOUNT @@VERSION"; // These are SQL keywords in a string.
			}
		}
	}
}
