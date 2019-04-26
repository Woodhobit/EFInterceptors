using EFInterceptors.Interceptors;

namespace EFInterceptors
{
    public class SearchBuilder
    {
        public static string Build(string searchTerm, int? companyId = null, int enforceSearchTermLimit = 0, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length >= enforceSearchTermLimit)
            {
                searchTerm = FullTextSearchInterceptor.FullTextSearch(searchTerm);
            }

            return string.IsNullOrEmpty(searchTerm) ? null : searchTerm;
        }

        public static bool IsFullTextApplied(string searchTerm)
        {
            return FullTextSearchInterceptor.IsFullTextApplied(searchTerm);
        }
    }
}
