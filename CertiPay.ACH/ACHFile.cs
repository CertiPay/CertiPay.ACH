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

        public virtual FileControl Control { get { return new FileControl(this); } }

        public ACHFile()
        {
            this.Header = new FileHeader { };
            this.Batches = new List<Batch>();
        }

        public virtual String Generate()
        {
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

            // Sequence (1): File header record

            // Sequence (2): Company/Batch header record

            // Sequence (3): Entry detail, corporate entry detail record

            // Sequence (4): Addenda record

            // Sequence (5): Company/Batch control record

            // Sequence (6): File control record

            output.Append(this.Header);

            var batchNumber = 1;

            foreach (var batch in this.Batches)
            {
                batch.Header.BatchNumber = batchNumber;

                output.Append(batch);

                batchNumber++; // increment the batch number
            }

            output.Append(this.Control);

            // An ACH file must be "BLOCKED":

            // contain enough ACH records to form a complete "block" (10 records = 1 block = 940 characters)

            // All records within each ACH file are counted (headers, controls, entry details, addenda)

            // If the total number of records do not equal a complete block, "9 filler records" must be added to complete the block

            // A filler record is 94 characters of 9's

            var linesNeeded = 10 - (GetNumberOfLines(this) % 10);

            var fillerRecord = GetFillerRecord();

            foreach (var i in Enumerable.Range(1, linesNeeded))
            {
                output.AppendLine(fillerRecord);
            }

            return output.ToString();
        }

        public static int GetNumberOfLines(ACHFile file)
        {
            var lines = 1; // file header

            foreach (var batch in file.Batches)
            {
                lines += 2; // batch header + control
                lines += batch.Entries.Count;
            }

            lines += 1; // file control

            return lines;
        }

        public static String GetFillerRecord()
        {
            return new String('9', CHARACTERS_PER_LINE);
        }
    }

    public enum TransactionCode
    {
        Checking_Credit = 22,

        Checking_Debit = 27,

        Checking_Credit_Prenote = 23,

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
        public readonly int RecordTypeCode = 1;

        public int PriorityCode = 1;

        public String ImmediateDestination = String.Empty; // ACHBankRoutingNumber

        public String ImmediateOrigin = String.Empty;

        public DateTime FileCreationDateTime = DateTime.UtcNow;

        // public String FileCreationTime = String.Empty;

        public String FileIDModifier = "A";

        public readonly int RecordSize = 94;

        public readonly int BlockingFactor = 10;

        public readonly int FormatCode = 1;

        public String ImmediateDestinationName = String.Empty; // ACHFileBankName

        public String ImmediateOriginName = String.Empty;

        public String ReferenceCode = String.Empty;

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordTypeCode);

            sb.Append(PriorityCode.ToString().TrimAndPadLeft(2, '0'));

            sb.Append(ImmediateDestination.TrimAndPadLeft(10));

            sb.Append(ImmediateOrigin.TrimAndPadRight(10));

            sb.Append(FileCreationDateTime.ToString("yyMMdd"));

            sb.Append(FileCreationDateTime.ToString("HHmm"));

            sb.Append(FileIDModifier.TrimAndPadRight(1));

            sb.Append(RecordSize.ToString().TrimAndPadLeft(3, '0'));

            sb.Append(BlockingFactor);

            sb.Append(FormatCode);

            sb.Append(ImmediateDestinationName.TrimAndPadRight(23));

            sb.Append(ImmediateOriginName.TrimAndPadRight(23));

            sb.Append(ReferenceCode.TrimAndPadRight(8));

            sb.AppendLine();

            return sb.ToString();
        }
    }

    /// <summary>
    /// Grand total of transactions and amounts
    /// </summary>
    public class FileControl
    {
        public FileControl()
        {
            // Nothing to do here
        }

        internal FileControl(ACHFile file)
        {
            this.BatchCount = file.Batches.Count;

            this.EntryCount = file.Batches.Sum(batch => batch.Entries.Count);

            this.TotalCredits = file.Batches.Sum(batch => batch.Control.TotalCredits);
            this.TotalDebits = file.Batches.Sum(batch => batch.Control.TotalDebits);

            this.BlockCount = Math.Ceiling(ACHFile.GetNumberOfLines(file) / 10m);

            this.EntryHash = CalculateEntryHash(file);
        }

        public readonly int RecordType = 9;

        public int BatchCount = 0;

        public Decimal BlockCount = 0;

        public int EntryCount = 0;

        public String EntryHash = String.Empty;

        public Decimal TotalDebits = 0;

        public Decimal TotalCredits = 0;

        public readonly String Reserved = "      ";

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordType);

            sb.Append(BatchCount.ToString().TrimAndPadLeft(6, '0'));

            sb.Append(BlockCount.ToString().TrimAndPadLeft(6, '0'));

            sb.Append(EntryCount.ToString().TrimAndPadLeft(8, '0'));

            sb.Append(EntryHash.TrimAndPadLeft(10, '0'));

            sb.Append((TotalDebits * 100).ToString().TrimAndPadLeft(12, '0')); // 123.45 => 000000012345

            sb.Append((TotalCredits * 100).ToString().TrimAndPadLeft(12, '0')); // 123.45 => 000000012345

            sb.Append(Reserved.TrimAndPadRight(39));

            sb.AppendLine();

            return sb.ToString();
        }

        public static String CalculateEntryHash(ACHFile file)
        {
            //FOR EACH ORIGINATED TRANSACTION, YOU HAVE
            //GENERATED A TYPE ‘6’ OR ENTRY DETAIL RECORD. ON THE
            //ENTRY DETAIL RECORD THERE IS A RECEIVING DEPOSITORY
            //FINANANCIAL INSTITUTION (RDFI)IDENTIFICATION(TRANSIT
            //ROUTING NUMBER) LOCATED IN POSITIONS 4 THROUGH 11.
            //THE FIRST 8 DIGITS OF EACH RDFI’s TRANSIT ROUTING
            //NUMBER IS TREATED AS A NUMBER.

            //ALL TRANSIT ROUTING NUMBERS WITHIN THE BATCH ARE
            //ADDED TOGETHER FOR THE ENTRY HASH ON THE TYPE '8',
            //BATCH CONTROL RECORD.ALL TRANSIT ROUTING NUMBERS
            //WITHIN EACH FILE ARE ADDED TOGETHER TO CALCULATE THE
            //VALUE OF THE ENTRY HASH ON THE TYPE '9', FILE CONTROL
            //RECORD. (NOTE: DO NOT INCLUDE THE CHECK DIGIT OF THE
            //TRANSIT ROUTING NUMBER, POSITION 12, IN THIS
            //CALCULATION.) THE ENTRY HASH CALCULATIION CHECK IS
            //USED IN THE PNC BANK FILE EDITING PROCESS TO HELP
            //ENSURE DATA INTEGRITY OF THE BATCH AND FILE
            //GENERATED BY YOUR PROCESSING.

            var sum =
                file
                .Batches
                .SelectMany(batch => batch.Entries)
                .Select(entry => entry.ReceivingDFIIdentification)
                .Select(routingNumber => routingNumber.Substring(0, 8))
                .Select(substring => Decimal.Parse(substring))
                .Sum();

            //IF THE SUM OF THE RDFI TRANSIT ROUTING NUMBERS IS A
            //NUMBER GREATER THAN TEN DIGITS, REMOVE OR DROP THE
            //NUMBER OF DIGITS FROM THE LEFT SIDE OF THE NUMBER
            //UNTIL ONLY TEN DIGITS REMAIN. FOR EXAMPLE, IF THE SUM
            //OF THE TRANSIT ROUTING NUMBERS IS 234567898765,
            //REMOVE THE “23” FOR A HASH OF 4567898765.

            var last10 = sum % 10000000000;

            return last10.ToString();
        }
    }

    public class Batch
    {
        public virtual BatchHeader Header { get; set; }

        public virtual ICollection<EntryDetail> Entries { get; set; }

        public virtual BatchControl Control { get { return new BatchControl(this); } }

        // TODO Addendas?

        public Batch()
        {
            this.Header = new BatchHeader { };
            this.Entries = new List<EntryDetail>();
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
        public readonly int RecordTypeCode = 5;

        public ServiceClassCode ServiceClass = ServiceClassCode.Mixed_Debits_And_Credits;

        public String CompanyName = String.Empty; // control name?

        public String CompanyDiscreationaryData = String.Empty;

        public String CompanyId = String.Empty; // ACH-EIN-Prefix + ControlEIN

        public String StandardEntryClassCode = "PPD";

        public String EntryDescription = "PAYROLL";

        public DateTime CompanyDescriptiveDate = DateTime.Today; // CheckDate

        public DateTime EffectiveEntryDate = DateTime.Today;

        public String SettlementDate = String.Empty;

        public char OriginatorStatusCode = '1';

        public String OriginatingDFIIdentification = String.Empty; // ACHBankRoutingNumber

        public int BatchNumber = 1;

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordTypeCode);

            sb.Append((int)ServiceClass);

            sb.Append(CompanyName.TrimAndPadRight(16));

            sb.Append(CompanyDiscreationaryData.TrimAndPadRight(20));

            sb.Append(CompanyId.TrimAndPadRight(10));

            sb.Append(StandardEntryClassCode.TrimAndPadRight(3));

            sb.Append(EntryDescription.TrimAndPadRight(10));

            sb.Append(CompanyDescriptiveDate.ToString("yyMMdd"));

            sb.Append(EffectiveEntryDate.ToString("yyMMdd"));

            sb.Append(SettlementDate.TrimAndPadLeft(3, ' '));

            sb.Append(OriginatorStatusCode);

            sb.Append(OriginatingDFIIdentification.TrimAndPadRight(8));

            sb.Append(BatchNumber.ToString().TrimAndPadLeft(7, '0'));

            sb.AppendLine();

            return sb.ToString();
        }
    }

    /// <summary>
    /// How many transactions and total amounts?
    /// </summary>
    public class BatchControl
    {
        public BatchControl()
        {
            // Nothing to do here
        }

        internal BatchControl(Batch batch)
        {
            this.ServiceClass = batch.Header.ServiceClass;
            this.CompanyId = batch.Header.CompanyId;
            this.BatchNumber = batch.Header.BatchNumber;

            this.EntryCount = batch.Entries.Count;

            this.EntryHash = CalculateEntryHash(batch);

            var debits = new[] { TransactionCode.Checking_Debit, TransactionCode.Checking_Debit_Prenote, TransactionCode.Saving_Debit, TransactionCode.Saving_Debit_Prenote };

            var credits = new[] { TransactionCode.Checking_Credit, TransactionCode.Checking_Credit_Prenote, TransactionCode.Saving_Credit, TransactionCode.Saving_Credit_Prenote };

            this.TotalDebits = batch.Entries.Where(entry => debits.Contains(entry.Transaction_Code)).Sum(entry => entry.Amount);

            this.TotalCredits = batch.Entries.Where(entry => credits.Contains(entry.Transaction_Code)).Sum(entry => entry.Amount);

            this.HasDebits = batch.Entries.Where(entry => debits.Contains(entry.Transaction_Code)).Any();
            this.HasCredits = batch.Entries.Where(entry => credits.Contains(entry.Transaction_Code)).Any();

            if (HasCredits && HasDebits)
            {
                this.ServiceClass = batch.Header.ServiceClass = ServiceClassCode.Mixed_Debits_And_Credits;
            }
            else if (HasDebits)
            {
                this.ServiceClass = batch.Header.ServiceClass = ServiceClassCode.Debits_Only;
            }
            else if (HasCredits)
            {
                this.ServiceClass = batch.Header.ServiceClass = ServiceClassCode.Credits_Only;
            }
        }

        /// <summary>
        /// THIS IS THE FIRST POSITION FOR ALL RECORD FORMATS. THE CODE IS
        /// UNIQUE FOR EACH RECORD TYPE. THE COMPANY/BATCH CONTROL RECORD
        /// USES RECORD TYPE CODE 8.
        /// </summary>
        public readonly int RecordType = 8;

        /// <summary>
        /// THE SERVICE CLASS CODE DEFINES THE TYPE OF 02-04 ENTRIES CONTAINED IN THE BATCH.
        /// </summary>
        public ServiceClassCode ServiceClass = ServiceClassCode.Mixed_Debits_And_Credits;

        public String CompanyId = String.Empty; // group name or ACHEINPrefix + EIN

        /// <summary>
        /// COUNT IS A TALLY OF EACH TYPE ‘6’ RECORD AND IF USED, ALSO EACH ADDENDA WITHIN THE BATCH.
        /// </summary>
        public int EntryCount = 0;

        /// <summary>
        /// FOR EACH ORIGINATED TRANSACTION, YOU HAVE
        /// GENERATED A TYPE ‘6’ OR ENTRY DETAIL RECORD. ON THE
        /// ENTRY DETAIL RECORD THERE IS A RECEIVING DEPOSITORY
        /// FINANANCIAL INSTITUTION (RDFI)IDENTIFICATION(TRANSIT
        /// ROUTING NUMBER) LOCATED IN POSITIONS 4 THROUGH 11.
        /// THE FIRST 8 DIGITS OF EACH RDFI’s TRANSIT ROUTING
        /// NUMBER IS TREATED AS A NUMBER.

        /// ALL TRANSIT ROUTING NUMBERS WITHIN THE BATCH ARE
        /// ADDED TOGETHER FOR THE ENTRY HASH ON THE TYPE '8',
        /// BATCH CONTROL RECORD.ALL TRANSIT ROUTING NUMBERS
        /// WITHIN EACH FILE ARE ADDED TOGETHER TO CALCULATE THE
        /// VALUE OF THE ENTRY HASH ON THE TYPE '9', FILE CONTROL
        /// RECORD. (NOTE: DO NOT INCLUDE THE CHECK DIGIT OF THE
        /// TRANSIT ROUTING NUMBER, POSITION 12, IN THIS
        /// CALCULATION.) THE ENTRY HASH CALCULATIION CHECK IS
        /// USED IN THE PNC BANK FILE EDITING PROCESS TO HELP
        /// ENSURE DATA INTEGRITY OF THE BATCH AND FILE
        /// GENERATED BY YOUR PROCESSING.
        /// </summary>
        public String EntryHash = String.Empty;

        /// <summary>
        /// SUM TOTAL OF ALL DEBIT AMOUNTS WITHIN BATCH’S TYPE ‘6’ RECORD.
        /// </summary>
        public Decimal TotalDebits = 0;

        /// <summary>
        /// SUM TOTAL OF ALL CREDIT AMOUNTS WITHIN BATCH’S TYPE ‘6’ RECORD
        /// </summary>
        public Decimal TotalCredits = 0;

        internal Boolean HasDebits = false;

        internal Boolean HasCredits = false;

        /// <summary>
        /// TAX ID PREFIXED WITH A NUMERIC
        /// </summary>
        public String CompanyIdentification = String.Empty;

        public readonly String MessageAuthenticationCode = String.Empty;

        public readonly String Reserved = "      ";

        /// <summary>
        /// FIRST 8 DIGITS OF BANK ABA NUMBER
        /// </summary>
        public String OriginatingDFIId = String.Empty;

        /// <summary>
        /// NUMBER ASSIGNED IN ASCENDING SEQUENCE TO EACH BATCH WITHIN THE FILE
        /// </summary>
        public int BatchNumber = 0;

        public static String CalculateEntryHash(Batch batch)
        {
            var sum =
                batch
                .Entries
                .Select(entry => entry.ReceivingDFIIdentification)
                .Select(routingNumber => routingNumber.Substring(0, 8))
                .Select(substring => Decimal.Parse(substring))
                .Sum();

            //IF THE SUM OF THE RDFI TRANSIT ROUTING NUMBERS IS A
            //NUMBER GREATER THAN TEN DIGITS, REMOVE OR DROP THE
            //NUMBER OF DIGITS FROM THE LEFT SIDE OF THE NUMBER
            //UNTIL ONLY TEN DIGITS REMAIN. FOR EXAMPLE, IF THE SUM
            //OF THE TRANSIT ROUTING NUMBERS IS 234567898765,
            //REMOVE THE “23” FOR A HASH OF 4567898765.

            var last10 = sum % 10000000000;

            return last10.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordType);

            sb.Append((int)ServiceClass);

            sb.Append(EntryCount.ToString().TrimAndPadLeft(6, '0'));

            sb.Append(EntryHash.TrimAndPadLeft(10, '0'));

            sb.Append((TotalDebits * 100).ToString().TrimAndPadLeft(12, '0'));

            sb.Append((TotalCredits * 100).ToString().TrimAndPadLeft(12, '0'));

            sb.Append(CompanyId.TrimAndPadRight(10));

            sb.Append(MessageAuthenticationCode.TrimAndPadRight(19));

            sb.Append(Reserved.TrimAndPadRight(6));

            sb.Append(OriginatingDFIId.TrimAndPadRight(8));

            sb.Append(BatchNumber.ToString().TrimAndPadLeft(7, '0'));

            sb.AppendLine();

            return sb.ToString();
        }
    }

    /// <summary>
    /// What is the RDFI, receiver, and amount?
    /// </summary>
    public class EntryDetail
    {
        public readonly int RecordTypeCode = 6;

        /// <summary>
        /// THE TRANSACTION CODE IDENTIFIES THE TYPE OF ENTRY.
        /// </summary>
        public TransactionCode Transaction_Code = TransactionCode.Checking_Credit;

        /// <summary>
        /// FIRST 8 DIGITS OF THE RECEIVER’S BANK TRANSIT ROUTING NUMBER AT
        /// THE FINANCIAL INSTITUTION WHERE THE RECEIVER'S ACCOUNT IS MAINTAINED.
        /// </summary>
        public String ReceivingDFIIdentification = String.Empty;

        /// <summary>
        /// LAST DIGIT OF RECEIVER'S BANK TRANSIT ROUTING NUMBER.
        /// </summary>
        public int CheckDigit;

        /// <summary>
        /// THIS IS THE RECEIVER’S BANK ACCOUNT NUMBER. IF THE ACCOUNT NUMBER
        /// EXCEEDS 17 POSITIONS, ONLY USE THE LEFT MOST 17 CHARACTERS. ANY
        /// SPACES WITHIN THE ACCOUNT NUMBER SHOULD BE OMITTED WHEN PREPARING
        /// THE ENTRY. THIS FIELD MUST BE LEFT JUSTIFIED.
        /// </summary>
        public String DFIAccountNumber = String.Empty;

        /// <summary>
        /// The amount of the transaction in decimal format. Will be converted when writing the file.
        /// </summary>
        public Decimal Amount = Decimal.Zero;

        /// <summary>
        /// THIS IS AN IDENTIFYING NUMBER BY WHICH THE RECEIVER IS KNOWN TO
        /// THE ORIGINATOR. IT IS INCLUDED FOR FURTHER IDENTIFICATION AND
        /// DESCRIPTIVE PURPOSES.
        /// </summary>
        public String IndividualIdentificationNumber = String.Empty;

        /// <summary>
        /// THIS IS THE NAME IDENTIFYING THE RECEIVER OF THE TRANSACTION.
        /// </summary>
        public String IndividualName = String.Empty;

        /// <summary>
        /// THIS FIELD MUST BE LEFT BLANK.
        /// </summary>
        public readonly String DiscretionaryData = String.Empty;

        /// <summary>
        /// IF PPD OR CCD, ENTER 0 IN THIS FIELD TO INDICATE NO ADDENDA
        /// RECORD WILL FOLLOW. IF AN ADDENDA DOES FOLLOW THIS DETAIL RECORD,
        /// ENTER 1 TO INDICATE A '7' RECORD WILL FOLLOW.
        /// </summary>
        public readonly int AddendaRecordIndicator = 0;

        /// <summary>
        /// THE TRACE NUMBER IS A MEANS FOR THE
        /// ORIGINATOR TO IDENTIFY THE INDIVIDUAL ENTRIES.THE FIRST 8 POSITIONS
        /// OF THE FIELD SHOULD BE YOUR PNC BANK TRANSIT ROUTING NUMBER (WITHOUT THE
        /// CHECK DIGIT). THE REMAINDER OF THE FIELD MUST BE A UNIQUE NUMBER,
        /// ASSIGNED IN ASCENDING ORDER FOR EACH ENTRY.TRACE NUMBERS MAY BE
        /// DUPLICATED ACROSS DIFFERENT FILES.
        /// </summary>
        public String TraceNumber = String.Empty; // Routing # + company record count

        public override string ToString()
        {
            var sb = new StringBuilder(ACHFile.CHARACTERS_PER_LINE);

            sb.Append(RecordTypeCode);

            sb.Append((int)Transaction_Code);

            sb.Append(ReceivingDFIIdentification.TrimAndPadRight(8));

            sb.Append(CheckDigit);

            sb.Append(DFIAccountNumber.TrimAndPadRight(17));

            sb.Append((Amount * 100).ToString().TrimAndPadLeft(10, '0')); // 123.45 => 0000012345

            sb.Append(IndividualIdentificationNumber.TrimAndPadRight(15));

            sb.Append(IndividualName.TrimAndPadRight(22));

            sb.Append(DiscretionaryData.TrimAndPadRight(2));

            sb.Append(AddendaRecordIndicator.ToString().TrimAndPadLeft(1, '0'));

            sb.Append(TraceNumber.ToString().TrimAndPadLeft(15, '0'));

            sb.AppendLine();

            return sb.ToString();
        }
    }
}