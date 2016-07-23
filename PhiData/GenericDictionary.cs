using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PhiClient
{
    /**
     * This class exists because Unity can't serialize
     * types that uses generic types.
     */
    [Serializable]
    public class GenericDictionary : Dictionary<string, object>
    {
        public GenericDictionary(): base()
        {

        }

        protected GenericDictionary(SerializationInfo information, StreamingContext context): base(information, context)
        {

        }
    }
}
