using UnityEngine.Events;
using System.Collections.Generic;

namespace Harris.GPC
{
    public abstract class EnumBase
    {
        public override string ToString()
        {
            return Value;
        }

        protected EnumBase(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }
    }
}