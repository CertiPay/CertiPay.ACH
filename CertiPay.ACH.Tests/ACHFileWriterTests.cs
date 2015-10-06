using CertiPay.Common.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Test, Unit]
        public void Generate_File_Header()
        {
            new FileHeader
            {
                PriorityCode = 1,
                ImmediateDestination = "051000033",
                ImmediateOrigin = "059999997",
                FileCreationDate = DateTime.Parse("10/6/2016"),
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
            // TODO
        }

        [Test, Unit]
        public void Generate_Batch_Header()
        {
            // TODO
        }

        [Test, Unit]
        public void Generate_Batch_Control()
        {
            // TODO
        }

        [Test, Unit]
        public void Generate_Detail_Entry_1()
        {
            // TODO
        }
    }
}
