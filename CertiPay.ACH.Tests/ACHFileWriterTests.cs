using CertiPay.Common.Testing;
using NUnit.Framework;
using System;

namespace CertiPay.ACH.Tests
{
    public class ACHFileWriterTests
    {
        // Test Cases:

        // File header

        // File control

        // Batch header

        // Batch control

        // Detail entry

        // Full file generation(s)

        // Ensure file is "blocked"

        [Test, Unit]
        public void Generate_File_1()
        {
            new ACHFile
            {
                // TODO
            }
            .ToString()
            .VerifyMe();
        }

        [Test, Unit]
        public void Generate_File_Header()
        {
            new FileHeader
            {
                PriorityCode = 1,
                ImmediateDestination = "051000033",
                ImmediateOrigin = "059999997",
                FileCreationDateTime = DateTime.Parse("10/6/2016"),
                FileIDModifier = "A",
                ImmediateDestinationName = "TCB Services",
                ImmediateOriginName = "ABC TRUST"
            }
            .ToString()
            .VerifyMe();
        }

        [Test, Unit]
        public void Generate_File_Control()
        {
            new ACHFile
            {
                // TODO
            }
            .Control
            .ToString()
            .VerifyMe();
        }

        [Test, Unit]
        public void Generate_Batch_Header()
        {
            new BatchHeader
            {
                ServiceClass = ServiceClassCode.Debits_Only,
                CompanyName = "MY BEST COMP.",
                CompanyDiscreationaryData = "INCLUDES OVERTIME",
                CompanyId = "141987123",
                StandardEntryClassCode = "4PP",
                EntryDescription = "DPAYROLL",
                CompanyDescriptiveDate = DateTime.Parse("10/1/2015"),
                EffectiveEntryDate = DateTime.Parse("10/7/2015"),
                SettlementDate = "60",
                OriginatorStatusCode = '2',
                OriginatingDFIIdentification = "1099912",
                BatchNumber = 3400001
            }
            .ToString()
            .VerifyMe();
        }

        [Test, Unit]
        public void Generate_Batch_Control()
        {
            new Batch
            {
                // TODO
            }
            .Control
            .ToString()
            .VerifyMe();
        }

        [Test, Unit]
        public void Generate_Detail_Entry_1()
        {
            new EntryDetail
            {
                ReceivingDFIIdentification = "03100005",
                CheckDigit = 3,
                DFIAccountNumber = "1234567890",
                Amount = 100.00m,
                IndividualName = "Wagner, Matt",
                TraceNumber = "0310000500001"
            }
            .ToString()
            .VerifyMe();
        }
    }
}