using Exiled.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLGameLogger
{
    public static class DamageTypeDictionary
    {
        public static readonly Dictionary<string, DamageType> Values =
            Enum.GetValues(typeof(DamageType)).Cast<DamageType>()
                .ToDictionary(
                    x => x.ToString(),
                    x => x,
                    StringComparer.OrdinalIgnoreCase);
    }
}

