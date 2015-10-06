using FileHelpers;
using System;
using System.Collections.Generic;

namespace CertiPay.ACH
{
    public class ACHFile
    {
        private static readonly IFileHelperEngine<ACHFile> _engine = new FileHelperEngine<ACHFile>(System.Text.Encoding.UTF8);

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
            // TODO

            return _engine.WriteString(new[] { this });
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

    [FixedLengthRecord]
    public class FileHeader
    {
        // TODO record_type, priority_code, immediate_destination, immediate_origin, transmission_datetime, file_id_modifier, record_size, blocking_factor, format_code, immediate_destination_name, immediate_origin_name, reference_code

        public String RecordType { get; set; }
    }

    [FixedLengthRecord]
    public class FileControl
    {
        // TODO record_type, batch_count, block_count, entry_count, entry_hash, debit_total, credit_total, reserved

        public String RecordType { get; set; }
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
    }

    [FixedLengthRecord]
    public class BatchHeader
    {
        // Header - record_type (5), service_class_code, company_name, company_discretionary_data, company_identification, standard_entry_class_code, company_entry_description, company_descriptive_date, effective_entry_date, settlement_date, originator_status_code, originating_dfi_identification, batch_number
    }

    [FixedLengthRecord]
    public class BatchControl
    {
        // Control - entry_count, debit_total, credit_total, entry_hash, has_debits, has_credits
    }

    [FixedLengthRecord]
    public class EntryDetail
    {
        // TODO
    }
}