using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CertiPay.ACH
{
    public class ACHFile
    {
        internal static readonly Encoding FileEncoding = System.Text.Encoding.UTF8;

        internal const int characters_per_line = 94;

        public virtual FileHeader Header { get; set; }

        public virtual ICollection<Batch> Batches { get; set; }

        public virtual FileControl Control { get; set; }

        public ACHFile()
        {
            this.Header = new FileHeader { };
            this.Batches = new List<Batch>();
            this.Control = new FileControl { };
        }

        public virtual String Generate()
        {
            // Sequence:

            // File header record

            // Company/Batch header record

            // Entry detail, corporate entry detail record

            // Addenda record

            // Company/Batch control record

            // File control record

            // Data Specifications:

            // Alphameric and Alphabetic fields: left-justified and space filled

            // Numeric fields: right-justified, unsigned and zero filled

            // Characters used are restricted to: 0-9, A-Z, a-z, space, special characters

            // Field specific: require specific data in them or a requirement to be left blank

            // Upper case characters MUST be used for:

            //      Standard Entry Class (SEC) code field
            //      File ID modifier field
            //      Change code field and refused COR change field
            //      Return reason code fields (all types of returns)
            //      Company entry description fields containing any of these words: reversal, reclaim, nonsettled, autoenroll, redepcheck, no check, return fee, hcclaimpmt

            var output = new StringBuilder { };

            output.Append(this.Header);

            foreach (var batch in this.Batches)
            {
                output.Append(batch);
            }

            output.Append(this.Control);

            return output.ToString();
        }

        public static String GetFillerRecord()
        {
            // Returns a line of 94 characters of 9's

            var nines = Enumerable.Range(1, characters_per_line).Select(_ => "9");

            return String.Join(String.Empty, nines);
        }
    }

    public enum TransactionCode
    {
        Checking_Credit = 22,

        Checking_Debit = 27,

        Checking_Credit_Prenot = 23,

        Checking_Debit_Prenote = 28,

        Saving_Credit = 32,

        Saving_Debit = 37,

        Saving_Credit_Prenote = 33,

        Saving_Debit_Prenote = 38,
    }

    public enum ServiceClassCode
    {
        Mixed_Debits_And_Credits = 200,

        Credits_Only = 220,

        Debits_Only = 225,

        Automated_Accounting_Advices = 280
    }

    /// <summary>
    /// Where did it come from and where is it going?
    /// </summary>
    [FixedLengthRecord]
    public class FileHeader
    {
        private static readonly IFileHelperEngine<FileHeader> _engine = new FileHelperEngine<FileHeader>(ACHFile.FileEncoding);

        [FieldFixedLength(1)]
        public int RecordTypeCode = 1;

        [FieldFixedLength(2)]
        [FieldAlign(AlignMode.Right, '0')]
        public int PriorityCode = 1;

        [FieldFixedLength(10)]
        public String ImmediateDestination = String.Empty;

        [FieldFixedLength(10)]
        public String ImmediateOrigin = String.Empty;

        [FieldFixedLength(6)]
        [FieldConverter(ConverterKind.Date, "YYMMDD")]
        public DateTime FileCreationDate = DateTime.UtcNow;

        [FieldFixedLength(4)]
        public String FileCreationTime = String.Empty;

        [FieldFixedLength(1)]
        public String FileIDModifier = "A";

        [FieldFixedLength(3)]
        [FieldAlign(AlignMode.Right, '0')]
        public int RecordSize = 94;

        [FieldFixedLength(2)]
        public int BlockingFactor = 10;

        [FieldFixedLength(1)]
        public int FormatCode = 1;

        [FieldFixedLength(23)]
        public String ImmediateDestinationName = String.Empty;

        [FieldFixedLength(23)]
        public String ImmediateOriginName = String.Empty;

        [FieldFixedLength(8)]
        public String ReferenceCode = String.Empty;

        public override string ToString()
        {
            return _engine.WriteString(new[] { this });
        }
    }

    /// <summary>
    /// Grand total of transactions and amounts
    /// </summary>
    [FixedLengthRecord]
    public class FileControl
    {
        private static readonly IFileHelperEngine<FileControl> _engine = new FileHelperEngine<FileControl>(ACHFile.FileEncoding);

        // TODO record_type, batch_count, block_count, entry_count, entry_hash, debit_total, credit_total, reserved

        public String RecordType { get; set; }

        public override string ToString()
        {
            return _engine.WriteString(new[] { this });
        }
    }

    public class Batch
    {
        public virtual BatchHeader Header { get; set; }

        public virtual ICollection<EntryDetail> Entries { get; set; }

        public virtual BatchControl Control { get; set; }

        // Addendas

        public Batch()
        {
            this.Header = new BatchHeader { };
            this.Entries = new List<EntryDetail>();
            this.Control = new BatchControl { };
        }

        public override string ToString()
        {
            var output = new StringBuilder { };

            output.Append(this.Header);

            foreach (var entry in Entries)
            {
                output.Append(entry);
            }

            output.Append(this.Control);

            return output.ToString();
        }
    }

    /// <summary>
    /// Who is it from and what is it?
    /// </summary>
    [FixedLengthRecord]
    public class BatchHeader
    {
        private static readonly IFileHelperEngine<BatchHeader> _engine = new FileHelperEngine<BatchHeader>(ACHFile.FileEncoding);

        // Header - record_type (5), service_class_code, company_name, company_discretionary_data, company_identification, standard_entry_class_code, company_entry_description, company_descriptive_date, effective_entry_date, settlement_date, originator_status_code, originating_dfi_identification, batch_number

        public override string ToString()
        {
            return _engine.WriteString(new[] { this });
        }
    }

    /// <summary>
    /// How many transactions and total amounts?
    /// </summary>
    [FixedLengthRecord]
    public class BatchControl
    {
        private static readonly IFileHelperEngine<BatchControl> _engine = new FileHelperEngine<BatchControl>(ACHFile.FileEncoding);

        // Control - entry_count, debit_total, credit_total, entry_hash, has_debits, has_credits

        public override string ToString()
        {
            return _engine.WriteString(new[] { this });
        }
    }

    /// <summary>
    /// What is the RDFI, receiver, and amount?
    /// </summary>
    [FixedLengthRecord]
    public class EntryDetail
    {
        private static readonly IFileHelperEngine<EntryDetail> _engine = new FileHelperEngine<EntryDetail>(ACHFile.FileEncoding);

        // TODO

        public override string ToString()
        {
            return _engine.WriteString(new[] { this });
        }
    }
}