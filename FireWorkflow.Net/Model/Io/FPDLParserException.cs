/**
 * 
 * @author chennieyun
 * @Revision to .NET 无忧 lwz0721@gmail.com 2010-02
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireWorkflow.Net.Model.Io
{
    public class FPDLParserException : Exception
    {
        /** Construct a new FPDLParserException. */

        public FPDLParserException()
            :base()
        {
        }

        /** Construct a new FPDLParserException with the specified message.

            @param message The error message
        */

        public FPDLParserException(String message)
            :base(message)
        {
        }

        /** Construct a new FPDLParserException with the specified nested error.

            @param t The nested error
        */

        public FPDLParserException(Exception t)
            :base(t.Message,t)
        {
        }

        /** Construct a new FPDLParserException with the specified error message
            and nested exception.

            @param message The error message
            @param t The nested error
        */

        public FPDLParserException(String message, Exception t)
            : base(message, t)
        {
        }
    }
}
