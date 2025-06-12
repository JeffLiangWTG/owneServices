using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;


namespace CargoWise.DPS.ContentManagement
{

    [IgnoreFirst(6)]
    [IgnoreEmptyLines()]
    [ConditionalRecord(RecordCondition.ExcludeIfMatchRegex, "^\"|^\t")]
    [DelimitedRecord("	")]

    public sealed class CA_OSFIIND_Entity
    {

        public String id;
        public String lastName;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String name1;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String name2;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String name3;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String name4;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String POB;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String altPOB;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String DOB;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String altDOB1;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String altDOB2;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String altDOB3;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String nationality;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String nationality1;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String nationality2;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String misc;
        [FieldQuoted('"', QuoteMode.OptionalForRead, MultilineMode.AllowForRead)]
        public String basis;

    }
}
