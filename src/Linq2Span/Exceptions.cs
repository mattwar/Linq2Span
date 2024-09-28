using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Span
{
    internal static class Exceptions
    {
        public static Exception GetSequenceContainsMoreThanOneElement() =>
            new InvalidOperationException("The input sequence contains more than one element.");

        public static Exception GetSequenceContainsMoreThanOneElementOrEmpty() =>
            new InvalidOperationException("The input sequence contains more than one element. - or - The input sequence is empty.");

        public static Exception GetSequenceIsEmpty() =>
            new InvalidOperationException("The source sequence is empty.");

        public static Exception GetSequenceIsEmptyOrNotSatisfied() =>
            new InvalidOperationException("No element satisfies the condition in predicate. -or- The source sequence is empty.");

        public static Exception GetSequenceIsEmptyOrNotSatisifiedOrContainsMoreThanOneElement() =>
            new InvalidOperationException("No element satisfies the condition in predicate. -or- The source sequence is empty. -or- More than one element satisfies the condition.");
    }
}
