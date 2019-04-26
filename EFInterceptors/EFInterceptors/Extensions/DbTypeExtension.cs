using System.Collections.Generic;

namespace EFInterceptors.Extensions
{
    public static class DbTypeExtension
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return (list as IList<T>).Contains(source);
        }
    }
}
