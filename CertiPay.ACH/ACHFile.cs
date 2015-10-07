using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CertiPay.ACH
{
    public class ACHFile
    {
        internal const int CHARACTERS_PER_LINE = 94;

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

            var nines = Enumerable.Range(1, CHARACTERS_PER_LINE).Select(_ => "9");

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
    public class FileHeader
    {
        public int RecordTypeCode = 1;

        public int PriorityCode = 1;

        public String ImmediateDestination = String.Empty;

        public String ImmediateOrigin = String.Empty;

        public DateTime FileCreationDateTime = DateTime.UtcNow;

        // public String FileCreationTime = String.Empty;

        public String FileIDModifier = "A";

        public readonly int RecordSize = 94;

        public readonly int BlockingFactor = 10;

        public int FormatCode = 1;

        public String ImmediateDestinationName = String.Empty;

        public String ImmediateOriginName = String.Empty;

        public String ReferenceCode = String.Empty;

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordTypeCode);

            sb.Append(PriorityCode.ToString().PadLeft(2, '0'));

            sb.Append(ImmediateDestination.PadRight(10));

            sb.Append(ImmediateOrigin.PadRight(10));

            sb.Append(FileCreationDateTime.ToString("yyMMdd"));

            sb.Append(FileCreationDateTime.ToString("HHmm"));

            sb.Append(FileIDModifier);

            sb.Append(RecordSize.ToString().PadLeft(3, '0'));

            sb.Append(BlockingFactor);

            sb.Append(FormatCode);

            sb.Append(ImmediateDestinationName.PadRight(23));

            sb.Append(ImmediateOriginName.PadRight(23));

            sb.Append(ReferenceCode.PadRight(8));

            return sb.ToString();
        }
    }

    /// <summary>
    /// Grand total of transactions and amounts
    /// </summary>
    public class FileControl
    {
        // TODO record_type, batch_count, block_count, entry_count, entry_hash, debit_total, credit_total, reserved

        public String RecordType { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            // TODO

            return sb.ToString();
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
    public class BatchHeader
    {
        public int RecordTypeCode = 5;

        public ServiceClassCode ServiceClass = ServiceClassCode.Debits_Only;

        public String CompanyName = String.Empty;

        public String CompanyDiscreationaryData = String.Empty;

        public String CompanyId = String.Empty;

        public String StandardEntryClassCode = String.Empty;

        public String EntryDescription = String.Empty;

        public String CompanyDescriptiveDate = String.Empty;

        public DateTime EffectiveEntryDate = DateTime.Today;

        public int SettlementDate;

        public char OriginatorStatusCode;

        public String OriginatingDFIIdentification = String.Empty;

        public int BatchNumber = 1;

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordTypeCode);

            sb.Append((int)ServiceClass);

            sb.Append(CompanyName.PadRight(16));

            sb.Append(CompanyDiscreationaryData.PadRight(20));

            sb.Append(CompanyId.PadRight(10));

            sb.Append(StandardEntryClassCode.PadRight(3));

            sb.Append(EntryDescription.PadRight(10));

            sb.Append(CompanyDescriptiveDate.PadRight(6));

            sb.Append(EffectiveEntryDate.ToString("yyMMdd"));

            sb.Append(SettlementDate.ToString().PadLeft(3, '0'));

            sb.Append(OriginatorStatusCode);

            sb.Append(OriginatingDFIIdentification.PadRight(8));

            sb.Append(BatchNumber.ToString().PadLeft(7, '0'));

            return sb.ToString();
        }
    }

    /// <summary>
    /// How many transactions and total amounts?
    /// </summary>
    public class BatchControl
    {
        // Control - entry_count, debit_total, credit_total, entry_hash, has_debits, has_credits

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            // TODO

            return sb.ToString();
        }
    }

    /// <summary>
    /// What is the RDFI, receiver, and amount?
    /// </summary>
    public class EntryDetail
    {
        // TODO

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            // TODO

            return sb.ToString();
        }
    }

    public static class StringExtensions
    {
        public static String TrimAndPadLeft(this String original, int length, char paddingChar = ' ')
        {
            return
                original
                .NotNull()
                .PadLeft(length, paddingChar)
                .Substring(0, length);
        }

        public static String TrimAndPadRight(this String original, int length, char paddingChar = ' ')
        {
            return
                original
                .NotNull()
                .PadRight(length, paddingChar)
                .Substring(0, length);
        }

        public static String NotNull(this String original)
        {
            return String.IsNullOrWhiteSpace(original) ? String.Empty : original;
        }
    }
}