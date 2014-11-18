using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownConverter {
    public static class InterfaceExtensions {
        public static void Swap<T>(this IList<T> enumerable, int i1, int i2) {
            var temp = enumerable[i1];
            enumerable[i1] = enumerable[i2];
            enumerable[i2] = temp;
        }
    }
}
